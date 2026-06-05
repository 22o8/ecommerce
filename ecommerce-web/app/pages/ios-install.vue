<template>
  <main class="ios-install-page rtl-text">
    <section class="ios-install-hero">
      <NuxtLink to="/intro" class="ios-back">
        <Icon name="mdi:arrow-right" />
        رجوع
      </NuxtLink>

      <div class="ios-logo-wrap">
        <img src="/apple-touch-icon.png" alt="Dr.Seoul Beauty" class="ios-logo" />
      </div>

      <span class="ios-badge">iPhone / iPad</span>
      <h1>تثبيت التطبيق على الآيفون</h1>
      <p>
        بسبب قيود iOS لا يمكن للموقع إنشاء الأيقونة تلقائياً، لكن يمكنك تثبيت المتجر بخطوات بسيطة من Safari.
      </p>
    </section>

    <section class="ios-steps">
      <article class="ios-step">
        <span>1</span>
        <div>
          <h2>افتح الموقع من Safari</h2>
          <p>تأكد أنك تستخدم متصفح Safari وليس Chrome أو Instagram Browser.</p>
        </div>
      </article>

      <article class="ios-step">
        <span>2</span>
        <div>
          <h2>اضغط زر المشاركة</h2>
          <p>زر المشاركة يكون أسفل الشاشة أو أعلى المتصفح حسب إصدار iOS.</p>
        </div>
      </article>

      <article class="ios-step">
        <span>3</span>
        <div>
          <h2>اختر إضافة إلى الشاشة الرئيسية</h2>
          <p>اختر Add to Home Screen ثم اضغط Add، وستظهر أيقونة المتجر كتطبيق.</p>
        </div>
      </article>
    </section>

    <section class="ios-actions">
      <button type="button" class="ios-primary" @click="copyCurrentLink">
        <Icon name="mdi:content-copy" />
        نسخ رابط المتجر
      </button>
      <NuxtLink to="/" class="ios-secondary">
        فتح المتجر
      </NuxtLink>
    </section>

    <p v-if="copied" class="ios-copy-note">تم نسخ الرابط، افتحه من Safari ثم طبق الخطوات أعلاه.</p>
  </main>
</template>

<script setup lang="ts">
import { ref } from 'vue'

useHead({ title: 'تثبيت تطبيق Dr.Seoul Beauty' })

const copied = ref(false)

async function copyCurrentLink() {
  copied.value = false
  const url = window.location.origin + '/intro'
  try {
    await navigator.clipboard.writeText(url)
    copied.value = true
  } catch {
    copied.value = true
  }
}
</script>

<style scoped>
.ios-install-page{
  min-height:100vh;
  padding:clamp(1.2rem,3vw,2.5rem);
  display:grid;
  align-content:center;
  gap:1.25rem;
  background:
    radial-gradient(circle at 12% 18%, rgba(var(--primary),.22), transparent 30%),
    radial-gradient(circle at 85% 82%, rgba(236,72,153,.16), transparent 34%),
    rgb(var(--bg));
  color:rgb(var(--text));
}
.ios-install-hero,.ios-steps,.ios-actions{
  width:min(920px,100%);
  margin-inline:auto;
}
.ios-install-hero{
  position:relative;
  border:1px solid rgb(var(--border));
  border-radius:2rem;
  padding:clamp(1.4rem,4vw,2.4rem);
  background:linear-gradient(135deg, rgb(var(--card)), rgba(var(--primary),.08));
  box-shadow:0 28px 80px rgba(0,0,0,.14);
}
.ios-back{
  position:absolute;
  top:1rem;
  inset-inline-start:1rem;
  display:inline-flex;
  align-items:center;
  gap:.35rem;
  min-height:40px;
  border-radius:999px;
  padding:0 .9rem;
  background:rgb(var(--surface));
  border:1px solid rgb(var(--border));
  font-weight:900;
}
.ios-logo-wrap{
  width:5.4rem;
  height:5.4rem;
  border-radius:1.6rem;
  display:grid;
  place-items:center;
  background:rgb(var(--surface));
  border:1px solid rgb(var(--border));
  overflow:hidden;
  margin-bottom:1rem;
}
.ios-logo{ width:100%; height:100%; object-fit:cover; }
.ios-badge{
  display:inline-flex;
  border-radius:999px;
  padding:.35rem .7rem;
  color:rgb(var(--primary));
  background:rgba(var(--primary),.13);
  font-weight:1000;
  font-size:.82rem;
}
.ios-install-hero h1{
  margin-top:.9rem;
  font-size:clamp(2rem,6vw,4.2rem);
  line-height:1;
  font-weight:1000;
  letter-spacing:-.04em;
}
.ios-install-hero p{
  margin-top:1rem;
  max-width:680px;
  color:rgb(var(--muted));
  line-height:1.9;
  font-size:1.03rem;
}
.ios-steps{
  display:grid;
  grid-template-columns:repeat(3,minmax(0,1fr));
  gap:1rem;
}
.ios-step{
  display:flex;
  gap:.9rem;
  border:1px solid rgb(var(--border));
  border-radius:1.4rem;
  padding:1rem;
  background:rgb(var(--card));
}
.ios-step span{
  flex:0 0 auto;
  width:2.4rem;
  height:2.4rem;
  border-radius:999px;
  display:grid;
  place-items:center;
  background:rgb(var(--primary));
  color:#08070b;
  font-weight:1000;
}
.ios-step h2{
  font-size:1rem;
  font-weight:1000;
}
.ios-step p{
  margin-top:.35rem;
  color:rgb(var(--muted));
  line-height:1.7;
  font-size:.9rem;
}
.ios-actions{
  display:flex;
  flex-wrap:wrap;
  gap:.75rem;
}
.ios-primary,.ios-secondary{
  min-height:50px;
  border-radius:999px;
  padding:0 1.2rem;
  display:inline-flex;
  align-items:center;
  justify-content:center;
  gap:.4rem;
  font-weight:1000;
}
.ios-primary{
  background:rgb(var(--primary));
  color:#08070b;
}
.ios-secondary{
  background:rgb(var(--surface));
  border:1px solid rgb(var(--border));
}
.ios-copy-note{
  width:min(920px,100%);
  margin:0 auto;
  padding:.9rem 1rem;
  border-radius:1rem;
  border:1px solid rgba(var(--primary),.25);
  background:rgba(var(--primary),.11);
  color:rgb(var(--text));
}
@media(max-width:820px){
  .ios-steps{ grid-template-columns:1fr; }
  .ios-install-hero{ padding-top:4rem; }
}
</style>
