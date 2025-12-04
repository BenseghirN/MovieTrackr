export interface PersonDetails {
  id: string;
  tmdbId: number | null;
  name: string;
  profilePath: string;
  biography: string | null;
  birthDate: string | null;
  deathDate: string | null;
  placeOfBirth: string | null;
  knownForDepartment: string | null;
  movieCredits: MovieCredit[];
}

export type PersonMovieCredits = MovieCredit[];

export interface MovieCredit {
  movieId: string;
  tmdbMovieId: number;
  title: string;
  posterPath: string;
  year: number;
  character: string | null;
  job: string | null;
  creditType: string | null;
}