<script setup>
import { RouterLink, RouterView } from 'vue-router'
</script>

<template>
  <div class="app-shell">
    <header class="topbar">
      <RouterLink class="brand" to="/tasks"><span class="brand-mark">✓</span><span>Task Tracker</span></RouterLink>
      <nav class="nav-links" aria-label="Main navigation"><RouterLink to="/tasks">Tasks</RouterLink><RouterLink to="/categories">Categories</RouterLink></nav>
    </header>
    <main class="page-container"><RouterView /></main>
  </div>
</template>
