using System;

namespace Pseudonym.Crypto.Invictus.Web.Client.Abstractions
{
    public interface IAppState
    {
        void Assign(Action onChangeAction);

        void NotifyStateChanged();
    }
}
