import { inject, Injectable } from "@angular/core";
import { ApiService } from "../../../core/services/api.service";
import { Observable } from "rxjs";
import { ConfigService } from "../../../core/services/config.service";
import { 
    AddMovieToListModel, 
    CreateListModel, 
    CreateListResponse, 
    UpdateListModel, 
    UpdateMoviePositionModel, 
    UserListDetails, 
    UserLists, 
    UserListSummary
} from "../models/user-list.model";

@Injectable({ providedIn: 'root' })
export class UserListService {
    private readonly api = inject(ApiService);
    private readonly config = inject(ConfigService);

    getMyLists(): Observable<UserLists> {
        return this.api.get<UserLists>(
            `${this.config.apiUrl}/me/lists`,
            { withCredentials: true }
        );
    }

    getListDetails(listId: string): Observable<UserListDetails> {
        return this.api.get<UserListDetails>(
            `${this.config.apiUrl}/me/lists/${listId}`,
            { withCredentials: true }
        );
    }

    createList(newList: CreateListModel): Observable<CreateListResponse> {
        return this.api.post<CreateListResponse>(`${this.config.apiUrl}/me/lists`,
            newList,
            { withCredentials: true }
        );
    }

    updateList(listId: string, updatedList: UpdateListModel): Observable<UserListSummary> {
        return this.api.put<UserListSummary>(`${this.config.apiUrl}/me/lists/${listId}`,
            updatedList,
            { withCredentials: true }
        );
    }

    deleteList(listId: string): Observable<void> {
        return this.api.delete<void>(`${this.config.apiUrl}/me/lists/${listId}`,
            { withCredentials: true }
        );
    }

    addMovieToList(listId: string, movieToAdd: AddMovieToListModel): Observable<void> {
        return this.api.post<void>(`${this.config.apiUrl}/me/lists/${listId}/movie`, 
            movieToAdd,
            { withCredentials: true }
        );
    }

    removeMovieFromList(listId: string, movieId: string): Observable<void> {
        return this.api.delete<void>(`${this.config.apiUrl}/me/lists/${listId}/movie/${movieId}`,
            { withCredentials: true }
        );
    }

    reorderMovieInList(listId: string, movieToMove: UpdateMoviePositionModel): Observable<void> {
        return this.api.put<void>(`${this.config.apiUrl}/me/lists/${listId}/movie/reorder`,
            movieToMove,
            { withCredentials: true }
        );
    }
};