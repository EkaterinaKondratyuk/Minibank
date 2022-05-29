using System.Threading;
using System.Threading.Tasks;

namespace Minibank.Core
{
    public interface IUnitOfWork
    {
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
