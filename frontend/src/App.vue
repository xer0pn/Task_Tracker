<script setup>
import { RouterLink, RouterView } from 'vue-router'
</script>

<template>
  <div class="app-shell">
    <header class="topbar">
      <RouterLink class="brand" to="/tasks"><img class="brand-logo" src="/task-tracker-logo.png" alt="" /><span>Task Tracker</span></RouterLink>
      <nav class="nav-links" aria-label="Main navigation"><RouterLink to="/dashboard">Dashboard</RouterLink><RouterLink to="/tasks">Tasks</RouterLink><RouterLink to="/categories">Categories</RouterLink></nav>
    </header>
    <main class="page-container"><RouterView /></main>
  </div>
</template>
