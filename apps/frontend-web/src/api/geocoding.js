import client from './client';

export const geocode = (q) =>
  client.get('/api/geocode', { params: { q } }).then((r) => r.data);
