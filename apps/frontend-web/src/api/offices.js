import api from '../lib/api';

export const createOffice = (data) => api.post('/api/workspaces', data).then((r) => r.data);

export const getOffices = (params) => api.get('/api/offices', { params }).then((r) => r.data);

export const getOffice = (id) => api.get(`/api/offices/${id}`).then((r) => r.data);

export const getWorkspacesByCompany = (companyId) =>
  api.get(`/api/workspaces/company/${companyId}`).then((r) => r.data);
