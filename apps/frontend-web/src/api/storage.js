import client from './client';

export async function uploadImage(file, container = 'offices') {
  const formData = new FormData();
  formData.append('file', file);
  formData.append('container', container);

  const { data } = await client.post('/api/storage/upload', formData, {
    headers: { 'Content-Type': 'multipart/form-data' },
  });

  return data.url;
}
