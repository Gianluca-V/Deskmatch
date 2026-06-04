import client from './client';

export const createOffice = (data) => client.post('/api/workspaces', data).then((r) => r.data);

export const getOffices = (params) => client.get('/api/offices', { params }).then((r) => r.data);

export const getOffice = (id) => client.get(`/api/offices/${id}`).then((r) => r.data);
