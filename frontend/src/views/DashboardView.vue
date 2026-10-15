<script setup>
import { computed, onMounted, ref } from 'vue'
import { RouterLink } from 'vue-router'
import { assistantApi, categoryApi, clearGlobalError, getApiError, setGlobalError, taskApi } from '../services/api'

const tasks = ref([])
const categories = ref([])
const loading = ref(true)
const error = ref('')
const assistantPrompt = ref('')
const assistantDraft = ref(null)
const assistantLoading = ref(false)
const assistantSaving = ref(false)
const assistantError = ref('')

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

async function createAssistantDraft() {
  assistantLoading.value = true
  assistantError.value = ''
  clearGlobalError()
  try {
    const response = await assistantApi.createTaskDraft(assistantPrompt.value)
    assistantDraft.value = response.data
  } catch (requestError) {
    if (requestError.response?.status === 503) setGlobalError('The AI assistant is temporarily unavailable. Try again later.')
    else assistantError.value = getApiError(requestError, 'Unable to create an AI task draft.')
  } finally {
    assistantLoading.value = false
  }
}

async function saveAssistantDraft() {
  if (!assistantDraft.value?.categoryId) {
    assistantError.value = 'Choose an existing category before saving this task.'
    return
  }
  assistantSaving.value = true
  assistantError.value = ''
  try {
    await taskApi.create({
      title: assistantDraft.value.title,
      description: assistantDraft.value.description,
      status: 'Todo',
      priority: assistantDraft.value.priority,
      dueDate: assistantDraft.value.dueDate,
      categoryId: assistantDraft.value.categoryId,
    })
    assistantDraft.value = null
    assistantPrompt.value = ''
    await loadDashboard()
  } catch (requestError) {
    assistantError.value = getApiError(requestError, 'Unable to save the drafted task.')
  } finally {
    assistantSaving.value = false
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
  <section class="card assistant-panel">
    <div class="panel-heading"><div><p class="eyebrow">Optional assistant</p><h2>Describe a task</h2></div><span class="assistant-note">Draft first, save after review</span></div>
    <form class="assistant-form" @submit.prevent="createAssistantDraft">
      <label class="sr-only" for="assistant-prompt">Describe a task</label>
      <textarea id="assistant-prompt" v-model="assistantPrompt" rows="2" placeholder="Example: Prepare the interview notes next Friday, high priority" required></textarea>
      <button class="button primary" type="submit" :disabled="assistantLoading">{{ assistantLoading ? 'Thinking...' : 'Create draft' }}</button>
    </form>
    <p v-if="assistantError" class="alert error">{{ assistantError }}</p>
    <div v-if="assistantDraft" class="assistant-draft">
      <div><span class="summary-label">Draft preview</span><strong dir="auto">{{ assistantDraft.title }}</strong><p v-if="assistantDraft.description" dir="auto">{{ assistantDraft.description }}</p><p dir="auto">Suggested category: {{ assistantDraft.categoryName }}</p></div>
      <div class="assistant-fields">
        <label>Priority<select v-model="assistantDraft.priority"><option>Low</option><option>Medium</option><option>High</option></select></label>
        <label>Category<select v-model="assistantDraft.categoryId"><option :value="null">Choose a category</option><option v-for="category in categories" :key="category.id" :value="category.id">{{ category.name }}</option></select></label>
      </div>
      <div class="form-actions"><button class="button secondary" type="button" @click="assistantDraft = null">Discard</button><button class="button primary" type="button" :disabled="assistantSaving" @click="saveAssistantDraft">{{ assistantSaving ? 'Saving...' : 'Save task' }}</button></div>
    </div>
  </section>
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
