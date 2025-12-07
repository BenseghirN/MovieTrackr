import { Component, computed, effect, inject, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { UserProfilesService } from '../../services/user-profiles.service';
import { UserProfile } from '../../models/user-profiles.models';
import { CommonModule, Location } from '@angular/common';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { NotificationService } from '../../../../core/services/notification.service';
import { toSignal } from '@angular/core/rxjs-interop';
import { map } from 'rxjs';
import { ButtonModule } from 'primeng/button';
import { UserProfileReviewsSectionComponent } from '../../components/user-profile-reviews-section/user-profile-reviews-section.component';
import { TabsModule } from 'primeng/tabs';
import { UserProfileListsSectionComponent } from '../../components/user-profile-lists-section/user-profile-lists-section.component';

@Component({
  selector: 'app-user-profile-page',
  standalone: true,
  imports: [CommonModule, ProgressSpinnerModule, ButtonModule, TabsModule, UserProfileReviewsSectionComponent, UserProfileListsSectionComponent],
  templateUrl: './user-profile.page.html',
  styleUrl: './user-profile.page.scss',
})
export class UserProfilePage {
  private readonly route = inject(ActivatedRoute);
  private readonly location = inject(Location);
  private readonly profilesService = inject(UserProfilesService);
  private readonly notificationService = inject(NotificationService);
  

  readonly loading = signal(false);
  readonly error = signal<string | null>(null);

  readonly userProfile = signal<UserProfile | null>(null);
  private readonly userId = toSignal(
    this.route.paramMap.pipe(map((params) => params.get('id'))),
    { initialValue: null }
  );
  readonly joinedYear = computed(() => {
    const p = this.userProfile();
    if (!p) return null;
    return new Date(p.createdAt).getFullYear();
  });

  readonly tabs = [
    { value: 'reviews', label: 'Critiques', icon: 'pi pi-star' },
    { value: 'lists', label: 'Listes', icon: 'pi pi-list' },
    // { value: 'comments', label: 'Commentaires', icon: 'pi pi-comments' },
  ];
  activeTab = signal<string>('reviews');

  constructor() {
    effect(() => {
      const id = this.userId();
      if (!id) {
          this.error.set('Identifiant utilisateur manquant.');
          return;          
      }

      this.loadUserProfile(id);
    });
  }

  onBack(): void {
    this.location.back();
  }

  private loadUserProfile(id: string): void {
    this.loading.set(true);
    this.error.set(null);

    this.profilesService.getProfileById(id).subscribe({
      next: (profile: UserProfile) => {
        this.userProfile.set(profile);
        this.loading.set(false);
      },
      error: (err) => {
        this.loading.set(false);
        if (err.status !== 404) {
          this.error.set("Une erreur est survenue lors du chargement du profil.");
          this.notificationService.error('Impossible de charger le profil utilisateur.');          
        }
      }
    });
  }
}
