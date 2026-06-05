<template>
  <div class="intro-page min-h-screen relative overflow-hidden bg-[#050509] text-white rtl-text">
    <iframe
      v-if="embedSrc"
      class="intro-page__media"
      :src="embedSrc"
      allow="autoplay; encrypted-media; picture-in-picture"
      allowfullscreen
    />
    <video v-else-if="videoSrc" class="intro-page__media" :src="videoSrc" autoplay muted loop playsinline />
    <div v-else class="intro-page__fallback" />
    <div class="intro-page__overlay" />

    <main class="relative z-10 grid min-h-screen place-items-center px-4 py-12">
      <section class="intro-page__card">
        <span class="intro-page__badge">Beauty Store</span>
        <h1>{{ intro.title || 'Dr.Seoul Beauty' }}</h1>
        <p>{{ intro.subtitle || 'منتجات مختارة بعناية لتجربة كوزمتك أنيقة وسريعة.' }}</p>
        <div class="intro-page__actions">
          <NuxtLink :to="intro.buttonUrl || '/products'" class="intro-page__primary">{{ intro.buttonText || 'ابدأ الآن' }}</NuxtLink>
          <NuxtLink :to="intro.secondaryButtonUrl || '/brands'" class="intro-page__ghost">{{ intro.secondaryButtonText || 'تصفح البراندات' }}</NuxtLink>
          <NuxtLink to="/" class="intro-page__skip">الدخول للمتجر</NuxtLink>
        </div>

        <div v-if="!isStandalone" class="intro-install-card">
          <div class="intro-install-icon">
            <Icon name="mdi:cellphone-arrow-down" />
          </div>
          <div class="intro-install-copy">
            <strong>احصل على التطبيق</strong>
            <span>ثبت المتجر على الشاشة الرئيسية لتجربة أسرع تشبه التطبيق.</span>
          </div>
          <button v-if="canInstall" type="button" class="intro-install-btn" @click="installApp">
            تثبيت الآن
          </button>
          <button v-else-if="isIOS" type="button" class="intro-install-btn intro-install-btn--ghost" @click="showIosHelp = !showIosHelp">
            طريقة التثبيت
          </button>
          <span v-else class="intro-install-note">افتح الموقع من Chrome حتى يظهر زر التثبيت.</span>
        </div>

        <div v-if="showIosHelp && isIOS && !isStandalone" class="intro-ios-help">
          <b>على الآيفون</b>
          <span>اضغط زر المشاركة في Safari، ثم اختر: إضافة إلى الشاشة الرئيسية.</span>
        </div>
      </section>
    </main>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'

definePageMeta({ layout: 'default' })
const appearance = useAppearanceStore()
const { buildAssetUrl } = useApi()
if (!appearance.loaded) await appearance.refresh()
const intro = computed(() => appearance.data.intro || { enabled: false })
const deferredPrompt = ref<any>(null)
const canInstall = computed(() => !!deferredPrompt.value)
const isIOS = ref(false)
const isStandalone = ref(false)
const showIosHelp = ref(false)

onMounted(() => {
  const ua = window.navigator.userAgent || ''
  isIOS.value = /iphone|ipad|ipod/i.test(ua)
  isStandalone.value = window.matchMedia('(display-mode: standalone)').matches || (window.navigator as any).standalone === true

  window.addEventListener('beforeinstallprompt', (event: any) => {
    event.preventDefault()
    deferredPrompt.value = event
  })
})

async function installApp() {
  if (!deferredPrompt.value) return
  deferredPrompt.value.prompt()
  await deferredPrompt.value.userChoice
  deferredPrompt.value = null
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
</script>

<style scoped>
.intro-page__media,.intro-page__fallback{ position:absolute; inset:0; width:100%; height:100%; object-fit:cover; border:0; }
.intro-page__fallback{ background:radial-gradient(circle at 76% 18%, rgba(var(--primary),.38), transparent 36%), radial-gradient(circle at 18% 76%, rgba(236,72,153,.24), transparent 42%), #050509; }
.intro-page__overlay{ position:absolute; inset:0; background:linear-gradient(90deg, rgba(0,0,0,.90), rgba(0,0,0,.45), rgba(0,0,0,.78)); }
.intro-page__card{ width:min(94vw,800px); border:1px solid rgba(255,255,255,.15); border-radius:40px; padding:clamp(1.7rem,4vw,3.2rem); background:rgba(8,8,14,.56); backdrop-filter:blur(18px); box-shadow:0 32px 90px rgba(0,0,0,.42); }
.intro-page__badge{ display:inline-flex; border:1px solid rgba(255,255,255,.18); border-radius:999px; padding:.38rem .75rem; color:rgba(255,255,255,.8); font-size:.75rem; font-weight:900; }
.intro-page h1{ margin-top:1.1rem; font-size:clamp(2.6rem,8vw,6.4rem); line-height:.95; font-weight:1000; letter-spacing:-.06em; }
.intro-page p{ margin-top:1rem; max-width:620px; color:rgba(255,255,255,.82); font-size:clamp(1rem,1.7vw,1.2rem); line-height:1.9; }
.intro-page__actions{ display:flex; flex-wrap:wrap; gap:.8rem; margin-top:2rem; }
.intro-page__primary,.intro-page__ghost,.intro-page__skip{ display:inline-flex; align-items:center; justify-content:center; min-height:52px; border-radius:999px; padding:0 1.3rem; font-weight:1000; transition:.2s ease; }
.intro-page__primary{ background:rgb(var(--primary)); color:#050509; }
.intro-page__ghost{ border:1px solid rgba(255,255,255,.18); color:white; background:rgba(255,255,255,.08); }
.intro-page__skip{ color:rgba(255,255,255,.72); }
.intro-page__primary:hover,.intro-page__ghost:hover,.intro-page__skip:hover{ transform:translateY(-2px); }
.intro-install-card{ margin-top:1.2rem; display:grid; grid-template-columns:auto minmax(0,1fr) auto; gap:.9rem; align-items:center; border:1px solid rgba(255,255,255,.14); border-radius:1.3rem; padding:.9rem; background:rgba(255,255,255,.075); }
.intro-install-icon{ width:2.8rem; height:2.8rem; border-radius:1rem; display:grid; place-items:center; background:rgba(var(--primary),.22); color:rgb(var(--primary)); font-size:1.35rem; }
.intro-install-copy{ display:grid; gap:.15rem; min-width:0; }
.intro-install-copy strong{ font-weight:1000; }
.intro-install-copy span,.intro-install-note{ color:rgba(255,255,255,.72); font-size:.86rem; line-height:1.7; }
.intro-install-btn{ min-height:42px; border-radius:999px; padding:0 1rem; font-weight:1000; background:rgb(var(--primary)); color:#050509; border:1px solid rgba(255,255,255,.12); }
.intro-install-btn--ghost{ background:rgba(255,255,255,.08); color:#fff; }
.intro-ios-help{ margin-top:.75rem; border:1px solid rgba(var(--primary),.32); border-radius:1rem; padding:.85rem; display:grid; gap:.25rem; background:rgba(var(--primary),.13); color:rgba(255,255,255,.86); }
.intro-ios-help b{ color:white; }
@media(max-width:640px){ .intro-page__card{ border-radius:28px; } .intro-page__actions{ flex-direction:column; } .intro-install-card{ grid-template-columns:auto 1fr; } .intro-install-btn,.intro-install-note{ grid-column:1 / -1; } }
</style>
