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
    setTimeout(() => this.scrollToBottom(), 100);
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

  private scrollToBottom(): void {
    const element = this.messagesContainer()?.nativeElement;
    if (!element) return;

    element.scrollTop = element.scrollHeight;
  }
}
