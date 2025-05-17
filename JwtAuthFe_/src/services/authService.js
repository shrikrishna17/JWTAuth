import axios from './axiosInstance';

export const register = async (userName, password) => {
  const response = await axios.post('/register', { userName, password });
  return response.data;
};

export const login = async (userName, password) => {
  const response = await axios.post('/login', { userName, password });
  const { accessToken, refreshToken } = response.data;
  localStorage.setItem('accessToken', accessToken);
  localStorage.setItem('refreshToken', refreshToken);
  return response.data;
};

export const getProtectedData = async () => {
  const response = await axios.get('/');
  return response.data;
};

export const getAdminOnly = async () => {
  const response = await axios.get('/admin-only');
  return response.data;
};
