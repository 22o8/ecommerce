<template>
  <div class="grid gap-6">
    <div>
      <h1 class="text-2xl font-bold">{{ t('admin.couponsLabel') }}</h1>
      <p class="text-sm text-white/70">{{ t('admin.couponsHint') }}</p>
    </div>

    <div class="grid gap-4 xl:grid-cols-[460px_1fr]">
      <div class="rounded-3xl border border-app bg-surface p-5">
        <div class="grid gap-3">
          <UiInput v-model="form.code" :placeholder="t('admin.couponCode')" />
          <UiInput v-model="form.title" :placeholder="t('admin.couponTitle')" />
          <div class="grid grid-cols-2 gap-3">
            <UiInput v-model.number="form.discountPercent" type="number" min="0" max="100" :placeholder="t('admin.discountPercent')" />
            <UiInput v-model.number="form.fixedDiscountIqd" type="number" min="0" :placeholder="t('admin.fixedDiscountIqd')" />
          </div>
          <div class="grid grid-cols-2 gap-3">
            <UiInput v-model.number="form.minimumOrderIqd" type="number" min="0" :placeholder="t('admin.minimumOrderIqd')" />
            <UiInput v-model.number="form.maxUses" type="number" min="0" :placeholder="t('admin.maxUses')" />
          </div>
          <div class="grid grid-cols-2 gap-3">
            <div>
              <label class="mb-1 block text-sm text-white/80">{{ t('admin.startsAtUtc') || 'Starts at' }}</label>
              <input v-model="form.startsAtUtc" type="datetime-local" class="w-full rounded-2xl border border-white/10 bg-white/5 px-4 py-3 outline-none focus:border-white/20" />
            </div>
            <div>
              <label class="mb-1 block text-sm text-white/80">{{ t('admin.endsAtUtc') || 'Ends at' }}</label>
              <input v-model="form.endsAtUtc" type="datetime-local" class="w-full rounded-2xl border border-white/10 bg-white/5 px-4 py-3 outline-none focus:border-white/20" />
            </div>
          </div>
          <label class="flex items-center gap-2 text-sm"><input v-model="form.isActive" type="checkbox" /> {{ t('common.active') }}</label>
          <div class="rounded-2xl border border-app bg-surface-2 p-3 text-sm text-white/70 rtl-text">
            يتم منع إعادة استخدام الكوبون من نفس الجهاز حتى لو تم إنشاء حساب جديد.
          </div>
          <div class="flex gap-2">
            <UiButton @click="save">{{ editingId ? t('common.save') : t('common.create') }}</UiButton>
            <UiButton variant="ghost" @click="resetForm">{{ t('common.cancel') }}</UiButton>
          </div>
        </div>
      </div>

      <div class="rounded-3xl border border-app bg-surface p-5">
        <div class="mb-3 flex items-center justify-between gap-3">
          <div class="text-sm text-white/70 rtl-text">{{ items.length }} كوبون</div>
          <input v-model="search" class="rounded-2xl border border-white/10 bg-white/5 px-4 py-2 outline-none focus:border-white/20" placeholder="بحث بالكود أو الاسم" />
        </div>
        <div class="grid gap-3">
          <div v-for="item in filteredItems" :key="item.id" class="rounded-2xl border border-app bg-surface-2 p-4">
            <div class="flex items-start justify-between gap-3">
              <div class="min-w-0">
                <div class="font-bold keep-ltr">{{ item.code }}</div>
                <div class="rtl-text font-semibold">{{ item.title }}</div>
                <div class="mt-2 flex flex-wrap gap-2 text-xs">
                  <span class="rounded-full border border-app px-2 py-1">{{ item.discountPercent }}%</span>
                  <span class="rounded-full border border-app px-2 py-1">{{ formatIqd(item.fixedDiscountIqd || 0) }}</span>
                  <span class="rounded-full border border-app px-2 py-1">{{ t('admin.maxUses') }}: {{ item.maxUses ?? '∞' }}</span>
                  <span class="rounded-full border border-app px-2 py-1">{{ t('admin.usedCount') || 'Used' }}: {{ item.usedCount ?? 0 }}</span>
                </div>
                <div class="mt-2 text-xs text-white/60">
                  <div>{{ t('admin.minimumOrderIqd') }}: {{ formatIqd(item.minimumOrderIqd || 0) }}</div>
                  <div v-if="item.startsAtUtc">Start: {{ fmtDate(item.startsAtUtc) }}</div>
                  <div v-if="item.endsAtUtc">End: {{ fmtDate(item.endsAtUtc) }}</div>
                </div>
              </div>
              <div class="flex flex-col items-end gap-2">
                <span class="rounded-full px-3 py-1 text-xs" :class="item.isActive ? 'bg-emerald-500/15 text-emerald-300' : 'bg-red-500/15 text-red-300'">{{ item.isActive ? 'مفعّل' : 'متوقف' }}</span>
                <div class="flex gap-2">
                  <UiButton size="sm" variant="ghost" @click="editItem(item)">{{ t('common.edit') }}</UiButton>
                  <UiButton size="sm" variant="ghost" @click="removeItem(item.id)">{{ t('common.delete') }}</UiButton>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
definePageMeta({ layout: 'admin', middleware: 'admin' })
import UiButton from '~/components/ui/UiButton.vue'
import UiInput from '~/components/ui/UiInput.vue'
import { formatIqd } from '~/composables/useMoney'
const { t } = useI18n()
const api = useAdminApi()
const items = ref<any[]>([])
const editingId = ref<string>('')
const search = ref('')
const form = reactive({ code:'', title:'', discountPercent:0, fixedDiscountIqd:0, minimumOrderIqd:0, maxUses:0 as any, isActive:true, startsAtUtc:'', endsAtUtc:'' })
const filteredItems = computed(() => {
  const s = search.value.trim().toLowerCase()
  if (!s) return items.value
  return items.value.filter((x:any) => String(x.code || '').toLowerCase().includes(s) || String(x.title || '').toLowerCase().includes(s))
})
async function load(){ items.value = await api.get('/admin/coupons') as any[] }
function resetForm(){ editingId.value=''; Object.assign(form,{ code:'', title:'', discountPercent:0, fixedDiscountIqd:0, minimumOrderIqd:0, maxUses:0, isActive:true, startsAtUtc:'', endsAtUtc:'' }) }
function toIsoOrNull(v:string){ return v ? new Date(v).toISOString() : null }
function fromIso(v?:string){ if(!v) return ''; const d=new Date(v); const pad=(n:number)=>String(n).padStart(2,'0'); return `${d.getFullYear()}-${pad(d.getMonth()+1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}` }
async function save(){
  const payload={...form, maxUses: Number(form.maxUses)||null, startsAtUtc: toIsoOrNull(form.startsAtUtc), endsAtUtc: toIsoOrNull(form.endsAtUtc)}
  if(editingId.value) await api.put(`/admin/coupons/${editingId.value}`, payload); else await api.post('/admin/coupons', payload);
  await load(); resetForm()
}
function editItem(item:any){ editingId.value=item.id; Object.assign(form,{ code:item.code, title:item.title, discountPercent:item.discountPercent, fixedDiscountIqd:item.fixedDiscountIqd, minimumOrderIqd:item.minimumOrderIqd, maxUses:item.maxUses||0, isActive:item.isActive, startsAtUtc:fromIso(item.startsAtUtc), endsAtUtc:fromIso(item.endsAtUtc) }) }
async function removeItem(id:string){ await api.del(`/admin/coupons/${id}`); await load(); if(editingId.value===id) resetForm() }
function fmtDate(v?:string){ return v ? new Date(v).toLocaleString() : '' }
onMounted(load)
</script>
