using System.Reflection;
using FastExpressionCompiler;
using Hangfire;
using Hangfire.PostgreSql;
using Hangfire.PostgreSql.Factories;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Mutualify.Configuration;
using Mutualify.Database;
using Mutualify.Database.Models;
using Mutualify.Jobs;
using Mutualify.Jobs.Interfaces;
using Mutualify.OsuApi;
using Mutualify.OsuApi.Interfaces;
using Mutualify.OsuApi.Models;
using Mutualify.Services;
using Mutualify.Services.Interfaces;
using Newtonsoft.Json;
using Npgsql;
using Serilog;
using Serilog.Settings.Configuration;
using SerilogTracing;
using UAParser;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration, new ConfigurationReaderOptions() {SectionName = "Logging" })
    .ReadFrom.Services(services));

using var tracer = new ActivityListenerConfiguration()
    .Instrument.AspNetCoreRequests()
    .TraceToSharedLogger();

#region Services

var dbConfig = builder.Configuration.GetSection("Database");
var osuConfig = builder.Configuration.GetSection("osuApi");
var basePath = builder.Configuration.GetValue<string>("PathBase");

builder.Services.Configure<OsuApiConfig>(osuConfig);

TypeAdapterConfig.GlobalSettings.Scan(Assembly.GetEntryAssembly()!);
TypeAdapterConfig.GlobalSettings.Compiler = x => x.CompileFast();

TypeAdapterConfig<OsuUser, User>.NewConfig()
    .Map(x => x.Rank, x => x.Statistics!.GlobalRank)
    .Map(x => x.UpdatedAt, x => DateTime.UtcNow)
    .Compile();

builder.Services.AddTransient<IMapper, Mapper>();

var connectionString = new NpgsqlConnectionStringBuilder
{
    Host = dbConfig["Host"],
    Port = int.Parse(dbConfig["Port"]!),
    Database = dbConfig["Database"],
    Username = dbConfig["Username"],
    Password = dbConfig["Password"]
};

builder.Services.AddDbContext<DatabaseContext>(options =>
    options.UseNpgsql(connectionString.ConnectionString));

builder.Services.AddHttpContextAccessor();

builder.Services.AddAuthentication("InternalCookies")
    .AddCookie("InternalCookies", options =>
    {
        // set some paths to empty to make auth not redirect API calls
        options.LoginPath = string.Empty;
        options.AccessDeniedPath = string.Empty;
        options.LogoutPath = string.Empty;
        options.Cookie.Path = "/";
        options.SlidingExpiration = true;
        options.Events.OnValidatePrincipal = context =>
        {
            var name = context.Principal?.Identity?.Name;
            if (string.IsNullOrEmpty(name) || !long.TryParse(name, out _))
            {
                context.RejectPrincipal();
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            }

            return Task.CompletedTask;
        };

        static Task UnauthorizedRedirect(RedirectContext<CookieAuthenticationOptions> context)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;

            return Task.CompletedTask;
        }

        options.Events.OnRedirectToLogin = UnauthorizedRedirect;
        options.Events.OnRedirectToAccessDenied = UnauthorizedRedirect;
    })
    .AddCookie("ExternalCookies")
    .AddOAuth("osu", options =>
    {
        options.SignInScheme = "ExternalCookies";

        options.TokenEndpoint = "https://osu.ppy.sh/oauth/token";
        options.AuthorizationEndpoint = "https://osu.ppy.sh/oauth/authorize";
        options.ClientId = osuConfig["ClientID"]!;
        options.ClientSecret = osuConfig["ClientSecret"]!;
        options.CallbackPath = osuConfig["CallbackUrl"];
        options.Scope.Add("public");
        options.Scope.Add("friends.read");

        options.CorrelationCookie.SameSite = SameSiteMode.Lax;

        options.SaveTokens = true;

        options.Validate();
    });


builder.Services.AddHttpClient<OsuApiProvider>();

builder.Services.AddSingleton<IOsuApiProvider, OsuApiProvider>();

builder.Services.AddTransient<IRelationsService, RelationsService>();
builder.Services.AddTransient<IUsersService, UsersService>();

builder.Services.AddTransient<IUserRelationsUpdateJob, UserRelationsUpdateJob>();
builder.Services.AddTransient<IUserUpdateJob, UserUpdateJob>();
builder.Services.AddTransient<IUserPopulateJob, UserPopulateJob>();

builder.Services.AddControllers().AddNewtonsoftJson(options =>
{
    options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHangfire(x =>
{
    x.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UseSerilogLogProvider()
        .UsePostgreSqlStorage(options => { options.UseNpgsqlConnection(connectionString.ConnectionString); },
            new PostgreSqlStorageOptions { UseSlidingInvisibilityTimeout = true });
});

builder.Services.AddHangfireServer(options =>
{
    options.SchedulePollingInterval = TimeSpan.FromHours(1);
});
#endregion

#region App

var app = builder.Build();

app.UseSerilogRequestLogging(options =>
{
    options.EnrichDiagnosticContext = (context, httpContext) =>
    {
        var parsedUserAgent = Parser.GetDefault()?.Parse(httpContext.Request.Headers.UserAgent);
        context.Set("UserId", httpContext.User.Identity?.Name);
        context.Set("Browser", parsedUserAgent?.Browser.ToString());
        context.Set("Device", parsedUserAgent?.Device.ToString());
        context.Set("OS", parsedUserAgent?.OS.ToString());
    };
});

app.UseForwardedHeaders(new ForwardedHeadersOptions { ForwardedHeaders = ForwardedHeaders.All });

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseCookiePolicy(new CookiePolicyOptions
    {
        Secure = CookieSecurePolicy.None,
        MinimumSameSitePolicy = SameSiteMode.Lax
    });

    app.UseCors(x =>
    {
        x.WithOrigins("http://localhost:3000").AllowAnyHeader().AllowAnyMethod().AllowCredentials();
    });
    app.UseSwagger(c =>
    {
        c.RouteTemplate = "swagger/{documentName}/swagger.json";
        c.PreSerializeFilters.Add((swaggerDoc, httpReq) =>
        {
            var scheme = app.Environment.IsStaging() ? "https" : httpReq.Scheme;
            swaggerDoc.Servers = new List<OpenApiServer>
                { new() { Url = $"{scheme}://{httpReq.Host.Value}{basePath}" } };
        });
    });
    app.UseSwaggerUI();
}

if (app.Environment.IsStaging() || app.Environment.IsProduction())
{
    app.Use((context, next) =>
    {
        context.Request.Scheme = "https";

        return next(context);
    });

    app.UseCookiePolicy(new CookiePolicyOptions
    {
        Secure = CookieSecurePolicy.SameAsRequest,
        MinimumSameSitePolicy = SameSiteMode.Lax
    });
}

app.UsePathBase(basePath);
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetService<DatabaseContext>();
    context?.Database.Migrate();
}

app.UseHangfireDashboard(options: new DashboardOptions
{
    Authorization = new[] { new AuthorizationFilter() }
});
app.MapHangfireDashboard();

RecurringJob.AddOrUpdate<IUserRelationsUpdateJob>("user-relations-update", x => x.Run(null!, CancellationToken.None), Cron.Daily(12));
RecurringJob.AddOrUpdate<IUserUpdateJob>("users-update", x => x.Run(null!, CancellationToken.None), Cron.Daily());
//BackgroundJob.Enqueue<IUserPopulateJob>(x => x.Run(null!, JobCancellationToken.Null));

try
{
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

#endregion
