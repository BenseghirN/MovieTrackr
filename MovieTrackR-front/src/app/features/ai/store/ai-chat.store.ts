import { computed, inject, Injectable, signal } from '@angular/core';
import { AiService } from '../services/ai.service';
import { ChatMessage, ChatRequest } from '../models/chat-request.model';

export type ChatRole = 'user' | 'assistant' | 'system';

@Injectable({ providedIn: 'root' })
export class AiChatStore {
  private readonly aiService = inject(AiService);

  readonly isOpen = signal(false);
  readonly loading = signal(false);

  readonly chatMessages = signal<ChatMessage[]>([]);

  readonly additionalContext = signal<string | null>(null);
  readonly attachments = signal<any[] | null>(null);
  readonly hasMessages = computed(() => this.chatMessages().length > 0);

  open(): void {
    this.isOpen.set(true);
  }

  close(): void {
    this.isOpen.set(false);
  }

  toggle(): void {
    this.isOpen.update((v) => !v);
  }

  resetSession(): void {
    this.chatMessages.set([]);
    this.additionalContext.set(null);
    this.attachments.set(null);

    this.aiService.resetSession().subscribe({
      error: () => {},
    });
  }

  sendMessage(text: string): void {
    console.log('TEXT: ', text);
    const content = (text ?? '').trim();
    if (!content || this.loading()) return;

    console.log('CONTENT: ', content);

    this.chatMessages.update((m) => [
      ...m,
      {
        role: 'user',
        content: content,
      },
    ]);

    console.log('chatMessages: ', this.chatMessages());

    this.loading.set(true);

    const request: ChatRequest = {
      messages: this.chatMessages().map((m) => ({ role: m.role, content: m.content })),
      agentContext: { additionalContext: this.additionalContext() },
    };

    console.log('request: ', request);

    this.aiService.chat(request).subscribe({
      next: (result) => {
        this.chatMessages.update((m) => [...m, { role: 'assistant', content: result.message }]);
        this.additionalContext.set(result.additionalContext ?? null);
        this.attachments.set(result.attachments ?? null);
        this.loading.set(false);
      },
      error: () => {
        this.chatMessages.update((m) => [
          ...m,
          { role: 'assistant', content: `Désolé, j'ai rencontré un souci technique. Réessaie.` },
        ]);
        this.loading.set(false);
      },
    });
  }
}
