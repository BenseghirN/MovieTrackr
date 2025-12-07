import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from '../../../core/services/api.service';
import { ConfigService } from '../../../core/services/config.service';
import { UserProfile } from '../models/user-profiles.models';
import { UserLists } from '../../user-lists/models/user-list.model';


@Injectable({ providedIn: 'root' })
export class UserProfilesService {
    private readonly api = inject(ApiService);
    private readonly config = inject(ConfigService);

    getProfileById(id: string): Observable<UserProfile> {
        return this.api.get<UserProfile>(
            `${this.config.apiUrl}/profiles/${id}`,
            { withCredentials: false }
        );
    }

    getListsByUser(id: string): Observable<UserLists> {
        return this.api.get<UserLists>(
            `${this.config.apiUrl}/profiles/${id}/lists`,
            { withCredentials: true }
        );
    }    
}
