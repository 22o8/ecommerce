<template>
  <div>
    <!-- Hero -->
    <section class="mx-auto max-w-7xl px-4 pt-10 md:pt-14">
      <div class="rounded-3xl border border-[rgba(var(--border),1)] bg-gradient-to-b from-white/5 to-transparent p-6 md:p-10">
        <div class="grid gap-8 md:grid-cols-2 md:items-center">
          <div>
            <h1 class="text-3xl font-extrabold tracking-tight md:text-5xl">
              <span class="block">{{ t('home.heroTitle1') }}</span>
              <span class="block text-[rgba(var(--text),0.9)]">{{ t('home.heroTitle2') }}</span>
            </h1>
            <p class="mt-4 text-[rgba(var(--muted),0.95)]">
              {{ t('home.heroSubtitle') }}
            </p>

            <div class="mt-6 flex flex-wrap gap-3">
              <NuxtLink
                to="/brands"
                class="rounded-xl bg-[rgb(var(--primary))] px-4 py-2 font-semibold text-black hover:opacity-90"
              >
                {{ t('home.browseBrands') }}
              </NuxtLink>

              <!-- Removed Dashboard button from hero per request -->
            </div>
          </div>

          <div class="hidden md:block">
            <div class="rounded-3xl border border-[rgba(var(--border),1)] bg-[rgba(var(--surface),0.85)] p-6">
              <div class="text-sm text-[rgba(var(--muted),0.9)]">{{ t('home.brands') }}</div>
              <div class="mt-2 text-2xl font-bold">{{ brands.length }}</div>
              <div class="mt-6 grid grid-cols-2 gap-3">
                <div class="rounded-2xl border border-[rgba(var(--border),1)] bg-black/20 p-4">
                  <div class="text-xs text-[rgba(var(--muted),0.9)]">{{ t('home.quick') }}</div>
                  <div class="mt-1 font-semibold">{{ t('home.quickBrands') }}</div>
                </div>
                <div class="rounded-2xl border border-[rgba(var(--border),1)] bg-black/20 p-4">
                  <div class="text-xs text-[rgba(var(--muted),0.9)]">{{ t('home.quick') }}</div>
                  <div class="mt-1 font-semibold">{{ t('home.quickProducts') }}</div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </section>

    <!-- Featured Brands Marquee -->
    <section class="mx-auto mt-10 max-w-7xl px-4">
      <div class="flex flex-wrap items-end justify-between gap-4">
        <div>
          <h2 class="text-2xl font-bold">{{ t('home.brands') }}</h2>
          <p class="mt-1 text-[rgba(var(--muted),0.9)]">{{ t('home.brandsSubtitle') }}</p>
        </div>
        <NuxtLink to="/brands" class="text-sm font-semibold text-[rgba(var(--muted),0.95)] hover:text-[rgb(var(--text))]">
          {{ t('home.viewAllBrands') }}
        </NuxtLink>
      </div>

      <div class="mt-5 overflow-hidden rounded-3xl border border-[rgba(var(--border),1)] bg-[rgba(var(--surface),0.85)]">
        <div v-if="pending" class="p-6 text-[rgba(var(--muted),0.9)]">{{ t('common.loading') }}</div>
        <div v-else-if="!marqueeItems.length" class="p-6 text-[rgba(var(--muted),0.9)]">{{ t('brands.empty') }}</div>

        <div v-else class="relative">
          <div class="marquee" :class="{ paused: hover }" @mouseenter="hover = true" @mouseleave="hover = false">
            <div class="marquee__track">
              <NuxtLink
                v-for="b in marqueeItems"
                :key="b._k"
                :to="`/brands/${b.slug}`"
                class="marquee__item"
              >
                <img
                  v-if="b.logoUrl"
                  :src="resolveImage(b.logoUrl)"
                  :alt="b.name"
                  class="h-10 w-10 rounded-xl object-cover"
                />
                <div v-else class="h-10 w-10 rounded-xl bg-white/10" />

                <div class="min-w-0">
                  <div class="truncate font-semibold">{{ b.name }}</div>
                  <div class="truncate text-xs text-[rgba(var(--muted),0.9)]">
                    {{ b.description || t('brands.noDescription') }}
                  </div>
                </div>
              </NuxtLink>
            </div>
          </div>

          <div class="pointer-events-none absolute inset-y-0 left-0 w-16 bg-gradient-to-r from-[rgba(var(--bg),0.85)] to-transparent" />
          <div class="pointer-events-none absolute inset-y-0 right-0 w-16 bg-gradient-to-l from-[rgba(var(--bg),0.85)] to-transparent" />
        </div>
      </div>
    </section>
  </div>
</template>

<script setup lang="ts">
const { t } = useI18n()
const brandsStore = useBrandsStore()
const hover = ref(false)

const { pending } = await useAsyncData('home_brands', async () => {
  await brandsStore.fetchPublic()
  return true
})

const brands = computed(() => brandsStore.publicItems || brandsStore.items || [])

const marqueeItems = computed(() => {
  const list = brands.value || []
  // duplication makes the loop seamless
  const doubled = [...list, ...list]
  return doubled.map((b: any, idx: number) => ({ ...b, _k: `${b.id || b.slug}-${idx}` }))
})

const { buildAssetUrl } = useApi()
function resolveImage(url?: string | null) {
  return buildAssetUrl(url || '')
}
</script>

<style scoped>
.marquee {
  overflow: hidden;
}

.marquee__track {
  display: flex;
  gap: 12px;
  width: max-content;
  padding: 16px;
  animation: scroll 25s linear infinite;
}

.marquee.paused .marquee__track {
  animation-play-state: paused;
}

.marquee__item {
  display: flex;
  align-items: center;
  gap: 12px;
  min-width: 240px;
  max-width: 280px;
  padding: 10px 12px;
  border-radius: 16px;
  border: 1px solid rgba(255, 255, 255, 0.08);
  background: rgba(255, 255, 255, 0.03);
}

@keyframes scroll {
  from {
    transform: translateX(0);
  }
  to {
    transform: translateX(-50%);
  }
}
</style>
