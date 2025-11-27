import { Injectable, inject } from "@angular/core";
import { environment } from "../../../environments/environment";
import { ApiService } from "./api.service";
import { Observable } from "rxjs";
import { PagedReviewComments } from "../models/review.model";

@Injectable({ providedIn: 'root' })
export class reviewCommentsService {
    private readonly api = inject(ApiService);
    private readonly baseUrl = environment.apiUrl;

    getComments(reviewId: string, page = 1, pageSize = 20): Observable<PagedReviewComments> {
        const queryParams = { page, pageSize };
        return this.api.get<PagedReviewComments>(
            `${this.baseUrl}/api/v1/reviews/${reviewId}/comments`,
            { params: queryParams, withCredentials: true }
        );
    }

    createComment(reviewId: string, content: string): Observable<void> {
        return this.api.post<void>(
            `${this.baseUrl}/api/v1/reviews/${reviewId}/comments`,
            { content },
            { withCredentials: true }
        );
    }

    updateComment(reviewId: string, commentId: string, content: string): Observable<void> {
        return this.api.put<void>(
            `${this.baseUrl}/api/v1/reviews/${reviewId}/comments/${commentId}`,
            { content },
            { withCredentials: true }
        );
    }

    deleteComment(reviewId: string, commentId: string): Observable<void> {
        return this.api.delete<void>(
            `${this.baseUrl}/api/v1/reviews/${reviewId}/comments/${commentId}`,
            { withCredentials: true }
        );
    }
}