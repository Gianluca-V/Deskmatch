import api from '../lib/api';

export async function uploadImage(file, container = 'offices') {
  const formData = new FormData();
  formData.append('file', file);
  formData.append('container', container);

  const { data } = await api.post('/api/storage/upload', formData, {
    headers: { 'Content-Type': 'multipart/form-data' },
  });

  return data.url;
}

export async function deleteImage(container, fileName) {
  await api.delete(`/api/storage/${container}/${fileName}`);
}
