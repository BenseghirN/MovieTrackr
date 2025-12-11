namespace MovieTrackR.Application.DTOs;

/// <summary>Représente les détails d'un commentaire associé à une review.</summary>
public sealed class CommentDto
{
    /// <summary>Identifiant du commentaire.</summary>
    public Guid Id { get; set; }

    /// <summary>Identifiant de la review concernée.</summary>
    public Guid ReviewId { get; set; }

    /// <summary>Identifiant de l'auteur du commentaire.</summary>
    public Guid UserId { get; set; }

    /// <summary>Pseudo de l'auteur du commentaire.</summary>
    public string UserName { get; set; } = default!;

    /// <summary>Contenu textuel du commentaire.</summary>
    public string Content { get; set; } = default!;

    /// <summary>Définis si le commentaire est visible publiquement ou pas (modération)</summary>
    public bool PubliclyVisible { get; set; } = true;

    /// <summary>Date de création (UTC).</summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>Payload pour la création d'un commentaire de review.</summary>
public sealed class CommentCreateDto
{
    /// <summary>Contenu du commentaire.</summary>
    public string Content { get; set; } = default!;
}

/// <summary>Payload pour la mise à jour d'un commentaire de review.</summary>
public sealed class CommentUpdateDto
{
    /// <summary>Nouveau contenu du commentaire.</summary>
    public string Content { get; set; } = default!;
}

/// <summary>Modèle pour un résultat paginé générique.</summary>
public sealed class PagedResult<T>
{
    /// <summary>Éléments de la page courante.</summary>
    public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();

    /// <summary>Nombre total d'éléments (toutes pages confondues).</summary>
    public int TotalCount { get; init; }

    /// <summary>Index de page actuel (1-based).</summary>
    public int Page { get; init; }

    /// <summary>Taille de page (nombre d'éléments).</summary>
    public int PageSize { get; init; }

    /// <summary>
    /// Nombre total de pages (calculé).
    /// </summary>
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling((double)TotalCount / PageSize);

    /// <summary>Construit un résultat paginé.</summary>
    public PagedResult() { }

    /// <summary>Construit un résultat paginé avec données.</summary>
    public PagedResult(IReadOnlyList<T> items, int totalCount, int page, int pageSize)
    {
        Items = items;
        TotalCount = totalCount;
        Page = page;
        PageSize = pageSize;
    }
}