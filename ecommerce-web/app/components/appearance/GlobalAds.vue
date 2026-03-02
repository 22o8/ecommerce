<template>
  <div v-if="enabled">
    <!-- Popup Ad -->
    <div
      v-if="popupAd && showPopup"
      class="fixed inset-0 z-[60] flex items-center justify-center p-4"
    >
      <div class="absolute inset-0 bg-black/55" @click="close" />
      <div class="relative w-full max-w-[560px] rounded-3xl overflow-hidden shadow-2xl border border-white/10 bg-white dark:bg-zinc-950">
        <button
          class="absolute top-3 left-3 z-10 h-10 w-10 rounded-full bg-black/40 text-white grid place-items-center hover:bg-black/55 transition"
          @click="close"
          aria-label="close"
        >
          ✕
        </button>

        <a
          :href="popupAd.linkUrl || '#'
          "
          :target="popupAd.linkUrl ? '_blank' : undefined"
          class="block"
          @click="onAdClick"
        >
          <img :src="popupAd.imageUrl" :alt="popupAd.title || 'ad'" class="w-full h-auto" />
        </a>
        <div v-if="popupAd.title" class="p-4 text-center font-semibold text-zinc-900 dark:text-zinc-100">
          {{ popupAd.title }}
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
const route = useRoute()
const store = useAppearanceStore()

const enabled = computed(() => !route.path.startsWith('/admin'))

const popupAd = computed(() => {
  const ads = store.data.ads || []
  return ads.find((a) => a.isActive && (a.position || 'popup') === 'popup')
})

const showPopup = ref(false)

function close() {
  showPopup.value = false
  if (process.client) {
    localStorage.setItem('appearance_seen_at', store.data.updatedAt || '')
  }
}

function onAdClick() {
  // close after click
  close()
}

onMounted(() => {
  const seen = localStorage.getItem('appearance_seen_at') || ''
  const current = store.data.updatedAt || ''
  // show only on home and only once per update
  if (route.path === '/' && popupAd.value && current && seen !== current) {
    showPopup.value = true
  }
})

watch(
  () => route.path,
  (p) => {
    if (p !== '/') showPopup.value = false
    else {
      const seen = process.client ? localStorage.getItem('appearance_seen_at') || '' : ''
      const current = store.data.updatedAt || ''
      if (popupAd.value && current && seen !== current) showPopup.value = true
    }
  }
)
</script>
