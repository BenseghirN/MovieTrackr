import { Injectable, inject } from "@angular/core";
import { Observable } from "rxjs";
import { ApiService } from "../../core/services/api.service";
import { ConfigService } from "../../core/services/config.service";
import { ReviewComment } from "../../features/reviews/models/review.model";

@Injectable({ providedIn: 'root' })
export class AdminCommentsService {
    private readonly api = inject(ApiService);
    private readonly config = inject(ConfigService);

    getAllComments(): Observable<ReviewComment[]> {
        return this.api.get<ReviewComment[]>(
            `${this.config.apiUrl}/comments`,
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

    changeCommentVisibility(id: string): Observable<void> {
        return this.api.put<void>(
            `${this.config.apiUrl}/comments/${id}`,
            {},
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
