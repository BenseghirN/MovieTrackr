using MovieTrackR.Domain.Enums;

namespace MovieTrackR.Domain.Entities;

public class User
{
    public Guid Id { get; init; }  // ← init au lieu de set
    public string Email { get; set; } = string.Empty;
    public string Pseudo { get; set; } = string.Empty;
    public string? ExternalId { get; set; } = string.Empty;
    public string GivenName { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public string AvatarUrl { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.User;
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<ReviewLike> ReviewLikes { get; set; } = new List<ReviewLike>();
    public ICollection<ReviewComment> ReviewComments { get; set; } = new List<ReviewComment>();
    public ICollection<UserList> Lists { get; set; } = new List<UserList>();

    public void PromoteToAdmin() => Role = UserRole.Admin;
    public void DemoteToUser() => Role = UserRole.User;

    public static User Create(
        string externalId,
        string email,
        string pseudo,
        string givenName,
        string surName,
        UserRole role = UserRole.User)
    {
        if (string.IsNullOrWhiteSpace(pseudo)) throw new ArgumentException("Pseudo invalide.");
        if (string.IsNullOrWhiteSpace(givenName)) throw new ArgumentException("Prénom invalide.");
        if (string.IsNullOrWhiteSpace(surName)) throw new ArgumentException("Nom invalide.");
        if (string.IsNullOrWhiteSpace(externalId)) throw new ArgumentException("ID externe invalide.");
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email invalide.");

        return new User
        {
            ExternalId = externalId,
            Email = email,
            Pseudo = pseudo,
            GivenName = givenName,
            Surname = surName,
            Role = role,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void UpdateProfile(
        string? pseudo,
        string? givenName,
        string? surName)
    {
        if (string.IsNullOrWhiteSpace(pseudo))
            throw new ArgumentException("Pseudo invalide.");

        Pseudo = pseudo;

        if (!string.IsNullOrWhiteSpace(givenName))
            GivenName = givenName;

        if (!string.IsNullOrWhiteSpace(surName))
            Surname = surName;
    }

    public void SetAvatar(string avatarUrl)
    {
        if (!string.IsNullOrWhiteSpace(avatarUrl))
            AvatarUrl = avatarUrl;
    }

    public void AddList(UserList list)
    {
        if (!Lists.Any(l => l.Id == list.Id))
            Lists.Add(list);
    }

    public void RemoveList(Guid listId)
    {
        UserList? list = Lists.FirstOrDefault(l => l.Id == listId);
        if (list != null)
            Lists.Remove(list);
    }
}

