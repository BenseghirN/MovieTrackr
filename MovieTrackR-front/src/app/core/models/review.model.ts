export interface ReviewDetails{
  id: string
  movieId: string
  userId: string
  rating: number
  content: string
  likesCount: number
  commentsCount: number
  createdAt: string
}

export interface PagedReviews {
  items: ReviewListItem[]
  totalCount: number
  page: number
  pageSize: number
  totalPages: number
}

export interface ReviewListItem {
  id: string
  movieId: string
  userId: string
  userName: string
  rating: number
  content: string
  likesCount: number
  commentsCount: number
  createdAt: string
  hasLiked: boolean
}

export interface PagedReviewComments {
  items: ReviewComment[]
  totalCount: number
  page: number
  pageSize: number
  totalPages: number
}

export interface ReviewComment {
  id: string
  reviewId: string
  userId: string
  content: string
  createdAt: string
}

export interface CreateReviewModel {
  movieId: string
  rating: number
  content: string
}

export interface UpdateReviewModel {
  rating: number
  content: string
}