<script setup lang="ts">
import { storeToRefs } from 'pinia'
import { useFavoritesStore } from '~/stores/favorites'
import { useAuthStore } from '~/stores/auth'

const { t } = useI18n()

const auth = useAuthStore()
const fav = useFavoritesStore()
const { items, loading } = storeToRefs(fav)

const count = computed(() => items.value.length)

onMounted(async () => {
  if (auth.token) await fav.load()
})

watch(
  () => auth.token,
  async (v) => {
    if (v) await fav.load()
    else items.value = []
  }
)
</script>

<template>
  <div class="max-w-6xl mx-auto px-4 py-10 favorites-page">
    <div class="flex items-start justify-between gap-4">
      <div>
        <h1 class="text-3xl font-bold">{{ t('favorites.title') }}</h1>
        <p class="text-sm opacity-80 mt-1">{{ t('favorites.subtitle') }}</p>
      </div>
      <div class="text-sm opacity-80 mt-2">
        {{ t('favorites.itemsCount', { count }) }}
      </div>
    </div>

    <div v-if="!auth.token" class="mt-8 favorites-panel p-8">
      <p class="text-lg font-semibold">{{ t('loginToCheckout') }}</p>
      <p class="text-sm opacity-80 mt-1">{{ t('favorites.subtitle') }}</p>
      <NuxtLink to="/login" class="inline-flex mt-5 btn-cta-animated rounded-full px-5 py-2.5 text-sm font-semibold">
        {{ t('loginTitle') }}
      </NuxtLink>
    </div>

    <div v-else class="mt-8">
      <div v-if="loading" class="text-sm opacity-80">{{ t('loading') }}...</div>

      <div v-else-if="count === 0" class="favorites-panel p-10 text-center">
        <h3 class="text-xl font-bold">{{ t('favorites.emptyTitle') }}</h3>
        <p class="text-sm opacity-80 mt-2">{{ t('favorites.emptyHint') }}</p>
        <NuxtLink to="/products" class="inline-flex mt-6 btn-cta-animated rounded-full px-6 py-3 text-sm font-semibold">
          {{ t('favorites.browseProducts') }}
        </NuxtLink>
      </div>

      <div v-else class="grid gap-6 grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
        <ProductCard v-for="p in items" :key="p.id" :product="p" />
      </div>
    </div>
  </div>
</template>


<style scoped>
.favorites-page{ min-height: calc(100vh - 120px); }
.favorites-panel{
  border-radius: 28px;
  border: 1px solid rgba(var(--border), .95);
  background: rgb(var(--surface));
  box-shadow: 0 18px 46px rgba(0,0,0,.08);
}
:global(html.theme-light) .favorites-panel{
  background: linear-gradient(180deg, rgba(255,255,255,.98), rgba(255,247,252,.94));
  box-shadow: 0 24px 60px rgba(232, 91, 154, .10), 0 10px 28px rgba(24,24,24,.05);
}
:global(html.theme-dark) .favorites-panel{
  background: linear-gradient(180deg, rgba(var(--surface-rgb), .98), rgba(var(--surface-2-rgb), .88));
  box-shadow: 0 18px 50px rgba(0,0,0,.28);
}
</style>
