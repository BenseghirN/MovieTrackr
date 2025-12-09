import { CommonModule } from '@angular/common';
import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { ProgressSpinner } from 'primeng/progressspinner';
import { UserListService } from '../../services/user-list.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { AuthService } from '../../../../core/services/auth.service';
import { DialogService, DynamicDialogRef } from 'primeng/dynamicdialog';
import { Router } from '@angular/router';
import { UserLists, UserListSummary } from '../../models/user-list.model';
import { ListFormModalComponent } from '../../components/list-form-modal/list-form-modal.component';
import { UserListCardComponent } from '../../components/user-list-card/user-list-card.component';

@Component({
  selector: 'app-my-lists-page',
  standalone: true,
  imports: [CommonModule, ButtonModule, CardModule, ProgressSpinner, UserListCardComponent],
  templateUrl: './my-lists.page.html',
  styleUrl: './my-lists.page.scss',
})
export class MyListsPage implements OnInit {
  private readonly listService = inject(UserListService);
  private readonly authService = inject(AuthService);
  private readonly dialogService = inject(DialogService);
  private readonly router = inject(Router);

  readonly lists = signal<UserLists>([]);
  readonly loading = signal(false);
  readonly error = signal<string | null>(null);

  readonly hasLists = computed(() => this.lists().length > 0);
  readonly isAuthenticated = this.authService.isAuthenticated;

  private dialogRef: DynamicDialogRef | null = null;

  ngOnInit(): void {
    this.loadUserLists();
  }

  onCreateList(): void {
    this.dialogRef = this.dialogService.open(ListFormModalComponent, {
      header: 'Créer une nouvelle liste',
      width: '600px'
    });

    this.dialogRef?.onClose.subscribe((created: boolean) => {
      if (created) {
        this.loadUserLists();
      }
    });
  }

  onViewList(list: UserListSummary): void {
    this.router.navigate(['/my-lists', list.id]);
  }

  onLogin(): void {
    this.authService.login(window.location.pathname);
  }

  onLoadLists(): void {
    this.loadUserLists();
  }

  onDeletedList(list: UserListSummary): void {
    this.lists.update((l) => l.filter(
      (x) => x.id !== list.id
    ));
  }

  onEditedList(updatedList: UserListSummary): void {
    this.lists.update((lists) =>
      lists.map((l) => l.id === updatedList.id ? {
        ...l,
        title: updatedList.title,
        description: updatedList.description
      } : l)
    );
  }

  private loadUserLists(): void {
    this.loading.set(true);
    this.error.set(null);

    this.listService.getMyLists().subscribe({
      next: (lists) => {
        this.lists.set(lists);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        this.lists.set([]);
        this.error.set("Erreur lors du chargement de vos listes.");
      }
    });
  }
}
