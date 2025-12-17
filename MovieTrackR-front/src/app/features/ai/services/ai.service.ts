import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from '../../../core/services/api.service';
import { ConfigService } from '../../../core/services/config.service';
import { ChatRequest, ChatResponse } from '../models/chat-request.model';

@Injectable({ providedIn: 'root' })
export class AiService {
  private readonly api = inject(ApiService);
  private readonly config = inject(ConfigService);

  chat(chatRequest: ChatRequest): Observable<ChatResponse> {
    return this.api.post<ChatResponse>(`${this.config.apiUrl}/ai/chat`, chatRequest, {
      withCredentials: true,
    });
  }

  resetSession(): Observable<void> {
    return this.api.delete<void>(`${this.config.apiUrl}/ai/chat/resetSession`, {
      withCredentials: true,
    });
  }
}
