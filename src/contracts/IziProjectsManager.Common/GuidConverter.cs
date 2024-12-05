using System;
using System.Text.Json;
using IziHardGames.Projects.Common;

namespace IziHardGames.DotNetProjects
{
    public class GuidConverter<T> : System.Text.Json.Serialization.JsonConverter<T>
        where T : IIdentityValueObject
    {
        public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TryGetGuid(out var guid))
            {
                var result = Activator.CreateInstance<T>();
                result.Guid = guid;
                return result;
            }
            return default(T);
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.Guid);
        }
    }
}