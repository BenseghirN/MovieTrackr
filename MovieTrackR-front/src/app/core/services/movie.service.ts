import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { MovieSearchParams, MovieSearchResponse } from '../models/movie.model';
import { environment } from '../../../environments/environment';


@Injectable({ providedIn: 'root' })
export class MovieService {
    private readonly api = inject(ApiService);
    private readonly baseUrl = environment.apiUrl;

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
            `${this.baseUrl}/api/v1/movies/search`,
            { params: queryParams, withCredentials: true }
        );
	}
}
