const tokenService = {
  setTokens(tokens) {
    localStorage.setItem('access_token', tokens.token);
    localStorage.setItem('refresh_token', tokens.refreshToken);
  },

  getAccessToken() {
    return localStorage.getItem('access_token');
  },

  getRefreshToken() {
    return localStorage.getItem('refresh_token');
  },

  removeTokens() {
    localStorage.removeItem('access_token');
    localStorage.removeItem('refresh_token');
  },
};

export default tokenService;
