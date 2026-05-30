<template>
  <Transition name="intro-fade">
    <div v-if="visible" class="intro-screen" role="dialog" aria-modal="true">
      <video
        v-if="videoSrc"
        class="intro-screen__video"
        :src="videoSrc"
        autoplay
        muted
        loop
        playsinline
      />
      <div v-else class="intro-screen__fallback" />
      <div class="intro-screen__overlay" />
      <div class="intro-screen__content rtl-text">
        <div class="intro-screen__badge">Beauty Store</div>
        <h1>{{ intro.title || 'اكتشفي جمالك بثقة' }}</h1>
        <p>{{ intro.subtitle || 'منتجات مختارة بعناية لتجربة كوزمتك أنيقة وسريعة.' }}</p>
        <div class="intro-screen__actions">
          <button type="button" class="intro-screen__primary" @click="startNow">
            {{ intro.buttonText || 'ابدأ الآن' }}
          </button>
          <button type="button" class="intro-screen__ghost" @click="goSecondary">
            {{ intro.secondaryButtonText || 'تصفح البراندات' }}
          </button>
          <button type="button" class="intro-screen__skip" @click="closeIntro">تخطي</button>
        </div>
      </div>
    </div>
  </Transition>
</template>

<script setup lang="ts">
const appearance = useAppearanceStore()
const router = useRouter()
const route = useRoute()
const { buildAssetUrl } = useApi()
if (!appearance.loaded) await appearance.refresh()

const visible = ref(false)
const intro = computed(() => appearance.data.intro || { enabled: false })
const videoSrc = computed(() => intro.value.videoUrl ? buildAssetUrl(String(intro.value.videoUrl)) : '')

function closeIntro() {
  visible.value = false
  if (!process.client) return
  if (route.path === '/intro') {
    router.push('/')
    return
  }
  sessionStorage.setItem('intro-seen', '1')
}
function startNow() {
  const url = intro.value.buttonUrl || '/products'
  closeIntro()
  router.push(url)
}
function goSecondary() {
  const url = intro.value.secondaryButtonUrl || '/brands'
  closeIntro()
  router.push(url)
}
watch(
  () => [appearance.loaded, intro.value.enabled, route.path],
  () => {
    if (!process.client) return
    if (route.path.startsWith('/admin')) return
    if (!appearance.loaded) return
    if (route.path === '/intro') {
      visible.value = true
      return
    }
    if (!intro.value.enabled) return
    if (sessionStorage.getItem('intro-seen') === '1') return
    visible.value = true
  },
  { immediate: true }
)
</script>

<style scoped>
.intro-screen{ position:fixed; inset:0; z-index:80; display:grid; place-items:center; padding:1rem; overflow:hidden; background:#050509; }
.intro-screen__video,.intro-screen__fallback{ position:absolute; inset:0; width:100%; height:100%; object-fit:cover; }
.intro-screen__fallback{ background:radial-gradient(circle at 76% 18%, rgba(var(--primary),.38), transparent 36%), radial-gradient(circle at 18% 76%, rgba(236,72,153,.24), transparent 42%), #050509; }
.intro-screen__overlay{ position:absolute; inset:0; background:linear-gradient(90deg, rgba(0,0,0,.9), rgba(0,0,0,.42), rgba(0,0,0,.76)); }
.intro-screen__content{ position:relative; width:min(94vw,760px); color:white; border:1px solid rgba(255,255,255,.15); border-radius:38px; padding:clamp(1.6rem,4vw,3rem); background:rgba(8,8,14,.52); backdrop-filter:blur(18px); box-shadow:0 32px 90px rgba(0,0,0,.42); }
.intro-screen__badge{ display:inline-flex; border:1px solid rgba(255,255,255,.18); border-radius:999px; padding:.38rem .75rem; color:rgba(255,255,255,.8); font-size:.75rem; font-weight:900; }
.intro-screen h1{ margin-top:1.2rem; font-size:clamp(2.6rem,7vw,6rem); line-height:.95; font-weight:1000; letter-spacing:-.06em; }
.intro-screen p{ margin-top:1rem; max-width:620px; color:rgba(255,255,255,.82); font-size:clamp(1rem,1.7vw,1.2rem); line-height:1.9; }
.intro-screen__actions{ display:flex; flex-wrap:wrap; gap:.8rem; margin-top:2rem; }
.intro-screen__primary,.intro-screen__ghost,.intro-screen__skip{ min-height:50px; border-radius:999px; padding:0 1.25rem; font-weight:1000; transition:.2s ease; }
.intro-screen__primary{ background:rgb(var(--primary)); color:#050509; }
.intro-screen__ghost{ border:1px solid rgba(255,255,255,.18); color:white; background:rgba(255,255,255,.08); }
.intro-screen__skip{ color:rgba(255,255,255,.72); background:transparent; }
.intro-screen__primary:hover,.intro-screen__ghost:hover,.intro-screen__skip:hover{ transform:translateY(-2px); }
.intro-fade-enter-active,.intro-fade-leave-active{ transition:opacity .35s ease; }
.intro-fade-enter-from,.intro-fade-leave-to{ opacity:0; }
@media(max-width:640px){ .intro-screen__content{ border-radius:28px; } .intro-screen__actions{ flex-direction:column; } }
</style>
