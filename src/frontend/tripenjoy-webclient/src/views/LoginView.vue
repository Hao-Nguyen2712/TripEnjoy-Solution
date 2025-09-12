<template>
  <div class="sign_in_up_bg">
    <div class="container">
      <div class="row justify-content-lg-center justify-content-md-center">
        <div class="col-lg-12">
          <div class="main_logo25" id="logo">
            <router-link to="/"><img :src="logoUrl" alt="" /></router-link>
            <router-link to="/"><img class="logo-inverse" :src="logoInverseUrl" alt="" /></router-link>
          </div>
        </div>

        <div class="col-lg-6 col-md-12">
          <div class="sign_form">
            <h2>Welcome Back</h2>
            <p>Log In to Your TripEnjoy Account!</p>
            <button class="social_lnk_btn color_btn_go mt-3"><i class="uil uil-google"></i>Continue with Google</button>
            <form @submit.prevent="handleLogin" class="mt-4" novalidate>
              <div class="ui left icon input swdh95 mb-3" >
                <input class="prompt srch_explore" type="email" v-model="email" required maxlength="64" placeholder="Email Address" />
                <i class="uil uil-envelope icon icon2"></i>
              </div>
              <div class="ui left icon input swdh95">
                <input class="prompt srch_explore" :type="passwordFieldType" v-model="password" required maxlength="64" placeholder="Password" />
                <i :class="['icon', 'icon2', passwordIconClass]" @click="togglePasswordVisibility" style="cursor: pointer; user-select: none;"></i>
              </div>

              <button class="login-btn" type="submit" :disabled="isLoading">
                {{ isLoading ? 'Signing In...' : 'Sign In' }}
              </button>
            </form>
            <p class="sgntrm145">Or <router-link to="/forgot-password">Forgot Password</router-link>.</p>
            <p class="mb-0 hvsng145">Don't have an account? <router-link to="/signup">Sign Up</router-link></p>
          </div>
          <div class="sign_footer">
            <img :src="signLogoUrl" alt="" />© 2025 <strong>TripEnjoy</strong>. All Rights Reserved.
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue';
import { useToast } from 'vue-toastification';
import { useRouter } from 'vue-router'; // Import useRouter
import authService from '@/services/authService';
import logoUrl from '@/assets/images/logo.svg';
import logoInverseUrl from '@/assets/images/ct_logo.svg';
import signLogoUrl from '@/assets/images/sign_logo.png';

const email = ref('');
const password = ref('');
const isLoading = ref(false);
const isPasswordVisible = ref(false);
const toast = useToast();
const router = useRouter(); // Initialize router

const passwordFieldType = computed(() => {
  return isPasswordVisible.value ? 'text' : 'password';
});

const passwordIconClass = computed(() => {
  // Using unicons classes for eye and eye-slash
  return isPasswordVisible.value ? 'uil uil-eye-slash' : 'uil uil-eye';
});

const togglePasswordVisibility = () => {
  isPasswordVisible.value = !isPasswordVisible.value;
};

const validateEmail = (emailToTest) => {
  const re = /^(([^<>()[\]\\.,;:\s@"]+(\.[^<>()[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
  return re.test(String(emailToTest).toLowerCase());
};

const handleLogin = async () => {
  // 1. Validation
  if (!email.value || !password.value) {
    toast.error("Vui lòng nhập đầy đủ email và mật khẩu.");
    return;
  }
  if (!validateEmail(email.value)) {
    toast.error("Định dạng email không hợp lệ.");
    return;
  }

  isLoading.value = true;
  try {
    // 2. API Call using the service
    const response = await authService.loginStepOne({
      email: email.value,
      password: password.value,
    });

    toast.success("Mã OTP đã được gửi đến email của bạn!");
    
    // Save email and redirect
    sessionStorage.setItem('user_email_for_otp', email.value);
    router.push('/verify-otp');

  } catch (error) {
    // 3. Error Handling
    if (error.response) {
      toast.error(error.response.data.message || "Đã xảy ra lỗi khi đăng nhập.");
    } else if (error.request) {
      toast.error("Không thể kết nối đến máy chủ. Vui lòng thử lại.");
    } else {
      toast.error("Đã có lỗi bất ngờ xảy ra.");
    }
    console.error('API Error:', error);
  } finally {
    isLoading.value = false;
  }
};
</script>

<style>
/* 
  Styles are now handled by AuthLayout.vue via auth.css
*/
</style>
