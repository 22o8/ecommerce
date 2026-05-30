<template>
  <div v-if="enabled" class="global-ads" :data-zone="zone">
    <section v-if="zone === 'top' && topSlider" class="ad-container ad-container--top">
      <div class="ad-slider relative overflow-hidden rounded-[2rem] border border-app bg-surface shadow-soft">
        <NuxtLink :to="safeLink(topSlider.linkUrl)" class="block">
          <Transition name="fade" mode="out-in">
            <img
              :key="currentTopImage"
              :src="asset(currentTopImage, `${topSlider.updatedAt || topSlider.id}-${sliderIndex}`)"
              :alt="topSlider.title || 'slider'"
              class="ad-slider__image"
            />
          </Transition>
          <div v-if="topSlider.title || topSlider.subtitle" class="ad-slider__caption rtl-text">
            <b>{{ topSlider.title }}</b>
            <span>{{ topSlider.subtitle }}</span>
          </div>
        </NuxtLink>
        <div v-if="topSliderImages.length > 1" class="ad-slider__dots">
          <button
            v-for="(_, idx) in topSliderImages"
            :key="idx"
            type="button"
            :class="idx === sliderIndex ? 'is-active' : ''"
            @click="sliderIndex = idx"
          />
        </div>
      </div>
    </section>

    <section v-else-if="zone === 'top' && topBanner" class="ad-container ad-container--top">
      <NuxtLink :to="safeLink(topBanner.linkUrl)" class="ad-banner">
        <img :src="asset(topBanner.imageUrl, topBanner.updatedAt || topBanner.id)" :alt="topBanner.title || 'banner'" />
        <div v-if="topBanner.title || topBanner.subtitle" class="ad-banner__content rtl-text">
          <b>{{ topBanner.title }}</b>
          <span>{{ topBanner.subtitle }}</span>
        </div>
      </NuxtLink>
    </section>

    <section v-if="zone === 'bottom' && bottomSlider" class="ad-container ad-container--bottom">
      <div class="ad-slider relative overflow-hidden rounded-[2rem] border border-app bg-surface shadow-soft">
        <NuxtLink :to="safeLink(bottomSlider.linkUrl)" class="block">
          <img :src="asset(bottomSliderImage, bottomSlider.updatedAt || bottomSlider.id)" :alt="bottomSlider.title || 'slider'" class="ad-slider__image" />
          <div v-if="bottomSlider.title || bottomSlider.subtitle" class="ad-slider__caption rtl-text">
            <b>{{ bottomSlider.title }}</b>
            <span>{{ bottomSlider.subtitle }}</span>
          </div>
        </NuxtLink>
      </div>
    </section>

    <section v-else-if="zone === 'bottom' && bottomBanner" class="ad-container ad-container--bottom">
      <NuxtLink :to="safeLink(bottomBanner.linkUrl)" class="ad-banner">
        <img :src="asset(bottomBanner.imageUrl, bottomBanner.updatedAt || bottomBanner.id)" :alt="bottomBanner.title || 'banner'" />
        <div v-if="bottomBanner.title || bottomBanner.subtitle" class="ad-banner__content rtl-text">
          <b>{{ bottomBanner.title }}</b>
          <span>{{ bottomBanner.subtitle }}</span>
        </div>
      </NuxtLink>
    </section>

    <div v-if="zone === 'top' && popupAd && showPopup" class="fixed inset-0 z-[70] flex items-center justify-center p-4">
      <div class="absolute inset-0 bg-black/60 backdrop-blur-sm" @click="close" />
      <div class="ad-popup relative w-full max-w-[620px] overflow-hidden rounded-[2rem] border border-white/15 bg-surface shadow-2xl">
        <button class="ad-popup__close" type="button" @click="close" aria-label="close">✕</button>
        <NuxtLink :to="safeLink(popupAd.linkUrl)" class="block" @click="close">
          <img :src="asset(popupAd.imageUrl, popupAd.updatedAt || popupAd.id)" :alt="popupAd.title || 'popup'" />
          <div v-if="popupAd.title || popupAd.subtitle" class="ad-popup__body rtl-text">
            <b>{{ popupAd.title }}</b>
            <span>{{ popupAd.subtitle }}</span>
          </div>
        </NuxtLink>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
const props = withDefaults(defineProps<{ zone?: 'top' | 'bottom' }>(), { zone: 'top' })
const zone = computed(() => props.zone)
const route = useRoute()
const api = useApi()

const enabled = computed(() => !route.path.startsWith('/admin'))
const ads = ref<any[]>([])
const showPopup = ref(false)
const loadingKey = ref(0)
const sliderIndex = ref(0)
let timer: ReturnType<typeof setInterval> | null = null

const isHome = computed(() => route.path === '/')
const findAd = (types: string[], placements: string[]) => ads.value.find((a: any) => types.includes(String(a?.type || '').toLowerCase()) && placements.includes(String(a?.placement || '')))

const topSlider = computed(() => isHome.value
  ? findAd(['slider'], ['home_top_slider', 'home_top'])
  : findAd(['slider'], ['page_top_slider', 'page_top'])
)
const topBanner = computed(() => isHome.value
  ? findAd(['banner'], ['home_top'])
  : findAd(['banner'], ['page_top'])
)
const bottomSlider = computed(() => isHome.value
  ? findAd(['slider'], ['home_bottom_slider', 'home_bottom'])
  : findAd(['slider'], ['page_bottom_slider', 'page_bottom'])
)
const bottomBanner = computed(() => isHome.value
  ? findAd(['banner'], ['home_bottom'])
  : findAd(['banner'], ['page_bottom'])
)
const popupAd = computed(() => findAd(['popup'], ['popup', 'site_popup']))

function imageList(ad: any) {
  const arr = Array.isArray(ad?.imageUrls) ? ad.imageUrls.filter(Boolean) : []
  return arr.length ? arr : (ad?.imageUrl ? [ad.imageUrl] : [])
}
const topSliderImages = computed(() => imageList(topSlider.value))
const currentTopImage = computed(() => topSliderImages.value[sliderIndex.value] || '')
const bottomSliderImages = computed(() => imageList(bottomSlider.value))
const bottomSliderImage = computed(() => bottomSliderImages.value[0] || '')

const asset = (p?: string, stamp?: any) => {
  const url = api.buildAssetUrl(p || '')
  if (!url) return ''
  const sep = url.includes('?') ? '&' : '?'
  return `${url}${sep}v=${encodeURIComponent(String(stamp || loadingKey.value || '1'))}`
}
function safeLink(link?: string) {
  const v = String(link || '#').trim()
  return v || '#'
}
function stopTimer() {
  if (timer) clearInterval(timer)
  timer = null
}
function startTimer() {
  stopTimer()
  if (topSliderImages.value.length <= 1) return
  timer = setInterval(() => {
    sliderIndex.value = (sliderIndex.value + 1) % topSliderImages.value.length
  }, 4500)
}
async function loadAds() {
  if (!enabled.value) return
  loadingKey.value = Date.now()
  try {
    const res: any = await $fetch('/api/bff/ads/active', {
      query: { _ts: loadingKey.value },
      headers: { 'cache-control': 'no-cache, no-store, must-revalidate', pragma: 'no-cache' },
    })
    ads.value = Array.isArray(res) ? res : (Array.isArray(res?.items) ? res.items : [])
  } catch { ads.value = [] }
  sliderIndex.value = 0
  showPopup.value = Boolean(process.client && zone.value === 'top' && popupAd.value)
  startTimer()
}
function close() { showPopup.value = false }

onMounted(() => {
  loadAds()
  if (process.client) window.addEventListener('ads:changed', loadAds)
})
watch(() => route.fullPath, () => loadAds())
watch(topSliderImages, () => { sliderIndex.value = 0; startTimer() })
onBeforeUnmount(() => {
  stopTimer()
  if (process.client) window.removeEventListener('ads:changed', loadAds)
})
</script>

<style scoped>
.ad-container{ width:min(100%, 92rem); margin-inline:auto; padding-inline:1rem; }
.ad-container--top{ padding-top:1rem; }
.ad-container--bottom{ padding-block:1rem; }
.ad-banner{ position:relative; display:block; overflow:hidden; border-radius:2rem; border:1px solid rgba(var(--border),.8); background:rgb(var(--surface)); box-shadow:var(--shadow-soft); }
.ad-banner img{ display:block; width:100%; max-height:280px; object-fit:cover; }
.ad-banner__content,.ad-slider__caption{ position:absolute; inset-inline:1rem; bottom:1rem; display:grid; gap:.25rem; width:max-content; max-width:min(520px, calc(100% - 2rem)); border:1px solid rgba(255,255,255,.16); border-radius:1.35rem; padding:.85rem 1rem; color:white; background:rgba(5,5,9,.48); backdrop-filter:blur(16px); }
.ad-banner__content b,.ad-slider__caption b{ font-size:1.1rem; font-weight:1000; }
.ad-banner__content span,.ad-slider__caption span{ font-size:.86rem; color:rgba(255,255,255,.78); }
.ad-slider{ min-height:170px; }
.ad-slider__image{ display:block; width:100%; height:clamp(180px, 22vw, 340px); object-fit:cover; }
.ad-slider__dots{ position:absolute; inset-inline:0; bottom:.8rem; display:flex; align-items:center; justify-content:center; gap:.4rem; }
.ad-slider__dots button{ width:.6rem; height:.6rem; border-radius:999px; background:rgba(255,255,255,.45); transition:.2s ease; }
.ad-slider__dots button.is-active{ width:2.2rem; background:white; }
.ad-popup{ animation:popupIn .28s ease both; }
.ad-popup > a > img{ display:block; width:100%; max-height:72vh; object-fit:cover; }
.ad-popup__close{ position:absolute; z-index:3; inset-inline-start:.8rem; top:.8rem; width:2.6rem; height:2.6rem; border-radius:999px; background:rgba(0,0,0,.52); color:white; font-weight:900; }
.ad-popup__body{ padding:1rem 1.25rem; display:grid; gap:.25rem; }
.ad-popup__body b{ color:rgb(var(--text)); font-size:1.2rem; font-weight:1000; }
.ad-popup__body span{ color:rgb(var(--muted)); }
@keyframes popupIn{ from{ opacity:0; transform:translateY(18px) scale(.98); } to{ opacity:1; transform:none; } }
@media(max-width:640px){ .ad-container{ padding-inline:.75rem; } .ad-banner,.ad-slider{ border-radius:1.35rem; } .ad-slider__image{ height:165px; } .ad-banner__content,.ad-slider__caption{ inset-inline:.65rem; bottom:.65rem; border-radius:1rem; padding:.65rem .75rem; } }
</style>
