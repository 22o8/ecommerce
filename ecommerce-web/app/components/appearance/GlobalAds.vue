<template>
  <div v-if="enabled">
    <div v-if="route.path === '/' && homeSliderAd" class="mx-auto max-w-7xl px-4 pt-4">
      <div class="relative overflow-hidden rounded-3xl border border-white/10 bg-white/5 shadow-card">
        <a
          :href="homeSliderAd.linkUrl || '#'
          "
          :target="homeSliderAd.linkUrl ? '_blank' : undefined"
          class="block"
        >
          <transition name="fade" mode="out-in">
            <img
              :key="currentSliderImage"
              :src="asset(currentSliderImage, `${homeSliderAd.updatedAt || homeSliderAd.id}-${sliderIndex}`)"
              :alt="homeSliderAd.title || 'slider'"
              class="block h-[180px] w-full object-cover sm:h-[240px] lg:h-[300px]"
            />
          </transition>
        </a>

        <div v-if="sliderImages.length > 1" class="absolute inset-x-0 bottom-3 flex items-center justify-center gap-2 px-4">
          <button
            v-for="(_, idx) in sliderImages"
            :key="idx"
            type="button"
            class="h-2.5 rounded-full transition-all"
            :class="idx === sliderIndex ? 'w-8 bg-white' : 'w-2.5 bg-white/45'"
            @click="sliderIndex = idx"
          />
        </div>
      </div>
    </div>

    <div v-if="bannerAd && route.path === '/' && !homeSliderAd" class="mx-auto max-w-7xl px-4 pt-4">
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
          aria-label="close"
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
const route = useRoute()
const api = useApi()

const enabled = computed(() => !route.path.startsWith('/admin'))
const ads = ref<any[]>([])
const showPopup = ref(false)
const loadingKey = ref(0)
const sliderIndex = ref(0)
const sliderTimer = ref<any>(null)

const bannerAd = computed(() => ads.value.find((a: any) => a?.type === 'banner' && a?.placement === 'home_top'))
const popupAd = computed(() => ads.value.find((a: any) => a?.type === 'popup'))
const homeSliderAd = computed(() => ads.value.find((a: any) => a?.type === 'slider' && (a?.placement === 'home_top_slider' || a?.placement === 'home_top')))
const sliderImages = computed<string[]>(() => {
  const arr = homeSliderAd.value?.imageUrls
  if (Array.isArray(arr) && arr.length) return arr
  return homeSliderAd.value?.imageUrl ? [homeSliderAd.value.imageUrl] : []
})
const currentSliderImage = computed(() => sliderImages.value[sliderIndex.value] || '')

const asset = (p?: string, stamp?: any) => {
  const url = api.buildAssetUrl(p || '')
  if (!url) return ''
  const sep = url.includes('?') ? '&' : '?'
  const v = encodeURIComponent(String(stamp || loadingKey.value || '1'))
  return `${url}${sep}v=${v}`
}

function stopSlider() {
  if (sliderTimer.value) {
    clearInterval(sliderTimer.value)
    sliderTimer.value = null
  }
}

function startSlider() {
  stopSlider()
  if (sliderImages.value.length <= 1) return
  sliderTimer.value = setInterval(() => {
    sliderIndex.value = (sliderIndex.value + 1) % sliderImages.value.length
  }, 4200)
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
  sliderIndex.value = 0
  syncPopup()
  startSlider()
}

function syncPopup() {
  showPopup.value = !!(process.client && route.path === '/' && popupAd.value)
}

function close() {
  showPopup.value = false
}

function onAdClick() {
  close()
}

onMounted(() => {
  loadAds()
  if (process.client) window.addEventListener('ads:changed', loadAds)
})
watch(() => route.path, () => { loadAds() })
watch(sliderImages, () => {
  sliderIndex.value = 0
  startSlider()
})
onBeforeUnmount(() => {
  stopSlider()
  if (process.client) window.removeEventListener('ads:changed', loadAds)
})
</script>

<style scoped>
.fade-enter-active,.fade-leave-active{ transition: opacity .55s ease, transform .55s ease; }
.fade-enter-from,.fade-leave-to{ opacity:0; transform:scale(1.02); }
</style>
