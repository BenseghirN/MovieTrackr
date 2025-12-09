import { Injectable, inject } from "@angular/core";
import { Observable } from "rxjs";
import { ApiService } from "../../../core/services/api.service";
import { ConfigService } from "../../../core/services/config.service";

@Injectable({ providedIn: 'root' })
export class ReviewLikesService {
    private readonly api = inject(ApiService);
    private readonly config = inject(ConfigService);

    likeReview(reviewId: string): Observable<void> {
        return this.api.post<void>(
            `${this.config.apiUrl}/reviews/${reviewId}/likes`, 
            null,
            { withCredentials: true }
        );
    }

    unlikeReview(reviewId: string): Observable<void> {
        return this.api.delete<void>(
            `${this.config.apiUrl}/reviews/${reviewId}/likes`,
            { withCredentials: true }
        );
    }
}