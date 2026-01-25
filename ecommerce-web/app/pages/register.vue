<script setup lang="ts">
import UiButton from '~/components/ui/UiButton.vue'
import UiCard from '~/components/ui/UiCard.vue'
import UiInput from '~/components/ui/UiInput.vue'
import { useAuthStore } from '~/app/stores/auth'
import { useProfileStore } from '~/app/stores/profile'
import { useI18n } from '~/app/composables/useI18n'

const { t } = useI18n()
const auth = useAuthStore()
const profile = useProfileStore()
const router = useRouter()

const fullName = ref('')
const email = ref('')
const phone = ref('')
const password = ref('')
const confirmPassword = ref('')
const submitting = ref(false)
const error = ref('')

const submit = async () => {
  error.value = ''
  if (!fullName.value || !email.value || !phone.value || !password.value) {
    error.value = t('required')
    return
  }
  if (password.value !== confirmPassword.value) {
    error.value = t('passwordMismatch')
    return
  }

  submitting.value = true
  try {
    await auth.register({ fullName: fullName.value, email: email.value, password: password.value })
    // نحفظ رقم الهاتف محليًا (بدون تعديل قاعدة البيانات)
    profile.setPhone(phone.value)

    // تسجيل دخول تلقائي
    await auth.login({ email: email.value, password: password.value })
    await router.push('/account')
  } catch (e: any) {
    error.value = e?.message || t('registerFailed')
  } finally {
    submitting.value = false
  }
}
</script>

<template>
  <div class="mx-auto max-w-md px-4 py-10">
    <UiCard>
      <div class="p-6">
        <h1 class="text-2xl font-semibold">{{ t('registerTitle') }}</h1>
        <p class="mt-1 text-sm text-white/70">{{ t('registerSubtitle') }}</p>

        <div class="mt-6 space-y-3">
          <UiInput v-model="fullName" :label="t('fullName')" />
          <UiInput v-model="email" type="email" :label="t('email')" />
          <UiInput v-model="phone" inputmode="tel" :label="t('phone')" />
          <UiInput v-model="password" type="password" :label="t('password')" />
          <UiInput v-model="confirmPassword" type="password" :label="t('confirmPassword')" />

          <p class="text-xs text-amber-200/90">
            {{ t('registerWarning') }}
          </p>

          <p v-if="error" class="text-sm text-red-300">{{ error }}</p>

          <UiButton class="w-full" :disabled="submitting" @click="submit">
            {{ submitting ? t('loading') : t('createAccount') }}
          </UiButton>

          <NuxtLink to="/login" class="block text-center text-sm text-white/70 hover:text-white">
            {{ t('alreadyHaveAccount') }}
          </NuxtLink>
        </div>
      </div>
    </UiCard>
  </div>
</template>
