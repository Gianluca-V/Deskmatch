import { useQuery } from '@tanstack/react-query';
import { getMyReservations } from '../api/reservations';

export function useMyReservations() {
  return useQuery({
    queryKey: ['my-reservations'],
    queryFn: getMyReservations,
  });
}
