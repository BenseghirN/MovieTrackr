using System.Runtime.Serialization;

namespace MovieTrackR.Domain.Enums;

public enum UserRole
{
    [EnumMember(Value = "Utilisateur")]
    User,
    [EnumMember(Value = "Administrateur")]
    Admin
}
