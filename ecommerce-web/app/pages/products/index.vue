<script setup lang="ts">
import { useBrandsStore } from '@/stores/brands'
import { useI18n } from '@/composables/useI18n'

const { t } = useI18n()
const brandsStore = useBrandsStore()

await useAsyncData('public-brands', async () => {
  await brandsStore.fetchPublic()
})

// Helpers
const q = ref('')
const normalize = (s: string) => (s || '').trim().toLowerCase()

const presetNames = [
  'Anua',
  'APRILSKIN',
  'VT (VT Global)',
  'Skinfood',
  'Medicube',
  'Numbuzin',
  'K-SECRET',
  'Equal Berry',
  'SKIN1004',
  'Beauty of Joseon',
  'JMsolution',
  'Tenzero',
  'Dr.Ceuracle',
  'Rejuran',
  'Celimax',
  'Medipeel',
  'Biodance',
  'Dr.CPU',
  'Anua KR',
]

const apiBrands = computed(() => brandsStore.publicItems || [])

const optionItems = computed(() => {
  const map = new Map<string, { name: string; slug?: string }>()
  // from API
  for (const b of apiBrands.value) {
    if (!b?.name) continue
    map.set(normalize(b.name), { name: b.name, slug: b.slug })
  }
  // presets (fallback)
  for (const name of presetNames) {
    if (!map.has(normalize(name))) map.set(normalize(name), { name })
  }
  return Array.from(map.values()).sort((a, b) => a.name.localeCompare(b.name))
})

const filteredBrands = computed(() => {
  const needle = normalize(q.value)
  if (!needle) return apiBrands.value
  return apiBrands.value.filter(b =>
    normalize(b.name).includes(needle) || normalize(b.slug).includes(needle)
  )
})
</script>

<template>
  <div class="min-h-[70vh]">
    <section class="container mx-auto px-4 pt-10 pb-6">
      <div class="mb-6">
        <h1 class="text-3xl font-black tracking-tight text-[rgb(var(--text))]">
          {{ t('brands.title') }}
        </h1>
        <p class="mt-2 text-sm text-[rgba(var(--muted),0.95)]">
          {{ t('brands.subtitle') }}
        </p>
      </div>

      <!-- Brand options -->
      <div class="rounded-3xl border border-[rgba(var(--border),1)] bg-[rgba(var(--surface),0.9)] p-4">
        <div class="flex items-center gap-3 flex-wrap">
          <div class="relative flex-1 min-w-[240px]">
            <input
              v-model="q"
              class="input"
              :placeholder="t('common.search')"
              aria-label="Search brands"
            />
          </div>

          <div class="flex-1 min-w-[240px] flex flex-wrap gap-2 justify-end">
            <NuxtLink
              v-for="it in optionItems"
              :key="it.name"
              :to="it.slug ? `/brands/${it.slug}` : `/products?search=${encodeURIComponent(it.name)}`"
              class="px-4 py-2 rounded-2xl border border-[rgba(var(--border),1)] bg-[rgba(var(--surface-2),1)] text-sm font-semibold text-[rgb(var(--text))] hover:opacity-90 transition"
            >
              {{ it.name }}
            </NuxtLink>
          </div>
        </div>
      </div>
    </section>

    <section class="container mx-auto px-4 pb-14">
      <div class="grid sm:grid-cols-2 lg:grid-cols-3 gap-5 mt-6">
        <BrandCard v-for="b in filteredBrands" :key="b.id" :brand="b" />
      </div>

      <div v-if="!brandsStore.publicLoading && filteredBrands.length === 0" class="mt-10 text-center">
        <p class="text-[rgba(var(--muted),0.95)]">{{ t('home.noBrandsFound') }}</p>
      </div>
    </section>
  </div>
</template>
