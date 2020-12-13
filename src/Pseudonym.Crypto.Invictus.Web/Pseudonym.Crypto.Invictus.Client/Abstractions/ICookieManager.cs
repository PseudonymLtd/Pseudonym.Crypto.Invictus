using System.Threading.Tasks;

namespace Pseudonym.Crypto.Invictus.Web.Client.Abstractions
{
    public interface ICookieManager
    {
        void Set<T>(string key, T data, int days);

        Task SetAsync<T>(string key, T data, int days);

        T Get<T>(string key);

        Task<T> GetAsync<T>(string key);
    }
}
