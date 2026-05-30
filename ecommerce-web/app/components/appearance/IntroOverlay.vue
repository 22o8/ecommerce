<template>
  <Teleport to="body">
    <Transition name="intro-fade">
      <div v-if="visible" class="intro-overlay">
        <div class="intro-backdrop" />
        <div class="intro-card">
          <button class="intro-close" type="button" aria-label="close" @click="dismiss">×</button>
          <div class="intro-media">
            <video
              v-if="intro.videoUrl"
              :src="asset(intro.videoUrl)"
              class="intro-video"
              autoplay
              muted
              playsinline
              loop
            />
            <div v-else class="intro-fallback">
              <span class="intro-orb" />
              <span class="intro-orb intro-orb--small" />
              <div class="intro-beauty-word">BEAUTY</div>
            </div>
          </div>
          <div class="intro-content rtl-text">
            <div class="intro-kicker">مرحباً بك</div>
            <h2>{{ intro.title || 'ابدأ رحلتك الجمالية' }}</h2>
            <p>{{ intro.subtitle || 'منتجات مختارة بعناية لتصل بسرعة إلى روتينك المناسب.' }}</p>
            <button type="button" class="intro-start" @click="dismiss">
              {{ intro.buttonText || 'ابدأ الآن' }}
            </button>
          </div>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<script setup lang="ts">
const route = useRoute()
const store = useAppearanceStore()
const api = useApi()
const closed = ref(false)
const intro = computed(() => store.data.intro || { enabled: false })
const storageKey = computed(() => `intro:${store.data.updatedAt || 'default'}`)

const visible = computed(() => {
  if (!process.client) return false
  if (route.path !== '/') return false
  if (!intro.value?.enabled) return false
  if (closed.value) return false
  return sessionStorage.getItem(storageKey.value) !== '1'
})

function asset(url?: string) {
  return api.buildAssetUrl(url || '')
}

function dismiss() {
  closed.value = true
  if (process.client) sessionStorage.setItem(storageKey.value, '1')
}
</script>

<style scoped>
.intro-overlay{ position:fixed; inset:0; z-index:100; display:grid; place-items:center; padding:18px; }
.intro-backdrop{ position:absolute; inset:0; background:rgba(0,0,0,.68); backdrop-filter:blur(18px); }
.intro-card{ position:relative; width:min(980px, 100%); overflow:hidden; border-radius:36px; border:1px solid rgba(var(--border), .92); background:linear-gradient(135deg, rgba(var(--surface-rgb), .98), rgba(var(--surface-2-rgb), .94)); box-shadow:0 40px 120px rgba(0,0,0,.38); display:grid; grid-template-columns:1.05fr .95fr; min-height:520px; }
.intro-close{ position:absolute; left:18px; top:18px; z-index:3; width:42px; height:42px; border-radius:999px; background:rgba(0,0,0,.42); color:#fff; font-size:24px; border:1px solid rgba(255,255,255,.18); }
.intro-media{ position:relative; min-height:420px; background:radial-gradient(circle at 30% 20%, rgba(var(--primary), .22), transparent 42%), linear-gradient(145deg, rgba(236,72,153,.18), rgba(124,58,237,.18)); }
.intro-video{ width:100%; height:100%; object-fit:cover; display:block; }
.intro-fallback{ position:absolute; inset:0; display:grid; place-items:center; overflow:hidden; }
.intro-orb{ position:absolute; width:380px; height:380px; border-radius:50%; background:radial-gradient(circle, rgba(255,255,255,.34), rgba(var(--primary), .20) 38%, transparent 70%); filter:blur(4px); animation:introFloat 7s ease-in-out infinite; }
.intro-orb--small{ width:190px; height:190px; right:12%; bottom:12%; animation-delay:-2s; }
.intro-beauty-word{ font-size:clamp(52px, 10vw, 118px); font-weight:1000; letter-spacing:.08em; color:rgba(255,255,255,.5); mix-blend-mode:overlay; }
.intro-content{ padding:clamp(30px, 5vw, 58px); display:flex; flex-direction:column; justify-content:center; }
.intro-kicker{ width:max-content; border:1px solid rgba(var(--primary), .35); background:rgba(var(--primary), .10); color:rgb(var(--primary)); border-radius:999px; padding:.45rem .8rem; font-size:.78rem; font-weight:900; margin-bottom:18px; }
.intro-content h2{ font-size:clamp(32px, 5vw, 64px); line-height:1.05; font-weight:1000; color:rgb(var(--text)); }
.intro-content p{ margin-top:18px; color:rgb(var(--muted)); font-size:1.05rem; line-height:2; max-width:34rem; }
.intro-start{ margin-top:28px; min-height:54px; border-radius:999px; padding:0 28px; background:linear-gradient(135deg, rgb(var(--primary)), rgba(236,72,153,.95)); color:white; font-weight:1000; box-shadow:0 18px 44px rgba(var(--primary), .22); width:max-content; }
.intro-fade-enter-active,.intro-fade-leave-active{ transition:opacity .32s ease; } .intro-fade-enter-from,.intro-fade-leave-to{ opacity:0; }
@keyframes introFloat{ 0%,100%{ transform:translate3d(0,0,0) scale(1); } 50%{ transform:translate3d(-18px,18px,0) scale(1.04); } }
@media(max-width:760px){ .intro-card{ grid-template-columns:1fr; min-height:0; border-radius:28px; } .intro-media{ min-height:240px; } .intro-content{ padding:26px; } .intro-start{ width:100%; } }
</style>
