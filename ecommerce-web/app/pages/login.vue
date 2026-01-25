<template>
  <div class="grid gap-6 lg:grid-cols-2 lg:items-center">
    <div class="card-soft p-6 md:p-8">
      <div class="flex items-center gap-3">
        <div class="h-11 w-11 rounded-2xl bg-[rgb(var(--primary))] text-black dark:text-[rgb(var(--bg))] grid place-items-center">
          <Icon name="mdi:account-lock-outline" class="text-2xl animate-floaty" />
        </div>
        <div>
          <h1 class="text-2xl font-black rtl-text">{{ t('loginTitle') }}</h1>
          <p class="text-sm text-muted rtl-text">{{ t('loginSubtitle') }}</p>
        </div>
      </div>

      <form class="mt-6 grid gap-4" @submit.prevent="submit">
        <UiInput v-model="email" type="email" autocomplete="email" :label="t('email')" class="keep-ltr" />
        <UiInput v-model="password" type="password" autocomplete="current-password" :label="t('password')" class="keep-ltr" />

        <UiButton :loading="loading" type="submit">
          <Icon name="mdi:login-variant" class="text-lg" />
          <span class="rtl-text">{{ t('login') }}</span>
        </UiButton>

        <p v-if="error" class="text-sm rtl-text text-[rgb(var(--danger))]">{{ error }}</p>
      </form>
    </div>

    <div class="card-soft p-6 md:p-8">
      <h2 class="text-xl font-extrabold rtl-text">{{ t('benefitsTitle') }}</h2>
      <div class="mt-4 grid gap-3">
        <div class="rounded-3xl border border-app bg-surface p-4">
          <div class="flex items-center gap-2">
            <Icon name="mdi:receipt-text-outline" class="text-xl" />
            <div class="font-bold rtl-text">{{ t('benefit.orders') }}</div>
          </div>
          <div class="text-sm text-muted rtl-text mt-1">{{ t('benefit.orders.desc') }}</div>
        </div>
        <div class="rounded-3xl border border-app bg-surface p-4">
          <div class="flex items-center gap-2">
            <Icon name="mdi:shield-check-outline" class="text-xl" />
            <div class="font-bold rtl-text">{{ t('benefit.security') }}</div>
          </div>
          <div class="text-sm text-muted rtl-text mt-1">{{ t('benefit.security.desc') }}</div>
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

const email = ref('')
const password = ref('')
const loading = ref(false)
const error = ref('')

async function submit(){
  loading.value = true
  error.value = ''
  try{
    await auth.login({ email: email.value, password: password.value })
    router.push('/')
  }catch(e:any){
    error.value = e?.data?.message || e?.message || t('loginFailed')
  }finally{
    loading.value = false
  }
}
</script>
