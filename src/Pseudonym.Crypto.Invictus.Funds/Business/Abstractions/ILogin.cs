using System;

namespace Pseudonym.Crypto.Invictus.Funds.Business.Abstractions
{
    public interface ILogin
    {
        string AccessToken { get; }

        DateTime ExpiresAt { get; }
    }
}
