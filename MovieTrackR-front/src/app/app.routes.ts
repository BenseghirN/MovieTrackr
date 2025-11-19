import { Routes } from '@angular/router';

export const routes: Routes = [
    {
        path: '',
        loadComponent: () => import('./features/home/pages/home-page/home-page').then(m => m.HomePage),
    },
    {
        path: 'movies',
        loadComponent: () => import('./features/movies/pages/movies-page/movies-page').then(m => m.MoviesPage),
    },
    {
        path: '**',
        redirectTo: ''
    },
];
