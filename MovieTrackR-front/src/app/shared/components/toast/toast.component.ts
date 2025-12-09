import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { NotificationService } from '../../../core/services/notification.service';
import { NotificationType } from '../../../core/models/notification.model';

@Component({
  selector: 'app-toast',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './toast.component.html',
  styleUrl: './toast.component.scss',
})
export class ToastComponent {
  readonly notificationService = inject(NotificationService);

  getClasses(type: NotificationType): string {
    const base = 'toast-item';
    switch (type) {
      case 'success': return `${base} toast-success`;
      case 'error': return `${base} toast-error`;
      case 'warning': return `${base} toast-warning`;
      case 'info': return `${base} toast-info`;
      default: return base;
    }
  }

  getIcon(type: NotificationType): string {
    switch (type) {
      case 'success': return '✓';
      case 'error': return '✗';
      case 'warning': return '⚠';
      case 'info': return 'ℹ';
      default: return '•';
    }
  }
}
