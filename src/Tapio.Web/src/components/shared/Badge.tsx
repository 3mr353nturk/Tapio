type BadgeVariant = 'success' | 'warning' | 'muted' | 'danger';

const variantClass: Record<BadgeVariant, string> = {
  success: 'badge badge-success',
  warning: 'badge badge-warning',
  muted: 'badge badge-muted',
  danger: 'badge badge-danger'
};

export function Badge({ children, variant = 'muted' }: { children: React.ReactNode; variant?: BadgeVariant }) {
  return <span className={variantClass[variant]}>{children}</span>;
}
