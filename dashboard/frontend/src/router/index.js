import { createWebHistory, createRouter } from "vue-router";
import HomePage from "@/components/HomePage.vue";
import DetailsPage from '@/components/DetailsPage.vue'
import SettingsPage from '@/components/SettingsPage.vue'

const routes = [
    {
        path: "/",
        name: "home",
        component: HomePage,
    },
    {
        path: "/details",
        name: "counter",
        component: DetailsPage,
    },
    {
        path: "/settings",
        name: "settings",
        component: SettingsPage,
    }
];

const router = createRouter({
    history: createWebHistory(),
    routes,
});

export default router;