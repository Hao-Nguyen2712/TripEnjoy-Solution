<template>
  <div class="app-layout">
    <header class="app-header">
      <div class="logo">TripEnjoy</div>
      <nav class="app-nav">
        <router-link to="/">Home</router-link>
      </nav>
      <div class="user-actions">
         <router-link to="/login">Login</router-link>
      </div>
    </header>
    <main class="app-main">
      <router-view />
    </main>
    <footer class="app-footer">
      <p>&copy; 2025 TripEnjoy. All rights reserved.</p>
    </footer>
  </div>
</template>

<script setup>
</script>

<style scoped>
.app-layout {
  display: flex;
  flex-direction: column;
  min-height: 100vh;
}
.app-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1rem 2rem;
  background-color: #fff;
  border-bottom: 1px solid #eaeaea;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.05);
}
.logo {
  font-weight: bold;
  font-size: 1.5rem;
  color: #35495E;
}
.app-nav a {
  margin: 0 1rem;
  text-decoration: none;
  color: #333;
  font-weight: 500;
}
.app-nav a.router-link-exact-active {
  color: #42b883;
}
.app-main {
  flex-grow: 1;
  padding: 2rem;
}
.app-footer {
  padding: 1rem 2rem;
  text-align: center;
  background-color: #f8f8f8;
  border-top: 1px solid #eaeaea;
  color: #666;
}
</style>
