import { useMutation, useQueryClient } from '@tanstack/react-query';
import { updateOffice } from '../api/offices';

export function useUpdateOffice({ onSuccess, onError, companyId } = {}) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, ...data }) => updateOffice(id, data),
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: ['profile-company'] });
      if (companyId) {
        queryClient.invalidateQueries({ queryKey: ['workspaces', companyId] });
      }
      onSuccess?.(data);
    },
    onError: (err) => onError?.(err),
  });
}
