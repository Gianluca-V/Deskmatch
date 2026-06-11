import { useQuery } from '@tanstack/react-query';
import { useAuth } from '../context/AuthContext';
import { profileAPI } from '../api/profile';

export function useProfile() {
  const { user: authUser } = useAuth();

  return useQuery({
    queryKey: ['profile', authUser?.email],
    queryFn: async () => {
      try {
        const apiData = await profileAPI.getProfile();
        // Derivar firstName/lastName desde fullName para que el nombre
        // se actualice en pantalla inmediatamente después de editar y hacer refetch.
        const fullName = apiData.fullName || '';
        const spaceIdx = fullName.indexOf(' ');
        const firstName = spaceIdx > 0 ? fullName.substring(0, spaceIdx) : (fullName || authUser?.firstName || 'Usuario');
        const lastName = spaceIdx > 0 ? fullName.substring(spaceIdx + 1) : (authUser?.lastName || '');
        return { ...apiData, firstName, lastName };
      } catch (error) {
        // Fallback al contexto si el API falla
        if (authUser) {
          return {
            firstName: authUser.firstName || 'Usuario',
            lastName: authUser.lastName || '',
            email: authUser.email,
            phoneNumber: null,
            location: null,
            profilePictureUrl: null,
          };
        }
        throw error;
      }
    },
    retry: 1,
    retryDelay: 1000,
    staleTime: 5 * 60 * 1000,
    enabled: !!authUser,
  });
}

export function useProfileCompany() {
  return useQuery({
    queryKey: ['profile-company'],
    queryFn: async () => {
      const data = await profileAPI.getCompany();
      return {
        ...data,
        phone: data.phoneNumber || data.phone,
      };
    },
    retry: 1,
    retryDelay: 1000,
    staleTime: 5 * 60 * 1000,
  });
}
