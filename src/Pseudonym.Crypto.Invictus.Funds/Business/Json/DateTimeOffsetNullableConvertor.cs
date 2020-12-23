using System;
using Newtonsoft.Json;
using Pseudonym.Crypto.Invictus.Shared;

namespace Pseudonym.Crypto.Invictus.Funds.Business.Json
{
    public sealed class DateTimeOffsetNullableConvertor : JsonConverter<DateTimeOffset?>
    {
        public override bool CanRead => false;

        public override void WriteJson(JsonWriter writer, DateTimeOffset? value, JsonSerializer serializer)
        {
            writer.WriteValue(value.HasValue
                ? value.Value.UtcDateTime.ToString(Format.DateTimeFormat)
                : null);
        }

        public override DateTimeOffset? ReadJson(JsonReader reader, Type objectType, DateTimeOffset? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            throw new NotSupportedException();
        }
    }
}