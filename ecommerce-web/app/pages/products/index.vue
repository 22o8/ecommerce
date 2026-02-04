<template>
  <div class="container mx-auto px-4 py-8">
    <div class="flex flex-col gap-4 sm:flex-row sm:items-end sm:justify-between">
      <div>
        <h1 class="text-3xl font-extrabold text-white">{{ t('brandsPage.title') }}</h1>
        <p class="text-white/60 mt-2">{{ t('brandsPage.subtitle') }}</p>
      </div>

      <div class="flex gap-2 w-full sm:w-auto">
        <div class="relative flex-1 sm:w-[360px]">
          <input
            v-model="q"
            :placeholder="t('brandsPage.search')"
            class="w-full rounded-xl bg-white/[0.04] border border-white/10 px-4 py-3 outline-none focus:ring-2 focus:ring-white/10"
          />
          <button
            v-if="q"
            class="absolute right-2 top-1/2 -translate-y-1/2 text-white/50 hover:text-white"
            @click="q = ''"
            aria-label="clear"
          >
            ✕
          </button>
        </div>
      </div>
    </div>

    <div class="mt-6 grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
      <BrandCard v-for="b in filtered" :key="b.slug" :b="b" />
    </div>

    <div v-if="!loading && filtered.length === 0" class="mt-10 rounded-2xl border border-white/10 bg-white/[0.03] p-10 text-center text-white/70">
      {{ t('brandsPage.empty') }}
    </div>
  </div>
</template>

<script setup lang="ts">
import BrandCard from '~/components/BrandCard.vue'

const { t } = useI18n()
const brands = useBrandsStore()

const q = ref('')

const loading = computed(() => brands.loading)

await useAsyncData('public-brands', async () => {
  await brands.fetchPublic()
  return true
})

const filtered = computed(() => {
  const qq = q.value.trim().toLowerCase()
  const list = brands.items || []
  if (!qq) return list
  return list.filter(b =>
    String(b.name || '').toLowerCase().includes(qq) ||
    String(b.slug || '').toLowerCase().includes(qq) ||
    String(b.description || '').toLowerCase().includes(qq)
  )
})
</script>
