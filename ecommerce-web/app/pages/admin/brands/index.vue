<script setup lang="ts">
definePageMeta({
  layout: 'admin',
  middleware: ['auth'],
})

const { t } = useI18n()
const toast = useToast()
const { buildAssetUrl } = useApi()
const brandsStore = useBrandsStore()

const q = ref('')
const status = ref<'all' | 'active' | 'inactive'>('all')
const loading = ref(false)
const error = ref<string | null>(null)

// تحديد متعدد (تحديد الكل + حذف جماعي)
const selectedIds = ref<string[]>([])

const allSelected = computed(() => {
  const total = items.value.length
  return total > 0 && selectedIds.value.length === total
})

function toggleAll() {
  if (allSelected.value) {
    selectedIds.value = []
  } else {
    selectedIds.value = items.value.map((b: any) => b.id)
  }
}

function toggleOne(id: string) {
  const set = new Set(selectedIds.value)
  if (set.has(id)) set.delete(id)
  else set.add(id)
  selectedIds.value = Array.from(set)
}

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

async function refresh() {
  loading.value = true
  error.value = null
  try {
    await brandsStore.fetchAdmin()
  } catch (e: any) {
    error.value = e?.data?.message || e?.message || 'Failed to load brands'
  } finally {
    loading.value = false
  }
}

async function removeBrand(id: string) {
  if (!confirm(t('common.confirmDelete') || 'Delete?')) return
  loading.value = true
  error.value = null
  try {
    await brandsStore.deleteBrand(id)
    await brandsStore.fetchAdmin()
    toast.success(t('common.deleted') || 'تم الحذف')
  } catch (e: any) {
    error.value = e?.data?.message || e?.message || 'Delete failed'
    toast.error(t('common.requestFailed') || 'فشل الطلب')
  } finally {
    loading.value = false
  }
}

async function removeSelected() {
  if (!selectedIds.value.length) return
  if (!confirm((t('common.confirmDelete') || 'Delete?') + ` (${selectedIds.value.length})`)) return
  loading.value = true
  error.value = null
  try {
    // حذف جماعي
    for (const id of selectedIds.value) {
      await brandsStore.deleteBrand(id)
    }
    selectedIds.value = []
    await brandsStore.fetchAdmin()
    toast.success(t('common.deleted') || 'تم الحذف')
  } catch (e: any) {
    error.value = e?.data?.message || e?.message || 'Delete failed'
    toast.error(t('common.requestFailed') || 'فشل الطلب')
  } finally {
    loading.value = false
  }
}

onMounted(refresh)

function initials(name?: string) {
  const s = (name || '').trim()
  if (!s) return 'B'
  const parts = s.split(/\s+/).filter(Boolean)
  const a = parts[0]?.[0] || 'B'
  const b = parts[1]?.[0] || ''
  return (a + b).toUpperCase()
}
</script>

<template>
  <div class="space-y-6">
    <div class="flex flex-col gap-3 md:flex-row md:items-center md:justify-between">
      <div>
        <h1 class="text-2xl font-semibold">{{ t('admin.brands.title') }}</h1>
        <p class="text-sm admin-muted">{{ t('admin.brands.hint') }}</p>
      </div>

      <div class="flex items-center gap-2">
        <NuxtLink
          to="/admin/brands/new"
          class="inline-flex items-center gap-2 rounded-xl bg-brand px-4 py-2 text-sm font-semibold text-white hover:opacity-90"
        >
          <span>+</span>
          <span>{{ t('admin.brands.new') }}</span>
        </NuxtLink>
        <button
          class="admin-ghost"
          :disabled="loading"
          @click="refresh"
        >
          {{ t('common.refresh') }}
        </button>
      </div>
    </div>

    <div class="admin-box p-4">
      <div class="flex flex-col gap-3 md:flex-row md:items-center">
        <div class="flex items-center gap-3">
          <label class="inline-flex items-center gap-2 text-sm">
            <input
              type="checkbox"
              class="h-4 w-4 accent-[rgb(var(--primary))]"
              :checked="allSelected"
              @change="toggleAll"
            />
            <span class="admin-muted">{{ t('common.selectAll') || 'تحديد الكل' }}</span>
          </label>

          <button
            v-if="selectedIds.length"
            class="admin-danger"
            :disabled="loading"
            @click="removeSelected"
          >
            {{ (t('common.delete') || 'حذف') }} ({{ selectedIds.length }})
          </button>
        </div>

        <input v-model="q" class="admin-input flex-1" :placeholder="t('admin.brands.searchPlaceholder')" />

        <select v-model="status" class="admin-input md:w-56">
          <option value="all">{{ t('common.all') }}</option>
          <option value="active">{{ t('common.active') }}</option>
          <option value="inactive">{{ t('common.inactive') }}</option>
        </select>

        <div class="text-sm admin-muted md:ml-auto">
          {{ t('common.total') }}: <span class="font-semibold">{{ items.length }}</span>
        </div>
      </div>
    </div>

    <div v-if="error" class="rounded-xl border border-red-500/30 bg-red-500/10 p-4 text-sm text-red-200">
      {{ error }}
    </div>

    <div v-if="loading" class="text-sm admin-muted">
      {{ t('common.loading') }}...
    </div>

    <div v-else class="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
      <div
        v-for="b in items"
        :key="b.id"
        class="admin-box group p-4 relative"
      >
        <label class="absolute left-4 top-4 inline-flex items-center gap-2">
          <input
            type="checkbox"
            class="h-4 w-4 accent-[rgb(var(--primary))]"
            :checked="selectedIds.includes(b.id)"
            @change="toggleOne(b.id)"
          />
        </label>

        <div class="flex items-start gap-3">
          <div class="h-12 w-12 overflow-hidden rounded-2xl bg-[rgba(var(--surface-2),0.6)] ring-1 ring-[rgba(var(--border),0.9)]">
            <SmartImage
              v-if="b.logoUrl"
              :src="buildAssetUrl(b.logoUrl)"
              :alt="b.name"
              class="h-full w-full object-cover"
            />
            <div v-else class="flex h-full w-full items-center justify-center text-sm font-bold">
              {{ initials(b.name) }}
            </div>
          </div>

          <div class="min-w-0 flex-1">
            <div class="flex items-center justify-between gap-2">
              <div class="min-w-0">
                <div class="truncate text-base font-semibold">{{ b.name }}</div>
                <div class="truncate text-xs admin-muted">/{{ b.slug }}</div>
              </div>

              <span
                class="shrink-0 rounded-full px-2 py-1 text-xs"
                :class="b.isActive ? 'bg-emerald-500/15 text-emerald-600 ring-1 ring-emerald-500/30' : 'bg-[rgba(var(--surface-2),0.7)] admin-muted ring-1 ring-[rgba(var(--border),0.9)]'"
              >
                {{ b.isActive ? t('common.active') : t('common.inactive') }}
              </span>
            </div>

            <p v-if="b.description" class="mt-2 line-clamp-2 text-sm admin-muted">
              {{ b.description }}
            </p>
          </div>
        </div>

        <div class="mt-4 flex items-center justify-end gap-2">
          <NuxtLink
            :to="`/admin/brands/${b.id}`"
            class="admin-ghost"
          >
            {{ t('common.details') }}
          </NuxtLink>
          <button
            class="admin-danger"
            :disabled="loading"
            @click="removeBrand(b.id)"
          >
            {{ t('common.delete') }}
          </button>
        </div>
      </div>
    </div>

    <div v-if="!loading && items.length === 0" class="admin-box p-10 text-center">
      <div class="text-lg font-semibold">{{ t('admin.brands.emptyTitle') || 'No brands' }}</div>
      <div class="mt-1 text-sm admin-muted">{{ t('admin.brands.emptyHint') || 'Create your first brand to get started.' }}</div>
      <NuxtLink
        to="/admin/brands/new"
        class="mt-6 inline-flex items-center justify-center rounded-xl bg-brand px-4 py-2 text-sm font-semibold text-white hover:opacity-90"
      >
        {{ t('admin.brands.new') }}
      </NuxtLink>
    </div>
  </div>
</template>

