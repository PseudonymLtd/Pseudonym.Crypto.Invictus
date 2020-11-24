using System.Threading.Tasks;
using Pseudonym.Crypto.Invictus.Funds.Business.Abstractions;

namespace Pseudonym.Crypto.Invictus.Funds.Abstractions
{
    public interface IOperationService
    {
        Task UploadOperationAsync(IOperation operation);
    }
}
