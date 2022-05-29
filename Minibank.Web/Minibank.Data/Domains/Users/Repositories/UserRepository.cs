using AutoMapper;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Minibank.Core;
using Minibank.Core.Domains.Users;
using Minibank.Core.Domains.Users.UserRepository;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Minibank.Data.Domain.Users.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly MinibankContext _context;
        private readonly IMapper _mapper;

        public UserRepository(MinibankContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<User>> GetAllAsync(CancellationToken cancellationToken)
        {
            List<UserDbModel> users = await _context.Users.AsNoTracking().ToListAsync(cancellationToken);
            return _mapper.Map<List<UserDbModel>, List <User>>(users);
        }

        public async Task<User> GetByIdAsync(string id, CancellationToken cancellationToken)
        {
            UserDbModel user = await GetDbUserByIdAsync(id, cancellationToken);
            if (user == null)
                throw new ObjectNotFoundException($"Объект {nameof(user.GetType)} с id = {id} не найден");
            return _mapper.Map<UserDbModel, User>(user);
        }

        public async Task<string> CreateAsync(User user, CancellationToken cancellationToken)
        {
            user.Id = Guid.NewGuid().ToString();
            UserDbModel entity = _mapper.Map<User, UserDbModel>(user);

            await _context.Users.AddAsync(entity, cancellationToken);
            return entity.Id;
        }

        public async Task UpdateAsync(User user, CancellationToken cancellationToken)
        {
            var entity = await GetDbUserByIdAsync(user.Id, cancellationToken);
            if (entity == null)
                throw new ObjectNotFoundException($"Объект {nameof(entity.GetType)} с id = {user.Id} не найден");
            entity = _mapper.Map<User, UserDbModel>(user);
            _context.Users.Update(entity);
        }

        public async Task DeleteAsync(string id, CancellationToken cancellationToken)
        {
            var user = await GetDbUserByIdAsync(id, cancellationToken);
            if (user == null)
                throw new ObjectNotFoundException($"Объект {nameof(user.GetType)} с id = {id} не найден");
            _context.Users.Remove(user);
        }

        public async Task<bool> ExistsAsync(string id, CancellationToken cancellationToken)
        {
            return await _context.Users.AsNoTracking().AnyAsync(x => x.Id == id, cancellationToken);
        }

        private async Task<UserDbModel> GetDbUserByIdAsync(string id, CancellationToken cancellationToken)
        {
            return await _context.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }
    }
}
