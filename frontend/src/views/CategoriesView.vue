<script setup>
import { onMounted, ref } from 'vue'
import { categoryApi, getApiError } from '../services/api'

const categories = ref([]); const name = ref(''); const error = ref(''); const saving = ref(false)
async function load() { categories.value = (await categoryApi.list()).data }
async function createCategory() { error.value = ''; if (!name.value.trim()) { error.value = 'Category name is required.'; return }; saving.value = true; try { await categoryApi.create({ name: name.value.trim() }); name.value = ''; await load() } catch (requestError) { error.value = getApiError(requestError, 'Unable to create category.') } finally { saving.value = false } }
onMounted(load)
</script>

<template>
  <section class="page-heading"><div><p class="eyebrow">Organization</p><h1>Categories</h1><p class="muted">Group tasks so they are easier to find.</p></div></section>
  <div class="two-column"><section class="card form-card"><h2>Add category</h2><p v-if="error" class="alert error">{{ error }}</p><form @submit.prevent="createCategory"><label>Name<input v-model="name" maxlength="50" placeholder="e.g. Home" required /></label><button class="button primary" :disabled="saving">{{ saving ? 'Adding...' : 'Add category' }}</button></form></section><section class="card category-card"><h2>All categories</h2><div v-for="category in categories" :key="category.id" class="category-row"><span>{{ category.name }}</span><span class="count-pill">{{ category.taskCount }} tasks</span></div></section></div>
</template>
