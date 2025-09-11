import apiClient from './apiClient';

const authService = {
  loginStepOne(credentials) {
    return apiClient.post('/auth/login-step-one', credentials);
  },
};

export default authService;
