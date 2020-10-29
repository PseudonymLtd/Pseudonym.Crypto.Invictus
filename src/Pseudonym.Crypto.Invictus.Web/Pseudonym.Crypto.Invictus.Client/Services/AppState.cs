using System;
using Pseudonym.Crypto.Invictus.Web.Client.Abstractions;

namespace Pseudonym.Crypto.Invictus.Web.Client.Services
{
    internal sealed class AppState : IAppState
    {
        public event Action OnChange;

        public event Action OnWalletChange;

        public void Assign(Action onChangeAction)
        {
            OnChange += onChangeAction;
        }

        public void AssignWalletChange(Action onChangeAction)
        {
            OnWalletChange += onChangeAction;
            OnChange += onChangeAction;
        }

        public void NotifyStateChanged() => OnChange?.Invoke();

        public void NotifyWalletChanged() => OnWalletChange?.Invoke();
    }
}
