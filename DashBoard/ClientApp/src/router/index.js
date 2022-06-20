import { createWebHistory, createRouter } from "vue-router";
import Home from "@/components/Home.vue";
import Details from '@/components/Details.vue'
import Settings from '@/components/Settings.vue'

const routes = [
    {
        path: "/",
        name: "home",
        component: Home,
    },
    {
        path: "/details",
        name: "counter",
        component: Details,
    },
    {
        path: "/settings",
        name: "settings",
        component: Settings,
    }
];

const router = createRouter({
    history: createWebHistory(),
    routes,
});

export default router;