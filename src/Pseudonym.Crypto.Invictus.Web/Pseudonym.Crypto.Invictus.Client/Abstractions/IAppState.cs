using System;

namespace Pseudonym.Crypto.Invictus.Web.Client.Abstractions
{
    public interface IAppState
    {
        void AssignWalletChange(Action onChangeAction);

        void Assign(Action onChangeAction);

        void NotifyStateChanged();

        void NotifyWalletChanged();
    }
}
