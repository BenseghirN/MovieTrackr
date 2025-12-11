import { Injectable, inject } from "@angular/core";
import { Observable } from "rxjs";
import { ApiService } from "../../core/services/api.service";
import { ConfigService } from "../../core/services/config.service";
import { ReviewDetails, ReviewListItem, UpdateReviewModel } from "../../features/reviews/models/review.model";

@Injectable({ providedIn: 'root' })
export class AdminReviewsService {
    private readonly api = inject(ApiService);
    private readonly config = inject(ConfigService);

    getAllReviews(): Observable<ReviewListItem[]> {
        return this.api.get<ReviewListItem[]>(
            `${this.config.apiUrl}/reviews`,
            { withCredentials: true }
        );
    }

    getReviewById(id: string): Observable<ReviewDetails> {
        return this.api.get<ReviewDetails>(
            `${this.config.apiUrl}/reviews/${id}`,
            { withCredentials: true }
        );
    }

    updateReview(id: string, updatedReview: UpdateReviewModel): Observable<void> {
        return this.api.put<void>(
            `${this.config.apiUrl}/reviews/${id}`,
            updatedReview,
            { withCredentials: true }
        );
    }

    changeReviewVisibility(id: string): Observable<ReviewListItem> {
        return this.api.put<ReviewListItem>(
            `${this.config.apiUrl}/reviews/${id}/togglevisible`,
            {},
            { withCredentials: true }
        );
    }

    deleteReview(id: string): Observable<void> {
        return this.api.delete(
            `${this.config.apiUrl}/movies/${id}`,
            { withCredentials: true }
        );
    }
}
