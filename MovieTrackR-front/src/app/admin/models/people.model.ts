export interface PersonForAdministration {
  id: string
  tmdbId: number
  name: string
  profilePath: string
  biography: string
  birthDate: string
  deathDate: string
  placeOfBirth: string
  knownForDepartment: string
  movieCredits: MovieCredit[]
}

export interface MovieCredit {
  movieId: string
  tmdbMovieId: number
  title: string
  posterPath: string
  year: number
  character: string
  job: string
  creditType: string
}