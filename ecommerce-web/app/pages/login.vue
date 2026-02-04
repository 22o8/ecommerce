<template>
  <div class="container mx-auto px-4 py-10">
    <div class="card-soft p-6 md:p-8 max-w-xl mx-auto">
      <div class="flex items-center gap-3">
        <div class="h-11 w-11 rounded-2xl bg-[rgb(var(--primary))] text-black dark:text-[rgb(var(--bg))] grid place-items-center">
          <Icon name="mdi:account-lock-outline" class="text-2xl animate-floaty" />
        </div>
        <div>
          <h1 class="text-2xl font-black rtl-text">{{ t('login.title') }}</h1>
          <p class="text-sm text-muted rtl-text">{{ t('login.subtitle') }}</p>
        </div>
      </div>

      <form class="mt-6 grid gap-4" @submit.prevent="submit">
        <UiInput v-model="email" type="email" autocomplete="email" :label="t('auth.email')" class="keep-ltr" />
        <UiInput v-model="password" type="password" autocomplete="current-password" :label="t('auth.password')" class="keep-ltr" />

        <UiButton :loading="loading" type="submit">
          <Icon name="mdi:login-variant" class="text-lg" />
          <span class="rtl-text">{{ t('nav.login') }}</span>
        </UiButton>

        <p class="text-sm text-muted rtl-text">
          {{ t('auth.noAccount') }}
          <NuxtLink to="/register" class="font-bold text-[rgb(var(--primary))]">
            {{ t('register.title') }}
          </NuxtLink>
        </p>

        <p v-if="error" class="text-sm rtl-text text-[rgb(var(--danger))]">{{ error }}</p>
      </form>
    </div>
  </div>
</template>

<script setup lang="ts">
import UiButton from '~/components/ui/UiButton.vue'
import UiInput from '~/components/ui/UiInput.vue'
const { t } = useI18n()
const auth = useAuthStore()
const router = useRouter()
const route = useRoute()

const email = ref('')
const password = ref('')
const loading = ref(false)
const error = ref('')

async function submit(){
  loading.value = true
  error.value = ''
  try{
    await auth.login({ email: email.value, password: password.value })
    const queryRedirect = typeof route.query.redirect === 'string' ? route.query.redirect : null
    const fallback = auth.isAdmin ? '/admin/' : '/'
    router.push(queryRedirect || fallback)
  }catch(e:any){
    error.value = e?.data?.message || e?.message || t('loginFailed')
  }finally{
    loading.value = false
  }
}
</script>
