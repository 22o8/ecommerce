<script setup lang="ts">
import { computed } from 'vue'
import { useAsyncData } from '#app'
import { useBrandsStore } from '~/stores/brands'
import { useProductsStore } from '~/stores/products'

const { t, locale } = useI18n()

const brandsStore = useBrandsStore()
const productsStore = useProductsStore()

await useAsyncData(
  'home-prefetch',
  async () => {
    await Promise.allSettled([
      brandsStore.fetchPublic(),
      productsStore.fetchFeatured(8),
      productsStore.fetchDiscounts(8),
      productsStore.fetch({ page: 1, pageSize: 8, sort: 'newest' }),
    ])
    return true
  },
  {
    server: false,
    default: () => true,
  }
)

const featured = computed(() => productsStore.featured)
const safeFeatured = computed(() => (featured.value ?? []).filter((p) => !!p && !!p.id))
const fallbackLatest = computed(() => productsStore.items?.slice(0, 8) ?? [])
const homeFeatured = computed(() => {
  if (safeFeatured.value.length) return safeFeatured.value
  return (fallbackLatest.value ?? []).filter((p) => !!p && !!p.id && !!p.isPublished).slice(0, 8)
})
const featuredList = homeFeatured

const tab = ref<'featured' | 'discounts'>('featured')
const displayedFeatured = computed(() => tab.value === 'featured'
  ? homeFeatured.value
  : (productsStore.discountItems ?? []).slice(0, 8)
)

const brands = computed(() => brandsStore.publicItems)
const topBrands = computed(() => {
  const seen = new Set<string>()
  const uniq = [] as typeof brands.value
  for (const b of (brands.value ?? [])) {
    if (!b) continue
    const key = (b.name ?? '').trim().toLowerCase()
    if (!key || seen.has(key)) continue
    seen.add(key)
    uniq.push(b)
  }
  return uniq.slice(0, 10)
})
const categoryCards = [
  { key: 'serum', icon: '💧', labelKey: 'home.catSerum', q: { ar: 'سيروم', en: 'serum' } },
  { key: 'moisturizer', icon: '🧴', labelKey: 'home.catMoisturizer', q: { ar: 'مرطب', en: 'moisturizer' } },
  { key: 'sunscreen', icon: '☀️', labelKey: 'home.catSunscreen', q: { ar: 'واقي شمس', en: 'sunscreen' } },
  { key: 'cleanser', icon: '🫧', labelKey: 'home.catCleanser', q: { ar: 'غسول', en: 'cleanser' } },
  { key: 'eye-care', icon: '👁️', labelKey: 'home.catEyeCare', q: { ar: 'العناية بالعين', en: 'eye care' } },
  { key: 'perfume', icon: '🌸', labelKey: 'home.catPerfume', q: { ar: 'عطر', en: 'perfume' } },
] as const

const categoryQuery = (c: (typeof categoryCards)[number]) => (locale.value === 'ar' ? c.q.ar : c.q.en)
const heroHighlights = computed(() => categoryCards.slice(0, 3))
</script>

<template>
  <div class="min-h-screen home-page-shell">
    <section class="relative mx-auto max-w-6xl px-4 pt-4 sm:pt-6">
      <div class="hero-premium-shell hero-shimmer overflow-hidden rounded-[2rem] border border-app">
        <div class="hero-aurora hero-aurora--one" />
        <div class="hero-aurora hero-aurora--two" />
        <div class="hero-aurora hero-aurora--three" />

        <div class="relative z-[1] mx-auto max-w-5xl px-5 py-16 sm:px-8 sm:py-20 lg:py-24">
          <div class="mx-auto max-w-4xl text-center">
            <div class="hero-mini-badges mb-6 flex flex-wrap items-center justify-center gap-3">
              <NuxtLink
                v-for="item in heroHighlights"
                :key="item.key"
                :to="`/products?category=${encodeURIComponent(item.key)}`"
                class="hero-mini-chip"
              >
                <span class="text-base">{{ item.icon }}</span>
                <span>{{ t(item.labelKey) }}</span>
              </NuxtLink>
            </div>

            <h1 class="text-4xl font-extrabold tracking-tight text-[rgb(var(--text))] sm:text-6xl xl:text-7xl">
              {{ t('homeHero.title1') }}
              <span class="hero-title-accent">{{ t('homeHero.title2') }}</span>
            </h1>

            <p class="mx-auto mt-6 max-w-2xl text-base leading-8 text-[rgb(var(--muted))] sm:text-lg">
              {{ t('homeHero.subtitle') }}
            </p>

            <div class="mt-8 flex flex-wrap items-center justify-center gap-3 sm:gap-4">
              <NuxtLink
                to="/products"
                class="btn-cta-animated inline-flex items-center justify-center rounded-full px-7 py-3.5 text-sm font-semibold hover:opacity-95"
              >
                {{ t('homeHero.products') }}
              </NuxtLink>

              <a
                href="#categories"
                class="btn-cta-animated btn-cta-secondary inline-flex items-center justify-center rounded-full px-7 py-3.5 text-sm font-semibold hover:opacity-95"
              >
                {{ t('homeHero.categories') }}
              </a>
            </div>

            <div class="hero-stat-grid mt-10 grid gap-3 sm:grid-cols-3">
              <div class="hero-stat-card">
                <div class="hero-stat-card__glow" />
                <div class="hero-stat-card__label">{{ t('homeHero.featuredProducts') }}</div>
                <div class="hero-stat-card__value">{{ featuredList.length }}</div>
              </div>
              <div class="hero-stat-card">
                <div class="hero-stat-card__glow" />
                <div class="hero-stat-card__label">{{ t('home.brands') }}</div>
                <div class="hero-stat-card__value">{{ topBrands.length }}</div>
              </div>
              <div class="hero-stat-card">
                <div class="hero-stat-card__glow" />
                <div class="hero-stat-card__label">{{ t('homeHero.categories') }}</div>
                <div class="hero-stat-card__value">{{ categoryCards.length }}</div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </section>

    <section class="mx-auto max-w-6xl px-4 pb-16 pt-12 sm:pt-14">
      <div class="home-section-panel">
        <div class="flex flex-col items-center justify-center gap-4 text-center">
          <div class="section-kicker" />
          <h2 class="text-2xl font-extrabold text-[rgb(var(--text))] sm:text-4xl">{{ t('homeHero.featuredProducts') }}</h2>

          <div class="inline-flex items-center rounded-full border border-app bg-surface p-1 shadow-soft">
            <button
              type="button"
              class="px-4 py-2 rounded-full text-sm font-bold transition"
              :class="tab === 'featured' ? 'bg-[rgb(var(--primary))] text-black shadow-[0_10px_24px_rgba(var(--primary),0.25)]' : 'text-[rgb(var(--text))] hover:bg-surface-2'"
              @click="tab = 'featured'"
            >
              {{ t('home.featuredTab') }}
            </button>
            <button
              type="button"
              class="px-4 py-2 rounded-full text-sm font-bold transition"
              :class="tab === 'discounts' ? 'bg-[rgb(var(--primary))] text-black shadow-[0_10px_24px_rgba(var(--primary),0.25)]' : 'text-[rgb(var(--text))] hover:bg-surface-2'"
              @click="tab = 'discounts'"
            >
              {{ t('home.discountsTab') }}
            </button>
            <NuxtLink to="/products" class="px-4 py-2 rounded-full text-sm font-bold text-[rgb(var(--text))] hover:bg-surface-2 transition">
              {{ t('home.viewAll') }}
            </NuxtLink>
          </div>
        </div>

        <div class="mt-10 grid gap-5 sm:grid-cols-2 lg:grid-cols-4">
          <RevealOnScroll
            v-for="(p, idx) in displayedFeatured"
            :key="p.id"
            :parity="idx % 2"
          >
            <ProductCard :p="p" />
          </RevealOnScroll>
        </div>
      </div>
    </section>

    <section class="mx-auto max-w-6xl px-4 pb-20">
      <div class="home-section-panel home-section-panel--brands">
        <div class="flex flex-col items-start justify-between gap-4 sm:flex-row sm:items-end">
          <div>
            <h2 class="text-2xl font-extrabold tracking-tight text-[rgb(var(--text))] sm:text-4xl">{{ t('home.brands') }}</h2>
            <p class="mt-2 max-w-2xl text-sm text-[rgb(var(--muted))] sm:text-base">{{ t('home.brandsSubtitle') }}</p>
          </div>
          <NuxtLink
            to="/brands"
            class="btn inline-flex items-center gap-2 rounded-full px-4 py-2 text-sm font-semibold shadow-soft"
          >
            {{ t('nav.brands') }}
            <span aria-hidden="true">→</span>
          </NuxtLink>
        </div>

        <BrandMarquee :brands="topBrands" />
      </div>
    </section>

    <section id="categories" class="mx-auto max-w-6xl px-4 pb-24 scroll-mt-24">
      <div class="home-section-panel home-section-panel--categories">
        <div class="flex flex-col items-start justify-between gap-4 sm:flex-row sm:items-end">
          <div>
            <h2 class="text-2xl font-extrabold tracking-tight text-[rgb(var(--text))] sm:text-4xl">
              {{ t('home.spotlightTitle') }}
            </h2>
            <p class="mt-2 max-w-2xl text-sm text-[rgb(var(--muted))] sm:text-base">
              {{ t('home.spotlightSubtitle') }}
            </p>
          </div>

          <NuxtLink to="/products" class="btn inline-flex items-center rounded-full px-4 py-2 text-sm font-semibold shadow-soft">
            {{ t('home.viewAll') }}
          </NuxtLink>
        </div>

        <div class="mt-6 grid gap-3 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-5">
          <RevealOnScroll
            v-for="(c, idx) in categoryCards"
            :key="c.key"
            :parity="(idx % 2) as 0 | 1"
            :delay="35 * idx"
          >
            <NuxtLink
              :to="`/products?category=${encodeURIComponent(c.key)}`"
              class="group category-simple-card"
            >
              <div class="category-simple-card__inner">
                <div class="category-simple-card__icon">{{ c.icon }}</div>
                <div class="min-w-0 flex-1">
                  <div class="truncate text-base font-black text-[rgb(var(--text))]">
                    {{ t(c.labelKey) }}
                  </div>
                  <div class="mt-1 truncate text-xs text-[rgb(var(--muted))]">
                    {{ t('home.tapToExplore') }}
                  </div>
                </div>
                <div class="category-simple-card__arrow">→</div>
              </div>
            </NuxtLink>
          </RevealOnScroll>
        </div>
      </div>
    </section>
  </div>
</template>

<style scoped>
.home-page-shell{
  position: relative;
}
.hero-premium-shell{
  position: relative;
  background:
    linear-gradient(180deg, rgba(var(--surface-rgb), .94), rgba(var(--surface-rgb), .82)),
    linear-gradient(135deg, rgba(var(--primary), .10), transparent 32%, rgba(var(--cta-glow-2), .08) 72%, transparent 100%);
  box-shadow: 0 34px 90px rgba(10, 10, 20, .10);
}
.hero-premium-shell::after{
  content: '';
  position: absolute;
  inset: 1px;
  border-radius: calc(2rem - 1px);
  border: 1px solid rgba(255,255,255,.18);
  pointer-events: none;
}
.hero-title-accent{
  display: inline-block;
  margin-inline-start: .35rem;
  color: rgb(var(--primary));
  text-shadow: 0 12px 34px rgba(var(--primary), .24);
}
.hero-aurora{
  position: absolute;
  border-radius: 999px;
  filter: blur(18px);
  opacity: .9;
  pointer-events: none;
}
.hero-aurora--one{
  width: 300px;
  height: 300px;
  top: -80px;
  inset-inline-start: -50px;
  background: radial-gradient(circle, rgba(var(--primary), .22), transparent 70%);
}
.hero-aurora--two{
  width: 340px;
  height: 340px;
  top: 15%;
  inset-inline-end: -60px;
  background: radial-gradient(circle, rgba(var(--cta-glow-2), .20), transparent 70%);
}
.hero-aurora--three{
  width: 360px;
  height: 180px;
  bottom: -50px;
  left: 50%;
  transform: translateX(-50%);
  background: radial-gradient(circle, rgba(var(--primary), .14), transparent 72%);
}
.hero-mini-chip{
  display: inline-flex;
  align-items: center;
  gap: .5rem;
  min-height: 42px;
  padding: .7rem 1rem;
  border-radius: 999px;
  border: 1px solid rgba(var(--border), .9);
  background: rgba(var(--surface-rgb), .72);
  box-shadow: 0 16px 36px rgba(0,0,0,.08);
  backdrop-filter: blur(10px);
  color: rgb(var(--text));
  font-size: .9rem;
  font-weight: 700;
  transition: transform .2s ease, border-color .2s ease, box-shadow .2s ease;
}
.hero-mini-chip:hover{
  transform: translateY(-2px);
  border-color: rgba(var(--primary), .42);
  box-shadow: 0 20px 42px rgba(var(--primary), .12);
}
.hero-stat-grid{
  align-items: stretch;
}
.hero-stat-card{
  position: relative;
  overflow: hidden;
  border-radius: 24px;
  border: 1px solid rgba(var(--border), .92);
  background: linear-gradient(180deg, rgba(var(--surface-rgb), .88), rgba(var(--surface-2-rgb), .82));
  padding: 1rem 1.1rem;
  box-shadow: 0 18px 44px rgba(0,0,0,.10);
}
.hero-stat-card__glow{
  position: absolute;
  inset-inline-start: 12px;
  top: -18px;
  width: 84px;
  height: 84px;
  border-radius: 999px;
  background: radial-gradient(circle, rgba(var(--primary), .22), transparent 72%);
  pointer-events: none;
}
.hero-stat-card__label{
  position: relative;
  z-index: 1;
  font-size: .82rem;
  color: rgb(var(--muted));
  font-weight: 700;
}
.hero-stat-card__value{
  position: relative;
  z-index: 1;
  margin-top: .45rem;
  font-size: 1.9rem;
  line-height: 1;
  font-weight: 900;
  color: rgb(var(--text));
}
.home-section-panel{
  position: relative;
  overflow: hidden;
  border-radius: 30px;
  border: 1px solid rgba(var(--border), .92);
  background:
    linear-gradient(180deg, rgba(var(--surface-rgb), .94), rgba(var(--surface-rgb), .86)),
    linear-gradient(135deg, rgba(var(--primary), .05), transparent 35%, rgba(var(--cta-glow-2), .05) 100%);
  padding: 1.5rem;
  box-shadow: 0 24px 70px rgba(15, 15, 25, .08);
}
.home-section-panel::before{
  content: '';
  position: absolute;
  inset: 0;
  pointer-events: none;
  background: radial-gradient(420px 160px at 10% 0%, rgba(var(--primary), .08), transparent 65%);
}
.home-section-panel--brands::before{
  background: radial-gradient(420px 160px at 85% 0%, rgba(var(--primary), .10), transparent 65%);
}
.home-section-panel--categories::before{
  background: radial-gradient(420px 160px at 50% 0%, rgba(var(--primary), .09), transparent 65%);
}
.section-kicker{
  width: 88px;
  height: 6px;
  border-radius: 999px;
  background: linear-gradient(90deg, rgba(var(--primary), .25), rgba(var(--primary), .88), rgba(var(--cta-glow-2), .35));
  box-shadow: 0 8px 24px rgba(var(--primary), .25);
}
.shadow-soft{
  box-shadow: 0 16px 38px rgba(0,0,0,.08);
}
.category-simple-card{
  display:block;
  border-radius: 24px;
  border: 1px solid rgba(var(--border), .95);
  transition: transform .22s ease, border-color .22s ease, box-shadow .22s ease, background-color .22s ease;
}
.category-simple-card__inner{
  display:flex;
  align-items:center;
  gap:14px;
  min-height: 104px;
  padding: 18px;
}
.category-simple-card__icon{
  display:grid;
  place-items:center;
  width: 52px;
  height: 52px;
  border-radius: 18px;
  background: linear-gradient(180deg, rgba(var(--primary), .16), rgba(var(--primary), .08));
  box-shadow: inset 0 1px 0 rgba(255,255,255,.45);
  font-size: 24px;
  flex: 0 0 auto;
}
.category-simple-card__arrow{
  display:grid;
  place-items:center;
  width: 36px;
  height: 36px;
  border-radius: 999px;
  border: 1px solid rgba(var(--border), .95);
  color: rgb(var(--muted));
  background: rgba(var(--surface-rgb), .7);
  transition: transform .22s ease, color .22s ease, border-color .22s ease, background-color .22s ease;
}
.category-simple-card:hover{
  transform: translateY(-3px);
  border-color: rgba(var(--primary), .34);
}
.category-simple-card:hover .category-simple-card__arrow{
  color: rgb(var(--primary));
  border-color: rgba(var(--primary), .28);
  background: rgba(var(--primary), .08);
  transform: translateX(2px);
}
:global(html.theme-light) .hero-premium-shell{
  background:
    linear-gradient(180deg, rgba(255,255,255,.97), rgba(255,246,251,.96)),
    linear-gradient(135deg, rgba(232, 91, 154, .10), transparent 35%, rgba(246, 180, 212, .12) 100%);
  box-shadow: 0 36px 90px rgba(232, 91, 154, .10), 0 14px 30px rgba(22,22,22,.05);
}
:global(html.theme-light) .hero-mini-chip,
:global(html.theme-light) .hero-stat-card,
:global(html.theme-light) .home-section-panel,
:global(html.theme-light) .category-simple-card{
  box-shadow: 0 18px 44px rgba(232, 91, 154, .08), 0 10px 26px rgba(24,24,24,.05);
}
:global(html.theme-light) .home-section-panel{
  background:
    linear-gradient(180deg, rgba(255,255,255,.98), rgba(255,247,252,.95)),
    linear-gradient(135deg, rgba(232,91,154,.05), transparent 35%, rgba(246,180,212,.06) 100%);
}
:global(html.theme-light) .category-simple-card{
  background: linear-gradient(180deg, rgba(255,255,255,.98), rgba(255,247,252,.94));
}
:global(html.theme-light) .category-simple-card:hover{
  background: linear-gradient(180deg, rgba(255,255,255,1), rgba(255,244,250,.98));
  box-shadow: 0 24px 56px rgba(232, 91, 154, .12), 0 12px 28px rgba(24,24,24,.06);
}
:global(html.theme-dark) .hero-premium-shell{
  background:
    linear-gradient(180deg, rgba(var(--surface-rgb), .96), rgba(var(--surface-2-rgb), .86)),
    linear-gradient(135deg, rgba(var(--primary), .08), transparent 35%, rgba(var(--cta-glow-2), .08) 100%);
  box-shadow: 0 34px 84px rgba(0,0,0,.28);
}
:global(html.theme-dark) .hero-mini-chip,
:global(html.theme-dark) .hero-stat-card,
:global(html.theme-dark) .home-section-panel,
:global(html.theme-dark) .category-simple-card{
  box-shadow: 0 18px 44px rgba(0,0,0,.24);
}
:global(html.theme-dark) .category-simple-card{
  background: linear-gradient(180deg, rgba(var(--surface-rgb), .98), rgba(var(--surface-2-rgb), .86));
}
@media (max-width: 640px){
  .hero-premium-shell{ border-radius: 26px; }
  .hero-stat-card__value{ font-size: 1.55rem; }
  .home-section-panel{ padding: 1rem; border-radius: 24px; }
  .category-simple-card__inner{ min-height: 94px; padding: 16px; }
  .category-simple-card__icon{ width: 46px; height: 46px; border-radius: 16px; font-size: 22px; }
  .category-simple-card__arrow{ width: 32px; height: 32px; }
}
</style>
