import { inject, Injectable } from "@angular/core";
import { environment } from "../../../environments/environment";
import { CreateReviewModel, PagedReviews, ReviewDetails, UpdateReviewModel } from "../models/review.model";
import { ApiService } from "./api.service";
import { Observable } from "rxjs";

@Injectable({ providedIn: 'root' })
export class reviewService {
    private readonly api = inject(ApiService);
    private readonly baseUrl = environment.apiUrl;

    getMovieReviews(movieId: string, page = 1, pageSize = 10): Observable<PagedReviews> {
        const queryParams = {
            page,
            pageSize
        };

        return this.api.get<PagedReviews>(
            `${this.baseUrl}/api/v1/reviews/by-movie/${movieId}`,
            { params: queryParams, withCredentials: true }
        );
    }

    getReviewById(reviewId: string): Observable<ReviewDetails> {
        return this.api.get<ReviewDetails>(
            `${this.baseUrl}/api/v1/reviews/${reviewId}`,
            { withCredentials: true }
        );
    }

    getUserReviews(userId: string, page = 1, pageSize = 10): Observable<PagedReviews> {
        const queryParams = { page, pageSize };
        return this.api.get<PagedReviews>(
            `${this.baseUrl}/api/v1/reviews/by-user/${userId}`,
            { params: queryParams, withCredentials: true }
        );
    }

    createReview(dto: CreateReviewModel): Observable<void> {
        return this.api.post<void>(
            `${this.baseUrl}/api/v1/reviews`,
            dto,
            { withCredentials: true }
        );
    }

    updateReview(reviewId: string, dto: UpdateReviewModel): Observable<void> {
        return this.api.put<void>(
            `${this.baseUrl}/api/v1/reviews/${reviewId}`,
            dto,
            { withCredentials: true }
        );
    }

    deleteReview(reviewId: string): Observable<void> {
        return this.api.delete<void>(
            `${this.baseUrl}/api/v1/reviews/${reviewId}`,
            { withCredentials: true }
        );
    }
};