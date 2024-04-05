using System.Text;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Mutualify.Contracts;
using Mutualify.Database;
using Mutualify.Database.Models;
using Mutualify.OsuApi.Interfaces;
using Mutualify.Services.Interfaces;

namespace Mutualify.Services;

public class RelationsService : IRelationsService
{
    private readonly DatabaseContext _databaseContext;
    private readonly IOsuApiProvider _osuApiDataService;
    private readonly IMapper _mapper;

    public RelationsService(IOsuApiProvider osuApiDataService,
        IMapper mapper,
        DatabaseContext databaseContext)
    {
        _osuApiDataService = osuApiDataService;
        _mapper = mapper;
        _databaseContext = databaseContext;
    }

    public Task<List<RelationUser>> GetFriends(int userId)
    {
        var friends = _databaseContext.Relations.AsNoTracking()
            .Where(x => x.FromId == userId)
            .Include(x => x.To)
            .Select(x => x.To);

        var followers = _databaseContext.Relations.AsNoTracking()
            .Where(x => x.ToId == userId)
            .Select(x => new { Id = x.FromId, AllowsHighlighting = x.From.AllowsFriendlistAccess, x.CreatedAt });

        var query = friends.Select(x => new RelationUser
        {
            CountryCode = x.CountryCode,
            Id = x.Id,
            Title = x.Title,
            Username = x.Username,
            Rank = x.Rank,
            AllowsFriendlistAccess = x.AllowsFriendlistAccess,
            Mutual = followers.Any(y => y.Id == x.Id && y.AllowsHighlighting),
            RelationCreatedAt = followers.Where(y => y.Id == x.Id && y.AllowsHighlighting).Select(y => y.CreatedAt).FirstOrDefault()
        });

        return query.OrderBy(x => x.Username).ToListAsync();
    }

    public async Task<UserFriendsContract> GetUsersFriends(int userId)
    {
        var user = await _databaseContext.Users.AsNoTracking().SingleOrDefaultAsync(x => x.Id == userId);

        var contract = new UserFriendsContract
        {
            User = user
        };

        if (user?.AllowsFriendlistAccess ?? false)
        {
            contract.Friends = await GetFriends(userId);
        }

        return contract;
    }

    public Task<List<RelationUser>> GetFollowers(int userId)
    {
        var followers = _databaseContext.Relations.AsNoTracking()
            .Where(x => x.ToId == userId)
            .Include(x => x.From)
            .Select(x => new { x.From, x.CreatedAt });

        var friends = _databaseContext.Relations.AsNoTracking()
            .Where(x => x.FromId == userId)
            .Select(x => x.ToId);

        var query = followers.Select(x => new RelationUser
        {
            CountryCode = x.From.CountryCode,
            Id = x.From.Id,
            Title = x.From.Title,
            Username = x.From.Username,
            Rank = x.From.Rank,
            AllowsFriendlistAccess = x.From.AllowsFriendlistAccess,
            Mutual = friends.Contains(x.From.Id),
            RelationCreatedAt = x.CreatedAt
        });

        return query.OrderBy(x => x.Username).ToListAsync();
    }

    public async Task UpdateRelations(int userId)
    {
        var token = await _databaseContext.Tokens.AsNoTracking().SingleOrDefaultAsync(x => x.UserId == userId);

        if (token is null)
            return;

        var friends = await _osuApiDataService.GetFriends(token.AccessToken);

        if (friends is null)
            return;

        await UpsertPlayers(friends.Select(x => _mapper.Map<User>(x)).ToList());

        var relations = friends.Select(x => new Relation
        {
            FromId = userId,
            ToId = x.Id,
            CreatedAt = DateTime.UtcNow
        }).ToList();

        var oldRelations = await _databaseContext.Relations.Where(x => x.FromId == userId).ToListAsync();

        var relationsToDelete = oldRelations.Where(x => relations.All(y => y.ToId != x.ToId)).ToList();
        var relationsToAdd = relations.Where(x => oldRelations.All(y => y.ToId != x.ToId)).ToList();

        _databaseContext.Relations.RemoveRange(relationsToDelete);
        await _databaseContext.Relations.AddRangeAsync(relationsToAdd);

        await _databaseContext.SaveChangesAsync();
    }

    public async Task UpsertPlayers(List<User> users)
    {
        var builder = new StringBuilder();
        for (var i = 0; i < users.Count; i++)
        {
            var user = users[i];

            builder.Append(
                $"({user.Id}, '{user.CountryCode}', '{user.Username.Replace("\'", "\'\'")}', {(user.Title is null ? "null" : $"`{user.Title.Replace("\'", "\'\'")}`")}, {user.FollowerCount}, false, {(user.Rank is null ? "null" : user.Rank)})");

            if (i != users.Count - 1)
            {
                builder.Append(", ");
            }
        }

        if (builder.Length <= 0)
            return; // return early to not produce incorrect sql

#pragma warning disable EF1002 // don't care
        await _databaseContext.Database.ExecuteSqlRawAsync(
            $@"insert into ""Users""(""Id"", ""CountryCode"", ""Username"", ""Title"", ""FollowerCount"", ""AllowsFriendlistAccess"", ""Rank"")
                      values{builder.ToString()}
                      on conflict (""Id"") do update
                      set (""CountryCode"", ""Username"", ""Title"", ""Rank"") = (EXCLUDED.""CountryCode"", EXCLUDED.""Username"", EXCLUDED.""Title"", EXCLUDED.""Rank"");"
        );
#pragma warning restore EF1002

        await _databaseContext.SaveChangesAsync();
    }
}
