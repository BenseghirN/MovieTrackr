import { CommonModule } from '@angular/common';
import { Component, inject, input, OnInit, signal } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { Popover, PopoverModule } from 'primeng/popover';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { UserListService } from '../../services/user-list.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { AuthService } from '../../../../core/auth/auth-service';
import { UserLists, UserListSummary } from '../../models/user-list.model';
import { DialogService, DynamicDialogRef } from 'primeng/dynamicdialog';
import { Tooltip } from 'primeng/tooltip';
import { ListFormModalComponent } from '../list-form-modal/list-form-modal.component';

@Component({
  selector: 'app-add-to-list-popover',
  standalone: true,
  imports: [CommonModule, ButtonModule, PopoverModule, ProgressSpinnerModule, Tooltip],
  templateUrl: './add-to-list-popover.component.html',
  styleUrl: './add-to-list-popover.component.scss',
})
export class AddToListPopoverComponent implements OnInit {
  readonly movieId = input.required<string>();
  readonly tmdbId = input<number | null>();

  private readonly listService = inject(UserListService);
  private readonly notificationService = inject(NotificationService);
  private readonly authService = inject(AuthService);
  private readonly dialogService = inject(DialogService);

  readonly lists = signal<UserLists>([]);
  readonly loading = signal(false);
  readonly isAuthenticated = this.authService.isAuthenticated(); 

  private dialogRef: DynamicDialogRef<ListFormModalComponent> | null = null;
  

  ngOnInit(): void {
    if (!this.isAuthenticated) {
      this.lists.set([]);
      return;
    }
    this.loadUserLists();
  }

  toggle(event: Event, popover: Popover): void {
    if (!this.isAuthenticated) {
      this.notificationService.warning('Vous devez être connecté pour ajouter des films à vos listes');
      return;      
    }
    popover.toggle(event);
  }

  addToList(list: UserListSummary, popover: Popover): void {
    this.listService.addMovieToList(list.id, 
      { 
        movieId: this.movieId()
      }).subscribe({
        next: () => {
          this.notificationService.success(`Film ajouté à "${list.title}"`);
          popover.hide();
        },
        error: () => {} 
      });
  }

  createNewList(popover: Popover): void {
    popover.hide();
    
    this.dialogRef = this.dialogService.open(ListFormModalComponent, {
      header: 'Créer une nouvelle liste',
      width: '600px',
      data: {
        movieId: this.movieId(),
        addMovieAfterCreation: true
      }
    });

    this.dialogRef?.onClose.subscribe((created: boolean) => {
      if (created) {
        this.loadUserLists();
      }
    });
  }

  private loadUserLists(): void {
    this.loading.set(true);

    this.listService.getMyLists().subscribe({
      next: (lists) => {
        this.lists.set(lists);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        this.lists.set([]);
        this.notificationService.error('Erreur lors du chargement de vos listes.');
      }
    });
  }
}
