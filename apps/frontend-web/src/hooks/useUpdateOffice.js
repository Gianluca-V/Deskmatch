import { useMutation, useQueryClient } from '@tanstack/react-query';
import { updateOffice } from '../api/offices';

export function useUpdateOffice({ onSuccess, onError } = {}) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, ...data }) => updateOffice(id, data),
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: ['profile-company'] });
      queryClient.refetchQueries({ queryKey: ['profile-company'] });
      onSuccess?.(data);
    },
    onError: (err) => onError?.(err),
  });
}
