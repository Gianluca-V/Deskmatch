import { useMutation, useQueryClient } from '@tanstack/react-query';
import { bulkPreview, bulkConfirm } from '../api/offices';

export function useBulkPreview({ onSuccess, onError } = {}) {
  return useMutation({
    mutationFn: bulkPreview,
    onSuccess: (data) => onSuccess?.(data),
    onError: (err) => onError?.(err),
  });
}

export function useBulkConfirm({ onSuccess, onError } = {}) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: bulkConfirm,
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: ['offices'] });
      queryClient.invalidateQueries({ queryKey: ['workspaces'] });
      onSuccess?.(data);
    },
    onError: (err) => onError?.(err),
  });
}
