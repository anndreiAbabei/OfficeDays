using System.Text.Json.Serialization;

namespace OfficeDays.Domain;

[JsonConverter(typeof(JsonStringEnumConverter<Appearance>))]
public enum Appearance
{
    System,
    Dark,
    Light
}
