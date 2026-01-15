<!-- app/pages/services/index.vue -->
<script setup lang="ts">
import { computed } from 'vue'
import { useApi } from '~/composables/useApi'
import { useI18n } from '~/composables/useI18n'
import { useSiteMeta } from '~/composables/useSiteMeta'

const api = useApi()
const { t } = useI18n()

useSiteMeta({
  title: `${t('services')} | Ecommerce`,
  description: 'Browse services',
  path: '/services',
})

function normalizeList(res: any) {
  if (Array.isArray(res)) return res
  if (res?.items && Array.isArray(res.items)) return res.items
  return []
}

const { data, pending } = await useAsyncData('services', async () => {
  return await api.get<any>('/Services')
})

const items = computed(() => normalizeList(data.value))
</script>

<template>
  <div class="grid gap-6">
    <section class="card p-8">
      <div class="flex items-center justify-between gap-4 flex-wrap">
        <div>
          <h1 class="h2">{{ t('services') }}</h1>
          <p class="mt-1 muted text-sm">Browse service packages</p>
        </div>
      </div>

      <div class="hr my-6"></div>

      <div class="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
        <template v-if="pending">
          <div v-for="i in 9" :key="i" class="soft p-5 animate-pulse">
            <div class="h-4 w-2/3 rounded bg-black/10 dark:bg-white/10"></div>
            <div class="h-3 w-1/2 rounded bg-black/10 dark:bg-white/10 mt-3"></div>
            <div class="h-10 rounded bg-black/10 dark:bg-white/10 mt-6"></div>
          </div>
        </template>
        <template v-else>
          <article v-for="s in items" :key="s.id || s.slug" class="soft p-5 hover:opacity-95 transition">
            <div class="flex items-start justify-between gap-3">
              <div>
                <div class="h3">{{ s.title || s.name || s.slug || 'Service' }}</div>
                <p class="text-sm muted mt-2 line-clamp-2">{{ s.description || '—' }}</p>
              </div>
              <div class="badge" v-if="s.priceUsd != null">
                ${{ s.priceUsd }}
              </div>
            </div>

            <div class="mt-5 flex items-center justify-between gap-2">
              <NuxtLink
                class="text-sm font-extrabold"
                :style="{ color: 'rgb(var(--primary))' }"
                :to="`/services/${s.slug || s.id}`"
              >
                {{ t('serviceDetails') }} →
              </NuxtLink>

              <NuxtLink :to="`/services/${s.slug || s.id}`">
                <AppButton variant="soft">{{ t('requestService') }}</AppButton>
              </NuxtLink>
            </div>
          </article>

          <div v-if="!items.length" class="soft p-6 sm:col-span-2 lg:col-span-3">
            <div class="font-extrabold">No services yet</div>
            <div class="text-sm muted mt-1">أضف خدمات من لوحة الأدمن (Admin Services).</div>
          </div>
        </template>
      </div>
    </section>
  </div>
</template>
