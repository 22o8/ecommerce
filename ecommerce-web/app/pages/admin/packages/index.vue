<template>
  <div class="grid gap-6">
    <div class="flex flex-col gap-3 md:flex-row md:items-center md:justify-between">
      <div>
        <h1 class="text-2xl font-black rtl-text">إدارة البكجات</h1>
        <p class="text-muted rtl-text mt-1">اجمع منتجات متعددة داخل بكج واحد، وحدد السعر والصورة ومكان عرضه.</p>
      </div>
      <button class="btn-primary" type="button" @click="resetForm">بكج جديد</button>
    </div>

    <div v-if="error" class="rounded-3xl border border-red-500/30 bg-red-500/10 p-4 text-red-200 rtl-text">{{ error }}</div>

    <section class="card-soft p-5 grid gap-5">
      <h2 class="text-xl font-black rtl-text">{{ form.id ? 'تعديل بكج' : 'إنشاء بكج' }}</h2>
      <div class="grid gap-3 md:grid-cols-2">
        <label class="field"><span>اسم البكج بالعربي</span><input v-model="form.nameAr" class="input" placeholder="مثال: بكج تفتيح البشرة" /></label>
        <label class="field"><span>اسم البكج بالإنكليزي</span><input v-model="form.nameEn" class="input" placeholder="Brightening package" /></label>
        <label class="field"><span>Slug الرابط</span><input v-model="form.slug" class="input" placeholder="brightening-package" /></label>
        <label class="field"><span>ملاحظة</span><input v-model="form.note" class="input" placeholder="ملاحظة تظهر للزبون" /></label>
        <label class="field md:col-span-2"><span>وصف قصير</span><textarea v-model="form.shortDescription" class="input min-h-20" /></label>
        <label class="field md:col-span-2"><span>وصف SEO طويل</span><textarea v-model="form.seoDescription" class="input min-h-24" /></label>
      </div>

      <div class="grid gap-3 md:grid-cols-3">
        <label class="field"><span>السعر الأصلي</span><input v-model.number="form.originalPriceIqd" class="input" type="number" min="0" /></label>
        <label class="field"><span>السعر النهائي</span><input v-model.number="form.finalPriceIqd" class="input" type="number" min="0" /></label>
        <label class="field"><span>ترتيب العرض</span><input v-model.number="form.sortOrder" class="input" type="number" /></label>
      </div>

      <div class="grid gap-3 md:grid-cols-3">
        <label class="check"><input v-model="form.isPublished" type="checkbox" /> مفعل</label>
        <label class="check"><input v-model="form.isFeatured" type="checkbox" /> مميز</label>
        <label class="check"><input v-model="form.showInSlider" type="checkbox" /> عرض في السلايدر</label>
        <label v-if="form.showInSlider" class="field md:col-span-2"><span>مكان السلايدر</span>
          <select v-model="form.sliderPlacement" class="input">
            <option value="home_top">بداية الصفحة الرئيسية</option>
            <option value="home_bottom">نهاية الصفحة الرئيسية</option>
            <option value="packages">صفحة البكجات فقط</option>
            <option value="offers">قسم العروض والبكجات</option>
          </select>
        </label>
      </div>

      <div class="grid gap-3 md:grid-cols-[1fr_auto] items-end">
        <label class="field"><span>صورة أو فيديو البكج</span><input class="input" type="file" accept="image/*,video/*" @change="uploadCover" /></label>
        <a v-if="form.coverUrl" :href="asset(form.coverUrl)" target="_blank" class="btn-secondary text-center">عرض الملف</a>
      </div>

      <div class="rounded-3xl border border-white/10 p-4 grid gap-3">
        <h3 class="font-black rtl-text">إضافة المنتجات</h3>
        <div class="grid gap-2 md:grid-cols-[1fr_auto]">
          <input v-model="productSearch" class="input" placeholder="ابحث باسم المنتج أو البراند" @keyup.enter="searchProducts" />
          <button class="btn-secondary" type="button" @click="searchProducts">بحث</button>
        </div>
        <div class="grid gap-2 md:grid-cols-2 xl:grid-cols-3 max-h-72 overflow-auto pe-1">
          <button v-for="p in searchResults" :key="p.id" type="button" class="rounded-2xl border border-white/10 bg-white/5 p-3 text-start hover:bg-white/10" @click="addProduct(p)">
            <div class="font-black rtl-text">{{ p.title }}</div>
            <div class="text-xs text-muted">{{ p.brand }} • {{ money(p.priceIqd) }} • مخزون {{ p.stockQuantity }}</div>
          </button>
        </div>
        <div v-if="form.items.length" class="grid gap-2">
          <div v-for="item in form.items" :key="item.productId" class="rounded-2xl border border-white/10 p-3 grid gap-2 md:grid-cols-[1fr_120px_auto] md:items-center">
            <div>
              <div class="font-black rtl-text">{{ item.productTitle }}</div>
              <div class="text-xs text-muted">المخزون: {{ item.stockQuantity }}</div>
              <div v-if="item.stockQuantity < item.quantity" class="text-xs text-red-300 rtl-text">تحذير: الكمية المطلوبة أكبر من المخزون.</div>
            </div>
            <input v-model.number="item.quantity" class="input" type="number" min="1" />
            <button class="btn-danger" type="button" @click="removeProduct(item.productId)">حذف</button>
          </div>
        </div>
      </div>

      <div class="flex flex-wrap gap-3">
        <button class="btn-primary" type="button" :disabled="saving" @click="savePackage">{{ saving ? 'جاري الحفظ...' : 'حفظ البكج' }}</button>
        <button class="btn-secondary" type="button" @click="resetForm">تفريغ النموذج</button>
      </div>
    </section>

    <section class="grid gap-3">
      <h2 class="text-xl font-black rtl-text">البكجات الحالية</h2>
      <div v-for="pkg in packages" :key="pkg.id" class="card-soft p-4 grid gap-3 lg:grid-cols-[1fr_auto] lg:items-center">
        <div class="grid gap-2">
          <div class="flex flex-wrap items-center gap-2">
            <h3 class="font-black text-lg rtl-text">{{ pkg.name }}</h3>
            <span class="badge" :class="pkg.canSell ? 'ok' : 'bad'">{{ pkg.canSell ? 'جاهز للبيع' : 'ناقص/غير متوفر' }}</span>
            <span class="badge">متوفر {{ pkg.availablePackages }}</span>
          </div>
          <p class="text-sm text-muted rtl-text">{{ pkg.shortDescription }}</p>
          <div class="text-sm text-muted rtl-text">{{ money(pkg.originalPriceIqd) }} ← <b class="text-white">{{ money(pkg.finalPriceIqd) }}</b> • توفير {{ money(pkg.savingsIqd) }}</div>
          <div v-if="pkg.warnings?.length" class="rounded-2xl border border-yellow-500/30 bg-yellow-500/10 p-3 text-yellow-100 rtl-text">
            تحذير: هذا البكج لا يمكن بيعه حالياً لأن منتجاً داخله ناقص أو غير منشور.
            <ul class="mt-2 list-disc pe-5">
              <li v-for="w in pkg.warnings" :key="w.productId">{{ w.productTitle }} — مطلوب {{ w.required }} / متوفر {{ w.available }}</li>
            </ul>
          </div>
        </div>
        <div class="flex flex-wrap gap-2">
          <button class="btn-secondary" type="button" @click="editPackage(pkg)">تعديل</button>
          <button class="btn-danger" type="button" @click="deletePackage(pkg.id)">حذف</button>
        </div>
      </div>
    </section>
  </div>
</template>

<script setup lang="ts">
import { buildAssetUrl } from '~/composables/useApi'

definePageMeta({ layout: 'admin' })
const api = useAdminApi()
const error = ref('')
const saving = ref(false)
const packages = ref<any[]>([])
const searchResults = ref<any[]>([])
const productSearch = ref('')

const emptyForm = () => ({
  id: '', nameAr: '', nameEn: '', slug: '', shortDescription: '', seoDescription: '', note: '', coverUrl: '', mediaType: 'image',
  originalPriceIqd: 0, finalPriceIqd: 0, category: '', problemCategory: '', isPublished: true, isFeatured: false, showInSlider: false, sliderPlacement: 'packages', sortOrder: 0,
  items: [] as any[]
})
const form = reactive(emptyForm())
const asset = (v: string) => buildAssetUrl(v)
const money = (v: number) => `${Number(v || 0).toLocaleString('en-US')} د.ع`

async function loadPackages() {
  packages.value = await api.get<any[]>('/admin/packages')
}
async function searchProducts() {
  searchResults.value = await api.get<any[]>('/admin/packages/product-search', { q: productSearch.value })
}
function addProduct(p: any) {
  if (form.items.some((x: any) => x.productId === p.id)) return
  form.items.push({ productId: p.id, quantity: 1, productTitle: p.title, stockQuantity: p.stockQuantity, priceIqd: p.priceIqd })
  if (!form.originalPriceIqd) recalcOriginal()
}
function removeProduct(id: string) {
  form.items = form.items.filter((x: any) => x.productId !== id) as any
  recalcOriginal()
}
function recalcOriginal() {
  form.originalPriceIqd = form.items.reduce((sum: number, x: any) => sum + Number(x.priceIqd || 0) * Number(x.quantity || 1), 0)
}
watch(() => form.items.map((x: any) => x.quantity).join(','), recalcOriginal)
async function uploadCover(e: Event) {
  const file = (e.target as HTMLInputElement).files?.[0]
  if (!file) return
  const fd = new FormData()
  fd.append('file', file)
  const res = await api.postForm<any>('/admin/packages/upload', fd)
  form.coverUrl = res.url
  form.mediaType = res.mediaType || 'image'
}
async function savePackage() {
  error.value = ''
  saving.value = true
  try {
    const payload = { ...form, items: form.items.map((x: any) => ({ productId: x.productId, quantity: Number(x.quantity || 1) })) }
    if (form.id) await api.put(`/admin/packages/${form.id}`, payload)
    else await api.post('/admin/packages', payload)
    resetForm()
    await loadPackages()
  } catch (e: any) {
    error.value = e?.friendlyMessage || e?.message || 'فشل حفظ البكج'
  } finally { saving.value = false }
}
function editPackage(pkg: any) {
  Object.assign(form, emptyForm(), {
    id: pkg.id, nameAr: pkg.nameAr, nameEn: pkg.nameEn, slug: pkg.slug, shortDescription: pkg.shortDescription, seoDescription: pkg.seoDescription, note: pkg.note,
    coverUrl: pkg.coverUrl, mediaType: pkg.mediaType, originalPriceIqd: pkg.originalPriceIqd, finalPriceIqd: pkg.finalPriceIqd, category: pkg.category, problemCategory: pkg.problemCategory,
    isPublished: pkg.isPublished, isFeatured: pkg.isFeatured, showInSlider: pkg.showInSlider, sliderPlacement: pkg.sliderPlacement, sortOrder: pkg.sortOrder,
    items: (pkg.items || []).map((i: any) => ({ productId: i.productId, quantity: i.quantity, productTitle: i.productTitle, stockQuantity: i.stockQuantity, priceIqd: i.priceIqd }))
  })
  window.scrollTo({ top: 0, behavior: 'smooth' })
}
async function deletePackage(id: string) {
  if (!confirm('حذف البكج؟')) return
  await api.del(`/admin/packages/${id}`)
  await loadPackages()
}
function resetForm() { Object.assign(form, emptyForm()) }

onMounted(async () => { await Promise.all([loadPackages(), searchProducts()]) })
</script>

<style scoped>
.field { display: grid; gap: .45rem; font-weight: 800; }
.field span { color: rgb(var(--muted)); font-size: .86rem; }
.input { width: 100%; border-radius: 1.25rem; border: 1px solid rgba(255,255,255,.12); background: rgba(255,255,255,.05); padding: .8rem 1rem; outline: none; }
.check { display: flex; gap: .55rem; align-items: center; border: 1px solid rgba(255,255,255,.1); border-radius: 1.25rem; padding: .85rem 1rem; }
.btn-primary,.btn-secondary,.btn-danger{border-radius:1.1rem;padding:.75rem 1rem;font-weight:900}
.btn-primary{background:rgb(var(--primary));color:#111827}.btn-secondary{border:1px solid rgba(255,255,255,.14);background:rgba(255,255,255,.06)}.btn-danger{border:1px solid rgba(239,68,68,.35);background:rgba(239,68,68,.12);color:#fecaca}.badge{border:1px solid rgba(255,255,255,.12);border-radius:999px;padding:.25rem .65rem;font-size:.75rem}.badge.ok{background:rgba(34,197,94,.14);color:#bbf7d0}.badge.bad{background:rgba(239,68,68,.14);color:#fecaca}
</style>
