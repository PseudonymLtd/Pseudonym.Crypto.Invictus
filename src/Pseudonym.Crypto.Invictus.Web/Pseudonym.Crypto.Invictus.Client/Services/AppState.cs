using System;
using Pseudonym.Crypto.Invictus.Web.Client.Abstractions;

namespace Pseudonym.Crypto.Invictus.Web.Client.Services
{
    internal sealed class AppState : IAppState
    {
        public event Action OnChange;

        public void Assign(Action onChangeAction)
        {
            OnChange += onChangeAction;
        }

        public void NotifyStateChanged() => OnChange?.Invoke();
    }
}
