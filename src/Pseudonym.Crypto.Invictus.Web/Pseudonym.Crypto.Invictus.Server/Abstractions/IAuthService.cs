using System.Threading.Tasks;
using Pseudonym.Crypto.Invictus.Shared.Models;

namespace Pseudonym.Crypto.Invictus.Web.Server.Abstractions
{
    public interface IAuthService
    {
        Task<ApiLogin> LoginAsync();
    }
}
