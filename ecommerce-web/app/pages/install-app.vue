<template>
  <main class="install-page rtl-text">
    <section class="install-hero">
      <div class="install-bg-glow one"></div>
      <div class="install-bg-glow two"></div>

      <NuxtLink to="/intro" class="install-back">
        <Icon name="mdi:arrow-right" />
        رجوع
      </NuxtLink>

      <div class="install-copy">
        <span class="install-kicker">
          <Icon name="mdi:cellphone-arrow-down" />
          تطبيق المتجر
        </span>

        <h1>ثبّت Dr Seoul Beauty على هاتفك</h1>
        <p>
          احصل على تجربة أسرع وأسهل من المتصفح: أيقونة على الشاشة الرئيسية، فتح مباشر للمتجر، وتصميم يعمل مثل التطبيق.
        </p>

        <div class="install-actions">
          <button type="button" class="install-primary" @click="installAndroid">
            <Icon name="mdi:android" />
            تحميل مباشر للأندرويد
          </button>

          <NuxtLink to="/ios-install" class="install-secondary">
            <Icon name="mdi:apple-ios" />
            تثبيت على الآيفون
          </NuxtLink>
        </div>

        <div class="install-note" v-if="androidMessage">
          <Icon name="mdi:information-outline" />
          {{ androidMessage }}
        </div>
      </div>

      <div class="phone-preview" aria-hidden="true">
        <div class="phone-shell">
          <div class="phone-top"></div>
          <div class="phone-screen">
            <img src="/apple-touch-icon.png" alt="" class="phone-logo" />
            <strong>DR SEOUL BEAUTY</strong>
            <span>Beauty Store</span>
            <div class="phone-row"></div>
            <div class="phone-grid">
              <span></span><span></span><span></span>
              <span></span><span></span><span></span>
            </div>
          </div>
        </div>
      </div>
    </section>

    <section class="install-options">
      <article class="install-card android-card">
        <div class="install-card-icon">
          <Icon name="mdi:android" />
        </div>
        <h2>Android</h2>
        <p>اضغط زر التثبيت، وسيظهر خيار إضافة التطبيق للشاشة الرئيسية مباشرة إذا كان المتصفح يدعم ذلك.</p>
        <button type="button" @click="installAndroid">
          تثبيت الآن
          <Icon name="mdi:download" />
        </button>
      </article>

      <article class="install-card ios-card">
        <div class="install-card-icon">
          <Icon name="mdi:apple-ios" />
        </div>
        <h2>iPhone / iPad</h2>
        <p>الآيفون يحتاج خطوة يدوية من Safari، جهزنا لك شرح مصور واضح وسريع.</p>
        <NuxtLink to="/ios-install">
          مشاهدة الشرح
          <Icon name="mdi:arrow-left" />
        </NuxtLink>
      </article>
    </section>
  </main>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'

useHead({
  title: 'تثبيت تطبيق DR SEOUL BEAUTY',
  meta: [
    { name: 'description', content: 'ثبت متجر DR SEOUL BEAUTY كتطبيق على هاتفك للأندرويد والآيفون.' }
  ]
})

const deferredPrompt = ref<any>(null)
const androidMessage = ref('')

onMounted(() => {
  window.addEventListener('beforeinstallprompt', (event: any) => {
    event.preventDefault()
    deferredPrompt.value = event
  })
})

async function installAndroid() {
  androidMessage.value = ''

  if (deferredPrompt.value) {
    deferredPrompt.value.prompt()
    const choice = await deferredPrompt.value.userChoice
    deferredPrompt.value = null
    androidMessage.value = choice?.outcome === 'accepted'
      ? 'تم إرسال طلب التثبيت بنجاح.'
      : 'تم إلغاء التثبيت، يمكنك المحاولة لاحقاً.'
    return
  }

  androidMessage.value = 'إذا لم تظهر نافذة التثبيت، افتح القائمة ⋮ في Chrome واختر: تثبيت التطبيق أو Add to Home Screen.'
}
</script>

<style scoped>
.install-page{
  min-height:100vh;
  padding:clamp(1rem,3vw,2.5rem);
  display:grid;
  gap:1.25rem;
  background:
    radial-gradient(circle at 8% 10%, rgba(var(--primary),.22), transparent 30%),
    radial-gradient(circle at 90% 75%, rgba(236,72,153,.18), transparent 35%),
    rgb(var(--bg));
  color:rgb(var(--text));
}
.install-hero{
  width:min(1180px,100%);
  margin:0 auto;
  min-height:540px;
  position:relative;
  overflow:hidden;
  display:grid;
  grid-template-columns:1.1fr .9fr;
  align-items:center;
  gap:2rem;
  border:1px solid rgb(var(--border));
  border-radius:2.2rem;
  padding:clamp(1.5rem,4vw,3.5rem);
  background:
    linear-gradient(135deg, rgba(var(--card),.94), rgba(var(--surface),.78)),
    radial-gradient(circle at 78% 30%, rgba(var(--primary),.16), transparent 32%);
  box-shadow:0 34px 120px rgba(0,0,0,.20);
}
.install-bg-glow{
  position:absolute;
  width:280px;
  height:280px;
  border-radius:999px;
  filter:blur(55px);
  opacity:.55;
  pointer-events:none;
}
.install-bg-glow.one{ background:rgba(var(--primary),.45); inset-block-start:-80px; inset-inline-end:18%; }
.install-bg-glow.two{ background:rgba(236,72,153,.35); inset-block-end:-100px; inset-inline-start:8%; }
.install-back{
  position:absolute;
  inset-block-start:1rem;
  inset-inline-start:1rem;
  z-index:2;
  display:inline-flex;
  align-items:center;
  gap:.35rem;
  min-height:42px;
  padding:0 .95rem;
  border-radius:999px;
  border:1px solid rgb(var(--border));
  background:rgba(var(--surface),.82);
  backdrop-filter:blur(16px);
  font-weight:1000;
}
.install-kicker{
  display:inline-flex;
  align-items:center;
  gap:.45rem;
  padding:.45rem .85rem;
  border-radius:999px;
  color:rgb(var(--primary));
  border:1px solid rgba(var(--primary),.35);
  background:rgba(var(--primary),.10);
  font-weight:1000;
  margin-bottom:1rem;
}
.install-copy h1{
  max-width:760px;
  font-size:clamp(2.5rem,6vw,5.6rem);
  line-height:1;
  letter-spacing:-.05em;
  font-weight:1000;
}
.install-copy p{
  max-width:680px;
  margin-top:1rem;
  color:rgb(var(--muted));
  line-height:1.95;
  font-size:1.08rem;
}
.install-actions{
  display:flex;
  flex-wrap:wrap;
  gap:.8rem;
  margin-top:1.5rem;
}
.install-primary,.install-secondary,.install-card button,.install-card a{
  min-height:52px;
  padding:0 1.25rem;
  border-radius:1rem;
  display:inline-flex;
  align-items:center;
  justify-content:center;
  gap:.5rem;
  font-weight:1000;
  border:1px solid transparent;
  transition:.2s ease;
}
.install-primary,.install-card button{
  color:white;
  background:linear-gradient(135deg, rgb(var(--primary)), #ec4899);
  box-shadow:0 18px 46px rgba(var(--primary),.28);
}
.install-secondary,.install-card a{
  color:rgb(var(--text));
  background:rgba(var(--surface),.86);
  border-color:rgb(var(--border));
}
.install-primary:hover,.install-secondary:hover,.install-card button:hover,.install-card a:hover{
  transform:translateY(-2px);
}
.install-note{
  margin-top:1rem;
  display:inline-flex;
  align-items:center;
  gap:.5rem;
  color:rgb(var(--muted));
  background:rgba(var(--surface),.7);
  border:1px solid rgb(var(--border));
  border-radius:1rem;
  padding:.8rem 1rem;
}
.phone-preview{
  position:relative;
  display:grid;
  place-items:center;
  min-height:430px;
}
.phone-shell{
  width:min(290px,82vw);
  aspect-ratio:9/18;
  border-radius:2.4rem;
  padding:.8rem;
  background:linear-gradient(145deg,#1f2937,#05070d);
  box-shadow:0 34px 90px rgba(0,0,0,.35);
  transform:rotate(-5deg);
}
.phone-top{
  width:90px;
  height:22px;
  border-radius:999px;
  background:#030712;
  margin:0 auto .5rem;
}
.phone-screen{
  height:calc(100% - 30px);
  border-radius:1.8rem;
  display:grid;
  align-content:start;
  justify-items:center;
  padding:2rem 1rem;
  background:
    radial-gradient(circle at 50% 18%, rgba(168,85,247,.30), transparent 32%),
    linear-gradient(180deg,#111827,#05070d);
  overflow:hidden;
  color:white;
}
.phone-logo{
  width:86px;
  height:86px;
  border-radius:1.5rem;
  object-fit:cover;
  box-shadow:0 14px 36px rgba(0,0,0,.28);
}
.phone-screen strong{ margin-top:1rem; font-size:1rem; }
.phone-screen span{ color:#cbd5e1; font-size:.85rem; margin-top:.25rem; }
.phone-row{
  width:100%;
  height:80px;
  margin-top:1.4rem;
  border-radius:1.2rem;
  background:rgba(255,255,255,.08);
}
.phone-grid{
  width:100%;
  display:grid;
  grid-template-columns:repeat(3,1fr);
  gap:.6rem;
  margin-top:.8rem;
}
.phone-grid span{
  height:54px;
  border-radius:1rem;
  background:rgba(255,255,255,.09);
}
.install-options,.install-benefits{
  width:min(1180px,100%);
  margin:0 auto;
  display:grid;
  gap:1rem;
}
.install-options{ grid-template-columns:repeat(2,minmax(0,1fr)); }
.install-card,.install-benefits div{
  border:1px solid rgb(var(--border));
  border-radius:1.6rem;
  padding:1.35rem;
  background:rgba(var(--card),.82);
  box-shadow:0 18px 60px rgba(0,0,0,.08);
}
.install-card-icon{
  width:56px;
  height:56px;
  border-radius:1.2rem;
  display:grid;
  place-items:center;
  font-size:1.6rem;
  color:white;
  background:linear-gradient(135deg, rgb(var(--primary)), #ec4899);
}
.install-card h2{ font-size:1.35rem; margin-top:1rem; font-weight:1000; }
.install-card p{ color:rgb(var(--muted)); line-height:1.8; margin:.45rem 0 1rem; }
.install-benefits{ grid-template-columns:repeat(3,minmax(0,1fr)); }
.install-benefits div{
  display:grid;
  gap:.4rem;
}
.install-benefits svg{ color:rgb(var(--primary)); font-size:1.4rem; }
.install-benefits strong{ font-weight:1000; }
.install-benefits span{ color:rgb(var(--muted)); font-size:.92rem; }

@media (max-width:900px){
  .install-hero{ grid-template-columns:1fr; padding-top:4rem; }
  .phone-preview{ min-height:320px; }
  .phone-shell{ width:220px; }
  .install-options,.install-benefits{ grid-template-columns:1fr; }
}
</style>
