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
            <h2>OTP Verification</h2>
            <p>Enter the code sent to your email.</p>
            <form @submit.prevent="handleOtpVerification" class="mt-4" novalidate>
              <div class="otp-inputs">
                <input
                  v-for="(digit, index) in otpDigits"
                  :key="index"
                  v-model="otpDigits[index]"
                  :ref="el => { if (el) otpInputs[index] = el }"
                  @input="handleInput($event, index)"
                  @keydown="handleKeyDown($event, index)"
                  type="text"
                  maxlength="1"
                  class="otp-input"
                  required
                />
              </div>

              <button class="login-btn" type="submit" :disabled="isLoading">
                {{ isLoading ? 'Verifying...' : 'Verify' }}
              </button>
            </form>
            <p class="mb-0 mt-30 hvsng145">Didn't receive a code? <a href="#">Resend</a></p>
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
import { ref, onMounted, computed } from 'vue'; // Import computed
import { useToast } from 'vue-toastification';
import { useRouter } from 'vue-router';
import authService from '@/services/authService'; // Import authService
import tokenService from '@/services/tokenService'; // Import tokenService
import logoUrl from '@/assets/images/logo.svg';
import logoInverseUrl from '@/assets/images/ct_logo.svg';
import signLogoUrl from '@/assets/images/sign_logo.png';

const otpDigits = ref(['', '', '', '', '', '']);
const otpInputs = ref([]);
const otp = computed(() => otpDigits.value.join(''));

const isLoading = ref(false);
const toast = useToast();
const router = useRouter();
let userEmail = '';

onMounted(() => {
  userEmail = sessionStorage.getItem('user_email_for_otp');
  if (!userEmail) {
    toast.error("Không tìm thấy thông tin xác thực. Vui lòng thử đăng nhập lại.");
    router.push('/login');
  }
});

const handleInput = (event, index) => {
  const input = event.target;
  let value = input.value;

  // Only allow digits
  if (!/^\d*$/.test(value)) {
    toast.error("Vui lòng chỉ nhập số.");
    otpDigits.value[index] = ''; // Clear the invalid input
    return;
  }
  
  // Move to next input if a digit is entered
  if (value && index < otpDigits.value.length - 1) {
    otpInputs.value[index + 1].focus();
  }
};

const handleKeyDown = (event, index) => {
  // Move to previous input on backspace if current input is empty
  if (event.key === 'Backspace' && !otpDigits.value[index] && index > 0) {
    otpInputs.value[index - 1].focus();
  }
};


const handleOtpVerification = async () => {
  if (otp.value.length !== 6) {
    toast.error("Vui lòng nhập mã OTP gồm 6 chữ số.");
    return;
  }
  
  isLoading.value = true;
  try {
    const response = await authService.loginStepTwo({
      email: userEmail,
      otp: otp.value,
    });
    
    toast.success("Xác thực thành công! Đăng nhập thành công.");
    
    // Save the tokens
    tokenService.setTokens(response.data.data);

    router.push('/'); // Redirect to home

  } catch (error) {
    if (error.response) {
      toast.error(error.response.data.message || "Mã OTP không chính xác hoặc đã hết hạn.");
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
