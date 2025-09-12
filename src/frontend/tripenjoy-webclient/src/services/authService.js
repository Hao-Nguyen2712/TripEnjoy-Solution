import apiClient from './apiClient';
import tokenService from './tokenService';

const authService = {
  loginStepOne(credentials) {
    return apiClient.post('/auth/login-step-one', credentials);
  },
  loginStepTwo(data) {
    return apiClient.post('/auth/login-step-two', data);
  },
  refreshToken() {
    return apiClient.post('/auth/refresh-token', {
      refreshToken: tokenService.getRefreshToken(),
    });
  },
  logout() {
    // Ideally, you might want to call a '/auth/logout' endpoint on the backend
    // to invalidate the refresh token. For now, we'll just clear local tokens.
    tokenService.removeTokens();
  }
};

export default authService;
