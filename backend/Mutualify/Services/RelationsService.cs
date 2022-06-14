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

    public Task<List<User>> GetFriends(int userId)
    {
        return _relationRepository.GetFriends(userId);
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
}
