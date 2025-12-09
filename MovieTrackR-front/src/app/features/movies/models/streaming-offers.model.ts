export interface MovieStreamingOffers {
  country: string
  link: string
  flatrate: Flatrate[]
  free: Free[]
}

export interface Flatrate {
  providerId: number
  providerName: string
  logoPath: string
}

export interface Free {
  providerId: number
  providerName: string
  logoPath: string
}
