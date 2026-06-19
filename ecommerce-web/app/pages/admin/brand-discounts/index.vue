<script setup lang="ts">
definePageMeta({
  layout: 'admin',
  middleware: ['admin'],
})

type BrandDiscount = {
  id: string
  slug: string
  name: string
  logoUrl?: string | null
  isActive: boolean
  productCount: number
  publishedProductCount: number
  discountedProductCount: number
  minDiscountPercent: number
  maxDiscountPercent: number
  averageDiscountPercent: number
  hasDiscount: boolean
  isUniformDiscount: boolean
  currentDiscountPercent: number
  discountLabel: string
}

type ProductPreview = {
  id: string
  title: string
  slug: string
  priceIqd: number
  discountPercent: number
  finalPriceIqd: number
  isPublished: boolean
  coverImage?: string | null
}

const { t } = useI18n()
const api = useAdminApi()
const toast = useToast()
const { buildAssetUrl } = useApi()

const loading = ref(false)
const saving = ref(false)
const error = ref<string | null>(null)
const search = ref('')
const filter = ref<'all' | 'discounted' | 'without' | 'inactive'>('all')
const brands = ref<BrandDiscount[]>([])
const selectedBrandSlug = ref('')
const discountPercent = ref<number>(10)
const publishedOnly = ref(false)
const productsPreview = ref<ProductPreview[]>([])
const previewLoading = ref(false)

const selectedBrand = computed(() => brands.value.find(x => x.slug === selectedBrandSlug.value) || null)

const stats = computed(() => ({
  totalBrands: brands.value.length,
  activeBrandDiscounts: brands.value.filter(x => x.hasDiscount).length,
  totalProducts: brands.value.reduce((sum, x) => sum + Number(x.productCount || 0), 0),
  discountedProducts: brands.value.reduce((sum, x) => sum + Number(x.discountedProductCount || 0), 0),
}))

const filteredBrands = computed(() => {
  const q = search.value.trim().toLowerCase()
  return brands.value.filter((b) => {
    if (filter.value === 'discounted' && !b.hasDiscount) return false
    if (filter.value === 'without' && b.hasDiscount) return false
    if (filter.value === 'inactive' && b.isActive) return false
    if (!q) return true
    return String(b.name || '').toLowerCase().includes(q) || String(b.slug || '').toLowerCase().includes(q)
  })
})

function formatIqd(value: number) {
  const n = Number(value || 0)
  return `${new Intl.NumberFormat('en-US').format(n)} د.ع`
}

function chooseBrand(slug: string) {
  selectedBrandSlug.value = slug
  const brand = brands.value.find(x => x.slug === slug)
  if (brand?.isUniformDiscount && brand.currentDiscountPercent > 0) {
    discountPercent.value = brand.currentDiscountPercent
  }
  loadPreview()
}

async function loadData() {
  loading.value = true
  error.value = null
  try {
    const res: any = await api.get('/admin/brand-discounts')
    const list = Array.isArray(res?.items) ? res.items : Array.isArray(res) ? res : []
    brands.value = list
    if (!selectedBrandSlug.value && list.length) {
      selectedBrandSlug.value = list[0].slug
      if (list[0]?.currentDiscountPercent > 0) discountPercent.value = list[0].currentDiscountPercent
      await loadPreview()
    }
  } catch (e: any) {
    error.value = e?.data?.message || e?.friendlyMessage || e?.message || t('common.requestFailed')
  } finally {
    loading.value = false
  }
}

async function loadPreview() {
  if (!selectedBrandSlug.value) {
    productsPreview.value = []
    return
  }
  previewLoading.value = true
  try {
    const res: any = await api.get(`/admin/brand-discounts/${selectedBrandSlug.value}/products`)
    productsPreview.value = Array.isArray(res?.items) ? res.items : []
  } catch {
    productsPreview.value = []
  } finally {
    previewLoading.value = false
  }
}

async function applyDiscount() {
  if (!selectedBrandSlug.value) return toast.error(t('admin.brandDiscountsPickBrand'))
  const value = Math.max(0, Math.min(100, Number(discountPercent.value || 0)))
  if (!confirm(t('admin.brandDiscountsApplyConfirm', { value, brand: selectedBrand.value?.name || selectedBrandSlug.value } as any))) return

  saving.value = true
  error.value = null
  try {
    const res: any = await api.post('/admin/brand-discounts/apply', {
      brandSlug: selectedBrandSlug.value,
      discountPercent: value,
      publishedOnly: publishedOnly.value,
    })
    toast.success(`${t('admin.brandDiscountsApplied')} (${res?.affectedProducts ?? 0})`)
    await loadData()
    await loadPreview()
  } catch (e: any) {
    error.value = e?.data?.message || e?.friendlyMessage || e?.message || t('common.requestFailed')
    toast.error(error.value || t('common.requestFailed'))
  } finally {
    saving.value = false
  }
}

async function clearDiscount(slug?: string) {
  const target = slug || selectedBrandSlug.value
  if (!target) return
  const brand = brands.value.find(x => x.slug === target)
  if (!confirm(t('admin.brandDiscountsClearConfirm', { brand: brand?.name || target } as any))) return

  saving.value = true
  error.value = null
  try {
    const res: any = await api.post('/admin/brand-discounts/clear', { brandSlug: target })
    toast.success(`${t('admin.brandDiscountsCleared')} (${res?.affectedProducts ?? 0})`)
    await loadData()
    await loadPreview()
  } catch (e: any) {
    error.value = e?.data?.message || e?.friendlyMessage || e?.message || t('common.requestFailed')
    toast.error(error.value || t('common.requestFailed'))
  } finally {
    saving.value = false
  }
}

onMounted(loadData)
</script>

<template>
  <div class="brand-discounts-page" dir="rtl">
    <section class="bd-hero">
      <div>
        <div class="bd-kicker">{{ t('admin.brandDiscountsKicker') }}</div>
        <h1>{{ t('admin.brandDiscountsTitle') }}</h1>
        <p>{{ t('admin.brandDiscountsSubtitle') }}</p>
      </div>
      <div class="bd-hero-actions">
        <button class="bd-btn bd-btn-ghost" type="button" :disabled="loading" @click="loadData">
          <Icon name="mdi:refresh" />
          <span>{{ t('common.refresh') }}</span>
        </button>
        <NuxtLink to="/discounts" class="bd-btn bd-btn-soft">
          <Icon name="mdi:tag-heart-outline" />
          <span>{{ t('admin.viewDiscountPage') }}</span>
        </NuxtLink>
      </div>
    </section>

    <section class="bd-tips">
      <div class="bd-tip-icon"><Icon name="mdi:lightbulb-on-outline" /></div>
      <div>
        <h2>{{ t('admin.brandDiscountsHowTitle') }}</h2>
        <ul>
          <li>{{ t('admin.brandDiscountsTip1') }}</li>
          <li>{{ t('admin.brandDiscountsTip2') }}</li>
          <li>{{ t('admin.brandDiscountsTip3') }}</li>
        </ul>
      </div>
    </section>

    <section class="bd-stats">
      <div class="bd-stat">
        <Icon name="mdi:storefront-outline" />
        <span>{{ t('admin.brandDiscountsTotalBrands') }}</span>
        <strong>{{ stats.totalBrands }}</strong>
      </div>
      <div class="bd-stat success">
        <Icon name="mdi:sale-outline" />
        <span>{{ t('admin.brandDiscountsActiveBrands') }}</span>
        <strong>{{ stats.activeBrandDiscounts }}</strong>
      </div>
      <div class="bd-stat warn">
        <Icon name="mdi:cube-outline" />
        <span>{{ t('admin.brandDiscountsTotalProducts') }}</span>
        <strong>{{ stats.totalProducts }}</strong>
      </div>
      <div class="bd-stat danger">
        <Icon name="mdi:tag-percent-outline" />
        <span>{{ t('admin.brandDiscountsDiscountedProducts') }}</span>
        <strong>{{ stats.discountedProducts }}</strong>
      </div>
    </section>

    <div v-if="error" class="bd-error">
      <Icon name="mdi:alert-circle-outline" />
      <span>{{ error }}</span>
    </div>

    <section class="bd-grid">
      <div class="bd-panel bd-form-panel">
        <div class="bd-panel-head">
          <div>
            <span>{{ t('admin.brandDiscountsEditor') }}</span>
            <h2>{{ t('admin.brandDiscountsCreateOrUpdate') }}</h2>
          </div>
          <Icon name="mdi:percent-box-outline" />
        </div>

        <div class="bd-form">
          <label class="bd-field">
            <span>{{ t('admin.brandDiscountsSelectBrand') }}</span>
            <select v-model="selectedBrandSlug" @change="loadPreview">
              <option disabled value="">{{ t('admin.brandDiscountsSelectBrandPlaceholder') }}</option>
              <option v-for="brand in brands" :key="brand.id" :value="brand.slug">
                {{ brand.name }} — {{ brand.productCount }} {{ t('admin.productsLabel') }}
              </option>
            </select>
          </label>

          <div class="bd-selected" v-if="selectedBrand">
            <div class="bd-logo">
              <SmartImage v-if="selectedBrand.logoUrl" :src="buildAssetUrl(selectedBrand.logoUrl)" :alt="selectedBrand.name" />
              <span v-else>{{ selectedBrand.name?.slice(0, 2) }}</span>
            </div>
            <div>
              <strong>{{ selectedBrand.name }}</strong>
              <small>/{{ selectedBrand.slug }}</small>
            </div>
            <span class="bd-status" :class="selectedBrand.hasDiscount ? 'on' : 'off'">
              {{ selectedBrand.discountLabel }}
            </span>
          </div>

          <label class="bd-field">
            <span>{{ t('admin.brandDiscountsPercent') }}</span>
            <div class="bd-percent-box">
              <input v-model.number="discountPercent" type="range" min="0" max="100" />
              <input v-model.number="discountPercent" type="number" min="0" max="100" />
              <b>%</b>
            </div>
          </label>

          <div class="bd-quick-values">
            <button v-for="value in [5, 10, 15, 20, 25, 30, 50]" :key="value" type="button" @click="discountPercent = value">
              {{ value }}%
            </button>
          </div>

          <label class="bd-check">
            <input v-model="publishedOnly" type="checkbox" />
            <span>{{ t('admin.brandDiscountsPublishedOnly') }}</span>
          </label>

          <div class="bd-form-actions">
            <button class="bd-btn bd-btn-primary" type="button" :disabled="saving || !selectedBrandSlug" @click="applyDiscount">
              <Icon name="mdi:content-save-check-outline" />
              <span>{{ t('admin.brandDiscountsApply') }}</span>
            </button>
            <button class="bd-btn bd-btn-danger" type="button" :disabled="saving || !selectedBrandSlug" @click="clearDiscount()">
              <Icon name="mdi:tag-off-outline" />
              <span>{{ t('admin.brandDiscountsClear') }}</span>
            </button>
          </div>
        </div>
      </div>

      <div class="bd-panel">
        <div class="bd-panel-head">
          <div>
            <span>{{ t('admin.brandDiscountsPreviewLabel') }}</span>
            <h2>{{ selectedBrand?.name || t('admin.brandDiscountsNoBrandSelected') }}</h2>
          </div>
          <Icon name="mdi:eye-outline" />
        </div>

        <div v-if="previewLoading" class="bd-empty">{{ t('common.loading') }}</div>
        <div v-else-if="!productsPreview.length" class="bd-empty">
          {{ t('admin.brandDiscountsNoProducts') }}
        </div>
        <div v-else class="bd-products-preview">
          <article v-for="product in productsPreview.slice(0, 8)" :key="product.id" class="bd-product-mini">
            <div class="bd-product-img">
              <SmartImage v-if="product.coverImage" :src="buildAssetUrl(product.coverImage)" :alt="product.title" />
              <Icon v-else name="mdi:image-off-outline" />
            </div>
            <div class="min-w-0">
              <strong>{{ product.title }}</strong>
              <small>/{{ product.slug }}</small>
              <div class="bd-price-line">
                <span>{{ formatIqd(product.finalPriceIqd || product.priceIqd) }}</span>
                <b v-if="product.discountPercent > 0">-{{ product.discountPercent }}%</b>
              </div>
            </div>
          </article>
        </div>
      </div>
    </section>

    <section class="bd-panel">
      <div class="bd-list-head">
        <div>
          <span>{{ t('admin.brandDiscountsBrandsList') }}</span>
          <h2>{{ t('admin.brandDiscountsAllBrands') }}</h2>
        </div>
        <div class="bd-filters">
          <input v-model="search" :placeholder="t('admin.brandDiscountsSearch')" />
          <select v-model="filter">
            <option value="all">{{ t('common.all') }}</option>
            <option value="discounted">{{ t('admin.brandDiscountsWithDiscount') }}</option>
            <option value="without">{{ t('admin.brandDiscountsWithoutDiscount') }}</option>
            <option value="inactive">{{ t('common.inactive') }}</option>
          </select>
        </div>
      </div>

      <div v-if="loading" class="bd-empty">{{ t('common.loading') }}</div>
      <div v-else-if="!filteredBrands.length" class="bd-empty">{{ t('common.noResults') }}</div>
      <div v-else class="bd-brand-list">
        <article v-for="brand in filteredBrands" :key="brand.id" class="bd-brand-card" :class="{ selected: brand.slug === selectedBrandSlug }" @click="chooseBrand(brand.slug)">
          <div class="bd-logo big">
            <SmartImage v-if="brand.logoUrl" :src="buildAssetUrl(brand.logoUrl)" :alt="brand.name" />
            <span v-else>{{ brand.name?.slice(0, 2) }}</span>
          </div>

          <div class="bd-brand-main">
            <div class="bd-brand-title">
              <strong>{{ brand.name }}</strong>
              <small>/{{ brand.slug }}</small>
            </div>
            <div class="bd-brand-meta">
              <span>{{ brand.productCount }} {{ t('admin.productsLabel') }}</span>
              <span>{{ brand.publishedProductCount }} {{ t('admin.brandDiscountsPublished') }}</span>
              <span>{{ brand.discountedProductCount }} {{ t('admin.brandDiscountsDiscounted') }}</span>
            </div>
          </div>

          <div class="bd-brand-actions" @click.stop>
            <span class="bd-status" :class="brand.hasDiscount ? 'on' : 'off'">
              {{ brand.discountLabel }}
            </span>
            <button class="bd-mini-btn" type="button" @click="chooseBrand(brand.slug)">
              {{ t('admin.brandDiscountsEdit') }}
            </button>
            <button v-if="brand.hasDiscount" class="bd-mini-btn danger" type="button" @click="clearDiscount(brand.slug)">
              {{ t('admin.brandDiscountsClearShort') }}
            </button>
          </div>
        </article>
      </div>
    </section>
  </div>
</template>

<style scoped>
.brand-discounts-page{
  display:grid;
  gap:1.2rem;
  color:rgb(var(--text));
}
.bd-hero,
.bd-panel,
.bd-tips,
.bd-stat{
  border:1px solid rgba(var(--border), .9);
  background:
    linear-gradient(180deg, rgba(var(--surface-rgb), .96), rgba(var(--surface-2-rgb), .88)),
    radial-gradient(circle at top right, rgba(var(--primary), .12), transparent 44%);
  border-radius:30px;
  box-shadow: 0 24px 70px rgba(0,0,0,.08);
}
.bd-hero{
  padding:1.35rem;
  display:flex;
  align-items:center;
  justify-content:space-between;
  gap:1rem;
}
.bd-kicker,
.bd-panel-head span,
.bd-list-head span{
  color:rgb(var(--primary));
  font-size:.8rem;
  font-weight:900;
}
.bd-hero h1{
  margin:.25rem 0;
  font-size:clamp(1.8rem, 4vw, 3.35rem);
  font-weight:1000;
  letter-spacing:-.04em;
}
.bd-hero p,
.bd-tips li,
.bd-empty,
.bd-brand-meta,
.bd-selected small,
.bd-product-mini small,
.bd-brand-title small{
  color:rgb(var(--muted));
}
.bd-hero-actions,
.bd-form-actions,
.bd-filters,
.bd-brand-actions{
  display:flex;
  flex-wrap:wrap;
  gap:.65rem;
  align-items:center;
}
.bd-btn,
.bd-mini-btn{
  border:1px solid rgba(var(--border), .95);
  border-radius:18px;
  display:inline-flex;
  align-items:center;
  justify-content:center;
  gap:.45rem;
  padding:.85rem 1rem;
  font-weight:900;
  transition:.18s ease;
}
.bd-btn:hover,
.bd-mini-btn:hover{ transform:translateY(-1px); }
.bd-btn-primary{
  color:rgb(var(--on-primary));
  background:linear-gradient(135deg, rgb(var(--primary)), rgb(var(--primary-2)));
  border-color:transparent;
}
.bd-btn-soft{ background:rgba(var(--primary), .13); color:rgb(var(--primary)); }
.bd-btn-ghost{ background:rgba(var(--surface-2-rgb), .9); }
.bd-btn-danger{ background:rgba(239,68,68,.12); color:#ef4444; border-color:rgba(239,68,68,.25); }
.bd-tips{
  display:flex;
  align-items:flex-start;
  gap:1rem;
  padding:1rem 1.2rem;
  border-color:rgba(var(--primary), .32);
}
.bd-tip-icon{
  width:48px;
  height:48px;
  border-radius:18px;
  display:grid;
  place-items:center;
  color:rgb(var(--primary));
  background:rgba(var(--primary), .12);
  font-size:1.4rem;
  flex:0 0 auto;
}
.bd-tips h2{ font-weight:1000; margin-bottom:.35rem; }
.bd-tips ul{ display:grid; gap:.2rem; font-size:.92rem; }
.bd-stats{
  display:grid;
  grid-template-columns:repeat(4, minmax(0, 1fr));
  gap:.9rem;
}
.bd-stat{
  padding:1rem;
  display:grid;
  gap:.35rem;
  min-height:118px;
}
.bd-stat svg{ font-size:1.7rem; color:rgb(var(--primary)); }
.bd-stat.success svg{ color:#22c55e; }
.bd-stat.warn svg{ color:#f59e0b; }
.bd-stat.danger svg{ color:#ef4444; }
.bd-stat span{ color:rgb(var(--muted)); font-size:.85rem; }
.bd-stat strong{ font-size:2rem; font-weight:1000; }
.bd-error{
  border:1px solid rgba(239,68,68,.35);
  background:rgba(239,68,68,.12);
  color:#ef4444;
  border-radius:22px;
  padding:1rem;
  display:flex;
  gap:.6rem;
  align-items:center;
  font-weight:800;
}
.bd-grid{
  display:grid;
  grid-template-columns:minmax(0, .95fr) minmax(0, 1.05fr);
  gap:1rem;
}
.bd-panel{ padding:1.15rem; }
.bd-panel-head,
.bd-list-head{
  display:flex;
  justify-content:space-between;
  align-items:flex-start;
  gap:1rem;
  margin-bottom:1rem;
}
.bd-panel-head h2,
.bd-list-head h2{
  margin-top:.2rem;
  font-size:1.35rem;
  font-weight:1000;
}
.bd-panel-head > svg{ color:rgb(var(--primary)); font-size:1.45rem; }
.bd-form{ display:grid; gap:1rem; }
.bd-field{ display:grid; gap:.45rem; font-weight:900; }
.bd-field > span{ font-size:.88rem; }
.bd-field input,
.bd-field select,
.bd-filters input,
.bd-filters select{
  width:100%;
  border-radius:18px;
  border:1px solid rgba(var(--border), .95);
  background:rgba(var(--surface-2-rgb), .86);
  color:rgb(var(--text));
  padding:.9rem 1rem;
  outline:none;
}
.bd-field input:focus,
.bd-field select:focus,
.bd-filters input:focus,
.bd-filters select:focus{
  border-color:rgba(var(--primary), .72);
  box-shadow:0 0 0 4px rgba(var(--primary), .13);
}
.bd-selected{
  display:flex;
  align-items:center;
  gap:.8rem;
  border:1px solid rgba(var(--border), .9);
  background:rgba(var(--surface-2-rgb), .75);
  border-radius:22px;
  padding:.75rem;
}
.bd-logo{
  width:48px;
  height:48px;
  border-radius:17px;
  overflow:hidden;
  background:rgba(var(--primary), .12);
  border:1px solid rgba(var(--border), .65);
  display:grid;
  place-items:center;
  font-weight:1000;
  flex:0 0 auto;
}
.bd-logo.big{ width:66px; height:66px; border-radius:22px; }
.bd-logo :deep(img){ width:100%; height:100%; object-fit:cover; }
.bd-status{
  margin-inline-start:auto;
  border-radius:999px;
  padding:.45rem .75rem;
  font-size:.78rem;
  font-weight:1000;
  border:1px solid rgba(var(--border), .75);
}
.bd-status.on{ color:#22c55e; background:rgba(34,197,94,.12); border-color:rgba(34,197,94,.28); }
.bd-status.off{ color:rgb(var(--muted)); background:rgba(var(--surface-rgb), .75); }
.bd-percent-box{
  display:grid;
  grid-template-columns:1fr 100px 32px;
  align-items:center;
  gap:.6rem;
  border:1px solid rgba(var(--border), .92);
  border-radius:20px;
  padding:.65rem;
  background:rgba(var(--surface-2-rgb), .75);
}
.bd-percent-box input[type="range"]{ accent-color:rgb(var(--primary)); padding:0; border:none; background:transparent; }
.bd-percent-box input[type="number"]{ text-align:center; font-weight:1000; }
.bd-percent-box b{ color:rgb(var(--primary)); font-size:1.25rem; }
.bd-quick-values{ display:flex; flex-wrap:wrap; gap:.5rem; }
.bd-quick-values button{
  border:1px solid rgba(var(--primary), .22);
  background:rgba(var(--primary), .09);
  color:rgb(var(--primary));
  border-radius:999px;
  padding:.55rem .8rem;
  font-weight:1000;
}
.bd-check{
  display:flex;
  align-items:center;
  gap:.55rem;
  border:1px solid rgba(var(--border), .9);
  background:rgba(var(--surface-2-rgb), .75);
  border-radius:18px;
  padding:.8rem 1rem;
  font-weight:800;
}
.bd-check input{ accent-color:rgb(var(--primary)); }
.bd-products-preview{ display:grid; gap:.7rem; }
.bd-product-mini{
  display:grid;
  grid-template-columns:58px 1fr;
  gap:.75rem;
  align-items:center;
  border:1px solid rgba(var(--border), .85);
  background:rgba(var(--surface-2-rgb), .72);
  border-radius:20px;
  padding:.65rem;
}
.bd-product-img{
  width:58px;
  height:58px;
  border-radius:17px;
  overflow:hidden;
  display:grid;
  place-items:center;
  background:rgba(var(--surface-rgb), .75);
}
.bd-product-img :deep(img){ width:100%; height:100%; object-fit:cover; }
.bd-product-mini strong,
.bd-brand-title strong{ display:block; overflow:hidden; text-overflow:ellipsis; white-space:nowrap; }
.bd-price-line{ display:flex; gap:.5rem; align-items:center; margin-top:.35rem; }
.bd-price-line span{ font-weight:1000; }
.bd-price-line b{ color:#ef4444; background:rgba(239,68,68,.12); padding:.2rem .45rem; border-radius:999px; }
.bd-empty{
  border:1px dashed rgba(var(--border), .85);
  border-radius:24px;
  padding:2rem;
  text-align:center;
}
.bd-list-head{ align-items:center; }
.bd-filters{ min-width:min(100%, 620px); }
.bd-filters input{ flex:1 1 260px; }
.bd-filters select{ flex:0 0 180px; }
.bd-brand-list{ display:grid; gap:.85rem; }
.bd-brand-card{
  display:grid;
  grid-template-columns:66px minmax(0, 1fr) auto;
  gap:.9rem;
  align-items:center;
  border:1px solid rgba(var(--border), .85);
  background:linear-gradient(180deg, rgba(var(--surface-2-rgb), .78), rgba(var(--surface-rgb), .62));
  border-radius:26px;
  padding:.9rem;
  cursor:pointer;
  transition:.18s ease;
}
.bd-brand-card:hover,
.bd-brand-card.selected{
  border-color:rgba(var(--primary), .48);
  box-shadow:0 20px 50px rgba(var(--primary), .08);
  transform:translateY(-1px);
}
.bd-brand-meta{ display:flex; flex-wrap:wrap; gap:.45rem; margin-top:.45rem; font-size:.82rem; }
.bd-brand-meta span{
  border:1px solid rgba(var(--border), .7);
  border-radius:999px;
  padding:.25rem .55rem;
  background:rgba(var(--surface-rgb), .55);
}
.bd-mini-btn{
  padding:.55rem .8rem;
  font-size:.82rem;
  background:rgba(var(--surface-rgb), .7);
}
.bd-mini-btn.danger{ color:#ef4444; border-color:rgba(239,68,68,.28); background:rgba(239,68,68,.1); }
@media (max-width: 1100px){
  .bd-grid{ grid-template-columns:1fr; }
  .bd-stats{ grid-template-columns:repeat(2, minmax(0, 1fr)); }
}
@media (max-width: 760px){
  .bd-hero{ align-items:stretch; flex-direction:column; }
  .bd-stats{ grid-template-columns:1fr; }
  .bd-brand-card{ grid-template-columns:54px 1fr; }
  .bd-brand-actions{ grid-column:1 / -1; }
  .bd-logo.big{ width:54px; height:54px; }
  .bd-percent-box{ grid-template-columns:1fr 86px 24px; }
}
</style>
