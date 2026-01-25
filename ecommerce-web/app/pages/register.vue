<template>
  <div class="grid gap-6 lg:grid-cols-2 lg:items-center">
    <div class="card-soft p-6 md:p-8">
      <div class="flex items-center gap-3">
        <div class="h-11 w-11 rounded-2xl bg-[rgb(var(--primary))] text-black dark:text-[rgb(var(--bg))] grid place-items-center">
          <Icon name="mdi:account-plus-outline" class="text-2xl animate-floaty" />
        </div>
        <div>
          <h1 class="text-2xl font-black rtl-text">{{ t('register.title') }}</h1>
          <p class="text-sm text-muted rtl-text">{{ t('register.subtitle') }}</p>
        </div>
      </div>

      <form class="mt-6 grid gap-4" @submit.prevent="submit">
        <UiInput v-model="fullName" autocomplete="name" :label="t('auth.fullName')" class="rtl-text" />
        <UiInput v-model="phone" autocomplete="tel" :label="t('auth.phone')" class="keep-ltr" />
        <UiInput v-model="email" type="email" autocomplete="email" :label="t('auth.email')" class="keep-ltr" />
        <UiInput v-model="password" type="password" autocomplete="new-password" :label="t('auth.password')" class="keep-ltr" />

        <UiButton :loading="loading" type="submit">
          <Icon name="mdi:account-check-outline" class="text-lg" />
          <span class="rtl-text">{{ t('createAccount') }}</span>
        </UiButton>

        <p class="text-xs rtl-text text-muted">
          <Icon name="mdi:alert-outline" class="inline-block align-[-2px]" />
          {{ t('accuracyWarning') }}
        </p>

        <p v-if="error" class="text-sm rtl-text text-[rgb(var(--danger))]">{{ error }}</p>
      </form>
    </div>

    <div class="card-soft p-6 md:p-8">
      <h2 class="text-xl font-extrabold rtl-text">{{ t('benefitsTitle') }}</h2>
      <div class="mt-4 grid gap-3">
        <div class="rounded-3xl border border-app bg-surface p-4">
          <div class="flex items-center gap-2">
            <Icon name="mdi:cart-outline" class="text-xl" />
            <div class="font-bold rtl-text">{{ t('cartTitle') }}</div>
          </div>
          <div class="text-sm text-muted rtl-text mt-1">{{ t('cartHint') }}</div>
        </div>
        <div class="rounded-3xl border border-app bg-surface p-4">
          <div class="flex items-center gap-2">
            <Icon name="mdi:receipt-text-outline" class="text-xl" />
            <div class="font-bold rtl-text">{{ t('benefit.orders') }}</div>
          </div>
          <div class="text-sm text-muted rtl-text mt-1">{{ t('benefit.orders.desc') }}</div>
        </div>
        <div class="rounded-3xl border border-app bg-surface p-4">
          <div class="flex items-center gap-2">
            <Icon name="mdi:whatsapp" class="text-xl" />
            <div class="font-bold rtl-text">{{ t('benefit.whatsapp') }}</div>
          </div>
          <div class="text-sm text-muted rtl-text mt-1">{{ t('benefit.whatsapp.desc') }}</div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import UiButton from '~/components/ui/UiButton.vue'
import UiInput from '~/components/ui/UiInput.vue'

const { t } = useI18n()
const auth = useAuthStore()
const router = useRouter()

const fullName = ref('')
const phone = ref('')
const email = ref('')
const password = ref('')
const loading = ref(false)
const error = ref('')

async function submit() {
  loading.value = true
  error.value = ''
  try {
    await auth.register({
      fullName: fullName.value,
      phone: phone.value,
      email: email.value,
      password: password.value,
    })
    router.push('/cart')
  } catch (e: any) {
    error.value = e?.data?.message || e?.message || t('registerFailed')
  } finally {
    loading.value = false
  }
}
</script>
