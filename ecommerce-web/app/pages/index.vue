<script setup lang="ts">
import { computed } from 'vue'
import { useAsyncData } from '#app'
import { useBrandsStore } from '~/stores/brands'
import { useProductsStore } from '~/stores/products'

const brandsStore = useBrandsStore()
const productsStore = useProductsStore()

// SSR-safe prefetch so Featured/Brands render immediately on Vercel
await useAsyncData('home-prefetch', async () => {
  await Promise.allSettled([
    brandsStore.fetchPublic(),
    productsStore.fetchFeatured(8),
	    // fallback list حتى ما تبقى الصفحة فاضية إذا endpoint المميز ما اشتغل
	    productsStore.fetch({ page: 1, pageSize: 8, sort: 'newest' }),
  ])
  return true
})

const featured = computed(() => productsStore.featured)
const fallbackLatest = computed(() => productsStore.items?.slice(0, 8) ?? [])
const featuredList = computed(() => (featured.value?.length ? featured.value : fallbackLatest.value))

const brands = computed(() => brandsStore.publicItems)
const topBrands = computed(() => {
  const seen = new Set<string>()
  const uniq = [] as typeof brands.value
  for (const b of (brands.value ?? [])) {
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
          v-for="p in featuredList"
          :key="p.id"
          :product="p"
        />
      </div>

    </section>

    <!-- Brands (marquee at the end) -->
    <section class="mx-auto max-w-6xl px-4 pb-20">
      <!-- Balanced card color for both light/dark themes -->
      <div class="rounded-3xl border border-[rgb(var(--border))] bg-surface-2 shadow-card p-6 sm:p-10">
        <div class="text-center">
          <h2 class="text-2xl font-extrabold text-[rgb(var(--text))] sm:text-4xl">Our Brands</h2>
          <p class="mt-3 text-[rgb(var(--muted))]">
            Explore our carefully curated selection of premium beauty brands.
          </p>
        </div>

	      <div class="mt-8">
	        <div
	          class="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-5"
	        >
	          <BrandCard
	            v-for="b in brandsList"
	            :key="(b as any).id ?? (b as any).name"
	            :brand="b"
	          />
	        </div>

	        <div class="mt-6 flex justify-center">
	          <NuxtLink
	            to="/brands"
	            class="inline-flex items-center rounded-full border border-[rgb(var(--border))] bg-surface-1 px-5 py-2 text-sm font-semibold text-[rgb(var(--text))] hover:bg-surface-2"
	          >
	            View All Brands
	          </NuxtLink>
	        </div>
	      </div>
      </div>
    </section>
  </div>
</template>
