import api from '../lib/api';

export const getHostDashboard = () =>
  api.get('/api/v1/hosts/me/dashboard').then((response) => response.data);
