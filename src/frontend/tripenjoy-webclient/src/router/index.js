import { createRouter, createWebHistory } from "vue-router";

const routes = [
  {
    path: "",
    name: "Home",
    component: () => import("@/views/HomeView.vue"),
  },
  // Other routes that use DefaultLayout can be added here
];

const router = createRouter({
  history: createWebHistory(),
  routes,
});

export default router;
