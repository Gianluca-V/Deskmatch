import api from '../lib/api';

export const createOffice = (data) => api.post('/api/workspaces', data).then((r) => r.data);

export const getWorkspace = (id) => api.get(`/api/workspaces/${id}`).then((r) => r.data);

export const getWorkspacesByCompany = (companyId) =>
  api.get(`/api/workspaces/company/${companyId}`).then((r) => r.data);

export const updateOffice = (id, data) =>
  api.put(`/api/workspaces/${id}`, data).then((r) => r.data);

export const deleteOffice = (id) =>
  api.delete(`/api/workspaces/${id}`);

export const getWorkspaces = (params) =>
  api.get('/api/workspaces', { params }).then((r) => r.data);

export const searchOffices = (params) =>
  api.get('/api/search/offices', { params }).then((r) => r.data);

export const aiSearch = (text, page = 1, pageSize = 12) =>
  api.get('/api/search/ai', { params: { text, page, pageSize } }).then((r) => r.data);

export const downloadBulkTemplate = () =>
  api.get('/api/workspaces/bulk/template', { responseType: 'blob' }).then((r) => {
    const url = window.URL.createObjectURL(new Blob([r.data]));
    const link = document.createElement('a');
    link.href = url;
    link.setAttribute('download', 'plantilla_espacios.xlsx');
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    window.URL.revokeObjectURL(url);
  });

export const bulkPreview = (formData) =>
  api.post('/api/workspaces/bulk/preview', formData, {
    headers: { 'Content-Type': 'multipart/form-data' },
  }).then((r) => r.data);

export const bulkConfirm = (formData) =>
  api.post('/api/workspaces/bulk/confirm', formData, {
    headers: { 'Content-Type': 'multipart/form-data' },
  }).then((r) => r.data);
