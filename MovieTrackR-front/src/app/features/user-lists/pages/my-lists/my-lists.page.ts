import { CommonModule } from '@angular/common';
import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { ProgressSpinner } from 'primeng/progressspinner';
import { UserListService } from '../../services/user-list.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { AuthService } from '../../../../core/auth/auth-service';
import { DialogService, DynamicDialogRef } from 'primeng/dynamicdialog';
import { Router } from '@angular/router';
import { UserLists, UserListSummary } from '../../models/user-list.model';
import { ListFormModalComponent } from '../../components/list-form-modal/list-form-modal.component';

@Component({
  selector: 'app-my-lists-page',
  standalone: true,
  imports: [CommonModule, ButtonModule, CardModule, ProgressSpinner],
  templateUrl: './my-lists.page.html',
  styleUrl: './my-lists.page.scss',
})
export class MyListsPage implements OnInit {
  private readonly listService = inject(UserListService);
  private readonly notificationService = inject(NotificationService);
  private readonly authService = inject(AuthService);
  private readonly dialogService = inject(DialogService);
  private readonly router = inject(Router);

  protected readonly lists = signal<UserLists>([]);
  public readonly loading = signal(false);
  protected readonly error = signal<string | null>(null);

  public readonly hasLists = computed(() => this.lists().length > 0);
  public readonly isAuthenticated = this.authService.isAuthenticated;

  private dialogRef: DynamicDialogRef | null = null;

  ngOnInit(): void {
    this.loadUserLists();
  }

  public onCreateList(): void {
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

  public onEditList(list: UserListSummary, event: Event): void {
    event.stopPropagation();

    this.dialogRef = this.dialogService.open(ListFormModalComponent, {
      header: 'Modifier la liste',
      width: '600px',
      data: { list }
    });

    this.dialogRef?.onClose.subscribe((created: boolean) => {
      if (created) {
        this.loadUserLists();
      }
    });
  }

  public onDeleteList(list: UserListSummary, event: Event): void {
    event.stopPropagation();

    if (!confirm(`Êtes-vous sûr de vouloir supprimer la liste "${list.title}" ?`)) return;

    this.listService.deleteList(list.id).subscribe({
      next: () => {
        this.notificationService.success(`La liste "${list.title}" a été supprimée.`)
        this.loadUserLists();
      },
      error: () => this.notificationService.error('Impossible de supprimer la liste')
    });
  }

  public onViewList(list: UserListSummary): void {
    this.router.navigate(['/my-lists', list.id]);
  }

  public onLogin(): void {
    this.authService.login(window.location.pathname);
  }

  public onLoadLists(): void {
    this.loadUserLists();
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
