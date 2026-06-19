<template>
  <div class="grid gap-6">
    <div class="card-soft p-6 md:p-10">
      <div class="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h1 class="text-2xl md:text-3xl font-black rtl-text">الإشعارات والهدايا</h1>
          <p class="mt-1 text-muted rtl-text">هنا تظهر هدايا النقاط والكوبونات والتنبيهات التي تصل إلى حسابك.</p>
        </div>
        <div class="flex flex-wrap gap-2">
          <UiButton variant="secondary" :loading="loading" @click="load"><Icon name="mdi:refresh" /><span>تحديث</span></UiButton>
          <UiButton v-if="unreadCount" :loading="marking" @click="markAllRead"><Icon name="mdi:check-all" /><span>تعليم الكل كمقروء</span></UiButton>
        </div>
      </div>
    </div>

    <div v-if="!auth.isAuthed" class="card-soft p-6 rtl-text">
      سجل دخولك حتى تشاهد إشعاراتك.
      <NuxtLink to="/login" class="font-bold text-[rgb(var(--primary))]">تسجيل الدخول</NuxtLink>
    </div>

    <div v-else class="grid gap-3">
      <div class="rounded-3xl border border-app bg-surface p-4 text-sm text-muted rtl-text">
        لديك <b class="text-[rgb(var(--primary))]">{{ unreadCount }}</b> إشعار غير مقروء.
      </div>
      <button
        v-for="g in gifts"
        :key="g.id"
        type="button"
        class="rounded-3xl border border-app bg-surface p-5 text-start transition hover:bg-surface-2"
        :class="{ 'ring-2 ring-[rgb(var(--primary))]/35': !g.isRead }"
        :aria-label="g.title || 'إشعار'"
        @click="markRead(g)"
      >
        <div class="flex items-start gap-3">
          <div class="grid h-11 w-11 place-items-center rounded-2xl bg-[rgb(var(--primary))]/20 text-[rgb(var(--primary))]">
            <Icon :name="g.giftType === 'Coupon' ? 'mdi:ticket-percent-outline' : g.giftType === 'Points' ? 'mdi:star-circle-outline' : 'mdi:gift-outline'" class="text-2xl" />
          </div>
          <div class="min-w-0 flex-1">
            <div class="flex flex-wrap items-center gap-2">
              <div class="font-black rtl-text">{{ g.title || 'هدية من الإدارة' }}</div>
              <span v-if="!g.isRead" class="rounded-full bg-red-500 px-2 py-0.5 text-[11px] font-black text-white">جديد</span>
            </div>
            <div class="mt-1 text-sm text-muted rtl-text">{{ g.message }}</div>
            <div class="mt-3 flex flex-wrap items-center gap-2">
              <span v-if="g.points" class="rounded-2xl bg-emerald-500/15 px-3 py-1 font-black text-emerald-400 keep-ltr">+{{ g.points }} نقطة</span>
              <span v-if="g.couponCode" class="rounded-2xl bg-[rgb(var(--primary))]/15 px-3 py-1 font-black keep-ltr">{{ g.couponCode }}</span>
              <span class="text-xs text-muted keep-ltr">{{ formatDate(g.createdAtUtc) }}</span>
            </div>
          </div>
        </div>
      </button>
      <div v-if="!gifts.length && !loading" class="card-soft p-6 text-muted rtl-text">لا توجد إشعارات حالياً.</div>
    </div>
  </div>
</template>

<script setup lang="ts">
import UiButton from '~/components/ui/UiButton.vue'
const api = useApi()
const auth = useAuthStore()
const loading = ref(false)
const marking = ref(false)
const gifts = ref<any[]>([])
const unreadCount = computed(() => gifts.value.filter((x: any) => !x.isRead).length)
function formatDate(value: string) {
  if (!value) return ''
  try { return new Date(value).toLocaleString('ar-IQ') } catch { return value }
}
async function load(){
  if (!auth.isAuthed) return
  loading.value = true
  try {
    const res:any = await api.get('/wallet/notifications?take=80')
    gifts.value = Array.isArray(res?.items) ? res.items : []
  }
  finally { loading.value = false }
}
async function markRead(g: any){
  if (!g?.id || g.isRead) return
  try {
    await api.post(`/wallet/notifications/${g.id}/read`, {})
    g.isRead = true
  } catch {}
}
async function markAllRead(){
  marking.value = true
  try {
    await api.post('/wallet/notifications/read-all', {})
    gifts.value = gifts.value.map((x: any) => ({ ...x, isRead: true }))
  } finally { marking.value = false }
}
onMounted(load)
</script>
