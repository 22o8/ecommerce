<template>
  <div class="brands-page container mx-auto px-4 py-10">
    <section class="brands-hero card-soft overflow-hidden p-6 sm:p-8">
      <div class="grid gap-6 lg:grid-cols-[1.1fr_.9fr] lg:items-end">
        <div>
          <div class="inline-flex items-center gap-2 rounded-full border border-app bg-surface px-3 py-1 text-xs font-bold text-[rgb(var(--muted))]">
            <span class="h-2 w-2 rounded-full bg-[rgb(var(--primary))]" />
            {{ t('home.brands') }}
          </div>
          <h1 class="mt-4 text-3xl sm:text-5xl font-extrabold tracking-tight rtl-text text-[rgb(var(--text))]">{{ t('home.brands') }}</h1>
          <p class="mt-3 max-w-2xl text-sm leading-7 text-[rgb(var(--muted))] sm:text-base rtl-text">{{ t('home.brandsSubtitle') }}</p>
        </div>

        <div class="brands-search-panel card-soft p-4 sm:p-5">
          <label class="block text-sm font-bold text-[rgb(var(--text))] rtl-text mb-2">{{ t('brandPage.search') }}</label>
          <div class="relative">
            <input v-model="q" :placeholder="t('brandPage.search')" class="input brands-search-input" />
            <button v-if="q" class="absolute left-3 top-1/2 -translate-y-1/2 text-sm text-[rgb(var(--muted))] hover:text-[rgb(var(--text))]" @click="q=''" :aria-label="t('common.clear')">✕</button>
          </div>
          <div class="mt-3 text-xs text-[rgb(var(--muted))] rtl-text">
            {{ t('productsPage.resultsCount', { count: filtered.length }) }}
          </div>
        </div>
      </div>
    </section>

    <section class="mt-6">
      <div v-if="pending" class="card-soft p-6 text-muted rtl-text">{{ t('common.loading') }}</div>
      <div v-else-if="filtered.length === 0" class="card-soft p-10 text-center text-muted rtl-text">{{ t('home.noBrandsFound') }}</div>
      <div v-else class="grid gap-4 sm:grid-cols-2 xl:grid-cols-3">
        <RevealOnScroll v-for="(b, idx) in filtered" :key="b.id || b.slug" :parity="(idx % 2) as 0 | 1" :delay="35 * (idx % 10)">
          <NuxtLink :to="`/brands/${b.slug}`" class="brand-list-card rounded-[30px] p-4 sm:p-5 group">
            <div class="flex items-center gap-4">
              <div class="brand-list-card__logo">
                <SmartImage v-if="b.logoUrl" :src="buildAssetUrl(b.logoUrl)" :alt="b.name" class="h-full w-full object-cover transition duration-500 group-hover:scale-110" />
                <div v-else class="text-xs text-[rgb(var(--muted))]">{{ t('brandPage.logoFallback') }}</div>
              </div>
              <div class="min-w-0 flex-1">
                <div class="text-lg font-extrabold line-clamp-1 rtl-text text-[rgb(var(--text))]">{{ b.name }}</div>
                <div class="mt-1 text-sm text-[rgb(var(--muted))] line-clamp-2 rtl-text">{{ b.description || t('brands.noDescription') }}</div>
              </div>
              <div class="brand-list-card__arrow">←</div>
            </div>
          </NuxtLink>
        </RevealOnScroll>
      </div>
    </section>
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
  return (list.value || []).filter((b:any) => {
    const name = String(b?.name || '').toLowerCase()
    const slug = String(b?.slug || '').toLowerCase()
    const desc = String(b?.description || '').toLowerCase()
    return name.includes(s) || slug.includes(s) || desc.includes(s)
  })
})
</script>

<style scoped>
.brands-hero, .brands-search-panel, .brand-list-card{
  border:1px solid rgba(var(--border), .95);
  background: rgb(var(--surface));
}
.brands-search-input{ min-height: 52px; }
.brand-list-card{
  display:block;
  transition: transform .22s ease, border-color .22s ease, box-shadow .22s ease;
}
.brand-list-card:hover{
  transform: translateY(-4px);
  border-color: rgba(var(--primary), .30);
}
.brand-list-card__logo{
  width:72px; height:72px; border-radius:22px; overflow:hidden; flex:0 0 auto;
  display:grid; place-items:center; border:1px solid rgba(var(--border), .95); background: rgb(var(--surface-2));
}
.brand-list-card__arrow{
  width:44px; height:44px; border-radius:999px; display:grid; place-items:center; flex:0 0 auto;
  border:1px solid rgba(var(--border), .95); background: rgba(var(--surface-rgb), .8); color: rgb(var(--text)); font-weight:900;
}
:global(html.theme-light) .brands-hero,
:global(html.theme-light) .brands-search-panel,
:global(html.theme-light) .brand-list-card{
  background: linear-gradient(180deg, rgba(255,255,255,.99), rgba(255,247,252,.95));
  box-shadow: 0 22px 54px rgba(232, 91, 154, .08), 0 10px 24px rgba(24,24,24,.05);
}
:global(html.theme-dark) .brands-hero,
:global(html.theme-dark) .brands-search-panel,
:global(html.theme-dark) .brand-list-card{
  background: linear-gradient(180deg, rgba(var(--surface-rgb), .98), rgba(var(--surface-2-rgb), .88));
  box-shadow: 0 18px 46px rgba(0,0,0,.26);
}
</style>
