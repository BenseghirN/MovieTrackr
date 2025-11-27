import { Routes } from '@angular/router';
import { HomePageComponent } from './features/home/pages/home-page/home.page';

export const routes: Routes = [
    {
        path: '',
        component: HomePageComponent
    },
    {
        path: 'movies',
        loadComponent: () => import('./features/movies/pages/movies/movies.page').then(m => m.MoviesPage),
    },
    { 
        path: 'movies/:id', 
        loadComponent: () => import('./features/movies/pages/movie-details/movie-details.page').then(m => m.MovieDetailsPage)
    },

    // {
    //     path: 'profile',
    //     canActivate: [authGuard],
    //     loadComponent: () => import('./features/users/pages/profile-page/profile-page.component').then(m => m.ProfilePageComponent)
    // },
    // {
    //     path: 'admin',
    //     canActivate: [authGuard, adminGuard],
    //     loadChildren: () => import('./features/admin/admin.routes')
    // },
    {
        path: 'forbidden',
        loadComponent: () => import('./shared/pages/forbidden/forbidden.page').then(m => m.ForbiddenPageComponent)
    },
    {
        path: 'not-found',
        loadComponent: () => import('./shared/pages/not-found/not-found.page').then(m => m.NotFoundPageComponent)
    },
    {
        path: '**',
        redirectTo: 'not-found'
    },
];
