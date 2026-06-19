import { useQuery } from '@tanstack/react-query';
import { searchOffices } from '../api/offices';

export function useWorkspaces(filters = {}) {
  const params = Object.fromEntries(
    Object.entries(filters).filter(([, v]) => v !== '' && v !== null && v !== undefined)
  );

  return useQuery({
    queryKey: ['workspaces', params],
    queryFn: () => searchOffices(params),
    staleTime: 30_000,
  });
}
