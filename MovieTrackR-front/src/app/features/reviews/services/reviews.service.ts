import { inject, Injectable } from "@angular/core";
import { CreateReviewModel, PagedReviews, ReviewDetails, UpdateReviewModel } from "../models/review.model";
import { ApiService } from "../../../core/services/api.service";
import { Observable } from "rxjs";
import { ConfigService } from "../../../core/services/config.service";

@Injectable({ providedIn: 'root' })
export class ReviewService {
    private readonly api = inject(ApiService);
    private readonly config = inject(ConfigService);

    getMovieReviews(movieId: string, page = 1, pageSize = 10): Observable<PagedReviews> {
        const queryParams = {
            page,
            pageSize
        };

        return this.api.get<PagedReviews>(
            `${this.config.apiUrl}/reviews/by-movie/${movieId}`,
            { params: queryParams, withCredentials: false }
        );
    }

    getReviewById(reviewId: string): Observable<ReviewDetails> {
        return this.api.get<ReviewDetails>(
            `${this.config.apiUrl}/reviews/${reviewId}`,
            { withCredentials: false }
        );
    }

    getUserReviews(userId: string, page = 1, pageSize = 10): Observable<PagedReviews> {
        const queryParams = { page, pageSize };
        return this.api.get<PagedReviews>(
            `${this.config.apiUrl}/reviews/by-user/${userId}`,
            { params: queryParams, withCredentials: true }
        );
    }

    createReview(newReview: CreateReviewModel): Observable<void> {
        return this.api.post<void>(
            `${this.config.apiUrl}/reviews`,
            newReview,
            { withCredentials: true }
        );
    }

    updateReview(reviewId: string, updatedReview: UpdateReviewModel): Observable<void> {
        return this.api.put<void>(
            `${this.config.apiUrl}/reviews/${reviewId}`,
            updatedReview,
            { withCredentials: true }
        );
    }

    deleteReview(reviewId: string): Observable<void> {
        return this.api.delete<void>(
            `${this.config.apiUrl}/reviews/${reviewId}`,
            { withCredentials: true }
        );
    }
};