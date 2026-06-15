import api from '../lib/api';

export const createReservation = (workspaceId, dto) =>
  api.post(`/api/workspaces/${workspaceId}/reservations`, dto).then((r) => r.data);

export const cancelReservation = (id) =>
  api.post(`/api/reservations/${id}/cancel`).then((r) => r.data);

export const getMyReservations = () =>
  api.get('/api/reservations/me').then((r) => r.data);
