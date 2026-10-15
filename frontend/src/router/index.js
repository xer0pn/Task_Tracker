import { createRouter, createWebHistory } from 'vue-router'
import TaskListView from '../views/TaskListView.vue'
import TaskFormView from '../views/TaskFormView.vue'
import CategoriesView from '../views/CategoriesView.vue'
import DashboardView from '../views/DashboardView.vue'

export default createRouter({
  history: createWebHistory(),
  routes: [
    { path: '/', redirect: '/tasks' },
    { path: '/dashboard', component: DashboardView },
    { path: '/tasks', component: TaskListView },
    { path: '/tasks/new', component: TaskFormView },
    { path: '/tasks/:id/edit', component: TaskFormView },
    { path: '/categories', component: CategoriesView },
  ],
})
