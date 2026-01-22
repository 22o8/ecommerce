<script setup lang="ts">
const api = useApi()
const router = useRouter()

const props = defineProps<{ p: any }>()

const remoteImg = ref('')

const firstImageUrl = computed(() => {
  const imgs = (props.p?.images || [])
  if (!imgs.length) return ''
  return imgs[0]?.url || imgs[0] || ''
})

const img = computed(() => api.buildAssetUrl(firstImageUrl.value))
const finalImg = computed(() => remoteImg.value || img.value)

onMounted(async () => {
  // إذا القائمة ما رجعت صور (غالباً endpoint list ما يضمّن الصور)
  // نحاول نجيب أول صورة من BFF.
  if (finalImg.value) return
  const id = String(props.p?.id || '')
  if (!id) return

  try {
    const res: any = await api.get(`/Products/${id}/images`)
    const items = Array.isArray(res) ? res : (res?.items || res?.data || [])
    const url = items?.[0]?.url || items?.[0] || ''
    if (url) remoteImg.value = api.buildAssetUrl(url)
  } catch {
    // ignore
  }
})

function open(){
  router.push(`/products/${props.p.slug || props.p.id}`)
}
</script>

<template>
  <div class="group rounded-2xl border border-white/10 bg-white/5 hover:bg-white/10 transition p-4 cursor-pointer" @click="open">
    <div class="rounded-xl overflow-hidden bg-white/5 border border-white/10 aspect-[4/3]">
      <img
        v-if="finalImg"
        :src="finalImg"
        class="h-full w-full object-cover will-change-transform transition duration-300 group-hover:scale-[1.03]"
        :alt="p.name"
        loading="lazy"
        decoding="async"
      />
      <div v-else class="h-full w-full grid place-items-center text-white/50 text-sm">
        لا توجد صورة
      </div>
    </div>

    <div class="mt-3 flex items-start justify-between gap-3">
      <div class="min-w-0">
        <div class="font-semibold truncate">{{ p.name }}</div>
        <div class="text-white/60 text-sm truncate">{{ p.shortDescription || p.description || '' }}</div>
      </div>

      <div class="text-right">
        <div class="font-bold">${{ Number(p.price || 0).toFixed(2) }}</div>
      </div>
    </div>

    <div class="mt-3 flex justify-end">
      <button class="text-sm px-3 py-2 rounded-xl bg-white/10 hover:bg-white/20 border border-white/10">شراء الآن →</button>
    </div>
  </div>
</template>
