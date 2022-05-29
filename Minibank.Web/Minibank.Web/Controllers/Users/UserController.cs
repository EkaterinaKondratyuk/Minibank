using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Minibank.Core.Domains.Users;
using Minibank.Core.Domains.Users.Services;
using Minibank.Web.Controllers.Users.Dto;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Minibank.Web.Controllers.Users
{
    [ApiController]
    [Route("api/v1/minibank/[controller]/[action]")]
    public class UserController
    {
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public UserController(IUserService userService, IMapper mapper)
        {
            _userService = userService;
            _mapper = mapper;
        }

        [HttpPost]
        public async Task<string> CreateAsync(UserDtoPublic outerModel, CancellationToken cancellationToken)
        {
            User user = _mapper.Map<UserDtoPublic, User>(outerModel);
            return await _userService.CreateAsync(user, cancellationToken);
        }

        [HttpPut("{id}")]
        public async Task<bool> UpdateAsync(string id, UserDtoPublic outerModel, CancellationToken cancellationToken)
        {
            User user = _mapper.Map<UserDtoPublic, User>(outerModel);
            user.Id = id;
            return await _userService.UpdateAsync(user, cancellationToken);
        }

        [HttpDelete("{id}")]
        public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken)
        {
            return await _userService.DeleteAsync(id, cancellationToken);
        }

        [HttpGet]
        public async Task<List<UserDto>> GetAllAsync(CancellationToken cancellationToken)
        {
            List<User> users = await _userService.GetAll(cancellationToken);
            return _mapper.Map<List<User>, List <UserDto>>(users);
        }

        [HttpGet]
        public async Task<UserDtoPublic> GetByIdAsync(string id, CancellationToken cancellationToken)
        {
            User user = await _userService.GetByIdAsync(id, cancellationToken);
            return _mapper.Map<User, UserDtoPublic>(user);
        }
    }
}
