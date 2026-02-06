<template>
  <div class="space-y-4">
    <div class="admin-box">
      <div class="flex flex-col md:flex-row md:items-center md:justify-between gap-3">
        <div>
          <div class="text-xl font-extrabold">{{ t('admin.brands.title') }}</div>
          <div class="text-sm admin-muted rtl-text">{{ t('admin.brandsHint') }}</div>
        </div>

        <div class="flex gap-2 flex-wrap">
          <NuxtLink to="/admin/brands/new" class="admin-primary">+ {{ t('admin.newBrand') }}</NuxtLink>
          <button class="admin-ghost" type="button" @click="fetchList">{{ t('common.refresh') }}</button>
        </div>
      </div>

      <div class="mt-4 grid gap-2 md:grid-cols-3">
        <input v-model="q" class="admin-input" :placeholder="t('admin.searchBrands')" />
        <select v-model="status" class="admin-input">
          <option value="">{{ t('admin.all') }}</option>
          <option value="active">{{ t('admin.active') }}</option>
          <option value="disabled">{{ t('admin.disabled') }}</option>
        </select>
        <button class="admin-ghost" type="button" @click="applyFilters">{{ t('admin.search') }}</button>
      </div>
    </div>

    <div class="admin-box overflow-hidden">
      <div v-if="loading" class="p-4 admin-muted rtl-text">{{ t('common.loading') }}</div>

      <div v-else-if="filtered.length === 0" class="p-6 text-center">
        <div class="text-lg font-extrabold rtl-text">{{ t('admin.noBrands') }}</div>
        <div class="admin-muted mt-1 rtl-text">{{ t('admin.noBrandsHint') }}</div>
        <NuxtLink to="/admin/brands/new" class="admin-primary inline-flex mt-4">+ {{ t('admin.addFirstBrand') }}</NuxtLink>
      </div>

      <div v-else class="admin-table">
        <div class="admin-tr brands-tr admin-th">
          <div class="rtl-text">{{ t('admin.brand') }}</div>
          <div class="rtl-text">{{ t('admin.status') }}</div>
          <div class="text-right rtl-text">{{ t('common.actions') }}</div>
        </div>

        <div v-for="b in filtered" :key="b.id" class="admin-tr brands-tr">
          <div class="flex items-center gap-3 min-w-0">
            <div class="thumb">
              <div class="thumb-inner overflow-hidden">
                <SmartImage v-if="b.logoUrl" :src="buildAssetUrl(b.logoUrl)" :alt="b.name" class="w-full h-full object-cover" />
                <span v-else class="text-xs admin-muted">LOGO</span>
              </div>
            </div>

            <div class="min-w-0">
              <div class="font-extrabold truncate">{{ b.name }}</div>
              <div class="text-xs admin-muted truncate">/{{ b.slug }}</div>
            </div>
          </div>

          <div>
            <span :class="b.isActive ? 'badge-on' : 'badge-off'">
              {{ b.isActive ? t('admin.active') : t('admin.disabled') }}
            </span>
          </div>

          <div class="flex justify-end gap-2">
            <NuxtLink class="admin-pill" :to="`/admin/brands/${b.id}`">{{ t('common.details') }}</NuxtLink>
            <button class="admin-btn-danger" type="button" @click="remove(b.id)" :disabled="pending">
              {{ t('admin.delete') }}
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import SmartImage from '~/components/SmartImage.vue'

const { t } = useI18n()
const { buildAssetUrl } = useApi()
const brands = useBrandsStore()

const q = ref('')
const status = ref('')
const pending = ref(false)

const loading = computed(() => brands.loading)

await useAsyncData('admin-brands', async () => {
  await brands.fetchAdmin()
  return true
})

const filtered = ref<any[]>([])

const applyFilters = () => {
  const qq = q.value.trim().toLowerCase()
  let list = [...(brands.items || [])]
  if (status.value === 'active') list = list.filter(b => b.isActive)
  if (status.value === 'disabled') list = list.filter(b => !b.isActive)
  if (qq) {
    list = list.filter(b =>
      String(b.name || '').toLowerCase().includes(qq) ||
      String(b.slug || '').toLowerCase().includes(qq)
    )
  }
  filtered.value = list
}

watch([() => brands.items, q, status], () => applyFilters(), { deep: true, immediate: true })

const fetchList = async () => {
  await brands.fetchAdmin()
  applyFilters()
}

const remove = async (id: string) => {
  if (!confirm(t('admin.confirmDelete'))) return
  pending.value = true
  try {
    await brands.deleteBrand(id)
    await fetchList()
  } finally {
    pending.value = false
  }
}
</script>
