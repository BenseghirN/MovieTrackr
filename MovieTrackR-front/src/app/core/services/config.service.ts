import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class ConfigService {
  readonly apiUrl = environment.apiUrl;
  readonly isProduction = environment.production;

  readonly features = {
    enableComments: true,
    enableReviews: true,
    enableWatchlist: true,
  };
}