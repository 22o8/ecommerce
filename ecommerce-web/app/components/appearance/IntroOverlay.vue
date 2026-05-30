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
      <div class="intro-screen__overlay" />
      <div class="intro-screen__content">
        <div class="intro-screen__badge">Beauty Store</div>
        <h1>{{ intro.title || 'اكتشفي جمالك بثقة' }}</h1>
        <p>{{ intro.subtitle || 'منتجات مختارة بعناية لتجربة كوزمتك أنيقة وسريعة.' }}</p>
        <div class="intro-screen__actions">
          <button type="button" class="intro-screen__primary" @click="startNow">
            {{ intro.buttonText || 'ابدأ الآن' }}
          </button>
          <button type="button" class="intro-screen__ghost" @click="closeIntro">تخطي</button>
        </div>
      </div>
    </div>
  </Transition>
</template>

<script setup lang="ts">
const appearance = useAppearanceStore()
const { buildAssetUrl } = useApi()
const router = useRouter()
const route = useRoute()
const visible = ref(false)

const intro = computed(() => appearance.data.intro || { enabled: false })
const videoSrc = computed(() => intro.value.videoUrl ? buildAssetUrl(String(intro.value.videoUrl)) : '')

function closeIntro() {
  visible.value = false
  if (process.client) sessionStorage.setItem('intro-seen', '1')
}

function startNow() {
  const url = intro.value.buttonUrl || '/products'
  closeIntro()
  router.push(url)
}

watch(
  () => [appearance.loaded, intro.value.enabled, route.path],
  () => {
    if (!process.client) return
    if (route.path.startsWith('/admin')) return
    if (!appearance.loaded || !intro.value.enabled) return
    if (sessionStorage.getItem('intro-seen') === '1') return
    visible.value = true
  },
  { immediate: true }
)
</script>

<style scoped>
.intro-screen{
  position:fixed;
  inset:0;
  z-index:9999;
  display:grid;
  place-items:center;
  overflow:hidden;
  background:#050509;
}
.intro-screen__video{
  position:absolute;
  inset:0;
  width:100%;
  height:100%;
  object-fit:cover;
  opacity:.72;
}
.intro-screen__overlay{
  position:absolute;
  inset:0;
  background:
    radial-gradient(circle at 72% 22%, rgba(var(--primary), .32), transparent 34%),
    linear-gradient(90deg, rgba(0,0,0,.86), rgba(0,0,0,.48), rgba(0,0,0,.78));
}
.intro-screen__content{
  position:relative;
  width:min(92vw, 760px);
  border:1px solid rgba(255,255,255,.16);
  border-radius:38px;
  padding:clamp(28px, 6vw, 64px);
  color:white;
  background:rgba(10,10,16,.48);
  box-shadow:0 40px 120px rgba(0,0,0,.44);
  backdrop-filter:blur(18px);
  text-align:start;
}
.intro-screen__badge{
  display:inline-flex;
  border:1px solid rgba(255,255,255,.18);
  border-radius:999px;
  padding:.45rem .9rem;
  color:rgba(255,255,255,.78);
  font-size:.76rem;
  font-weight:900;
  letter-spacing:.12em;
  text-transform:uppercase;
}
.intro-screen h1{
  margin-top:1.2rem;
  font-size:clamp(2.4rem, 7vw, 5.8rem);
  line-height:.95;
  font-weight:1000;
  letter-spacing:-.05em;
}
.intro-screen p{
  margin-top:1.2rem;
  max-width:620px;
  color:rgba(255,255,255,.78);
  font-size:clamp(1rem, 2vw, 1.25rem);
  line-height:1.9;
}
.intro-screen__actions{ display:flex; flex-wrap:wrap; gap:.8rem; margin-top:2rem; }
.intro-screen__primary,.intro-screen__ghost{
  min-height:52px;
  border-radius:999px;
  padding:0 1.35rem;
  font-weight:1000;
  transition:.2s ease;
}
.intro-screen__primary{ background:rgb(var(--primary)); color:#050509; }
.intro-screen__ghost{ border:1px solid rgba(255,255,255,.18); color:white; background:rgba(255,255,255,.08); }
.intro-screen__primary:hover,.intro-screen__ghost:hover{ transform:translateY(-2px); }
.intro-fade-enter-active,.intro-fade-leave-active{ transition:opacity .35s ease; }
.intro-fade-enter-from,.intro-fade-leave-to{ opacity:0; }
@media(max-width:640px){ .intro-screen__content{ border-radius:28px; } }
</style>
