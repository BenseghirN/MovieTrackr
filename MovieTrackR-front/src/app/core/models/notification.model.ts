export type NotificationType = 'success' | 'error' | 'warning' | 'info';

export interface Notification {
    id: string;
    type: NotificationType;
    message: string;
    duration?: number;  // Durée en ms (5000 par défaut)
}