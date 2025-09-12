import axios from 'axios';
import tokenService from './tokenService';
import authService from './authService';

const apiClient = axios.create({
  baseURL: 'https://localhost:7199/api/v1',
  headers: {
    'Content-Type': 'application/json',
  },
});

// Request interceptor to add the auth token to every request
apiClient.interceptors.request.use(
  (config) => {
    const token = tokenService.getAccessToken();
    if (token) {
      config.headers['Authorization'] = 'Bearer ' + token;
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// Response interceptor to handle token refresh
let isRefreshing = false;
let failedQueue = [];

const processQueue = (error, token = null) => {
  failedQueue.forEach(prom => {
    if (error) {
      prom.reject(error);
    } else {
      prom.resolve(token);
    }
  });
  failedQueue = [];
};

apiClient.interceptors.response.use(
  (response) => {
    return response;
  },
  async (error) => {
    const originalRequest = error.config;

    // Check if the error is 401 and it's not a retry request
    if (error.response?.status === 401 && !originalRequest._retry) {
      if (isRefreshing) {
        // If token is already being refreshed, queue the request
        return new Promise((resolve, reject) => {
          failedQueue.push({ resolve, reject });
        })
        .then(token => {
          originalRequest.headers['Authorization'] = 'Bearer ' + token;
          return apiClient(originalRequest);
        })
        .catch(err => {
          return Promise.reject(err);
        });
      }

      originalRequest._retry = true;
      isRefreshing = true;

      try {
        const { data } = await authService.refreshToken();
        const newTokens = data.data; // Assuming the new tokens are in data.data
        tokenService.setTokens(newTokens);
        apiClient.defaults.headers.common['Authorization'] = 'Bearer ' + newTokens.token;
        originalRequest.headers['Authorization'] = 'Bearer ' + newTokens.token;
        
        processQueue(null, newTokens.token);
        return apiClient(originalRequest);

      } catch (refreshError) {
        processQueue(refreshError, null);
        authService.logout();
        // Redirect to login page
        window.location = '/login';
        return Promise.reject(refreshError);
      } finally {
        isRefreshing = false;
      }
    }

    return Promise.reject(error);
  }
);


export default apiClient;
