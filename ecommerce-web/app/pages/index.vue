<script setup lang="ts">
import { computed } from 'vue'
import { useAsyncData } from '#app'
import { useBrandsStore } from '~/stores/brands'
import { useProductsStore } from '~/stores/products'

const { t } = useI18n()

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

            <NuxtLink
              to="/brands"
              class="btn-cta-animated btn-cta-outline inline-flex items-center justify-center rounded-full px-6 py-3 text-sm font-semibold hover:opacity-95"
            >
              {{ t('homeHero.categories') }}
            </NuxtLink>
          </div>

          <!-- quick stats -->
          <div class="mt-10 grid grid-cols-1 gap-3 sm:grid-cols-3">
            <div class="glass-panel glow-border rounded-2xl p-4">
              <div class="text-xs opacity-70">{{ t('home.statsFastShipping') }}</div>
              <div class="mt-1 text-2xl font-extrabold">24-48h</div>
            </div>
            <div class="glass-panel glow-border rounded-2xl p-4">
              <div class="text-xs opacity-70">{{ t('home.statsOriginal') }}</div>
              <div class="mt-1 text-2xl font-extrabold">100%</div>
            </div>
            <div class="glass-panel glow-border rounded-2xl p-4">
              <div class="text-xs opacity-70">{{ t('home.statsSupport') }}</div>
              <div class="mt-1 text-2xl font-extrabold">24/7</div>
            </div>
          </div>
        </div>
      </div>
    </section>

    <!-- Featured Products -->
    <section class="mx-auto max-w-6xl px-4 pb-16">
      <div class="text-center">
        <h2 class="text-2xl font-extrabold text-[rgb(var(--text))] sm:text-4xl">{{ t('homeHero.featuredProducts') }}</h2>
      </div>

      <div class="mt-10 grid gap-5 sm:grid-cols-2 lg:grid-cols-4">
        <RevealOnScroll
          v-for="(p, idx) in homeFeatured"
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

    <!-- 3D brand showcase (WOW) -->
    <section class="mx-auto max-w-6xl px-4 pb-24">
      <div class="glass-panel glow-border rounded-3xl p-6 sm:p-10">
        <div class="flex flex-col items-start justify-between gap-3 sm:flex-row sm:items-end">
          <div>
            <h2 class="text-2xl font-extrabold tracking-tight sm:text-4xl">{{ t('home.brandShowcaseTitle') }}</h2>
            <p class="mt-2 max-w-2xl text-sm text-[rgb(var(--muted))] sm:text-base">{{ t('home.brandShowcaseSubtitle') }}</p>
          </div>
          <NuxtLink to="/brands" class="btn inline-flex items-center gap-2 rounded-full px-4 py-2 text-sm font-semibold">
            {{ t('home.viewAll') }}
          </NuxtLink>
        </div>

        <div class="mt-6">
          <BrandOrbit3D :brands="topBrands" />
        </div>
      </div>
    </section>

    <!-- Spotlight categories -->
    <section class="mx-auto max-w-6xl px-4 pb-24">
      <div class="flex flex-col items-start justify-between gap-4 sm:flex-row sm:items-end">
        <div>
          <h2 class="text-2xl font-extrabold tracking-tight text-[rgb(var(--text))] sm:text-4xl">{{ t('home.spotlightTitle') }}</h2>
          <p class="mt-2 max-w-2xl text-sm text-[rgb(var(--muted))] sm:text-base">{{ t('home.spotlightSubtitle') }}</p>
        </div>
        <NuxtLink to="/products" class="btn inline-flex items-center gap-2 rounded-full px-4 py-2 text-sm font-semibold">
          {{ t('home.viewAll') }}
          <span aria-hidden="true">→</span>
        </NuxtLink>
      </div>

      <div class="mt-6 flex flex-wrap gap-2">
        <NuxtLink to="/products?category=skincare" class="chip">🧴 {{ t('home.catSkincare') }}</NuxtLink>
        <NuxtLink to="/products?category=serum" class="chip">💧 {{ t('home.catSerum') }}</NuxtLink>
        <NuxtLink to="/products?category=sunscreen" class="chip">☀️ {{ t('home.catSunscreen') }}</NuxtLink>
        <NuxtLink to="/products?category=makeup" class="chip">💄 {{ t('home.catMakeup') }}</NuxtLink>
        <NuxtLink to="/products?category=perfume" class="chip">🌸 {{ t('home.catPerfume') }}</NuxtLink>
      </div>
    </section>
  </div>
</template>
