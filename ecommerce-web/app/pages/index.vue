<!-- app/pages/index.vue -->
<script setup lang="ts">
import { computed } from 'vue'
import { useApi } from '~/composables/useApi'
import { useAuthStore } from '~/stores/auth'
import { useI18n } from '~/composables/useI18n'

const { t } = useI18n()
const auth = useAuthStore()
const api = useApi()

const { data, pending } = await useAsyncData('home-products', async () => {
  // أول 6 منتجات
  return await api.get<any>('/Products', { Page: 1, PageSize: 6 })
})

const items = computed(() => data.value?.items || [])
</script>

<template>
  <div class="grid gap-10">
    <!-- HERO -->
    <section class="grid gap-6 lg:grid-cols-2 items-center">
      <div class="card-strong p-8">
        <div class="badge mb-4">
          <span>⚡</span>
          <span>{{ t('home.badge') }}</span>
        </div>

        <h1 class="h1">{{ t('home.title') }}</h1>

        <p class="mt-4 muted" style="max-width: 52ch;">{{ t('home.subtitle') }}</p>

        <div class="mt-6 flex gap-3 flex-wrap">
          <NuxtLink to="/products">
            <AppButton variant="primary">{{ t('browseProducts') }}</AppButton>
          </NuxtLink>

          <NuxtLink v-if="!auth.isAuthed" to="/login">
            <AppButton variant="soft">{{ t('login') }}</AppButton>
          </NuxtLink>
          <NuxtLink v-else to="/account">
            <AppButton variant="soft">{{ t('home.goToAccount') }}</AppButton>
          </NuxtLink>
        </div>

        <div class="mt-7 grid gap-3 sm:grid-cols-3">
          <div class="soft p-4">
            <div class="text-sm font-extrabold">✅ {{ t('home.features.instant.title') }}</div>
            <div class="text-xs muted mt-1">{{ t('home.features.instant.desc') }}</div>
          </div>
          <div class="soft p-4">
            <div class="text-sm font-extrabold">🔒 {{ t('home.features.secure') }}</div>
            <div class="text-xs muted mt-1">{{ t('home.features.secureDesc') }}</div>
          </div>
          <div class="soft p-4">
            <div class="text-sm font-extrabold">{{ t('home.features.support') }}</div>
            <div class="text-xs muted mt-1">{{ t('home.features.supportDesc') }}</div>
          </div>
        </div>
      </div>

      <!-- RIGHT CARD -->
      <div class="card p-8">
        <h2 class="h2">{{ t('home.whyTitle') }}</h2>
        <p class="mt-2 muted">{{ t('home.whySubtitle') }}</p>

        <div class="mt-5 grid gap-3">
          <div class="soft p-4 flex items-start gap-3">
            <div class="badge">✅</div>
            <div>
              <div class="font-extrabold leading-snug break-words">{{ t('home.why.aTitle') }}</div>
              <div class="text-sm muted mt-1 leading-relaxed break-words clamp-2">{{ t('home.why.aDesc') }}</div>
            </div>
          </div>

          <div class="soft p-4 flex items-start gap-3">
            <div class="badge">✅</div>
            <div>
              <div class="font-extrabold leading-snug break-words">{{ t('home.why.bTitle') }}</div>
              <div class="text-sm muted mt-1 leading-relaxed break-words clamp-2">{{ t('home.why.bDesc') }}</div>
            </div>
          </div>

          <div class="soft p-4 flex items-start gap-3">
            <div class="badge">✅</div>
            <div>
              <div class="font-extrabold leading-snug break-words">{{ t('home.why.cTitle') }}</div>
              <div class="text-sm muted mt-1 leading-relaxed break-words clamp-2">{{ t('home.why.cDesc') }}</div>
            </div>
          </div>
        </div>
      </div>
    </section>

    <!-- FEATURED PRODUCTS -->
    <section class="card p-8">
      <div class="flex items-center justify-between gap-3 flex-wrap">
        <div>
          <h2 class="h2">{{ t('home.featured.title') }}</h2>
          <div class="text-sm muted mt-1 leading-relaxed break-words clamp-2">{{ t('home.featured.subtitle') }}</div>
        </div>

        <NuxtLink to="/products" class="text-sm font-extrabold" :style="{ color: 'rgb(var(--primary))' }">
          {{ t('browseProducts') }}
        </NuxtLink>
      </div>

      <div class="mt-6 grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
        <template v-if="pending">
          <div v-for="i in 6" :key="i" class="soft p-5 animate-pulse">
            <div class="h-4 w-2/3 rounded bg-black/10 dark:bg-white/10"></div>
            <div class="h-3 w-1/2 rounded bg-black/10 dark:bg-white/10 mt-3"></div>
            <div class="h-10 rounded bg-black/10 dark:bg-white/10 mt-6"></div>
          </div>
        </template>
        <template v-else>
          <ProductCard v-for="p in items" :key="p.id" :item="p" />
          <div v-if="!items.length" class="soft p-6 sm:col-span-2 lg:col-span-3">
            <div class="font-extrabold leading-snug break-words">{{ t('empty.noProductsTitle') }}</div>
            <div class="text-sm muted mt-1 leading-relaxed break-words clamp-2">{{ t('empty.noProductsDesc') }}</div>
          </div>
        </template>
      </div>
    </section>
  </div>
</template>


<style scoped>
.clamp-2{
  display:-webkit-box;
  -webkit-line-clamp:2;
  -webkit-box-orient:vertical;
  overflow:hidden;
}
</style>
