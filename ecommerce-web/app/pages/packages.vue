<template>
  <div class="grid gap-8">
    <section class="card-soft p-6 md:p-10 text-center">
      <div class="mx-auto h-14 w-14 rounded-3xl bg-[rgb(var(--primary))]/15 grid place-items-center">
        <Icon name="mdi:package-variant-closed" class="text-3xl text-[rgb(var(--primary))]" />
      </div>
      <h1 class="mt-4 text-3xl md:text-5xl font-black rtl-text">بكجات العناية الكورية</h1>
      <p class="mt-3 text-muted rtl-text max-w-2xl mx-auto">روتينات جاهزة تجمع أكثر من منتج بسعر خاص وتوفير واضح.</p>
    </section>

    <div v-if="pending" class="card-soft p-6 text-center rtl-text">جاري تحميل البكجات...</div>
    <div v-else-if="!packages.length" class="card-soft p-6 text-center rtl-text text-muted">لا توجد بكجات متاحة حالياً.</div>

    <section v-else class="grid gap-5 md:grid-cols-2 xl:grid-cols-3">
      <article v-for="pkg in packages" :key="pkg.id" class="card-soft overflow-hidden grid">
        <div class="aspect-[4/3] bg-black/20 grid place-items-center overflow-hidden">
          <video v-if="pkg.mediaType === 'video' && pkg.coverUrl" :src="asset(pkg.coverUrl)" muted playsinline controls preload="metadata" class="h-full w-full object-contain"></video>
          <img v-else-if="pkg.coverUrl" :src="asset(pkg.coverUrl)" :alt="pkg.name" class="h-full w-full object-contain" loading="lazy" />
          <Icon v-else name="mdi:package-variant" class="text-7xl text-white/30" />
        </div>
        <div class="p-5 grid gap-3">
          <div class="flex items-start justify-between gap-3">
            <h2 class="text-xl font-black rtl-text">{{ pkg.name }}</h2>
            <span v-if="pkg.savingsPercent > 0" class="badge">وفر {{ pkg.savingsPercent }}%</span>
          </div>
          <p class="text-sm text-muted rtl-text leading-7">{{ pkg.shortDescription }}</p>
          <div class="flex flex-wrap items-center gap-3">
            <span class="text-2xl font-black text-[rgb(var(--primary))]">{{ money(pkg.finalPriceIqd) }}</span>
            <span v-if="pkg.originalPriceIqd > pkg.finalPriceIqd" class="text-muted line-through">{{ money(pkg.originalPriceIqd) }}</span>
          </div>
          <div class="rounded-2xl border border-white/10 p-3 text-sm rtl-text">
            <div class="font-black mb-2">محتويات البكج:</div>
            <ul class="grid gap-1 text-muted">
              <li v-for="item in pkg.items" :key="item.productId">{{ item.quantity }}× {{ item.productTitle }}</li>
            </ul>
          </div>
          <div v-if="!pkg.canSell" class="rounded-2xl border border-yellow-500/30 bg-yellow-500/10 p-3 text-yellow-100 rtl-text text-sm">هذا البكج غير متوفر حالياً.</div>
          <button class="btn-buy" :disabled="!pkg.canSell || buying === pkg.id" @click="buy(pkg)">{{ buying === pkg.id ? 'جاري الطلب...' : 'شراء البكج' }}</button>
        </div>
      </article>
    </section>
  </div>
</template>

<script setup lang="ts">
import { buildAssetUrl } from '~/composables/useApi'

const api = useApi()
const packages = ref<any[]>([])
const pending = ref(true)
const buying = ref('')
const asset = (v: string) => buildAssetUrl(v)
const money = (v: number) => `${Number(v || 0).toLocaleString('en-US')} د.ع`

async function loadPackages() {
  pending.value = true
  try { packages.value = await api.get<any[]>('/packages') }
  finally { pending.value = false }
}
async function buy(pkg: any) {
  buying.value = pkg.id
  try {
    const res = await api.post<any>('/checkout/packages', { packageId: pkg.id, quantity: 1 })
    await navigateTo(`/orders/${res.orderId}`)
  } catch (e: any) {
    alert(e?.friendlyMessage || e?.message || 'فشل إنشاء الطلب')
  } finally { buying.value = '' }
}

useHead({ title: 'بكجات العناية الكورية | DR SEOUL BEAUTY' })
onMounted(loadPackages)
</script>

<style scoped>
.badge{border:1px solid rgba(255,255,255,.12);border-radius:999px;padding:.25rem .65rem;font-size:.75rem;background:rgba(34,197,94,.15);color:#bbf7d0}.btn-buy{border-radius:1.1rem;padding:.85rem 1rem;font-weight:900;background:rgb(var(--primary));color:#111827}.btn-buy:disabled{opacity:.5;cursor:not-allowed}
</style>
