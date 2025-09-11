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
              <div class="ui left icon input swdh95">
                <input class="prompt srch_explore" type="text" v-model="otp" required placeholder="Enter OTP Code" />
                <i class="uil uil-comment-alt-verify icon icon2"></i>
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
import { ref } from 'vue';
import { useToast } from 'vue-toastification';
import { useRouter } from 'vue-router';
import logoUrl from '@/assets/images/logo.svg';
import logoInverseUrl from '@/assets/images/ct_logo.svg';
import signLogoUrl from '@/assets/images/sign_logo.png';

const otp = ref('');
const isLoading = ref(false);
const toast = useToast();
const router = useRouter();

const handleOtpVerification = async () => {
  if (!otp.value || otp.value.length < 6) { // Assuming OTP is 6 digits
    toast.error("Please enter a valid OTP code.");
    return;
  }
  
  toast.success("Verification successful!");
  // Placeholder for API call
  // After successful verification, you might redirect to a 'reset-password' page
  console.log("OTP Submitted:", otp.value);
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
