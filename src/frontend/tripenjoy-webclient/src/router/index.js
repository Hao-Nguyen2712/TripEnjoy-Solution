import { createRouter, createWebHistory } from 'vue-router';

const routes = [
  {
    path: '/',
    component: () => import('@/layouts/DefaultLayout.vue'),
    children: [
      {
        path: '',
        name: 'Home',
        component: () => import('@/views/HomeView.vue'),
      },
      // Other routes that use DefaultLayout can be added here
    ],
  },
  {
    path: '/auth',
    component: () => import('@/layouts/AuthLayout.vue'),
    children: [
      {
        path: '/login',
        name: 'Login',
        component: () => import('@/views/LoginView.vue'),
      },
      {
        path: '/signup',
        name: 'SignUp',
        component: () => import('@/views/SignUpView.vue'),
      },
      {
        path: '/forgot-password',
        name: 'ForgotPassword',
        component: () => import('@/views/ForgotPasswordView.vue'),
      },
      {
        path: '/verify-otp',
        name: 'VerifyOtp',
        component: () => import('@/views/OtpVerificationView.vue'),
      },
    ]
  }
];

const router = createRouter({
  history: createWebHistory(),
  routes,
});

export default router;
