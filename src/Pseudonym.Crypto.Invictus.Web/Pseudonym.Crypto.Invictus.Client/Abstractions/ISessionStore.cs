using System.Threading.Tasks;

namespace Pseudonym.Crypto.Invictus.Web.Client.Abstractions
{
    public interface ISessionStore
    {
        void Set<T>(string key, T data);

        Task SetAsync<T>(string key, T data);

        T Get<T>(string key);

        Task<T> GetAsync<T>(string key);

        Task RemoveItemAsync(string key);

        Task ClearAsync();

        Task<int> LengthAsync();

        Task<string> KeyAsync(int index);
    }
}
