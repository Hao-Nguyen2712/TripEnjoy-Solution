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
            <button class="social_lnk_btn color_btn_go"><i class="uil uil-google"></i>Continue with Google</button>
            <form @submit.prevent="handleLogin" class="mt-4" novalidate>
              <div class="ui left icon input swdh95" >
                <input class="prompt srch_explore" type="email" v-model="email" required maxlength="64" placeholder="Email Address" />
                <i class="uil uil-envelope icon icon2"></i>
              </div>
              <div class="ui left icon input swdh95">
                <input class="prompt srch_explore" type="password" v-model="password" required maxlength="64" placeholder="Password" />
                <i class="uil uil-key-skeleton-alt icon icon2"></i>
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
import { ref } from 'vue';
import { useToast } from 'vue-toastification';
import authService from '@/services/authService';
import logoUrl from '@/assets/images/logo.svg';
import logoInverseUrl from '@/assets/images/ct_logo.svg';
import signLogoUrl from '@/assets/images/sign_logo.png';

const email = ref('');
const password = ref('');
const isLoading = ref(false);
const toast = useToast();

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

    toast.success("Đăng nhập thành công!");
    console.log('API Response:', response.data);
    // Bạn có thể xử lý chuyển trang hoặc lưu token ở đây

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
.sign_form .input.swdh95 {
  margin-bottom: 20px;
}

.sign_form .login-btn {
  margin-top: 10px;
}

.sign_in_up_bg {
  background-color: #f7f7f7;
  display: flex;
  align-items: center;
  justify-content: center;
  min-height: 100vh;
}
</style>
