<template>
  <div class="grid gap-8">
    <!-- Hero -->
    <section class="card-soft overflow-hidden">
      <div class="grid gap-6 p-6 md:p-10 lg:grid-cols-2 lg:items-center">
        <div class="grid gap-4">

          <h1 class="text-3xl md:text-5xl font-black leading-tight rtl-text">
            {{ t('heroTitle') }}
          </h1>

          <p class="text-muted text-base md:text-lg rtl-text">
            {{ t('home.subtitle') }}
          </p>

          <div class="flex flex-wrap gap-3">
            <NuxtLink to="/products">
              <UiButton>
                <Icon name="mdi:shopping-search-outline" class="text-lg" />
                <span class="rtl-text">{{ t('browseProducts') }}</span>
              </UiButton>
            </NuxtLink>

            <NuxtLink to="/contact">
              <UiButton variant="secondary">
                <Icon name="mdi:message-text-outline" class="text-lg" />
                <span class="rtl-text">{{ t('contact') }}</span>
              </UiButton>
            </NuxtLink>
          </div>

          <div class="grid gap-3 mt-2 md:grid-cols-3">
            <div class="rounded-3xl border border-app bg-surface p-4">
              <div class="flex items-center gap-2">
                <Icon name="mdi:truck-fast-outline" class="text-xl animate-floaty" />
                <div class="font-bold rtl-text">{{ t('fastDelivery') }}</div>
              </div>
              <div class="text-sm text-muted rtl-text mt-1">{{ t('whyUs') }}</div>
            </div>

            <div class="rounded-3xl border border-app bg-surface p-4">
              <div class="flex items-center gap-2">
                <Icon name="mdi:shield-check-outline" class="text-xl animate-floaty" />
                <div class="font-bold rtl-text">{{ t('securePayments') }}</div>
              </div>
              <div class="text-sm text-muted rtl-text mt-1">{{ t('whyUs') }}</div>
            </div>

            <div class="rounded-3xl border border-app bg-surface p-4">
              <div class="flex items-center gap-2">
                <Icon name="mdi:headset" class="text-xl animate-floaty" />
                <div class="font-bold rtl-text">{{ t('support') }}</div>
              </div>
              <div class="text-sm text-muted rtl-text mt-1">{{ t('whyUs') }}</div>
            </div>
          </div>
        </div>

        <!-- Hero Image -->
        <div class="relative">
          <div class="absolute -inset-10 bg-[rgba(var(--primary),.08)] blur-3xl" />
          <div class="relative rounded-3xl border border-app bg-surface overflow-hidden">
            <picture>
              <source srcset="/hero/images.avif" type="image/avif" />
              <img
                src="/hero-placeholder.svg"
                alt="Hero"
                class="h-[320px] md:h-[420px] w-full object-cover"
                loading="lazy"
                decoding="async"
              />
            </picture>

            <div
              class="absolute inset-0 bg-gradient-to-t from-[rgba(0,0,0,.45)] via-transparent to-transparent pointer-events-none"
            />

            <div class="absolute bottom-4 left-4 right-4 flex items-center justify-between gap-3">
              <UiBadge>
                <span class="rtl-text">{{ t('home.badge') }}</span>
              </UiBadge>

              <UiBadge>
                <span class="rtl-text">{{ t('home.section.new') }}</span>
              </UiBadge>
            </div>
          </div>
        </div>
      </div>
    </section>

    <!-- Featured products -->
    <section class="grid gap-4">
      <div class="flex items-center justify-between gap-3">
        <h2 class="text-xl md:text-2xl font-extrabold rtl-text">
          {{ t('home.section.featured') }}
        </h2>

        <NuxtLink to="/products">
          <UiButton variant="ghost">
            <span class="rtl-text">{{ t('home.viewAll') }}</span>
            <Icon name="mdi:arrow-right" class="keep-ltr text-lg" />
          </UiButton>
        </NuxtLink>
      </div>

      <div v-if="loading" class="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
        <div v-for="i in 6" :key="i" class="card-soft p-4 grid gap-3">
          <div class="skeleton h-40" />
          <div class="skeleton h-5 w-3/4" />
          <div class="skeleton h-4 w-1/2" />
          <div class="skeleton h-10" />
        </div>
      </div>

      <div v-else-if="items.length === 0" class="card-soft p-8 text-center">
        <Icon name="mdi:package-variant-closed" class="text-4xl opacity-70 mx-auto" />
        <div class="mt-3 font-bold rtl-text">{{ t('home.empty') }}</div>
      </div>

      <div v-else class="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
        <ProductCard v-for="p in items" :key="p.id" :p="p" />
      </div>
    </section>
  </div>
</template>

<script setup lang="ts">
import UiButton from '~/components/ui/UiButton.vue'
import UiBadge from '~/components/ui/UiBadge.vue'
import ProductCard from '~/components/ProductCard.vue'
import { useProductsStore } from '~/stores/products'

const { t } = useI18n()
const products = useProductsStore()

const loading = ref(true)
const items = computed(() => products.items.slice(0, 6))

onMounted(async () => {
  loading.value = true
  await products.fetch({ page: 1, pageSize: 12 }).catch(() => {})
  loading.value = false
})
</script>
