<template>
  <section>
    <div class="flex items-center justify-between gap-4">
      <div>
        <h2 class="text-xl font-semibold">Featured Products</h2>
        <p class="text-sm text-muted">Discover our handpicked selection of bestsellers</p>
      </div>

      <NuxtLink to="/products" class="text-sm font-medium text-accent hover:opacity-90">
        View All Products →
      </NuxtLink>
    </div>

    <div class="relative mt-5">
      <button
        type="button"
        class="carousel-arrow left-2"
        aria-label="Previous"
        @click="scrollBy(-1)"
      >
        ‹
      </button>

      <div
        ref="scroller"
        class="no-scrollbar flex gap-4 overflow-x-auto scroll-smooth py-2"
      >
        <div
          v-for="(p, i) in products"
          :key="p.id || p.slug || i"
          class="min-w-[250px] max-w-[250px] snap-start"
        >
          <ProductCard :p="p" />
        </div>
      </div>

      <button
        type="button"
        class="carousel-arrow right-2"
        aria-label="Next"
        @click="scrollBy(1)"
      >
        ›
      </button>
    </div>
  </section>
</template>

<script setup lang="ts">
const props = defineProps<{ products: any[] }>()

const scroller = ref<HTMLElement | null>(null)

function scrollBy(dir: 1 | -1) {
  if (!scroller.value) return
  const el = scroller.value
  const card = el.querySelector<HTMLElement>('[data-card]')
  const step = card ? card.offsetWidth + 16 : 280
  el.scrollBy({ left: dir * step, behavior: 'smooth' })
}
</script>

<style scoped>
.no-scrollbar::-webkit-scrollbar { display: none; }
.no-scrollbar { -ms-overflow-style: none; scrollbar-width: none; }

.carousel-arrow {
  position: absolute;
  top: 50%;
  transform: translateY(-50%);
  width: 44px;
  height: 44px;
  border-radius: 9999px;
  background: rgba(255,255,255,0.75);
  border: 1px solid rgba(0,0,0,0.08);
  box-shadow: 0 10px 25px rgba(0,0,0,0.12);
  display: grid;
  place-items: center;
  font-size: 28px;
  line-height: 1;
  z-index: 10;
}
:global(.dark) .carousel-arrow {
  background: rgba(17, 17, 17, 0.75);
  border-color: rgba(255,255,255,0.12);
  box-shadow: 0 10px 25px rgba(0,0,0,0.35);
  color: white;
}
</style>
