import api from '../lib/api';

export const createOffice = (data) => api.post('/api/workspaces', data).then((r) => r.data);

// GET /api/workspaces?page=1&pageSize=20&city=...&minPrice=...&maxPrice=...&minCapacity=...&amenities=WiFi,AC
export const getWorkspaces = (params) => api.get('/api/workspaces', { params }).then((r) => r.data);

export const getWorkspace = (id) => api.get(`/api/workspaces/${id}`).then((r) => r.data);

export const getWorkspacesByCompany = (companyId) =>
  api.get(`/api/workspaces/company/${companyId}`).then((r) => r.data);

export const updateOffice = (id, data) =>
  api.put(`/api/workspaces/${id}`, data).then((r) => r.data);

export const deleteOffice = (id) =>
  api.delete(`/api/workspaces/${id}`);
