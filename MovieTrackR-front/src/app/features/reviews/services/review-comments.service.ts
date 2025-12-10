import { Injectable, inject } from "@angular/core";
import { ApiService } from "../../../core/services/api.service";
import { Observable } from "rxjs";
import { PagedComments, PagedReviews } from "../models/review.model";
import { ConfigService } from "../../../core/services/config.service";

@Injectable({ providedIn: 'root' })
export class ReviewCommentsService {
    private readonly api = inject(ApiService);
    private readonly config = inject(ConfigService);

    getComments(reviewId: string, page = 1, pageSize = 20): Observable<PagedComments> {
        const queryParams = { page, pageSize };
        return this.api.get<PagedComments>(
            `${this.config.apiUrl}/reviews/${reviewId}/comments`,
            { params: queryParams, withCredentials: true }
        );
    }

    createComment(reviewId: string, content: string): Observable<void> {
        return this.api.post<void>(
            `${this.config.apiUrl}/reviews/${reviewId}/comments`,
            { content },
            { withCredentials: true }
        );
    }

    updateComment(reviewId: string, commentId: string, content: string): Observable<void> {
        return this.api.put<void>(
            `${this.config.apiUrl}/reviews/${reviewId}/comments/${commentId}`,
            { content },
            { withCredentials: true }
        );
    }

    deleteComment(reviewId: string, commentId: string): Observable<void> {
        return this.api.delete<void>(
            `${this.config.apiUrl}/reviews/${reviewId}/comments/${commentId}`,
            { withCredentials: true }
        );
    }
}