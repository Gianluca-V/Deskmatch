import { useQuery } from '@tanstack/react-query';
import { getWorkspaces } from '../api/offices';

export function useWorkspaces(filters = {}) {
  // Remove empty/undefined values so they don't end up as ?city=&minPrice= etc.
  const params = Object.fromEntries(
    Object.entries(filters).filter(([, v]) => v !== '' && v !== null && v !== undefined)
  );

  return useQuery({
    queryKey: ['workspaces', params],
    queryFn: () => getWorkspaces(params),
    staleTime: 30_000,
  });
}
