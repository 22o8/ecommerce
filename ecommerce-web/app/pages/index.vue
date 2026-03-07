<script setup lang="ts">
import { computed } from 'vue'
import { useAsyncData } from '#app'
import { useBrandsStore } from '~/stores/brands'
import { useProductsStore } from '~/stores/products'

const { t, locale } = useI18n()

const brandsStore = useBrandsStore()
const productsStore = useProductsStore()

// IMPORTANT:
// - على Vercel (SSR) ما نضمن وجود access token/cookies بنفس لحظة الـ render.
// - وحتى لو رجع SSR فاضي، بعد الـ hydration لازم يجيب البيانات من جديد.
// لذلك نخلي الـ fetch يصير Client-only حتى ما تختفي الداتا بعد الـ refresh.
await useAsyncData(
  'home-prefetch',
  async () => {
    await Promise.allSettled([
      brandsStore.fetchPublic(),
      productsStore.fetchFeatured(8),
      productsStore.fetchDiscounts(8),
      // fallback list حتى ما تبقى الصفحة فاضية إذا endpoint المميز ما اشتغل
      productsStore.fetch({ page: 1, pageSize: 8, sort: 'newest' }),
    ])
    return true
  },
  {
    server: false,
    // always run on client after refresh/hydration
    default: () => true,
  }
)

const featured = computed(() => productsStore.featured)
// حماية SSR: فلترة أي عناصر ناقصة (undefined/null)
const safeFeatured = computed(() => (featured.value ?? []).filter((p) => !!p && !!p.id))

// fallback: آخر المنتجات المنشورة (حتى لا تبقى الصفحة فاضية)
const fallbackLatest = computed(() => productsStore.items?.slice(0, 8) ?? [])

// إذا ماكو منتجات مميزة، نعرض آخر المنتجات المنشورة حتى لا تبقى الصفحة فارغة
const homeFeatured = computed(() => {
  if (safeFeatured.value.length) return safeFeatured.value
  return (fallbackLatest.value ?? []).filter((p) => !!p && !!p.id && !!p.isPublished).slice(0, 8)
})
// للتوافق مع الكود القديم بالـ template
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
  { key: "serum", icon: "💧", labelKey: "home.catSerum", q: { ar: "سيروم", en: "serum" } },
  { key: "moisturizer", icon: "🧴", labelKey: "home.catMoisturizer", q: { ar: "مرطب", en: "moisturizer" } },
  { key: "sunscreen", icon: "☀️", labelKey: "home.catSunscreen", q: { ar: "واقي شمس", en: "sunscreen" } },
  { key: "cleanser", icon: "🫧", labelKey: "home.catCleanser", q: { ar: "غسول", en: "cleanser" } },
  { key: "perfume", icon: "🌸", labelKey: "home.catPerfume", q: { ar: "عطر", en: "perfume" } },
] as const

const categoryQuery = (c: (typeof categoryCards)[number]) => (locale.value === "ar" ? c.q.ar : c.q.en)

</script>

<template>
  <div class="min-h-screen">
    <!-- Hero -->
    <section class="relative hero-shimmer rounded-3xl mx-auto max-w-6xl px-4">
      <div class="mx-auto max-w-6xl px-4 py-20 sm:py-24">
        <div class="text-center">
          <h1 class="text-4xl font-extrabold tracking-tight text-[rgb(var(--text))] sm:text-6xl">
            {{ t('homeHero.title1') }}
            <span class="text-[rgb(var(--primary))]">{{ t('homeHero.title2') }}</span>
          </h1>
          <p class="mx-auto mt-6 max-w-2xl text-base text-[rgb(var(--muted))] sm:text-lg">
            {{ t('homeHero.subtitle') }}
          </p>

          
          <div class="mt-8 flex items-center justify-center gap-3">
            <NuxtLink
              to="/products"
              class="hero-action hero-action-primary inline-flex items-center justify-center rounded-full px-6 py-3 text-sm font-semibold hover:opacity-95"
            >
              {{ t('homeHero.products') }}
            </NuxtLink>

            <!-- نفس لون/هوية زر المنتجات (باللايت والدراك) -->
            <a
              href="#categories"
              class="hero-action hero-action-secondary inline-flex items-center justify-center rounded-full px-6 py-3 text-sm font-semibold hover:opacity-95"
            >
              {{ t('homeHero.categories') }}
            </a>
          </div>

        </div>
      </div>
    </section>

    <!-- Featured Products -->
    <section class="mx-auto max-w-6xl px-4 pb-16">
      <div class="flex flex-col items-center justify-center gap-4">
        <h2 class="text-2xl font-extrabold text-[rgb(var(--text))] sm:text-4xl">{{ t('homeHero.featuredProducts') }}</h2>

        <div class="home-switcher inline-flex items-center rounded-full border border-app bg-surface p-1">
          <button
            type="button"
            class="px-4 py-2 rounded-full text-sm font-bold transition"
            :class="tab === 'featured' ? 'active-tab bg-[rgb(var(--primary))] text-black' : 'text-[rgb(var(--text))] hover:bg-surface-2'"
            @click="tab = 'featured'"
          >
            {{ t('home.featuredTab') }}
          </button>
          <button
            type="button"
            class="px-4 py-2 rounded-full text-sm font-bold transition"
            :class="tab === 'discounts' ? 'active-tab bg-[rgb(var(--primary))] text-black' : 'text-[rgb(var(--text))] hover:bg-surface-2'"
            @click="tab = 'discounts'"
          >
            {{ t('home.discountsTab') }}
          </button>
          <NuxtLink to="/discounts" class="px-4 py-2 rounded-full text-sm font-bold text-[rgb(var(--text))] hover:bg-surface-2 transition">
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

    </section>

    <!-- Brands -->
    <section class="mx-auto max-w-6xl px-4 pb-20">
      <div class="flex flex-col items-start justify-between gap-4 sm:flex-row sm:items-end">
        <div>
          <h2 class="text-2xl font-extrabold tracking-tight text-[rgb(var(--text))] sm:text-4xl">{{ t('home.brands') }}</h2>
          <p class="mt-2 max-w-2xl text-sm text-[rgb(var(--muted))] sm:text-base">{{ t('home.brandsSubtitle') }}</p>
        </div>
        <NuxtLink
          to="/brands"
          class="btn inline-flex items-center gap-2 rounded-full px-4 py-2 text-sm font-semibold"
        >
          {{ t('nav.brands') }}
          <span aria-hidden="true">→</span>
        </NuxtLink>
      </div>

      <!-- Natural brands showcase -->
      <BrandMarquee :brands="topBrands" />
    </section>

    <!-- Spotlight categories -->
    <section id="categories" class="mx-auto max-w-6xl px-4 pb-24 scroll-mt-24">
      <div class="flex flex-col items-start justify-between gap-4 sm:flex-row sm:items-end">
        <div>
          <h2 class="text-2xl font-extrabold tracking-tight text-[rgb(var(--text))] sm:text-4xl">
            {{ t('home.spotlightTitle') }}
          </h2>
          <p class="mt-2 max-w-2xl text-sm text-[rgb(var(--muted))] sm:text-base">
            {{ t('home.spotlightSubtitle') }}
          </p>
        </div>

        <NuxtLink to="/products" class="btn inline-flex items-center rounded-full px-4 py-2 text-sm font-semibold">
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
            :to="`/products?q=${encodeURIComponent(categoryQuery(c))}`"
            class="group category-simple-card category-simple-card--elevated"
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
    </section>
<style scoped>
.hero-action{
  position: relative;
  border: 1px solid rgba(var(--border), .95);
  transition: transform .22s ease, box-shadow .22s ease, border-color .22s ease, background .22s ease, color .22s ease;
}
.hero-action:hover{ transform: translateY(-1px); }
.hero-action-primary{
  background: linear-gradient(180deg, rgba(var(--surface-rgb), .96), rgba(var(--surface-2-rgb), .92));
  color: rgb(var(--text));
  box-shadow: 0 18px 40px rgba(0,0,0,.12);
}
.hero-action-secondary{
  background: rgba(var(--surface-rgb), .72);
  color: rgb(var(--text));
  box-shadow: inset 0 1px 0 rgba(255,255,255,.18);
}
.home-switcher{
  padding: 6px;
  background: linear-gradient(180deg, rgba(var(--surface-rgb), .96), rgba(var(--surface-2-rgb), .94));
  box-shadow: 0 16px 36px rgba(0,0,0,.08);
}
.home-switcher :deep(button),
.home-switcher :deep(a){
  min-width: 112px;
  text-align: center;
}
.category-simple-card{
  display:block;
  border-radius: 28px;
  border: 1px solid rgba(var(--border), .95);
  transition: transform .22s ease, border-color .22s ease, box-shadow .22s ease, background-color .22s ease;
}
.category-simple-card--elevated{
  background: linear-gradient(180deg, rgba(var(--surface-rgb), .96), rgba(var(--surface-2-rgb), .92));
  box-shadow: 0 18px 40px rgba(16,24,40,.08);
}
.category-simple-card__inner{
  display:flex;
  align-items:center;
  gap:14px;
  min-height: 112px;
  padding: 18px;
}
.category-simple-card__icon{
  display:grid; place-items:center; width:52px; height:52px; border-radius:18px;
  background: linear-gradient(180deg, rgba(var(--primary), .14), rgba(var(--primary), .06));
  border: 1px solid rgba(var(--primary), .18);
  font-size: 24px; flex: 0 0 auto;
}
.category-simple-card__arrow{
  display:grid; place-items:center; width:40px; height:40px; border-radius:999px; flex: 0 0 auto;
  border: 1px solid rgba(var(--border), .95);
  background: rgba(var(--surface-rgb), .82);
  color: rgb(var(--text)); font-weight: 900;
}
.category-simple-card:hover{
  transform: translateY(-4px);
  border-color: rgba(var(--primary), .28);
  box-shadow: 0 24px 50px rgba(236,72,153,.12);
}
:global(html.theme-light) .hero-action-primary{
  background: linear-gradient(180deg, rgba(26, 28, 40, .98), rgba(35, 37, 52, .94));
  color: white;
  border-color: rgba(24,24,40,.12);
  box-shadow: 0 18px 44px rgba(17,24,39,.16);
}
:global(html.theme-light) .hero-action-secondary{
  background: linear-gradient(180deg, rgba(255,255,255,.98), rgba(250,244,248,.94));
  color: rgb(var(--text));
  box-shadow: 0 14px 30px rgba(236,72,153,.10);
}
:global(html.theme-light) .home-switcher{
  background: linear-gradient(180deg, rgba(255,255,255,.98), rgba(250,244,248,.94));
  box-shadow: 0 16px 38px rgba(236,72,153,.08);
}
:global(html.theme-light) .home-switcher :deep(button.active-tab),
:global(html.theme-light) .home-switcher :deep(a.active-tab){
  color:white;
}
:global(html.theme-light) .category-simple-card--elevated{
  background: linear-gradient(180deg, rgba(255,255,255,.98), rgba(251,245,249,.94));
  border-color: rgba(228, 208, 221, .95);
  box-shadow: 0 20px 44px rgba(17,24,39,.05), 0 8px 24px rgba(236,72,153,.08);
}
:global(html.theme-dark) .category-simple-card--elevated{
  background: linear-gradient(180deg, rgba(var(--surface-rgb), .96), rgba(var(--surface-2-rgb), .92));
  box-shadow: 0 18px 40px rgba(0,0,0,.28);
}
</style>
