using Minibank.Core;
using System.Threading;
using System.Threading.Tasks;

namespace Minibank.Data
{
    public class EfUnitOfWork : IUnitOfWork
    {
        private readonly MinibankContext _context;

        public EfUnitOfWork(MinibankContext context)
        {
            _context = context;
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
