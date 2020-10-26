using System.Threading.Tasks;

namespace Pseudonym.Crypto.Invictus.Web.Client.Abstractions
{
    public interface ISessionStore
    {
        Task SetAsync<T>(string key, T data);

        Task<T> GetAsync<T>(string key);

        Task RemoveItemAsync(string key);

        Task ClearAsync();

        Task<int> LengthAsync();

        Task<string> KeyAsync(int index);
    }
}
