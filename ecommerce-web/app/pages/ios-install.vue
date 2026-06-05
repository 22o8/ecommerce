<template>
  <main class="ios-install-page rtl-text">
    <section class="ios-hero">
      <NuxtLink to="/install-app" class="ios-back">
        <Icon name="mdi:arrow-right" />
        رجوع
      </NuxtLink>

      <div class="ios-hero-content">
        <span class="ios-kicker">
          <Icon name="mdi:apple-ios" />
          iPhone / iPad
        </span>

        <div class="ios-logo-row">
          <img src="/apple-touch-icon.png" alt="DR SEOUL BEAUTY" class="ios-logo" />
          <div>
            <strong>DR SEOUL BEAUTY</strong>
            <span>تثبيت المتجر على الشاشة الرئيسية</span>
          </div>
        </div>

        <h1>طريقة تثبيت التطبيق على الآيفون</h1>
        <p>
          آبل لا تسمح للمواقع بإضافة الأيقونة تلقائياً، لذلك اتبع هذه الخطوات المصورة مرة واحدة فقط، بعدها سيفتح المتجر مثل تطبيق من شاشة الهاتف.
        </p>

        <div class="ios-actions">
          <button type="button" class="ios-primary" @click="copyStoreLink">
            <Icon name="mdi:content-copy" />
            نسخ رابط المتجر
          </button>
          <NuxtLink to="/" class="ios-secondary">
            <Icon name="mdi:storefront-outline" />
            فتح المتجر
          </NuxtLink>
        </div>

        <p v-if="copied" class="ios-copy-note">
          تم نسخ الرابط. افتحه من Safari ثم طبق الخطوات المصورة بالأسفل.
        </p>
      </div>

      <div class="ios-phone-demo" aria-hidden="true">
        <div class="iphone-frame">
          <div class="iphone-notch"></div>
          <div class="iphone-browser">
            <div class="browser-top">
              <span>ecommerce-kappa-ivory.vercel.app</span>
            </div>
            <div class="browser-card">
              <img src="/apple-touch-icon.png" alt="" />
              <strong>DR SEOUL BEAUTY</strong>
              <small>Beauty Store</small>
            </div>
            <div class="share-highlight">
              <Icon name="mdi:export-variant" />
            </div>
          </div>
        </div>
      </div>
    </section>

    <section class="ios-visual-steps">
      <article class="visual-step">
        <div class="step-number">1</div>
        <div class="step-illustration safari">
          <div class="mini-phone">
            <div class="mini-bar"></div>
            <Icon name="mdi:apple-safari" />
            <span>Safari</span>
          </div>
        </div>
        <h2>افتح الرابط من Safari</h2>
        <p>إذا كان الرابط مفتوحاً داخل Instagram أو Chrome، انسخه وافتحه داخل Safari أولاً.</p>
      </article>

      <article class="visual-step">
        <div class="step-number">2</div>
        <div class="step-illustration share">
          <div class="share-sheet">
            <Icon name="mdi:export-variant" />
            <span>Share</span>
          </div>
        </div>
        <h2>اضغط زر المشاركة</h2>
        <p>زر المشاركة يكون أسفل الشاشة غالباً، أو أعلى المتصفح حسب إصدار iOS.</p>
      </article>

      <article class="visual-step">
        <div class="step-number">3</div>
        <div class="step-illustration addhome">
          <div class="home-card">
            <Icon name="mdi:plus-box-outline" />
            <span>Add to Home Screen</span>
          </div>
        </div>
        <h2>اختر Add to Home Screen</h2>
        <p>من القائمة اختر إضافة إلى الشاشة الرئيسية، ثم اضغط Add بالأعلى.</p>
      </article>

      <article class="visual-step">
        <div class="step-number">4</div>
        <div class="step-illustration final">
          <div class="app-icon-preview">
            <img src="/apple-touch-icon.png" alt="" />
          </div>
          <span class="done-badge">تم</span>
        </div>
        <h2>افتح المتجر من الأيقونة</h2>
        <p>ستظهر أيقونة المتجر على الشاشة الرئيسية، وافتحه بعدها كتطبيق مستقل.</p>
      </article>
    </section>

    <section class="ios-tips">
      <h2>ملاحظات مهمة</h2>
      <div class="tips-grid">
        <div>
          <Icon name="mdi:check-circle-outline" />
          <span>الطريقة تعمل فقط من Safari على iPhone و iPad.</span>
        </div>
        <div>
          <Icon name="mdi:check-circle-outline" />
          <span>إذا لم يظهر الخيار، حدّث iOS أو افتح الرابط من Safari مباشرة.</span>
        </div>
        <div>
          <Icon name="mdi:check-circle-outline" />
          <span>بعد التثبيت لا تحتاج الدخول للمتصفح كل مرة.</span>
        </div>
      </div>
    </section>
  </main>
</template>

<script setup lang="ts">
import { ref } from 'vue'

useHead({
  title: 'تثبيت تطبيق DR SEOUL BEAUTY على الآيفون',
  meta: [
    { name: 'description', content: 'شرح مصور لتثبيت متجر DR SEOUL BEAUTY على شاشة الآيفون.' }
  ]
})

const copied = ref(false)

async function copyStoreLink() {
  copied.value = false
  const url = window.location.origin
  try {
    await navigator.clipboard.writeText(url)
  } catch {}
  copied.value = true
}
</script>

<style scoped>
.ios-install-page{
  min-height:100vh;
  padding:clamp(1rem,3vw,2.5rem);
  display:grid;
  gap:1.25rem;
  background:
    radial-gradient(circle at 8% 10%, rgba(var(--primary),.22), transparent 30%),
    radial-gradient(circle at 92% 82%, rgba(236,72,153,.18), transparent 35%),
    rgb(var(--bg));
  color:rgb(var(--text));
}
.ios-hero,.ios-visual-steps,.ios-tips{
  width:min(1180px,100%);
  margin:0 auto;
}
.ios-hero{
  position:relative;
  overflow:hidden;
  min-height:520px;
  display:grid;
  grid-template-columns:1.1fr .9fr;
  gap:2rem;
  align-items:center;
  border:1px solid rgb(var(--border));
  border-radius:2.2rem;
  padding:clamp(1.5rem,4vw,3.5rem);
  background:
    linear-gradient(135deg, rgba(var(--card),.96), rgba(var(--surface),.78)),
    radial-gradient(circle at 85% 30%, rgba(var(--primary),.18), transparent 34%);
  box-shadow:0 34px 120px rgba(0,0,0,.20);
}
.ios-back{
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
  background:rgba(var(--surface),.86);
  backdrop-filter:blur(16px);
  font-weight:1000;
}
.ios-kicker{
  display:inline-flex;
  align-items:center;
  gap:.45rem;
  padding:.45rem .85rem;
  border-radius:999px;
  color:rgb(var(--primary));
  border:1px solid rgba(var(--primary),.35);
  background:rgba(var(--primary),.10);
  font-weight:1000;
}
.ios-logo-row{
  display:flex;
  align-items:center;
  gap:.85rem;
  margin-top:1.15rem;
}
.ios-logo{
  width:72px;
  height:72px;
  border-radius:1.35rem;
  object-fit:cover;
  border:1px solid rgb(var(--border));
  box-shadow:0 14px 35px rgba(0,0,0,.15);
}
.ios-logo-row strong{ display:block; font-weight:1000; font-size:1.05rem; }
.ios-logo-row span{ color:rgb(var(--muted)); font-size:.9rem; }
.ios-hero h1{
  max-width:760px;
  margin-top:1.2rem;
  font-size:clamp(2.4rem,6vw,5rem);
  line-height:1;
  font-weight:1000;
  letter-spacing:-.05em;
}
.ios-hero p{
  max-width:680px;
  margin-top:1rem;
  color:rgb(var(--muted));
  line-height:1.95;
  font-size:1.05rem;
}
.ios-actions{
  display:flex;
  flex-wrap:wrap;
  gap:.75rem;
  margin-top:1.4rem;
}
.ios-primary,.ios-secondary{
  min-height:52px;
  padding:0 1.15rem;
  border-radius:1rem;
  display:inline-flex;
  align-items:center;
  justify-content:center;
  gap:.5rem;
  font-weight:1000;
  border:1px solid transparent;
  transition:.2s ease;
}
.ios-primary{
  color:white;
  background:linear-gradient(135deg, rgb(var(--primary)), #ec4899);
  box-shadow:0 18px 46px rgba(var(--primary),.28);
}
.ios-secondary{
  color:rgb(var(--text));
  background:rgba(var(--surface),.88);
  border-color:rgb(var(--border));
}
.ios-primary:hover,.ios-secondary:hover{ transform:translateY(-2px); }
.ios-copy-note{
  display:inline-flex;
  margin-top:1rem;
  color:rgb(var(--primary));
  font-weight:900;
}
.ios-phone-demo{
  display:grid;
  place-items:center;
}
.iphone-frame{
  width:min(310px,82vw);
  aspect-ratio:9/18;
  border-radius:2.5rem;
  padding:.75rem;
  background:linear-gradient(145deg,#1f2937,#030712);
  box-shadow:0 34px 90px rgba(0,0,0,.35);
  transform:rotate(4deg);
}
.iphone-notch{
  width:95px;
  height:24px;
  border-radius:999px;
  background:#030712;
  margin:0 auto .55rem;
}
.iphone-browser{
  position:relative;
  height:calc(100% - 34px);
  border-radius:1.85rem;
  overflow:hidden;
  padding:1rem;
  display:grid;
  align-content:start;
  justify-items:center;
  background:
    radial-gradient(circle at 50% 20%, rgba(168,85,247,.26), transparent 34%),
    linear-gradient(180deg,#111827,#05070d);
}
.browser-top{
  width:100%;
  min-height:36px;
  display:flex;
  align-items:center;
  justify-content:center;
  border-radius:999px;
  background:rgba(255,255,255,.08);
  color:#cbd5e1;
  font-size:.62rem;
}
.browser-card{
  width:100%;
  margin-top:2rem;
  border-radius:1.4rem;
  padding:1rem;
  display:grid;
  justify-items:center;
  background:rgba(255,255,255,.08);
  border:1px solid rgba(255,255,255,.10);
  color:white;
}
.browser-card img{ width:74px; height:74px; object-fit:cover; border-radius:1.2rem; }
.browser-card strong{ margin-top:.8rem; }
.browser-card small{ color:#cbd5e1; }
.share-highlight{
  position:absolute;
  bottom:1.2rem;
  left:50%;
  transform:translateX(-50%);
  width:58px;
  height:58px;
  border-radius:999px;
  display:grid;
  place-items:center;
  color:white;
  background:linear-gradient(135deg, rgb(var(--primary)), #ec4899);
  box-shadow:0 0 0 10px rgba(168,85,247,.15), 0 18px 42px rgba(0,0,0,.35);
  font-size:1.6rem;
}
.ios-visual-steps{
  display:grid;
  grid-template-columns:repeat(4,minmax(0,1fr));
  gap:1rem;
}
.visual-step{
  position:relative;
  min-height:320px;
  overflow:hidden;
  border:1px solid rgb(var(--border));
  border-radius:1.7rem;
  padding:1.2rem;
  background:rgba(var(--card),.84);
  box-shadow:0 18px 60px rgba(0,0,0,.08);
}
.step-number{
  width:42px;
  height:42px;
  border-radius:999px;
  display:grid;
  place-items:center;
  color:white;
  background:linear-gradient(135deg, rgb(var(--primary)), #ec4899);
  font-weight:1000;
}
.step-illustration{
  height:132px;
  margin:1rem 0;
  border-radius:1.4rem;
  display:grid;
  place-items:center;
  background:
    radial-gradient(circle at 50% 30%, rgba(var(--primary),.18), transparent 45%),
    rgba(var(--surface),.72);
  border:1px dashed rgba(var(--primary),.35);
}
.mini-phone,.share-sheet,.home-card{
  display:grid;
  place-items:center;
  gap:.4rem;
  padding:1rem;
  min-width:150px;
  border-radius:1.2rem;
  background:rgb(var(--card));
  border:1px solid rgb(var(--border));
  box-shadow:0 14px 34px rgba(0,0,0,.10);
  font-weight:1000;
}
.mini-bar{
  width:72px;
  height:8px;
  border-radius:999px;
  background:rgba(var(--primary),.22);
}
.step-illustration svg{ font-size:2.1rem; color:rgb(var(--primary)); }
.app-icon-preview{
  width:84px;
  height:84px;
  border-radius:1.45rem;
  overflow:hidden;
  border:1px solid rgb(var(--border));
  box-shadow:0 14px 34px rgba(0,0,0,.14);
}
.app-icon-preview img{ width:100%; height:100%; object-fit:cover; }
.done-badge{
  margin-top:.5rem;
  border-radius:999px;
  padding:.35rem .7rem;
  color:#16a34a;
  background:rgba(22,163,74,.12);
  font-weight:1000;
}
.visual-step h2{
  font-size:1.1rem;
  font-weight:1000;
  margin-bottom:.4rem;
}
.visual-step p{
  color:rgb(var(--muted));
  line-height:1.75;
  font-size:.94rem;
}
.ios-tips{
  border:1px solid rgb(var(--border));
  border-radius:1.7rem;
  padding:1.3rem;
  background:rgba(var(--card),.78);
}
.ios-tips h2{ font-weight:1000; font-size:1.3rem; margin-bottom:1rem; }
.tips-grid{
  display:grid;
  grid-template-columns:repeat(3,minmax(0,1fr));
  gap:.8rem;
}
.tips-grid div{
  display:flex;
  align-items:flex-start;
  gap:.55rem;
  color:rgb(var(--muted));
  line-height:1.7;
}
.tips-grid svg{ color:#22c55e; flex:0 0 auto; margin-top:.2rem; }

@media (max-width:1000px){
  .ios-hero{ grid-template-columns:1fr; padding-top:4rem; }
  .ios-visual-steps{ grid-template-columns:repeat(2,minmax(0,1fr)); }
}
@media (max-width:640px){
  .ios-visual-steps,.tips-grid{ grid-template-columns:1fr; }
  .iphone-frame{ width:230px; }
}
</style>
