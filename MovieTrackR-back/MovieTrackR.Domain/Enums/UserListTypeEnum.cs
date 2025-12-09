using System.Runtime.Serialization;

namespace MovieTrackR.Domain.Enums;

public enum UserListType
{
    [EnumMember(Value = "Custom")]
    Custom,
    [EnumMember(Value = "Watchlist")]
    Watchlist,
    [EnumMember(Value = "Favorites")]
    Favorites
}