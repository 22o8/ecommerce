<template>
  <Teleport to="body">
    <Transition name="welcome-pop">
      <div v-if="offer" class="new-user-welcome" role="dialog" aria-modal="true" aria-labelledby="new-user-welcome-title" dir="rtl">
        <div class="new-user-welcome__backdrop" @click="close" />
        <article class="new-user-welcome__card">
          <button type="button" class="new-user-welcome__close" aria-label="إغلاق إعلان الترحيب" @click="close">×</button>

          <div v-if="offer.imageUrl" class="new-user-welcome__media">
            <video v-if="isVideoUrl(offer.imageUrl)" :src="assetUrl(offer.imageUrl)" controls playsinline preload="metadata" />
            <img v-else :src="assetUrl(offer.imageUrl)" alt="هدية ترحيب" loading="eager" decoding="async" />
          </div>

          <div class="new-user-welcome__body">
            <span class="new-user-welcome__badge">هدية تسجيل جديدة</span>
            <h2 id="new-user-welcome-title">{{ offer.title || 'مبروك!' }}</h2>
            <p>{{ offer.subtitle || 'مبروك حصلت على خصم و10 نقاط، استمتع بالتسوق داخل التطبيق.' }}</p>

            <div class="new-user-welcome__rewards" v-if="Number(offer.points || 0) > 0 || offer.couponCode">
              <span v-if="Number(offer.points || 0) > 0">+{{ offer.points }} نقطة</span>
              <span v-if="offer.couponCode">كود الخصم: <b class="keep-ltr">{{ offer.couponCode }}</b></span>
            </div>

            <button type="button" class="new-user-welcome__action" @click="closeAndGo">
              <Icon name="mdi:shopping-outline" />
              <span>ابدأ التسوق</span>
            </button>
          </div>
        </article>
      </div>
    </Transition>
  </Teleport>
</template>

<script setup lang="ts">
const api = useApi()
const router = useRouter()
const offer = ref<any | null>(null)
const storageKey = 'drsb_pending_welcome_offer'

function assetUrl(url: string) {
  return api.buildAssetUrl(url || '')
}

function isVideoUrl(url: string) {
  return /\.(mp4|webm|ogg)(\?|#|$)/i.test(url || '')
}

function readPendingOffer() {
  if (!process.client) return
  try {
    const raw = sessionStorage.getItem(storageKey)
    if (!raw) return
    const parsed = JSON.parse(raw)
    if (parsed && typeof parsed === 'object') offer.value = parsed
  } catch {
    sessionStorage.removeItem(storageKey)
  }
}

function close() {
  offer.value = null
  if (process.client) sessionStorage.removeItem(storageKey)
}

function closeAndGo() {
  close()
  router.push('/products')
}

function onCustomEvent() {
  readPendingOffer()
}

onMounted(() => {
  readPendingOffer()
  window.addEventListener('drsb:welcome-offer', onCustomEvent)
})

onBeforeUnmount(() => {
  window.removeEventListener('drsb:welcome-offer', onCustomEvent)
})
</script>

<style scoped>
.new-user-welcome{position:fixed;inset:0;z-index:9999;display:grid;place-items:center;padding:1rem;}
.new-user-welcome__backdrop{position:absolute;inset:0;background:rgba(2,6,15,.82);backdrop-filter:blur(12px);}
.new-user-welcome__card{position:relative;width:min(94vw,540px);overflow:hidden;border-radius:32px;border:1px solid rgba(255,255,255,.14);background:#0b0f19;color:#fff;box-shadow:0 35px 120px rgba(0,0,0,.62),0 0 0 1px rgba(255,255,255,.04) inset;}
.new-user-welcome__close{position:absolute;top:.8rem;left:.8rem;z-index:3;width:42px;height:42px;border-radius:999px;border:1px solid rgba(255,255,255,.18);background:rgba(10,15,26,.95);color:#fff;font-size:1.35rem;line-height:1;}
.new-user-welcome__media{background:#111827;max-height:300px;}
.new-user-welcome__media img,.new-user-welcome__media video{display:block;width:100%;max-height:300px;object-fit:cover;}
.new-user-welcome__body{padding:1.55rem;text-align:center;background:linear-gradient(180deg,#0b0f19,#101726);}
.new-user-welcome__badge{display:inline-flex;border-radius:999px;padding:.38rem .8rem;background:rgba(236,72,153,.16);color:#f9a8d4;font-size:.82rem;font-weight:1000;}
.new-user-welcome__body h2{margin:.75rem 0 .25rem;font-size:clamp(1.7rem,5vw,2.35rem);font-weight:1000;}
.new-user-welcome__body p{margin:0 auto;color:#d8deec;line-height:1.85;max-width:28rem;font-weight:800;}
.new-user-welcome__rewards{display:flex;flex-wrap:wrap;justify-content:center;gap:.6rem;margin-top:1rem;}
.new-user-welcome__rewards span{border-radius:999px;padding:.5rem .85rem;background:rgba(16,185,129,.16);color:#34d399;font-weight:1000;}
.new-user-welcome__action{margin-top:1.15rem;display:inline-flex;align-items:center;justify-content:center;gap:.5rem;border-radius:999px;background:rgb(var(--primary));color:#080b12;padding:.85rem 1.35rem;font-weight:1000;box-shadow:0 16px 35px rgba(var(--primary),.25);}
.welcome-pop-enter-active,.welcome-pop-leave-active{transition:.22s ease;}
.welcome-pop-enter-from,.welcome-pop-leave-to{opacity:0;transform:scale(.98);}
@media(max-width:640px){.new-user-welcome__card{border-radius:26px}.new-user-welcome__body{padding:1.25rem}.new-user-welcome__media,.new-user-welcome__media img,.new-user-welcome__media video{max-height:240px}}
</style>
