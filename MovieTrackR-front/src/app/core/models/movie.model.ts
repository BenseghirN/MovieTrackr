export interface MovieSearchResult {
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

export interface MovieSearchResponse {
  items: MovieSearchResult[];
  meta: PageMeta;
}

export interface MovieSearchParams {
	query?: string | null;
	year?: number | null;
	genreId?: string | null;
	page?: number;
	pageSize?: number;
	sort?: string | null;
}