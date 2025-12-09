export interface SearchPeopleResult {
  id: string | null;
  tmdbId: number;
  name: string;
  profilePath: string | null;
  knownForDepartment: string | null;
  isLocal: boolean;
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

export interface SearchPeopleResponse {
  items: SearchPeopleResult[];
  meta: PageMeta;
}

export interface SearchPeopleParams {
	query?: string | null;
	page?: number;
	pageSize?: number;
}