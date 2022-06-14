using System.Text;
using Microsoft.EntityFrameworkCore;
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

    public Task<List<User>> GetFollowerRanking(int limit = 50)
    {
        return _databaseContext.Users.AsNoTracking()
            .OrderBy(x=> x.FollowerCount)
            .Take(limit)
            .ToListAsync();
    }

    public Task<int> GetRegisteredUserCount()
    {
        // a bit of a hack - we know that every user that was imported as a friend has 0 followers
        return _databaseContext.Users.AsNoTracking()
            .CountAsync(x => x.FollowerCount > 0);
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
            if (i != users.Count - 1)
            {
                builder.Append(
                    $"({user.Id}, '{user.CountryCode}', '{user.Username}', {(user.Title is null ? "null" : $"`{user.Title}`")}, {user.FollowerCount}), ");
            }
            else
            {
                builder.Append(
                    $"({user.Id}, '{user.CountryCode}', '{user.Username}', {(user.Title is null ? "null" : $"`{user.Title}`")}, {user.FollowerCount}) ");
            }
        }

        await _databaseContext.Database.ExecuteSqlRawAsync(
            $@"insert into ""Users""(""Id"", ""CountryCode"", ""Username"", ""Title"", ""FollowerCount"")
                      values{builder.ToString()}
                      on conflict (""Id"") do update
                      set (""CountryCode"", ""Username"", ""Title"") = (EXCLUDED.""CountryCode"", EXCLUDED.""Username"", EXCLUDED.""Title"");"
            );

        await _databaseContext.SaveChangesAsync();
    }

    public async Task Update(User user)
    {
        _databaseContext.Users.Update(user);

        await _databaseContext.SaveChangesAsync();
    }

    public Task Remove(User user)
    {
        throw new NotImplementedException();
    }

    public Task<Token?> GetTokens(int userId)
    {
        return _databaseContext.Tokens.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == userId);
    }

    public async Task UpsertTokens(Token token)
    {
        var existingTokens = await _databaseContext.Tokens.FirstOrDefaultAsync(x => x.UserId == token.User.Id);
        if (existingTokens is not null)
        {
            existingTokens.AccessToken = token.AccessToken;
            existingTokens.RefreshToken = token.RefreshToken;
            token = existingTokens;
        }

        _databaseContext.Tokens.Update(token);

        await _databaseContext.SaveChangesAsync();
    }
}
