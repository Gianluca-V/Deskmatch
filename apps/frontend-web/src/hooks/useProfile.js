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
      try {
        const data = await profileAPI.getCompany();
        return {
          ...data,
          phone: data.phoneNumber || data.phone,
        };
      } catch (error) {
        // En desarrollo, devolver datos de prueba si no hay conexión, no hay empresa o token inválido
        if (import.meta.env.DEV && (error.response?.status === 404 || error.response?.status === 401 || error.code === 'ERR_NETWORK' || !error.response)) {
          return {
            id: 1,
            name: 'WorkSpace Solutions',
            contactEmail: 'owner@example.com',
            phoneNumber: '+34 912 345 678',
            phone: '+34 912 345 678',
            location: 'Madrid, España',
            description: 'Proveedor líder de espacios de trabajo flexibles en España. Ofrecemos oficinas modernas, salas de reuniones y espacios de coworking diseñados para maximizar la productividad de tu equipo.',
            websiteUrl: 'https://www.workspacesolutions.es',
            isVerified: true,
            averageRating: 4.8,
            reviewCount: 127,
            spaces: [
              {
                id: 1,
                name: 'Oficina Moderna Centro',
                city: 'Madrid',
                location: 'Madrid',
                type: 'Oficina Privada',
                price: '€250',
                image: 'https://images.unsplash.com/photo-1552664730-d307ca884978?w=400&h=300&fit=crop'
              },
              {
                id: 2,
                name: 'Coworking Barcelona Tech',
                city: 'Barcelona',
                location: 'Barcelona',
                type: 'Coworking',
                price: '€35',
                image: 'https://images.unsplash.com/photo-1559027615-cd4628902d4a?w=400&h=300&fit=crop'
              },
              {
                id: 3,
                name: 'Sala de Reuniones Premium',
                city: 'Madrid',
                location: 'Madrid',
                type: 'Sala de Reuniones',
                price: '€120',
                image: 'https://images.unsplash.com/photo-1554224311-beee415c15cb?w=400&h=300&fit=crop'
              }
            ],
            reservations: [
              {
                id: 1,
                space: {
                  id: 1,
                  name: 'Oficina Moderna Centro',
                  image: 'https://images.unsplash.com/photo-1552664730-d307ca884978?w=100&h=100&fit=crop'
                },
                user: {
                  firstName: 'María',
                  lastName: 'González'
                },
                startDate: '2026-06-04',
                endDate: '2026-06-09',
                totalPrice: '€1250',
                status: 'Confirmada'
              },
              {
                id: 2,
                space: {
                  id: 2,
                  name: 'Coworking Barcelona Tech',
                  image: 'https://images.unsplash.com/photo-1559027615-cd4628902d4a?w=100&h=100&fit=crop'
                },
                user: {
                  firstName: 'María',
                  lastName: 'González'
                },
                startDate: '2026-06-14',
                endDate: '2026-06-19',
                totalPrice: '€175',
                status: 'Pendiente'
              },
              {
                id: 3,
                space: {
                  id: 3,
                  name: 'Sala de Reuniones Premium',
                  image: 'https://images.unsplash.com/photo-1554224311-beee415c15cb?w=100&h=100&fit=crop'
                },
                user: {
                  firstName: 'María',
                  lastName: 'González'
                },
                startDate: '2026-05-19',
                endDate: '2026-05-20',
                totalPrice: '€120',
                status: 'Cancelada'
              }
            ]
          };
        }
        // Si no estamos en desarrollo o si es un error diferente
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
