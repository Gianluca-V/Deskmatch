import { useQuery } from '@tanstack/react-query';
import { STORAGE_KEY } from '../lib/api';
import api from '../lib/api';

function getUserIdFromSession() {
  try {
    const raw = localStorage.getItem(STORAGE_KEY);
    if (!raw) return null;
    const session = JSON.parse(raw);
    const token = session?.accessToken;
    if (!token) return null;
    const payload = JSON.parse(atob(token.split('.')[1]));
    return payload?.sub ?? null;
  } catch {
    return null;
  }
}

export function useMyCompany() {
  const userId = getUserIdFromSession();

  return useQuery({
    queryKey: ['my-company', userId],
    enabled: !!userId,
    queryFn: async () => {
      const { data } = await api.get('/api/companies');
      return data.find((c) => c.ownerId === userId) ?? null;
    },
  });
}
