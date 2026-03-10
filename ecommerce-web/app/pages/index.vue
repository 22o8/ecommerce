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
            {{ t('home.featuredTab') }}
          </button>
          <button
            type="button"
            class="px-4 py-2 rounded-full text-sm font-bold transition"
            :class="tab === 'discounts' ? 'bg-[rgb(var(--primary))] text-black' : 'text-[rgb(var(--text))] hover:bg-surface-2'"
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
    </section>
  </div>
</template>

<style scoped>
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
:global(html.theme-light) .category-simple-card{
  background: linear-gradient(180deg, rgba(255,255,255,.98), rgba(255,247,252,.94));
  box-shadow: 0 18px 44px rgba(232, 91, 154, .08), 0 10px 26px rgba(24,24,24,.05);
}
:global(html.theme-light) .category-simple-card:hover{
  background: linear-gradient(180deg, rgba(255,255,255,1), rgba(255,244,250,.98));
  box-shadow: 0 24px 56px rgba(232, 91, 154, .12), 0 12px 28px rgba(24,24,24,.06);
}
:global(html.theme-dark) .category-simple-card{
  background: linear-gradient(180deg, rgba(var(--surface-rgb), .98), rgba(var(--surface-2-rgb), .86));
  box-shadow: 0 16px 40px rgba(0,0,0,.24);
}
@media (max-width: 640px){
  .category-simple-card__inner{ min-height: 94px; padding: 16px; }
  .category-simple-card__icon{ width: 46px; height: 46px; border-radius: 16px; font-size: 22px; }
  .category-simple-card__arrow{ width: 32px; height: 32px; }
}
</style>
