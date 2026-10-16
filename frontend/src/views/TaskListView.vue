<script setup>
import { onMounted, ref, watch } from "vue";
import { RouterLink } from "vue-router";
import { categoryApi, getApiError, taskApi } from "../services/api";
import AppIcon from "../components/AppIcon.vue";
import { priorityLabel, statusLabel, t } from "../i18n";

const tasks = ref([]);
const categories = ref([]);
const loading = ref(true);
const error = ref("");
const page = ref(1);
const pageSize = 8;
const totalPages = ref(1);
const totalCount = ref(0);
const filters = ref({ search: "", status: "", categoryId: "" });
const statuses = ["Todo", "InProgress", "Done"];

async function loadCategories() {
  try {
    categories.value = (await categoryApi.list()).data;
  } catch (requestError) {
    error.value = getApiError(requestError, t("unableCategories"));
  }
}

async function loadTasks() {
  loading.value = true;
  error.value = "";

  try {
    const response = await taskApi.list({
      ...filters.value,
      page: page.value,
      pageSize,
    });

    tasks.value = response.data.items;
    totalPages.value = Math.max(response.data.totalPages, 1);
    totalCount.value = response.data.totalCount;
  } catch (requestError) {
    error.value = getApiError(requestError, t("unableTasks"));
  } finally {
    loading.value = false;
  }
}

function resetToFirstPage() {
  page.value = 1;
  loadTasks();
}

async function changeStatus(task, status) {
  try {
    await taskApi.updateStatus(task.id, status);
    await loadTasks();
  } catch (requestError) {
    error.value = getApiError(requestError, t("unableSave"));
  }
}

async function removeTask(task) {
  if (!window.confirm(`${t("delete")} “${task.title}”?`)) {
    return;
  }

  try {
    await taskApi.remove(task.id);
    await loadTasks();
  } catch (requestError) {
    error.value = getApiError(requestError, t("unableSave"));
  }
}

function formatDate(date) {
  return date ? new Date(date).toLocaleDateString() : "—";
}

function badgeClass(value) {
  return value.toLowerCase().replace("inprogress", "in-progress");
}

watch(filters, resetToFirstPage, { deep: true });
watch(page, loadTasks);
onMounted(() => Promise.all([loadCategories(), loadTasks()]));
</script>

<template>
  <section class="page-heading">
    <div>
      <p class="eyebrow">{{ t("workspace") }}</p>
      <h1>{{ t("tasks") }}</h1>
      <p class="muted">{{ t("keepMoving") }}</p>
    </div>

    <RouterLink class="button primary" to="/tasks/new">
      + {{ t("newTask") }}
    </RouterLink>
  </section>

  <section class="toolbar card">
    <label class="search-field">
      <span class="sr-only">{{ t("tasks") }}</span>
      <input
        v-model="filters.search"
        type="search"
        :placeholder="t('search')"
      />
    </label>

    <select v-model="filters.status" :aria-label="t('status')">
      <option value="">{{ t("allStatuses") }}</option>
      <option v-for="status in statuses" :key="status" :value="status">
        {{ statusLabel(status) }}
      </option>
    </select>

    <select v-model="filters.categoryId" :aria-label="t('category')">
      <option value="">{{ t("allCategories") }}</option>
      <option
        v-for="category in categories"
        :key="category.id"
        :value="category.id"
      >
        {{ category.name }}
      </option>
    </select>
  </section>

  <p v-if="error" class="alert error">{{ error }}</p>

  <section v-if="loading" class="card state-message">
    {{ t("loadingTasks") }}
  </section>

  <section v-else-if="tasks.length === 0" class="card state-message">
    <strong>{{ t("noTasks") }}</strong>
    <span>{{ t("filterHint") }}</span>
  </section>

  <section v-else class="card table-card">
    <div class="table-header">
      <span>
        {{ totalCount }}
        {{ totalCount === 1 ? t("task") : t("tasksPlural") }}
      </span>
    </div>

    <div class="table-wrap">
      <table>
        <thead>
          <tr>
            <th>{{ t("title") }}</th>
            <th>{{ t("category") }}</th>
            <th>{{ t("status") }}</th>
            <th>{{ t("priority") }}</th>
            <th>{{ t("dueDate") }}</th>
            <th></th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="task in tasks" :key="task.id">
            <td>
              <strong>{{ task.title }}</strong>
              <small v-if="task.description">{{ task.description }}</small>
            </td>
            <td>{{ task.categoryName }}</td>
            <td>
              <select
                :value="task.status"
                :aria-label="`${t('status')} ${task.title}`"
                @change="changeStatus(task, $event.target.value)"
              >
                <option
                  v-for="status in statuses"
                  :key="status"
                  :value="status"
                >
                  {{ statusLabel(status) }}
                </option>
              </select>
            </td>
            <td>
              <span class="badge" :class="badgeClass(task.priority)">
                {{ priorityLabel(task.priority) }}
              </span>
            </td>
            <td>{{ formatDate(task.dueDate) }}</td>
            <td class="actions">
              <RouterLink
                class="icon-button"
                :to="`/tasks/${task.id}/edit`"
                :aria-label="`${t('edit')} ${task.title}`"
                :title="t('edit')"
              >
                <AppIcon name="edit" />
              </RouterLink>
              <button
                class="icon-button danger-text"
                :aria-label="`${t('delete')} ${task.title}`"
                :title="t('delete')"
                @click="removeTask(task)"
              >
                <AppIcon name="delete" />
              </button>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <div class="pagination">
      <button class="button secondary" :disabled="page <= 1" @click="page--">
        {{ t("previous") }}
      </button>
      <span>{{ t("page") }} {{ page }} {{ t("of") }} {{ totalPages }}</span>
      <button
        class="button secondary"
        :disabled="page >= totalPages"
        @click="page++"
      >
        {{ t("next") }}
      </button>
    </div>
  </section>
</template>
