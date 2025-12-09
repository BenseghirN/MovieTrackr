import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { Genre } from '../models/genre.model';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class GenresService {
    private readonly api = inject(ApiService);
    private readonly baseUrl = environment.apiUrl;
  
    getAll(): Observable<Genre[]> {
        return this.api.get<Genre[]>(
        `${this.baseUrl}/api/v1/genres`
        );
    }
}