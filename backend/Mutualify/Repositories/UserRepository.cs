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

    public Task<User?> Get(int id)
    {
        return _databaseContext.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
    }

    public Task<List<User>> Get(List<int> ids)
    {
        return _databaseContext.Users.Where(x => ids.Contains(x.Id)).ToListAsync();
    }

    public async Task<User> Add(User user)
    {
        var addedUser = await _databaseContext.Users.AddAsync(user);

        await _databaseContext.SaveChangesAsync();

        return addedUser.Entity;
    }

    public async Task AddRange(List<User> users)
    {
        await _databaseContext.Users.AddRangeAsync(users);

        await _databaseContext.SaveChangesAsync();
    }

    public Task Update(User user)
    {
        throw new NotImplementedException();
    }

    public Task Remove(User user)
    {
        throw new NotImplementedException();
    }

    public Task<Token?> GetTokens(int userId)
    {
        return _databaseContext.Tokens.AsNoTracking().FirstOrDefaultAsync(x => x.User.Id == userId);
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
