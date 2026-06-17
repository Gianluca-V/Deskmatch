import { useMutation, useQueryClient } from '@tanstack/react-query';
import { createReservation } from '../api/reservations';

export function useCreateReservation({ onSuccess, onError } = {}) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ workspaceId, dto }) => createReservation(workspaceId, dto),
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: ['my-reservations'] });
      onSuccess?.(data);
    },
    onError: (err) => {
      onError?.(err);
    },
  });
}
