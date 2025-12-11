import { Routes } from '@angular/router';
import { HomePageComponent } from './features/home/pages/home-page/home.page';
import { authGuard } from './core/guards/auth.guard';

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
    {
        path: 'my-lists',
        canActivate: [authGuard],
        loadComponent: () => import('./features/user-lists/pages/my-lists/my-lists.page').then(m => m.MyListsPage)
    },
    {
        path: 'my-lists/:id',
        canActivate: [authGuard],
        loadComponent: () => import('./features/user-lists/pages/list-details/list-details.page').then(m => m.ListDetailsPage)
    },
    {
        path: 'people',
        loadComponent: () => import('./features/people/pages/people/people.page').then(m => m.PeoplePage),
    },
    { 
        path: 'people/:id', 
        loadComponent: () => import('./features/people/pages/person-details/person-details.page').then(m => m.PersonDetailsPage)
    },
    { 
        path: 'profiles/:id', 
        loadComponent: () => import('./features/user-profiles/pages/user-profile/user-profile.page').then(m => m.UserProfilePage)
    },
    { 
        path: 'me',
        canActivate: [authGuard],
        loadComponent: () => import('./features/user-profiles/pages/user-profile/user-profile.page').then(m => m.UserProfilePage)
    },    
    {
        path: 'forbidden',
        loadComponent: () => import('./shared/pages/forbidden/forbidden.page').then(m => m.ForbiddenPageComponent)
    },
    {
        path: 'not-found',
        loadComponent: () => import('./shared/pages/not-found/not-found.page').then(m => m.NotFoundPageComponent)
    },
    {
        path: 'admin',
        loadChildren: () => import('./admin/admin.routes').then(m => m.adminRoutes)
    },
    {
        path: '**',
        redirectTo: 'not-found'
    },
];
