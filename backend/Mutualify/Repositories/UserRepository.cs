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

    public Task<List<int>> GetAllIds()
    {
        // this makes the job too slow, doing a slow enumerating query is way faster in the long run
        //return _databaseContext.Users.AsNoTracking().Select(x => x.Id).ToListAsync();

        return _databaseContext.Relations.AsNoTracking()
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

    public Task<int> GetRegisteredUserCount()
    {
        return _databaseContext.Relations.AsNoTracking()
            .Select(x=> x.FromId)
            .Distinct()
            .CountAsync();
#if false
        // a bit of a hack - we know that every user that was imported as a friend has 0 followers
        return _databaseContext.Users.AsNoTracking()
            .CountAsync(x => x.FollowerCount > 0);
#endif
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
                    $"({user.Id}, '{user.CountryCode}', '{user.Username.Replace("\'", "\'\'")}', {(user.Title is null ? "null" : $"`{user.Title.Replace("\'", "\'\'")}`")}, {user.FollowerCount}, false), ");
            }
            else
            {
                builder.Append(
                    $"({user.Id}, '{user.CountryCode}', '{user.Username.Replace("\'", "\'\'")}', {(user.Title is null ? "null" : $"`{user.Title.Replace("\'", "\'\'")}`")}, {user.FollowerCount}, false) ");
            }
        }

        if (builder.Length <= 0)
            return; // return early to not produce incorrect sql

        await _databaseContext.Database.ExecuteSqlRawAsync(
            $@"insert into ""Users""(""Id"", ""CountryCode"", ""Username"", ""Title"", ""FollowerCount"", ""AllowsFriendlistAccess"")
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
        // uhhhhhhhhhhhh
        // this looks bad
        _databaseContext.Relations.RemoveRange(_databaseContext.Relations.Where(x=> x.FromId == user.Id));
        _databaseContext.Relations.RemoveRange(_databaseContext.Relations.Where(x => x.ToId == user.Id));
        _databaseContext.Tokens.Remove(_databaseContext.Tokens.First(x => x.UserId == user.Id));
        _databaseContext.Users.Remove(_databaseContext.Users.First(x => x.Id == user.Id));

        return _databaseContext.SaveChangesAsync();
    }

    public Task RemoveTokens(int userId)
    {
        _databaseContext.Tokens.Remove(_databaseContext.Tokens.First(x => x.UserId == userId));
        return _databaseContext.SaveChangesAsync();
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
