import { PagedResult } from "../../../core/models/paginated-result.model"

export type PagedReviews = PagedResult<ReviewListItem>;
export interface ReviewListItem {
  id: string;
  movieId: string;
  userId: string;
  userName: string;
  rating: number;
  content: string;
  likesCount: number;
  commentsCount: number;
  createdAt: string;
  hasLiked: boolean;
}

export interface ReviewDetails{
  id: string;
  movieId: string;
  userId: string;
  rating: number;
  content: string;
  likesCount: number;
  commentsCount: number;
  createdAt: string;
}

export type PagedComments = PagedResult<ReviewComment>;
export interface ReviewComment {
  id: string;
  reviewId: string;
  userId: string;
  userName: string;
  content: string;
  createdAt: string;
}

export interface CreateReviewModel {
  movieId: string;
  rating: number;
  content: string;
}

export interface UpdateReviewModel {
  rating: number;
  content: string;
}