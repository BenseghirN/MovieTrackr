import { CommonModule } from '@angular/common';
import { Component, inject, OnInit, signal } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { TableModule } from 'primeng/table';
import { ToggleSwitchModule } from 'primeng/toggleswitch';
import { AdminUsersService } from '../../services/admin-users.service';
import { AllUsers, UserForAdministration } from '../../models/user.model';
import { FormsModule } from '@angular/forms';
import { NotificationService } from '../../../core/services/notification.service';
import { TooltipModule } from 'primeng/tooltip';
import { RouterLink } from '@angular/router';
import { Column } from '../../models/table.model';

@Component({
  selector: 'app-admin-users-table',
  standalone: true,
  imports: [CommonModule, ProgressSpinnerModule, TableModule, ButtonModule, ConfirmDialogModule, ToggleSwitchModule, FormsModule, TooltipModule, RouterLink],
  templateUrl: './admin-users-table.component.html',
  styleUrl: './admin-users-table.component.scss',
})
export class AdminUsersTableComponent implements OnInit {
  private readonly adminUserService = inject(AdminUsersService);
  private readonly notificationService = inject(NotificationService);

  readonly users = signal<UserForAdministration[]>([]);
  readonly loading = signal(false);
  readonly error = signal<string | null>(null);

  ngOnInit(): void {
    this.loadUsers();
  }

  private loadUsers(): void {
    this.loading.set(true);
    this.error.set(null);

    this.adminUserService.getAllUsers().subscribe({
      next: (result: AllUsers) => {
        this.users.set(result);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        this.error.set('Impossible de charger la liste des utilisateurs.');
        this.notificationService.error(this.error()!);
      }
    });
  }

  onToggleRole(user: UserForAdministration): void {
    this.loading.set(true);
    const isUserAdmin = this.isAdmin(user);
    const action = isUserAdmin
      ? this.adminUserService.demoteToUser(user.id)
      : this.adminUserService.promoteToAdmin(user.id)

    action.subscribe({
      next: (updatedUser: UserForAdministration) => {
        this.loading.set(false);
        this.users.update((users) => 
          users.map((u) => u.id === updatedUser.id ? {
            ...u,
            role: updatedUser.role
          } : u)
        );
        this.notificationService.success(isUserAdmin ? `Utilisateur ${updatedUser.pseudo} rétrogradé avec succès.` : `Utilisateur ${updatedUser.pseudo} promu avec succès.`);
      },
      error: () => {
        this.loading.set(false);
        this.notificationService.error(`Impossible de changer le role de l'utilisateur.`);
      }
    });
  }

  isAdmin(user: UserForAdministration): boolean{
    return user.role === 'Admin';
  }

  getInitial(user: UserForAdministration): string {
    return (user.pseudo?.charAt(0) || user.email?.charAt(0) || '?').toUpperCase();
  }
}
