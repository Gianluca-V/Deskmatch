import api from '../lib/api';

export const createOffice = (data) => api.post('/api/workspaces', data).then((r) => r.data);

export const getWorkspace = (id) => api.get(`/api/workspaces/${id}`).then((r) => r.data);

export const getWorkspacesByCompany = (companyId) =>
  api.get(`/api/workspaces/company/${companyId}`).then((r) => r.data);

export const updateOffice = (id, data) =>
  api.put(`/api/workspaces/${id}`, data).then((r) => r.data);

export const deleteOffice = (id) =>
  api.delete(`/api/workspaces/${id}`);

export const searchOffices = (params) =>
  api.get('/api/search/offices', { params }).then((r) => r.data);

export const aiSearch = (text, page = 1, pageSize = 12) =>
  api.get('/api/search/ai', { params: { text, page, pageSize } }).then((r) => r.data);
