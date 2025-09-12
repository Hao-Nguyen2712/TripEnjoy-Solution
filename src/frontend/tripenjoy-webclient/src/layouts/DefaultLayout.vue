<template>
  <div class="app-layout">
    <header class="app-header">
   
     <div class="container-fluid bg-dark px-0 header">
         <div class="row gx-0">
             <div class="col-lg-3 bg-dark d-none d-lg-block">
                 <router-link to="/"
                    class="navbar-brand w-100 h-100 m-0 p-0 d-flex align-items-center justify-content-center">
                     <h1 class="m-0 text-warning text-uppercase">tripenjoy</h1>
                 </router-link>
             </div>
             <div class="col-lg-9">
                 <nav class="navbar navbar-expand-lg bg-dark navbar-dark p-3 p-lg-0">
                     <router-link to="/" class="navbar-brand d-block d-lg-none">
                         <h1 class="m-0 text-warning text-uppercase">tripenjoy</h1>
                     </router-link>
                     <button type="button" class="navbar-toggler" data-bs-toggle="collapse"
                             data-bs-target="#navbarCollapse">
                         <span class="navbar-toggler-icon"></span>
                     </button>
                     <div class="collapse navbar-collapse justify-content-between" id="navbarCollapse">
                         <div class="navbar-nav mr-auto py-0">
                             <router-link to="/" class="nav-link">Home</router-link>
                             <router-link to="/about" class="nav-link">About</router-link>
                             <div class="nav-item dropdown">
                                 <router-link to="/partnership" class="nav-link" >Partnership</router-link>
                             </div>
                         </div>
                         
                         <div class="navbar-nav">
                           <template v-if="isLoggedIn">
                             <a @click.prevent="handleLogout" class="nav-item nav-link" style="cursor: pointer;">Logout</a>
                           </template>
                           <template v-else>
                             <router-link to="/login" class="nav-item nav-link">Login</router-link>
                             <router-link to="/signup" class="nav-item nav-link">Sign Up</router-link>
                           </template>
                         </div>
                     </div>
                 </nav>
             </div>
         </div>
     </div>


     <div class="container-fluid p-0 mb-5">
         <div id="header-carousel" class="carousel slide" data-bs-ride="carousel">
             <div class="carousel-inner">
                 <div class="carousel-item active">
                     <img class="w-100" src="../assets/images/carousel-1.jpg" alt="Image">
                     <div class="carousel-caption d-flex flex-column align-items-center justify-content-center">
                         <div class="p-3" style="max-width: 700px;">
                             <h6 class="section-title text-white text-uppercase mb-3 animated slideInDown">
                                 TripEnjoy
                             </h6>
                             <h1 class="display-3 text-white mb-4 animated slideInDown">
                                 Smart Booking <br>
                                 Enjoy Your Journey
                             </h1>
                             <a href="#"
                                class="btn btn-warning text-white py-md-3 px-md-5 me-3 animated slideInLeft">
                                 Our Rooms
                             </a>
                             <a href="#vietnam-hotels" class="btn btn-light text-black py-md-3 px-md-5 animated slideInRight">
                                 Booking
                                 Now
                             </a>
                         </div>
                     </div>
                 </div>
                 <div class="carousel-item">
                     <img class="w-100" src="../assets/images/carousel-1.jpg" alt="Image">
                     <div class="carousel-caption d-flex flex-column align-items-center justify-content-center">
                         <div class="p-3" style="max-width: 700px;">
                             <h6 class="section-title text-white text-uppercase mb-3 animated slideInDown">
                                 Trip Enjoy
                             </h6>
                             <h1 class="display-3 text-white mb-4 animated slideInDown">
                                 Smart Booking <br />
                                 Enjoy The Journey
                             </h1>

                             <a href="#vietnam-hotels"
                                class="btn btn-warning text-white py-md-3 px-md-5 me-3 animated slideInLeft">
                                 Our Rooms
                             </a>
                             <a href="#vietnam-hotels" class="btn btn-light text-black py-md-3 px-md-5 animated slideInRight">
                                 Booking
                                 Now
                             </a>
                         </div>
                     </div>
                 </div>
             </div>
             <button class="carousel-control-prev" type="button" data-bs-target="#header-carousel"
                     data-bs-slide="prev">
                 <span class="carousel-control-prev-icon" aria-hidden="true"></span>
                 <span class="visually-hidden">Previous</span>
             </button>
             <button class="carousel-control-next" type="button" data-bs-target="#header-carousel"
                     data-bs-slide="next">
                 <span class="carousel-control-next-icon" aria-hidden="true"></span>
                 <span class="visually-hidden">Next</span>
             </button>
         </div>
     </div>
    </header>

    <main class="app-main">
      <router-view />
    </main>

    <footer class="app-footer">
      <!-- Footer Start -->
      <div class="container-fluid bg-dark text-light footer wow fadeIn" data-wow-delay="0.1s">
       <div class="container pb-5">
           <div class="row g-5">
               <div class="col-md-6 col-lg-4">
                   <div class="bg-primary rounded p-4">
                       <router-link to="/">
                           <h1 class="text-white text-uppercase mb-3">TripEnjoy</h1>
                       </router-link>
                       <p class="text-white mb-0">Book Your Stay, Embrace the Journey.</p>
                   </div>
               </div>
               <div class="col-md-6 col-lg-3">
                   <h6 class="section-title text-start text-primary text-uppercase mb-4">Contact</h6>
                   <p class="mb-2"><i class="fa fa-map-marker-alt me-3"></i>Nguyen Van Cu, Can Tho, Viet Nam</p>
                   <p class="mb-2"><i class="fa fa-phone-alt me-3"></i>+012 345 67890</p>
                   <p class="mb-2">
                       <i class="fa fa-envelope me-3"></i><a href="mailto:info@tripenjoy.com">info@tripenjoy.com</a>
                   </p>
                   <div class="d-flex pt-2">
                       <a class="btn btn-outline-light btn-social" href><i class="fab fa-twitter"></i></a>
                       <a class="btn btn-outline-light btn-social" href><i class="fab fa-facebook-f"></i></a>
                       <a class="btn btn-outline-light btn-social" href><i class="fab fa-youtube"></i></a>
                       <a class="btn btn-outline-light btn-social" href><i class="fab fa-linkedin-in"></i></a>
                   </div>
               </div>
               <div class="col-lg-5 col-md-12">
                   <div class="row gy-5 g-4">
                       <div class="col-md-6">
                           <h6 class="section-title text-start text-primary text-uppercase mb-4">Company</h6>
                           <a class="btn btn-link" href>About Us</a>
                           <a class="btn btn-link" href>Contact Us</a>
                           <a class="btn btn-link" href>Privacy Policy</a>
                           <a class="btn btn-link" href>Terms & Condition</a>
                           <a class="btn btn-link" href>Support</a>
                       </div>
                       <div class="col-md-6">
                           <h6 class="section-title text-start text-primary text-uppercase mb-4">Services</h6>
                           <a class="btn btn-link" href>Food & Restaurant</a>
                           <a class="btn btn-link" href>Spa & Fitness</a>
                           <a class="btn btn-link" href>Sports & Gaming</a>
                           <a class="btn btn-link" href>Event & Party</a>
                           <a class="btn btn-link" href>GYM & Yoga</a>
                       </div>
                   </div>
               </div>
           </div>
       </div>
       <div class="container">
           <div class="copyright">
               <div class="row">
                   <div class="col-md-6 text-center text-md-start mb-3 mb-md-0">
                       &copy; <a class="border-bottom" href="#">TripEnjoy</a>, All Right Reserved. Designed By
                       <a class="border-bottom" href="/">Group 04</a>
                   </div>
                   <div class="col-md-6 text-center text-md-end">
                       <div class="footer-menu">
                           <a href>Home</a>
                           <a href>Cookies</a>
                           <a href>Help</a>
                           <a href>FQAs</a>
                       </div>
                   </div>
               </div>
           </div>
       </div>
   </div>

   <a href="#" class="btn btn-lg btn-primary btn-lg-square back-to-top"><i class="bi bi-arrow-up"></i></a>
      <!-- Footer End -->
    </footer>
  </div>
</template>

<script setup>
import { ref, onMounted } from "vue";
import { useRouter } from "vue-router";
import tokenService from "@/services/tokenService";
import authService from "@/services/authService";

const router = useRouter();
const isLoggedIn = ref(false);

onMounted(() => {
  // Check token existence to determine login status
  const token = tokenService.getAccessToken();
  isLoggedIn.value = !!token;
});

const handleLogout = () => {
  authService.logout();
  isLoggedIn.value = false;
  // Use .push for SPA navigation, .href for full page reload
  router.push("/login");
};
</script>

<style scoped>
:root {
  --primary: #0dcaf0; /* Using a light blue color from the image hint */
  --light: #f8f9fa;
  --dark: #1B1F24;
}

.app-main {
  min-height: 60vh;
}

/*** Navbar ***/
.sticky-top {
    top: -100px;
    transition: .5s;
}

.navbar .navbar-nav .nav-link {
    margin-right: 35px;
    padding: 25px 0;
    color: var(--dark);
    font-weight: 500;
    outline: none;
}

.navbar .navbar-nav .nav-link:hover,
.navbar .navbar-nav .nav-link.active {
    color: var(--primary);
}

.navbar.sticky-top a.navbar-brand {
    padding-left: 0;
}

.btn-outline-primary {
    color: var(--primary);
    border-color: var(--primary);
}

.btn-outline-primary:hover {
    background-color: var(--primary);
    color: white;
}

/*** Footer ***/
.footer {
    background: var(--light);
    color: #6c757d;
}

.footer h2, .footer h4 {
    color: var(--dark);
}

.footer .btn-link {
    display: block;
    margin-bottom: 5px;
    padding: 0;
    text-align: left;
    color: #6c757d;
    font-weight: normal;
    text-transform: capitalize;
    transition: .3s;
    border: 1px solid #dee2e6;
    padding: 5px 10px;
    text-align: center;
}

.footer .btn-link:hover {
    color: var(--primary);
    letter-spacing: 1px;
    box-shadow: none;
}

.footer .btn-social {
    display: flex;
    align-items: center;
    justify-content: center;
    width: 35px;
    height: 35px;
    border-radius: 2px;
    margin-right: 5px;
}

.copyright {
    padding: 25px 0;
    font-size: 15px;
    border-top: 1px solid rgba(0, 0, 0, .08);
}

.copyright a {
    color: var(--dark);
}

.copyright a:hover {
    color: var(--primary);
}
</style>
