import axios from 'axios';

const apiClient = axios.create({
  baseURL: 'https://localhost:7199/api/v1',
  headers: {
    'Content-Type': 'application/json',
  },
});

export default apiClient;
