import { CommonModule } from '@angular/common';
import { Component, computed, effect, ElementRef, inject, signal, viewChild } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { BadgeModule } from 'primeng/badge';
import { ButtonModule } from 'primeng/button';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { TextareaModule } from 'primeng/textarea';
import { AiChatStore } from '../../store/ai-chat.store';
import { RouterModule } from '@angular/router';
import { TooltipModule } from 'primeng/tooltip';
import { TmdbImageService } from '../../../../core/services/tmdb-image.service';
import { AuthService } from '../../../../core/services/auth.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { ChatAttachment, MovieAttachment, PersonAttachment } from '../../models/chat-request.model';

@Component({
  selector: 'app-ai-chat-widget',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterModule,
    ButtonModule,
    TextareaModule,
    ProgressSpinnerModule,
    BadgeModule,
    TooltipModule,
  ],
  templateUrl: './ai-chat-widget.component.html',
  styleUrl: './ai-chat-widget.component.scss',
})
export class AiChatWidgetComponent {
  private readonly authService = inject(AuthService);
  readonly isAuthenticated = this.authService.isAuthenticated;
  private readonly notificationService = inject(NotificationService);
  readonly store = inject(AiChatStore);
  readonly imageService = inject(TmdbImageService);

  readonly draft = signal('');
  readonly canSend = computed(() => this.draft().trim().length > 0 && !this.store.loading());

  readonly messagesContainer = viewChild<ElementRef>('messagesContainer');

  constructor() {
    effect(() => {
      this.store.chatMessages();
      queueMicrotask(() => this.scrollToBottom());
    });
  }

  onToggle(): void {
    if (!this.authService.isAuthenticated()) {
      this.notificationService.info("Connectez-vous pour discuter avec l'assistant IA");
      return;
    }
    this.store.toggle();
    if (this.store.isOpen()) {
      queueMicrotask(() => this.scrollToBottom());
    }
  }

  onClose(): void {
    this.store.close();
  }

  onSend(): void {
    if (!this.canSend()) return;

    const message = this.draft();
    this.draft.set('');
    this.store.sendMessage(message);

    queueMicrotask(() => this.scrollToBottom());
    setTimeout(() => this.scrollToBottom(), 50);
  }

  onKeyDown(event: KeyboardEvent): void {
    if (event.key === 'Enter' && !event.shiftKey) {
      event.preventDefault();
      this.onSend();
    }
  }

  reset(): void {
    this.store.resetSession();
    queueMicrotask(() => this.scrollToBottom());
  }

  getMovieLink(attachment: any): string[] {
    if (attachment.localId) {
      return ['/movies', attachment.localId];
    }
    if (attachment.tmdbId) {
      return ['/movies', attachment.tmdbId.toString()];
    }
    return [];
  }

  onSelectPerson(person: PersonAttachment): void {
    const technicalMessage = `Oui celui-ci: index=${person.index}, localId=${person.localId}, tmdbId=${person.tmdbId}`;
    const displayMessage = `Le #${person.index}: ${person.name}`;
    this.store.sendMessage(technicalMessage, { hidden: true, displayAs: displayMessage });

    queueMicrotask(() => this.scrollToBottom());
    setTimeout(() => this.scrollToBottom(), 50);
  }

  isPersonAttachment(attachment: ChatAttachment): attachment is PersonAttachment {
    return 'name' in attachment && !('title' in attachment);
  }

  isMovieAttachment(attachment: ChatAttachment): attachment is MovieAttachment {
    return 'title' in attachment;
  }

  private scrollToBottom(): void {
    const element = this.messagesContainer()?.nativeElement;
    if (!element) return;

    element.scrollTop = element.scrollHeight;
  }
}
