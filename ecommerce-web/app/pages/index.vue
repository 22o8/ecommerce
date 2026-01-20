<script setup lang="ts">
import { computed } from 'vue'
import { useRuntimeConfig } from '#app'
import { useI18n } from '~/composables/useI18n'
import { useAuthStore } from '~/stores/auth'
import { useProductsStore } from '~/stores/products'

const { t } = useI18n()
const auth = useAuthStore()
const products = useProductsStore()
const cfg = useRuntimeConfig()

await products.fetchFeatured()

// صورة الهيرو يمكن تكون رابط خارجي او داخل public/
const heroImage = computed(() => {
  const v = (cfg.public.heroImage as string | undefined) || ''
  return v.trim() || '/hero/images.avif'
})

const featured = computed(() => products.featured)
</script>

<template>
  <main class="page">
    <!-- HERO (صورة خلف النص + طبقة Overlay) -->
    <section class="hero" :style="{ backgroundImage: `url(${heroImage})` }">
      <div class="hero-overlay" />

      <div class="hero-inner">
        <div class="hero-copy">
          <div class="kicker keep-rtl">{{ t('home.split.left.kicker') }}</div>

          <h1 class="title keep-rtl">{{ t('home.split.left.title') }}</h1>
          <p class="desc keep-rtl">{{ t('home.split.left.desc') }}</p>

          <div class="hero-actions">
            <NuxtLink to="/products">
              <AppButton size="lg" variant="primary">{{ t('browseProducts') }}</AppButton>
            </NuxtLink>

            <NuxtLink v-if="!auth.isAuthed" to="/login">
              <AppButton size="lg" variant="ghost">{{ t('login') }}</AppButton>
            </NuxtLink>
            <NuxtLink v-else to="/account">
              <AppButton size="lg" variant="ghost">{{ t('account') }}</AppButton>
            </NuxtLink>
          </div>

          <div class="hero-metrics" aria-label="highlights">
            <div class="metric">
              <div class="metric-ico" aria-hidden="true">⚡</div>
              <div class="metric-body">
                <div class="metric-title keep-rtl">{{ t('home.features.instant.title') }}</div>
                <div class="metric-sub keep-rtl">{{ t('home.features.instant.desc') }}</div>
              </div>
            </div>

            <div class="metric">
              <div class="metric-ico" aria-hidden="true">🔒</div>
              <div class="metric-body">
                <div class="metric-title keep-rtl">{{ t('home.features.secure') }}</div>
                <div class="metric-sub keep-rtl">{{ t('home.features.secureDesc') }}</div>
              </div>
            </div>

            <div class="metric">
              <div class="metric-ico" aria-hidden="true">💬</div>
              <div class="metric-body">
                <div class="metric-title keep-rtl">{{ t('home.features.support') }}</div>
                <div class="metric-sub keep-rtl">{{ t('home.features.supportDesc') }}</div>
              </div>
            </div>
          </div>
        </div>

        <!-- تم نقل (Quick contact) الى صفحة /contact حسب طلبك -->
      </div>

      <div class="hero-glow" aria-hidden="true" />
    </section>

    <!-- FEATURED -->
    <section class="section">
      <div class="section-head">
        <h2 class="section-title keep-rtl">{{ t('home.section.featured') }}</h2>
        <NuxtLink to="/products" class="view-all">{{ t('home.viewAll') }}</NuxtLink>
      </div>

      <div v-if="featured?.length" class="grid">
        <ProductCard v-for="p in featured" :key="p.id" :item="p" />
      </div>

      <EmptyState v-else :title="t('home.empty')" />
    </section>
  </main>
</template>

<style scoped>
.page{
  padding: 24px 0 40px;
}

.hero{
  position: relative;
  border-radius: 28px;
  overflow: hidden;
  border: 1px solid rgb(var(--border));
  box-shadow: var(--shadow2);
  background-size: cover;
  background-position: center;
  min-height: 540px;
}

.hero-overlay{
  position: absolute;
  inset: 0;
  background: linear-gradient(90deg, rgba(255,255,255,.92), rgba(255,255,255,.72) 52%, rgba(255,255,255,.92));
}

:global(.theme-dark) .hero-overlay{
  background: linear-gradient(90deg, rgba(8,10,18,.86), rgba(8,10,18,.68) 52%, rgba(8,10,18,.86));
}

.hero-glow{
  position: absolute;
  inset: -120px;
  background: radial-gradient(circle at 20% 25%, rgba(var(--primary), .22), transparent 55%),
              radial-gradient(circle at 80% 30%, rgba(var(--primary2), .22), transparent 55%),
              radial-gradient(circle at 50% 100%, rgba(var(--primary), .12), transparent 60%);
  pointer-events: none;
}

.hero-inner{
  position: relative;
  z-index: 1;
  display: grid;
  grid-template-columns: 1.55fr .95fr;
  gap: 20px;
  padding: 34px;
}

.hero-copy{
  padding: 14px 10px;
}

.kicker{
  display: inline-flex;
  padding: 8px 12px;
  border: 1px solid rgb(var(--border));
  border-radius: 999px;
  background: rgba(var(--panel), .8);
  font-weight: 900;
  letter-spacing: .2px;
  font-size: 12px;
  width: fit-content;
}

.title{
  margin: 16px 0 10px;
  font-size: clamp(34px, 4.2vw, 58px);
  line-height: 1.03;
  font-weight: 1000;
  letter-spacing: -0.02em;
  color: rgb(var(--text-strong));
  white-space: pre-line;
}

.desc{
  max-width: 58ch;
  color: rgb(var(--text));
  opacity: .9;
  font-size: 15px;
  line-height: 1.75;
}

.hero-actions{
  margin-top: 18px;
  display: flex;
  gap: 10px;
  flex-wrap: wrap;
}

.hero-metrics{
  margin-top: 22px;
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  gap: 12px;
}

.metric{
  display: flex;
  gap: 10px;
  align-items: flex-start;
  padding: 14px 14px;
  border-radius: 18px;
  border: 1px solid rgb(var(--border));
  background: rgba(var(--panel), .86);
  box-shadow: var(--shadow0);
  transition: transform .18s ease, box-shadow .18s ease;
}

.metric:hover{
  transform: translateY(-2px);
  box-shadow: var(--shadow2);
}

.metric-ico{
  width: 40px;
  height: 40px;
  border-radius: 14px;
  display: grid;
  place-items: center;
  background: rgba(var(--primary), .14);
  border: 1px solid rgba(var(--primary), .24);
}

.metric-title{
  font-weight: 1000;
  color: rgb(var(--text-strong));
  font-size: 13px;
}

.metric-sub{
  margin-top: 2px;
  font-size: 12px;
  color: rgb(var(--text));
  opacity: .85;
}

.contact-card{
  border-radius: 22px;
  border: 1px solid rgb(var(--border));
  background: rgba(var(--panel2), .88);
  box-shadow: var(--shadow1);
  padding: 18px;
  display: grid;
  align-content: start;
  gap: 14px;
}

.contact-title{
  font-weight: 1000;
  color: rgb(var(--text-strong));
}

.contact-sub{
  margin-top: 6px;
  color: rgb(var(--text));
  opacity: .85;
  font-size: 13px;
  line-height: 1.6;
}

.contact-chips{
  display: flex;
  flex-wrap: wrap;
  gap: 10px;
}

.chip{
  display: inline-flex;
  align-items: center;
  gap: 8px;
  padding: 10px 12px;
  border-radius: 999px;
  border: 1px solid rgb(var(--border));
  background: rgba(var(--panel), .92);
  text-decoration: none;
  color: rgb(var(--text-strong));
  font-weight: 900;
  transition: transform .18s ease, box-shadow .18s ease;
  box-shadow: var(--shadow0);
}

.chip:hover{ transform: translateY(-1px); box-shadow: var(--shadow2); }

.chip-ico{ display: inline-block; }

.contact-cta{ margin-top: 4px; }

.section{
  margin-top: 26px;
}

.section-head{
  display: flex;
  align-items: baseline;
  justify-content: space-between;
  gap: 16px;
  margin: 0 0 14px;
}

.section-title{
  font-size: 22px;
  font-weight: 1000;
  color: rgb(var(--text-strong));
}

.view-all{
  font-weight: 900;
  text-decoration: none;
  color: rgb(var(--primary));
}

.grid{
  display: grid;
  grid-template-columns: repeat(4, minmax(0, 1fr));
  gap: 16px;
}

@media (max-width: 1100px){
  .grid{ grid-template-columns: repeat(3, minmax(0, 1fr)); }
}

@media (max-width: 900px){
  .hero-inner{ grid-template-columns: 1fr; }
  .hero{ min-height: 620px; }
  .hero-metrics{ grid-template-columns: 1fr; }
  .grid{ grid-template-columns: repeat(2, minmax(0, 1fr)); }
}

@media (max-width: 520px){
  .hero-inner{ padding: 20px; }
  .grid{ grid-template-columns: 1fr; }
}
</style>
