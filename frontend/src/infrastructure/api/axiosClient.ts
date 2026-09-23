import axios from 'axios';

const axiosClient = axios.create({
  baseURL: process.env.NEXT_PUBLIC_API_URL || 'http://localhost:8888/api',
  headers: {
    'Content-Type': 'application/json',
  },
});

axiosClient.interceptors.request.use(
  (config) => {
    if (typeof window !== 'undefined') {
      const token = localStorage.getItem('token');
      if (token) {
        config.headers.Authorization = `Bearer ${token}`;
      }
      
      const deviceHash = localStorage.getItem('deviceHash');
      if (deviceHash) {
        config.headers['X-Device-Hash'] = deviceHash;
      }
    }
    return config;
  },
  (error) => Promise.reject(error)
);

axiosClient.interceptors.response.use(
  (response) => response.data,
  (error) => {
    // Handle global errors here (e.g., 401 Unauthorized, 403 Banned)
    return Promise.reject(error.response?.data || error.message);
  }
);

export default axiosClient;

