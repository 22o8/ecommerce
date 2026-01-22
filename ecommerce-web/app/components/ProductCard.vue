<template>
  <NuxtLink :to="`/products/${p.slug || p.id}`" class="group card-soft overflow-hidden transition duration-300 hover:-translate-y-0.5 hover:shadow-lg">
    <div class="relative">
      <div class="h-44 bg-surface-2 grid place-items-center">
        <img v-if="img" :src="img" class="h-full w-full object-cover will-change-transform transition duration-300 group-hover:scale-[1.03]" :alt="p.name" loading="lazy" decoding="async" />
        <div v-else class="text-center grid gap-2 px-4">
          <Icon name="mdi:image-outline" class="text-3xl opacity-70 mx-auto" />
          <div class="text-sm text-muted rtl-text">{{ t('noImage') }}</div>
        </div>
      </div>
      <div class="absolute top-3 left-3">
        <UiBadge>
          <Icon name="mdi:star-outline" />
          <span class="rtl-text">{{ t('home.section.new') }}</span>
        </UiBadge>
      </div>
    </div>

    <div class="p-4 grid gap-2">
      <div class="font-extrabold rtl-text">{{ p.name }}</div>
      <div class="text-sm text-muted rtl-text">{{ p.description || '' }}</div>

      <div class="flex items-center justify-between mt-2">
        <div class="font-black keep-ltr">{{ fmt(p.price) }}</div>
        <div class="inline-flex items-center gap-2 text-sm text-muted">
          <span class="rtl-text">{{ t('buy') }}</span>
          <Icon name="mdi:arrow-right" class="keep-ltr transition group-hover:translate-x-1" />
        </div>
      </div>
    </div>
  </NuxtLink>
</template>

<script setup lang="ts">
import UiBadge from '~/components/ui/UiBadge.vue'
import { useApi } from '~/composables/useApi'
const { t } = useI18n()
const api = useApi()
const props = defineProps<{ p: any }>()

const firstImageUrl = computed(() => {
  const p = props.p || {}
  const images = p?.images
  const first = Array.isArray(images) ? images[0] : null

  if (typeof first === 'string') return first
  if (first && typeof first === 'object') {
    return first.url || first.path || first.fileUrl || first.imageUrl || first.src || ''
  }

  return p?.imageUrl || p?.image || p?.thumbnailUrl || ''
})

const img = computed(() => api.buildAssetUrl(firstImageUrl.value))

function fmt(v: any) {
  const n = Number(v || 0)
  return new Intl.NumberFormat(undefined, { style: 'currency', currency: 'USD' }).format(n)
}
</script>
