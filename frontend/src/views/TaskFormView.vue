<script setup>
import { onMounted, reactive, ref } from 'vue'
import { RouterLink, useRoute, useRouter } from 'vue-router'
import { categoryApi, getApiError, taskApi } from '../services/api'

const route = useRoute(); const router = useRouter(); const categories = ref([]); const loading = ref(false); const error = ref(''); const isEditing = Boolean(route.params.id)
const form = reactive({ title: '', description: '', status: 'Todo', priority: 'Medium', dueDate: '', categoryId: '' })

async function load() {
  try {
    categories.value = (await categoryApi.list()).data
    if (isEditing) { const task = (await taskApi.get(route.params.id)).data; Object.assign(form, { ...task, dueDate: task.dueDate?.slice(0, 10) || '' }) }
    else if (categories.value.length) form.categoryId = categories.value[0].id
  } catch (requestError) { error.value = getApiError(requestError, 'Unable to load the form.') }
}

async function submit() {
  error.value = ''
  if (!form.title.trim()) { error.value = 'Title is required.'; return }
  if (!form.categoryId) { error.value = 'Category is required.'; return }
  loading.value = true
  try {
    const payload = { ...form, title: form.title.trim(), description: form.description.trim() || null, categoryId: Number(form.categoryId), dueDate: form.dueDate || null }
    if (isEditing) await taskApi.update(route.params.id, payload); else await taskApi.create(payload)
    router.push('/tasks')
  } catch (requestError) { error.value = getApiError(requestError, 'Unable to save the task.') } finally { loading.value = false }
}

onMounted(load)
</script>

<template>
  <section class="page-heading"><div><p class="eyebrow">Task details</p><h1>{{ isEditing ? 'Edit task' : 'Create task' }}</h1><p class="muted">Add the details needed to make the next step clear.</p></div></section>
  <form class="card form-card" @submit.prevent="submit"><p v-if="error" class="alert error">{{ error }}</p><label>Title<input v-model="form.title" maxlength="200" required placeholder="What needs to be done?" /></label><label>Description<textarea v-model="form.description" rows="4" placeholder="Add useful context (optional)"></textarea></label><div class="form-grid"><label>Status<select v-model="form.status"><option>Todo</option><option>InProgress</option><option>Done</option></select></label><label>Priority<select v-model="form.priority"><option>Low</option><option>Medium</option><option>High</option></select></label><label>Category<select v-model="form.categoryId" required><option value="" disabled>Select a category</option><option v-for="category in categories" :key="category.id" :value="category.id">{{ category.name }}</option></select></label><label>Due date<input v-model="form.dueDate" type="date" /></label></div><div class="form-actions"><RouterLink class="button secondary" to="/tasks">Cancel</RouterLink><button class="button primary" :disabled="loading">{{ loading ? 'Saving...' : 'Save task' }}</button></div></form>
</template>
