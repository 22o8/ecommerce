<script setup lang="ts">
definePageMeta({
  layout: 'admin',
  middleware: ['auth'],
})

const { t } = useI18n()
const brandsStore = useBrandsStore()

const q = ref('')
const status = ref<'all' | 'active' | 'inactive'>('all')
const loading = ref(false)
const error = ref<string | null>(null)

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
    await brandsStore.remove(id)
    await brandsStore.fetchAdmin()
  } catch (e: any) {
    error.value = e?.data?.message || e?.message || 'Delete failed'
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
        <p class="text-sm text-white/60">{{ t('admin.brands.hint') }}</p>
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
          class="rounded-xl border border-white/15 px-4 py-2 text-sm hover:bg-white/5"
          :disabled="loading"
          @click="refresh"
        >
          {{ t('common.refresh') }}
        </button>
      </div>
    </div>

    <div class="card p-4">
      <div class="flex flex-col gap-3 md:flex-row md:items-center">
        <input
          v-model="q"
          class="input flex-1"
          :placeholder="t('admin.brands.searchPlaceholder')"
        />

        <select v-model="status" class="input md:w-56">
          <option value="all">{{ t('common.all') }}</option>
          <option value="active">{{ t('common.active') }}</option>
          <option value="inactive">{{ t('common.inactive') }}</option>
        </select>

        <div class="text-sm text-white/60 md:ml-auto">
          {{ t('common.total') }}: <span class="text-white">{{ items.length }}</span>
        </div>
      </div>
    </div>

    <div v-if="error" class="rounded-xl border border-red-500/30 bg-red-500/10 p-4 text-sm text-red-200">
      {{ error }}
    </div>

    <div v-if="loading" class="text-sm text-white/60">
      {{ t('common.loading') }}...
    </div>

    <div v-else class="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
      <div
        v-for="b in items"
        :key="b.id"
        class="card group p-4"
      >
        <div class="flex items-start gap-3">
          <div class="h-12 w-12 overflow-hidden rounded-2xl bg-white/5 ring-1 ring-white/10">
            <img
              v-if="b.logoUrl"
              :src="b.logoUrl"
              :alt="b.name"
              class="h-full w-full object-cover"
            />
            <div v-else class="flex h-full w-full items-center justify-center text-sm font-bold text-white/80">
              {{ initials(b.name) }}
            </div>
          </div>

          <div class="min-w-0 flex-1">
            <div class="flex items-center justify-between gap-2">
              <div class="min-w-0">
                <div class="truncate text-base font-semibold">{{ b.name }}</div>
                <div class="truncate text-xs text-white/60">/{{ b.slug }}</div>
              </div>

              <span
                class="shrink-0 rounded-full px-2 py-1 text-xs"
                :class="b.isActive ? 'bg-emerald-500/15 text-emerald-200 ring-1 ring-emerald-500/30' : 'bg-white/10 text-white/70 ring-1 ring-white/15'"
              >
                {{ b.isActive ? t('common.active') : t('common.inactive') }}
              </span>
            </div>

            <p v-if="b.description" class="mt-2 line-clamp-2 text-sm text-white/70">
              {{ b.description }}
            </p>
          </div>
        </div>

        <div class="mt-4 flex items-center justify-end gap-2">
          <NuxtLink
            :to="`/admin/brands/${b.id}`"
            class="rounded-xl border border-white/15 px-3 py-2 text-sm hover:bg-white/5"
          >
            {{ t('common.details') }}
          </NuxtLink>
          <button
            class="rounded-xl border border-white/15 px-3 py-2 text-sm text-red-200 hover:bg-red-500/10"
            :disabled="loading"
            @click="removeBrand(b.id)"
          >
            {{ t('common.delete') }}
          </button>
        </div>
      </div>
    </div>

    <div v-if="!loading && items.length === 0" class="card p-10 text-center">
      <div class="text-lg font-semibold">{{ t('admin.brands.emptyTitle') || 'No brands' }}</div>
      <div class="mt-1 text-sm text-white/60">{{ t('admin.brands.emptyHint') || 'Create your first brand to get started.' }}</div>
      <NuxtLink
        to="/admin/brands/new"
        class="mt-6 inline-flex items-center justify-center rounded-xl bg-brand px-4 py-2 text-sm font-semibold text-white hover:opacity-90"
      >
        {{ t('admin.brands.new') }}
      </NuxtLink>
    </div>
  </div>
</template>

<style scoped>
.card {
  @apply rounded-2xl border border-white/10 bg-white/[0.03] backdrop-blur;
}
.input {
  @apply rounded-xl border border-white/10 bg-white/[0.03] px-4 py-2 text-sm outline-none placeholder:text-white/40 focus:border-white/20;
}
</style>
