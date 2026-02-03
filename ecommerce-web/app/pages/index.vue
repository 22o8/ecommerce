<template>
  <div class="pb-10">
    <!-- Hero -->
    <section
      class="relative overflow-hidden rounded-3xl border border-white/10 bg-gradient-to-b from-white/5 to-transparent p-6 md:p-10"
    >
      <div class="grid gap-8 md:grid-cols-2 md:items-center">
        <div>
          <h1 class="text-4xl font-extrabold tracking-tight md:text-5xl">
            {{ t('home.hero.title') }}
          </h1>
          <p class="mt-3 max-w-xl text-white/70">
            {{ t('home.hero.subtitle') }}
          </p>

          <div class="mt-6 flex flex-wrap gap-3">
            <NuxtLink to="/products" class="btn-primary">
              <span class="i-lucide-shopping-bag mr-2" />
              {{ t('home.hero.browse') }}
            </NuxtLink>

            <a href="#contact" class="btn-ghost">
              <span class="i-lucide-message-square mr-2" />
              {{ t('home.hero.contact') }}
            </a>
          </div>
        </div>

        <div class="relative">
          <div class="aspect-[4/3] overflow-hidden rounded-3xl border border-white/10">
            <img
              src="/hero-placeholder.svg"
              alt="Hero"
              class="h-full w-full object-cover"
              loading="lazy"
            />
          </div>

          <div class="pointer-events-none absolute -inset-6 rounded-[2rem] bg-purple-500/10 blur-3xl" />
        </div>
      </div>
    </section>

    <!-- Brands (بدل Featured/Products) -->
    <section class="mt-10">
      <div class="mb-4 flex items-center justify-between">
        <h2 class="text-2xl font-bold">
          {{ t('home.section.brands') }}
        </h2>

        <NuxtLink to="/products" class="text-sm text-white/70 hover:text-white">
          {{ t('home.section.viewAll') }}
          <span class="i-lucide-arrow-right ml-1" />
        </NuxtLink>
      </div>

      <div class="rounded-3xl border border-white/10 bg-white/[0.03] p-4">
        <div v-if="loading" class="py-10 text-center text-white/60">
          {{ t('common.loading') }}
        </div>

        <div v-else-if="brands.length === 0" class="py-10 text-center">
          <div class="mx-auto mb-2 h-10 w-10 opacity-60 i-lucide-store" />
          <div class="text-white/70">{{ t('brands.empty.title') }}</div>
          <div class="text-sm text-white/50">{{ t('brands.empty.subtitle') }}</div>
        </div>

        <div v-else class="marquee">
          <div class="marquee__track">
            <div class="marquee__row">
              <BrandCard v-for="b in brands" :key="b.id" :b="b" />
            </div>
            <!-- تكرار نفس الصف حتى يصير تحريك مستمر -->
            <div class="marquee__row" aria-hidden="true">
              <BrandCard v-for="b in brands" :key="`dup-${b.id}`" :b="b" />
            </div>
          </div>
        </div>
      </div>
    </section>

    <div id="contact" class="mt-16" />
  </div>
</template>

<script setup lang="ts">
import { storeToRefs } from 'pinia'

const { t } = useI18n()

const brandsStore = useBrandsStore()
const { items: brands, loading } = storeToRefs(brandsStore)

// SSR-safe: حتى لو فشل الطلب، تبقى arrays فارغة بدون أخطاء
await brandsStore.fetchPublic({ page: 1, pageSize: 30, sort: 'new' })
</script>

<style scoped>
.btn-primary {
  @apply inline-flex items-center rounded-xl bg-purple-500 px-4 py-2 text-sm font-semibold text-black shadow-sm hover:bg-purple-400;
}
.btn-ghost {
  @apply inline-flex items-center rounded-xl border border-white/15 bg-white/5 px-4 py-2 text-sm font-semibold text-white hover:bg-white/10;
}

.marquee {
  overflow: hidden;
}
.marquee__track {
  display: flex;
  gap: 16px;
  width: max-content;
  animation: marquee 20s linear infinite;
}
.marquee__row {
  display: flex;
  gap: 16px;
}

@keyframes marquee {
  0% {
    transform: translateX(0);
  }
  100% {
    transform: translateX(-50%);
  }
}

@media (prefers-reduced-motion: reduce) {
  .marquee__track {
    animation: none;
  }
}
</style>
