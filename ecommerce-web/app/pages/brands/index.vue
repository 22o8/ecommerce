<template>
  <div class="container mx-auto px-4 py-10 brands-page">
    <div class="flex flex-col sm:flex-row sm:items-end sm:justify-between gap-4">
      <div>
        <h1 class="text-3xl sm:text-4xl font-extrabold rtl-text">{{ t('home.brands') }}</h1>
        <p class="mt-2 text-muted rtl-text">{{ t('home.brandsSubtitle') }}</p>
      </div>

      <div class="brands-search-box rounded-[28px] p-2 w-full sm:w-[380px]">
        <div class="relative">
          <input
            v-model="q"
            :placeholder="t('brandPage.search')"
            class="input"
          />
          <button
            v-if="q"
            class="absolute left-3 top-1/2 -translate-y-1/2 icon-btn"
            @click="q = ''"
            :aria-label="t('common.clear')"
          >
            ✕
          </button>
        </div>
      </div>
    </div>

    <div class="mt-6">
      <div v-if="pending" class="card-soft p-6 text-muted rtl-text">{{ t('common.loading') }}</div>

      <div v-else-if="filtered.length === 0" class="card-soft p-10 text-center text-muted rtl-text">
        {{ t('home.noBrandsFound') }}
      </div>

      <div v-else class="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-5 gap-4">
        <RevealOnScroll
          v-for="(b, idx) in filtered"
          :key="b.id || b.slug"
          :parity="(idx % 2) as 0 | 1"
          :delay="30 * (idx % 10)"
        >
          <NuxtLink
            :to="`/brands/${b.slug}`"
            class="group brand-grid-card rounded-[28px] p-4"
          >
          <div class="flex items-start gap-3">
            <div
              class="w-14 h-14 rounded-2xl border border-app bg-surface-2 overflow-hidden grid place-items-center flex-shrink-0"
            >
              <SmartImage
                v-if="b.logoUrl"
                :src="buildAssetUrl(b.logoUrl)"
                :alt="b.name"
                class="w-full h-full object-cover transition duration-500 group-hover:scale-110"
              />
              <div v-else class="text-xs text-muted">{{ t('brandPage.logoFallback') }}</div>
            </div>

            <div class="min-w-0">
              <div class="font-extrabold line-clamp-1 rtl-text group-hover:opacity-95">{{ b.name }}</div>
              <div class="mt-1 text-xs text-muted line-clamp-2 rtl-text">
                {{ b.description || t('brands.noDescription') }}
              </div>
            </div>
          </div>
          </NuxtLink>
        </RevealOnScroll>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import SmartImage from '~/components/SmartImage.vue'

const { t } = useI18n()
const brandsStore = useBrandsStore()
const { buildAssetUrl } = useApi()

const q = ref('')

const { pending } = await useAsyncData('brands_page', async () => {
  await brandsStore.fetchPublic()
  return true
})

const list = computed(() => brandsStore.publicItems || brandsStore.items || [])

const filtered = computed(() => {
  const s = q.value.trim().toLowerCase()
  if (!s) return list.value
  return (list.value || []).filter((b: any) => {
    const name = String(b?.name || '').toLowerCase()
    const slug = String(b?.slug || '').toLowerCase()
    const desc = String(b?.description || '').toLowerCase()
    return name.includes(s) || slug.includes(s) || desc.includes(s)
  })
})
</script>


<style scoped>
.brands-search-box{
  border: 1px solid rgba(var(--border), .95);
  background: rgb(var(--surface));
}
.brand-grid-card{
  display:block;
  border: 1px solid rgba(var(--border), .95);
  transition: transform .22s ease, border-color .22s ease, box-shadow .22s ease, background-color .22s ease;
}
.brand-grid-card:hover{
  transform: translateY(-4px);
  border-color: rgba(var(--primary), .30);
}
:global(html.theme-light) .brands-search-box{
  background: linear-gradient(180deg, rgba(255,255,255,.98), rgba(255,247,252,.96));
  box-shadow: 0 24px 56px rgba(232, 91, 154, .10), 0 10px 28px rgba(24,24,24,.05);
}
:global(html.theme-light) .brand-grid-card{
  background: linear-gradient(180deg, rgba(255,255,255,.98), rgba(255,247,252,.94));
  box-shadow: 0 22px 54px rgba(232, 91, 154, .08), 0 10px 24px rgba(24,24,24,.05);
}
:global(html.theme-light) .brand-grid-card:hover{
  box-shadow: 0 28px 64px rgba(232, 91, 154, .12), 0 12px 28px rgba(24,24,24,.06);
}
:global(html.theme-dark) .brands-search-box,
:global(html.theme-dark) .brand-grid-card{
  background: linear-gradient(180deg, rgba(var(--surface-rgb), .98), rgba(var(--surface-2-rgb), .88));
  box-shadow: 0 18px 46px rgba(0,0,0,.26);
}
</style>
