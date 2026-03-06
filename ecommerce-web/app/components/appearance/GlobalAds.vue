<template>
  <div v-if="enabled">
    <div v-if="route.path === '/' && bannerAd" class="mx-auto max-w-7xl px-4 pt-4">
      <a
        :href="bannerAd.linkUrl || '#'"
        :target="bannerAd.linkUrl ? '_blank' : undefined"
        class="block overflow-hidden rounded-3xl border border-white/10 bg-white/5 shadow-card"
      >
        <img :src="asset(bannerAd.imageUrl)" :alt="bannerAd.title || 'banner'" class="h-auto w-full object-cover" />
      </a>
    </div>

    <div v-if="popupAd && showPopup" class="fixed inset-0 z-[60] flex items-center justify-center p-4">
      <div class="absolute inset-0 bg-black/55" @click="close" />
      <div class="relative w-full max-w-[560px] overflow-hidden rounded-3xl border border-white/10 bg-white shadow-2xl dark:bg-zinc-950">
        <button
          class="absolute left-3 top-3 z-10 grid h-10 w-10 place-items-center rounded-full bg-black/40 text-white transition hover:bg-black/55"
          @click="close"
          aria-label="close"
        >✕</button>
        <a :href="popupAd.linkUrl || '#'" :target="popupAd.linkUrl ? '_blank' : undefined" class="block" @click="onAdClick">
          <img :src="asset(popupAd.imageUrl)" :alt="popupAd.title || 'ad'" class="h-auto w-full" />
        </a>
        <div v-if="popupAd.title" class="p-4 text-center font-semibold text-zinc-900 dark:text-zinc-100">{{ popupAd.title }}</div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
const route = useRoute()
const api = useApi()

const enabled = computed(() => !route.path.startsWith('/admin'))
const ads = ref<any[]>([])
const showPopup = ref(false)
const { liteMode } = useMobilePerf()
const CACHE_KEY = 'global_active_ads_v2'
const CACHE_TTL = 1000 * 60 * 5

const bannerAd = computed(() => ads.value.find((a: any) => a?.type === 'banner' && a?.placement === 'home_top'))
const popupAd = computed(() => ads.value.find((a: any) => a?.type === 'popup'))

const asset = (p?: string) => api.buildAssetUrl(p || '')

function readCache() {
  if (!import.meta.client) return null
  try {
    const raw = sessionStorage.getItem(CACHE_KEY)
    if (!raw) return null
    const parsed = JSON.parse(raw)
    if (Date.now() - Number(parsed?.at || 0) > CACHE_TTL) return null
    return Array.isArray(parsed?.items) ? parsed.items : null
  } catch {
    return null
  }
}

function writeCache(items: any[]) {
  if (!import.meta.client) return
  try {
    sessionStorage.setItem(CACHE_KEY, JSON.stringify({ at: Date.now(), items }))
  } catch {}
}

async function loadAds(force = false) {
  if (!enabled.value) return
  const cached = !force ? readCache() : null
  if (cached) {
    ads.value = cached
    syncPopup()
    return
  }
  try {
    const res: any = await api.get('/ads/active')
    const items = Array.isArray(res) ? res : (Array.isArray(res?.items) ? res.items : [])
    ads.value = items
    writeCache(items)
  } catch {
    ads.value = []
  }
  syncPopup()
}

function popupKey() {
  return popupAd.value ? `popup_seen_${popupAd.value.id}_${popupAd.value.updatedAt || ''}` : ''
}

function syncPopup() {
  if (!process.client || liteMode.value || route.path !== '/' || !popupAd.value) {
    showPopup.value = false
    return
  }
  const key = popupKey()
  showPopup.value = key ? localStorage.getItem(key) !== '1' : false
}

function close() {
  showPopup.value = false
  if (process.client) {
    const key = popupKey()
    if (key) localStorage.setItem(key, '1')
  }
}

function onAdClick() {
  close()
}

onMounted(() => loadAds())
watch(() => route.path, (to, from) => {
  if (to === from) return
  if (to === '/' || from === '/') loadAds()
  else syncPopup()
})
</script>
