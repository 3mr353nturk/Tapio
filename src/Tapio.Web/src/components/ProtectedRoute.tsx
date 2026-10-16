import { Navigate } from 'react-router-dom';
import { useAuth } from '../hooks/useAuth';
import { LoadingState } from './shared/StateViews';

export function ProtectedRoute({ children, role }: { children: React.ReactNode; role?: 'Admin' | 'BusinessUser' }) {
  const { user, loading } = useAuth();

  if (loading) return <LoadingState />;
  if (!user) return <Navigate to="/login" replace />;
  if (role && user.role !== role) {
    return <Navigate to={user.role === 'Admin' ? '/admin' : '/app'} replace />;
  }
  return <>{children}</>;
}
