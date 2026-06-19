import { useQuery } from '@tanstack/react-query';
import {
  getCompanyReservations,
  getCompanyReservationsSummary,
  getWorkspaceReservations,
} from '../api/reservations';

export function useCompanyReservations() {
  return useQuery({
    queryKey: ['company-reservations'],
    queryFn: getCompanyReservations,
  });
}

export function useCompanyReservationsSummary() {
  return useQuery({
    queryKey: ['company-reservations-summary'],
    queryFn: getCompanyReservationsSummary,
  });
}

export function useWorkspaceReservations(workspaceId) {
  return useQuery({
    queryKey: ['workspace-reservations', workspaceId],
    queryFn: () => getWorkspaceReservations(workspaceId),
    enabled: !!workspaceId,
  });
}
