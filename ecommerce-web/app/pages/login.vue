<template>
  <div class="grid grid-cols-1 gap-6 lg:grid-cols-2 items-start">
    <!-- Left: why register -->
    <section
      class="rounded-3xl border p-6"
      :style="{
        borderColor: 'rgb(var(--border))',
        background: 'rgba(255,255,255,.04)',
      }"
    >
      <h2 class="text-2xl font-extrabold">{{ t('whyRegister') }}</h2>

      <ul class="mt-6 space-y-3">
        <li
          class="rounded-2xl border px-4 py-3 flex items-center justify-between"
          :style="{
            borderColor: 'rgb(var(--border))',
            background: 'rgba(0,0,0,.12)',
          }"
        >
          <span class="opacity-80">{{ t('secureCheckout') }}</span>
          <span style="color: rgb(var(--success))">✓</span>
        </li>

        <li
          class="rounded-2xl border px-4 py-3 flex items-center justify-between"
          :style="{
            borderColor: 'rgb(var(--border))',
            background: 'rgba(0,0,0,.12)',
          }"
        >
          <span class="opacity-80">{{ t('downloadAfterPayment') }}</span>
          <span style="color: rgb(var(--success))">✓</span>
        </li>

        <li
          class="rounded-2xl border px-4 py-3 flex items-center justify-between"
          :style="{
            borderColor: 'rgb(var(--border))',
            background: 'rgba(0,0,0,.12)',
          }"
        >
          <span class="opacity-80">{{ t('historySaved') }}</span>
          <span style="color: rgb(var(--success))">✓</span>
        </li>
      </ul>
    </section>

    <!-- Right: auth card -->
    <section
      class="rounded-3xl border p-6"
      :style="{
        borderColor: 'rgb(var(--border))',
        background: 'rgba(255,255,255,.04)',
      }"
    >
      <div class="flex items-start justify-between gap-3">
        <div>
          <h1 class="text-2xl font-extrabold">👋 {{ t('login') }}</h1>
          <p class="mt-1 opacity-70">
            {{ mode === 'login' ? t('loginToContinue') : t('registerToContinue') }}
          </p>
        </div>

        <div class="flex items-center gap-2">
          <button
            class="px-3 py-2 rounded-lg hover:opacity-90 border"
            :style="{ borderColor: 'rgb(var(--border))' }"
            type="button"
            @click="switchLocale('ar')"
          >
            AR
          </button>

          <button
            class="px-3 py-2 rounded-lg hover:opacity-90 border"
            :style="{ borderColor: 'rgb(var(--border))' }"
            type="button"
            @click="switchLocale('en')"
          >
            EN
          </button>
        </div>
      </div>

      <!-- tabs -->
      <div
        class="mt-6 rounded-2xl border p-1 flex"
        :style="{
          borderColor: 'rgb(var(--border))',
          background: 'rgba(0,0,0,.12)',
        }"
      >
        <button
          class="flex-1 rounded-xl py-2 text-sm font-semibold transition"
          :style="mode === 'login'
            ? { background: 'rgba(255,255,255,.10)' }
            : { background: 'transparent' }"
          @click="mode = 'login'"
          type="button"
        >
          {{ t('login') }}
        </button>

        <button
          class="flex-1 rounded-xl py-2 text-sm font-semibold transition"
          :style="mode === 'register'
            ? { background: 'rgba(255,255,255,.10)' }
            : { background: 'transparent' }"
          @click="mode = 'register'"
          type="button"
        >
          {{ t('createAccount') }}
        </button>
      </div>

      <!-- form -->
      <form class="mt-6 space-y-4" @submit.prevent="onSubmit">
        <AppInput
          v-if="mode === 'register'"
          v-model="form.fullName"
          :label="t('fullName')"
          autocomplete="name"
          :error="fieldError === 'fullName' ? error : ''"
        />

        <AppInput
          v-model="form.email"
          type="email"
          :label="t('email')"
          autocomplete="email"
          :error="fieldError === 'email' ? error : ''"
        />

        <AppInput
          v-model="form.password"
          type="password"
          :label="t('password')"
          autocomplete="current-password"
          :error="fieldError === 'password' ? error : ''"
        />

        <!-- general error -->
        <div
          v-if="generalError"
          class="rounded-2xl border px-4 py-3 text-sm"
          :style="{
            borderColor: 'rgba(239,68,68,.35)',
            background: 'rgba(239,68,68,.10)',
            color: 'rgb(var(--text))',
          }"
        >
          {{ generalError }}
        </div>

        <div class="flex items-center gap-3">
          <AppButton type="submit" :loading="pending">
            {{ mode === 'login' ? t('login') : t('createAccount') }}
          </AppButton>

          <span v-if="success" class="text-sm" style="color: rgb(var(--success))">
            {{ success }}
          </span>
        </div>
      </form>
    </section>
  </div>
</template>

<script setup lang="ts">
import { reactive, ref } from 'vue'
import { navigateTo } from '#app'
import { useI18n } from '~/composables/useI18n'
import { useSiteMeta } from '~/composables/useSiteMeta'
import { useAuthStore } from '~/stores/auth'
import { useUiStore } from '~/stores/ui'

const { t, setLocale } = useI18n()
const ui = useUiStore()
const auth = useAuthStore()

useSiteMeta({
  title: `Login | Ecommerce`,
  description: 'Login or create an account to access digital products and downloads.',
  path: '/login',
})

const mode = ref<'login' | 'register'>('login')
const pending = ref(false)

// errors
const error = ref('')
const generalError = ref('')
const fieldError = ref<'' | 'fullName' | 'email' | 'password'>('')

// success
const success = ref('')

const form = reactive({
  fullName: '',
  email: '',
  password: '',
})

function resetMessages() {
  error.value = ''
  generalError.value = ''
  fieldError.value = ''
  success.value = ''
}

function validate() {
  // reset per validate
  error.value = ''
  generalError.value = ''
  fieldError.value = ''

  if (mode.value === 'register' && !form.fullName.trim()) {
    fieldError.value = 'fullName'
    error.value = t('required')
    return false
  }

  if (!form.email.trim()) {
    fieldError.value = 'email'
    error.value = t('required')
    return false
  }

  if (!form.email.includes('@')) {
    fieldError.value = 'email'
    error.value = t('invalidEmail')
    return false
  }

  if (!form.password) {
    fieldError.value = 'password'
    error.value = t('required')
    return false
  }

  return true
}

function switchLocale(l: 'ar' | 'en') {
  // حسب طلبك: لا نغير الاتجاه أبداً
  setLocale(l)
  ui.applyLocaleToHtml(l) // لازم هذه ما تغيّر dir (يظل ltr)
}

async function onSubmit() {
  if (!validate()) return

  pending.value = true
  success.value = ''
  generalError.value = ''

  try {
    if (mode.value === 'login') {
      await auth.login({ email: form.email, password: form.password })
      success.value = t('loggedIn')
      await navigateTo('/products')
    } else {
      await auth.register({
        fullName: form.fullName,
        email: form.email,
        password: form.password,
      })
      success.value = t('accountCreated')
      mode.value = 'login'
    }
  } catch (e: any) {
    // هذا خطأ عام من API
    generalError.value = e?.data?.message || e?.message || t('requestFailed')
  } finally {
    pending.value = false
  }
}
</script>
