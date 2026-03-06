<script setup lang="ts">
import { computed } from 'vue'
import { useAsyncData } from '#app'
import { useBrandsStore } from '~/stores/brands'
import { useProductsStore } from '~/stores/products'

const { t, locale } = useI18n()
const { isMobile, liteMode } = useMobilePerf()

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
      brandsStore.fetchPublic(isMobile.value ? 8 : 10),
      productsStore.fetchFeatured(isMobile.value ? 6 : 8),
    ])
    return true
  },
  {
    server: false,
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
watch(tab, async (v) => {
  if (v !== 'discounts') return
  if (productsStore.discountItems?.length) return
  try {
    await productsStore.fetchDiscounts(isMobile.value ? 6 : 8)
  } catch {}
})
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

          
          <div class="mt-6 flex flex-wrap items-center justify-center gap-2">
            <span class="rounded-full border border-app bg-surface/80 px-3 py-1 text-xs font-bold text-[rgb(var(--text))]">{{ t('home.quickBadge1') }}</span>
            <span class="rounded-full border border-app bg-surface/80 px-3 py-1 text-xs font-bold text-[rgb(var(--text))]">{{ t('home.quickBadge2') }}</span>
            <span class="rounded-full border border-app bg-surface/80 px-3 py-1 text-xs font-bold text-[rgb(var(--text))]">{{ t('home.quickBadge3') }}</span>
          </div>

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

          <div class="mt-10 grid gap-4 text-start sm:grid-cols-3">
            <div class="rounded-3xl border border-app bg-surface/78 p-4 shadow-card backdrop-blur">
              <div class="text-xs font-bold uppercase tracking-[0.2em] text-[rgb(var(--muted))]">{{ t('home.heroCard1Label') }}</div>
              <div class="mt-2 text-lg font-black text-[rgb(var(--text))]">{{ t('home.heroCard1Title') }}</div>
              <div class="mt-1 text-sm text-[rgb(var(--muted))]">{{ t('home.heroCard1Desc') }}</div>
            </div>
            <div class="rounded-3xl border border-app bg-surface/78 p-4 shadow-card backdrop-blur">
              <div class="text-xs font-bold uppercase tracking-[0.2em] text-[rgb(var(--muted))]">{{ t('home.heroCard2Label') }}</div>
              <div class="mt-2 text-lg font-black text-[rgb(var(--text))]">{{ t('home.heroCard2Title') }}</div>
              <div class="mt-1 text-sm text-[rgb(var(--muted))]">{{ t('home.heroCard2Desc') }}</div>
            </div>
            <div class="rounded-3xl border border-app bg-surface/78 p-4 shadow-card backdrop-blur">
              <div class="text-xs font-bold uppercase tracking-[0.2em] text-[rgb(var(--muted))]">{{ t('home.heroCard3Label') }}</div>
              <div class="mt-2 text-lg font-black text-[rgb(var(--text))]">{{ t('home.heroCard3Title') }}</div>
              <div class="mt-1 text-sm text-[rgb(var(--muted))]">{{ t('home.heroCard3Desc') }}</div>
            </div>
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
        <component
          :is="liteMode ? 'div' : 'RevealOnScroll'"
          v-for="(p, idx) in displayedFeatured"
          :key="p.id"
          :parity="idx % 2"
        >
          <ProductCard :p="p" />
        </component>
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
      <BrandMarquee :brands="topBrands" :show-name="!liteMode" />
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
        <component
          :is="liteMode ? 'div' : 'RevealOnScroll'"
          v-for="(c, idx) in categoryCards"
          :key="c.key"
          :parity="(idx % 2) as 0 | 1"
          :delay="50 * idx"
        >
          <NuxtLink
            :to="`/products?q=${encodeURIComponent(categoryQuery(c))}`"
            class="group relative overflow-hidden rounded-2xl border border-app bg-surface/70 p-4 backdrop-blur transition will-change-transform hover:-translate-y-0.5 hover:bg-surface-2 hover:shadow-lg hover:shadow-[rgb(var(--primary))]/12 home-cat-card"
          >
            <div class="flex items-center gap-3">
              <div class="flex h-10 w-10 items-center justify-center rounded-xl bg-surface text-lg transition group-hover:scale-105">
                {{ c.icon }}
              </div>
              <div class="min-w-0">
                <div class="truncate text-sm font-extrabold text-[rgb(var(--text))]">
                  {{ t(c.labelKey) }}
                </div>
                <div class="mt-0.5 truncate text-xs text-[rgb(var(--muted))]">
                  {{ t('home.tapToExplore') }}
                </div>
              </div>
            </div>

            <div class="pointer-events-none absolute -right-10 -top-10 h-24 w-24 rounded-full bg-gradient-to-br from-[rgb(var(--primary))]/25 to-transparent opacity-0 blur-2xl transition group-hover:opacity-100"></div>
            <div class="pointer-events-none absolute inset-0 opacity-0 transition group-hover:opacity-100">
              <div class="absolute inset-x-0 bottom-0 h-px bg-gradient-to-r from-transparent via-[rgb(var(--primary))]/50 to-transparent"></div>
            </div>
          </NuxtLink>
        </component>
      </div>
    </section>
  </div>
</template>
