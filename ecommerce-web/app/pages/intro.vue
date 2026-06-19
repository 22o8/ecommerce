<template>
  <div class="intro-page min-h-screen relative overflow-hidden bg-[#050509] text-white rtl-text">
    <iframe
      v-if="embedSrc"
      class="intro-page__media"
      :src="embedSrc"
      allow="autoplay; encrypted-media; picture-in-picture"
      loading="eager"
      fetchpriority="high"
      allowfullscreen
    />
    <video
      v-else-if="videoSrc"
      ref="introVideoEl"
      class="intro-page__media"
      :src="videoSrc"
      autoplay
      muted
      loop
      playsinline
      preload="auto"
      disablepictureinpicture
      controlslist="nodownload noplaybackrate noremoteplayback"
      @loadedmetadata="forcePlayIntroVideo"
      @loadeddata="forcePlayIntroVideo"
      @canplay="forcePlayIntroVideo"
      @canplaythrough="forcePlayIntroVideo"
      @pause="forcePlayIntroVideo"
      @stalled="forcePlayIntroVideo"
      @waiting="forcePlayIntroVideo"
    />
    <div v-else class="intro-page__fallback" />
    <div class="intro-page__overlay" />

    <main class="relative z-10 grid min-h-screen place-items-center px-4 py-12">
      <section class="intro-page__card">
        <span class="intro-page__badge">Beauty Store</span>
        <h1>{{ intro.title || 'Dr.Seoul Beauty' }}</h1>
        <p>{{ intro.subtitle || 'منتجات مختارة بعناية لتجربة كوزمتك أنيقة وسريعة.' }}</p>
        <div class="intro-page__actions">
          <NuxtLink :to="intro.buttonUrl || '/products'" prefetch class="intro-page__primary">{{ intro.buttonText || 'ابدأ الآن' }}</NuxtLink>
          <NuxtLink :to="intro.secondaryButtonUrl || '/brands'" prefetch class="intro-page__ghost">{{ intro.secondaryButtonText || 'تصفح البراندات' }}</NuxtLink>
        </div>
      </section>
    </main>
  </div>
</template>

<script setup lang="ts">
import { computed, nextTick, onBeforeUnmount, onMounted, ref, watch } from 'vue'

definePageMeta({ layout: 'default' })
const appearance = useAppearanceStore()
const { buildAssetUrl } = useApi()
if (!appearance.loaded) await appearance.refresh()
const intro = computed(() => appearance.data.intro || { enabled: false })
const deferredPrompt = ref<any>(null)
const isIOS = ref(false)
const isAndroid = ref(false)
const isStandalone = ref(false)
const installMessage = ref('')
const introVideoEl = ref<HTMLVideoElement | null>(null)
let introPlayTimer: any = null
let introRetryTimer: any = null
let introUserEventsBound = false

async function forcePlayIntroVideo() {
  await nextTick()
  const el = introVideoEl.value
  if (!el) return

  // أهم إعدادات تشغيل الفيديو تلقائياً على الهاتف
  el.muted = true
  el.defaultMuted = true
  el.playsInline = true
  el.setAttribute('muted', '')
  el.setAttribute('playsinline', '')
  el.setAttribute('webkit-playsinline', '')
  el.setAttribute('preload', 'auto')

  try {
    // لا نعيد الفيديو للبداية إذا كان بدأ يشتغل فعلاً
    if (el.paused || el.readyState < 3) {
      await el.play()
    }
  } catch {
    // بعض أجهزة iPhone/Android تؤخر التشغيل لغاية أول لمسة.
  }
}


function startIntroVideoRetry() {
  if (typeof window === 'undefined') return
  stopIntroVideoRetry()
  let tries = 0
  introRetryTimer = window.setInterval(() => {
    tries++
    forcePlayIntroVideo()
    const el = introVideoEl.value
    if ((el && !el.paused && el.currentTime > 0) || tries > 18) {
      stopIntroVideoRetry()
    }
  }, 350)
}

function stopIntroVideoRetry() {
  if (typeof window === 'undefined') return
  if (introRetryTimer) {
    window.clearInterval(introRetryTimer)
    introRetryTimer = null
  }
}

function bindIntroUserPlayback() {
  if (introUserEventsBound || typeof window === 'undefined') return
  introUserEventsBound = true
  const play = () => forcePlayIntroVideo()
  window.addEventListener('pointerdown', play, { passive: true })
  window.addEventListener('touchstart', play, { passive: true })
  window.addEventListener('click', play, { passive: true })
}

function handleVisibilityPlay() {
  if (typeof document !== 'undefined' && !document.hidden) forcePlayIntroVideo()
}


onMounted(() => {
  const ua = window.navigator.userAgent || ''
  isIOS.value = /iphone|ipad|ipod/i.test(ua)
  isAndroid.value = /android/i.test(ua)
  isStandalone.value = window.matchMedia('(display-mode: standalone)').matches || (window.navigator as any).standalone === true

  bindIntroUserPlayback()
  document.addEventListener('visibilitychange', handleVisibilityPlay)
  startIntroVideoRetry()
  introPlayTimer = window.setTimeout(() => forcePlayIntroVideo(), 60)

  window.addEventListener('beforeinstallprompt', (event: any) => {
    event.preventDefault()
    deferredPrompt.value = event
  })
})

onBeforeUnmount(() => {
  stopIntroVideoRetry()
  if (introPlayTimer) window.clearTimeout(introPlayTimer)
  document.removeEventListener('visibilitychange', handleVisibilityPlay)
})

async function installAndroidApp() {
  installMessage.value = ''
  if (isIOS.value) {
    await navigateTo('/ios-install')
    return
  }

  if (!deferredPrompt.value) {
    installMessage.value = isAndroid.value
      ? 'افتح الموقع من Chrome وانتظر ثواني حتى يظهر خيار التثبيت.'
      : 'التثبيت المباشر متاح غالباً على Android من متصفح Chrome.'
    return
  }

  deferredPrompt.value.prompt()
  const choice = await deferredPrompt.value.userChoice
  deferredPrompt.value = null
  installMessage.value = choice?.outcome === 'accepted'
    ? 'تم إرسال طلب التثبيت بنجاح.'
    : 'يمكنك تثبيت التطبيق لاحقاً من نفس الزر.'
}

function isEmbeddableVideo(v: string) { return /youtube\.com|youtu\.be|instagram\.com|tiktok\.com/i.test(v) }
function toEmbedUrl(raw?: string) {
  const v = String(raw || '').trim()
  if (!v) return ''
  try {
    const u = new URL(v)
    const host = u.hostname.replace(/^www\./, '')
    if (host.includes('youtu.be')) return `https://www.youtube.com/embed/${u.pathname.replace('/', '')}?autoplay=1&mute=1&loop=1&playsinline=1`
    if (host.includes('youtube.com')) {
      const id = u.searchParams.get('v') || u.pathname.split('/').filter(Boolean).pop()
      return id ? `https://www.youtube.com/embed/${id}?autoplay=1&mute=1&loop=1&playsinline=1` : ''
    }
    if (host.includes('instagram.com')) {
      const parts = u.pathname.split('/').filter(Boolean)
      const type = parts[0] || 'reel'
      const code = parts[1]
      return code ? `https://www.instagram.com/${type}/${code}/embed` : ''
    }
    if (host.includes('tiktok.com')) return v
  } catch {}
  return ''
}
const embedSrc = computed(() => toEmbedUrl(intro.value.videoUrl))
const videoSrc = computed(() => {
  const raw = String(intro.value.videoUrl || '')
  if (!raw || isEmbeddableVideo(raw)) return ''
  return buildAssetUrl(raw)
})

watch(videoSrc, () => {
  useHead({
    link: videoSrc.value ? [
      { rel: 'preload', as: 'video', href: videoSrc.value, fetchpriority: 'high' as any }
    ] : []
  })
  startIntroVideoRetry()
  forcePlayIntroVideo()
}, { flush: 'post', immediate: true })
</script>

<style scoped>
.intro-page__media,.intro-page__fallback{ position:absolute; inset:0; width:100%; height:100%; object-fit:cover; border:0; transform:translateZ(0); will-change:transform; backface-visibility:hidden; }
.intro-page__fallback{ background:radial-gradient(circle at 76% 18%, rgba(var(--primary),.38), transparent 36%), radial-gradient(circle at 18% 76%, rgba(236,72,153,.24), transparent 42%), #050509; }
.intro-page__overlay{ position:absolute; inset:0; background:linear-gradient(90deg, rgba(0,0,0,.90), rgba(0,0,0,.45), rgba(0,0,0,.78)); }
.intro-page__card{ width:min(94vw,800px); border:1px solid rgba(255,255,255,.15); border-radius:40px; padding:clamp(1.7rem,4vw,3.2rem); background:rgba(8,8,14,.56); backdrop-filter:blur(18px); box-shadow:0 32px 90px rgba(0,0,0,.42); }
.intro-page__badge{ display:inline-flex; border:1px solid rgba(255,255,255,.18); border-radius:999px; padding:.38rem .75rem; color:rgba(255,255,255,.8); font-size:.75rem; font-weight:900; }
.intro-page h1{ margin-top:1.1rem; font-size:clamp(2.6rem,8vw,6.4rem); line-height:.95; font-weight:1000; letter-spacing:-.06em; }
.intro-page p{ margin-top:1rem; max-width:620px; color:rgba(255,255,255,.82); font-size:clamp(1rem,1.7vw,1.2rem); line-height:1.9; }
.intro-page__actions{ display:flex; flex-wrap:wrap; gap:.8rem; margin-top:2rem; }
.intro-page__primary,.intro-page__ghost{ display:inline-flex; align-items:center; justify-content:center; min-height:52px; border-radius:999px; padding:0 1.3rem; font-weight:1000; transition:.2s ease; }
.intro-page__primary{ background:rgb(var(--primary)); color:#050509; }
.intro-page__ghost{ border:1px solid rgba(255,255,255,.18); color:white; background:rgba(255,255,255,.08); }
.intro-page__primary:hover,.intro-page__ghost:hover{ transform:translateY(-2px); }
.intro-install-panel{ margin-top:1.2rem; display:grid; gap:1rem; border:1px solid rgba(255,255,255,.14); border-radius:1.5rem; padding:1rem; background:linear-gradient(135deg, rgba(var(--primary),.16), rgba(255,255,255,.07)); }
.intro-install-head{ display:flex; gap:.85rem; align-items:center; }
.intro-install-icon{ flex:0 0 auto; width:2.9rem; height:2.9rem; border-radius:1rem; display:grid; place-items:center; background:rgba(var(--primary),.24); color:rgb(var(--primary)); font-size:1.35rem; }
.intro-install-head strong{ display:block; font-weight:1000; }
.intro-install-head span:not(.intro-install-icon){ display:block; margin-top:.12rem; color:rgba(255,255,255,.72); font-size:.88rem; line-height:1.7; }
.intro-install-actions{ display:grid; grid-template-columns:repeat(2,minmax(0,1fr)); gap:.75rem; }
.intro-install-btn{ min-height:48px; border-radius:999px; padding:0 1rem; font-weight:1000; display:inline-flex; align-items:center; justify-content:center; gap:.45rem; border:1px solid rgba(255,255,255,.12); transition:.2s ease; }
.intro-install-btn:hover{ transform:translateY(-2px); }
.intro-install-btn--android{ background:rgb(var(--primary)); color:#050509; }
.intro-install-btn--ios{ background:rgba(255,255,255,.09); color:#fff; }
.intro-install-message{ margin:0; padding:.75rem .9rem; border-radius:1rem; background:rgba(0,0,0,.24); color:rgba(255,255,255,.84); font-size:.88rem; }
@media(max-width:640px){ .intro-page__card{ border-radius:28px; } .intro-page__actions{ flex-direction:column; } .intro-install-actions{ grid-template-columns:1fr; } }
</style>
