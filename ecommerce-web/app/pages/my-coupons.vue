<template>
  <div class="grid gap-6">
    <div class="card-soft p-6 md:p-10">
      <div class="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h1 class="text-2xl md:text-3xl font-black rtl-text">قسائمي</h1>
          <p class="mt-1 text-muted rtl-text">الكوبونات التي وصلت لك من الإدارة بعد الهدايا أو استبدال النقاط.</p>
        </div>
        <UiButton variant="secondary" :loading="loading" @click="load"><Icon name="mdi:refresh" /><span>تحديث</span></UiButton>
      </div>
    </div>

    <div v-if="!auth.isAuthed" class="card-soft p-6 rtl-text">
      سجل دخولك حتى تشاهد قسائمك.
      <NuxtLink to="/login" class="font-bold text-[rgb(var(--primary))]">تسجيل الدخول</NuxtLink>
    </div>

    <div v-else class="grid gap-3 md:grid-cols-2">
      <div v-for="c in coupons" :key="c.id" class="rounded-3xl border border-app bg-surface p-5">
        <div class="text-sm text-muted rtl-text">كوبون خصم</div>
        <div class="mt-2 inline-flex rounded-2xl bg-[rgb(var(--primary))]/15 px-4 py-2 text-2xl font-black keep-ltr">{{ c.couponCode }}</div>
        <p class="mt-3 text-sm text-muted rtl-text">{{ c.message || 'استخدم هذا الكود في السلة عند الشراء.' }}</p>
      </div>
      <div v-if="!coupons.length && !loading" class="card-soft p-6 text-muted rtl-text md:col-span-2">لا توجد قسائم حالياً.</div>
    </div>
  </div>
</template>

<script setup lang="ts">
import UiButton from '~/components/ui/UiButton.vue'
const api = useApi()
const auth = useAuthStore()
const loading = ref(false)
const coupons = ref<any[]>([])
async function load(){
  if (!auth.isAuthed) return
  loading.value = true
  try { const w:any = await api.get('/wallet/me'); coupons.value = (w?.gifts || []).filter((x:any)=>!!x.couponCode) }
  finally { loading.value = false }
}
onMounted(load)
</script>
