import { useQuery } from '@tanstack/react-query';
import { getHostDashboard } from '../api/dashboard';

export function useHostDashboard() {
  return useQuery({
    queryKey: ['host-dashboard'],
    queryFn: getHostDashboard,
    retry: false,
    refetchOnWindowFocus: false,
    refetchOnReconnect: false,
  });
}
