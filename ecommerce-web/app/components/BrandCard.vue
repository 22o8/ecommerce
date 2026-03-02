<template>
  <!-- حماية SSR: إذا رجع الـ API عنصر ناقص/undefined لا نكسر الصفحة -->
  <component
    :is="safeTo ? 'NuxtLink' : 'div'"
    :to="safeTo ? `/brands/${b?.slug}` : undefined"
    class="group relative block rounded-2xl border border-app bg-surface-2 hover:bg-surface transition overflow-hidden will-change-transform hover:-translate-y-0.5 hover:shadow-xl hover:shadow-[rgb(var(--primary))]/10"
  >
    <div class="p-4 flex gap-4 items-center">
      <div
        class="w-14 h-14 sm:w-16 sm:h-16 rounded-xl bg-surface border border-app overflow-hidden flex items-center justify-center"
      >
        <SmartImage
          :src="logo || ''"
          :alt="b?.name || 'Brand'"
          fit="cover"
          wrapper-class="w-full h-full"
          img-class="w-full h-full object-cover transition duration-500 group-hover:scale-110"
        />
      </div>

      <div class="min-w-0">
        <div class="font-semibold text-[rgb(var(--text))] truncate">{{ b?.name || '' }}</div>
        <div class="text-sm text-[rgba(var(--muted),0.9)] line-clamp-2">
          {{ b?.description || '' }}
        </div>
      </div>

      <div class="ms-auto text-[rgb(var(--text))]/40 group-hover:text-[rgba(var(--muted),0.95)] transition group-hover:translate-x-0.5">
        <span class="i-lucide-chevron-right" />
      </div>
    </div>
    <div class="pointer-events-none absolute inset-x-4 -bottom-px h-px bg-gradient-to-r from-transparent via-[rgb(var(--primary))]/55 to-transparent opacity-0 transition group-hover:opacity-100"></div>
</component>
</template>

<script setup lang="ts">
import SmartImage from '~/components/SmartImage.vue'

const props = defineProps<{ b: any }>()
const { buildAssetUrl } = useApi()

const logo = computed(() => {
  const raw =
    props.b?.logoUrl ||
    props.b?.logo ||
    props.b?.imageUrl ||
    props.b?.image ||
    ''
  return buildAssetUrl(raw)
})
const safeTo = computed(() => !!props.b && !!props.b.slug)
</script>
