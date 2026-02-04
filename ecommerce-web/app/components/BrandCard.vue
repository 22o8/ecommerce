<template>
  <NuxtLink
    :to="`/brands/${b.slug}`"
    class="group block rounded-2xl border border-white/10 bg-white/[0.03] hover:bg-white/[0.06] transition overflow-hidden"
  >
    <div class="p-4 flex gap-4 items-center">
      <div
        class="w-14 h-14 sm:w-16 sm:h-16 rounded-xl bg-black/20 border border-white/10 overflow-hidden flex items-center justify-center"
      >
        <SmartImage
          v-if="logo"
          :src="logo"
          :alt="b.name"
          class="w-full h-full object-cover"
        />
        <div v-else class="text-xs text-white/50">Logo</div>
      </div>

      <div class="min-w-0">
        <div class="font-semibold text-white truncate">{{ b.name }}</div>
        <div class="text-sm text-white/60 line-clamp-2">
          {{ b.description || '' }}
        </div>
      </div>

      <div class="ms-auto text-white/40 group-hover:text-white/70 transition">
        <span class="i-lucide-chevron-right" />
      </div>
    </div>
  </NuxtLink>
</template>

<script setup lang="ts">
import SmartImage from '~/components/SmartImage.vue'

const props = defineProps<{ b: any }>()
const { buildAssetUrl } = useApi()

const logo = computed(() => buildAssetUrl(props.b?.logoUrl || ''))
</script>
