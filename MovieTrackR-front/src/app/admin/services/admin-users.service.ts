import { inject, Injectable } from '@angular/core';
import { ApiService } from '../../core/services/api.service';
import { ConfigService } from '../../core/services/config.service';
import { Observable } from 'rxjs';
import { AllUsers, UserForAdministration } from '../models/user.model';
import { UpdatedUserModel, UserProfile } from '../../features/user-profiles/models/user-profiles.models';


@Injectable({ providedIn: 'root' })
export class AdminUsersService {
    private readonly api = inject(ApiService);
    private readonly config = inject(ConfigService);

    getAllUsers(): Observable<AllUsers> {
        return this.api.get<AllUsers>(
            `${this.config.apiUrl}/users`,
            { withCredentials: true }
        );
    }

    getUserById(id: string): Observable<UserForAdministration> {
        return this.api.get<UserForAdministration>(
            `${this.config.apiUrl}/users/${id}`,
            { withCredentials: true }
        );
    }

    updateUserInfo(id: string, updatedUser: UpdatedUserModel): Observable<UserProfile> {
        return this.api.put<UserProfile>(
            `${this.config.apiUrl}/users/${id}`,
            updatedUser,
            { withCredentials: true }
        );
    }

    promoteToAdmin(id: string): Observable<UserForAdministration> {
        return this.api.put<UserForAdministration>(
            `${this.config.apiUrl}/users/${id}/promote`,
            {},
            { withCredentials: true }
        );
    }

    demoteToUser(id: string): Observable<UserForAdministration> {
        return this.api.put<UserForAdministration>(
            `${this.config.apiUrl}/users/${id}/demote`,
            {},
            { withCredentials: true }
        );
    }
}
