import api from '../lib/api';

// Profile endpoints
export const profileAPI = {
  // Get current user profile
  getProfile: () => api.get('/api/auth/me').then((r) => r.data),

  // Get company information
  getCompany: () => api.get('/api/companies/me/profile').then((r) => r.data),

  // Update user profile
  updateProfile: (data) =>
    api.put('/api/auth/me', data).then((r) => r.data),

  // Update company information
  updateCompany: (data) =>
    api.put('/api/companies/me/profile', data).then((r) => r.data),
};

export default profileAPI;
