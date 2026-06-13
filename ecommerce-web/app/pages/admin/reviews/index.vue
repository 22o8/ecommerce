<script setup lang="ts">
definePageMeta({ layout: 'admin', middleware: 'admin' })

const api = useAdminApi()
const toast = useToast()
const status = ref('all')

const { data, pending, refresh } = await useAsyncData(
  () => `admin-reviews-${status.value}`,
  async () => await api.get<any>('/admin/reviews', { status: status.value, page: 1, pageSize: 80 }),
  { watch: [status] }
)

const items = computed(() => Array.isArray(data.value?.items) ? data.value.items : [])
const stats = computed(() => {
  const arr = items.value
  return {
    total: Number(data.value?.totalCount || arr.length),
    approved: arr.filter((x:any) => x.status === 'Approved').length,
    pending: arr.filter((x:any) => x.status === 'Pending').length,
    hidden: arr.filter((x:any) => x.status === 'Hidden').length,
    verified: arr.filter((x:any) => x.isVerifiedPurchase).length,
  }
})

function imagesOf(r: any) {
  try {
    const raw = r.imageUrlsJson || r.ImageUrlsJson
    const arr = typeof raw === 'string' ? JSON.parse(raw) : raw
    return Array.isArray(arr) ? arr : []
  } catch { return [] }
}

async function updateReview(r:any, patch:any) {
  try {
    await api.put(`/admin/reviews/${r.id}`, patch)
    toast.success('تم تحديث التقييم')
    await refresh()
  } catch (e:any) {
    toast.error(e?.data?.message || e?.message || 'تعذر تحديث التقييم')
  }
}

async function deleteReview(r:any) {
  if (!confirm('حذف هذا التقييم نهائياً؟')) return
  try {
    await api.del(`/admin/reviews/${r.id}`)
    toast.success('تم حذف التقييم')
    await refresh()
  } catch (e:any) {
    toast.error(e?.data?.message || e?.message || 'تعذر حذف التقييم')
  }
}
</script>

<template>
  <main class="grid gap-6">
    <section class="rounded-[2rem] border border-app bg-surface p-6">
      <div class="flex flex-wrap items-center justify-between gap-4">
        <div>
          <h1 class="text-2xl font-black">Reviews & Ratings Pro</h1>
          <p class="mt-2 text-sm text-muted rtl-text">إدارة التقييمات التي تغذي Product Schema ونجوم Google Rich Results.</p>
        </div>
        <select v-model="status" class="rounded-2xl border border-app bg-surface px-4 py-3">
          <option value="all">كل التقييمات</option>
          <option value="Approved">المقبولة</option>
          <option value="Pending">بانتظار المراجعة</option>
          <option value="Hidden">المخفية</option>
        </select>
      </div>
      <div class="mt-5 grid gap-3 sm:grid-cols-2 lg:grid-cols-5">
        <div class="seo-stat"><b>{{ stats.total }}</b><span>الإجمالي</span></div>
        <div class="seo-stat"><b>{{ stats.approved }}</b><span>مقبولة</span></div>
        <div class="seo-stat"><b>{{ stats.pending }}</b><span>قيد المراجعة</span></div>
        <div class="seo-stat"><b>{{ stats.hidden }}</b><span>مخفية</span></div>
        <div class="seo-stat"><b>{{ stats.verified }}</b><span>مشتري موثق</span></div>
      </div>
    </section>

    <section class="rounded-[2rem] border border-app bg-surface p-4 sm:p-6">
      <div v-if="pending" class="text-muted">جاري التحميل...</div>
      <div v-else-if="!items.length" class="rounded-3xl bg-surface-2 p-6 text-center text-muted">لا توجد تقييمات حالياً.</div>
      <div v-else class="grid gap-4">
        <article v-for="r in items" :key="r.id" class="rounded-3xl border border-app bg-surface-2 p-4">
          <div class="flex flex-wrap items-start justify-between gap-3">
            <div>
              <NuxtLink v-if="r.productSlug" :to="`/products/${r.productSlug}`" class="font-black hover:text-primary">{{ r.productTitle || 'منتج' }}</NuxtLink>
              <div class="mt-1 text-sm text-muted">{{ r.userName || r.reviewerName || 'مستخدم' }}</div>
            </div>
            <div class="flex items-center gap-1 text-amber-400">
              <Icon v-for="n in 5" :key="n" :name="Number(r.rating) >= n ? 'mdi:star' : 'mdi:star-outline'" class="text-lg" />
            </div>
          </div>

          <p v-if="r.comment" class="mt-3 text-sm leading-7 rtl-text whitespace-pre-line">{{ r.comment }}</p>
          <div v-if="imagesOf(r).length" class="mt-3 flex gap-2 overflow-x-auto">
            <img v-for="img in imagesOf(r)" :key="img" :src="img" class="h-20 w-20 rounded-2xl object-cover border border-app" alt="review image" loading="lazy" />
          </div>

          <div class="mt-4 flex flex-wrap items-center gap-2">
            <span class="rounded-full px-3 py-1 text-xs font-bold" :class="r.status === 'Approved' ? 'bg-emerald-500/15 text-emerald-400' : r.status === 'Hidden' ? 'bg-red-500/15 text-red-400' : 'bg-amber-500/15 text-amber-400'">{{ r.status }}</span>
            <span v-if="r.isVerifiedPurchase" class="rounded-full bg-sky-500/15 px-3 py-1 text-xs font-bold text-sky-400">✓ مشتري موثق</span>
            <button class="admin-mini" @click="updateReview(r, { status: 'Approved' })">قبول</button>
            <button class="admin-mini" @click="updateReview(r, { status: 'Hidden' })">إخفاء</button>
            <button class="admin-mini" @click="updateReview(r, { isVerifiedPurchase: true })">توثيق الشراء</button>
            <button class="admin-mini danger" @click="deleteReview(r)">حذف</button>
          </div>
        </article>
      </div>
    </section>
  </main>
</template>

<style scoped>
.seo-stat{border:1px solid rgba(var(--border),.9);border-radius:1.25rem;padding:1rem;background:rgba(var(--surface-2-rgb),.72);display:grid;gap:.25rem}.seo-stat b{font-size:1.6rem}.seo-stat span{font-size:.8rem;color:rgb(var(--muted))}.admin-mini{border:1px solid rgba(var(--border),.9);border-radius:999px;padding:.45rem .8rem;font-size:.8rem;font-weight:900;background:rgba(var(--surface-rgb),.8)}.admin-mini.danger{color:rgb(var(--danger))}
</style>
