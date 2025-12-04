import { inject, Injectable } from '@angular/core';
import { Observable, throwError } from 'rxjs';
import { ApiService } from '../../../core/services/api.service';
import { ConfigService } from '../../../core/services/config.service';
import { SearchPeopleParams, SearchPeopleResponse, SearchPeopleResult } from '../models/people.model';
import { MovieStreamingOffers } from '../../movies/models/streaming-offers.model';
import { PersonDetails, PersonMovieCredits } from '../models/person-details.model';


@Injectable({ providedIn: 'root' })
export class PeopleService {
    private readonly api = inject(ApiService);
    private readonly config = inject(ConfigService);

	search(params: SearchPeopleParams): Observable<SearchPeopleResponse> {
        const queryParams: Record<string, string | number> = {
            Page: params.page ?? 1,
            PageSize: params.pageSize ?? 20
        };

        if (params.query) queryParams['Query'] = params.query;

		return this.api.get<SearchPeopleResponse>(
            `${this.config.apiUrl}/people/search`,
            { params: queryParams, withCredentials: false }
        );
	}

    getPersonByRouteId(rawId: string): Observable<PersonDetails> {
        if (!rawId)
            return throwError(() => new Error('ID de la personne manquant'));

        const isGuid = rawId.includes('-');

        return isGuid
            ? this.getLocalPerson(rawId)
            : this.getTmdbPerson(Number(rawId));
    }

    getPersonDetails(result: SearchPeopleResult): Observable<PersonDetails> {
        if (result.isLocal && result.id) {
        return this.getLocalPerson(result.id);
        } else if (result.tmdbId) {
        return this.getTmdbPerson(result.tmdbId);
        } else {
        throw new Error('Personne sans ID valide');
        }
    }

    getPersonMovieCredits(id: string): Observable<PersonMovieCredits> {
        if (!id)
            return throwError(() => new Error('ID de la personne manquant'));

        return this.api.get<PersonMovieCredits>(
            `${this.config.apiUrl}/people/${id}/credits`,
            { withCredentials: false }
        );
    }

    private getLocalPerson(localId: string): Observable<PersonDetails> {
        return this.api.get<PersonDetails>(
            `${this.config.apiUrl}/people/${localId}`,
            { withCredentials: false }
        );
    }

    private getTmdbPerson(tmdbId: number): Observable<PersonDetails> {
        return this.api.get<PersonDetails>(
            `${this.config.apiUrl}/people/tmdb/${tmdbId}`,
            { withCredentials: false }
        );
    }
}
