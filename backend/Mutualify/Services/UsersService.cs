﻿using System.Net;
using Microsoft.EntityFrameworkCore;
using Mutualify.Contracts;
using Mutualify.Database;
using Mutualify.Database.Models;
using Mutualify.OsuApi.Interfaces;
using Mutualify.OsuApi.Models;
using Mutualify.Services.Interfaces;

namespace Mutualify.Services
{
    public class UsersService : IUsersService
    {
        private readonly IOsuApiProvider _osuApiDataService;
        private readonly DatabaseContext _databaseContext;
        private readonly ILogger<UsersService> _logger;

        public UsersService(IOsuApiProvider osuApiDataService, ILogger<UsersService> logger, DatabaseContext databaseContext)
        {
            _osuApiDataService = osuApiDataService;
            _logger = logger;
            _databaseContext = databaseContext;
        }

        public async Task ToggleFriendlistAccess(int userId, bool allow)
        {
            var user = await _databaseContext.Users.FindAsync(userId);
            if (user is not null)
            {
                user.AllowsFriendlistAccess = allow;
                _databaseContext.Users.Update(user);

                await _databaseContext.SaveChangesAsync();
            }
        }

        public async Task<User?> Get(int userId)
        {
            return await _databaseContext.Users.AsNoTracking().SingleOrDefaultAsync(x => x.Id == userId);
        }

        public async Task<StatsContract> GetStats()
        {
            var registeredUsers = await _databaseContext.Relations.AsNoTracking()
                .Select(x => x.FromId)
                .Distinct()
                .CountAsync();

            var relationCount = await _databaseContext.Relations.AsNoTracking()
                .LongCountAsync();

            var lastDayRegistered = await _databaseContext.Users.AsNoTracking()
                .Where(x => x.CreatedAt > DateTime.UtcNow.Date.AddDays(-1))
                .CountAsync();

            var updateEligible = await _databaseContext.Tokens.AsNoTracking()
                .CountAsync();

            return new StatsContract
            {
                RegisteredCount = registeredUsers,
                RelationCount = relationCount,
                EligibleForUpdateCount = updateEligible,
                LastDayRegisteredCount = lastDayRegistered,
            };
        }

        public async Task<RankingsContract> GetFollowerLeaderboard(int offset)
        {
            var users = await _databaseContext.Users.AsNoTracking()
                .OrderByDescending(x => x.FollowerCount)
                .Skip(offset)
                .Take(50)
                .ToListAsync();

            var total = await _databaseContext.Relations.AsNoTracking()
                    .Select(x => x.FromId)
                    .Distinct()
                    .CountAsync();

            return new RankingsContract
            {
                Total = total,
                Users = users
            };
        }

        public async Task<int> GetFollowerLeaderboardRanking(int userId)
        {
            return await _databaseContext.Database
                .SqlQuery<int>($"select x.row_number as \"Value\" from (SELECT \"Id\", ROW_NUMBER() OVER(order by \"FollowerCount\" desc) FROM \"Users\") x WHERE x.\"Id\" = {userId}")
                .SingleOrDefaultAsync();
        }

        public async Task Update(int userId)
        {
            var token = await _databaseContext.Tokens.FindAsync(userId);
            if (token is null)
            {
                return;
            }

            // refresh close-to-expiration tokens first
            if (token.ExpiresOn <= DateTime.UtcNow.AddDays(1))
            {
                if (!await RefreshToken(token))
                {
                    return;
                }
            }

            OsuUser? osuUser = null;

            try
            {
                osuUser = await _osuApiDataService.GetUser(token.AccessToken);
            }
            catch (HttpRequestException e)
            {
                if (e.StatusCode == HttpStatusCode.Unauthorized)
                {
                    if (await RefreshToken(token))
                    {
                        osuUser = await _osuApiDataService.GetUser(token.AccessToken);
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            if (osuUser is null)
            {
                return;
            }

            var user = await _databaseContext.Users.FindAsync(userId);
            if (user is not null)
            {
                if (osuUser.IsRestricted)
                {
                    _logger.LogInformation("User {User} tried updating, but they are restricted!", osuUser.Id);
                    _databaseContext.Users.Remove(user);
                }
                else
                {
                    user.Username = osuUser.Username;
                    user.CountryCode = osuUser.CountryCode;
                    user.FollowerCount = osuUser.FollowerCount;
                    user.Title = osuUser.Title;
                    user.Rank = osuUser.Statistics?.GlobalRank;
                    user.UpdatedAt = DateTime.UtcNow;

                    _databaseContext.Users.Update(user);
                }

                await _databaseContext.SaveChangesAsync();
            }
        }

        private async Task<bool> RefreshToken(Token token)
        {
            var updated = false;

            var newToken = await _osuApiDataService.RefreshToken(token.RefreshToken, token.AccessToken);
            if (newToken is not null)
            {
                token.AccessToken = newToken.AccessToken;
                token.RefreshToken = newToken.RefreshToken;
                token.ExpiresOn = DateTime.UtcNow.AddSeconds(newToken.ExpiresIn);

                _databaseContext.Tokens.Update(token);
                _logger.LogInformation("Updated tokens for user {UserId}", token.UserId);
                updated = true;
            }
            else
            {
                _logger.LogWarning("Couldn't update tokens for user {UserId}, removing from database...", token.UserId);
                _databaseContext.Tokens.Remove(token);
            }

            await _databaseContext.SaveChangesAsync();

            return updated;
        }
    }
}
