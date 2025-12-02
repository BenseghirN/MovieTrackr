import { CommonModule } from '@angular/common';
import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { CreateListModel, CreateListResponse, UpdateListModel, UserListSummary } from '../../models/user-list.model';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { TextareaModule } from 'primeng/textarea';
import { FloatLabelModule } from 'primeng/floatlabel';
import { DynamicDialogConfig, DynamicDialogRef } from 'primeng/dynamicdialog';
import { UserListService } from '../../services/user-list.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { toSignal } from '@angular/core/rxjs-interop';
import { Observable } from 'rxjs';

interface ListFormDialogData {
  list?: UserListSummary;
  movieId?: string;
  addMovieAfterCreation?: boolean;
}

@Component({
  selector: 'app-list-form-modal',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, ButtonModule, InputTextModule, TextareaModule, FloatLabelModule],
  templateUrl: './list-form-modal.component.html',
  styleUrl: './list-form-modal.component.scss',
})
export class ListFormModalComponent implements OnInit {
  private readonly dialogRef = inject(DynamicDialogRef);
  private readonly dialogConfig = inject(DynamicDialogConfig<ListFormDialogData>);
  private readonly listService = inject(UserListService);
  private readonly notificationService = inject(NotificationService);

  protected existingList?: UserListSummary;
  protected movieId?: string;
  protected addMovieAfterCreation = false;

  private readonly formBuilder = inject(FormBuilder);
  protected readonly listForm = this.formBuilder.nonNullable.group({
    title: this.formBuilder.nonNullable.control<string>('', {
      validators: [Validators.required, Validators.minLength(3), Validators.maxLength(100)]
    }),
    description: this.formBuilder.nonNullable.control<string>('', {
      validators: [Validators.maxLength(500)]
    })
  });
  protected readonly title = toSignal(
    this.listForm.controls.title.valueChanges,
    { initialValue: this.listForm.controls.title.value }
  );
  
  protected readonly description = toSignal(
    this.listForm.controls.description.valueChanges,
    { initialValue: this.listForm.controls.description.value }
  );

  protected readonly loading = signal(false);
  protected readonly validationErrors = signal<Partial<Record<string, string[]>>>({});

  protected readonly isEditMode = computed(() => !!this.existingList);
  protected readonly titleLength = computed(() => (this.title() ?? '').length);
  protected readonly descriptionLength = computed(() => (this.description() ?? '').length);
  
  protected readonly canSubmit = computed(() => {
    const title = (this.title() ?? '').trim();
    return title.length >= 3 && title.length <= 100 && !this.loading();
  });

  ngOnInit(): void {
    this.existingList = this.dialogConfig.data?.list;
    this.movieId = this.dialogConfig.data?.movieId;
    this.addMovieAfterCreation = this.dialogConfig.data?.addMovieAfterCreation ?? false;

    if (this.existingList) {
      this.listForm.patchValue({
        title: this.existingList.title,
        description: this.existingList.description ?? ''
      });
    }
  }

  protected onSubmit(): void {
    if (!this.canSubmit) {
      this.listForm.markAllAsTouched();
      return;
    }

    this.loading.set(true);
    this.validationErrors.set({});

    const values = this.listForm.getRawValue();
    const payload = {
      title: values.title.trim(),
      description: values.description.trim() || undefined
    };

    if (this.isEditMode()) {
      this.listService.updateList(this.existingList!.id, payload as UpdateListModel).subscribe({
        next: () => {
          this.loading.set(false);
          this.notificationService.success('List modifiée avec succès.');
          this.dialogRef.close(true);
        },
        error: (err) => this.handleError(err)
      });
    } else {
      this.listService.createList(payload as CreateListModel).subscribe({
        next: (result) => {
          this.loading.set(false);
          const message = this.isEditMode()
            ? 'List modifiée avec succès.'
            : 'Liste créée avec succès.';
          this.notificationService.success(message);
          if (this.movieId && this.addMovieAfterCreation && result) this.addMovieToList(result.id);
          this.dialogRef.close(true);
        },
        error: (err) => this.handleError(err)
      });
    }

  }

  protected onCancel(): void {
    this.dialogRef.close(false);
  }

  protected isInvalid(fieldName: 'title' | 'description'): boolean {
    const field = this.listForm.controls[fieldName];
    return field.invalid && field.touched;
  }
  
  private handleError(err: any): void {
    this.loading.set(false);
    if (err.status === 400 && err.errors) {
      this.validationErrors.set(err.errors);
      return;
    }

    if (err.status === 400 && !err.errors) {
      this.notificationService.error(
        err.message || `Une erreur est survenue lors de l'enregistrement de la liste.`
      );
      return;
    }
  }
  
  private addMovieToList(listId: string): void {
    if (!this.movieId){
      this.dialogRef.close(true);
      return;
    }

    this.listService.addMovieToList(listId, { movieId: this.movieId }).subscribe({
      next: () => {
        this.notificationService.success('Film ajouté à la liste avec succès.');
        this.dialogRef.close(true);
      },
      error: () => this.dialogRef.close(true)
    });
  }
}
