import { CommonModule } from '@angular/common';
import { Component, effect, inject, input, signal } from '@angular/core';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { UserProfilesService } from '../../services/user-profiles.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { UserLists, UserListSummary } from '../../../user-lists/models/user-list.model';
import { UserListCardComponent } from '../../../user-lists/components/user-list-card/user-list-card.component';
import { Router } from '@angular/router';

@Component({
  selector: 'app-user-profile-lists-section',
  standalone: true,
  imports: [CommonModule, ProgressSpinnerModule, UserListCardComponent],
  templateUrl: './user-profile-lists-section.component.html',
  styleUrl: './user-profile-lists-section.component.scss',
})
export class UserProfileListsSectionComponent {
  readonly userId = input.required<string | undefined>();

  private readonly userProfileService = inject(UserProfilesService);
  private readonly notificationService = inject(NotificationService);
  private readonly router = inject(Router);

  readonly userLists = signal<UserLists>([]);
  readonly loading = signal(false);
  readonly error = signal<string | null>(null);
  
  constructor() {
    effect(() => {
      const id = this.userId();
      if (!id) return;
      
      this.loadLists(id);
    });
  }    

  onViewList(list: UserListSummary): void {
    this.router.navigate(['/my-lists', list.id]);
  }

  onDeletedList(list: UserListSummary): void {
    this.userLists.update((l) => l.filter(
      (x) => x.id !== list.id
    ));
  }

  onEditedList(updatedList: UserListSummary): void {
    this.userLists.update((lists) =>
      lists.map((l) => l.id === updatedList.id ? {
        ...l,
        title: updatedList.title,
        description: updatedList.description
      } : l)
    );
  }  

  private loadLists(userId: string): void {
    this.loading.set(true);
    this.error.set(null);

    this.userProfileService.getListsByUser(userId).subscribe({
      next: (result: UserLists) => {
        this.userLists.set(result);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        this.userLists.set([]);
        this.error.set('Erreur lors de la récupération des critiques');
        this.notificationService.error("Impossible de charger les listes de l'utilisateur.");
      }
    });
  }
}
