<script setup>
import { onMounted, ref } from 'vue'
import { categoryApi, getApiError } from '../services/api'
import { t } from '../i18n'

const categories = ref([]); const name = ref(''); const error = ref(''); const saving = ref(false)
async function load() { categories.value = (await categoryApi.list()).data }
async function createCategory() { error.value = ''; if (!name.value.trim()) { error.value = t('categoryRequired'); return }; saving.value = true; try { await categoryApi.create({ name: name.value.trim() }); name.value = ''; await load() } catch (requestError) { error.value = getApiError(requestError, t('unableCreateCategory')) } finally { saving.value = false } }
onMounted(load)
</script>

<template>
  <section class="page-heading"><div><p class="eyebrow">{{ t('organization') }}</p><h1>{{ t('categories') }}</h1><p class="muted">{{ t('groupTasks') }}</p></div></section>
  <div class="two-column"><section class="card form-card"><h2>{{ t('addCategory') }}</h2><p v-if="error" class="alert error">{{ error }}</p><form @submit.prevent="createCategory"><label>{{ t('name') }}<input v-model="name" maxlength="50" :placeholder="t('exampleHome')" required /></label><button class="button primary" :disabled="saving">{{ saving ? t('adding') : t('addCategory') }}</button></form></section><section class="card category-card"><h2>{{ t('allCategories') }}</h2><div v-for="category in categories" :key="category.id" class="category-row"><span>{{ category.name }}</span><span class="count-pill">{{ category.taskCount }} {{ t('tasksPlural') }}</span></div></section></div>
</template>
