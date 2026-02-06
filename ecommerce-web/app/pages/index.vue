<script setup lang="ts">
import { onMounted, computed } from 'vue'
import { useBrandsStore } from '~/app/stores/brands'
import { useProductsStore } from '~/app/stores/products'

const brandsStore = useBrandsStore()
const productsStore = useProductsStore()

onMounted(async () => {
  await Promise.all([
    brandsStore.fetchPublic(),
    productsStore.fetchFeatured(12),
  ])
})

const featured = computed(() => productsStore.featured)
const brands = computed(() => brandsStore.items)
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
        <p class="mt-3 text-[rgb(var(--muted))]">Discover our handpicked selection of bestsellers</p>
      </div>

      <div class="mt-10 grid gap-5 sm:grid-cols-2 lg:grid-cols-4">
        <ProductCard
          v-for="p in featured"
          :key="p.id"
          :product="p"
        />
      </div>

      <div class="mt-10 text-center">
        <NuxtLink
          to="/products"
          class="inline-flex items-center gap-2 rounded-full border border-[rgb(var(--border))] bg-white px-6 py-3 text-sm font-semibold text-[rgb(var(--primary))] shadow-sm hover:bg-[rgb(var(--surface-2))]"
        >
          View All Products
          <span aria-hidden="true">→</span>
        </NuxtLink>
      </div>
    </section>

    <!-- Brands (marquee at the end) -->
    <section class="mx-auto max-w-6xl px-4 pb-20">
      <div class="rounded-3xl border border-[rgb(var(--border))] bg-white/80 p-6 sm:p-10">
        <div class="text-center">
          <h2 class="text-2xl font-extrabold text-[rgb(var(--text))] sm:text-4xl">Our Brands</h2>
          <p class="mt-3 text-[rgb(var(--muted))]">
            Explore our carefully curated selection of premium beauty brands.
          </p>
        </div>

        <div class="mt-8">
          <BrandMarquee :brands="brands" />
        </div>
      </div>
    </section>
  </div>
</template>
