using System.Threading.Tasks;

namespace Pseudonym.Crypto.Invictus.Funds.Abstractions
{
    public interface IRepository<TEntity>
    {
        Task UploadItemsAsync(params TEntity[] items);
    }
}
