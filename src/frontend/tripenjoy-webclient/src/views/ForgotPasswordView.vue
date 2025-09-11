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
            <h2>Forgot Password</h2>
            <p>Enter your email to receive a reset link.</p>
            <form @submit.prevent="handleForgotPassword" class="mt-4" novalidate>
              <div class="ui left icon input swdh95">
                <input class="prompt srch_explore" type="email" v-model="email" required placeholder="Email Address" />
                <i class="uil uil-envelope icon icon2"></i>
              </div>

              <button class="login-btn" type="submit" :disabled="isLoading">
                {{ isLoading ? 'Sending...' : 'Send Reset Link' }}
              </button>
            </form>
            <p class="mb-0 mt-30 hvsng145">Back to <router-link to="/login">Log In</router-link></p>
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

const email = ref('');
const isLoading = ref(false);
const toast = useToast();
const router = useRouter();

const validateEmail = (emailToTest) => {
  const re = /^(([^<>()[\]\\.,;:\s@"]+(\.[^<>()[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
  return re.test(String(emailToTest).toLowerCase());
};

const handleForgotPassword = async () => {
  if (!email.value || !validateEmail(email.value)) {
    toast.error("Please enter a valid email address.");
    return;
  }
  
  toast.success(`A reset link has been sent to ${email.value}. Redirecting...`);
  // Placeholder for API call
  // isLoading.value = true;
  // try {
  //   await authService.forgotPassword({ email: email.value });
  //   toast.success("A reset link has been sent to your email.");
  //   router.push('/verify-otp');
  // } catch (error) {
  //   toast.error("Failed to send reset link.");
  // } finally {
  //   isLoading.value = false;
  // }

  // Simulate success and redirect
  setTimeout(() => {
    router.push('/verify-otp');
  }, 2000);
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
