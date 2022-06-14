﻿using Microsoft.EntityFrameworkCore;
using Mutualify.Database;
using Mutualify.Database.Models;
using Mutualify.Repositories.Interfaces;

namespace Mutualify.Repositories;

public class RelationRepository : IRelationRepository
{
    private readonly DatabaseContext _databaseContext;

    public RelationRepository(DatabaseContext databaseContext)
    {
        _databaseContext = databaseContext;
    }

    public Task<List<User>> GetFriends(int userId)
    {
        return _databaseContext.Relations.AsNoTracking()
            .Where(x => x.FromId == userId)
            .Include(x=> x.To)
            .Select(x=> x.To)
            .OrderBy(x=> x.Username)
            .ToListAsync();
    }

    public Task<List<User>> GetFollowers(int userId, bool filterMutuals)
    {
        var query = _databaseContext.Relations.AsNoTracking()
            .Where(x => x.ToId == userId)
            .Include(x => x.From)
            .Select(x => x.From);

        if (filterMutuals)
        {
            var friends = _databaseContext.Relations.AsNoTracking()
                .Where(x => x.FromId == userId)
                .Select(x => x.ToId);

            query = query.Where(x => !friends.Contains(x.Id));
        }

        return query.OrderBy(x => x.Username).ToListAsync();
    }

    public async Task Add(List<Relation> relations)
    {
        await _databaseContext.Relations.AddRangeAsync(relations);
        await _databaseContext.SaveChangesAsync();
    }

    public async Task Remove(int userId)
    {
        var relations = _databaseContext.Relations.Where(x => x.FromId == userId);
        _databaseContext.Relations.RemoveRange(relations);

        await _databaseContext.SaveChangesAsync();
    }

    public async Task ReplaceRelations(int userId, List<Relation> relations)
    {
        var transaction = await _databaseContext.Database.BeginTransactionAsync();

        var oldRelations = _databaseContext.Relations.Where(x => x.FromId == userId);
        _databaseContext.Relations.RemoveRange(oldRelations);

        await _databaseContext.Relations.AddRangeAsync(relations);
        await transaction.CommitAsync();

        await _databaseContext.SaveChangesAsync();
    }
}
