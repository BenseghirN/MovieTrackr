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

  sendMessage(text: string, options?: { hidden?: boolean; displayAs?: string }): void {
    const technicalContent = (text ?? '').trim();
    if (!technicalContent || this.loading()) return;

    const displayContent =
      options?.hidden && options?.displayAs ? options.displayAs : technicalContent;

    const request: ChatRequest = {
      messages: [
        ...this.chatMessages().map((m) => ({ role: m.role, content: m.content })),
        { role: 'user', content: technicalContent },
      ],
      agentContext: { additionalContext: this.additionalContext() },
    };
    this.chatMessages.update((m) => [...m, { role: 'user', content: displayContent }]);

    this.loading.set(true);
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
