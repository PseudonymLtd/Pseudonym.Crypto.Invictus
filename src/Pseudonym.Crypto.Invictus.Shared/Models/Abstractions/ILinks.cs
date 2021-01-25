using System;

namespace Pseudonym.Crypto.Invictus.Shared.Models.Abstractions
{
    public interface ILinks
    {
        Uri Self { get; }

        Uri Detail { get; }

        Uri Fact { get; }

        Uri External { get; }
    }
}
