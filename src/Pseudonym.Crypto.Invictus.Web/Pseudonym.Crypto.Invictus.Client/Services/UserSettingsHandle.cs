using System;
using System.Threading.Tasks;
using Pseudonym.Crypto.Invictus.Web.Client.Abstractions;
using Pseudonym.Crypto.Invictus.Web.Client.Configuration;

namespace Pseudonym.Crypto.Invictus.Web.Client.Services
{
    internal sealed class UserSettingsHandle : IUserSettingsHandle
    {
        private readonly UserSettings userSettings;
        private readonly ISessionStore sessionStore;

        public UserSettingsHandle(
            ISessionStore sessionStore,
            UserSettings userSettings)
        {
            this.userSettings = userSettings;
            this.sessionStore = sessionStore;
        }

        public IUserSettings Settings => userSettings;

        public async Task UpdateAsync(Action<UserSettings> updateFunc)
        {
            updateFunc(userSettings);

            await sessionStore.SetAsync(StoreKeys.UserSettings, userSettings);
        }
    }
}
