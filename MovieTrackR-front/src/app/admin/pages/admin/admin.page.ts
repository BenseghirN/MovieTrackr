import { CommonModule } from '@angular/common';
import { Component, signal } from '@angular/core';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { TabsModule } from 'primeng/tabs';
import { AdminUsersTableComponent } from '../../components/admin-users-table/admin-users-table.component';
import { AdminReviewsTableComponent } from '../../components/admin-reviews-table/admin-reviews-table-component';
import { AdminMoviesTableComponent } from '../../components/admin-movies-table/admin-movies-table-component';
import { AdminCommentsTableComponent } from '../../components/admin-comments-table/admin-comments-table-component';

interface AdminTab {
  value: string;
  label: string;
  icon: string;
}

@Component({
  selector: 'app-admin-page',
  imports: [CommonModule, ProgressSpinnerModule, TabsModule, AdminUsersTableComponent, AdminReviewsTableComponent, AdminMoviesTableComponent, AdminCommentsTableComponent],
  templateUrl: './admin.page.html',
  styleUrl: './admin.page.scss',
})
export class AdminPage {
  readonly activeTab = signal('users');
  
  readonly tabs: AdminTab[] = [
    { value: 'users', label: 'Utilisateurs', icon: 'pi pi-users' },
    { value: 'movies', label: 'Films', icon: 'pi pi-video' },
    { value: 'reviews', label: 'Critiques', icon: 'pi pi-star' },
    { value: 'comments', label: 'Commentaires', icon: 'pi pi-comments' }
  ];
}
