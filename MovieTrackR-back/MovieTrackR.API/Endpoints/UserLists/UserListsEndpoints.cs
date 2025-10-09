using Asp.Versioning;
using Asp.Versioning.Builder;
using MovieTrackR.API.Filters;
using MovieTrackR.Application.DTOs;

namespace MovieTrackR.API.Endpoints.UserLists;

public static class UserListsEndpoints
{
  public static IEndpointRouteBuilder MapMoviesEndpoints(this IEndpointRouteBuilder app)
  {
    ApiVersionSet vset = app.NewApiVersionSet()
        .HasApiVersion(new ApiVersion(1, 0))
        .Build();

    RouteGroupBuilder group = app.MapGroup("/api/v{version:apiVersion}/me/lists")
        .WithApiVersionSet(vset)
        .MapToApiVersion(1, 0)
        .WithTags("UserLists")
        .WithOpenApi();

    group.MapGet("/", UserListsHandlers.GetMine)
      .WithName("Get_My_Lists")
      .WithSummary("Récupère les listes de l'utilisateur courant.")
      .WithDescription("Retourne la vue 'résumé' des listes appartenant à l'utilisateur authentifié.")
      .Produces<IReadOnlyList<UserListDto>>(StatusCodes.Status200OK);

    group.MapGet("/{id:guid}", UserListsHandlers.GetById)
      .WithName("Get_My_List_ById")
      .WithSummary("Récupère une de mes listes par son identifiant.")
      .WithDescription("Retourne la vue 'détails' d’une liste, incluant les films ordonnés par position. Restreint au propriétaire de la liste.")
      .Produces<UserListDetailsDto>(StatusCodes.Status200OK)
      .Produces(StatusCodes.Status404NotFound);

    group.MapPost("/", UserListsHandlers.Create)
      .AddEndpointFilter<ValidationFilter<CreateListDto>>()
      .WithName("Create_My_List")
      .WithSummary("Crée une nouvelle liste pour l'utilisateur courant.")
      .Accepts<CreateListDto>("application/json")
      .Produces(StatusCodes.Status201Created)
      .ProducesProblem(StatusCodes.Status400BadRequest)
      .ProducesProblem(StatusCodes.Status409Conflict);

    group.MapPut("/{id:guid}", UserListsHandlers.Update)
      .AddEndpointFilter<ValidationFilter<UpdateListDto>>()
      .WithName("Update_My_List")
      .WithSummary("Met à jour une de mes listes.")
      .Accepts<UpdateListDto>("application/json")
      .Produces(StatusCodes.Status204NoContent)
      .ProducesProblem(StatusCodes.Status400BadRequest)
      .Produces(StatusCodes.Status404NotFound)
      .ProducesProblem(StatusCodes.Status409Conflict);

    group.MapDelete("/{id:guid}", UserListsHandlers.Delete)
      .WithName("Delete_My_List")
      .WithSummary("Supprime une de mes listes.")
      .Produces(StatusCodes.Status204NoContent)
      .Produces(StatusCodes.Status404NotFound);

    group.MapPost("/{id:guid}/movie", UserListsHandlers.AddItem)
      .AddEndpointFilter<ValidationFilter<AddMovieToListDto>>()
      .WithName("Add_Movie_To_My_List")
      .WithSummary("Ajoute un film à l’une de mes listes (import TMDb si nécessaire).")
      .Accepts<AddMovieToListDto>("application/json")
      .Produces(StatusCodes.Status204NoContent)
      .ProducesProblem(StatusCodes.Status400BadRequest)
      .Produces(StatusCodes.Status404NotFound);

    group.MapDelete("/{id:guid}/movie/{movieId:guid}", UserListsHandlers.RemoveItem)
      .WithName("Remove_Movie_From_My_List")
      .WithSummary("Retire un film de l’une de mes listes.")
      .Produces(StatusCodes.Status204NoContent)
      .Produces(StatusCodes.Status404NotFound);

    group.MapPut("/{id:guid}/movie/reorder", UserListsHandlers.ReorderItem)
      .AddEndpointFilter<ValidationFilter<ReorderListItemDto>>()
      .WithName("Reorder_My_List_Item")
      .WithSummary("Change la position d’un film dans l’une de mes listes.")
      .Accepts<ReorderListItemDto>("application/json")
      .Produces(StatusCodes.Status204NoContent)
      .ProducesProblem(StatusCodes.Status400BadRequest)
      .Produces(StatusCodes.Status404NotFound);

    return app;
  }
}