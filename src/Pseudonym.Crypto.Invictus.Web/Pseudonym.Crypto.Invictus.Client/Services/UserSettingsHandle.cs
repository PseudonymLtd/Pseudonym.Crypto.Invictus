using System;
using System.Threading.Tasks;
using Pseudonym.Crypto.Invictus.Web.Client.Abstractions;
using Pseudonym.Crypto.Invictus.Web.Client.Configuration;

namespace Pseudonym.Crypto.Invictus.Web.Client.Services
{
    internal sealed class UserSettingsHandle : IUserSettingsHandle
    {
        private readonly UserSettings userSettings;
        private readonly ICookieManager cookieManager;
        private readonly ISessionStore sessionStore;

        public UserSettingsHandle(
            ICookieManager cookieManager,
            ISessionStore sessionStore,
            UserSettings userSettings)
        {
            this.userSettings = userSettings;
            this.cookieManager = cookieManager;
            this.sessionStore = sessionStore;
        }

        public IUserSettings Settings => userSettings;

        public async Task UpdateAsync(Action<UserSettings> updateFunc)
        {
            updateFunc(userSettings);

            await cookieManager.SetAsync(CookieKeys.WalletAddresses, userSettings.WalletAddress, 90);
            await cookieManager.SetAsync(CookieKeys.CurrencyCode, userSettings.CurrencyCode, 90);
            await sessionStore.SetAsync(StoreKeys.Funds, userSettings.Funds);
        }
    }
}
