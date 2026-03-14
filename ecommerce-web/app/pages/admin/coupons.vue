<template>
  <div class="grid gap-6">
    <div>
      <h1 class="text-2xl font-bold">{{ t('admin.couponsLabel') }}</h1>
      <p class="text-sm text-white/70">{{ t('admin.couponsHint') }}</p>
    </div>

    <div class="grid gap-4 lg:grid-cols-[420px_1fr]">
      <div class="rounded-3xl border border-app bg-surface p-5">
        <div class="grid gap-3">
          <UiInput v-model="form.code" :placeholder="t('admin.couponCode')" />
          <UiInput v-model="form.title" :placeholder="t('admin.couponTitle')" />
          <UiInput v-model.number="form.discountPercent" type="number" min="0" max="100" :placeholder="t('admin.discountPercent')" />
          <UiInput v-model.number="form.fixedDiscountIqd" type="number" min="0" :placeholder="t('admin.fixedDiscountIqd')" />
          <UiInput v-model.number="form.minimumOrderIqd" type="number" min="0" :placeholder="t('admin.minimumOrderIqd')" />
          <UiInput v-model.number="form.maxUses" type="number" min="0" :placeholder="t('admin.maxUses')" />
          <label class="flex items-center gap-2 text-sm"><input v-model="form.isActive" type="checkbox" /> {{ t('common.active') }}</label>
          <div class="flex gap-2">
            <UiButton @click="save">{{ editingId ? t('common.save') : t('common.create') }}</UiButton>
            <UiButton variant="ghost" @click="resetForm">{{ t('common.cancel') }}</UiButton>
          </div>
        </div>
      </div>

      <div class="rounded-3xl border border-app bg-surface p-5">
        <div class="grid gap-3">
          <div v-for="item in items" :key="item.id" class="rounded-2xl border border-app bg-surface-2 p-4 flex items-start justify-between gap-3">
            <div>
              <div class="font-bold keep-ltr">{{ item.code }}</div>
              <div class="rtl-text">{{ item.title }}</div>
              <div class="text-xs text-muted mt-1">{{ item.discountPercent }}% / {{ formatIqd(item.fixedDiscountIqd || 0) }}</div>
            </div>
            <div class="flex gap-2">
              <UiButton size="sm" variant="ghost" @click="editItem(item)">{{ t('common.edit') }}</UiButton>
              <UiButton size="sm" variant="ghost" @click="removeItem(item.id)">{{ t('common.delete') }}</UiButton>
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
const form = reactive({ code:'', title:'', discountPercent:0, fixedDiscountIqd:0, minimumOrderIqd:0, maxUses:0 as any, isActive:true })
async function load(){ items.value = await api.get('/admin/coupons') as any[] }
function resetForm(){ editingId.value=''; Object.assign(form,{ code:'', title:'', discountPercent:0, fixedDiscountIqd:0, minimumOrderIqd:0, maxUses:0, isActive:true }) }
async function save(){ const payload={...form, maxUses: Number(form.maxUses)||null}; if(editingId.value) await api.put(`/admin/coupons/${editingId.value}`, payload); else await api.post('/admin/coupons', payload); await load(); resetForm() }
function editItem(item:any){ editingId.value=item.id; Object.assign(form,{ code:item.code, title:item.title, discountPercent:item.discountPercent, fixedDiscountIqd:item.fixedDiscountIqd, minimumOrderIqd:item.minimumOrderIqd, maxUses:item.maxUses||0, isActive:item.isActive }) }
async function removeItem(id:string){ await api.del(`/admin/coupons/${id}`); await load(); if(editingId.value===id) resetForm() }
onMounted(load)
</script>