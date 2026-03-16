<template>
  <div class="grid gap-6 coupons-admin-page">
    <div class="coupons-hero rounded-[30px] border border-app bg-surface p-5 shadow-sm">
      <div class="flex flex-col gap-5 xl:flex-row xl:items-center xl:justify-between">
        <div>
      <h1 class="text-3xl font-black rtl-text text-[rgb(var(--text))]">
        {{ t('admin.couponsLabel') }}
      </h1>
      <p class="mt-1 text-sm text-muted rtl-text">
        {{ t('admin.couponsHeroHint') }}
      </p>
        </div>

        <div class="grid gap-3 sm:grid-cols-3">
          <div class="coupon-stat-card">
            <div class="coupon-stat-card__label rtl-text">{{ t('admin.couponsLabel') }}</div>
            <div class="coupon-stat-card__value keep-ltr">{{ items.length }}</div>
          </div>
          <div class="coupon-stat-card">
            <div class="coupon-stat-card__label rtl-text">{{ t('admin.active') }}</div>
            <div class="coupon-stat-card__value keep-ltr">{{ activeCount }}</div>
          </div>
          <div class="coupon-stat-card">
            <div class="coupon-stat-card__label rtl-text">{{ totalUsesLabel }}</div>
            <div class="coupon-stat-card__value keep-ltr">{{ totalUses }}</div>
          </div>
        </div>
      </div>
    </div>

    <div class="grid gap-5 xl:grid-cols-[minmax(0,540px)_1fr]">
      <div class="panel-shell rounded-[30px] border border-app bg-surface p-5 shadow-sm">
        <div class="mb-4 rounded-2xl border border-app bg-surface-2 p-4">
          <div class="text-sm font-bold rtl-text">{{ t('admin.couponRules') }}</div>
          <div class="mt-2 grid gap-2 text-sm text-muted rtl-text">
            <div>• {{ t('admin.couponRule1') }}</div>
            <div>• {{ t('admin.couponRule2') }}</div>
            <div>• {{ t('admin.couponRule3') }}</div>
            <div>• {{ t('admin.couponRule4') }}</div>
          </div>
        </div>

        <div class="grid gap-4">
          <UiInput
            v-model="form.code"
            :label="t('admin.couponCode')"
            :hint="t('admin.couponCodeHint')"
            placeholder="SAVE10"
          />

          <UiInput
            v-model="form.title"
            :label="t('admin.couponTitle')"
            :hint="t('admin.couponTitleHint')"
            :placeholder="t('admin.couponTitle')"
          />

          <div class="grid gap-4 md:grid-cols-2">
            <UiInput
              v-model.number="form.discountPercent"
              type="number"
              min="0"
              max="100"
              :label="t('admin.discountPercent')"
              :hint="t('admin.discountPercentHint')"
              placeholder="مثال: 10"
            />

            <UiInput
              v-model.number="form.maxUses"
              type="number"
              min="0"
              :label="t('admin.maxUses')"
              :hint="t('admin.maxUsesHint')"
              placeholder="مثال: 100"
            />
          </div>

          <div class="grid gap-4 md:grid-cols-2">
            <div class="rounded-2xl border border-app bg-surface-2 p-4">
              <label class="mb-2 block text-sm font-bold rtl-text text-[rgb(var(--text))]">
                {{ t('admin.activationStart') }}
              </label>
              <input
                v-model="form.startsAtUtc"
                type="datetime-local"
                class="w-full rounded-2xl border border-app bg-surface px-4 py-3 text-sm text-[rgb(var(--text))] outline-none transition focus:ring-0"
              />
              <p class="mt-2 text-xs text-muted rtl-text">
                {{ t('admin.activationStartHint') }}
              </p>
            </div>

            <div class="rounded-2xl border border-app bg-surface-2 p-4">
              <label class="mb-2 block text-sm font-bold rtl-text text-[rgb(var(--text))]">
                {{ t('admin.activationEnd') }}
              </label>
              <input
                v-model="form.endsAtUtc"
                type="datetime-local"
                class="w-full rounded-2xl border border-app bg-surface px-4 py-3 text-sm text-[rgb(var(--text))] outline-none transition focus:ring-0"
              />
              <p class="mt-2 text-xs text-muted rtl-text">
                {{ t('admin.activationEndHint') }}
              </p>
            </div>
          </div>

          <label
            class="flex items-center gap-2 rounded-2xl border border-app bg-surface-2 px-4 py-3 text-sm text-[rgb(var(--text))]"
          >
            <input v-model="form.isActive" type="checkbox" />
            <span class="rtl-text font-semibold">{{ t('admin.couponActiveNow') }}</span>
          </label>

          <div class="rounded-2xl border border-app bg-surface-2 p-4 text-sm text-muted rtl-text">
            {{ t('admin.couponDeviceLimit') }}
          </div>

          <div class="flex flex-wrap gap-3">
            <UiButton @click="save">
              {{ editingId ? t('admin.saveChanges') : t('admin.createCoupon') }}
            </UiButton>
            <UiButton variant="ghost" @click="resetForm">{{ t('common.cancel') }}</UiButton>
          </div>
        </div>
      </div>

      <div class="panel-shell rounded-[30px] border border-app bg-surface p-5 shadow-sm">
        <div class="mb-4 flex flex-col gap-3 md:flex-row md:items-center md:justify-between">
          <div>
            <div class="text-sm font-bold rtl-text text-[rgb(var(--text))]">{{ t('admin.couponList') }}</div>
            <div class="mt-1 text-sm text-muted rtl-text">
              {{ filteredItems.length }} {{ t('admin.couponCountFrom') }} {{ items.length }} {{ t('admin.couponsLabel') }}
            </div>
          </div>

          <input
            v-model="search"
            class="w-full rounded-2xl border border-app bg-surface px-4 py-3 text-sm text-[rgb(var(--text))] outline-none transition focus:ring-0 md:w-[280px]"
            :placeholder="t('admin.searchByCodeOrTitle')"
          />
        </div>

        <div
          v-if="!filteredItems.length"
          class="rounded-3xl border border-app bg-surface-2 p-8 text-center"
        >
          <div class="text-lg font-bold rtl-text text-[rgb(var(--text))]">
            {{ t('common.noResults') || 'لا توجد نتائج' }}
          </div>
          <div class="mt-2 text-sm text-muted rtl-text">
            {{ t('common.tryDifferentSearch') || 'جرّب تغيير البحث أو أنشئ كوبون جديد من النموذج.' }}
          </div>
        </div>

        <div v-else class="grid gap-3">
          <div
            v-for="item in filteredItems"
            :key="item.id"
            class="coupon-card rounded-3xl border border-app bg-surface-2 p-4 transition hover:shadow-sm"
          >
            <div class="flex flex-col gap-4 md:flex-row md:items-start md:justify-between">
              <div class="min-w-0 flex-1">
                <div class="flex flex-wrap items-center gap-2">
                  <div class="font-black keep-ltr text-lg text-[rgb(var(--text))]">
                    {{ item.code }}
                  </div>

                  <span
                    class="rounded-full px-3 py-1 text-xs font-bold"
                    :class="
                      item.isActive
                        ? 'bg-emerald-500/15 text-emerald-600 dark:text-emerald-300'
                        : 'bg-red-500/15 text-red-600 dark:text-red-300'
                    "
                  >
                    {{ item.isActive ? t('admin.active') : t('admin.disabled') }}
                  </span>
                </div>

                <div class="mt-1 rtl-text text-base font-bold text-[rgb(var(--text))]">
                  {{ item.title }}
                </div>

                <div class="mt-3 grid gap-2 sm:grid-cols-2 xl:grid-cols-3">
                  <div class="rounded-2xl border border-app bg-surface px-3 py-2 text-sm rtl-text">
                    <div class="text-muted">{{ t('admin.discountPercent') }}</div>
                    <div class="mt-1 font-bold text-[rgb(var(--text))]">
                      {{ item.discountPercent }}%
                    </div>
                  </div>

                  <div class="rounded-2xl border border-app bg-surface px-3 py-2 text-sm rtl-text">
                    <div class="text-muted">{{ t('admin.maxUses') }}</div>
                    <div class="mt-1 font-bold text-[rgb(var(--text))]">
                      {{ item.maxUses ?? t('common.unlimited') }}
                    </div>
                  </div>

                  <div class="rounded-2xl border border-app bg-surface px-3 py-2 text-sm rtl-text">
                    <div class="text-muted">{{ totalUsesLabel }}</div>
                    <div class="mt-1 font-bold text-[rgb(var(--text))]">
                      {{ item.usedCount ?? 0 }}
                    </div>
                  </div>
                </div>

                <div class="mt-3 grid gap-2 sm:grid-cols-2 text-sm">
                  <div class="rounded-2xl border border-app bg-surface px-3 py-2 rtl-text">
                    <div class="text-muted">{{ t('admin.activationStart') }}</div>
                    <div class="mt-1 font-semibold text-[rgb(var(--text))]">
                      {{ item.startsAtUtc ? fmtDate(item.startsAtUtc) : (t('common.immediate') || 'مباشر') }}
                    </div>
                  </div>

                  <div class="rounded-2xl border border-app bg-surface px-3 py-2 rtl-text">
                    <div class="text-muted">{{ t('admin.activationEnd') }}</div>
                    <div class="mt-1 font-semibold text-[rgb(var(--text))]">
                      {{ item.endsAtUtc ? fmtDate(item.endsAtUtc) : (t('common.noEnd') || 'بدون نهاية') }}
                    </div>
                  </div>
                </div>
              </div>

              <div class="flex shrink-0 gap-2 self-start">
                <UiButton size="sm" variant="ghost" @click="editItem(item)">{{ t('common.edit') }}</UiButton>
                <UiButton size="sm" variant="ghost" @click="removeItem(item.id)">{{ t('common.delete') }}</UiButton>
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

const { t } = useI18n()
const api = useAdminApi()

const items = ref<any[]>([])
const editingId = ref<string>('')
const search = ref('')


const totalUsesLabel = computed(() => {
  const translated = t('admin.totalUses')
  if (translated && translated !== 'admin.totalUses') return translated
  const fallback = t('admin.usageCount')
  if (fallback && fallback !== 'admin.usageCount') return fallback
  return 'إجمالي الاستخدامات'
})


const form = reactive({
  code: '',
  title: '',
  discountPercent: 0,
  fixedDiscountIqd: 0,
  minimumOrderIqd: 0,
  maxUses: 0 as any,
  isActive: true,
  startsAtUtc: '',
  endsAtUtc: ''
})

const activeCount = computed(() => items.value.filter((x: any) => x?.isActive).length)
const totalUses = computed(() => items.value.reduce((sum: number, x: any) => sum + Number(x?.usedCount ?? 0), 0))

const filteredItems = computed(() => {
  const s = search.value.trim().toLowerCase()
  if (!s) return items.value

  return items.value.filter(
    (x: any) =>
      String(x.code || '')
        .toLowerCase()
        .includes(s) ||
      String(x.title || '')
        .toLowerCase()
        .includes(s)
  )
})

async function load() {
  items.value = (await api.get('/admin/coupons')) as any[]
}

function resetForm() {
  editingId.value = ''
  Object.assign(form, {
    code: '',
    title: '',
    discountPercent: 0,
    fixedDiscountIqd: 0,
    minimumOrderIqd: 0,
    maxUses: 0,
    isActive: true,
    startsAtUtc: '',
    endsAtUtc: ''
  })
}

function toIsoOrNull(v: string) {
  return v ? new Date(v).toISOString() : null
}

function fromIso(v?: string) {
  if (!v) return ''
  const d = new Date(v)
  const pad = (n: number) => String(n).padStart(2, '0')
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}`
}

async function save() {
  const payload = {
    ...form,
    fixedDiscountIqd: 0,
    minimumOrderIqd: 0,
    maxUses: Number(form.maxUses) || null,
    startsAtUtc: toIsoOrNull(form.startsAtUtc),
    endsAtUtc: toIsoOrNull(form.endsAtUtc)
  }

  if (editingId.value) {
    await api.put(`/admin/coupons/${editingId.value}`, payload)
  } else {
    await api.post('/admin/coupons', payload)
  }

  await load()
  resetForm()
}

function editItem(item: any) {
  editingId.value = item.id
  Object.assign(form, {
    code: item.code,
    title: item.title,
    discountPercent: item.discountPercent,
    fixedDiscountIqd: 0,
    minimumOrderIqd: 0,
    maxUses: item.maxUses || 0,
    isActive: item.isActive,
    startsAtUtc: fromIso(item.startsAtUtc),
    endsAtUtc: fromIso(item.endsAtUtc)
  })
}

async function removeItem(id: string) {
  await api.del(`/admin/coupons/${id}`)
  await load()
  if (editingId.value === id) resetForm()
}

function fmtDate(v?: string) {
  return v ? new Date(v).toLocaleString('ar-IQ') : ''
}

onMounted(load)
</script>
<style scoped>
.coupons-admin-page{ --coupon-shadow: 0 26px 84px rgba(12,16,32,.16); }
.coupons-hero,.panel-shell{ position:relative; overflow:hidden; border-radius:30px; border:1px solid rgba(var(--border), .95); background:linear-gradient(180deg, rgba(var(--surface-rgb), .97), rgba(var(--surface-rgb), .90)); box-shadow:var(--coupon-shadow); }
.coupons-hero::before,.panel-shell::before{ content:''; position:absolute; inset:0; pointer-events:none; background-image: linear-gradient(rgba(var(--border), .18) 1px, transparent 1px), linear-gradient(90deg, rgba(var(--border), .14) 1px, transparent 1px); background-size: 100% 84px, 84px 100%; mask-image: linear-gradient(180deg, rgba(0,0,0,.18), transparent 18%, transparent 82%, rgba(0,0,0,.14)); opacity:.22; }
.coupons-hero > *, .panel-shell > *{ position:relative; z-index:1; }
.coupons-hero::after{ content:''; position:absolute; inset:auto auto -100px -40px; width:260px; height:260px; border-radius:999px; background:radial-gradient(circle, rgba(var(--primary), .16), transparent 68%); pointer-events:none; }
.coupon-stat-card{ border-radius:22px; border:1px solid rgba(var(--border), .92); background:rgba(var(--surface-2-rgb), .88); padding:14px 16px; min-width:120px; }
.coupon-stat-card__label{ font-size:12px; color:rgb(var(--muted)); font-weight:800; }
.coupon-stat-card__value{ font-size:2rem; line-height:1; font-weight:1000; margin-top:8px; }
@media (max-width:768px){ .coupons-hero,.panel-shell{ border-radius:22px !important; } .coupons-hero::before,.panel-shell::before{ display:none; } }
</style>
