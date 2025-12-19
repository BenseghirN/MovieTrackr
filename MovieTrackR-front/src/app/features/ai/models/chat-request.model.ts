// ------------
//   REQUEST
// ------------

export interface ChatRequest {
  messages: ChatMessage[];
  agentContext?: AgentContext;
}

export type ChatRole = 'user' | 'assistant';
export interface ChatMessage {
  role: ChatRole;
  content: string;
}

export interface AgentContext {
  additionalContext?: string | null;
}

// ------------
//   REPONSE
// ------------

export interface ChatResponse {
  message: string;
  authorRole: AuthorRole;
  additionalContext?: string | null;
  attachments?: ChatAttachment[] | null;
}

export type AuthorRoleLabel = 'assistant' | 'user' | 'system';
export interface AuthorRole {
  label: AuthorRoleLabel;
}

export interface MovieAttachment {
  type: 'movie';
  index: number;
  localId: string | null;
  tmdbId: number | null;
  title: string;
  year?: number;
  posterPath: string | null;
}

export interface PersonAttachment {
  type: 'person';
  index: number;
  localId: string | null;
  tmdbId: number | null;
  name: string;
  profilePath: string | null;
}

export type ChatAttachment = MovieAttachment | PersonAttachment;
