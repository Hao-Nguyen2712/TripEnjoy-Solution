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
            <h2>Create an Account</h2>
            <p>Sign Up to Your TripEnjoy Account!</p>
            <button class="social_lnk_btn color_btn_go"><i class="uil uil-google"></i>Sign up with Google</button>
            <form @submit.prevent="handleSignUp" class="mt-4" novalidate>
              <div class="ui left icon input swdh95">
                <input class="prompt srch_explore" type="text" v-model="fullName" required placeholder="Full Name" />
                <i class="uil uil-user icon icon2"></i>
              </div>
              <div class="ui left icon input swdh95">
                <input class="prompt srch_explore" type="email" v-model="email" required placeholder="Email Address" />
                <i class="uil uil-envelope icon icon2"></i>
              </div>
              <div class="ui left icon input swdh95">
                <input class="prompt srch_explore" type="password" v-model="password" required placeholder="Password" />
                <i class="uil uil-key-skeleton-alt icon icon2"></i>
              </div>
              <div class="ui left icon input swdh95">
                <input class="prompt srch_explore" type="password" v-model="confirmPassword" required placeholder="Confirm Password" />
                <i class="uil uil-key-skeleton-alt icon icon2"></i>
              </div>

              <button class="login-btn" type="submit" :disabled="isLoading">
                {{ isLoading ? 'Signing Up...' : 'Sign Up' }}
              </button>
            </form>
            <p class="mb-0 mt-30 hvsng145">Already have an account? <router-link to="/login">Log In</router-link></p>
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
// import authService from '@/services/authService'; // Sẽ dùng sau
import logoUrl from '@/assets/images/logo.svg';
import logoInverseUrl from '@/assets/images/ct_logo.svg';
import signLogoUrl from '@/assets/images/sign_logo.png';

const fullName = ref('');
const email = ref('');
const password = ref('');
const confirmPassword = ref('');
const isLoading = ref(false);
const toast = useToast();

const validateEmail = (emailToTest) => {
  const re = /^(([^<>()[\]\\.,;:\s@"]+(\.[^<>()[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
  return re.test(String(emailToTest).toLowerCase());
};

const handleSignUp = async () => {
  // Validation
  if (!fullName.value || !email.value || !password.value || !confirmPassword.value) {
    toast.error("Vui lòng điền đầy đủ thông tin.");
    return;
  }
  if (!validateEmail(email.value)) {
    toast.error("Định dạng email không hợp lệ.");
    return;
  }
  if (password.value !== confirmPassword.value) {
    toast.error("Mật khẩu xác nhận không khớp.");
    return;
  }

  toast.success("Validation thành công! Sẵn sàng để gọi API đăng ký.");
  console.log({
    fullName: fullName.value,
    email: email.value,
    password: password.value,
  });
  // Logic gọi API đăng ký sẽ được thêm ở đây
};
</script>

<style>
/* 
  Styles are now handled by AuthLayout.vue via auth.css
*/
</style>
