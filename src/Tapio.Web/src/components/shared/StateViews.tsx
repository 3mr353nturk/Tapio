export function LoadingState({ message = 'Yükleniyor...' }: { message?: string }) {
  return <div className="loading-state">{message}</div>;
}

export function EmptyState({ message }: { message: string }) {
  return <div className="empty-state">{message}</div>;
}

export function ErrorState({ message }: { message: string }) {
  return <div className="error-state">{message}</div>;
}
