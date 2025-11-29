import { inject, Injectable } from '@angular/core';
import { Observable, throwError } from 'rxjs';
import { ApiService } from '../../../core/services/api.service';
import { MovieSearchParams, MovieSearchResponse, MovieSearchResult } from '../models/movie.model';
import { MovieDetails } from '../models/movie-details.model';
import { ConfigService } from '../../../core/services/config.service';
import { MovieStreamingOffers } from '../models/streaming-offers.model';


@Injectable({ providedIn: 'root' })
export class MovieService {
    private readonly api = inject(ApiService);
    private readonly config = inject(ConfigService);

	search(params: MovieSearchParams): Observable<MovieSearchResponse> {
        const queryParams: Record<string, string | number> = {
            Page: params.page ?? 1,
            PageSize: params.pageSize ?? 20
        };

        if (params.query) queryParams['Query'] = params.query;
        if (params.year) queryParams['Year'] = params.year;
        if (params.genreId) queryParams['GenreId'] = params.genreId;
        if (params.sort) queryParams['Sort'] = params.sort;


		return this.api.get<MovieSearchResponse>(
            `${this.config.apiUrl}/movies/search`,
            { params: queryParams, withCredentials: false }
        );
	}

    getMovieByRouteId(rawId: string): Observable<MovieDetails> {
        if (!rawId)
            return throwError(() => new Error('ID du film manquant'));

        const isGuid = rawId.includes('-');

        return isGuid
            ? this.getLocalMovie(rawId)
            : this.getTmdbMovie(Number(rawId));
    }

    getMovieDetails(result: MovieSearchResult): Observable<MovieDetails> {
        if (result.isLocal && result.localId) {
        return this.getLocalMovie(result.localId);
        } else if (result.tmdbId) {
        return this.getTmdbMovie(result.tmdbId);
        } else {
        throw new Error('Film sans ID valide');
        }
    }

    getStreamingOffers(tmdbId: number, country: string): Observable<MovieStreamingOffers> {
        if (!tmdbId)
            return throwError(() => new Error('ID du film manquant'));

        return this.api.get<MovieStreamingOffers>(
            `${this.config.apiUrl}/movies/${tmdbId}/streaming`,
            { params: {country: country}, withCredentials: false }
        );
    }

    private getLocalMovie(localId: string): Observable<MovieDetails> {
        return this.api.get<MovieDetails>(
            `${this.config.apiUrl}/movies/${localId}`,
            { withCredentials: false }
        );
    }

    private getTmdbMovie(tmdbId: number): Observable<MovieDetails> {
        return this.api.get<MovieDetails>(
            `${this.config.apiUrl}/movies/tmdb/${tmdbId}`,
            { withCredentials: false }
        );
    }

}
