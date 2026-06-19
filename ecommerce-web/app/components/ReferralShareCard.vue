<template>
  <div class="rounded-[2rem] border border-app bg-surface/90 p-5 shadow-xl">
    <div class="flex items-start gap-3">
      <div class="grid h-12 w-12 shrink-0 place-items-center rounded-2xl bg-[rgb(var(--primary))]/20 text-[rgb(var(--primary))]">
        <Icon name="mdi:share-variant-outline" class="text-2xl" />
      </div>
      <div class="min-w-0 flex-1">
        <h3 class="text-lg font-black rtl-text">شارك لصديقك واربح نقاط</h3>
        <p class="mt-1 text-sm text-muted rtl-text">انسخ رابطك أو كودك وأرسله لصديقك. عند تسجيله بنجاح يظهر عند الإدارة كمشاركة ناجحة.</p>
      </div>
    </div>

    <div v-if="!auth.isAuthed" class="mt-4 rounded-2xl border border-app bg-app/50 p-4 text-sm rtl-text text-muted">
      سجل دخولك أو أنشئ حساب حتى يظهر رابط المشاركة الخاص بك.
      <NuxtLink to="/login" class="font-bold text-[rgb(var(--primary))]">تسجيل الدخول</NuxtLink>
    </div>

    <div v-else class="mt-4 grid gap-3">
      <label class="grid gap-2">
        <span class="text-sm font-bold rtl-text">رابط المشاركة</span>
        <div class="grid gap-2 md:grid-cols-[1fr_auto]">
          <input class="input keep-ltr" readonly :value="wallet?.referralUrl || 'جاري تجهيز الرابط...'" aria-label="رابط المشاركة" />
          <UiButton type="button" :disabled="loading || !wallet?.referralUrl" @click="copy(wallet?.referralUrl || '')">
            <Icon name="mdi:content-copy" />
            <span class="rtl-text">نسخ الرابط</span>
          </UiButton>
        </div>
      </label>

      <label class="grid gap-2">
        <span class="text-sm font-bold rtl-text">كود المشاركة</span>
        <div class="grid gap-2 md:grid-cols-[1fr_auto]">
          <input class="input keep-ltr" readonly :value="wallet?.referralCode || 'جاري تجهيز الكود...'" aria-label="كود المشاركة" />
          <UiButton type="button" variant="secondary" :disabled="loading || !wallet?.referralCode" @click="copy(wallet?.referralCode || '')">
            <Icon name="mdi:ticket-confirmation-outline" />
            <span class="rtl-text">نسخ الكود</span>
          </UiButton>
        </div>
      </label>

      <div class="rounded-2xl border border-app bg-app/50 p-4 text-center">
        <div class="text-xs text-muted rtl-text">رصيد نقاطك</div>
        <div class="mt-1 text-3xl font-black keep-ltr">{{ wallet?.balance ?? 0 }}</div>
      </div>

      <p v-if="copied" class="text-sm text-emerald-400 rtl-text">تم النسخ بنجاح.</p>
    </div>
  </div>
</template>

<script setup lang="ts">
import UiButton from '~/components/ui/UiButton.vue'
const api = useApi()
const auth = useAuthStore()
const wallet = ref<any>(null)
const loading = ref(false)
const copied = ref(false)

async function load() {
  if (!auth.isAuthed || wallet.value || loading.value) return
  loading.value = true
  try { wallet.value = await api.get('/wallet/me') }
  catch { wallet.value = null }
  finally { loading.value = false }
}

async function copy(value: string) {
  if (!value) return
  try { await navigator.clipboard?.writeText(value) } catch {}
  copied.value = true
  setTimeout(() => copied.value = false, 1600)
}

onMounted(load)
</script>
