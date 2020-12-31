using System.Threading.Tasks;

namespace Pseudonym.Crypto.Invictus.Web.Client.Abstractions
{
    public interface ICookieManager
    {
        bool Consented { get; }

        void Set<T>(string key, T data, int days);

        Task SetAsync<T>(string key, T data, int days);

        Task ConsentAsync();

        T Get<T>(string key);

        Task<T> GetAsync<T>(string key);
    }
}
