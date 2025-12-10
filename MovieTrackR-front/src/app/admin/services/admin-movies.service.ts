import { inject, Injectable } from '@angular/core';
import { ApiService } from '../../core/services/api.service';
import { ConfigService } from '../../core/services/config.service';
import { Observable } from 'rxjs';
import { UpdatedUserModel, UserProfile } from '../../features/user-profiles/models/user-profiles.models';
import { MovieDetails } from '../../features/movies/models/movie-details.model';
import { CreateMovieModelForAdministration, UpdateMovieModelForAdministration } from '../models/movie.model';


@Injectable({ providedIn: 'root' })
export class AdminMoviesService {
    private readonly api = inject(ApiService);
    private readonly config = inject(ConfigService);

    getAllMovies(): Observable<MovieDetails[]> {
        return this.api.get<MovieDetails[]>(
            `${this.config.apiUrl}/movies`,
            { withCredentials: true }
        );
    }

    getMovieById(id: string): Observable<MovieDetails> {
        return this.api.get<MovieDetails>(
            `${this.config.apiUrl}/movies/${id}`,
            { withCredentials: true }
        );
    }

    createMovie(id: string, newMovie: CreateMovieModelForAdministration): Observable<void> {
        return this.api.post(
            `${this.config.apiUrl}/movies/${id}`,
            newMovie,
            { withCredentials: true }
        );
    }

    updateMovie(id: string, updateMovie: UpdateMovieModelForAdministration): Observable<void> {
        return this.api.put(
            `${this.config.apiUrl}/movies/${id}`,
            updateMovie,
            { withCredentials: true }
        );
    }

    deleteMovie(id: string): Observable<void> {
        return this.api.delete(
            `${this.config.apiUrl}/movies/${id}`,
            { withCredentials: true }
        );
    }
}
