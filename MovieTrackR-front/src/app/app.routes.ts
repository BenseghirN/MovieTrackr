import { Routes } from '@angular/router';
import { HomePageComponent } from './features/home/pages/home-page/home.page';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
    {
        path: '',
        component: HomePageComponent,
        title: 'Accueil | MovieTrackR'
    },
    {
        path: 'movies',
        loadComponent: () => import('./features/movies/pages/movies/movies.page').then(m => m.MoviesPage),
        title: 'Recherche de films | MovieTrackR'
    },
    { 
        path: 'movies/:id', 
        loadComponent: () => import('./features/movies/pages/movie-details/movie-details.page').then(m => m.MovieDetailsPage),
        title: 'Détails du film | MovieTrackR'
    },
    {
        path: 'my-lists',
        canActivate: [authGuard],
        loadComponent: () => import('./features/user-lists/pages/my-lists/my-lists.page').then(m => m.MyListsPage),
        title: 'Mes listes | MovieTrackR'
    },
    {
        path: 'my-lists/:id',
        canActivate: [authGuard],
        loadComponent: () => import('./features/user-lists/pages/list-details/list-details.page').then(m => m.ListDetailsPage),
        title: 'Détails de la liste | MovieTrackR'
    },
    {
        path: 'people',
        loadComponent: () => import('./features/people/pages/people/people.page').then(m => m.PeoplePage),
        title: 'Recherche de personnes | MovieTrackR'
    },
    { 
        path: 'people/:id', 
        loadComponent: () => import('./features/people/pages/person-details/person-details.page').then(m => m.PersonDetailsPage),
        title: 'Détails de la personne | MovieTrackR'
    },
    { 
        path: 'profiles/:id', 
        loadComponent: () => import('./features/user-profiles/pages/user-profile/user-profile.page').then(m => m.UserProfilePage),
        title: 'Profil utilisateur | MovieTrackR'
    },
    { 
        path: 'me',
        canActivate: [authGuard],
        loadComponent: () => import('./features/user-profiles/pages/user-profile/user-profile.page').then(m => m.UserProfilePage),
        title: 'Mon profil | MovieTrackR'
    },    
    {
        path: 'forbidden',
        loadComponent: () => import('./shared/pages/forbidden/forbidden.page').then(m => m.ForbiddenPageComponent),
        title: 'Accès refusé | MovieTrackR'
    },
    {
        path: 'not-found',
        loadComponent: () => import('./shared/pages/not-found/not-found.page').then(m => m.NotFoundPageComponent),
        title: 'Page introuvable | MovieTrackR'
    },
    {
        path: 'admin',
        loadChildren: () => import('./admin/admin.routes').then(m => m.adminRoutes),
        title: 'Administration | MovieTrackR'
    },
    {
        path: '**',
        redirectTo: 'not-found'
    },
];
