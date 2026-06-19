import { useMutation, useQueryClient } from '@tanstack/react-query';
import { createOffice } from '../api/offices';

export function useCreateOffice({ onSuccess, onError, companyId } = {}) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: createOffice,
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: ['offices'] });
      if (companyId) {
        queryClient.invalidateQueries({ queryKey: ['workspaces', companyId] });
      }
      onSuccess?.(data);
    },
    onError: (err) => {
      onError?.(err);
    },
  });
}
