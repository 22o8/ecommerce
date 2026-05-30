<template>
  <ClientOnly>
    <div v-if="enabled && hasRenderableAd" class="global-ads" :data-zone="zone">
      <section v-if="zone === 'top' && primaryTopAd" class="ad-container ad-container--hero">
        <div class="ad-hero-frame" :class="`ad-hero-frame--${primaryTopAd.type || 'banner'}`">
          <NuxtLink :to="safeLink(primaryTopAd.linkUrl)" class="ad-hero-frame__link">
            <Transition name="fade" mode="out-in">
              <component
                :is="mediaComponent(currentTopMedia)"
                :key="currentTopMedia"
                v-bind="mediaAttrs(currentTopMedia, primaryTopAd.title || 'advertisement')"
                class="ad-hero-frame__media"
              />
            </Transition>
            <div class="ad-hero-frame__shade" />
            <div v-if="primaryTopAd.title || primaryTopAd.subtitle" class="ad-hero-frame__content rtl-text">
              <span class="ad-hero-frame__eyebrow">إعلان</span>
              <b>{{ primaryTopAd.title }}</b>
              <small>{{ primaryTopAd.subtitle }}</small>
            </div>
          </NuxtLink>
          <div v-if="topMedia.length > 1" class="ad-slider__dots">
            <button
              v-for="(_, idx) in topMedia"
              :key="idx"
              type="button"
              :class="idx === sliderIndex ? 'is-active' : ''"
              @click="sliderIndex = idx"
            />
          </div>
        </div>
      </section>

      <section v-if="zone === 'bottom' && primaryBottomAd" class="ad-container ad-container--bottom">
        <div class="ad-bottom-frame">
          <NuxtLink :to="safeLink(primaryBottomAd.linkUrl)" class="ad-bottom-frame__link">
            <component
              :is="mediaComponent(currentBottomMedia)"
              v-bind="mediaAttrs(currentBottomMedia, primaryBottomAd.title || 'advertisement')"
              class="ad-bottom-frame__media"
            />
            <div v-if="primaryBottomAd.title || primaryBottomAd.subtitle" class="ad-bottom-frame__content rtl-text">
              <b>{{ primaryBottomAd.title }}</b>
              <span>{{ primaryBottomAd.subtitle }}</span>
            </div>
          </NuxtLink>
        </div>
      </section>

      <Transition name="ad-notification">
        <div v-if="zone === 'top' && notificationAd && showNotification" class="ad-notification-wrap">
          <NuxtLink :to="safeLink(notificationAd.linkUrl)" class="ad-notification rtl-text" @click="closeNotification">
            <span class="ad-notification__icon"><Icon name="mdi:bell-ring-outline" /></span>
            <span class="ad-notification__body">
              <b>{{ notificationAd.title || 'تنبيه جديد' }}</b>
              <small>{{ notificationAd.subtitle || 'لديك عرض جديد داخل المتجر.' }}</small>
            </span>
          </NuxtLink>
          <button type="button" class="ad-notification__close" aria-label="close" @click="closeNotification">×</button>
        </div>
      </Transition>

      <div v-if="zone === 'top' && popupAd && showPopup" class="fixed inset-0 z-[70] flex items-center justify-center p-4">
        <div class="absolute inset-0 bg-black/60 backdrop-blur-sm" @click="close" />
        <div class="ad-popup relative w-full max-w-[620px] overflow-hidden rounded-[2rem] border border-white/15 bg-surface shadow-2xl">
          <button class="ad-popup__close" type="button" @click="close" aria-label="close">✕</button>
          <NuxtLink :to="safeLink(popupAd.linkUrl)" class="block" @click="close">
            <component
              :is="mediaComponent(firstMedia(popupAd))"
              v-bind="mediaAttrs(firstMedia(popupAd), popupAd.title || 'popup')"
              class="ad-popup__media"
            />
            <div v-if="popupAd.title || popupAd.subtitle" class="ad-popup__body rtl-text">
              <b>{{ popupAd.title }}</b>
              <span>{{ popupAd.subtitle }}</span>
            </div>
          </NuxtLink>
        </div>
      </div>
    </div>
  </ClientOnly>
</template>

<script setup lang="ts">
const props = withDefaults(defineProps<{ zone?: 'top' | 'bottom' }>(), { zone: 'top' })
const zone = computed(() => props.zone)
const route = useRoute()
const api = useApi()

const enabled = computed(() => !route.path.startsWith('/admin'))
const ads = ref<any[]>([])
const showPopup = ref(false)
const showNotification = ref(false)
const loadingKey = ref(0)
const sliderIndex = ref(0)
let timer: ReturnType<typeof setInterval> | null = null

const isHome = computed(() => route.path === '/')
const norm = (v: any) => String(v || '').trim().toLowerCase()
const hasMedia = (ad: any) => mediaList(ad).length > 0

function adScore(ad: any, placements: string[]) {
  const p = norm(ad?.placement)
  const idx = placements.indexOf(p)
  return idx < 0 ? 999 : idx
}
function findAd(types: string[], placements: string[]) {
  const typeSet = new Set(types.map(norm))
  const placeSet = new Set(placements.map(norm))
  return [...ads.value]
    .filter((a: any) => typeSet.has(norm(a?.type)) && placeSet.has(norm(a?.placement)) && hasMedia(a))
    .sort((a: any, b: any) => adScore(a, placements.map(norm)) - adScore(b, placements.map(norm)) || Number(a?.sortOrder || 0) - Number(b?.sortOrder || 0))[0] || null
}

const topPlacements = computed(() => isHome.value
  ? ['home_hero_slider', 'home_hero_top', 'home_top_slider', 'home_top', 'hero_top', 'top']
  : ['page_top_slider', 'page_top', 'top']
)
const bottomPlacements = computed(() => isHome.value
  ? ['home_bottom_slider', 'home_bottom', 'bottom']
  : ['page_bottom_slider', 'page_bottom', 'bottom']
)

const topSlider = computed(() => findAd(['slider'], topPlacements.value))
const topBanner = computed(() => findAd(['banner'], topPlacements.value))
const primaryTopAd = computed(() => topSlider.value || topBanner.value)
const bottomSlider = computed(() => findAd(['slider'], bottomPlacements.value))
const bottomBanner = computed(() => findAd(['banner'], bottomPlacements.value))
const primaryBottomAd = computed(() => bottomSlider.value || bottomBanner.value)
const popupAd = computed(() => findAd(['popup'], ['popup', 'site_popup', 'home_popup']))
const notificationAd = computed(() => findAd(['notification'], isHome.value ? ['home_notification', 'site_notification', 'notification'] : ['site_notification', 'notification']))
const hasRenderableAd = computed(() => Boolean(primaryTopAd.value || primaryBottomAd.value || popupAd.value || notificationAd.value))

function mediaList(ad: any) {
  const arr = Array.isArray(ad?.imageUrls) ? ad.imageUrls.filter(Boolean) : []
  const merged = arr.length ? arr : (ad?.imageUrl ? [ad.imageUrl] : [])
  return merged.map((x: any) => String(x || '').trim()).filter(Boolean)
}
function firstMedia(ad: any) { return mediaList(ad)[0] || '' }
const topMedia = computed(() => primaryTopAd.value ? mediaList(primaryTopAd.value) : [])
const currentTopMedia = computed(() => topMedia.value[sliderIndex.value] || topMedia.value[0] || '')
const bottomMedia = computed(() => primaryBottomAd.value ? mediaList(primaryBottomAd.value) : [])
const currentBottomMedia = computed(() => bottomMedia.value[0] || '')

function isVideo(url?: string) {
  const u = String(url || '').split('?')[0].toLowerCase()
  return /\.(mp4|webm|ogg|mov)$/i.test(u)
}
function mediaComponent(url?: string) { return isVideo(url) ? 'video' : 'img' }
function mediaAttrs(path?: string, alt?: string) {
  const src = asset(path)
  if (isVideo(src)) return { src, autoplay: true, muted: true, loop: true, playsinline: true }
  return { src, alt: alt || 'advertisement', loading: 'eager' }
}
const asset = (p?: string) => {
  const url = api.buildAssetUrl(p || '')
  if (!url) return ''
  const sep = url.includes('?') ? '&' : '?'
  return `${url}${sep}v=${encodeURIComponent(String(loadingKey.value || '1'))}`
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
  if (topMedia.value.length <= 1) return
  timer = setInterval(() => {
    sliderIndex.value = (sliderIndex.value + 1) % topMedia.value.length
  }, 5200)
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
  showNotification.value = Boolean(process.client && zone.value === 'top' && notificationAd.value)
  startTimer()
}
function close() { showPopup.value = false }
function closeNotification() { showNotification.value = false }

onMounted(() => {
  loadAds()
  if (process.client) window.addEventListener('ads:changed', loadAds)
})
watch(() => route.fullPath, () => loadAds())
watch(topMedia, () => { sliderIndex.value = 0; startTimer() })
onBeforeUnmount(() => {
  stopTimer()
  if (process.client) window.removeEventListener('ads:changed', loadAds)
})
</script>

<style scoped>
.global-ads{ position:relative; z-index:4; }
.ad-container{ width:min(100%, 96rem); margin-inline:auto; padding-inline:clamp(.85rem,2vw,1.5rem); }
.ad-container--hero{ padding-top:clamp(.7rem,1.4vw,1.15rem); padding-bottom:clamp(.25rem,.8vw,.65rem); }
.ad-container--bottom{ padding-block:clamp(1rem,2vw,1.4rem); }
.ad-hero-frame,.ad-bottom-frame{ position:relative; overflow:hidden; border:1px solid rgba(var(--border),.78); background:linear-gradient(135deg, rgba(var(--surface-rgb),.92), rgba(var(--surface-2-rgb),.72)); box-shadow:0 22px 70px rgba(0,0,0,.18); }
.ad-hero-frame{ border-radius:clamp(1.35rem,2.2vw,2.4rem); min-height:clamp(180px,23vw,360px); }
.ad-bottom-frame{ border-radius:2rem; }
.ad-hero-frame__link,.ad-bottom-frame__link{ display:block; position:relative; min-height:inherit; color:inherit; }
.ad-hero-frame__media,.ad-bottom-frame__media{ display:block; width:100%; object-fit:cover; background:rgb(var(--surface-2)); }
.ad-hero-frame__media{ height:clamp(190px,24vw,380px); }
.ad-bottom-frame__media{ height:clamp(150px,18vw,270px); }
.ad-hero-frame__shade{ pointer-events:none; position:absolute; inset:0; background:linear-gradient(90deg, rgba(0,0,0,.66), rgba(0,0,0,.24) 44%, rgba(0,0,0,.08)); }
:global(html[dir="rtl"]) .ad-hero-frame__shade{ background:linear-gradient(270deg, rgba(0,0,0,.66), rgba(0,0,0,.24) 44%, rgba(0,0,0,.08)); }
.ad-hero-frame__content{ position:absolute; inset-block:auto 1.25rem; inset-inline:1.25rem auto; display:grid; gap:.35rem; max-width:min(620px, calc(100% - 2.5rem)); color:white; }
:global(html[dir="rtl"]) .ad-hero-frame__content{ inset-inline:auto 1.25rem; }
.ad-hero-frame__eyebrow{ width:max-content; border:1px solid rgba(255,255,255,.22); background:rgba(255,255,255,.12); backdrop-filter:blur(12px); border-radius:999px; padding:.35rem .7rem; font-size:.74rem; font-weight:1000; }
.ad-hero-frame__content b{ font-size:clamp(1.4rem,3vw,3.1rem); font-weight:1000; line-height:1.05; text-shadow:0 16px 48px rgba(0,0,0,.35); }
.ad-hero-frame__content small{ max-width:520px; font-size:clamp(.86rem,1.1vw,1.05rem); color:rgba(255,255,255,.82); line-height:1.8; }
.ad-bottom-frame__content{ position:absolute; inset-inline:1rem; bottom:1rem; display:grid; gap:.25rem; width:max-content; max-width:min(520px, calc(100% - 2rem)); border:1px solid rgba(255,255,255,.16); border-radius:1.35rem; padding:.85rem 1rem; color:white; background:rgba(5,5,9,.48); backdrop-filter:blur(16px); }
.ad-bottom-frame__content b{ font-size:1.1rem; font-weight:1000; }
.ad-bottom-frame__content span{ font-size:.86rem; color:rgba(255,255,255,.78); }
.ad-slider__dots{ position:absolute; inset-inline:0; bottom:.8rem; display:flex; align-items:center; justify-content:center; gap:.4rem; z-index:5; }
.ad-slider__dots button{ width:.58rem; height:.58rem; border-radius:999px; background:rgba(255,255,255,.55); transition:.2s ease; }
.ad-slider__dots button.is-active{ width:2.3rem; background:white; }
 .ad-notification-wrap{ position:fixed; inset-inline-end:clamp(.85rem,2vw,1.35rem); top:5.6rem; z-index:68; display:flex; align-items:center; gap:.5rem; max-width:min(420px, calc(100vw - 1.5rem)); }
.ad-notification{ display:flex; align-items:center; gap:.8rem; min-width:280px; max-width:100%; border:1px solid rgba(var(--primary),.32); background:linear-gradient(135deg, rgba(var(--surface-rgb),.96), rgba(var(--surface-2-rgb),.92)); color:rgb(var(--text)); border-radius:1.35rem; padding:.85rem 1rem; box-shadow:0 24px 70px rgba(0,0,0,.24), 0 0 0 1px rgba(255,255,255,.04) inset; backdrop-filter:blur(18px); text-decoration:none; }
.ad-notification__icon{ width:42px; height:42px; border-radius:16px; display:grid; place-items:center; flex:0 0 auto; background:rgb(var(--primary)); color:#06050a; font-size:1.35rem; box-shadow:0 14px 30px rgba(var(--primary),.25); }
.ad-notification__body{ display:grid; gap:.15rem; min-width:0; }
.ad-notification__body b{ font-size:.95rem; font-weight:1000; overflow:hidden; text-overflow:ellipsis; white-space:nowrap; }
.ad-notification__body small{ color:rgb(var(--muted)); line-height:1.5; display:-webkit-box; -webkit-line-clamp:2; -webkit-box-orient:vertical; overflow:hidden; }
.ad-notification__close{ width:2.25rem; height:2.25rem; border-radius:999px; background:rgba(var(--surface-rgb),.86); border:1px solid rgba(var(--border),.75); color:rgb(var(--text)); font-size:1.25rem; font-weight:900; box-shadow:0 10px 26px rgba(0,0,0,.12); }
.ad-notification-enter-active,.ad-notification-leave-active{ transition:opacity .24s ease, transform .24s ease; }
.ad-notification-enter-from,.ad-notification-leave-to{ opacity:0; transform:translateY(-12px) scale(.97); }

.ad-popup{ animation:popupIn .28s ease both; }
.ad-popup__media{ display:block; width:100%; max-height:72vh; object-fit:cover; }
.ad-popup__close{ position:absolute; z-index:3; inset-inline-start:.8rem; top:.8rem; width:2.6rem; height:2.6rem; border-radius:999px; background:rgba(0,0,0,.52); color:white; font-weight:900; }
.ad-popup__body{ padding:1rem 1.25rem; display:grid; gap:.25rem; }
.ad-popup__body b{ color:rgb(var(--text)); font-size:1.2rem; font-weight:1000; }
.ad-popup__body span{ color:rgb(var(--muted)); }
@keyframes popupIn{ from{ opacity:0; transform:translateY(18px) scale(.98); } to{ opacity:1; transform:none; } }
@media(max-width:640px){ .ad-notification-wrap{ inset-inline:.75rem; top:5.1rem; max-width:none; } .ad-notification{ flex:1; min-width:0; padding:.75rem .85rem; border-radius:1.1rem; } .ad-notification__icon{ width:38px; height:38px; border-radius:14px; } .ad-container{ padding-inline:.75rem; } .ad-hero-frame{ border-radius:1.35rem; min-height:168px; } .ad-hero-frame__media{ height:178px; } .ad-hero-frame__shade{ background:linear-gradient(0deg, rgba(0,0,0,.72), rgba(0,0,0,.10)); } .ad-hero-frame__content{ inset-inline:.9rem !important; bottom:.9rem; max-width:calc(100% - 1.8rem); } .ad-hero-frame__content b{ font-size:1.45rem; } .ad-hero-frame__content small{ font-size:.82rem; } .ad-bottom-frame{ border-radius:1.35rem; } .ad-bottom-frame__media{ height:160px; } .ad-bottom-frame__content{ inset-inline:.65rem; bottom:.65rem; border-radius:1rem; padding:.65rem .75rem; } }
</style>
