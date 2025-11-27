import { Genre } from "../../../core/models/genre.model";

export interface MovieDetails {
  id: string;
  tmdbId: number | null;
  title: string;
  originalTitle: string | null;
  year: number | null;
  posterUrl: string | null;
  backdropPath: string | null;
  trailerUrl: string | null;
  duration: number | null;
  overview: string | null;
  releaseDate: string | null;
  voteAverage: number | null;
  createdAt: string;
  genres: Genre[];
  cast: CastMember[];
  crew: CrewMember[];
}

export interface CastMember {
  name: string;
  character: string | null;
  profilePath: string | null;
  order: number | null;
}

export interface CrewMember {
  name: string;
  job: string;
  department: string | null;
  profilePath: string | null;
}