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
          // En desarrollo, devolver datos de prueba
          if (import.meta.env.DEV) {
            return {
              id: 1,
              name: 'WorkSpace Solutions',
              email: 'owner@example.com',
              phone: '+34 912 345 678',
              location: 'Madrid, España',
              description: 'Proveedor líder de espacios de trabajo flexibles en España. Ofrecemos oficinas modernas, salas de reuniones y espacios de coworking diseñados para maximizar la productividad de tu equipo.',
              website: 'www.workspacesolutions.es',
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
