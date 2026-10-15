<script setup>
import { RouterLink, RouterView } from 'vue-router'
import { clearGlobalError, globalError } from './services/api'
import { direction, isArabic, t, toggleLanguage } from './i18n'
</script>

<template>
  <div class="app-shell" :dir="direction" :lang="isArabic ? 'ar' : 'en'">
    <header class="topbar">
      <RouterLink class="brand" to="/tasks"><img class="brand-logo" src="/task-tracker-logo.png" alt="" /><span>Task Tracker</span></RouterLink>
      <nav class="nav-links" :aria-label="t('dashboard')"><RouterLink to="/dashboard">{{ t('dashboard') }}</RouterLink><RouterLink to="/tasks">{{ t('tasks') }}</RouterLink><RouterLink to="/categories">{{ t('categories') }}</RouterLink><button class="language-toggle" type="button" @click="toggleLanguage">{{ isArabic ? 'English' : 'العربية' }}</button></nav>
    </header>
    <div v-if="globalError" class="global-error" role="alert"><span>{{ globalError }}</span><button class="text-button" type="button" @click="clearGlobalError">{{ t('dismiss') }}</button></div>
    <main class="page-container"><RouterView /></main>
  </div>
</template>
