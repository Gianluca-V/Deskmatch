import { useQuery } from '@tanstack/react-query';
import { searchOffices, getWorkspaces } from '../api/offices';

export function useWorkspaces(filters = {}) {
  const params = Object.fromEntries(
    Object.entries(filters).filter(([, v]) => v !== '' && v !== null && v !== undefined)
  );

  return useQuery({
    queryKey: ['workspaces', params],
    queryFn: async () => {
      try {
        return await searchOffices(params);
      } catch (error) {
        console.warn('Search failed, falling back to basic workspaces API', error);
        // Fallback: use basic workspaces endpoint if search fails
        const data = await getWorkspaces({
          page: params.page || 1,
          pageSize: params.pageSize || 20
        });
        // Transform to match search response format
        return {
          items: data.items || [],
          page: data.page || 1,
          pageSize: data.pageSize || 20,
          totalCount: data.totalCount || 0,
          totalPages: data.totalPages || 0
        };
      }
    },
    staleTime: 30_000,
  });
}
