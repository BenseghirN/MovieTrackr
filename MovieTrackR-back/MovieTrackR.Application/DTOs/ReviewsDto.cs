namespace MovieTrackR.Application.DTOs;

/// <summary>
/// Modèle de création d'une critique (review) pour un film.
/// </summary>
/// <remarks>
/// Une review est unique par couple (Utilisateur, Film).
/// La note est exprimée sur 5.
/// </remarks>
public sealed class CreateReviewDto
{
    /// <summary>Identifiant du film concerné par la critique.</summary>
    public Guid MovieId { get; set; }
    /// <summary>Note attribuée au film (0 à 5).</summary>
    public float Rating { get; set; }
    /// <summary>Contenu textuel de la critique (optionnel).</summary>
    public string? Content { get; set; }
}

/// <summary>Modèle de mise à jour d'une critique (review).</summary>
public sealed class UpdateReviewDto
{
    /// <summary>Nouvelle note attribuée (0 à 10).</summary>
    public float Rating { get; set; }
    /// <summary>Nouveau contenu textuel de la critique (optionnel).</summary>
    public string? Content { get; set; }
}

/// <summary>Élément d'une liste de critiques.</summary>
public sealed class ReviewListItemDto
{
    /// <summary>Identifiant de la critique.</summary>
    public Guid Id { get; set; }

    /// <summary>Identifiant du film.</summary>
    public Guid MovieId { get; set; }

    /// <summary>Identifiant de l'auteur de la critique.</summary>
    public Guid UserId { get; set; }

    /// <summary>Note attribuée (0 à 10).</summary>
    public float Rating { get; set; }

    /// <summary>Texte de la critique (optionnel).</summary>
    public string? Content { get; set; }

    /// <summary>Nombre total de likes sur cette critique.</summary>
    public int LikesCount { get; set; }

    /// <summary>Nombre total de commentaires associés.</summary>
    public int CommentsCount { get; set; }

    /// <summary>Date de création (UTC).</summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>Détails complets d'une critique.</summary>
public sealed class ReviewDetailsDto
{
    /// <summary>Identifiant de la critique.</summary>
    public Guid Id { get; set; }

    /// <summary>Identifiant du film.</summary>
    public Guid MovieId { get; set; }

    /// <summary>Identifiant de l'auteur de la critique.</summary>
    public Guid UserId { get; set; }

    /// <summary>Note attribuée (0 à 10).</summary>
    public float Rating { get; set; }

    /// <summary>Texte de la critique (optionnel).</summary>
    public string? Content { get; set; }

    /// <summary>Nombre total de likes.</summary>
    public int LikesCount { get; set; }

    /// <summary>Nombre total de commentaires.</summary>
    public int CommentsCount { get; set; }

    /// <summary>Date de création (UTC).</summary>
    public DateTime CreatedAt { get; set; }
}
