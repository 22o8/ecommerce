<template>
  <div class="grid gap-6 lg:grid-cols-2">
    <div class="card-soft p-6 md:p-8">
      <div class="flex items-center justify-between gap-3">
        <div>
          <div class="text-sm text-muted rtl-text">{{ t('contactPage.kicker') }}</div>
          <h1 class="text-2xl md:text-3xl font-black rtl-text">{{ t('contactPage.title') }}</h1>
          <p class="text-muted rtl-text mt-1">{{ t('contactPage.sub') }}</p>
        </div>
        <Icon name="mdi:message-processing-outline" class="text-5xl opacity-70 animate-floaty" />
      </div>

      <div class="mt-6 grid gap-3">
        <a class="rounded-3xl border border-app bg-surface p-4 hover:bg-surface-2 transition keep-ltr" :href="`tel:${supportPhone}`">
          <Icon name="mdi:phone-outline" class="inline-block text-xl align-middle" />
          <span class="ml-2">{{ supportPhone }}</span>
        </a>
        <a class="rounded-3xl border border-app bg-surface p-4 hover:bg-surface-2 transition keep-ltr" :href="`mailto:${supportEmail}`">
          <Icon name="mdi:email-outline" class="inline-block text-xl align-middle" />
          <span class="ml-2">{{ supportEmail }}</span>
        </a>
        <a class="rounded-3xl border border-app bg-surface p-4 hover:bg-surface-2 transition keep-ltr" :href="instagramUrl" target="_blank" rel="noreferrer">
          <Icon name="mdi:instagram" class="inline-block text-xl align-middle" />
          <span class="ml-2">Instagram</span>
        </a>
      </div>

      <div class="mt-6 rounded-3xl border border-app bg-surface-2 p-4">
        <div class="font-bold rtl-text">{{ t('contactPage.noteTitle') }}</div>
        <div class="text-sm text-muted rtl-text mt-1">{{ t('contactPage.noteBody') }}</div>
      </div>
    </div>

    <div class="card-soft p-6 md:p-8">
      <h2 class="text-xl font-extrabold rtl-text">{{ t('contactPage.form.title') }}</h2>
      <p class="text-muted rtl-text">{{ t('contactPage.form.sub') }}</p>

      <form class="mt-6 grid gap-4" @submit.prevent="fakeSend">
        <UiInput v-model="name" :label="t('contactPage.form.name')" :placeholder="t('contactPage.form.namePh')" />
        <label class="block">
          <span class="mb-2 block text-sm font-semibold rtl-text">{{ t('contactPage.form.message') }}</span>
          <textarea v-model="message" rows="6" class="w-full rounded-2xl border border-app bg-surface px-4 py-3 text-sm outline-none" :placeholder="t('contactPage.form.messagePh')" />
          <p class="mt-2 text-xs text-muted rtl-text">{{ t('contactPage.form.hint') }}</p>
        </label>

        <UiButton type="submit">
          <Icon name="mdi:send-outline" class="text-lg" />
          <span class="rtl-text">{{ t('contactPage.form.send') }}</span>
        </UiButton>

        <p v-if="sent" class="text-sm rtl-text text-[rgb(var(--success))]">{{ t('sentOk') }}</p>
      </form>
    </div>
  </div>
</template>

<script setup lang="ts">
import UiButton from '~/components/ui/UiButton.vue'
import UiInput from '~/components/ui/UiInput.vue'
const { t } = useI18n()
const config = useRuntimeConfig()
const supportEmail = String((config.public as any).supportEmail || '')
const supportPhone = String((config.public as any).supportPhone || '')
const instagramUrl = String((config.public as any).instagramUrl || '')

const name = ref('')
const message = ref('')
const sent = ref(false)
function fakeSend(){
  sent.value = true
  setTimeout(()=>{ sent.value = false }, 2500)
}
</script>
