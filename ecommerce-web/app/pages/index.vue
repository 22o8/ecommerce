<script setup lang="ts">
import { computed } from 'vue'
import { useAsyncData } from '#app'
import { useBrandsStore } from '~/stores/brands'
import { useProductsStore } from '~/stores/products'

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
    <section class="relative">
      <div class="mx-auto max-w-6xl px-4 py-20 sm:py-24">
        <div class="text-center">
          <h1 class="text-4xl font-extrabold tracking-tight text-[rgb(var(--text))] sm:text-6xl">
            Discover Your Perfect
            <span class="text-[rgb(var(--primary))]">Beauty Routine</span>
          </h1>
          <p class="mx-auto mt-6 max-w-2xl text-base text-[rgb(var(--muted))] sm:text-lg">
            Explore our curated collection of premium skincare and beauty products from the world's best brands.
          </p>

          <div class="mt-8 flex items-center justify-center gap-3">
            <NuxtLink
              to="/products"
              class="inline-flex items-center gap-2 rounded-full bg-[rgb(var(--primary))] px-6 py-3 text-sm font-semibold text-white shadow-[0_12px_30px_rgba(236,72,153,0.25)] hover:opacity-90"
            >
              Shop Now
              <span aria-hidden="true">→</span>
            </NuxtLink>
          </div>
        </div>
      </div>
    </section>

    <!-- Featured Products -->
    <section class="mx-auto max-w-6xl px-4 pb-16">
      <div class="text-center">
        <h2 class="text-2xl font-extrabold text-[rgb(var(--text))] sm:text-4xl">Featured Products</h2>
      </div>

      <div class="mt-10 grid gap-5 sm:grid-cols-2 lg:grid-cols-4">
        <ProductCard
          v-for="p in homeFeatured"
          :key="p.id"
          :p="p"
        />
      </div>

    </section>

    <!-- Brands -->
    <section class="mx-auto max-w-6xl px-4 pb-20">
      <div class="rounded-3xl border border-[rgb(var(--border))] bg-surface-2 shadow-card p-4 sm:p-6">
        <div class="mt-2">
          <!-- Moving brands (logos only) -->
          <BrandMarquee :brands="topBrands" :showName="false" />
        </div>
      </div>
    </section>
  </div>
</template>
