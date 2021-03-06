﻿using System;
using Newtonsoft.Json;
using Pseudonym.Crypto.Invictus.Shared;

namespace Pseudonym.Crypto.Invictus.Funds.Business.Json
{
    public sealed class TimeSpanConverter : JsonConverter<TimeSpan>
    {
        public override bool CanRead => false;

        public override void WriteJson(JsonWriter writer, TimeSpan value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString(Format.TimeSpanFormat));
        }

        public override TimeSpan ReadJson(JsonReader reader, Type objectType, TimeSpan existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            throw new NotSupportedException();
        }
    }
}
