import { Routes } from '@angular/router';
import { HomePage } from './features/home/pages/home-page/home-page';

export const routes: Routes = [
    {
        path: '',
        component: HomePage
    },
    {
        path: 'movies',
        loadComponent: () => import('./features/movies/pages/movies-page/movies-page').then(m => m.MoviesPage),
    },
    { 
        path: 'movies/:id', 
        loadComponent: () => import('./features/movies/pages/movie-details-page/movie-details-page').then(m => m.MovieDetailsPage)
    },
    {
        path: '**',
        redirectTo: ''
    },
];
