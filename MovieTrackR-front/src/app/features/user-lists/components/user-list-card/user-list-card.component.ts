import { CommonModule } from '@angular/common';
import { Component, inject, input, output } from '@angular/core';
import { CardModule } from 'primeng/card';
import { UserListService } from '../../services/user-list.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { ButtonModule } from 'primeng/button';
import { UserListSummary } from '../../models/user-list.model';
import { DialogService, DynamicDialogRef } from 'primeng/dynamicdialog';
import { ListFormModalComponent } from '../list-form-modal/list-form-modal.component';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ConfirmationService } from 'primeng/api';

@Component({
  selector: 'app-user-list-card',
  standalone: true,
  imports: [CommonModule, CardModule, ButtonModule, ConfirmDialogModule],
  templateUrl: './user-list-card.component.html',
  styleUrl: './user-list-card.component.scss',
})
export class UserListCardComponent {
  readonly list = input.required<UserListSummary>();
  readonly deletedList = output<UserListSummary>();
  readonly editedList = output<UserListSummary>();

  private readonly listService = inject(UserListService);
  private readonly notificationService = inject(NotificationService);
  private readonly confirmationService = inject(ConfirmationService);
  private readonly dialogService = inject(DialogService);

  private dialogRef: DynamicDialogRef | null = null;

  onEditList(list: UserListSummary, event: Event): void {
    event.stopPropagation();

    this.dialogRef = this.dialogService.open(ListFormModalComponent, {
      header: 'Modifier la liste',
      width: '600px',
      data: { list }
    });

    this.dialogRef?.onClose.subscribe((edited: boolean) => {
      if (edited) {
        this.editedList.emit(this.list());
      }
    });
  }

  onDeleteList(list: UserListSummary, event: Event): void {
    event.stopPropagation();
    this.confirmationService.confirm({
      header: 'Confirmation',
      message: `Êtes-vous sûr de vouloir supprimer la liste "${list.title}" ?`,
      acceptLabel: 'Supprimer',
      rejectLabel: 'Annuler',
      closeOnEscape: true,
      accept: () => {
        this.listService.deleteList(list.id).subscribe({
          next: () => {
            this.notificationService.success(`La liste "${list.title}" a été supprimée.`)
            this.deletedList.emit(this.list());
          },
          error: () => this.notificationService.error('Impossible de supprimer la liste')
        });
      }
    });
  }
}
