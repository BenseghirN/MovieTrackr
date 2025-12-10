import { Routes } from '@angular/router';
import { adminGuard } from '../core/guards/admin.guard';

export const adminRoutes: Routes = [
    {
        path: '',
        canActivate: [adminGuard],
        loadComponent: () => import('./pages/admin/admin.page').then(m => m.AdminPage)
    },
];
