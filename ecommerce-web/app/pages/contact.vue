<!-- app/pages/contact.vue -->
<template>
  <div class="mx-auto max-w-3xl space-y-6">
    <div class="rounded-3xl border border-white/10 bg-white/5 p-8">
      <h1 class="text-2xl font-black">{{ t('contactTitle') }}</h1>
      <p class="mt-2 text-sm text-white/70">{{ t('contactSub') }}</p>

      <form class="mt-6 space-y-4" @submit.prevent="send">
        <AppInput v-model="email" :label="t('email')" type="email" autocomplete="email" />
        <div>
          <label class="text-sm text-white/70">{{ t('message') }}</label>
          <textarea
            v-model="message"
            class="mt-1 w-full rounded-2xl border border-white/10 bg-black/20 px-4 py-3 outline-none focus:border-white/20 min-h-[140px]"
          ></textarea>
        </div>

        <div v-if="info" class="rounded-2xl border border-white/10 bg-black/20 px-4 py-3 text-sm text-white/70">
          {{ info }}
        </div>

        <AppButton type="submit" :loading="pending">{{ t('send') }}</AppButton>
      </form>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { useI18n } from '~/composables/useI18n'
import { useSiteMeta } from '~/composables/useSiteMeta'

const { t } = useI18n()
useSiteMeta({ title: `${t('contact')} | Ecommerce`, description: 'Contact us', path: '/contact' })

const email = ref('')
const message = ref('')
const pending = ref(false)
const info = ref('')

async function send() {
  pending.value = true
  info.value = ''
  try {
    // هنا تقدر تربطه بباك اندك لاحقاً
    await new Promise((r) => setTimeout(r, 500))
    info.value = 'تم إرسال الرسالة (Dummy). اربطها بباك اندك لاحقاً.'
    email.value = ''
    message.value = ''
  } finally {
    pending.value = false
  }
}
</script>
