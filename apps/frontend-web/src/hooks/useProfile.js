import { useQuery } from '@tanstack/react-query';
import { useAuth } from '../context/AuthContext';
import { profileAPI } from '../api/profile';

export function useProfile() {
  const { user: authUser } = useAuth();

  // Preparar los datos del usuario del contexto
  const userDataFromContext = authUser && (authUser.firstName || authUser.lastName) ? {
    firstName: authUser.firstName,
    lastName: authUser.lastName,
    email: authUser.email,
    phoneNumber: null,
    location: null,
  } : null;

  return useQuery({
    queryKey: ['profile', authUser?.email],
    queryFn: async () => {
      // Si tenemos datos del usuario en el contexto de auth, usarlos
      if (userDataFromContext) {
        return userDataFromContext;
      }
      // Si no, intentar obtener del API (fallará si el API no está disponible)
      try {
        return await profileAPI.getProfile();
      } catch (error) {
        // Si falla, devolver datos del auth user de todas formas
        if (authUser) {
          return {
            firstName: authUser.firstName || 'Usuario',
            lastName: authUser.lastName || '',
            email: authUser.email,
            phoneNumber: null,
            location: null,
          };
        }
        throw error;
      }
    },
    retry: 1,
    retryDelay: 1000,
    staleTime: 5 * 60 * 1000, // 5 minutes
    enabled: !!authUser, // Solo ejecutar si hay user autenticado
  });
}

export function useProfileCompany() {
  return useQuery({
    queryKey: ['profile-company'],
    queryFn: async () => {
      try {
        return await profileAPI.getCompany();
      } catch (error) {
        // Retornar null si no hay empresa asociada
        if (error.response?.status === 404) {
          return null;
        }
        throw error;
      }
    },
    retry: 1,
    retryDelay: 1000,
    staleTime: 5 * 60 * 1000, // 5 minutes
  });
}
