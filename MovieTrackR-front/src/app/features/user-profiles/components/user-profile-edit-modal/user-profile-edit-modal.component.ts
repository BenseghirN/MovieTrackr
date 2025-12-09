import { CommonModule } from '@angular/common';
import { Component, computed, effect, inject, input, OnInit, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { InputTextModule } from 'primeng/inputtext';
import { SkeletonModule } from 'primeng/skeleton';
import { FileUploadHandlerEvent, FileUploadModule } from 'primeng/fileupload';
import { UserProfilesService } from '../../services/user-profiles.service';
import { UpdatedUserModel, UserProfile } from '../../models/user-profiles.models';
import { DynamicDialogConfig, DynamicDialogRef } from 'primeng/dynamicdialog';
import { NotificationService } from '../../../../core/services/notification.service';
import { AuthService } from '../../../../core/auth/auth-service';
import { toSignal } from '@angular/core/rxjs-interop';
import { ProgressSpinnerModule } from 'primeng/progressspinner';

interface UserFormDialogData {
  userId?: string;
}

@Component({
  selector: 'app-user-profile-edit-modal',
  standalone: true,
  imports: [CommonModule, ProgressSpinnerModule, ReactiveFormsModule, CardModule, ButtonModule, SkeletonModule, InputTextModule, FileUploadModule ],
  templateUrl: './user-profile-edit-modal.component.html',
  styleUrl: './user-profile-edit-modal.component.scss',
})
export class UserProfileModalComponent implements OnInit {
  private readonly dialogRef = inject(DynamicDialogRef);
  private readonly dialogConfig = inject(DynamicDialogConfig<UserFormDialogData>);
  private readonly userProfileService = inject(UserProfilesService);
  private readonly notificationService = inject(NotificationService);
  private readonly authService = inject(AuthService);

  readonly currentUser = this.authService.currentUser;
  readonly isAuthenticated = this.authService.isAuthenticated;

  userId!: string;

  private readonly formBuilder = inject(FormBuilder);
  readonly profileForm = this.formBuilder.nonNullable.group({
    pseudo: this.formBuilder.nonNullable.control<string>('', {
      validators: [Validators.required],
    }),
    givenName: this.formBuilder.nonNullable.control<string>('', {
      validators: [Validators.required],
    }),
    surname: this.formBuilder.nonNullable.control<string>('', {
      validators: [Validators.required],
    }),
  });

  readonly pseudo = toSignal(
    this.profileForm.controls.pseudo.valueChanges,
    { initialValue: this.profileForm.controls.pseudo.value }
  );
  readonly givenName = toSignal(
    this.profileForm.controls.givenName.valueChanges,
    { initialValue: this.profileForm.controls.givenName.value }
  );
  readonly surname = toSignal(
    this.profileForm.controls.surname.valueChanges,
    { initialValue: this.profileForm.controls.surname.value }
  );
  
  readonly user = signal<UserProfile | null>(null);
  readonly loading = signal(false);
  readonly savingInfos = signal(false);
  readonly savingAvatar = signal(false);

  readonly avatarUrl = computed(() => this.user()?.avatarUrl ?? null);
  readonly avatarInitials = computed(() => {
    const u = this.user();
    if (!u) return '?';
    const first = (u.givenName || u.pseudo || '?').trim()[0] ?? '?';
    const last = (u.surname || '').trim()[0] ?? '';
    return (first + last).toUpperCase();
  });

  readonly canSubmitInfos = computed(() =>
    this.profileForm.valid && !this.savingInfos()
  );

  ngOnInit(): void {
    const dialogUserId = this.dialogConfig.data?.userId;
    const current = this.currentUser();

    if (dialogUserId) {
      this.userId = dialogUserId;
    } else if (current?.id) {
      this.userId = current.id;
    } else {
      this.dialogRef.close();
      return;
    }

    this.loadUser();
  }
  
  onClose(): void {
    this.dialogRef.close(this.user());
  }

  onSubmitInfos(): void {
    this.savingInfos.set(true);
    this.loading.set(true);

    const user = this.user();
    if (!user) return;

    if (!this.isAuthenticated()) {
      this.notificationService.warning('Vous devez être connecté pour modifier votre profil.');
      this.authService.login(window.location.href);
      return;
    }

    if (this.profileForm.invalid) {
      this.profileForm.markAllAsTouched();
      return;
    }

    const values = this.profileForm.getRawValue();
    const updatedUser = {
      pseudo: values.pseudo.trim(),
      givenName: values.givenName.trim(),
      surname: values.surname.trim()
    } as UpdatedUserModel;


    this.userProfileService.updateUserInfo(user.id, updatedUser).subscribe({
      next: (updated: UserProfile) => {
        this.user.set(updated);
        this.loading.set(false);
        this.savingInfos.set(false);
        this.notificationService.success('Profil mis à jour avec succès.');
      },
      error: () => {
        this.loading.set(false);
        this.savingInfos.set(false);
        this.notificationService.error('Impossible de mettre à jour le profil.');
      }
    });
  }

  onAvatarUpload(event: FileUploadHandlerEvent): void {
    const usr = this.user();
    if (!usr) return;

    const file = event.files?.[0];
    if (!file) {
      this.notificationService.error("Aucun fichier n'a été sélectionné.");
      return;
    }

    this.savingAvatar.set(true);
    this.loading.set(true);

    this.userProfileService.updateUserAvatar(usr.id, file).subscribe({
      next: (updated) => {
        this.user.set(updated);
        this.savingAvatar.set(false);
        this.loading.set(false);
        this.notificationService.success('Avatar mis à jour avec succès.');
      },
      error: () => {
        this.savingAvatar.set(false);
        this.loading.set(false);
        this.notificationService.error("Impossible de mettre à jour l'avatar.");
      },
    });
  }

  onCancel(): void {
    const u = this.user();
    if (!u) return;

    this.profileForm.reset(
      {
        pseudo: u.pseudo ?? '',
        givenName: u.givenName ?? '',
        surname: u.surname ?? '',
      },
      { emitEvent: false }
    );
    this.dialogRef.close();
  }

  private loadUser(): void {
    this.loading.set(true);

    this.userProfileService.getProfileById(this.userId).subscribe({
      next: (user) => {
        this.user.set(user);
        this.profileForm.patchValue({
          pseudo: user.pseudo ?? '',
          givenName: user.givenName ?? '',
          surname: user.surname ?? ''
        }, { emitEvent: false });
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        this.notificationService.error('Impossible de charger le profil utilisateur.');
        this.dialogRef.close();
      }
    });
  }
}
