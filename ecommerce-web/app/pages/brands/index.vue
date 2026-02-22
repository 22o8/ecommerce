<template>
  <div class="container mx-auto px-4 py-8">
    <div class="flex flex-col sm:flex-row sm:items-end sm:justify-between gap-4">
      <div>
        <h1 class="text-3xl sm:text-4xl font-extrabold rtl-text">{{ t('home.brands') }}</h1>
        <p class="mt-2 text-muted rtl-text">{{ t('home.brandsSubtitle') }}</p>
      </div>

      <div class="control-box control-box-strong glass-panel glow-border rounded-2xl p-2 w-full sm:w-[360px]">
        <div class="relative">
          <input
            v-model="q"
            :placeholder="t('brandPage.search')"
            class="input"
          />
          <button
            v-if="q"
            class="absolute left-3 top-1/2 -translate-y-1/2 icon-btn"
            @click="q = ''"
            aria-label="clear"
          >
            ✕
          </button>
        </div>
      </div>
    </div>

    <div class="mt-6">
      <div v-if="pending" class="card-soft p-6 text-muted rtl-text">{{ t('common.loading') }}</div>

      <div v-else-if="filtered.length === 0" class="card-soft p-10 text-center text-muted rtl-text">
        {{ t('home.noBrandsFound') }}
      </div>

      <div v-else class="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-5 gap-4">
        <NuxtLink
          v-for="b in filtered"
          :key="b.id || b.slug"
          :to="`/brands/${b.slug}`"
          class="group card-soft glass-panel glow-border rounded-2xl p-4 hover:-translate-y-0.5 transition duration-300 hover:shadow-lg"
        >
          <div class="flex items-start gap-3">
            <div
              class="w-14 h-14 rounded-2xl border border-app bg-surface-2 overflow-hidden grid place-items-center flex-shrink-0"
            >
              <SmartImage
                v-if="b.logoUrl"
                :src="buildAssetUrl(b.logoUrl)"
                :alt="b.name"
                class="w-full h-full object-cover"
              />
              <div v-else class="text-xs text-muted">Logo</div>
            </div>

            <div class="min-w-0">
              <div class="font-extrabold line-clamp-1 rtl-text group-hover:opacity-95">{{ b.name }}</div>
              <div class="mt-1 text-xs text-muted line-clamp-2 rtl-text">
                {{ b.description || t('brands.noDescription') }}
              </div>
            </div>
          </div>
        </NuxtLink>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import SmartImage from '~/components/SmartImage.vue'

const { t } = useI18n()
const brandsStore = useBrandsStore()
const { buildAssetUrl } = useApi()

const q = ref('')

const { pending } = await useAsyncData('brands_page', async () => {
  await brandsStore.fetchPublic()
  return true
})

const list = computed(() => brandsStore.publicItems || brandsStore.items || [])

const filtered = computed(() => {
  const s = q.value.trim().toLowerCase()
  if (!s) return list.value
  return (list.value || []).filter((b: any) => {
    const name = String(b?.name || '').toLowerCase()
    const slug = String(b?.slug || '').toLowerCase()
    const desc = String(b?.description || '').toLowerCase()
    return name.includes(s) || slug.includes(s) || desc.includes(s)
  })
})
</script>
