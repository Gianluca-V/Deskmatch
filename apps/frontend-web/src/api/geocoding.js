import api from '../lib/api';

export const geocode = (q) =>
  api.get('/api/geocode', { params: { q } }).then((r) => r.data);
