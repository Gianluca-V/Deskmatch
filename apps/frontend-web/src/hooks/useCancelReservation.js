import { useMutation, useQueryClient } from '@tanstack/react-query';
import { cancelReservation } from '../api/reservations';

export function useCancelReservation({ onSuccess, onError } = {}) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id) => cancelReservation(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['my-reservations'] });
      onSuccess?.();
    },
    onError: (err) => {
      onError?.(err);
    },
  });
}
