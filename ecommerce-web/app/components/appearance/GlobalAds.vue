<template>
  <ClientOnly>
    <div v-if="enabled && hasInlineAd" class="global-ads" :data-zone="zone">
      <section v-if="zone === 'top' && currentTopSlide" class="ad-container ad-container--hero">
        <div class="ad-hero-frame" :class="`ad-hero-frame--${currentTopSlide.type || 'slider'}`">
          <button
            v-if="topSlides.length > 1 && previousTopSlide"
            type="button"
            class="ad-hero-frame__peek ad-hero-frame__peek--prev"
            aria-label="الإعلان السابق"
            @mouseenter="goToPreviousSlide"
            @click="goToPreviousSlide"
          >
            <component
              :is="mediaComponent(previousTopSlide.media)"
              v-bind="mediaAttrs(previousTopSlide.media, previousTopSlide.title || 'advertisement')"
              class="ad-hero-frame__peek-media"
            />
          </button>

          <button
            v-if="topSlides.length > 1 && nextTopSlide"
            type="button"
            class="ad-hero-frame__peek ad-hero-frame__peek--next"
            aria-label="الإعلان التالي"
            @mouseenter="goToNextSlide"
            @click="goToNextSlide"
          >
            <component
              :is="mediaComponent(nextTopSlide.media)"
              v-bind="mediaAttrs(nextTopSlide.media, nextTopSlide.title || 'advertisement')"
              class="ad-hero-frame__peek-media"
            />
          </button>

          <button type="button" class="ad-hero-frame__link" :aria-label="currentTopSlide.title || 'فتح الإعلان'" @click="handleSlideClick(currentTopSlide)">
            <div class="ad-hero-frame__media-layer">
              <component
                v-for="(slide, idx) in topSlides"
                :is="mediaComponent(slide.media)"
                :key="slide.key"
                v-bind="mediaAttrs(slide.media, slide.title || 'advertisement')"
                class="ad-hero-frame__media"
                :class="{ 'is-active': idx === sliderIndex }"
              />
            </div>
            <div class="ad-hero-frame__shade" />
            <div v-if="currentTopSlide.title || currentTopSlide.subtitle" class="ad-hero-frame__content rtl-text">
              <span class="ad-hero-frame__eyebrow">إعلان</span>
              <b>{{ currentTopSlide.title }}</b>
              <small v-if="currentTopSlide.subtitle">{{ currentTopSlide.subtitle }}</small>
            </div>
          </button>

          <button
            v-if="topSlides.length > 1"
            type="button"
            class="ad-slider__nav ad-slider__nav--prev"
            aria-label="الإعلان السابق"
            @click="goToPreviousSlide"
          >‹</button>
          <button
            v-if="topSlides.length > 1"
            type="button"
            class="ad-slider__nav ad-slider__nav--next"
            aria-label="الإعلان التالي"
            @mouseenter="goToNextSlide"
            @click="goToNextSlide"
          >›</button>

          <div v-if="topSlides.length > 1" class="ad-slider__dots">
            <button
              v-for="(_, idx) in topSlides"
              :key="idx"
              type="button"
              :class="idx === sliderIndex ? 'is-active' : ''"
              @click="setSlide(idx); startTimer()"
              :aria-label="`slide ${idx + 1}`"
            />
          </div>
        </div>
      </section>

      <section v-if="zone === 'bottom' && currentBottomSlide" class="ad-container ad-container--bottom">
        <div class="ad-bottom-frame">
          <button type="button" class="ad-bottom-frame__link" :aria-label="currentBottomSlide.title || 'فتح الإعلان'" @click="handleSlideClick(currentBottomSlide)">
            <component
              :is="mediaComponent(currentBottomSlide.media)"
              v-bind="mediaAttrs(currentBottomSlide.media, currentBottomSlide.title || 'advertisement')"
              class="ad-bottom-frame__media"
            />
            <div v-if="isVideo(currentBottomSlide.media)" class="ad-bottom-frame__video-hint">
              ▶ اضغط لتشغيل الفيديو بالصوت
            </div>
            <div v-if="currentBottomSlide.title || currentBottomSlide.subtitle" class="ad-bottom-frame__content rtl-text">
              <b>{{ currentBottomSlide.title }}</b>
              <span v-if="currentBottomSlide.subtitle">{{ currentBottomSlide.subtitle }}</span>
            </div>
          </button>

          <button
            v-if="bottomSlides.length > 1"
            type="button"
            class="ad-slider__nav ad-slider__nav--prev"
            aria-label="الإعلان السابق"
            @click="goToPreviousBottomSlide"
          >‹</button>
          <button
            v-if="bottomSlides.length > 1"
            type="button"
            class="ad-slider__nav ad-slider__nav--next"
            aria-label="الإعلان التالي"
            @click="goToNextBottomSlide"
          >›</button>

          <div v-if="bottomSlides.length > 1" class="ad-slider__dots">
            <button
              v-for="(_, idx) in bottomSlides"
              :key="idx"
              type="button"
              :class="idx === bottomSliderIndex ? 'is-active' : ''"
              @click="setBottomSlide(idx); startBottomTimer()"
              :aria-label="`bottom slide ${idx + 1}`"
            />
          </div>
        </div>
      </section>
    </div>

    <Teleport to="body">
      <Transition name="ad-popup-fade">
        <div
          v-if="enabled && isHome && zone === 'top' && popupAd && showPopup"
          class="ad-popup-layer"
          role="dialog"
          aria-modal="true"
          :aria-label="popupAd.title || 'إعلان خاص'"
        >
          <button class="ad-popup-layer__overlay" type="button" aria-label="إغلاق الإعلان" @click="close" />
          <article class="ad-popup-card rtl-text">
            <button class="ad-popup__close" type="button" @click="close" aria-label="إغلاق">✕</button>

            <NuxtLink
              :to="safeLink(popupAd.linkUrl)"
              class="ad-popup-card__link"
              @click="close"
            >
              <div v-if="firstMedia(popupAd)" class="ad-popup-card__media-wrap">
                <component
                  :is="mediaComponent(firstMedia(popupAd))"
                  v-bind="mediaAttrs(firstMedia(popupAd), popupAd.title || 'popup')"
                  class="ad-popup-card__media"
                />
              </div>

              <div class="ad-popup-card__content" :class="{ 'is-text-only': !firstMedia(popupAd) }">
                <span class="ad-popup__badge">إعلان خاص</span>
                <b>{{ popupAd.title || 'عرض خاص لك' }}</b>
                <p>{{ popupAd.subtitle || 'اكتشف أحدث العروض المختارة داخل المتجر.' }}</p>
                <strong v-if="popupAd.linkUrl" class="ad-popup__cta">اضغط للمتابعة</strong>
              </div>
            </NuxtLink>
          </article>
        </div>
      </Transition>
    </Teleport>
    <Teleport to="body">
      <Transition name="ad-popup-fade">
        <div v-if="activeVideoUrl" class="ad-video-modal" role="dialog" aria-modal="true" aria-label="مشاهدة فيديو الإعلان">
          <button class="ad-video-modal__overlay" type="button" aria-label="إغلاق الفيديو" @click="closeVideoModal" />
          <article class="ad-video-modal__card">
            <button class="ad-video-modal__close" type="button" aria-label="إغلاق" @click="closeVideoModal">✕</button>
            <video
              :key="activeVideoUrl"
              :src="activeVideoUrl"
              class="ad-video-modal__video"
              controls
              autoplay
              playsinline
              preload="metadata"
              controlslist="nodownload noplaybackrate noremoteplayback"
            />
          </article>
        </div>
      </Transition>
    </Teleport>
  </ClientOnly>
</template>

<script setup lang="ts">
const props = withDefaults(defineProps<{ zone?: 'top' | 'bottom' }>(), { zone: 'top' })
const zone = computed(() => props.zone)
const route = useRoute()
const api = useApi()

const enabled = computed(() => !route.path.startsWith('/admin'))
const ads = useState<any[]>('public-global-ads', () => [])
const showPopup = ref(false)
const loadingKey = ref(0)
const sliderIndex = ref(0)
const bottomSliderIndex = ref(0)
const activeVideoUrl = ref('')
let timer: ReturnType<typeof setInterval> | null = null
let bottomTimer: ReturnType<typeof setInterval> | null = null
let popupTimer: ReturnType<typeof setTimeout> | null = null

const isHome = computed(() => route.path === '/')
const norm = (v: any) => String(v ?? '').trim().toLowerCase()
const hasMedia = (ad: any) => mediaList(ad).length > 0
const enabledAds = computed(() => ads.value.filter((ad: any) => (ad?.isEnabled ?? ad?.IsEnabled) !== false))

function mediaList(ad: any) {
  const raw = ad?.imageUrls ?? ad?.ImageUrls
  const arr = Array.isArray(raw) ? raw : (Array.isArray(raw?.$values) ? raw.$values : [])
  const merged = arr.length ? arr : ((ad?.imageUrl || ad?.ImageUrl) ? [ad?.imageUrl || ad?.ImageUrl] : [])
  return merged.map((x: any) => String(x || '').trim()).filter(Boolean)
}
function firstMedia(ad: any) { return mediaList(ad)[0] || '' }
const hasText = (ad: any) => Boolean(String(ad?.title || ad?.Title || ad?.subtitle || ad?.Subtitle || '').trim())
const isRenderable = (ad: any, requireMedia = true) => !requireMedia || hasMedia(ad) || hasText(ad)

function adType(ad: any) {
  const raw = ad?.type ?? ad?.Type ?? ''
  if (typeof raw === 'number') return ['slider', 'banner', 'popup', 'product'][raw] || ''
  return norm(raw)
}
function adPlacement(ad: any) {
  return norm(ad?.placement ?? ad?.Placement ?? '')
}
function adOrder(ad: any) {
  return Number(ad?.sortOrder ?? ad?.SortOrder ?? 0)
}
function adScore(ad: any, placements: string[]) {
  const p = adPlacement(ad)
  const idx = placements.map(norm).indexOf(p)
  return idx < 0 ? 999 : idx
}
function findAd(types: string[], placements: string[], requireMedia = true) {
  const typeSet = new Set(types.map(norm))
  const placeSet = new Set(placements.map(norm))
  return [...enabledAds.value]
    .filter((a: any) => typeSet.has(adType(a)) && placeSet.has(adPlacement(a)) && isRenderable(a, requireMedia))
    .sort((a: any, b: any) => adScore(a, placements) - adScore(b, placements) || adOrder(a) - adOrder(b))[0] || null
}

// البانر والسلايدر: فقط بداية الصفحة أو آخر الصفحة.
// القيم القديمة مدعومة فقط حتى لا تختفي إعلانات محفوظة سابقاً.
const topPlacements = computed(() => [
  'home_hero_slider',
  'home_hero_top',
  'home_top',
  'hero_top',
  'above_hero',
])
const bottomPlacements = computed(() => [
  'home_inline_slider',
  'home_bottom_slider',
  'home_bottom',
  'bottom',
  'home_footer',
])

const topAds = computed(() => {
  const typeSet = new Set(['slider', 'banner'])
  const placeSet = new Set(topPlacements.value.map(norm))
  return [...enabledAds.value]
    .filter((a: any) => typeSet.has(adType(a)) && placeSet.has(adPlacement(a)) && isRenderable(a, true))
    .sort((a: any, b: any) => adScore(a, topPlacements.value) - adScore(b, topPlacements.value) || adOrder(a) - adOrder(b))
})
const primaryTopAd = computed(() => topAds.value[0] || null)
const bottomAds = computed(() => {
  const typeSet = new Set(['slider', 'banner'])
  const placeSet = new Set(bottomPlacements.value.map(norm))
  return [...enabledAds.value]
    .filter((a: any) => typeSet.has(adType(a)) && placeSet.has(adPlacement(a)) && isRenderable(a, true))
    .sort((a: any, b: any) => adScore(a, bottomPlacements.value) - adScore(b, bottomPlacements.value) || adOrder(a) - adOrder(b))
})
const primaryBottomAd = computed(() => bottomAds.value[0] || null)

// حل جذري للمنبثق: لا نعتمد على مكان واحد فقط.
// أي إعلان نوعه popup/modal/popover ويكون مفعلاً سيظهر، حتى لو كان placement قديماً أو فارغاً.
const popupAd = computed(() => {
  const popupTypes = new Set(['popup', 'modal', 'popover', 'منبثق'])
  const preferredPlaces = ['popup', 'site_popup', 'global_popup', 'home_popup', 'modal', 'site_modal', '']
  return [...enabledAds.value]
    .filter((a: any) => popupTypes.has(adType(a)) && isRenderable(a, false))
    .sort((a: any, b: any) => adScore(a, preferredPlaces) - adScore(b, preferredPlaces) || adOrder(a) - adOrder(b))[0] || null
})

const hasInlineAd = computed(() => Boolean(primaryTopAd.value || primaryBottomAd.value))

const topSlides = computed(() => {
  const seen = new Set<string>()
  return topAds.value.flatMap((ad: any) => {
    const media = mediaList(ad)
    return media.map((m: string) => ({
      key: `${ad?.id ?? ad?.Id ?? adOrder(ad)}-${m}`,
      media: m,
      title: ad?.title || ad?.Title || '',
      subtitle: ad?.subtitle || ad?.Subtitle || '',
      linkUrl: ad?.linkUrl || ad?.LinkUrl || '',
      type: adType(ad),
    }))
  }).filter((slide: any) => {
    if (seen.has(slide.key)) return false
    seen.add(slide.key)
    return true
  })
})
const currentTopSlide = computed(() => topSlides.value[sliderIndex.value] || topSlides.value[0] || null)
const nextTopSlide = computed(() => {
  if (topSlides.value.length <= 1) return null
  return topSlides.value[(sliderIndex.value + 1) % topSlides.value.length] || null
})
const previousTopSlide = computed(() => {
  if (topSlides.value.length <= 1) return null
  return topSlides.value[(sliderIndex.value - 1 + topSlides.value.length) % topSlides.value.length] || null
})
const bottomSlides = computed(() => {
  const seen = new Set<string>()
  return bottomAds.value.flatMap((ad: any) => {
    const media = mediaList(ad)
    return media.map((m: string) => ({
      key: `${ad?.id ?? ad?.Id ?? adOrder(ad)}-${m}`,
      media: m,
      title: ad?.title || ad?.Title || '',
      subtitle: ad?.subtitle || ad?.Subtitle || '',
      linkUrl: ad?.linkUrl || ad?.LinkUrl || '',
      type: adType(ad),
    }))
  }).filter((slide: any) => {
    if (seen.has(slide.key)) return false
    seen.add(slide.key)
    return true
  })
})
const currentBottomSlide = computed(() => bottomSlides.value[bottomSliderIndex.value] || bottomSlides.value[0] || null)

function isVideo(url?: string) {
  const u = String(url || '').split('?')[0].toLowerCase()
  return /\.(mp4|webm|ogg|mov)$/i.test(u)
}
function mediaComponent(url?: string) { return isVideo(url) ? 'span' : 'img' }
function mediaAttrs(path?: string, alt?: string, mode: 'auto' | 'manual' = 'auto') {
  const src = asset(path)
  if (isVideo(src)) {
    if (mode === 'manual') {
      return {
        src,
        controls: true,
        autoplay: false,
        muted: false,
        loop: false,
        playsinline: true,
        preload: 'metadata',
        controlslist: 'nodownload noplaybackrate noremoteplayback',
      }
    }
    return {
      src,
      autoplay: true,
      muted: true,
      loop: true,
      playsinline: true,
      preload: 'none',
      controls: false,
      disablepictureinpicture: true,
      controlslist: 'nodownload noplaybackrate noremoteplayback',
    }
  }
  return { src, alt: alt || 'advertisement', loading: 'eager', decoding: 'async', fetchpriority: 'high' }
}
const asset = (p?: string) => api.buildAssetUrl(p || '')
function safeLink(link?: string) {
  const v = String(link || '#').trim()
  return v || '#'
}
function stopTimer() {
  if (timer) clearInterval(timer)
  timer = null
}
function setSlide(index: number) {
  if (topSlides.value.length <= 0) return
  sliderIndex.value = (index + topSlides.value.length) % topSlides.value.length
}
function goToNextSlide() {
  if (topSlides.value.length <= 1) return
  setSlide(sliderIndex.value + 1)
  startTimer()
}
function goToPreviousSlide() {
  if (topSlides.value.length <= 1) return
  setSlide(sliderIndex.value - 1)
  startTimer()
}
function startTimer() {
  stopTimer()
  if (topSlides.value.length <= 1) return
  timer = setInterval(() => {
    setSlide(sliderIndex.value + 1)
  }, 5000)
}
function stopBottomTimer() {
  if (bottomTimer) clearInterval(bottomTimer)
  bottomTimer = null
}
function setBottomSlide(index: number) {
  if (bottomSlides.value.length <= 0) return
  bottomSliderIndex.value = (index + bottomSlides.value.length) % bottomSlides.value.length
}
function goToNextBottomSlide() {
  if (bottomSlides.value.length <= 1) return
  setBottomSlide(bottomSliderIndex.value + 1)
  startBottomTimer()
}
function goToPreviousBottomSlide() {
  if (bottomSlides.value.length <= 1) return
  setBottomSlide(bottomSliderIndex.value - 1)
  startBottomTimer()
}
function startBottomTimer() {
  stopBottomTimer()
  if (bottomSlides.value.length <= 1) return
  bottomTimer = setInterval(() => {
    setBottomSlide(bottomSliderIndex.value + 1)
  }, 5000)
}
async function handleSlideClick(slide: any) {
  const media = asset(slide?.media || '')
  if (isVideo(media)) {
    activeVideoUrl.value = media
    return
  }
  const link = safeLink(slide?.linkUrl)
  if (link && link !== '#') await navigateTo(link)
}
function closeVideoModal() { activeVideoUrl.value = '' }
function openPopupSoon() {
  if (!process.client || zone.value !== 'top' || !isHome.value) return
  if (popupTimer) clearTimeout(popupTimer)
  showPopup.value = false
  if (!popupAd.value) return
  popupTimer = setTimeout(() => {
    showPopup.value = Boolean(popupAd.value)
  }, 220)
}
async function loadAds() {
  if (!enabled.value) return
  loadingKey.value = Date.now()
  try {
    const res: any = await $fetch('/api/bff/ads/active', {
      query: { _ts: loadingKey.value },
      headers: { 'cache-control': 'no-cache, no-store, must-revalidate', pragma: 'no-cache' },
    })
    ads.value = Array.isArray(res) ? res : (Array.isArray(res?.items) ? res.items : (Array.isArray(res?.data) ? res.data : []))
  } catch { ads.value = [] }
  sliderIndex.value = 0
  bottomSliderIndex.value = 0
  startTimer()
  startBottomTimer()
  openPopupSoon()
}
function close() { showPopup.value = false }

onMounted(() => {
  loadAds()
  if (process.client) window.addEventListener('ads:changed', loadAds)
})
watch(() => route.fullPath, () => loadAds())
watch(() => topSlides.value.map((x: any) => x.key).join('|'), () => { sliderIndex.value = 0; startTimer() })
watch(() => bottomSlides.value.map((x: any) => x.key).join('|'), () => { bottomSliderIndex.value = 0; startBottomTimer() })
watch(popupAd, () => openPopupSoon())
onBeforeUnmount(() => {
  stopTimer()
  stopBottomTimer()
  if (popupTimer) clearTimeout(popupTimer)
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
.ad-hero-frame__link,.ad-bottom-frame__link{ display:block; position:relative; width:100%; min-height:inherit; color:inherit; z-index:2; border:0; padding:0; margin:0; background:transparent; text-align:inherit; cursor:pointer; }
.ad-hero-frame__media-layer{ position:relative; width:100%; height:clamp(190px,24vw,380px); overflow:hidden; background:linear-gradient(90deg, rgba(0,0,0,.36), rgba(var(--surface-rgb),.72) 24%, rgba(var(--surface-rgb),.90) 50%, rgba(var(--surface-rgb),.72) 76%, rgba(0,0,0,.20)); }
.ad-hero-frame__media{ position:absolute; inset:0; display:block; width:100%; height:100%; object-fit:contain; object-position:center; opacity:0; transform:translateZ(0) scale(.985); transition:opacity .55s ease, transform .55s ease; background:transparent; }
.ad-hero-frame__media.is-active{ opacity:1; z-index:1; transform:translateZ(0) scale(1); }
.ad-hero-frame__peek{ position:absolute; top:0; bottom:0; width:min(28%, 420px); z-index:3; display:flex; align-items:center; justify-content:center; padding:clamp(.7rem,1.2vw,1.15rem); border:0; outline:0; background:transparent; cursor:pointer; overflow:hidden; transition:opacity .28s ease, transform .28s ease, filter .28s ease; }
.ad-hero-frame__peek::before{ content:""; position:absolute; inset:clamp(.5rem,1vw,.9rem); border-radius:clamp(1rem,1.7vw,1.9rem); background:rgba(255,255,255,.10); box-shadow:inset 0 0 0 1px rgba(255,255,255,.10), 0 20px 60px rgba(0,0,0,.22); backdrop-filter:blur(10px); opacity:.75; }
.ad-hero-frame__peek--prev{ left:0; justify-content:flex-start; background:linear-gradient(90deg, rgba(0,0,0,.50), rgba(0,0,0,.12), transparent); }
.ad-hero-frame__peek--next{ right:0; justify-content:flex-end; background:linear-gradient(270deg, rgba(0,0,0,.46), rgba(0,0,0,.10), transparent); }
.ad-hero-frame__peek-media{ position:relative; z-index:1; display:block; width:100%; height:82%; object-fit:contain; object-position:center; opacity:.34; filter:blur(5px) saturate(.95); transform:scale(.88); transition:opacity .28s ease, filter .28s ease, transform .28s ease; border-radius:1.4rem; }
.ad-hero-frame__peek:hover .ad-hero-frame__peek-media{ opacity:.58; filter:blur(2px) saturate(1.04); transform:scale(.94); }
.ad-hero-frame__peek:hover::before{ opacity:1; }
.ad-bottom-frame__media{ display:block; width:100%; object-fit:contain; object-position:center; background:linear-gradient(135deg, rgba(var(--surface-rgb),.92), rgba(var(--surface-2-rgb),.72)); }
.ad-bottom-frame__media{ height:clamp(150px,18vw,270px); }
.ad-bottom-frame__video-hint{ position:absolute; inset-inline:1rem auto; top:1rem; z-index:4; width:max-content; max-width:calc(100% - 2rem); border:1px solid rgba(255,255,255,.18); border-radius:999px; padding:.55rem .85rem; color:#fff; background:rgba(0,0,0,.48); backdrop-filter:blur(14px); font-size:.82rem; font-weight:1000; }
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
.ad-slider__nav{ position:absolute; top:50%; z-index:6; display:grid; place-items:center; width:clamp(2.35rem,3.2vw,3.1rem); height:clamp(2.35rem,3.2vw,3.1rem); transform:translateY(-50%); border:1px solid rgba(255,255,255,.26); border-radius:999px; color:white; background:rgba(0,0,0,.42); box-shadow:0 14px 40px rgba(0,0,0,.22); backdrop-filter:blur(14px); font-size:clamp(1.45rem,2vw,2rem); font-weight:1000; line-height:1; transition:transform .2s ease, background .2s ease, opacity .2s ease; }
.ad-slider__nav:hover{ transform:translateY(-50%) scale(1.06); background:rgba(var(--primary),.88); color:#050509; }
.ad-slider__nav--prev{ left:clamp(.75rem,1.3vw,1.15rem); }
.ad-slider__nav--next{ right:clamp(.75rem,1.3vw,1.15rem); }
.ad-slider__dots{ position:absolute; inset-inline:0; bottom:.8rem; display:flex; align-items:center; justify-content:center; gap:.4rem; z-index:5; }
.ad-slider__dots button{ width:.58rem; height:.58rem; border-radius:999px; background:rgba(255,255,255,.55); transition:.2s ease; }
.ad-slider__dots button.is-active{ width:2.3rem; background:white; }


.ad-hero-frame__media[data-video-src],.ad-hero-frame__peek-media[data-video-src],.ad-bottom-frame__media[data-video-src],.ad-popup-card__media[data-video-src]{
  display:grid; place-items:center; background:linear-gradient(135deg, rgba(var(--primary),.22), rgba(15,23,42,.55)); position:relative; color:white;
}
.ad-hero-frame__media[data-video-src]::after,.ad-hero-frame__peek-media[data-video-src]::after,.ad-bottom-frame__media[data-video-src]::after,.ad-popup-card__media[data-video-src]::after{
  content:'▶ تشغيل الفيديو'; padding:.75rem 1rem; border-radius:999px; background:rgba(0,0,0,.48); backdrop-filter:blur(10px); font-weight:900; font-size:.9rem;
}

.ad-popup-layer{ position:fixed; inset:0; z-index:99999; display:flex; align-items:center; justify-content:center; padding:1rem; isolation:isolate; }
.ad-popup-layer__overlay{ position:absolute; inset:0; z-index:0; cursor:pointer; background:rgba(3,5,10,.76); backdrop-filter:blur(12px); }
.ad-popup-card{ position:relative; z-index:1; width:min(94vw, 660px); overflow:hidden; border-radius:2rem; border:1px solid rgba(255,255,255,.16); background:linear-gradient(145deg, rgb(var(--surface)), rgb(var(--surface-2))); box-shadow:0 34px 100px rgba(0,0,0,.55), 0 0 0 1px rgba(var(--primary),.16); animation:popupIn .28s ease both; }
.ad-popup-card__link{ display:grid; color:inherit; text-decoration:none; }
.ad-popup-card__media-wrap{ max-height:min(62vh, 430px); overflow:hidden; background:rgb(var(--surface-2)); }
.ad-popup-card__media{ display:block; width:100%; height:100%; max-height:min(62vh, 430px); object-fit:contain; object-position:center; background:rgb(var(--surface-2)); }
.ad-popup-card__content{ display:grid; gap:.55rem; padding:1.25rem; text-align:center; }
.ad-popup-card__content.is-text-only{ min-height:280px; place-content:center; padding:2rem; background:radial-gradient(circle at top right, rgba(var(--primary),.22), transparent 45%); }
.ad-popup-card__content b{ color:rgb(var(--text)); font-size:clamp(1.35rem,2.4vw,2rem); font-weight:1000; line-height:1.2; }
.ad-popup-card__content p{ margin:0; color:rgb(var(--muted)); line-height:1.8; }
.ad-popup__close{ position:absolute; z-index:5; inset-inline-start:.85rem; top:.85rem; width:2.65rem; height:2.65rem; border-radius:999px; background:rgba(0,0,0,.58); color:white; border:1px solid rgba(255,255,255,.18); font-weight:1000; box-shadow:0 10px 30px rgba(0,0,0,.25); }
.ad-popup__badge{ justify-self:center; width:max-content; border-radius:999px; padding:.36rem .85rem; background:rgba(var(--primary),.14); color:rgb(var(--primary)) !important; font-size:.78rem; font-weight:1000; }
.ad-popup__cta{ justify-self:center; margin-top:.45rem; border-radius:999px; padding:.76rem 1.2rem; background:rgb(var(--primary)); color:#050509; font-size:.88rem; font-weight:1000; }
.ad-popup-fade-enter-active,.ad-popup-fade-leave-active{ transition:opacity .2s ease; }
.ad-popup-fade-enter-from,.ad-popup-fade-leave-to{ opacity:0; }
@keyframes popupIn{ from{ opacity:0; transform:translateY(18px) scale(.96); } to{ opacity:1; transform:none; } }

@media(max-width:640px){
  .ad-container{ padding-inline:.75rem; }
  .ad-hero-frame__peek{ display:none; }
  .ad-slider__nav{ width:2.55rem; height:2.55rem; font-size:1.8rem; background:rgba(0,0,0,.52); }
  .ad-slider__nav--prev{ left:.5rem; }
  .ad-slider__nav--next{ right:.5rem; }
  .ad-hero-frame{ border-radius:1.35rem; min-height:168px; }
  .ad-hero-frame__media-layer{ height:178px; }
  .ad-hero-frame__shade{ background:linear-gradient(0deg, rgba(0,0,0,.72), rgba(0,0,0,.10)); }
  .ad-hero-frame__content{ inset-inline:.9rem !important; bottom:.9rem; max-width:calc(100% - 1.8rem); }
  .ad-hero-frame__content b{ font-size:1.45rem; }
  .ad-hero-frame__content small{ font-size:.82rem; }
  .ad-bottom-frame{ border-radius:1.35rem; }
  .ad-bottom-frame__media{ height:160px; }
  .ad-bottom-frame__content{ inset-inline:.65rem; bottom:.65rem; border-radius:1rem; padding:.65rem .75rem; }
  .ad-popup-card{ border-radius:1.35rem; }
  .ad-popup-card__content{ padding:1rem; }
}

.ad-video-modal{ position:fixed; inset:0; z-index:100000; display:flex; align-items:center; justify-content:center; padding:clamp(.75rem,2vw,1.5rem); }
.ad-video-modal__overlay{ position:absolute; inset:0; background:rgba(3,5,10,.82); backdrop-filter:blur(14px); border:0; cursor:pointer; }
.ad-video-modal__card{ position:relative; z-index:1; width:min(94vw, 980px); max-height:90vh; border-radius:1.6rem; overflow:hidden; border:1px solid rgba(255,255,255,.16); background:#050509; box-shadow:0 34px 110px rgba(0,0,0,.65); }
.ad-video-modal__video{ display:block; width:100%; max-height:90vh; background:#000; object-fit:contain; }
.ad-video-modal__close{ position:absolute; z-index:3; top:.8rem; inset-inline-start:.8rem; width:2.6rem; height:2.6rem; border-radius:999px; border:1px solid rgba(255,255,255,.22); background:rgba(0,0,0,.62); color:#fff; font-weight:1000; }

</style>

