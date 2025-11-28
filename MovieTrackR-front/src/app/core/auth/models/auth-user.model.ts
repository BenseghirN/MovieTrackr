export interface AuthUser {
  id: string
  externalId: string
  email: string
  pseudo: string
  givenName: string
  surname: string
  role: 'User' | 'Admin';
}

export interface MeClaims {
  claims: Array<{
    type: string;
    value: string;
  }>;
}