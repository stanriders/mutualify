using Microsoft.EntityFrameworkCore;
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

    public Task<List<RelationUser>> GetFriends(int userId, bool highlightMutuals)
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
            Mutual = followers.Any(y=> y.Id == x.Id && y.AllowsHighlighting),
            RelationCreatedAt = followers.Where(y => y.Id == x.Id && y.AllowsHighlighting).Select(y=> y.CreatedAt).FirstOrDefault()
        });
        
        return query.OrderBy(x => x.Username).ToListAsync();
    }

    public Task<List<RelationUser>> GetFollowers(int userId)
    {
        var followers = _databaseContext.Relations.AsNoTracking()
            .Where(x => x.ToId == userId)
            .Include(x => x.From)
            .Select(x => new {x.From, x.CreatedAt});

        var friends = _databaseContext.Relations.AsNoTracking()
            .Where(x => x.FromId == userId)
            .Select(x => x.ToId);

        var query = followers.Select(x=> new RelationUser
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

    public async Task ReplaceRelations(int userId, List<Relation> relations)
    {
        var oldRelations = await _databaseContext.Relations.Where(x => x.FromId == userId).ToListAsync();

        var relationsToDelete = oldRelations.Where(x => relations.All(y => y.ToId != x.ToId)).ToList();
        var relationsToAdd = relations.Where(x => oldRelations.All(y => y.ToId != x.ToId)).ToList();

        _databaseContext.Relations.RemoveRange(relationsToDelete);
        await _databaseContext.Relations.AddRangeAsync(relationsToAdd);

        await _databaseContext.SaveChangesAsync();
    }

    public Task<long> GetRelationCount()
    {
        return _databaseContext.Relations.AsNoTracking().LongCountAsync();
    }
}
