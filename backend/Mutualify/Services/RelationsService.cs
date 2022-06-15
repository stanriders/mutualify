using MapsterMapper;
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

    public async Task<List<User>> GetFriends(int userId, bool shouldCheckForAllowance)
    {
        if (shouldCheckForAllowance)
        {
            var user = await _userRepository.Get(userId);

            if (!user?.AllowsFriendlistAccess ?? false)
                return new List<User>();
        }

        return await _relationRepository.GetFriends(userId);
    }

    public Task<List<User>> GetFollowers(int userId, bool filterMutuals)
    {
        return _relationRepository.GetFollowers(userId, filterMutuals);
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
            ToId = x.Id
        }).ToList();

        await _relationRepository.Remove(userId);
        await _relationRepository.Add(relations);
    }

    // TODO: this shouldn't be here
    public async Task ToggleFriendlistAccess(int userId, bool allow)
    {
        var user = await _userRepository.Get(userId);
        if (user is not null)
        {
            user.AllowsFriendlistAccess = allow;
            await _userRepository.Update(user);
        }
    }

    public Task<long> GetRelationCount()
    {
        return _relationRepository.GetRelationCount();
    }
}
