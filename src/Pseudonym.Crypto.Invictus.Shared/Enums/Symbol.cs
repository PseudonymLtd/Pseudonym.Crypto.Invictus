using System.ComponentModel.DataAnnotations;

namespace Pseudonym.Crypto.Invictus.Shared.Enums
{
    public enum Symbol
    {
        [Fund]
        [ContractAddress("0x26e75307fc0c021472feb8f727839531f112f317")]
        C20,

        [Fund]
        [ContractAddress("0x000c100050e98c91f9114fa5dd75ce6869bf4f53")]
        C10,

        [Fund]
        [ContractAddress("0xaf1250fa68d7decd34fd75de8742bc03b29bd58e")]
        IHF,

        [Fund]
        [ContractAddress("0x7ca598a636647b114292bb66e1336865fc262d11")]
        IML,

        [Fund]
        [ContractAddress("0xc4d5c69439e028b9bc86af0ae5c038990bfac43c")]
        EMS,

        [Fund]
        [ContractAddress("0x8df1be0fdf7161a6ff56c8189d7e10358727a96c")]
        IGP,

        [Fund]
        [ContractAddress("0xa32ec8db6ba73ee374dfed2dd566a53f6dc23ebe")]
        IBA,

        [Stake]
        [ContractAddress("0xd83c569268930fadad4cde6d0cb64450fef32b65")]
        ICAP
    }
}
