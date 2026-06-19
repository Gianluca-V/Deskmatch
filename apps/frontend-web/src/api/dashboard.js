import api from '../lib/api';

export const getHostDashboard = () =>
  api.get('/api/hosts/me/dashboard').then((response) => response.data);
