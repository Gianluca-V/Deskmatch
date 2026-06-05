import api from '../lib/api';

export const createOffice = (data) => api.post('/api/workspaces', data).then((r) => r.data);

export const getOffices = (params) => api.get('/api/offices', { params }).then((r) => r.data);

export const getOffice = (id) => api.get(`/api/offices/${id}`).then((r) => r.data);

export const getWorkspacesByCompany = (companyId) =>
  api.get(`/api/workspaces/company/${companyId}`).then((r) => r.data);

export const updateOffice = (id, data) =>
  api.put(`/api/workspaces/${id}`, data).then((r) => r.data);

export const deleteOffice = (id) =>
  api.delete(`/api/workspaces/${id}`);
