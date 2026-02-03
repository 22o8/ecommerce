<template>
  <div class="grid gap-6">
    <!-- العنوان + الأدوات: متوازن على الهاتف والحاسوب -->
    <div class="flex flex-col md:flex-row md:items-end md:justify-between gap-4">
      <div>
        <h1 class="text-2xl md:text-3xl font-black rtl-text">{{ t('productsPage.title') }}</h1>

        <!-- Brand chips (Desktop فقط) -->
        <div class="mt-3 hidden md:block">
          <div class="text-xs font-bold text-muted rtl-text mb-2">{{ t('productsPage.brandsTitle') }}</div>

          <!-- Chips: سكرول على الهاتف / التفاف على الشاشات الأكبر -->
          <div class="-mx-1">
            <div
              class="px-1 flex gap-2 overflow-x-auto no-scrollbar pb-1 md:flex-wrap md:overflow-visible"
              style="-webkit-overflow-scrolling: touch"
            >
              <button
                type="button"
                class="shrink-0 rounded-2xl border border-app bg-surface-2 px-3 py-2 text-xs font-bold whitespace-nowrap"
                :class="selectedBrand ? 'opacity-70 hover:opacity-100' : 'ring-2 ring-[rgb(var(--primary))]'"
                @click="clearBrand"
              >
                {{ t('productsPage.allBrands') }}
              </button>

              <button
                v-for="b in brands"
                :key="b.key"
                type="button"
                class="shrink-0 rounded-2xl border border-app bg-surface px-3 py-2 text-xs font-bold whitespace-nowrap hover:bg-surface-2"
                :class="selectedBrand === b.key ? 'ring-2 ring-[rgb(var(--primary))]' : ''"
                @click="selectBrand(b)"
                :title="b.label"
              >
                {{ b.label }}
              </button>
            </div>
          </div>
        </div>
      </div>

      <div class="w-full md:w-auto flex flex-col sm:flex-row sm:flex-wrap gap-2">
        <div class="flex items-center gap-2 rounded-2xl border border-app bg-surface px-3 py-2 w-full sm:w-auto">
          <Icon name="mdi:magnify" class="text-lg opacity-70" />
          <input
            v-model="q"
            class="bg-transparent outline-none text-sm rtl-text w-full sm:w-56"
            :placeholder="t('productsPage.searchPlaceholder')"
            @keydown.enter="apply()"
          />
        </div>

        <!-- Sort (Desktop/Tablet) -->
        <select v-model="sort" class="hidden sm:block rounded-2xl border border-app bg-surface px-4 py-2 text-sm w-full sm:w-auto">
          <option value="new">{{ t('productsPage.sort.new') }}</option>
          <option value="priceAsc">{{ t('productsPage.sort.priceAsc') }}</option>
          <option value="priceDesc">{{ t('productsPage.sort.priceDesc') }}</option>
        </select>

        <!-- Mobile menus: Sort + Brands (مرتب وسلس) -->
        <div class="sm:hidden grid grid-cols-1 gap-2">
          <!-- Sort menu button -->
          <details ref="sortMenu" class="relative">
            <summary class="list-none cursor-pointer">
              <div class="rounded-2xl border border-app bg-surface px-4 py-2 text-sm w-full flex items-center justify-between">
                <span class="rtl-text">{{ sortLabel }}</span>
                <Icon name="mdi:chevron-down" class="text-lg opacity-70" />
              </div>
            </summary>
            <div class="absolute z-40 mt-2 w-full rounded-2xl border border-app bg-surface shadow-soft overflow-hidden">
              <button
                v-for="o in sortOptions"
                :key="o.value"
                type="button"
                class="w-full px-4 py-3 text-sm rtl-text flex items-center justify-between hover:bg-surface-2"
                @click="setSort(o.value)"
              >
                <span>{{ o.label }}</span>
                <Icon v-if="sort === o.value" name="mdi:check" class="text-lg" />
              </button>
            </div>
          </details>

          <!-- Brands menu button -->
          <details ref="brandMenu" class="relative">
            <summary class="list-none cursor-pointer">
              <div class="rounded-2xl border border-app bg-surface px-4 py-2 text-sm w-full flex items-center justify-between">
                <span class="rtl-text">{{ selectedBrandLabel }}</span>
                <Icon name="mdi:chevron-down" class="text-lg opacity-70" />
              </div>
            </summary>
            <div class="absolute z-40 mt-2 w-full rounded-2xl border border-app bg-surface shadow-soft overflow-hidden">
              <div class="px-4 py-3 text-xs font-bold text-muted rtl-text border-b border-app">{{ t('productsPage.brandsTitle') }}</div>
              <div class="max-h-[55vh] overflow-auto p-2 grid grid-cols-2 gap-2">
                <button
                  type="button"
                  class="rounded-2xl border border-app bg-surface-2 px-3 py-2 text-xs font-bold rtl-text"
                  :class="!selectedBrand ? 'ring-2 ring-[rgb(var(--primary))]' : 'opacity-80'"
                  @click="clearBrand()"
                >
                  {{ t('productsPage.allBrands') }}
                </button>
                <button
                  v-for="b in brands"
                  :key="b.key"
                  type="button"
                  class="rounded-2xl border border-app bg-surface px-3 py-2 text-xs font-bold rtl-text hover:bg-surface-2"
                  :class="selectedBrand === b.key ? 'ring-2 ring-[rgb(var(--primary))]' : ''"
                  @click="selectBrand(b)"
                >
                  {{ b.label }}
                </button>
              </div>
            </div>
          </details>
        </div>

        <UiButton variant="secondary" class="w-full sm:w-auto" @click="apply">
          <Icon name="mdi:filter-outline" class="text-lg" />
          <span class="rtl-text">{{ t('productsPage.search') }}</span>
        </UiButton>
      </div>
    </div>

    <div v-if="loading" class="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
      <div v-for="i in 9" :key="i" class="card-soft p-4 grid gap-3">
        <div class="skeleton h-44" />
        <div class="skeleton h-5 w-3/4" />
        <div class="skeleton h-4 w-1/2" />
        <div class="skeleton h-10" />
      </div>
    </div>

    <div v-else-if="items.length === 0" class="card-soft p-10 text-center">
      <Icon name="mdi:database-off-outline" class="text-4xl opacity-70 mx-auto" />
      <div class="mt-3 font-bold rtl-text">{{ t('productsPage.emptyTitle') }}</div>
      <div class="mt-1 text-sm text-muted rtl-text">{{ t('productsPage.emptyDesc') }}</div>
    </div>

    <div v-else>
      <MasonryGrid :items="items" :columns="masonryCols" :gap="16" v-slot="{ item }">
        <ProductCard :p="item" />
      </MasonryGrid>
    </div>

    <ProductQuickPreviewModal />

    <div class="flex items-center justify-between gap-3 pt-2">
      <UiButton variant="ghost" @click="prev" :disabled="page<=1">
        <Icon name="mdi:chevron-left" class="text-xl keep-ltr" />
        <span class="rtl-text">{{ t('productsPage.prev') }}</span>
      </UiButton>

      <UiBadge>
        <span class="rtl-text">{{ t('productsPage.page') }}</span>
        <span class="keep-ltr"> {{ page }} </span>
      </UiBadge>

      <UiButton variant="ghost" @click="next" :disabled="items.length < pageSize">
        <span class="rtl-text">{{ t('productsPage.next') }}</span>
        <Icon name="mdi:chevron-right" class="text-xl keep-ltr" />
      </UiButton>
    </div>
  </div>
</template>

<script setup lang="ts">
import UiButton from '~/components/ui/UiButton.vue'
import UiBadge from '~/components/ui/UiBadge.vue'
import ProductCard from '~/components/ProductCard.vue'
import MasonryGrid from '~/components/MasonryGrid.vue'
import ProductQuickPreviewModal from '~/components/ProductQuickPreviewModal.vue'
import { useProductsStore } from '~/stores/products'

const { t } = useI18n()
const route = useRoute()
const router = useRouter()
const products = useProductsStore()
const qp = useQuickPreview()
const api = useApi()

const pageSize = 12
const page = computed(() => Number(route.query.page || 1))
// v-model needs writable refs; computed() without setter throws and breaks rendering.
const q = ref(String(route.query.q || ''))
const sort = ref(String(route.query.sort || 'new'))

// تفاصيل قائمة الفرز/البراندات (مهم للموبايل حتى نسدّ أي مشاكل بالسلوك)
const sortMenu = ref<HTMLDetailsElement | null>(null)
const brandMenu = ref<HTMLDetailsElement | null>(null)

// ✅ خيارات العلامات التجارية (مرتبة وسلسة)
// مهم: نخليها Objects لأن الـ template يستعمل b.key و b.label
const defaultBrands = [
  { key: 'Anua', label: 'Anua' },
  { key: 'APRILSKIN', label: 'APRILSKIN' },
  { key: 'VT (VT Global)', label: 'VT (VT Global)' },
  { key: 'Skinfood', label: 'Skinfood' },
  { key: 'Medicube', label: 'Medicube' },
  { key: 'Numbuzin', label: 'Numbuzin' },
  { key: 'K-SECRET', label: 'K-SECRET' },
  { key: 'Equal Berry', label: 'Equal Berry' },
  { key: 'SKIN1004', label: 'SKIN1004' },
  { key: 'Beauty of Joseon', label: 'Beauty of Joseon' },
  { key: 'JMsolution', label: 'JMsolution' },
  { key: 'Tenzero', label: 'Tenzero' },
  { key: 'Dr.Ceuracle', label: 'Dr.Ceuracle' },
  { key: 'Rejuran', label: 'Rejuran' },
  { key: 'Celimax', label: 'Celimax' },
  { key: 'Medipeel', label: 'Medipeel' },
  { key: 'Biodance', label: 'Biodance' },
  { key: 'Dr.CPU', label: 'Dr.CPU' },
  { key: 'Anua KR', label: 'Anua KR' },
]

const brands = ref<{ key: string; label: string }[]>(defaultBrands)

// حمّل البراندات من الباك (إذا متاح) حتى تكون الفهرسة ديناميكية
await useAsyncData('brands', async () => {
  try {
    const res: any = await api.get('/Brands')
    const items = (res as any)?.items ?? (res as any)?.data?.items ?? []
    if (Array.isArray(items) && items.length) {
      brands.value = items
    }
  } catch (e) {
    // خليه على defaultBrands
  }
  return true
})

const brandKeys = computed(() => brands.value.map((b) => b.key))

const sortOptions = computed(() => [
  { value: 'new', label: t('productsPage.sort.new') },
  { value: 'priceAsc', label: t('productsPage.sort.priceAsc') },
  { value: 'priceDesc', label: t('productsPage.sort.priceDesc') },
])

const sortLabel = computed(() => sortOptions.value.find((o) => o.value === sort.value)?.label || t('productsPage.sort.new'))

const selectedBrand = computed(() => {
  const b = String((route.query.brand as any) || 'All')
  return brandKeys.value.includes(b) ? b : 'All'
})

const selectedBrandLabel = computed(() =>
  selectedBrand.value === 'All' ? t('productsPage.brands.all') : String(selectedBrand.value)
)

function setSort(v: string) {
  const value = String(v || 'new')
  sort.value = value
  apply({ sort: value, page: 1 })
  sortMenu.value?.removeAttribute('open')
}

function pickBrand(label: string) {
  const next = String(label || '').trim()
  const current = String(route.query.brand || '').trim()
  // نفس البراند؟ اعتبره إلغاء تحديد
  const chosen = current === next ? '' : next
  // ✅ نخلي q = اسم البراند حتى يفلتر المنتجات مباشرة
  q.value = chosen
  apply({ brand: chosen || undefined, q: chosen || undefined, page: 1 })
}

function clearBrand() {
  q.value = ''
  apply({ brand: undefined, q: undefined, page: 1 })
  brandMenu.value?.removeAttribute('open')
}

function selectBrand(b: string) {
  const v = String(b || '').trim()
  if (!v || v === 'All') return clearBrand()
  q.value = v
  apply({ brand: v, q: v, page: 1 })
  brandMenu.value?.removeAttribute('open')
}

watch(
  () => route.query.q,
  (v) => {
    q.value = String(v || '')
  }
)

watch(
  () => route.query.sort,
  (v) => {
    sort.value = String(v || 'new')
  }
)

const masonryCols = ref(3)

function updateMasonryCols() {
  if (process.server) return
  const w = window.innerWidth
  masonryCols.value = w < 640 ? 1 : w < 1024 ? 2 : 3
}

onMounted(() => {
  updateMasonryCols()
  window.addEventListener('resize', updateMasonryCols)
})

onBeforeUnmount(() => {
  if (process.server) return
  window.removeEventListener('resize', updateMasonryCols)
})

const items = computed(() => products.items)

// فتح Quick Preview عبر رابط قابل للمشاركة: /products?p=<id>
watch(
  () => route.query.p,
  async (v) => {
    if (!v) return
    const id = String(v)
    let prod: any = items.value.find((x: any) => String(x.id ?? x.slug ?? '') === id)
    if (!prod) {
      try {
        prod = await api.get(`/Products/${id}`)
      } catch (e) {
        // إذا المنتج غير موجود لا نكسر الصفحة
        return
      }
    }
    if (prod) qp.show(prod)
  },
  { immediate: true }
)

// SSR + CSR: حمّل المنتجات بشكل مضمون من أول تحميل
const { pending: loading } = await useAsyncData(
  () => `products:${route.fullPath}`,
  async () => {
    await products.fetch({ page: page.value, pageSize, q: q.value, sort: sort.value, brand: selectedBrand.value !== "All" ? selectedBrand.value : undefined })
    return true
  },
  { watch: [() => route.fullPath] }
)

function apply(extra?: { q?: string; brand?: string | undefined; page?: number }){
  const brand = typeof extra?.brand !== 'undefined' ? extra.brand : selectedBrand.value
  const qq = typeof extra?.q !== 'undefined' ? extra.q : q.value
  const p = typeof extra?.page === 'number' ? extra.page : 1

  router.push({
    query: {
      ...(qq ? { q: qq } : {}),
      ...(brand ? { brand } : {}),
      sort: sort.value,
      page: p,
    },
  })
}

function prev(){
  if (page.value <= 1) return
  const brand = selectedBrand.value
  router.push({
    query: {
      ...(q.value ? { q: q.value } : {}),
      ...(brand ? { brand } : {}),
      sort: sort.value,
      page: page.value - 1,
    },
  })
}

function next(){
  const brand = selectedBrand.value
  router.push({
    query: {
      ...(q.value ? { q: q.value } : {}),
      ...(brand ? { brand } : {}),
      sort: sort.value,
      page: page.value + 1,
    },
  })
}
</script>
