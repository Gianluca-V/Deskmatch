import { useQuery } from '@tanstack/react-query';
import { getWorkspacesByCompany } from '../api/offices';

export function useWorkspacesByCompany(companyId) {
  return useQuery({
    queryKey: ['workspaces', companyId],
    enabled: !!companyId,
    queryFn: () => getWorkspacesByCompany(companyId),
  });
}
