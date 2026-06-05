import { useMutation, useQueryClient } from '@tanstack/react-query';
import { deleteOffice } from '../api/offices';

export function useDeleteOffice({ onSuccess } = {}) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id) => deleteOffice(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['workspaces'] });
      onSuccess?.();
    },
  });
}
