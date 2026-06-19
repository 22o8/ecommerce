<template>
  <div v-if="auth.isAuthed" class="notification-center">
    <button
      type="button"
      class="notification-center__button"
      aria-label="الإشعارات"
      title="الإشعارات"
      @click="togglePanel"
    >
      <Icon name="mdi:bell-outline" class="text-lg" />
      <span class="hidden md:inline rtl-text">الإشعارات</span>
      <span v-if="unreadCount" class="notification-center__badge" aria-hidden="true"></span>
    </button>

    <div v-if="panelOpen" class="notification-center__panel" role="dialog" aria-modal="false" aria-label="مركز الإشعارات">
      <div class="notification-center__head">
        <div>
          <b class="rtl-text">الإشعارات والهدايا</b>
          <p class="rtl-text">النقاط والكوبونات والتنبيهات الجديدة تظهر هنا.</p>
        </div>
        <button type="button" aria-label="إغلاق الإشعارات" @click="panelOpen = false">
          <Icon name="mdi:close" />
        </button>
      </div>

      <div v-if="loading" class="notification-center__empty rtl-text">جاري التحميل...</div>
      <div v-else-if="!items.length" class="notification-center__empty rtl-text">لا توجد إشعارات حالياً.</div>
      <div v-else class="notification-center__list">
        <button
          v-for="item in items"
          :key="item.id"
          type="button"
          class="notification-center__item"
          :class="{ 'is-unread': !item.isRead }"
          :aria-label="item.title || 'إشعار'"
          @click="markRead(item)"
        >
          <span class="notification-center__icon">
            <Icon :name="iconFor(item)" />
          </span>
          <span class="min-w-0 flex-1">
            <b class="rtl-text">{{ item.title || 'إشعار جديد' }}</b>
            <small class="rtl-text">{{ item.message || 'لديك إشعار جديد.' }}</small>
            <em v-if="item.points" class="keep-ltr">+{{ item.points }} نقطة</em>
            <em v-if="item.couponCode" class="keep-ltr">{{ item.couponCode }}</em>
          </span>
        </button>
      </div>

      <div class="notification-center__actions">
        <NuxtLink to="/notifications" class="rtl-text" @click="panelOpen = false">عرض الكل</NuxtLink>
        <button v-if="unreadCount" type="button" class="rtl-text" @click="markAllRead">تعليم الكل كمقروء</button>
      </div>
    </div>

    <Transition name="gift-toast">
      <div v-if="toastItem" class="notification-center__toast" role="status" aria-live="polite">
        <div class="notification-center__toast-icon">
          <Icon :name="iconFor(toastItem)" />
        </div>
        <div class="min-w-0 flex-1">
          <b class="rtl-text">{{ toastItem.title || 'وصلتك هدية جديدة' }}</b>
          <p class="rtl-text">{{ toastItem.message || 'لديك إشعار جديد في حسابك.' }}</p>
          <div class="notification-center__toast-actions">
            <NuxtLink to="/notifications" @click="dismissToast">عرض الإشعار</NuxtLink>
            <button type="button" @click="dismissToast">إغلاق</button>
          </div>
        </div>
      </div>
    </Transition>
  </div>
</template>

<script setup lang="ts">
const api = useApi()
const auth = useAuthStore()
const panelOpen = ref(false)
const loading = ref(false)
const unreadCount = ref(0)
const items = ref<any[]>([])
const toastItem = ref<any | null>(null)
let pollTimer: ReturnType<typeof setInterval> | null = null

function iconFor(item: any) {
  const type = String(item?.giftType || '').toLowerCase()
  if (type.includes('coupon')) return 'mdi:ticket-percent-outline'
  if (type.includes('point')) return 'mdi:star-circle-outline'
  return 'mdi:gift-outline'
}

async function loadNotifications(showToast = false) {
  if (!auth.isAuthed) return
  loading.value = true
  try {
    const res: any = await api.get('/wallet/notifications?take=20')
    const list = Array.isArray(res?.items) ? res.items : []
    items.value = list
    unreadCount.value = Number(res?.unread ?? list.filter((x: any) => !x.isRead).length ?? 0)
    if (showToast && unreadCount.value > 0) {
      const firstUnread = list.find((x: any) => !x.isRead)
      if (firstUnread && process.client) {
        const seenKey = `drsb_seen_notification_${firstUnread.id}`
        if (!localStorage.getItem(seenKey)) {
          toastItem.value = firstUnread
          localStorage.setItem(seenKey, '1')
        }
      }
    }
  } catch {
    items.value = []
    unreadCount.value = 0
  } finally {
    loading.value = false
  }
}

async function togglePanel() {
  panelOpen.value = !panelOpen.value
  if (panelOpen.value) await loadNotifications(false)
}

async function markRead(item: any) {
  if (!item?.id || item.isRead) return
  try {
    await api.post(`/wallet/notifications/${item.id}/read`, {})
    item.isRead = true
    unreadCount.value = Math.max(0, unreadCount.value - 1)
  } catch {}
}

async function markAllRead() {
  try {
    await api.post('/wallet/notifications/read-all', {})
    items.value = items.value.map((x: any) => ({ ...x, isRead: true }))
    unreadCount.value = 0
  } catch {}
}

async function dismissToast() {
  const item = toastItem.value
  toastItem.value = null
  if (item?.id) await markRead(item)
}

watch(() => auth.isAuthed, (value) => {
  if (value) loadNotifications(false)
  else {
    items.value = []
    unreadCount.value = 0
    toastItem.value = null
  }
}, { immediate: true })

onMounted(() => {
  if (auth.isAuthed) loadNotifications(false)
  pollTimer = setInterval(() => {
    if (auth.isAuthed) loadNotifications(false)
  }, 60000)
})

onBeforeUnmount(() => {
  if (pollTimer) clearInterval(pollTimer)
})
</script>

<style scoped>
.notification-center{ position:relative; }
.notification-center__button{
  position:relative;
  display:inline-flex;
  align-items:center;
  justify-content:center;
  gap:.45rem;
  min-height:40px;
  border-radius:1rem;
  border:1px solid rgb(var(--border));
  background:rgb(var(--surface));
  padding:.5rem .7rem;
  font-weight:800;
  color:rgb(var(--text));
  transition:.18s ease;
}
.notification-center__button:hover{ transform:translateY(-1px); background:rgb(var(--surface-2)); }
.notification-center__badge{
  position:absolute;
  top:.18rem;
  right:.22rem;
  width:.62rem;
  height:.62rem;
  border-radius:999px;
  background:#ef4444;
  border:2px solid rgb(var(--surface));
  box-shadow:0 0 0 4px rgba(239,68,68,.14), 0 8px 18px rgba(239,68,68,.35);
}
.notification-center__panel{
  position:absolute;
  top:calc(100% + .65rem);
  left:0;
  z-index:70;
  width:min(22rem, calc(100vw - 1rem));
  overflow:hidden;
  border-radius:1.35rem;
  border:1px solid rgb(var(--border));
  background:rgb(var(--surface));
  background:#0b0f19;
  box-shadow:0 30px 90px rgba(0,0,0,.58), 0 0 0 1px rgba(255,255,255,.04) inset;
  backdrop-filter:none;
  isolation:isolate;
}
.notification-center__head{ display:flex; justify-content:space-between; gap:1rem; padding:1rem; border-bottom:1px solid rgba(255,255,255,.10); background:#101624; }
.notification-center__head p{ margin-top:.15rem; font-size:.78rem; color:rgb(var(--muted)); }
.notification-center__head button{ display:grid; place-items:center; width:2rem; height:2rem; border-radius:.85rem; background:#1b2233; color:#fff; }
.notification-center__list{ max-height:22rem; overflow:auto; padding:.55rem; background:#0b0f19; }
.notification-center__item{ width:100%; display:flex; gap:.75rem; text-align:start; border-radius:1rem; padding:.8rem; transition:.18s ease; background:#111827; border:1px solid rgba(255,255,255,.07); margin-bottom:.5rem; color:#fff; }
.notification-center__item:hover{ background:#172033; border-color:rgba(var(--primary), .32); }
.notification-center__item.is-unread{ background:#171d31; border-color:rgba(var(--primary), .42); box-shadow:0 10px 28px rgba(var(--primary), .10); }
.notification-center__icon{ display:grid; width:2.35rem; height:2.35rem; flex:0 0 auto; place-items:center; border-radius:.9rem; background:rgba(var(--primary), .18); color:rgb(var(--primary)); font-size:1.25rem; }
.notification-center__item b{ display:block; font-size:.9rem; }
.notification-center__item small{ display:block; margin-top:.18rem; color:#b7c0d4; font-size:.76rem; line-height:1.45; }
.notification-center__item em{ display:inline-flex; margin-top:.45rem; margin-inline-end:.35rem; border-radius:999px; background:rgba(16,185,129,.15); color:#10b981; padding:.16rem .5rem; font-size:.72rem; font-style:normal; font-weight:1000; }
.notification-center__empty{ padding:1rem; color:rgb(var(--muted)); font-size:.9rem; }
.notification-center__actions{ display:flex; align-items:center; justify-content:space-between; gap:.75rem; border-top:1px solid rgba(255,255,255,.10); padding:.8rem 1rem; font-size:.82rem; font-weight:900; color:rgb(var(--primary)); background:#101624; }
.notification-center__toast{
  position:fixed;
  right:1rem;
  bottom:1rem;
  z-index:90;
  display:flex;
  width:min(24rem, calc(100vw - 2rem));
  gap:.9rem;
  border-radius:1.35rem;
  border:1px solid rgba(var(--primary), .28);
  background:linear-gradient(135deg, #0f1422, #151c2d);
  box-shadow:0 24px 75px rgba(0,0,0,.30);
  padding:1rem;
  backdrop-filter:none;
}
.notification-center__toast-icon{ display:grid; width:3rem; height:3rem; place-items:center; border-radius:1rem; background:rgba(var(--primary), .18); color:rgb(var(--primary)); font-size:1.5rem; }
.notification-center__toast p{ margin-top:.25rem; color:rgb(var(--muted)); font-size:.85rem; line-height:1.6; }
.notification-center__toast-actions{ display:flex; gap:.7rem; margin-top:.75rem; font-size:.82rem; font-weight:1000; color:rgb(var(--primary)); }
.gift-toast-enter-active,.gift-toast-leave-active{ transition:.25s ease; }
.gift-toast-enter-from,.gift-toast-leave-to{ opacity:0; transform:translateY(12px); }
@media (max-width:640px){
  .notification-center__button{ min-width:40px; padding:.5rem .6rem; }
  .notification-center__panel{ position:fixed; top:5.5rem; left:.5rem; right:.5rem; width:auto; }
}
</style>
