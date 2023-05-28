using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using Mutualify.Database;
using Mutualify.Database.Models;
using Mutualify.Repositories.Interfaces;

namespace Mutualify.Repositories;

public class UserRepository : IUserRepository
{
    private readonly DatabaseContext _databaseContext;

    public UserRepository(DatabaseContext databaseContext)
    {
        _databaseContext = databaseContext;
    }

    public Task<User?> Get(int id, bool track = false)
    {
        if (track)
            return _databaseContext.Users.FirstOrDefaultAsync(x => x.Id == id);

        return _databaseContext.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
    }

    public Task<List<User>> Get(List<int> ids)
    {
        return _databaseContext.Users.AsNoTracking().Where(x => ids.Contains(x.Id)).ToListAsync();
    }

    public Task<List<int>> GetUsersForUpdateJob()
    {
        return _databaseContext.Relations.AsNoTracking()
            .Where(x => _databaseContext.Tokens.Any(t => t.UserId == x.FromId))
            .Select(x => x.FromId)
            .Distinct()
            .ToListAsync();
    }

    public Task<List<User>> GetFollowerRanking(int limit = 50, int offset = 0)
    {
        return _databaseContext.Users.AsNoTracking()
            .OrderByDescending(x => x.FollowerCount)
            .Skip(offset)
            .Take(limit)
            .ToListAsync();
    }

    public Task<int> GetUserFollowerRankingPlacement(int userId)
    {
        return _databaseContext.UserFollowerRankingPlacements
            .FromSqlInterpolated($"select x.row_number from (SELECT \"Id\", ROW_NUMBER() OVER(order by \"FollowerCount\" desc) FROM \"Users\") x WHERE x.\"Id\" = {userId}")
            .Select(x=> x.RowNumber)
            .FirstOrDefaultAsync();
    }

    public Task<int> GetRegisteredUserCount()
    {
        return _databaseContext.Relations.AsNoTracking()
            .Select(x=> x.FromId)
            .Distinct()
            .CountAsync();
    }

    public async Task<User> Add(User user)
    {
        var addedUser = await _databaseContext.Users.AddAsync(user);

        await _databaseContext.SaveChangesAsync();

        return addedUser.Entity;
    }

    public async Task UpsertRange(List<User> users)
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

        await _databaseContext.Database.ExecuteSqlRawAsync(
            $@"insert into ""Users""(""Id"", ""CountryCode"", ""Username"", ""Title"", ""FollowerCount"", ""AllowsFriendlistAccess"", ""Rank"")
                      values{builder.ToString()}
                      on conflict (""Id"") do update
                      set (""CountryCode"", ""Username"", ""Title"", ""Rank"") = (EXCLUDED.""CountryCode"", EXCLUDED.""Username"", EXCLUDED.""Title"", EXCLUDED.""Rank"");"
            );

        await _databaseContext.SaveChangesAsync();
    }

    public async Task Update(User user)
    {
        _databaseContext.Users.Update(user);

        await _databaseContext.SaveChangesAsync();
    }

    public async Task Remove(User user)
    {
        // FKs exist
        _databaseContext.Users.Remove(await _databaseContext.Users.FirstAsync(x => x.Id == user.Id));

        await _databaseContext.SaveChangesAsync();
    }

    public async Task RemoveTokens(int userId)
    {
        _databaseContext.Tokens.Remove(await _databaseContext.Tokens.FirstAsync(x => x.UserId == userId));
        await _databaseContext.SaveChangesAsync();
    }

    public Task<Token?> GetTokens(int userId)
    {
        return _databaseContext.Tokens.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == userId);
    }

    public async Task UpsertTokens(Token token)
    {
        if (string.IsNullOrEmpty(token.AccessToken))
        {
            // somehow this can actually happen?????
            return;
        }

        var existingTokens = await _databaseContext.Tokens.FirstOrDefaultAsync(x => x.UserId == token.UserId);
        if (existingTokens is not null)
        {
            existingTokens.AccessToken = token.AccessToken;
            existingTokens.RefreshToken = token.RefreshToken;

            // this does NOT work as upsert :/
            _databaseContext.Tokens.Update(existingTokens);
        }
        else
        {
            await _databaseContext.Tokens.AddAsync(token);
        }

        await _databaseContext.SaveChangesAsync();
    }
}
