<script setup>
import { computed, onMounted, ref } from 'vue'
import { RouterLink } from 'vue-router'
import { categoryApi, getApiError, taskApi } from '../services/api'

const tasks = ref([])
const categories = ref([])
const loading = ref(true)
const error = ref('')

const counts = computed(() => ({
  total: tasks.value.length,
  todo: tasks.value.filter((task) => task.status === 'Todo').length,
  inProgress: tasks.value.filter((task) => task.status === 'InProgress').length,
  done: tasks.value.filter((task) => task.status === 'Done').length,
  overdue: tasks.value.filter((task) => task.dueDate && task.status !== 'Done' && new Date(task.dueDate) < new Date()).length,
}))

const upcomingTasks = computed(() => tasks.value
  .filter((task) => task.dueDate && task.status !== 'Done')
  .sort((a, b) => new Date(a.dueDate) - new Date(b.dueDate))
  .slice(0, 5))

async function loadDashboard() {
  loading.value = true
  error.value = ''
  try {
    const [taskResponse, categoryResponse] = await Promise.all([
      taskApi.list({ page: 1, pageSize: 50 }),
      categoryApi.list(),
    ])
    tasks.value = taskResponse.data.items
    categories.value = categoryResponse.data
  } catch (requestError) {
    error.value = getApiError(requestError, 'Unable to load dashboard.')
  } finally {
    loading.value = false
  }
}

function formatDate(date) {
  return new Date(date).toLocaleDateString(undefined, { month: 'short', day: 'numeric' })
}

onMounted(loadDashboard)
</script>

<template>
  <section class="page-heading">
    <div><p class="eyebrow">Overview</p><h1>Dashboard</h1><p class="muted">A quick read on the work in your tracker.</p></div>
    <RouterLink class="button primary" to="/tasks/new">+ New task</RouterLink>
  </section>

  <p v-if="error" class="alert error">{{ error }}</p>
  <section v-if="loading" class="card state-message">Loading dashboard...</section>
  <template v-else>
    <section class="summary-grid" aria-label="Task summary">
      <article class="card summary-card"><span class="summary-label">Total tasks</span><strong>{{ counts.total }}</strong><span class="summary-note">Across all categories</span></article>
      <article class="card summary-card"><span class="summary-label">To do</span><strong>{{ counts.todo }}</strong><span class="summary-note">Ready to start</span></article>
      <article class="card summary-card"><span class="summary-label">In progress</span><strong>{{ counts.inProgress }}</strong><span class="summary-note">Currently active</span></article>
      <article class="card summary-card" :class="{ 'summary-warning': counts.overdue > 0 }"><span class="summary-label">Overdue</span><strong>{{ counts.overdue }}</strong><span class="summary-note">Open tasks past due</span></article>
    </section>

    <section class="dashboard-grid">
      <article class="card dashboard-panel">
        <div class="panel-heading"><div><p class="eyebrow">Next up</p><h2>Upcoming work</h2></div><RouterLink to="/tasks">View all</RouterLink></div>
        <div v-if="upcomingTasks.length === 0" class="panel-empty">No upcoming tasks.</div>
        <RouterLink v-for="task in upcomingTasks" :key="task.id" class="upcoming-row" :to="`/tasks/${task.id}/edit`"><span><strong>{{ task.title }}</strong><small>{{ task.categoryName }}</small></span><time :datetime="task.dueDate">{{ formatDate(task.dueDate) }}</time></RouterLink>
      </article>
      <article class="card dashboard-panel">
        <div class="panel-heading"><div><p class="eyebrow">Distribution</p><h2>By category</h2></div><RouterLink to="/categories">Manage</RouterLink></div>
        <div v-for="category in categories" :key="category.id" class="category-progress"><div><span>{{ category.name }}</span><strong>{{ category.taskCount }}</strong></div><div class="progress-track"><span :style="{ width: `${counts.total ? (category.taskCount / counts.total) * 100 : 0}%` }"></span></div></div>
      </article>
    </section>
  </template>
</template>
