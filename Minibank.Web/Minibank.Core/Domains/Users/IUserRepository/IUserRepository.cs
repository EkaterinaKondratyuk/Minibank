using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Minibank.Core.Domains.Users.UserRepository
{
    public interface IUserRepository
    {
        Task<List<User>> GetAllAsync(CancellationToken cancellationToken);
        Task<string> CreateAsync(User user, CancellationToken cancellationToken);
        Task DeleteAsync(string id, CancellationToken cancellationToken);
        Task UpdateAsync(User user, CancellationToken cancellationToken);
        Task<bool> ExistsAsync(string id, CancellationToken cancellationToken);
        Task<User> GetByIdAsync(string id, CancellationToken cancellationToken);
    }
}
