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
          <label class="text-sm font-bold rtl-text">هل أتيت عن طريق رابط مشاركة؟</label>
          <UiInput v-model="referralCode" label="كود المشاركة (اختياري)" class="keep-ltr" />
          <p class="text-xs text-muted rtl-text">إذا دخلت من رابط مشاركة فالكود يمتلئ تلقائياً. وإذا وصلك الكود فقط اكتبه هنا يدوياً، والحقل اختياري.</p>
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

  <Teleport to="body">
    <div v-if="welcomeOffer" class="welcome-offer-layer" role="dialog" aria-modal="true" aria-labelledby="welcome-offer-title" dir="rtl">
      <div class="welcome-offer-card">
        <button type="button" class="welcome-offer-close" aria-label="إغلاق" @click="closeWelcomeOffer">×</button>
        <div v-if="welcomeOffer.imageUrl" class="welcome-offer-media">
          <video v-if="isVideoUrl(welcomeOffer.imageUrl)" :src="assetUrl(welcomeOffer.imageUrl)" controls playsinline />
          <img v-else :src="assetUrl(welcomeOffer.imageUrl)" alt="إعلان ترحيب" />
        </div>
        <div class="welcome-offer-body">
          <span class="welcome-offer-badge">هدية ترحيبية</span>
          <h2 id="welcome-offer-title">{{ welcomeOffer.title || 'مبروك!' }}</h2>
          <p>{{ welcomeOffer.subtitle || 'مبروك حصلت على خصم و10 نقاط، استمتع بالتسوق داخل التطبيق.' }}</p>
          <div class="welcome-offer-rewards">
            <span v-if="Number(welcomeOffer.points || 0) > 0">+{{ welcomeOffer.points }} نقطة</span>
            <span v-if="welcomeOffer.couponCode">كود الخصم: <b class="keep-ltr">{{ welcomeOffer.couponCode }}</b></span>
          </div>
          <button type="button" class="welcome-offer-action" @click="closeWelcomeOffer">ابدأ التسوق</button>
        </div>
      </div>
    </div>
  </Teleport>
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
const api = useApi()
const welcomeOffer = ref<any | null>(null)

function assetUrl(url: string) {
  return api.buildAssetUrl(url || '')
}

function isVideoUrl(url: string) {
  return /\.(mp4|webm|ogg)(\?|#|$)/i.test(url || '')
}

function closeWelcomeOffer() {
  welcomeOffer.value = null
  router.push('/account')
}

async function submit() {
  loading.value = true
  error.value = ''
  try {
    const res: any = await auth.register({
      fullName: fullName.value,
      phone: phone.value,
      email: email.value,
      password: password.value,
      referralCode: referralCode.value,
    })
    if (res?.welcomeOffer) {
      welcomeOffer.value = res.welcomeOffer
    } else {
      router.push('/account')
    }
  } catch (e: any) {
    error.value = e?.data?.message || e?.message || 'فشل إنشاء الحساب'
  } finally {
    loading.value = false
  }
}
</script>


<style scoped>
.welcome-offer-layer {
  position: fixed;
  inset: 0;
  z-index: 9999;
  display: grid;
  place-items: center;
  padding: 1rem;
  background: rgba(2, 6, 15, .78);
  backdrop-filter: blur(12px);
}
.welcome-offer-card {
  width: min(94vw, 520px);
  overflow: hidden;
  position: relative;
  border-radius: 30px;
  border: 1px solid rgba(255,255,255,.13);
  background: #0d111c;
  box-shadow: 0 30px 100px rgba(0,0,0,.55);
  color: white;
}
.welcome-offer-close {
  position: absolute;
  top: .8rem;
  left: .8rem;
  width: 40px;
  height: 40px;
  border-radius: 999px;
  border: 1px solid rgba(255,255,255,.16);
  background: rgba(15,23,42,.9);
  color: white;
  font-size: 1.3rem;
  z-index: 2;
}
.welcome-offer-media {
  background: #111827;
  max-height: 290px;
}
.welcome-offer-media img,
.welcome-offer-media video {
  width: 100%;
  max-height: 290px;
  object-fit: cover;
  display: block;
}
.welcome-offer-body {
  padding: 1.4rem;
  text-align: center;
}
.welcome-offer-badge {
  display: inline-flex;
  padding: .35rem .75rem;
  border-radius: 999px;
  color: #f9a8d4;
  background: rgba(236,72,153,.14);
  font-size: .8rem;
  font-weight: 900;
}
.welcome-offer-body h2 {
  margin: .75rem 0 .4rem;
  font-size: clamp(1.7rem, 4vw, 2.4rem);
  font-weight: 1000;
}
.welcome-offer-body p {
  margin: 0 auto;
  max-width: 32rem;
  color: rgba(255,255,255,.78);
  line-height: 1.9;
}
.welcome-offer-rewards {
  margin: 1rem 0;
  display: flex;
  flex-wrap: wrap;
  gap: .55rem;
  justify-content: center;
}
.welcome-offer-rewards span {
  border-radius: 999px;
  background: rgba(139,92,246,.16);
  color: #ddd6fe;
  border: 1px solid rgba(139,92,246,.28);
  padding: .55rem .8rem;
  font-weight: 900;
}
.welcome-offer-action {
  width: 100%;
  min-height: 48px;
  border: 0;
  border-radius: 18px;
  background: linear-gradient(135deg, #a78bfa, #ec4899);
  color: white;
  font-weight: 1000;
}
</style>
