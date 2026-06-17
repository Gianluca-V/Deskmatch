import api from '../lib/api';

export const createReservation = (workspaceId, dto) =>
  api.post(`/api/workspaces/${workspaceId}/reservations`, dto).then((r) => r.data);

export const cancelReservation = (id) =>
  api.post(`/api/reservations/${id}/cancel`).then((r) => r.data);

export const getMyReservations = () =>
  api.get('/api/reservations/me').then((r) => r.data);

export const getCompanyReservations = () =>
  api.get('/api/companies/me/reservations').then((r) => r.data);

export const getCompanyReservationsSummary = () =>
  api.get('/api/companies/me/reservations/summary').then((r) => r.data);

export const getWorkspaceReservations = (workspaceId) =>
  api.get(`/api/workspaces/${workspaceId}/reservations`).then((r) => r.data);
