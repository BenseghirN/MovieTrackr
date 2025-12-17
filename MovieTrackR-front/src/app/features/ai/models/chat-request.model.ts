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
  attachments?: any[] | null;
}

export type AuthorRoleLabel = 'assistant' | 'user' | 'system';
export interface AuthorRole {
  label: AuthorRoleLabel;
}
