namespace Pseudonym.Crypto.Invictus.Funds.Configuration
{
    public sealed class AwsSettings
    {
        public string Region { get; set; }

        public string AccessKey { get; set; }

        public string SecretAccessKey { get; set; }

        public bool UseConfiguredSecurity()
        {
            return !string.IsNullOrEmpty(AccessKey) &&
                !string.IsNullOrEmpty(SecretAccessKey);
        }
    }
}
