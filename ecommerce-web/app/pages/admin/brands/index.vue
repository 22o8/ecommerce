<script setup lang="ts">
definePageMeta({
  layout: 'admin',
  middleware: ['admin'],
})

const { t } = useI18n()
const toast = useToast()
const { buildAssetUrl } = useApi()
const brandsStore = useBrandsStore()

const q = ref('')
const status = ref<'all' | 'active' | 'inactive'>('all')
const loading = ref(false)
const error = ref<string | null>(null)
const selectedIds = ref<string[]>([])

const stats = computed(() => {
  const all = brandsStore.items || []
  const active = all.filter((b: any) => !!b.isActive).length
  return {
    total: all.length,
    active,
    inactive: Math.max(0, all.length - active),
  }
})

const items = computed(() => {
  let list = brandsStore.items || []
  if (status.value !== 'all') {
    const isActive = status.value === 'active'
    list = list.filter((b: any) => !!b.isActive === isActive)
  }
  if (q.value.trim()) {
    const s = q.value.trim().toLowerCase()
    list = list.filter((b: any) =>
      String(b.name ?? '').toLowerCase().includes(s) ||
      String(b.slug ?? '').toLowerCase().includes(s) ||
      String(b.description ?? '').toLowerCase().includes(s)
    )
  }
  return list
})

const allSelected = computed(() => {
  const total = items.value.length
  return total > 0 && selectedIds.value.length === total
})

const selectedCount = computed(() => selectedIds.value.length)

function toggleAll() {
  selectedIds.value = allSelected.value ? [] : items.value.map((b: any) => b.id)
}

function toggleOne(id: string) {
  const set = new Set(selectedIds.value)
  if (set.has(id)) set.delete(id)
  else set.add(id)
  selectedIds.value = Array.from(set)
}

function clearSelection() {
  selectedIds.value = []
}

async function refresh() {
  loading.value = true
  error.value = null
  try {
    await brandsStore.fetchAdmin()
  } catch (e: any) {
    error.value = e?.data?.message || e?.message || t('common.requestFailed')
  } finally {
    loading.value = false
  }
}

async function removeBrand(id: string) {
  if (!confirm(t('common.confirmDelete'))) return
  loading.value = true
  error.value = null
  try {
    await brandsStore.deleteBrand(id)
    await brandsStore.fetchAdmin()
    toast.success(t('common.deleted'))
  } catch (e: any) {
    error.value = e?.data?.message || e?.message || t('common.requestFailed')
    toast.error(t('common.requestFailed'))
  } finally {
    loading.value = false
  }
}

async function removeSelected() {
  if (!selectedIds.value.length) return
  if (!confirm(`${t('common.confirmDelete')} (${selectedIds.value.length})`)) return
  loading.value = true
  error.value = null
  try {
    for (const id of selectedIds.value) {
      await brandsStore.deleteBrand(id)
    }
    clearSelection()
    await brandsStore.fetchAdmin()
    toast.success(t('common.deleted'))
  } catch (e: any) {
    error.value = e?.data?.message || e?.message || t('common.requestFailed')
    toast.error(t('common.requestFailed'))
  } finally {
    loading.value = false
  }
}

function initials(name?: string) {
  const s = (name || '').trim()
  if (!s) return 'B'
  const parts = s.split(/\s+/).filter(Boolean)
  const a = parts[0]?.[0] || 'B'
  const b = parts[1]?.[0] || ''
  return (a + b).toUpperCase()
}

onMounted(refresh)
</script>

<template>
  <div class="space-y-6">
    <section class="brands-hero admin-box">
      <div class="flex flex-col gap-4 lg:flex-row lg:items-center lg:justify-between">
        <div>
          <h1 class="text-2xl font-extrabold rtl-text">{{ t('admin.brands.title') }}</h1>
          <p class="mt-1 text-sm admin-muted rtl-text">{{ t('admin.brands.hint') }}</p>
        </div>

        <div class="flex flex-wrap items-center gap-2">
          <NuxtLink to="/admin/brands/new" class="admin-primary btn-touch">
            <span>+</span>
            <span>{{ t('admin.brands.new') }}</span>
          </NuxtLink>
          <button class="admin-ghost btn-touch" :disabled="loading" @click="refresh">
            {{ t('common.refresh') }}
          </button>
        </div>
      </div>

      <div class="mt-5 grid gap-3 sm:grid-cols-3">
        <div class="stat-chip">
          <div class="stat-label">{{ t('common.total') }}</div>
          <div class="stat-value keep-ltr">{{ stats.total }}</div>
        </div>
        <div class="stat-chip success">
          <div class="stat-label">{{ t('common.active') }}</div>
          <div class="stat-value keep-ltr">{{ stats.active }}</div>
        </div>
        <div class="stat-chip muted">
          <div class="stat-label">{{ t('common.inactive') }}</div>
          <div class="stat-value keep-ltr">{{ stats.inactive }}</div>
        </div>
      </div>
    </section>

    <section class="admin-box p-4 sm:p-5">
      <div class="grid gap-3 xl:grid-cols-[1.4fr,.7fr,.45fr] xl:items-center">
        <label class="search-shell">
          <Icon name="mdi:magnify" class="text-lg opacity-70" />
          <input v-model="q" class="search-input" :placeholder="t('admin.brands.searchPlaceholder')" />
        </label>

        <select v-model="status" class="admin-input h-12">
          <option value="all">{{ t('common.all') }}</option>
          <option value="active">{{ t('common.active') }}</option>
          <option value="inactive">{{ t('common.inactive') }}</option>
        </select>

        <button class="admin-ghost h-12" type="button" @click="clearSelection" :disabled="!selectedCount">
          {{ t('common.selected') }}: {{ selectedCount }}
        </button>
      </div>

      <div class="mt-4 flex flex-col gap-3 md:flex-row md:items-center md:justify-between">
        <label class="inline-flex items-center gap-2 text-sm">
          <input type="checkbox" class="h-4 w-4 accent-[rgb(var(--primary))]" :checked="allSelected" @change="toggleAll" />
          <span class="admin-muted">{{ t('common.selectAll') }}</span>
        </label>

        <div class="flex flex-wrap items-center gap-2">
          <button v-if="selectedIds.length" class="admin-danger btn-touch" :disabled="loading" @click="removeSelected">
            {{ t('common.delete') }} ({{ selectedIds.length }})
          </button>
          <div class="count-pill">{{ t('common.total') }}: <span class="font-bold keep-ltr">{{ items.length }}</span></div>
        </div>
      </div>
    </section>

    <div v-if="error" class="rounded-2xl border border-red-500/30 bg-red-500/10 p-4 text-sm text-red-200">
      {{ error }}
    </div>

    <div v-if="loading" class="grid gap-4 sm:grid-cols-2 xl:grid-cols-3">
      <div v-for="n in 6" :key="n" class="brand-skeleton"></div>
    </div>

    <div v-else-if="items.length" class="grid gap-4 sm:grid-cols-2 xl:grid-cols-3">
      <article v-for="b in items" :key="b.id" class="brand-card">
        <div class="flex items-start gap-3">
          <label class="mt-1 inline-flex items-center">
            <input
              type="checkbox"
              class="h-4 w-4 accent-[rgb(var(--primary))]"
              :checked="selectedIds.includes(b.id)"
              @change="toggleOne(b.id)"
            />
          </label>

          <div class="brand-avatar">
            <SmartImage
              v-if="b.logoUrl"
              :src="buildAssetUrl(b.logoUrl)"
              :alt="b.name"
              class="h-full w-full object-cover"
            />
            <div v-else class="flex h-full w-full items-center justify-center text-sm font-black">
              {{ initials(b.name) }}
            </div>
          </div>

          <div class="min-w-0 flex-1">
            <div class="flex items-start justify-between gap-2">
              <div class="min-w-0">
                <div class="truncate text-base font-extrabold">{{ b.name }}</div>
                <div class="truncate text-xs admin-muted keep-ltr">/{{ b.slug }}</div>
              </div>

              <span class="status-pill" :class="b.isActive ? 'active' : 'inactive'">
                {{ b.isActive ? t('common.active') : t('common.inactive') }}
              </span>
            </div>

            <p v-if="b.description" class="mt-3 line-clamp-2 text-sm admin-muted rtl-text">
              {{ b.description }}
            </p>
          </div>
        </div>

        <div class="mt-4 flex flex-wrap items-center gap-2">
          <NuxtLink :to="`/admin/brands/${b.id}`" class="admin-ghost flex-1 justify-center text-center btn-touch">
            {{ t('common.details') }}
          </NuxtLink>
          <button class="admin-danger btn-touch" :disabled="loading" @click="removeBrand(b.id)">
            {{ t('common.delete') }}
          </button>
        </div>
      </article>
    </div>

    <div v-else class="admin-box p-10 text-center">
      <div class="text-lg font-semibold">{{ t('admin.brands.emptyTitle') }}</div>
      <div class="mt-1 text-sm admin-muted">{{ t('admin.brands.emptyHint') }}</div>
      <NuxtLink to="/admin/brands/new" class="admin-primary mx-auto mt-6 inline-flex items-center justify-center btn-touch">
        {{ t('admin.brands.new') }}
      </NuxtLink>
    </div>
  </div>
</template>

<style scoped>
.brands-hero{
  border-radius: 24px;
  background: linear-gradient(135deg, rgba(124,58,237,.10), rgba(255,255,255,.02));
}
.stat-chip{
  border-radius: 18px;
  border: 1px solid rgb(var(--border));
  background: rgba(255,255,255,.03);
  padding: 14px;
}
.stat-chip.success{
  background: rgba(16,185,129,.10);
  border-color: rgba(16,185,129,.18);
}
.stat-chip.muted{
  background: rgba(255,255,255,.02);
}
.stat-label{
  color: rgb(var(--muted));
  font-size: 12px;
  font-weight: 700;
}
.stat-value{
  margin-top: 4px;
  font-size: 28px;
  font-weight: 900;
}
.search-shell{
  display: flex;
  align-items: center;
  gap: 10px;
  height: 48px;
  border-radius: 18px;
  border: 1px solid rgb(var(--border));
  background: rgba(255,255,255,.03);
  padding: 0 14px;
}
.search-input{
  width: 100%;
  background: transparent;
  outline: none;
}
.count-pill{
  display: inline-flex;
  align-items: center;
  gap: 6px;
  min-height: 44px;
  border-radius: 999px;
  border: 1px solid rgb(var(--border));
  background: rgba(255,255,255,.03);
  padding: 0 14px;
  font-size: 13px;
}
.brand-card{
  border-radius: 22px;
  border: 1px solid rgb(var(--border));
  background: linear-gradient(180deg, rgba(255,255,255,.03), rgba(255,255,255,.015));
  padding: 16px;
  box-shadow: 0 10px 24px rgba(0,0,0,.10);
}
.brand-avatar{
  width: 60px;
  height: 60px;
  overflow: hidden;
  border-radius: 18px;
  border: 1px solid rgba(var(--border),0.9);
  background: rgba(var(--surface-2),0.65);
}
.status-pill{
  white-space: nowrap;
  border-radius: 999px;
  padding: 6px 10px;
  font-size: 11px;
  font-weight: 800;
}
.status-pill.active{
  background: rgba(34,197,94,.15);
  color: rgb(74 222 128);
  border: 1px solid rgba(34,197,94,.25);
}
.status-pill.inactive{
  background: rgba(255,255,255,.04);
  color: rgb(var(--muted));
  border: 1px solid rgba(var(--border),0.9);
}
.brand-skeleton{
  height: 172px;
  border-radius: 22px;
  border: 1px solid rgb(var(--border));
  background: linear-gradient(90deg, rgba(255,255,255,.04), rgba(255,255,255,.08), rgba(255,255,255,.04));
  background-size: 200% 100%;
  animation: shimmer 1.25s infinite linear;
}
.btn-touch{
  min-height: 46px;
  padding-inline: 16px;
}
@keyframes shimmer {
  from { background-position: 200% 0; }
  to { background-position: -200% 0; }
}
@media (max-width: 640px){
  .brand-card{ padding: 14px; border-radius: 20px; }
  .brand-avatar{ width: 54px; height: 54px; border-radius: 16px; }
  .stat-value{ font-size: 24px; }
}
</style>
