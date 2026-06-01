<template>
  <ClientOnly>
    <div v-if="enabled && hasInlineAd" class="global-ads" :data-zone="zone">
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
              <small v-if="primaryTopAd.subtitle">{{ primaryTopAd.subtitle }}</small>
            </div>
          </NuxtLink>
          <div v-if="topMedia.length > 1" class="ad-slider__dots">
            <button
              v-for="(_, idx) in topMedia"
              :key="idx"
              type="button"
              :class="idx === sliderIndex ? 'is-active' : ''"
              @click="sliderIndex = idx"
              :aria-label="`slide ${idx + 1}`"
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
              <span v-if="primaryBottomAd.subtitle">{{ primaryBottomAd.subtitle }}</span>
            </div>
          </NuxtLink>
        </div>
      </section>
    </div>

    <Teleport to="body">
      <Transition name="ad-popup-fade">
        <div
          v-if="enabled && zone === 'top' && popupAd && showPopup"
          class="ad-popup-layer"
          role="dialog"
          aria-modal="true"
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
let timer: ReturnType<typeof setInterval> | null = null
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
  'home_bottom_slider',
  'home_bottom',
  'bottom',
  'home_footer',
])

const topSlider = computed(() => findAd(['slider'], topPlacements.value, true))
const topBanner = computed(() => findAd(['banner'], topPlacements.value, true))
const primaryTopAd = computed(() => topSlider.value || topBanner.value)
const bottomSlider = computed(() => findAd(['slider'], bottomPlacements.value, true))
const bottomBanner = computed(() => findAd(['banner'], bottomPlacements.value, true))
const primaryBottomAd = computed(() => bottomSlider.value || bottomBanner.value)

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
  if (isVideo(src)) return { src, autoplay: true, muted: true, loop: true, playsinline: true, controls: false }
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
function openPopupSoon() {
  if (!process.client || zone.value !== 'top') return
  if (popupTimer) clearTimeout(popupTimer)
  showPopup.value = false
  if (!popupAd.value) return
  popupTimer = setTimeout(() => {
    showPopup.value = Boolean(popupAd.value)
  }, 650)
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
  startTimer()
  openPopupSoon()
}
function close() { showPopup.value = false }

onMounted(() => {
  loadAds()
  if (process.client) window.addEventListener('ads:changed', loadAds)
})
watch(() => route.fullPath, () => loadAds())
watch(topMedia, () => { sliderIndex.value = 0; startTimer() })
watch(popupAd, () => openPopupSoon())
onBeforeUnmount(() => {
  stopTimer()
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

.ad-popup-layer{ position:fixed; inset:0; z-index:99999; display:flex; align-items:center; justify-content:center; padding:1rem; isolation:isolate; }
.ad-popup-layer__overlay{ position:absolute; inset:0; z-index:0; cursor:pointer; background:rgba(3,5,10,.76); backdrop-filter:blur(12px); }
.ad-popup-card{ position:relative; z-index:1; width:min(94vw, 660px); overflow:hidden; border-radius:2rem; border:1px solid rgba(255,255,255,.16); background:linear-gradient(145deg, rgb(var(--surface)), rgb(var(--surface-2))); box-shadow:0 34px 100px rgba(0,0,0,.55), 0 0 0 1px rgba(var(--primary),.16); animation:popupIn .28s ease both; }
.ad-popup-card__link{ display:grid; color:inherit; text-decoration:none; }
.ad-popup-card__media-wrap{ max-height:min(62vh, 430px); overflow:hidden; background:rgb(var(--surface-2)); }
.ad-popup-card__media{ display:block; width:100%; height:100%; max-height:min(62vh, 430px); object-fit:cover; }
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
  .ad-hero-frame{ border-radius:1.35rem; min-height:168px; }
  .ad-hero-frame__media{ height:178px; }
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
</style>
