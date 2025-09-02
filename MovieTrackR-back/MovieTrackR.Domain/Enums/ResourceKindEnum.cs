using System.Runtime.Serialization;

namespace MovieTrackR.Domain.Enums;

public enum ResourceKind
{
    [EnumMember(Value = "Review")]
    Review,
    [EnumMember(Value = "UserList")]
    UserList
}
