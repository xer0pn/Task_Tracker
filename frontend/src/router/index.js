import { createRouter, createWebHistory } from 'vue-router'
import TaskListView from '../views/TaskListView.vue'
import TaskFormView from '../views/TaskFormView.vue'
import CategoriesView from '../views/CategoriesView.vue'

export default createRouter({
  history: createWebHistory(),
  routes: [
    { path: '/', redirect: '/tasks' },
    { path: '/tasks', component: TaskListView },
    { path: '/tasks/new', component: TaskFormView },
    { path: '/tasks/:id/edit', component: TaskFormView },
    { path: '/categories', component: CategoriesView },
  ],
})
