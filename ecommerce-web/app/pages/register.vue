<template>
  <div class="grid gap-6 lg:grid-cols-2 lg:items-center">
    <div class="card-soft p-6 md:p-8">
      <div class="flex items-center gap-3">
        <div class="h-11 w-11 rounded-2xl bg-[rgb(var(--primary))] text-black dark:text-[rgb(var(--bg))] grid place-items-center">
          <Icon name="mdi:account-plus-outline" class="text-2xl animate-floaty" />
        </div>
        <div>
          <h1 class="text-2xl font-black rtl-text">إنشاء حساب</h1>
          <p class="text-sm text-muted rtl-text">يمكن إنشاء الحساب بالإيميل أو رقم الهاتف، وبعد التسجيل تظهر لك محفظة النقاط الخاصة بك.</p>
        </div>
      </div>

      <form class="mt-6 grid gap-4" @submit.prevent="submit">
        <UiInput v-model="fullName" autocomplete="name" label="الاسم الكامل" class="rtl-text" />
        <UiInput v-model="phone" autocomplete="tel" label="رقم الهاتف" class="keep-ltr" />
        <UiInput v-model="email" type="email" autocomplete="email" label="الإيميل (اختياري إذا تستخدم الهاتف)" class="keep-ltr" />
        <div class="grid gap-1">
          <label class="text-sm font-bold rtl-text">هل أتيت عن طريق رابط مشاركة؟ ضع كود المشاركة الذي وصلك</label>
          <UiInput v-model="referralCode" label="كود المشاركة (اختياري)" class="keep-ltr" />
          <p class="text-xs text-muted rtl-text">إذا ما عندك كود اترك الحقل فارغ.</p>
        </div>
        <UiInput v-model="password" type="password" autocomplete="new-password" label="كلمة المرور" class="keep-ltr" />

        <UiButton :loading="loading" type="submit">
          <Icon name="mdi:account-check-outline" class="text-lg" />
          <span class="rtl-text">إنشاء الحساب</span>
        </UiButton>

        <p class="text-xs rtl-text text-muted">يجب إدخال إيميل أو رقم هاتف واحد على الأقل.</p>
        <p v-if="error" class="text-sm rtl-text text-[rgb(var(--danger))]">{{ error }}</p>
      </form>
    </div>

    <div class="card-soft p-6 md:p-8">
      <h2 class="text-xl font-extrabold rtl-text">مميزات الحساب</h2>
      <div class="mt-4 grid gap-3">
        <div class="rounded-3xl border border-app bg-surface p-4 rtl-text">محفظة نقاط لكل عميل.</div>
        <div class="rounded-3xl border border-app bg-surface p-4 rtl-text">كود مشاركة خاص بك.</div>
        <div class="rounded-3xl border border-app bg-surface p-4 rtl-text">متابعة الطلبات والهدايا والكوبونات.</div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import UiButton from '~/components/ui/UiButton.vue'
import UiInput from '~/components/ui/UiInput.vue'

const auth = useAuthStore()
const router = useRouter()
const route = useRoute()

const fullName = ref('')
const phone = ref('')
const email = ref('')
const referralCode = ref(typeof route.query.ref === 'string' ? route.query.ref : '')
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
      referralCode: referralCode.value,
    })
    router.push('/account')
  } catch (e: any) {
    error.value = e?.data?.message || e?.message || 'فشل إنشاء الحساب'
  } finally {
    loading.value = false
  }
}
</script>
