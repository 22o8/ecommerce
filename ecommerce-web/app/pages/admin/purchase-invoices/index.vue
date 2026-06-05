<template>
  <div class="purchase-admin-page" dir="rtl">
    <section class="admin-hero-card">
      <div>
        <p class="eyebrow">إدارة المبيعات</p>
        <h1>فواتير الشراء</h1>
        <p class="hint">
          كل عملية شراء تظهر هنا بحالة <b>قيد التنفيذ</b>. لا يتم خصم كمية المنتج ولا تسجيل الأرباح إلا بعد ضغط <b>تم البيع</b>.
        </p>
      </div>
      <div class="hero-actions">
        <button class="btn ghost" type="button" @click="fetchInvoices" :disabled="loading">
          <Icon name="mdi:refresh" />
          تحديث
        </button>
        <button class="btn danger" type="button" @click="deleteAllInvoices" :disabled="loading || invoices.length === 0">
          <Icon name="mdi:trash-can-outline" />
          حذف الكل
        </button>
      </div>
    </section>

    <section class="rules-card">
      <div class="rule"><Icon name="mdi:clock-outline" /><span>قيد التنفيذ: فاتورة محفوظة بانتظار تأكيد الأدمن.</span></div>
      <div class="rule"><Icon name="mdi:check-decagram-outline" /><span>تم البيع: يتم خصم الكمية وتسجيل الربح.</span></div>
      <div class="rule"><Icon name="mdi:close-octagon-outline" /><span>لم تبع: يتم حذف الفاتورة ولا تتأثر كمية المنتج.</span></div>
    </section>

    <section class="stats-grid">
      <div class="stat-card">
        <Icon name="mdi:receipt-text-outline" />
        <div><strong>{{ stats.total }}</strong><span>كل الفواتير</span></div>
      </div>
      <div class="stat-card warning">
        <Icon name="mdi:timer-sand" />
        <div><strong>{{ stats.pending }}</strong><span>قيد التنفيذ</span></div>
      </div>
      <div class="stat-card success">
        <Icon name="mdi:cash-check" />
        <div><strong>{{ stats.sold }}</strong><span>تم البيع</span></div>
      </div>
      <div class="stat-card profit">
        <Icon name="mdi:chart-line" />
        <div><strong>{{ formatIqd(stats.profit) }}</strong><span>الأرباح المسجلة</span></div>
      </div>
    </section>

    <section class="toolbar-card">
      <input v-model="search" class="admin-control" type="search" placeholder="ابحث باسم الزبون، البريد، أو المنتج..." @keyup.enter="fetchInvoices" />
      <select v-model="status" class="admin-control" @change="fetchInvoices">
        <option value="all">كل الحالات</option>
        <option value="PendingSale">قيد التنفيذ</option>
        <option value="Sold">تم البيع</option>
      </select>
      <button class="btn primary" type="button" @click="fetchInvoices" :disabled="loading">
        <Icon name="mdi:magnify" />
        بحث
      </button>
    </section>

    <section class="invoices-card">
      <div v-if="loading" class="empty-state">جاري تحميل الفواتير...</div>
      <div v-else-if="error" class="error-state">{{ error }}</div>
      <div v-else-if="filteredInvoices.length === 0" class="empty-state">
        <Icon name="mdi:receipt-text-remove-outline" />
        <strong>لا توجد فواتير حالياً</strong>
        <span>عند تنفيذ شراء جديد سيظهر هنا تلقائياً.</span>
      </div>

      <article v-for="invoice in filteredInvoices" v-else :key="invoice.id" class="invoice-row">
        <div class="invoice-main">
          <div class="topline">
            <span :class="statusClass(invoice.status)">{{ statusLabel(invoice.status) }}</span>
            <span class="code">#{{ invoiceCode(invoice) }}</span>
            <span class="date">{{ formatDate(invoice.createdAt) }}</span>
          </div>

          <h3>{{ invoice.primaryItemTitle || invoice.items?.[0]?.title || 'فاتورة شراء' }}</h3>
          <p class="customer">
            <Icon name="mdi:account-outline" />
            {{ invoice.customer?.name || invoice.customer?.email || 'زبون' }}
          </p>

          <div class="items-list">
            <div v-for="item in invoice.items" :key="item.id" class="item-line">
              <span>{{ item.title }}</span>
              <small>{{ item.brand || 'بدون براند' }}</small>
              <b>× {{ item.quantity }}</b>
              <strong>{{ formatIqd(item.lineTotalIqd) }}</strong>
            </div>
          </div>
        </div>

        <div class="invoice-side">
          <div class="money-box">
            <span>الإجمالي</span>
            <strong>{{ formatIqd(invoice.totalIqd) }}</strong>
          </div>
          <div v-if="invoice.discountAmountIqd" class="discount-line">
            الخصم: {{ formatIqd(invoice.discountAmountIqd) }}
          </div>
          <div v-if="invoice.status === 'Sold'" class="profit-line">
            الربح: {{ formatIqd(invoice.profitIqd || invoice.totalIqd) }}
          </div>

          <div class="actions">
            <button
              v-if="invoice.status !== 'Sold'"
              class="btn success"
              type="button"
              :disabled="busyId === invoice.id"
              @click="markSold(invoice)"
            >
              <Icon name="mdi:check-bold" />
              تم البيع
            </button>
            <button
              v-if="invoice.status !== 'Sold'"
              class="btn danger"
              type="button"
              :disabled="busyId === invoice.id"
              @click="markNotSold(invoice)"
            >
              <Icon name="mdi:close" />
              لم تبع
            </button>
            <button
              class="btn danger-outline"
              type="button"
              :disabled="busyId === invoice.id"
              @click="deleteInvoice(invoice)"
            >
              <Icon name="mdi:trash-can-outline" />
              حذف
            </button>
            <span v-if="invoice.status === 'Sold'" class="sold-note">تم حفظ الفاتورة وتسجيل الأرباح</span>
          </div>
        </div>
      </article>
    </section>
  </div>
</template>

<script setup lang="ts">
definePageMeta({ layout: 'admin', middleware: ['admin'] })

import { computed, ref } from 'vue'
import { useApi } from '~/composables/useApi'
import { formatIqd } from '~/composables/useMoney'

type InvoiceItem = {
  id: string
  productId?: string
  title: string
  brand?: string
  quantity: number
  unitPriceIqd: number
  lineTotalIqd: number
  stockQuantity?: number
}

type Invoice = {
  id: string
  code: string
  status: string
  rawStatus?: string
  subtotalIqd: number
  discountAmountIqd: number
  totalIqd: number
  totalUsd: number
  profitIqd: number
  createdAt: string
  soldAt?: string
  primaryItemTitle?: string
  customer?: { name?: string; email?: string }
  items: InvoiceItem[]
}

const api = useApi()
const invoices = ref<Invoice[]>([])
const loading = ref(false)
const error = ref('')
const status = ref('all')
const search = ref('')
const busyId = ref('')

const filteredInvoices = computed(() => invoices.value)

const stats = computed(() => {
  const total = invoices.value.length
  const sold = invoices.value.filter(x => x.status === 'Sold').length
  const pending = invoices.value.filter(x => x.status !== 'Sold').length
  const profit = invoices.value.filter(x => x.status === 'Sold').reduce((sum, x) => sum + Number(x.profitIqd || x.totalIqd || 0), 0)
  return { total, sold, pending, profit }
})



function normalizeStatus(s?: string) {
  const v = String(s || '').toLowerCase()
  if (v.includes('sold') || v.includes('paid') || v.includes('completed') || v.includes('succeeded')) return 'Sold'
  if (v.includes('notsold') || v.includes('cancel')) return 'NotSold'
  return 'PendingSale'
}

function invoiceCode(invoice: Invoice) {
  const raw = String(invoice.code || invoice.id || '').replace(/-/g, '')
  return raw ? raw.slice(0, 8).toUpperCase() : '—'
}

function statusLabel(s: string) {
  if (s === 'Sold') return 'تم البيع'
  if (s === 'NotSold') return 'لم تبع'
  return 'قيد التنفيذ'
}

function statusClass(s: string) {
  if (s === 'Sold') return 'badge success'
  if (s === 'NotSold') return 'badge danger'
  return 'badge warning'
}

function formatDate(v?: string) {
  if (!v) return '—'
  try { return new Date(v).toLocaleString('ar-IQ') } catch { return v }
}

function extractErr(e: any) {
  return e?.data?.message || e?.friendlyMessage || e?.message || 'تعذر تنفيذ العملية'
}

async function fetchInvoices() {
  loading.value = true
  error.value = ''
  try {
    const res = await api.get<Invoice[]>('/admin/purchase-invoices', {
      status: status.value,
      search: search.value || undefined,
    })
    invoices.value = (Array.isArray(res) ? res : []).map(x => ({ ...x, status: normalizeStatus(x.status || x.rawStatus) }))
  } catch (e: any) {
    error.value = extractErr(e)
  } finally {
    loading.value = false
  }
}

async function markSold(invoice: Invoice) {
  const ok = confirm(`تأكيد بيع الفاتورة #${invoiceCode(invoice)}؟\nسيتم خصم الكمية من المنتجات وتسجيل الأرباح.`)
  if (!ok) return
  busyId.value = invoice.id
  error.value = ''
  try {
    await api.post(`/admin/purchase-invoices/${invoice.id}/sold`, {})
    await fetchInvoices()
  } catch (e: any) {
    error.value = extractErr(e)
  } finally {
    busyId.value = ''
  }
}

async function markNotSold(invoice: Invoice) {
  const ok = confirm(`هل تريد إلغاء الفاتورة #${invoiceCode(invoice)}؟\nلن يتم خصم أي كمية وسيتم حذف الفاتورة غير المكتملة.`)
  if (!ok) return
  busyId.value = invoice.id
  error.value = ''
  try {
    await api.post(`/admin/purchase-invoices/${invoice.id}/not-sold`, {})
    invoices.value = invoices.value.filter(x => x.id !== invoice.id)
  } catch (e: any) {
    error.value = extractErr(e)
  } finally {
    busyId.value = ''
  }
}

async function deleteInvoice(invoice: Invoice) {
  const ok = confirm(`هل تريد حذف الفاتورة #${invoiceCode(invoice)} نهائياً؟`)
  if (!ok) return
  busyId.value = invoice.id
  error.value = ''
  try {
    await api.del(`/admin/purchase-invoices/${invoice.id}`)
    invoices.value = invoices.value.filter(x => x.id !== invoice.id)
  } catch (e: any) {
    error.value = extractErr(e)
  } finally {
    busyId.value = ''
  }
}

async function deleteAllInvoices() {
  const ok = confirm('هل تريد حذف جميع الفواتير؟\nسيتم حذف الفواتير غير المكتملة، أما الفواتير المباعة فستبقى محفوظة في سجل الأرباح.')
  if (!ok) return
  loading.value = true
  error.value = ''
  try {
    await api.del('/admin/purchase-invoices/all')
    await fetchInvoices()
  } catch (e: any) {
    error.value = extractErr(e)
  } finally {
    loading.value = false
  }
}

fetchInvoices()
</script>

<style scoped>
.purchase-admin-page{ display:grid; gap:18px; }
.admin-hero-card,.rules-card,.toolbar-card,.invoices-card,.stat-card{
  border:1px solid rgba(var(--border),.95);
  background:linear-gradient(180deg, rgba(var(--surface-rgb),.97), rgba(var(--surface-rgb),.89));
  box-shadow:0 24px 70px rgba(5,8,22,.18);
  border-radius:28px;
  padding:18px;
}
.admin-hero-card{ display:flex; justify-content:space-between; align-items:flex-start; gap:16px; }
.hero-actions{ display:flex; flex-wrap:wrap; gap:10px; justify-content:flex-end; }
.eyebrow{ color:rgb(var(--primary)); font-weight:900; font-size:12px; margin-bottom:8px; }
h1{ font-size:clamp(28px,4vw,52px); font-weight:1000; line-height:1.05; }
.hint{ color:rgb(var(--muted)); margin-top:10px; max-width:820px; line-height:1.9; }
.rules-card{ display:grid; grid-template-columns:repeat(3,minmax(0,1fr)); gap:12px; }
.rule{ display:flex; gap:10px; align-items:center; color:rgb(var(--muted)); font-weight:800; }
.rule .iconify{ color:rgb(var(--primary)); font-size:22px; }
.stats-grid{ display:grid; grid-template-columns:repeat(4,minmax(0,1fr)); gap:14px; }
.stat-card{ display:flex; align-items:center; gap:14px; }
.stat-card>.iconify{ font-size:28px; width:54px; height:54px; padding:13px; border-radius:18px; background:rgba(var(--primary),.16); color:rgb(var(--primary)); }
.stat-card.success>.iconify{ background:rgba(16,185,129,.16); color:rgb(16,185,129); }
.stat-card.warning>.iconify{ background:rgba(245,158,11,.16); color:rgb(245,158,11); }
.stat-card.profit>.iconify{ background:rgba(59,130,246,.16); color:rgb(59,130,246); }
.stat-card strong{ display:block; font-size:24px; font-weight:1000; }
.stat-card span{ color:rgb(var(--muted)); font-size:13px; }
.toolbar-card{ display:grid; grid-template-columns:minmax(0,1fr) 220px auto; gap:12px; }
.admin-control{ height:48px; border-radius:16px; border:1px solid rgba(var(--border),.95); background:rgba(var(--surface-2-rgb),.9); color:rgb(var(--fg)); padding:0 14px; outline:none; }
.admin-control:focus{ border-color:rgba(var(--primary),.65); box-shadow:0 0 0 4px rgba(var(--primary),.11); }
.btn{ height:44px; display:inline-flex; align-items:center; justify-content:center; gap:8px; border-radius:16px; padding:0 16px; border:1px solid rgba(var(--border),.95); background:rgba(var(--surface-2-rgb),.95); color:rgb(var(--fg)); font-weight:900; transition:.18s ease; }
.btn:hover{ transform:translateY(-1px); }
.btn.primary{ background:linear-gradient(135deg, rgba(var(--primary),.95), rgba(var(--primary),.7)); border-color:rgba(var(--primary),.55); color:white; }
.btn.success{ border-color:rgba(16,185,129,.45); background:rgba(16,185,129,.14); color:rgb(16,185,129); }
.btn.danger{ border-color:rgba(239,68,68,.45); background:rgba(239,68,68,.14); color:rgb(248,113,113); }
.btn.danger-outline{ border-color:rgba(239,68,68,.34); background:transparent; color:rgb(248,113,113); }
.btn.ghost{ background:rgba(var(--surface-2-rgb),.85); }
.invoices-card{ display:grid; gap:14px; }
.invoice-row{ display:grid; grid-template-columns:minmax(0,1fr) 260px; gap:16px; padding:16px; border:1px solid rgba(var(--border),.88); border-radius:24px; background:rgba(var(--surface-2-rgb),.62); }
.topline{ display:flex; flex-wrap:wrap; gap:8px; align-items:center; margin-bottom:8px; }
.code{ font-family:ui-monospace, SFMono-Regular, Menlo, monospace; color:rgb(var(--primary)); font-weight:900; }
.date{ color:rgb(var(--muted)); font-size:12px; }
.invoice-row h3{ font-size:20px; font-weight:1000; }
.customer{ color:rgb(var(--muted)); display:flex; align-items:center; gap:6px; margin-top:6px; }
.items-list{ display:grid; gap:8px; margin-top:14px; }
.item-line{ display:grid; grid-template-columns:minmax(0,1fr) 120px 54px 110px; gap:10px; align-items:center; padding:10px 12px; border-radius:16px; background:rgba(var(--surface-rgb),.72); border:1px solid rgba(var(--border),.65); }
.item-line span{ font-weight:900; overflow:hidden; text-overflow:ellipsis; white-space:nowrap; }
.item-line small{ color:rgb(var(--muted)); }
.item-line strong{ text-align:end; }
.invoice-side{ display:grid; align-content:start; gap:10px; }
.money-box{ padding:14px; border-radius:18px; background:rgba(var(--surface-rgb),.78); border:1px solid rgba(var(--border),.72); }
.money-box span{ display:block; color:rgb(var(--muted)); font-size:12px; }
.money-box strong{ display:block; font-size:24px; font-weight:1000; margin-top:4px; }
.discount-line,.profit-line,.sold-note{ color:rgb(var(--muted)); font-weight:800; }
.profit-line{ color:rgb(16,185,129); }
.actions{ display:grid; gap:8px; margin-top:4px; }
.badge{ display:inline-flex; align-items:center; justify-content:center; padding:7px 11px; border-radius:999px; font-size:12px; font-weight:1000; }
.badge.success{ color:rgb(16,185,129); border:1px solid rgba(16,185,129,.38); background:rgba(16,185,129,.13); }
.badge.warning{ color:rgb(245,158,11); border:1px solid rgba(245,158,11,.38); background:rgba(245,158,11,.13); }
.badge.danger{ color:rgb(248,113,113); border:1px solid rgba(239,68,68,.38); background:rgba(239,68,68,.13); }
.empty-state,.error-state{ min-height:180px; display:grid; place-items:center; text-align:center; color:rgb(var(--muted)); gap:8px; }
.empty-state .iconify{ font-size:46px; color:rgb(var(--primary)); }
.empty-state strong{ color:rgb(var(--fg)); font-size:22px; }
.error-state{ color:rgb(248,113,113); border:1px solid rgba(239,68,68,.35); border-radius:20px; background:rgba(239,68,68,.08); }
@media (max-width: 960px){
  .rules-card,.stats-grid{ grid-template-columns:1fr 1fr; }
  .toolbar-card{ grid-template-columns:1fr; }
  .invoice-row{ grid-template-columns:1fr; }
  .item-line{ grid-template-columns:1fr auto; }
  .item-line small{ display:none; }
}
@media (max-width: 640px){
  .rules-card,.stats-grid{ grid-template-columns:1fr; }
  .admin-hero-card{ flex-direction:column; }
}
</style>
