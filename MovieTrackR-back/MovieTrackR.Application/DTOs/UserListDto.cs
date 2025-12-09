using MovieTrackR.Domain.Enums;

namespace MovieTrackR.Application.DTOs;

/// <summary>Représente une liste d'utilisateur (vue "résumé").</summary>
public class UserListDto
{
    /// <summary>Identifiant unique de la liste.</summary>
    public Guid Id { get; set; }

    /// <summary>Titre de la liste.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Description optionnelle de la liste.</summary>
    public string? Description { get; set; }

    /// <summary>Date de création de la liste (UTC).</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Nombre de films présents dans la liste.</summary>
    public int MoviesCount { get; set; }

    /// <summary>Type de liste utilisateur.</summary>
    public UserListType Type { get; set; }

    /// <summary>Définit si la liste est une liste systeme défaut ou non.</summary>
    public bool IsSystemList { get; set; }

    /// <summary>Id de l'utilisateur à qui appartient cette liste</summary>
    public Guid userId { get; set; }
}

/// <summary>Représente une liste d'utilisateur (vue "détails" avec les films ordonnés).</summary>
public class UserListDetailsDto
{
    /// <summary>Identifiant unique de la liste.</summary>
    public Guid Id { get; set; }

    /// <summary>Titre de la liste.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Description optionnelle de la liste.</summary>
    public string? Description { get; set; }

    /// <summary>Date de création de la liste (UTC).</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Type de liste utilisateur.</summary>
    public UserListType Type { get; set; }

    /// <summary>Définit si la liste est une liste systeme défaut ou non.</summary>
    public bool IsSystemList { get; set; }

    /// <summary>Films contenus dans la liste, triés par position croissante.</summary>
    public IReadOnlyList<UserListMovieDto> Movies { get; set; } = Array.Empty<UserListMovieDto>();
}

/// <summary>Élément (film) appartenant à une liste d'utilisateur.</summary>
public class UserListMovieDto
{
    /// <summary>Identifiant du film (local GUID).</summary>
    public Guid MovieId { get; set; }

    /// <summary>Position d’affichage/tri dans la liste.</summary>
    public int Position { get; set; }

    /// <summary>Résumé du film pour affichage dans une liste.</summary>
    public MovieSummaryDto Movie { get; set; } = null!;
}

/// <summary>Résumé d’un film, utile pour lister des éléments de liste.</summary>
public class MovieSummaryDto
{
    /// <summary>Identifiant du film (local GUID).</summary>
    public Guid Id { get; set; }

    /// <summary>Titre du film.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Année de sortie (si connue).</summary>
    public int? Year { get; set; }

    /// <summary>URL complète de l’affiche (si disponible).</summary>
    public string? PosterUrl { get; set; }
}

/// <summary>Données pour créer une nouvelle liste utilisateur.</summary>
public sealed record CreateListDto
{
    /// <summary>Titre de la liste.</summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>Description optionnelle de la liste.</summary>
    public string? Description { get; init; }
}

/// <summary>Données pour mettre à jour le titre et/ou la description d’une liste.</summary>
public sealed record UpdateListDto
{
    /// <summary>Nouveau titre de la liste.</summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>Nouvelle description (optionnelle).</summary>
    public string? Description { get; init; }
}

/// <summary>Données pour ajouter un film à une liste utilisateur.</summary>
public sealed record AddMovieToListDto
{
    /// <summary>Identifiant local du film (si déjà importé).</summary>
    public Guid? MovieId { get; init; }

    /// <summary>Identifiant TMDb du film (si MovieId non fourni).</summary>
    public int? TmdbId { get; init; }

    /// <summary>Position souhaitée (optionnelle); par défaut, ajout en fin.</summary>
    public int? Position { get; init; }
}

/// <summary>Données pour changer la position d’un film dans une liste.</summary>
public sealed record ReorderListItemDto
{
    /// <summary>Identifiant du film à déplacer.</summary>
    public Guid MovieId { get; init; }

    /// <summary>Nouvelle position du film dans la liste.</summary>
    public int NewPosition { get; init; }
}
