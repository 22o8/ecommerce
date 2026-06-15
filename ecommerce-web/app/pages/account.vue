<template>
  <div class="grid gap-6">
    <div class="card-soft p-6 md:p-10">
      <div class="flex flex-col gap-3 md:flex-row md:items-center md:justify-between">
        <div>
          <h1 class="text-2xl md:text-3xl font-black rtl-text">حسابي ومحفظة النقاط</h1>
          <p class="text-muted rtl-text mt-1">تابع نقاطك، هداياك، وكود المشاركة الخاص بك.</p>
        </div>
        <UiButton variant="secondary" @click="load" :loading="loading">
          <Icon name="mdi:refresh" class="text-lg" />
          <span>تحديث</span>
        </UiButton>
      </div>

      <div v-if="!auth.isAuthed" class="mt-6 rounded-3xl border border-app bg-surface p-5 rtl-text">
        سجل دخولك حتى تشوف المحفظة والنقاط.
        <NuxtLink to="/login" class="font-bold text-[rgb(var(--primary))]">تسجيل الدخول</NuxtLink>
      </div>

      <div v-else class="mt-6 grid gap-4 md:grid-cols-3">
        <div class="rounded-3xl border border-app bg-surface p-5">
          <div class="text-sm text-muted rtl-text">رصيد النقاط</div>
          <div class="mt-2 text-4xl font-black keep-ltr">{{ wallet?.balance ?? 0 }}</div>
        </div>
        <div class="rounded-3xl border border-app bg-surface p-5">
          <div class="text-sm text-muted rtl-text">إجمالي المكتسب</div>
          <div class="mt-2 text-4xl font-black keep-ltr">{{ wallet?.lifetimeEarned ?? 0 }}</div>
        </div>
        <div class="rounded-3xl border border-app bg-surface p-5">
          <div class="text-sm text-muted rtl-text">عدد التسجيلات من رابطك</div>
          <div class="mt-2 text-4xl font-black keep-ltr">{{ wallet?.referralCount ?? 0 }}</div>
        </div>
      </div>
    </div>

    <ReferralShareCard v-if="auth.isAuthed" />

    <div v-if="auth.isAuthed" class="card-soft p-6 md:p-8">
      <h2 class="text-xl font-black rtl-text">استبدال النقاط</h2>
      <p class="mt-1 text-sm text-muted rtl-text">
        عند وصولك إلى عدد نقاط مناسب، تواصل مع الإدارة. الإدارة تسحب النقاط من محفظتك وتجهز لك كوبون تخفيض وترسله لك كهدية داخل الإشعارات والقسائم.
      </p>
      <div class="mt-4 grid gap-3 sm:grid-cols-3">
        <NuxtLink to="/notifications" class="rounded-3xl border border-app bg-surface p-4 rtl-text font-bold transition hover:bg-surface-2">الإشعارات والهدايا</NuxtLink>
        <NuxtLink to="/my-coupons" class="rounded-3xl border border-app bg-surface p-4 rtl-text font-bold transition hover:bg-surface-2">قسائمي</NuxtLink>
        <NuxtLink to="/contact" class="rounded-3xl border border-app bg-surface p-4 rtl-text font-bold transition hover:bg-surface-2">طلب استبدال النقاط</NuxtLink>
      </div>
    </div>

    <div v-if="auth.isAuthed" class="grid gap-6 lg:grid-cols-2">
      <div class="card-soft p-6 md:p-8">
        <h2 class="text-xl font-black rtl-text">حركة النقاط</h2>
        <div class="mt-4 grid gap-3">
          <div v-for="tx in wallet?.transactions || []" :key="tx.id" class="rounded-3xl border border-app bg-surface p-4 flex items-center justify-between gap-3">
            <div>
              <div class="font-bold rtl-text">{{ tx.note || tx.type }}</div>
              <div class="text-xs text-muted keep-ltr">{{ tx.createdAtUtc }}</div>
            </div>
            <div class="text-lg font-black keep-ltr" :class="tx.points >= 0 ? 'text-emerald-400' : 'text-red-400'">{{ tx.points }}</div>
          </div>
          <div v-if="!(wallet?.transactions || []).length" class="text-muted rtl-text">لا توجد حركات بعد.</div>
        </div>
      </div>

      <div class="card-soft p-6 md:p-8">
        <h2 class="text-xl font-black rtl-text">الهدايا والتنبيهات</h2>
        <div class="mt-4 grid gap-3">
          <div v-for="g in wallet?.gifts || []" :key="g.id" class="rounded-3xl border border-app bg-surface p-4">
            <div class="font-bold rtl-text">{{ g.title }}</div>
            <div class="text-sm text-muted rtl-text mt-1">{{ g.message }}</div>
            <div v-if="g.couponCode" class="mt-2 inline-flex rounded-2xl bg-[rgb(var(--primary))]/15 px-3 py-1 font-black keep-ltr">{{ g.couponCode }}</div>
            <div v-if="g.points" class="mt-2 text-emerald-400 font-black keep-ltr">+{{ g.points }} نقطة</div>
          </div>
          <div v-if="!(wallet?.gifts || []).length" class="text-muted rtl-text">لا توجد هدايا بعد.</div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import UiButton from '~/components/ui/UiButton.vue'
const api = useApi()
const auth = useAuthStore()
const loading = ref(false)
const wallet = ref<any>(null)

async function load(){
  if (!auth.isAuthed) return
  loading.value = true
  try { wallet.value = await api.get('/wallet/me') }
  finally { loading.value = false }
}
onMounted(load)
</script>
