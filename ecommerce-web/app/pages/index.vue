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
              class="btn-cta-animated inline-flex items-center justify-center rounded-full px-6 py-3 text-sm font-semibold hover:opacity-95"
            >
              {{ t('homeHero.products') }}
            </NuxtLink>

            <!-- نفس لون/هوية زر المنتجات (باللايت والدراك) -->
            <a
              href="#categories"
              class="btn-cta-animated btn-cta-secondary inline-flex items-center justify-center rounded-full px-6 py-3 text-sm font-semibold hover:opacity-95"
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

        <div class="inline-flex items-center rounded-full border border-app bg-surface p-1">
          <button
            type="button"
            class="px-4 py-2 rounded-full text-sm font-bold transition"
            :class="tab === 'featured' ? 'bg-[rgb(var(--primary))] text-black' : 'text-[rgb(var(--text))] hover:bg-surface-2'"
            @click="tab = 'featured'"
          >
            Featured
          </button>
          <button
            type="button"
            class="px-4 py-2 rounded-full text-sm font-bold transition"
            :class="tab === 'discounts' ? 'bg-[rgb(var(--primary))] text-black' : 'text-[rgb(var(--text))] hover:bg-surface-2'"
            @click="tab = 'discounts'"
          >
            Discounts
          </button>
          <NuxtLink to="/discounts" class="px-4 py-2 rounded-full text-sm font-bold text-[rgb(var(--text))] hover:bg-surface-2 transition">
            View all
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

      <div class="mt-6 grid gap-3 sm:grid-cols-2 lg:grid-cols-5">
        <RevealOnScroll
          v-for="(c, idx) in categoryCards"
          :key="c.key"
          :parity="(idx % 2) as 0 | 1"
          :delay="50 * idx"
        >
          <NuxtLink
            :to="`/products?q=${encodeURIComponent(categoryQuery(c))}`"
            class="group category-premium-card relative overflow-hidden rounded-[28px] border border-app bg-surface/80 p-5 transition duration-300 hover:-translate-y-1 hover:border-[rgb(var(--primary))]/35"
          >
            <div class="flex items-center justify-between gap-3">
              <div class="flex items-center gap-3">
                <div class="flex h-12 w-12 items-center justify-center rounded-2xl bg-[rgb(var(--primary))]/12 text-xl shadow-inner shadow-black/10 transition duration-300 group-hover:scale-105 group-hover:bg-[rgb(var(--primary))]/18">
                  {{ c.icon }}
                </div>
                <div class="min-w-0">
                  <div class="truncate text-base font-extrabold text-[rgb(var(--text))]">
                    {{ t(c.labelKey) }}
                  </div>
                  <div class="mt-1 truncate text-xs text-[rgb(var(--muted))]">
                    {{ t('home.tapToExplore') }}
                  </div>
                </div>
              </div>
              <div class="grid h-9 w-9 place-items-center rounded-full border border-white/10 bg-white/5 text-sm text-[rgb(var(--text))] transition duration-300 group-hover:bg-[rgb(var(--primary))] group-hover:text-black">
                ←
              </div>
            </div>

            <div class="mt-5 h-1.5 w-20 rounded-full bg-gradient-to-r from-[rgb(var(--primary))]/75 via-[rgb(var(--primary))]/35 to-transparent transition-all duration-300 group-hover:w-28"></div>
            <div class="pointer-events-none absolute inset-x-4 bottom-0 h-px bg-gradient-to-r from-transparent via-[rgb(var(--primary))]/35 to-transparent opacity-60"></div>
            <div class="pointer-events-none absolute -left-8 top-3 h-20 w-20 rounded-full bg-[rgb(var(--primary))]/8 blur-2xl"></div>
          </NuxtLink>
        </RevealOnScroll>
      </div>
    </section>
  </div>
</template>

<style scoped>
.category-premium-card{
  box-shadow: 0 14px 40px rgba(0,0,0,.10);
}
:global(html.theme-light) .category-premium-card{
  background: linear-gradient(180deg, rgba(255,255,255,.96), rgba(249,247,255,.92));
}
:global(html.theme-dark) .category-premium-card{
  background: linear-gradient(180deg, rgba(var(--surface-1), .96), rgba(var(--surface-2), .78));
}
</style>
