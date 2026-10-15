<script setup>
import { RouterLink, RouterView } from 'vue-router'
import { clearGlobalError, globalError } from './services/api'
</script>

<template>
  <div class="app-shell">
    <header class="topbar">
      <RouterLink class="brand" to="/tasks"><img class="brand-logo" src="/task-tracker-logo.png" alt="" /><span>Task Tracker</span></RouterLink>
      <nav class="nav-links" aria-label="Main navigation"><RouterLink to="/dashboard">Dashboard</RouterLink><RouterLink to="/tasks">Tasks</RouterLink><RouterLink to="/categories">Categories</RouterLink></nav>
    </header>
    <div v-if="globalError" class="global-error" role="alert"><span>{{ globalError }}</span><button class="text-button" type="button" @click="clearGlobalError">Dismiss</button></div>
    <main class="page-container"><RouterView /></main>
  </div>
</template>
