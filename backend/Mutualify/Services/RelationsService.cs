using MapsterMapper;
using Mutualify.Contracts;
using Mutualify.Database.Models;
using Mutualify.OsuApi.Interfaces;
using Mutualify.Repositories.Interfaces;
using Mutualify.Services.Interfaces;

namespace Mutualify.Services;

public class RelationsService : IRelationsService
{
    private readonly IRelationRepository _relationRepository;
    private readonly IUserRepository _userRepository;
    private readonly IOsuApiProvider _osuApiDataService;
    private readonly IMapper _mapper;

    public RelationsService(IRelationRepository relationRepository,
        IOsuApiProvider osuApiDataService,
        IUserRepository userRepository,
        IMapper mapper)
    {
        _relationRepository = relationRepository;
        _osuApiDataService = osuApiDataService;
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public Task<List<RelationUser>> GetFriends(int userId, bool orderByRank)
    {
        return _relationRepository.GetFriends(userId, false, orderByRank);
    }

    public async Task<UserFriendsContract> GetUsersFriends(int userId, bool orderByRank)
    {
        var user = await _userRepository.Get(userId);

        var contract = new UserFriendsContract
        {
            User = user
        };

        if (user?.AllowsFriendlistAccess ?? false)
        {
            contract.Friends = await _relationRepository.GetFriends(userId, true, orderByRank);
        }

        return contract;
    }

    public Task<List<RelationUser>> GetFollowers(int userId, bool orderByRank)
    {
        return _relationRepository.GetFollowers(userId, orderByRank);
    }

    public async Task UpdateRelations(int userId)
    {
        var token = await _userRepository.GetTokens(userId);
        if (token is null)
            return;

        var friends = await _osuApiDataService.GetFriends(token.AccessToken);
        if (friends is null)
            return;

        await _userRepository.UpsertRange(friends.Select(x => _mapper.Map<User>(x)).ToList());

        var relations = friends.Select(x => new Relation
        {
            FromId = userId,
            ToId = x.Id,
            CreatedAt = DateTime.UtcNow
        }).ToList();

        await _relationRepository.ReplaceRelations(userId, relations);
    }

    public Task<long> GetRelationCount()
    {
        return _relationRepository.GetRelationCount();
    }
}
