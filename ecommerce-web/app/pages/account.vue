<!-- app/pages/account.vue -->
<template>
  <div class="mx-auto max-w-3xl space-y-6">
    <div class="rounded-3xl border border-white/10 bg-white/5 p-6">
      <h1 class="text-2xl font-black">{{ t('account') }}</h1>
      <p class="mt-2 text-sm text-white/70">
        {{ auth.user ? JSON.stringify(auth.user) : '—' }}
      </p>

      <div class="mt-6">
        <AppButton type="button" variant="soft" @click="auth.logout()">
          {{ t('logout') }}
        </AppButton>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { useAuthStore } from '~/stores/auth'
import { useI18n } from '~/composables/useI18n'
import { useSiteMeta } from '~/composables/useSiteMeta'

const auth = useAuthStore()
const { t } = useI18n()

useSiteMeta({
  title: `${t('account')} | Ecommerce`,
  description: 'Your account dashboard.',
  path: '/account',
})
</script>
