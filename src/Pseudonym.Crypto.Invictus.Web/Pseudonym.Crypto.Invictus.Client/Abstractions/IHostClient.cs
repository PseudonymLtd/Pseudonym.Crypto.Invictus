using System.Threading.Tasks;
using Pseudonym.Crypto.Invictus.Shared.Models;

namespace Pseudonym.Crypto.Invictus.Web.Client.Abstractions
{
    public interface IHostClient
    {
        Task<ApiLogin> LoginAsync();
    }
}
