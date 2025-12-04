export interface SearchMovieResult {
  localId: string | null;
  tmdbId: number;
  title: string;
  year: number | null;
  originalTitle: string | null;
  posterPath: string | null;
  isLocal: boolean;
  voteAverage: number | null;
  popularity: number;
  overview: string | null;
}

export interface PageMeta {
  page: number;
  pageSize: number;
  totalLocal: number;
  totalTmdb: number;
  totalResults: number;
  totalTmdbPages: number | null;
  hasMore: boolean;
}

export interface SearchMovieResponse {
  items: SearchMovieResult[];
  meta: PageMeta;
}

export interface SearchMovieParams {
	query?: string | null;
	year?: number | null;
	page?: number;
	pageSize?: number;
}