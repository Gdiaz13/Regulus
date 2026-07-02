import type { ReactNode } from 'react';
import { Navigate, useLocation } from 'react-router-dom';
import ResourceStatus from '../Components/AsyncResource/ResourceStatus';
import { useAuth } from './useAuth';

type Props = {
  children: ReactNode;
};

export default function ProtectedRoute({ children }: Props) {
  const auth = useAuth();
  const location = useLocation();
  if (auth.status === 'loading') {
    return <ResourceStatus status="loading" message={null} />;
  }
  return auth.user ? children : <Navigate to="/login" replace state={{ from: location }} />;
}
