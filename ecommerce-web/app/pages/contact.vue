<script setup lang="ts">
import { computed } from 'vue'
import { useI18n } from '~/composables/useI18n'

const { t } = useI18n()
const cfg = useRuntimeConfig()

const phone = computed(() => (cfg.public.supportPhone as string | undefined) || '')
const email = computed(() => (cfg.public.supportEmail as string | undefined) || '')
const instagramUrl = computed(() => (cfg.public.instagramUrl as string | undefined) || '')

const whatsappUrl = computed(() => {
  if (!phone.value) return ''
  const normalized = phone.value.replace(/\s+/g, '')
  const noPlus = normalized.startsWith('+') ? normalized.slice(1) : normalized
  return `https://wa.me/${noPlus}`
})
</script>

<template>
  <main class="page">
    <section class="card shell">
      <div class="head">
        <div class="kicker keep-rtl">{{ t('contactPage.kicker') }}</div>
        <h1 class="title keep-rtl">{{ t('contactPage.title') }}</h1>
        <p class="sub keep-rtl">{{ t('contactPage.sub') }}</p>
      </div>

      <div class="grid">
        <div class="pane">
          <div class="row" v-if="phone">
            <div class="ico" aria-hidden="true">📞</div>
            <div class="body">
              <div class="label keep-rtl">{{ t('contactPage.phone') }}</div>
              <a class="value keep-ltr" :href="`tel:${phone}`">{{ phone }}</a>
            </div>
            <a v-if="whatsappUrl" class="btn" :href="whatsappUrl" target="_blank" rel="noopener">
              WhatsApp
            </a>
          </div>

          <div class="row" v-if="email">
            <div class="ico" aria-hidden="true">✉️</div>
            <div class="body">
              <div class="label keep-rtl">{{ t('contactPage.email') }}</div>
              <a class="value keep-ltr" :href="`mailto:${email}`">{{ email }}</a>
            </div>
            <a class="btn" :href="`mailto:${email}`">{{ t('contactPage.send') }}</a>
          </div>

          <div class="row" v-if="instagramUrl">
            <div class="ico" aria-hidden="true">📷</div>
            <div class="body">
              <div class="label keep-rtl">{{ t('contactPage.instagram') }}</div>
              <a class="value keep-ltr" :href="instagramUrl" target="_blank" rel="noopener">{{ t('contactPage.openInstagram') }}</a>
            </div>
            <a class="btn" :href="instagramUrl" target="_blank" rel="noopener">Instagram</a>
          </div>

          <div class="note keep-rtl">
            <div class="noteTitle">{{ t('contactPage.noteTitle') }}</div>
            <div class="noteBody">{{ t('contactPage.noteBody') }}</div>
          </div>
        </div>

        <div class="pane form">
          <div class="formTitle keep-rtl">{{ t('contactPage.form.title') }}</div>
          <div class="formSub keep-rtl">{{ t('contactPage.form.sub') }}</div>

          <form @submit.prevent>
            <label class="fLabel keep-rtl">{{ t('contactPage.form.name') }}</label>
            <AppInput class="mb" :placeholder="t('contactPage.form.namePh')" />

            <label class="fLabel keep-rtl">{{ t('contactPage.form.message') }}</label>
            <textarea class="ta" :placeholder="t('contactPage.form.messagePh')" rows="5" />

            <div class="actions">
              <AppButton variant="primary" type="button">{{ t('contactPage.form.send') }}</AppButton>
              <NuxtLink to="/products">
                <AppButton variant="ghost" type="button">{{ t('browseProducts') }}</AppButton>
              </NuxtLink>
            </div>

            <div class="hint keep-rtl">{{ t('contactPage.form.hint') }}</div>
          </form>
        </div>
      </div>
    </section>
  </main>
</template>

<style scoped>
.page{ padding: 24px 0 50px; }
.shell{ padding: 22px; border-radius: 28px; }
.head{ padding: 10px 6px 18px; }
.kicker{ display:inline-flex; gap:8px; align-items:center; padding: 8px 12px; border-radius: 999px; border: 1px solid rgb(var(--border)); background: rgb(var(--panel)); font-weight: 700; }
.title{ margin: 12px 0 6px; font-size: clamp(28px, 5vw, 44px); line-height: 1.05; }
.sub{ margin: 0; opacity: .8; max-width: 70ch; }
.grid{ display:grid; grid-template-columns: 1.15fr .85fr; gap: 16px; }
.pane{ border: 1px solid rgb(var(--border)); background: rgb(var(--panel)); border-radius: 22px; padding: 16px; box-shadow: var(--shadow1); }
.row{ display:flex; align-items:center; gap: 12px; padding: 12px; border-radius: 18px; border: 1px solid rgb(var(--border)); background: rgb(var(--bg)); margin-bottom: 12px; }
.ico{ width: 44px; height: 44px; display:grid; place-items:center; border-radius: 14px; background: rgba(var(--primary), 0.12); }
.body{ flex:1; min-width: 0; }
.label{ font-weight: 800; font-size: 12px; opacity: .75; }
.value{ font-weight: 800; text-decoration: none; display:inline-block; margin-top: 3px; }
.value:hover{ text-decoration: underline; }
.btn{ text-decoration:none; border:1px solid rgb(var(--border)); padding: 10px 12px; border-radius: 14px; background: rgba(var(--text),0.04); font-weight: 800; }
.btn:hover{ background: rgba(var(--text),0.06); }
.note{ margin-top: 14px; padding: 14px; border-radius: 18px; border: 1px dashed rgba(var(--border), 0.8); }
.noteTitle{ font-weight: 900; margin-bottom: 6px; }
.noteBody{ opacity: .8; }
.formTitle{ font-weight: 900; font-size: 18px; }
.formSub{ opacity: .8; margin: 4px 0 12px; }
.fLabel{ display:block; font-weight: 900; font-size: 13px; margin: 10px 0 6px; }
.mb{ margin-bottom: 8px; }
.ta{ width: 100%; border: 1px solid rgb(var(--border)); background: rgb(var(--bg)); border-radius: 16px; padding: 12px; outline: none; resize: vertical; }
.ta:focus{ box-shadow: 0 0 0 4px rgba(var(--primary), 0.16); }
.actions{ display:flex; gap: 10px; flex-wrap: wrap; margin-top: 12px; }
.hint{ margin-top: 10px; opacity: .75; font-size: 12px; }

@media (max-width: 900px){
  .grid{ grid-template-columns: 1fr; }
}
</style>
