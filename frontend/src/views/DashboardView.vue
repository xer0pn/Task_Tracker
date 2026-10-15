<script setup>
import { computed, onMounted, ref } from 'vue'
import { RouterLink } from 'vue-router'
import { assistantApi, categoryApi, clearGlobalError, getApiError, setGlobalError, taskApi } from '../services/api'
import { isArabic, t } from '../i18n'

const tasks = ref([])
const categories = ref([])
const loading = ref(true)
const error = ref('')
const assistantPrompt = ref('')
const assistantDraft = ref(null)
const assistantLoading = ref(false)
const assistantSaving = ref(false)
const assistantError = ref('')

const assistantRtl = computed(() => {
  const text = `${assistantPrompt.value} ${assistantDraft.value?.title ?? ''} ${assistantDraft.value?.description ?? ''}`
  return isArabic.value || /[\u0600-\u06ff\u0750-\u077f]/.test(text)
})

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
    if (requestError.response?.status === 503) setGlobalError(t('assistantUnavailable'))
    else assistantError.value = getApiError(requestError, t('unableAssistant'))
  } finally {
    assistantLoading.value = false
  }
}

async function saveAssistantDraft() {
  if (!assistantDraft.value?.categoryId) {
    assistantError.value = t('chooseExisting')
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
    assistantError.value = getApiError(requestError, t('unableSaveDraft'))
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
    <div><p class="eyebrow">{{ t('overview') }}</p><h1>{{ t('dashboard') }}</h1><p class="muted">{{ t('trackerSummary') }}</p></div>
    <RouterLink class="button primary" to="/tasks/new">+ {{ t('newTask') }}</RouterLink>
  </section>

  <p v-if="error" class="alert error">{{ error }}</p>
  <section class="card assistant-panel" :dir="assistantRtl ? 'rtl' : 'ltr'">
    <div class="panel-heading"><div><p class="eyebrow">{{ t('optionalAssistant') }}</p><h2>{{ t('describeTask') }}</h2></div><span class="assistant-note">{{ t('draftFirst') }}</span></div>
    <form class="assistant-form" @submit.prevent="createAssistantDraft">
      <label class="sr-only" for="assistant-prompt">{{ t('describeTask') }}</label>
      <textarea id="assistant-prompt" v-model="assistantPrompt" rows="2" :placeholder="t('assistantPlaceholder')" required></textarea>
      <button class="button primary" type="submit" :disabled="assistantLoading">{{ assistantLoading ? t('thinking') : t('createDraft') }}</button>
    </form>
    <p v-if="assistantError" class="alert error">{{ assistantError }}</p>
    <div v-if="assistantDraft" class="assistant-draft">
      <div><span class="summary-label">{{ t('draftPreview') }}</span><strong dir="auto">{{ assistantDraft.title }}</strong><p v-if="assistantDraft.description" dir="auto">{{ assistantDraft.description }}</p><p dir="auto">{{ t('suggestedCategory') }}: {{ assistantDraft.categoryName }}</p></div>
      <div class="assistant-fields">
        <label>{{ t('priority') }}<select v-model="assistantDraft.priority"><option value="Low">{{ t('low') }}</option><option value="Medium">{{ t('medium') }}</option><option value="High">{{ t('high') }}</option></select></label>
        <label>{{ t('category') }}<select v-model="assistantDraft.categoryId"><option :value="null">{{ t('chooseCategory') }}</option><option v-for="category in categories" :key="category.id" :value="category.id">{{ category.name }}</option></select></label>
      </div>
      <div class="form-actions"><button class="button secondary" type="button" @click="assistantDraft = null">{{ t('discard') }}</button><button class="button primary" type="button" :disabled="assistantSaving" @click="saveAssistantDraft">{{ assistantSaving ? t('saving') : t('saveTask') }}</button></div>
    </div>
  </section>
  <section v-if="loading" class="card state-message">{{ t('loadingDashboard') }}</section>
  <template v-else>
    <section class="summary-grid" :aria-label="t('overview')">
      <article class="card summary-card"><span class="summary-label">{{ t('totalTasks') }}</span><strong>{{ counts.total }}</strong><span class="summary-note">{{ t('acrossCategories') }}</span></article>
      <article class="card summary-card"><span class="summary-label">{{ t('toDo') }}</span><strong>{{ counts.todo }}</strong><span class="summary-note">{{ t('readyToStart') }}</span></article>
      <article class="card summary-card"><span class="summary-label">{{ t('inProgress') }}</span><strong>{{ counts.inProgress }}</strong><span class="summary-note">{{ t('currentlyActive') }}</span></article>
      <article class="card summary-card" :class="{ 'summary-warning': counts.overdue > 0 }"><span class="summary-label">{{ t('overdue') }}</span><strong>{{ counts.overdue }}</strong><span class="summary-note">{{ t('openPastDue') }}</span></article>
    </section>

    <section class="dashboard-grid">
      <article class="card dashboard-panel">
        <div class="panel-heading"><div><p class="eyebrow">{{ t('nextUp') }}</p><h2>{{ t('upcomingWork') }}</h2></div><RouterLink to="/tasks">{{ t('viewAll') }}</RouterLink></div>
        <div v-if="upcomingTasks.length === 0" class="panel-empty">{{ t('noUpcoming') }}</div>
        <RouterLink v-for="task in upcomingTasks" :key="task.id" class="upcoming-row" :to="`/tasks/${task.id}/edit`"><span><strong>{{ task.title }}</strong><small>{{ task.categoryName }}</small></span><time :datetime="task.dueDate">{{ formatDate(task.dueDate) }}</time></RouterLink>
      </article>
      <article class="card dashboard-panel">
        <div class="panel-heading"><div><p class="eyebrow">{{ t('distribution') }}</p><h2>{{ t('byCategory') }}</h2></div><RouterLink to="/categories">{{ t('manage') }}</RouterLink></div>
        <div v-for="category in categories" :key="category.id" class="category-progress"><div><span>{{ category.name }}</span><strong>{{ category.taskCount }}</strong></div><div class="progress-track"><span :style="{ width: `${counts.total ? (category.taskCount / counts.total) * 100 : 0}%` }"></span></div></div>
      </article>
    </section>
  </template>
</template>
