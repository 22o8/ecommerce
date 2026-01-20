<!-- app/pages/404.vue -->
<template>
  <div class="mx-auto max-w-2xl">
    <EmptyState :title="t('notFound')" :desc="t('requestFailed')">
      <NuxtLink to="/">
        <AppButton type="button">{{ t('backHome') }}</AppButton>
      </NuxtLink>
    </EmptyState>
  </div>
</template>

<script setup lang="ts">
import { useI18n } from '~/composables/useI18n'
const { t } = useI18n()
</script>
