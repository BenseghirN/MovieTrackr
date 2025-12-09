export interface UserProfile {
  id: string;
  pseudo: string;
  email: string;
  givenName: string;
  surname: string;
  avatarUrl: string | null;
  reviewsCount: number;
  listsCount: number;
  createdAt: string;
}

export interface UpdatedUserModel {
  pseudo: string;
  givenName: string;
  surname: string;
}