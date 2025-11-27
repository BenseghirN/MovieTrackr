import { Injectable, inject } from "@angular/core";
import { Observable } from "rxjs";
import { environment } from "../../../environments/environment";
import { ApiService } from "./api.service";

@Injectable({ providedIn: 'root' })
export class reviewLikesService {
    private readonly api = inject(ApiService);
    private readonly baseUrl = environment.apiUrl;

    likeReview(reviewId: string): Observable<void> {
        return this.api.post<void>(
            `${this.baseUrl}/api/v1/reviews/${reviewId}/likes`,
            {},
            { withCredentials: true }
        );
    }

    unlikeReview(reviewId: string): Observable<void> {
        return this.api.delete<void>(
            `${this.baseUrl}/api/v1/reviews/${reviewId}/likes`,
            { withCredentials: true }
        );
    }
}