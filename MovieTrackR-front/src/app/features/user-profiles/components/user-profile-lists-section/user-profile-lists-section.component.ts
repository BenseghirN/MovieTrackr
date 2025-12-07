import { CommonModule } from '@angular/common';
import { Component, effect, inject, input, signal } from '@angular/core';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { UserProfilesService } from '../../services/user-profiles.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { UserLists } from '../../../user-lists/models/user-list.model';
import { UserListCardComponent } from '../../../user-lists/components/user-list-card/user-list-card.component';

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
