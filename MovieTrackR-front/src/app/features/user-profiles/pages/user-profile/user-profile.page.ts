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
import { AuthService } from '../../../../core/services/auth.service';
import { DialogService, DynamicDialogRef } from 'primeng/dynamicdialog';
import { UserProfileModalComponent } from '../../components/user-profile-edit-modal/user-profile-edit-modal.component';
import { TooltipModule } from 'primeng/tooltip';

@Component({
  selector: 'app-user-profile-page',
  standalone: true,
  imports: [CommonModule, ProgressSpinnerModule, TooltipModule, ButtonModule, TabsModule, UserProfileReviewsSectionComponent, UserProfileListsSectionComponent],
  templateUrl: './user-profile.page.html',
  styleUrl: './user-profile.page.scss',
})
export class UserProfilePage {
  private readonly route = inject(ActivatedRoute);
  private readonly location = inject(Location);
  private readonly profilesService = inject(UserProfilesService);
  private readonly authService = inject(AuthService);
  private readonly notificationService = inject(NotificationService);
  private readonly dialogService = inject(DialogService);
  private DialogRef: DynamicDialogRef<UserProfileModalComponent> | null = null;

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

  readonly canEditProfile = computed(() => {
    const profile = this.userProfile();
    const current = this.authService.currentUser();
    if (!profile?.id || !current?.id) return false;

    const isOwner = current.id === profile.id;
    const isAdmin = current.role === 'Admin';

    return isOwner || isAdmin;
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

  openEditProfileModal(): void {
    const profile = this.userProfile();
    if (!profile) return;

    if (!this.authService.isAuthenticated()) {
      this.notificationService.warning('Vous devez être connecté pour modifier votre profil.');
      this.authService.login(window.location.href);
      return;
    }

    this.DialogRef = this.dialogService.open(UserProfileModalComponent, {
      header: 'Modifier mon profil',
      width: '600px',
      data: {
        userId: profile.id
      },
      closable: true,
      dismissableMask: true
    });

    if (this.DialogRef) {
      this.DialogRef.onClose.subscribe((updated?: UserProfile) => {
        if (updated) {
          this.userProfile.set(updated);
        }
      });
    }
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
