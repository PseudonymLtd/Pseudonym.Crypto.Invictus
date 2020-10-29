using System;
using System.Threading.Tasks;
using Pseudonym.Crypto.Invictus.Web.Client.Configuration;

namespace Pseudonym.Crypto.Invictus.Web.Client.Abstractions
{
    public interface IUserSettingsHandle
    {
        IUserSettings Settings { get; }

        Task UpdateAsync(Action<UserSettings> updateFunc);
    }
}
