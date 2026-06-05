import { useQuery } from '@tanstack/react-query';
import api from '../lib/api';

export function useMyCompany() {
  return useQuery({
    queryKey: ['my-company'],
    queryFn: async () => {
      const { data } = await api.get('/api/companies/me');
      return data;
    },
  });
}
