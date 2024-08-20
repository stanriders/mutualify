using AutoFixture.AutoNSubstitute;
using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mutualify.Database;
using Mutualify.Database.Models;
using Mutualify.OsuApi.Interfaces;
using Mutualify.Services;
using Mutualify.Services.Interfaces;
using Newtonsoft.Json;
using NSubstitute;

namespace Mutualify.Tests
{
    public class Tests
    {
        private IUsersService _usersService = null!;
        private DatabaseContext _databaseContext = null!;

        private readonly IFixture _fixture = new Fixture();

        private User _user = null!;
        private Token _token = null!;

        [SetUp]
        public async Task Setup()
        {
            _fixture.Customize(new CompositeCustomization(new AutoNSubstituteCustomization()));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            var options = new DbContextOptionsBuilder<DatabaseContext>()
                .UseInMemoryDatabase("TestDb")
                .EnableSensitiveDataLogging()
                .Options;

            _databaseContext = new DatabaseContext(options);

            using var logFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var logger = logFactory.CreateLogger<UsersService>();

            _usersService = new UsersService(Substitute.For<IOsuApiProvider>(), logger, _databaseContext);

            await PopulateUser();
        }

        [TearDown]
        public void Teardown()
        {
            _databaseContext.Dispose();
        }

        [Test]
        public async Task GetSelfReturnsCorrectData()
        {
            var result = await _usersService.Get(1);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Id, Is.EqualTo(_user.Id));
            Assert.That(result.AllowsFriendlistAccess, Is.EqualTo(_user.AllowsFriendlistAccess));
            Assert.That(result.CountryCode, Is.EqualTo(_user.CountryCode));
            Assert.That(result.FollowerCount, Is.EqualTo(_user.FollowerCount));
            Assert.That(result.Username, Is.EqualTo(_user.Username));

            Assert.That(result.Token, Is.Not.Null);

            // make sure token isn't being sent to the user
            var json = JsonConvert.SerializeObject(result);
            var deserialized = JsonConvert.DeserializeObject<User>(json);
            
            Assert.That(deserialized, Is.Not.Null);
            Assert.That(deserialized!.Token, Is.Null);
        }

        [Test]
        public async Task ToggleFriendlistAccessWorks()
        {
            var isAllowed = _user.AllowsFriendlistAccess;
            await _usersService.ToggleFriendlistAccess(1, !isAllowed);

            var result = await _usersService.Get(1);
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.AllowsFriendlistAccess, Is.Not.EqualTo(isAllowed));
        }

        private async Task PopulateUser()
        {
            _user = _fixture.Create<User>();
            _user.Id = 1;

            _token = _fixture.Create<Token>();
            _token.UserId = 1;
            _token.User = _user;
            _user.Token = _token;

            await _databaseContext.Users.AddAsync(_user);
            await _databaseContext.Tokens.AddAsync(_token);
            await _databaseContext.SaveChangesAsync();
        }
    }
}
