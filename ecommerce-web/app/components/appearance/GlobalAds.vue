<template>
  <div v-if="enabled">
    <div v-if="route.path === '/' && bannerAd" class="mx-auto max-w-7xl px-4 pt-4">
      <a
        :href="bannerAd.linkUrl || '#'"
        :target="bannerAd.linkUrl ? '_blank' : undefined"
        class="block overflow-hidden rounded-3xl border border-white/10 bg-white/5 shadow-card"
      >
        <img :src="asset(bannerAd.imageUrl, bannerAd.updatedAt || bannerAd.id)" :alt="bannerAd.title || 'banner'" class="h-auto w-full object-cover" />
      </a>
    </div>

    <div v-if="popupAd && showPopup" class="fixed inset-0 z-[60] flex items-center justify-center p-4">
      <div class="absolute inset-0 bg-black/55" @click="close" />
      <div class="relative w-full max-w-[560px] overflow-hidden rounded-3xl border border-white/10 bg-white shadow-2xl dark:bg-zinc-950">
        <button
          class="absolute left-3 top-3 z-10 grid h-10 w-10 place-items-center rounded-full bg-black/40 text-white transition hover:bg-black/55"
          @click="close"
          :aria-label="t('common.close')"
        >✕</button>
        <a :href="popupAd.linkUrl || '#'" :target="popupAd.linkUrl ? '_blank' : undefined" class="block" @click="onAdClick">
          <img :src="asset(popupAd.imageUrl, popupAd.updatedAt || popupAd.id)" :alt="popupAd.title || 'ad'" class="h-auto w-full" />
        </a>
        <div v-if="popupAd.title" class="p-4 text-center font-semibold text-zinc-900 dark:text-zinc-100">{{ popupAd.title }}</div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
const { t } = useI18n()
const route = useRoute()
const api = useApi()

const enabled = computed(() => !route.path.startsWith('/admin'))
const ads = ref<any[]>([])
const showPopup = ref(false)
const loadingKey = ref(0)

const bannerAd = computed(() => ads.value.find((a: any) => a?.type === 'banner' && a?.placement === 'home_top'))
const popupAd = computed(() => ads.value.find((a: any) => a?.type === 'popup'))

const asset = (p?: string, stamp?: any) => {
  const url = api.buildAssetUrl(p || '')
  if (!url) return ''
  const sep = url.includes('?') ? '&' : '?'
  const v = encodeURIComponent(String(stamp || loadingKey.value || '1'))
  return `${url}${sep}v=${v}`
}

async function loadAds() {
  if (!enabled.value) return
  loadingKey.value = Date.now()
  try {
    const res: any = await $fetch('/api/bff/ads/active', {
      method: 'GET',
      query: { _ts: loadingKey.value },
      headers: { 'cache-control': 'no-cache, no-store, must-revalidate', pragma: 'no-cache' },
    })
    ads.value = Array.isArray(res) ? res : (Array.isArray(res?.items) ? res.items : [])
  } catch {
    ads.value = []
  }
  syncPopup()
}

function popupKey() {
  return popupAd.value ? `popup_seen_${popupAd.value.id}_${popupAd.value.updatedAt || ''}` : ''
}

function syncPopup() {
  if (!process.client || route.path !== '/' || !popupAd.value) {
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

onMounted(() => {
  loadAds()
  if (process.client) window.addEventListener('ads:changed', loadAds)
})
watch(() => route.path, () => { loadAds() })
onBeforeUnmount(() => {
  if (process.client) window.removeEventListener('ads:changed', loadAds)
})
</script>
