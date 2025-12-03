import { CommonModule } from '@angular/common';
import { Component, computed, effect, inject, signal } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { MovieCardComponent } from '../../../movies/components/movie-card/movie-card.component';
import { ActivatedRoute, Router } from '@angular/router';
import { UserListService } from '../../services/user-list.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { AuthService } from '../../../../core/auth/auth-service';
import { DialogService, DynamicDialogRef } from 'primeng/dynamicdialog';
import { toSignal } from '@angular/core/rxjs-interop';
import { map, of, switchMap } from 'rxjs';
import { UserListDetails } from '../../models/user-list.model';
import { ListFormModalComponent } from '../../components/list-form-modal/list-form-modal.component';
import { ReorderMoviesPopoverComponent } from '../../components/reorder-movies-popover/reorder-movies-popover.component';

@Component({
  selector: 'app-list-details-page',
  standalone: true,
  imports: [CommonModule, ButtonModule, ProgressSpinnerModule, MovieCardComponent, ReorderMoviesPopoverComponent],
  templateUrl: './list-details.page.html',
  styleUrl: './list-details.page.scss',
})
export class ListDetailsPage {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly listService = inject(UserListService);
  private readonly notificationService = inject(NotificationService);
  private readonly authService = inject(AuthService);
  private readonly dialogService = inject(DialogService);

  public readonly loading = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly reloadKey = signal(0);

  protected readonly listDetails = signal<UserListDetails | null>(null);
  private readonly listId = toSignal(
    this.route.paramMap.pipe(map((params) => params.get('id'))),
    { initialValue: null }
  );

  protected readonly hasMovies = computed(() => 
    (this.listDetails()?.movies.length ?? 0) > 0
  );

  public readonly isAuthenticated = this.authService.isAuthenticated;
  private dialogRef: DynamicDialogRef | null = null;

  constructor() {
    effect(() => {
      const id = this.listId();
      const reload = this.reloadKey();

      if (!id) {
          this.error.set('ID de liste manquant');
          return;          
      }

      this.loadList(id);
    });
  }

  public onRemoveMovie(movieId: string): void {
    const list = this.listDetails();
    if (!list) return;

    if (!confirm('Êtes-vous sûr de vouloir retirer ce film de la liste ?')) return;

    this.listService.removeMovieFromList(list.id, movieId).subscribe({
      next: () => {
        this.notificationService.success('Film retiré de la liste');
        this.reloadKey.update((x) => x + 1)
      },
      error: () => this.notificationService.error('Impossible de retirer le film')
    });
  }

  public onEditList(): void {
    const list = this.listDetails();
    if (!list) return;

    this.dialogRef = this.dialogService.open(ListFormModalComponent, {
      header: 'Modifier la liste',
      width: '600px',
      data: { 
        list: {
          id: list.id,
          title: list.title,
          description: list.description,
          createdAt: list.createdAt,
          moviesCount: list.movies.length
        }
      }
    });

    this.dialogRef?.onClose.subscribe((updated : boolean) => {
      if (updated) this.reloadKey.update((x) => x + 1);
    });
  }

  public onDeleteList(): void {
    const list = this.listDetails();
    if (!list) return;
    
    if (!confirm(`Êtes-vous sûr de vouloir supprimer la liste "${list.title}" ?`)) return;
    
    this.listService.deleteList(list.id).subscribe({
      next: () => {
        this.notificationService.success('Liste supprimée');
        this.router.navigate(['/my-lists']);
      },
      error: () => {
        this.notificationService.error('Impossible de supprimer la liste');
      }
    });
  }

  public onViewMovie(movieId : string): void {
    this.router.navigate(['/movies', movieId]);
  }

  public onBack(): void {
    this.router.navigate(['/my-lists']);
  }

  public onReload(): void {
    this.reloadKey.update((x) => x + 1);
  }

  public onReordered(): void {
  this.reloadKey.update(x => x + 1);
}

  private loadList(id: string) {
    this.loading.set(true);
    this.error.set(null);

     this.listService.getListDetails(id).subscribe({
      next: (details) => {
        this.listDetails.set(details);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        this.notificationService.error('Impossible de charger les détails de la liste.')
      }
     });
  }
}
