// Vue 'résumé' des listes appartenant à l'utilisateur authentifié.
export type UserLists = UserListSummary[]
export interface UserListSummary {
  id: string
  title: string
  description?: string
  createdAt: string
  moviesCount: number
}

export interface CreateListModel {
  title: string
  description?: string
}

export interface CreateListResponse {
  id: string;
}

export interface UpdateListModel {
  title: string
  description?: string
}

// Détails d'une liste utilisateur spécifique, incluant les films qu'elle contient.
export interface UserListDetails {
  id: string
  title: string
  description?: string
  createdAt: string
  movies: UserListMovie[]
}

export interface UserListMovie {
  movieId: string
  position: number
  movie: MovieSummary
}

export interface MovieSummary {
  id: string
  title: string
  year?: number
  posterUrl?: string
}

export interface AddMovieToListModel {
  movieId: string
  tmdbId?: number
  position?: number
}

export interface UpdateMoviePositionModel {
  movieId: string
  newPosition: number
}