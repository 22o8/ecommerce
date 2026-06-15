<template>
  <div class="grid gap-6">
    <div class="flex flex-col gap-3 md:flex-row md:items-center md:justify-between">
      <div>
        <h1 class="text-2xl font-black rtl-text">نظام النقاط والمشاركات</h1>
        <p class="text-muted rtl-text">راقب محفظة العملاء، المشاركات، وأرسل هدايا نقاط أو كوبونات يدوياً.</p>
      </div>
      <UiButton @click="load" :loading="loading"><Icon name="mdi:refresh" /> تحديث</UiButton>
    </div>

    <div class="grid gap-4 md:grid-cols-3">
      <div class="card-soft p-5"><div class="text-muted rtl-text">عدد العملاء</div><div class="text-3xl font-black">{{ data?.users?.length || 0 }}</div></div>
      <div class="card-soft p-5"><div class="text-muted rtl-text">عمليات المشاركة</div><div class="text-3xl font-black">{{ data?.referrals?.length || 0 }}</div></div>
      <div class="card-soft p-5"><div class="text-muted rtl-text">أعلى مشارك</div><div class="text-xl font-black rtl-text">{{ data?.leaderboard?.[0]?.fullName || '-' }}</div></div>
    </div>

    <div class="card-soft p-5 overflow-x-auto">
      <h2 class="font-black text-xl rtl-text mb-4">أكثر العملاء مشاركة</h2>
      <table class="w-full text-sm">
        <thead><tr class="text-muted"><th class="text-start p-3">العميل</th><th class="p-3">الكود</th><th class="p-3">المشاركات</th><th class="p-3">النقاط</th><th class="p-3">إرسال هدية</th></tr></thead>
        <tbody>
          <tr v-for="u in data?.leaderboard || []" :key="u.id" class="border-t border-app">
            <td class="p-3"><div class="font-bold rtl-text">{{ u.fullName || u.email || u.phone }}</div><div class="text-xs text-muted keep-ltr">{{ u.email || u.phone }}</div></td>
            <td class="p-3 keep-ltr">{{ u.referralCode }}</td>
            <td class="p-3 text-center font-bold">{{ u.referrals }}</td>
            <td class="p-3 text-center font-bold">{{ u.points }}</td>
            <td class="p-3"><div class="flex gap-2"><button class="btn-mini" @click="grantPoints(u.id)">نقاط</button><button class="btn-mini" @click="grantCoupon(u.id)">كوبون</button></div></td>
          </tr>
        </tbody>
      </table>
    </div>

    <div class="card-soft p-5 overflow-x-auto">
      <h2 class="font-black text-xl rtl-text mb-4">آخر التسجيلات بكود المشاركة</h2>
      <table class="w-full text-sm">
        <thead><tr class="text-muted"><th class="text-start p-3">صاحب الكود</th><th class="text-start p-3">الحساب الجديد</th><th class="p-3">الحالة</th><th class="p-3">هدية</th></tr></thead>
        <tbody>
          <tr v-for="r in data?.referrals || []" :key="r.id" class="border-t border-app">
            <td class="p-3 rtl-text">{{ r.referrer?.fullName || r.referrer?.email }}</td>
            <td class="p-3 rtl-text">{{ r.referred?.fullName || r.referred?.email }}</td>
            <td class="p-3 text-center">{{ r.rewarded ? 'تمت المكافأة' : 'ينتظر هدية' }}</td>
            <td class="p-3"><div class="flex gap-2 justify-center"><button class="btn-mini" @click="grantPoints(r.referrer?.referrerUserId, r.id)">نقاط</button><button class="btn-mini" @click="grantCoupon(r.referrer?.referrerUserId, r.id)">كوبون</button></div></td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>

<script setup lang="ts">
import UiButton from '~/components/ui/UiButton.vue'
definePageMeta({ layout: 'admin' })
const api = useApi()
const loading = ref(false)
const data = ref<any>(null)
async function load(){ loading.value = true; try{ data.value = await api.get('/admin/loyalty/summary') } finally{ loading.value = false } }
async function grantPoints(userId:string, referralId?:string){
  const points = Number(prompt('كم نقطة تريد إرسالها؟', '100') || '0')
  if (!points) return
  await api.post('/admin/loyalty/grant-points', { userId, points, referralId, note: 'هدية من الإدارة' })
  await load()
}
async function grantCoupon(userId:string, referralId?:string){
  const couponCode = prompt('اكتب كود الكوبون الذي تريد إرساله')
  if (!couponCode) return
  await api.post('/admin/loyalty/grant-coupon', { userId, couponCode, referralId, note: 'كوبون هدية من الإدارة' })
  await load()
}
onMounted(load)
</script>

<style scoped>
.btn-mini{ @apply rounded-2xl border border-app bg-surface px-3 py-2 font-bold hover:bg-[rgb(var(--primary))]/10; }
</style>
