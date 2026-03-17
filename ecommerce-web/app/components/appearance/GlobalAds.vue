<template>
  <div v-if="enabled">
    <div v-if="route.path === '/' && sliderAds.length" class="mx-auto max-w-7xl px-4 pt-4">
      <div class="ad-slider-shell">
        <div class="ad-slider-track" :style="trackStyle">
          <a
            v-for="(ad, index) in sliderAds"
            :key="`${ad.id}-${index}`"
            :href="ad.linkUrl || '#'"
            :target="ad.linkUrl ? '_blank' : undefined"
            class="ad-slide"
            @click="onAdClick"
          >
            <img
              :src="asset(ad.imageUrl, ad.updatedAt || ad.id)"
              :alt="ad.title || `slide-${index + 1}`"
              class="ad-slide__image"
              loading="eager"
            />
            <div v-if="ad.title || ad.subtitle" class="ad-slide__overlay">
              <div v-if="ad.title" class="ad-slide__title">{{ ad.title }}</div>
              <div v-if="ad.subtitle" class="ad-slide__subtitle">{{ ad.subtitle }}</div>
            </div>
          </a>
        </div>

        <button
          v-if="sliderAds.length > 1"
          type="button"
          class="ad-slider-nav ad-slider-nav--prev"
          aria-label="previous"
          @click="prevSlide"
        >
          ‹
        </button>
        <button
          v-if="sliderAds.length > 1"
          type="button"
          class="ad-slider-nav ad-slider-nav--next"
          aria-label="next"
          @click="nextSlide"
        >
          ›
        </button>

        <div v-if="sliderAds.length > 1" class="ad-slider-dots">
          <button
            v-for="(ad, index) in sliderAds"
            :key="`dot-${ad.id}-${index}`"
            type="button"
            class="ad-slider-dot"
            :class="{ 'is-active': currentSlide === index }"
            :aria-label="`slide ${index + 1}`"
            @click="goToSlide(index)"
          />
        </div>
      </div>
    </div>

    <div v-else-if="route.path === '/' && bannerAd" class="mx-auto max-w-7xl px-4 pt-4">
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
let sliderTimer: ReturnType<typeof setInterval> | null = null

const route = useRoute()
const api = useApi()

const enabled = computed(() => !route.path.startsWith('/admin'))
const ads = ref<any[]>([])
const showPopup = ref(false)
const loadingKey = ref(0)
const currentSlide = ref(0)

const sliderAds = computed(() => {
  return ads.value
    .filter((a: any) => a?.type === 'banner' && a?.placement === 'home_top_slider' && a?.imageUrl)
    .sort((a: any, b: any) => Number(a?.sortOrder || 0) - Number(b?.sortOrder || 0))
})
const bannerAd = computed(() => ads.value.find((a: any) => a?.type === 'banner' && a?.placement === 'home_top' && a?.imageUrl))
const popupAd = computed(() => ads.value.find((a: any) => a?.type === 'popup' && a?.imageUrl))

const trackStyle = computed(() => ({
  transform: `translateX(${currentSlide.value * -100}%)`,
}))

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
  restartSlider()
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

function goToSlide(index: number) {
  if (!sliderAds.value.length) return
  const max = sliderAds.value.length - 1
  currentSlide.value = Math.max(0, Math.min(index, max))
  restartSlider()
}

function nextSlide() {
  if (sliderAds.value.length <= 1) return
  currentSlide.value = (currentSlide.value + 1) % sliderAds.value.length
}

function prevSlide() {
  if (sliderAds.value.length <= 1) return
  currentSlide.value = (currentSlide.value - 1 + sliderAds.value.length) % sliderAds.value.length
  restartSlider()
}

function stopSlider() {
  if (sliderTimer) {
    clearInterval(sliderTimer)
    sliderTimer = null
  }
}

function restartSlider() {
  stopSlider()
  if (!process.client || route.path !== '/' || sliderAds.value.length <= 1) return
  sliderTimer = setInterval(() => {
    nextSlide()
  }, 4200)
}

onMounted(() => {
  loadAds()
  if (process.client) window.addEventListener('ads:changed', loadAds)
})
watch(() => route.path, () => {
  currentSlide.value = 0
  loadAds()
})
onBeforeUnmount(() => {
  stopSlider()
  if (process.client) window.removeEventListener('ads:changed', loadAds)
})
</script>

<style scoped>
.ad-slider-shell{
  position: relative;
  overflow: hidden;
  border-radius: 1.75rem;
  border: 1px solid rgba(255,255,255,.1);
  background: rgba(255,255,255,.04);
  box-shadow: 0 18px 45px rgba(0,0,0,.18);
}
.ad-slider-track{
  display: flex;
  width: 100%;
  transition: transform .75s cubic-bezier(.22,1,.36,1);
  will-change: transform;
}
.ad-slide{
  position: relative;
  min-width: 100%;
  display: block;
}
.ad-slide__image{
  display: block;
  width: 100%;
  height: clamp(190px, 30vw, 360px);
  object-fit: cover;
}
.ad-slide__overlay{
  position: absolute;
  inset-inline-start: 1rem;
  inset-inline-end: 1rem;
  bottom: 1rem;
  padding: .95rem 1.1rem;
  border-radius: 1.2rem;
  background: linear-gradient(180deg, rgba(6,10,20,.1), rgba(6,10,20,.72));
  backdrop-filter: blur(10px);
  color: white;
}
.ad-slide__title{
  font-size: 1rem;
  font-weight: 900;
}
.ad-slide__subtitle{
  margin-top: .25rem;
  font-size: .85rem;
  opacity: .92;
}
.ad-slider-nav{
  position: absolute;
  top: 50%;
  transform: translateY(-50%);
  z-index: 2;
  width: 2.75rem;
  height: 2.75rem;
  border: 0;
  border-radius: 999px;
  background: rgba(8,12,23,.52);
  color: white;
  backdrop-filter: blur(10px);
  box-shadow: 0 8px 24px rgba(0,0,0,.22);
  cursor: pointer;
}
.ad-slider-nav--prev{ inset-inline-start: .9rem; }
.ad-slider-nav--next{ inset-inline-end: .9rem; }
.ad-slider-dots{
  position: absolute;
  inset-inline: 0;
  bottom: .8rem;
  z-index: 2;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: .45rem;
}
.ad-slider-dot{
  width: .65rem;
  height: .65rem;
  border-radius: 999px;
  border: 0;
  background: rgba(255,255,255,.34);
  transition: all .25s ease;
}
.ad-slider-dot.is-active{
  width: 1.6rem;
  background: rgba(var(--primary), .95);
}
@media (max-width: 640px){
  .ad-slider-shell{ border-radius: 1.35rem; }
  .ad-slide__image{ height: 160px; }
  .ad-slide__overlay{ inset-inline-start: .7rem; inset-inline-end: .7rem; bottom: .7rem; padding: .75rem .85rem; }
  .ad-slide__title{ font-size: .92rem; }
  .ad-slide__subtitle{ font-size: .76rem; }
  .ad-slider-nav{ width: 2.35rem; height: 2.35rem; }
}
</style>
