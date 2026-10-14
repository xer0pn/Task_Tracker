<script setup>
import { onMounted, ref, watch } from 'vue'
import { RouterLink } from 'vue-router'
import { categoryApi, getApiError, taskApi } from '../services/api'

const tasks = ref([])
const categories = ref([])
const loading = ref(true)
const error = ref('')
const page = ref(1)
const pageSize = 8
const totalPages = ref(1)
const totalCount = ref(0)
const filters = ref({ search: '', status: '', categoryId: '' })
const statuses = ['Todo', 'InProgress', 'Done']

async function loadCategories() {
  try { categories.value = (await categoryApi.list()).data } catch (requestError) { error.value = getApiError(requestError, 'Unable to load categories.') }
}

async function loadTasks() {
  loading.value = true; error.value = ''
  try {
    const response = await taskApi.list({ ...filters.value, page: page.value, pageSize })
    tasks.value = response.data.items; totalPages.value = Math.max(response.data.totalPages, 1); totalCount.value = response.data.totalCount
  } catch (requestError) { error.value = getApiError(requestError, 'Unable to load tasks.') } finally { loading.value = false }
}

function resetToFirstPage() { page.value = 1; loadTasks() }
async function changeStatus(task, status) { try { await taskApi.updateStatus(task.id, status); await loadTasks() } catch (requestError) { error.value = getApiError(requestError, 'Unable to update task status.') } }
async function removeTask(task) { if (!window.confirm(`Delete “${task.title}”?`)) return; try { await taskApi.remove(task.id); await loadTasks() } catch (requestError) { error.value = getApiError(requestError, 'Unable to delete task.') } }
function formatDate(date) { return date ? new Date(date).toLocaleDateString() : '—' }
function badgeClass(value) { return value.toLowerCase().replace('inprogress', 'in-progress') }

watch(filters, resetToFirstPage, { deep: true })
watch(page, loadTasks)
onMounted(() => Promise.all([loadCategories(), loadTasks()]))
</script>

<template>
  <section class="page-heading"><div><p class="eyebrow">Workspace</p><h1>Tasks</h1><p class="muted">Keep the important work moving.</p></div><RouterLink class="button primary" to="/tasks/new">+ New task</RouterLink></section>
  <section class="toolbar card"><label class="search-field"><span class="sr-only">Search tasks</span><input v-model="filters.search" type="search" placeholder="Search task titles..." /></label><select v-model="filters.status" aria-label="Filter by status"><option value="">All statuses</option><option v-for="status in statuses" :key="status" :value="status">{{ status }}</option></select><select v-model="filters.categoryId" aria-label="Filter by category"><option value="">All categories</option><option v-for="category in categories" :key="category.id" :value="category.id">{{ category.name }}</option></select></section>
  <p v-if="error" class="alert error">{{ error }}</p>
  <section v-if="loading" class="card state-message">Loading tasks...</section>
  <section v-else-if="tasks.length === 0" class="card state-message"><strong>No tasks found</strong><span>Try changing your filters or create a new task.</span></section>
  <section v-else class="card table-card"><div class="table-header"><span>{{ totalCount }} {{ totalCount === 1 ? 'task' : 'tasks' }}</span></div><div class="table-wrap"><table><thead><tr><th>Task</th><th>Category</th><th>Status</th><th>Priority</th><th>Due</th><th></th></tr></thead><tbody><tr v-for="task in tasks" :key="task.id"><td><strong>{{ task.title }}</strong><small v-if="task.description">{{ task.description }}</small></td><td>{{ task.categoryName }}</td><td><select :value="task.status" :aria-label="`Status for ${task.title}`" @change="changeStatus(task, $event.target.value)"><option v-for="status in statuses" :key="status" :value="status">{{ status }}</option></select></td><td><span class="badge" :class="badgeClass(task.priority)">{{ task.priority }}</span></td><td>{{ formatDate(task.dueDate) }}</td><td class="actions"><RouterLink :to="`/tasks/${task.id}/edit`">Edit</RouterLink><button class="text-button danger-text" @click="removeTask(task)">Delete</button></td></tr></tbody></table></div><div class="pagination"><button class="button secondary" :disabled="page <= 1" @click="page--">Previous</button><span>Page {{ page }} of {{ totalPages }}</span><button class="button secondary" :disabled="page >= totalPages" @click="page++">Next</button></div></section>
</template>
