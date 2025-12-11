export type AllUsers = UserForAdministration[]

export interface UserForAdministration {
  id: string
  externalId: string
  email: string
  pseudo: string
  givenName: string
  surname: string
  avatarUrl: string
  role: string
}