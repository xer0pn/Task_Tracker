<script setup>
import { onMounted, reactive, ref } from "vue";
import { RouterLink, useRoute, useRouter } from "vue-router";
import { categoryApi, getApiError, taskApi } from "../services/api";
import { t } from "../i18n";

const route = useRoute();
const router = useRouter();
const categories = ref([]);
const loading = ref(false);
const error = ref("");
const isEditing = Boolean(route.params.id);

const form = reactive({
  title: "",
  description: "",
  status: "Todo",
  priority: "Medium",
  dueDate: "",
  categoryId: "",
});

async function load() {
  try {
    categories.value = (await categoryApi.list()).data;
    if (isEditing) {
      const task = (await taskApi.get(route.params.id)).data;

      Object.assign(form, {
        ...task,
        description: task.description || "",
        dueDate: task.dueDate?.slice(0, 10) || "",
      });
    } else if (categories.value.length) {
      form.categoryId = categories.value[0].id;
    }
  } catch (requestError) {
    error.value = getApiError(requestError, t("unableLoad"));
  }
}

async function submit() {
  error.value = "";
  if (!form.title.trim()) {
    error.value = t("titleRequired");
    return;
  }

  if (!form.categoryId) {
    error.value = t("categoryRequired");
    return;
  }

  loading.value = true;

  try {
    const payload = {
      ...form,
      title: form.title.trim(),
      description: (form.description || "").trim() || null,
      categoryId: Number(form.categoryId),
      dueDate: form.dueDate || null,
    };

    if (isEditing) {
      await taskApi.update(route.params.id, payload);
    } else {
      await taskApi.create(payload);
    }

    router.push("/tasks");
  } catch (requestError) {
    error.value = getApiError(requestError, t("unableSave"));
  } finally {
    loading.value = false;
  }
}

onMounted(load);
</script>

<template>
  <section class="page-heading">
    <div>
      <p class="eyebrow">{{ t("taskDetails") }}</p>
      <h1>{{ isEditing ? t("editTask") : t("createTask") }}</h1>
      <p class="muted">{{ t("addDetails") }}</p>
    </div>
  </section>

  <form class="card form-card" @submit.prevent="submit">
    <p v-if="error" class="alert error">{{ error }}</p>

    <label>
      {{ t("title") }}
      <input
        v-model="form.title"
        maxlength="200"
        required
        :placeholder="t('whatNeedsDone')"
      />
    </label>

    <label>
      {{ t("description") }}
      <textarea
        v-model="form.description"
        rows="4"
        :placeholder="t('usefulContext')"
      ></textarea>
    </label>

    <div class="form-grid">
      <label>
        {{ t("status") }}
        <select v-model="form.status">
          <option value="Todo">{{ t("statusTodo") }}</option>
          <option value="InProgress">{{ t("statusInProgress") }}</option>
          <option value="Done">{{ t("statusDone") }}</option>
        </select>
      </label>

      <label>
        {{ t("priority") }}
        <select v-model="form.priority">
          <option value="Low">{{ t("low") }}</option>
          <option value="Medium">{{ t("medium") }}</option>
          <option value="High">{{ t("high") }}</option>
        </select>
      </label>

      <label>
        {{ t("category") }}
        <select v-model="form.categoryId" required>
          <option value="" disabled>{{ t("selectCategory") }}</option>
          <option
            v-for="category in categories"
            :key="category.id"
            :value="category.id"
          >
            {{ category.name }}
          </option>
        </select>
      </label>

      <label>
        {{ t("dueDate") }}
        <input v-model="form.dueDate" type="date" />
      </label>
    </div>

    <div class="form-actions">
      <RouterLink class="button secondary" to="/tasks">
        {{ t("cancel") }}
      </RouterLink>
      <button class="button primary" :disabled="loading">
        {{ loading ? t("saving") : t("saveTask") }}
      </button>
    </div>
  </form>
</template>
