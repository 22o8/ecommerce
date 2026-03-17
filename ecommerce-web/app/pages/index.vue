<script setup lang="ts">
import { computed } from 'vue'
import { useAsyncData } from '#app'
import { useBrandsStore } from '~/stores/brands'
import { useProductsStore } from '~/stores/products'
import heroImage from '~/assets/img/hero-brand-bg.jpg'

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
  { key: 'serum', icon: '💧', labelKey: 'home.catSerum', accent: 'from-cyan-500/25 to-indigo-500/10' },
  { key: 'moisturizer', icon: '🧴', labelKey: 'home.catMoisturizer', accent: 'from-fuchsia-500/20 to-rose-500/10' },
  { key: 'sunscreen', icon: '☀️', labelKey: 'home.catSunscreen', accent: 'from-amber-500/25 to-orange-500/10' },
  { key: 'cleanser', icon: '🫧', labelKey: 'home.catCleanser', accent: 'from-sky-500/20 to-violet-500/10' },
  { key: 'toner', icon: '🧊', labelKey: 'home.catToner', accent: 'from-blue-500/20 to-cyan-500/10' },
  { key: 'mask', icon: '✨', labelKey: 'home.catMask', accent: 'from-pink-500/20 to-violet-500/10' },
  { key: 'eye-care', icon: '👁️', labelKey: 'home.catEyeCare', accent: 'from-emerald-500/20 to-cyan-500/10' },
] as const

const heroHighlights = computed(() => categoryCards.slice(0, 4))
const heroBrandBgSrc = heroImage

</script>

<template>
  <div class="min-h-screen home-page-shell">
    <section class="relative mx-auto max-w-6xl px-4 pt-4 sm:pt-6">
      <div class="hero-premium-shell hero-shimmer overflow-hidden rounded-[2rem] border border-app">
        <div v-if="heroBrandBgSrc !== '/'" class="hero-brand-bg-wrap" aria-hidden="true">
          <img :src="heroBrandBgSrc" alt="" class="hero-brand-bg-image" />
        </div>
        <div v-else class="hero-brand-bg-placeholder" aria-hidden="true">
          <span>ضع صورة الخلفية هنا</span>
        </div>
        <div class="hero-aurora hero-aurora--one" />
        <div class="hero-aurora hero-aurora--two" />
        <div class="hero-aurora hero-aurora--three" />

        <div class="relative z-[1] mx-auto max-w-6xl px-5 py-14 sm:px-8 sm:py-18 lg:px-10 lg:py-20">
          <div class="hero-content-panel me-auto max-w-[46rem] text-center lg:text-start lg:max-w-[42rem] lg:me-[38%] xl:me-[41%]">
            <div class="hero-mini-badges mb-6 flex flex-wrap items-center justify-center gap-3">
              <NuxtLink
                v-for="item in heroHighlights"
                :key="item.key"
                :to="`/categories/${encodeURIComponent(item.key)}`"
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

            <div class="mt-8 flex flex-wrap items-center justify-center gap-3 sm:gap-4 lg:justify-start">
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

            <div class="hero-stat-grid mt-10 grid gap-3 sm:grid-cols-3 lg:max-w-3xl">
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

        <div class="category-showcase mt-8 grid gap-4 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 2xl:grid-cols-4">
          <RevealOnScroll
            v-for="(c, idx) in categoryCards"
            :key="c.key"
            :parity="(idx % 2) as 0 | 1"
            :delay="35 * idx"
          >
            <NuxtLink
              :to="`/categories/${encodeURIComponent(c.key)}`"
              class="group category-simple-card"
            >
              <div class="category-simple-card__inner" :class="`bg-gradient-to-br ${c.accent}`">
                <div class="category-simple-card__icon">{{ c.icon }}</div>
                <div class="min-w-0 flex-1">
                  <div class="truncate text-base font-black text-[rgb(var(--text))]">
                    {{ t(c.labelKey) }}
                  </div>
                  <div class="mt-1 truncate text-xs text-[rgb(var(--muted))]">
                    {{ t('home.tapToExplore') }}
                  </div>
                  <div class="category-simple-card__meta">{{ c.key }}</div>
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
.hero-content-panel{
  position: relative;
  width: min(100%, 42rem);
  padding: 1.6rem 1.4rem;
  border-radius: 30px;
  background: linear-gradient(180deg, rgba(var(--surface-rgb), .78), rgba(var(--surface-2-rgb), .64));
  border: 1px solid rgba(var(--border), .78);
  backdrop-filter: blur(12px);
  box-shadow: 0 24px 60px rgba(5, 10, 20, .16);
}
:global(html.theme-light) .hero-content-panel{
  background: linear-gradient(180deg, rgba(255,255,255,.90), rgba(249,250,252,.84));
  border-color: rgba(214, 218, 228, .95);
  box-shadow: 0 26px 60px rgba(15, 20, 35, .08);
}
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
:global(html.theme-light) .hero-mini-chip,
:global(html.theme-light) .hero-stat-card,
:global(html.theme-light) .home-section-panel,

.category-showcase{
  align-items:stretch;
}
.category-simple-card{
  position:relative;
  overflow:hidden;
  min-height:116px;
  border-radius:28px;
  border:1px solid rgba(var(--border), .92);
  background:linear-gradient(180deg, rgba(var(--surface-rgb), .84), rgba(var(--surface-rgb), .68));
  box-shadow:0 18px 40px rgba(8,10,20,.12);
  transition:transform .22s ease, border-color .22s ease, box-shadow .22s ease;
}
.category-simple-card::before{
  content:'';
  position:absolute; inset:auto -12% -55% auto;
  width:160px; height:160px;
  border-radius:999px;
  background:radial-gradient(circle, rgba(var(--primary), .18), transparent 66%);
  pointer-events:none;
}
.category-simple-card__inner{
  position:relative;
  display:flex;
  align-items:center;
  gap:1rem;
  height:100%;
  min-height:116px;
  padding:1.1rem 1rem 1.1rem 1.1rem;
}
.category-simple-card__icon{
  display:grid; place-items:center;
  width:64px; height:64px;
  border-radius:22px;
  border:1px solid rgba(255,255,255,.14);
  background:rgba(255,255,255,.12);
  box-shadow: inset 0 1px 0 rgba(255,255,255,.14);
  font-size:1.9rem;
  flex:0 0 auto;
}
.category-simple-card__meta{
  display:inline-flex;
  margin-top:.55rem;
  padding:.28rem .58rem;
  border-radius:999px;
  border:1px solid rgba(var(--border), .85);
  background:rgba(255,255,255,.06);
  color:rgb(var(--muted));
  font-size:.68rem;
  font-weight:800;
  text-transform:uppercase;
  letter-spacing:.06em;
  max-width:max-content;
}
.category-simple-card__arrow{
  display:grid; place-items:center;
  width:42px; height:42px;
  border-radius:999px;
  border:1px solid rgba(var(--border), .9);
  background:rgba(255,255,255,.08);
  font-size:1rem;
  flex:0 0 auto;
}
.category-simple-card:hover{
  transform:translateY(-4px);
  border-color:rgba(var(--primary), .34);
  box-shadow:0 26px 60px rgba(8,10,20,.18);
}
.category-simple-card:hover .category-simple-card__arrow{
  transform:translateX(-2px);
}
@media (max-width: 768px){
  .category-showcase{ grid-template-columns:1fr; }
  .category-simple-card{ min-height:102px; border-radius:24px; }
  .category-simple-card__inner{ min-height:102px; padding:1rem; gap:.85rem; }
  .category-simple-card__icon{ width:56px; height:56px; border-radius:18px; font-size:1.6rem; }
}
:global(html.theme-light) .category-simple-card{
  box-shadow: 0 18px 44px rgba(232, 91, 154, .08), 0 10px 26px rgba(24,24,24,.05);
}
:global(html.theme-light) .home-section-panel{
  background:
    linear-gradient(180deg, rgba(255,255,255,.995), rgba(255,255,255,.985)),
    linear-gradient(135deg, rgba(236,72,153,.018), transparent 42%, rgba(244,114,182,.026) 100%);
}
:global(html.theme-light) .category-simple-card{
  background: linear-gradient(180deg, rgba(255,255,255,.995), rgba(255,255,255,.982));
}
:global(html.theme-light) .category-simple-card:hover{
  background: linear-gradient(180deg, rgba(255,255,255,1), rgba(255,255,255,.99));
  box-shadow: 0 18px 40px rgba(22,22,22,.06);
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
  .hero-premium-shell{
    display:flex;
    flex-direction:column;
    gap:0;
    border-radius:26px;
    overflow:hidden;
  }
  .hero-premium-shell > .hero-brand-bg-wrap,
  .hero-premium-shell > .hero-brand-bg-placeholder{
    order:2;
    position:relative;
    inset:auto;
    display:block;
    width:100%;
    height:260px;
    min-height:260px;
    margin-top:0;
    pointer-events:none;
  }
  .hero-premium-shell > .relative.z-\[1\]{
    order:1;
    width:100%;
  }
  .hero-premium-shell > .hero-aurora{ display:none; }
  .hero-content-panel{
    width:100%;
    max-width:none;
    margin:0;
    padding:1.1rem 1rem 1rem;
    border-radius:0;
    text-align:center;
    background:transparent;
    border:0;
    box-shadow:none;
    backdrop-filter:none;
  }
  :global(html.theme-light) .hero-content-panel{
    background:transparent;
    border-color:transparent;
    box-shadow:none;
  }
  .hero-mini-badges{
    gap:.55rem;
    margin-bottom:1rem;
    justify-content:center;
  }
  .hero-mini-chip{ min-height:38px; padding:.55rem .8rem; font-size:.82rem; }
  .hero-stat-grid{
    display:grid;
    grid-template-columns:repeat(3,minmax(0,1fr));
    gap:.7rem;
    margin-top:1rem;
  }
  .hero-stat-card{
    padding:.8rem .6rem;
    border-radius:18px;
    text-align:center;
  }
  .hero-stat-card__label{
    font-size:.74rem;
    line-height:1.35;
  }
  .hero-stat-card__value{ font-size:1.55rem; }
  .home-section-panel{ padding: 1rem; border-radius: 24px; }
  .category-simple-card__inner{ min-height: 94px; padding: 16px; }
  .category-simple-card__icon{ width: 46px; height: 46px; border-radius: 16px; font-size: 22px; }
  .category-simple-card__arrow{ width: 32px; height: 32px; }
}
</style>


<style scoped>
.hero-brand-bg-wrap,
.hero-brand-bg-placeholder{
  position:absolute;
  inset:0;
  pointer-events:none;
  z-index:0;
}
.hero-brand-bg-image{
  position:absolute;
  inset-inline-end: clamp(.75rem, 2vw, 1.35rem);
  top:50%;
  width:min(37vw, 470px);
  max-width:37%;
  transform:translateY(-50%);
  object-fit:cover;
  object-position:center;
  border-radius: 28px;
  opacity:.86;
  filter: grayscale(1) brightness(.96) contrast(1.06) drop-shadow(0 22px 68px rgba(0,0,0,.18));
}
.hero-brand-bg-placeholder{
  display:flex;
  align-items:center;
  justify-content:flex-end;
  padding-inline:2rem;
}
.hero-brand-bg-placeholder span{
  display:inline-flex;
  align-items:center;
  justify-content:center;
  min-width:220px;
  width:min(42vw, 520px);
  min-height:220px;
  border-radius:32px;
  border:1px dashed rgba(var(--border), .65);
  background:linear-gradient(135deg, rgba(var(--surface-rgb), .28), rgba(var(--primary), .08));
  color:rgba(var(--text), .35);
  font-size:.95rem;
  font-weight:800;
}
@media (max-width: 1024px){
  .hero-brand-bg-image{
    inset-inline-end:.8rem;
    width:min(42vw, 360px);
    max-width:42%;
    opacity:.78;
  }
  .hero-brand-bg-placeholder{
    justify-content:center;
    padding-inline:1rem;
    padding-top:1rem;
  }
  .hero-brand-bg-placeholder span{
    width:min(78vw, 420px);
    min-height:160px;
  }
}
@media (max-width: 640px){
  .hero-brand-bg-image{
    position:relative;
    inset:auto;
    top:auto;
    bottom:auto;
    transform:none;
    display:block;
    width:calc(100% - 1.5rem);
    height:100%;
    max-width:none;
    margin:.25rem .75rem .9rem;
    opacity:1;
    border-radius:24px;
    object-fit:cover;
    object-position:center top;
    filter:none;
  }
  .hero-brand-bg-placeholder{
    justify-content:center;
    align-items:center;
    padding:.75rem;
  }
  .hero-brand-bg-placeholder span{
    width:calc(100% - 1.5rem);
    min-height:180px;
    font-size:.82rem;
  }
}
</style>
