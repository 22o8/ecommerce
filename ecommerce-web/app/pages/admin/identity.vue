<template>
  <div class="admin-identity space-y-6">
    <section class="admin-panel identity-hero">
      <div>
        <div class="eyebrow rtl-text">هوية المتجر</div>
        <h1 class="rtl-text">الشعار الدائم وواجهة المتجر</h1>
        <p class="rtl-text">غيّر شعار المتجر من هنا بشكل مستقل. هذا الشعار يبقى ثابتاً بعد تحديث الصفحة ويظهر في الهيدر والفوتر.</p>
      </div>
      <div class="flex flex-wrap gap-2">
        <button class="admin-secondary px-4 py-3" type="button" @click="resetLogo">استخدام الافتراضي</button>
        <button class="admin-primary px-5 py-3" type="button" :disabled="saving || uploading" @click="saveIdentity">
          {{ saving ? 'جارِ الحفظ...' : 'حفظ الشعار' }}
        </button>
      </div>
    </section>

    <section class="grid gap-6 xl:grid-cols-[520px_1fr]">
      <div class="admin-panel identity-card">
        <h2 class="rtl-text">تغيير الشعار</h2>
        <p class="mt-2 text-sm leading-7 text-[rgb(var(--muted))] rtl-text">ارفع صورة مربعة أو شفافة. يفضل PNG أو WebP حتى يظهر الشعار بجودة عالية في الثيم الأبيض والأسود.</p>

        <div class="mt-6 logo-workspace">
          <div class="logo-preview-xl">
            <img v-if="logoPreview" :src="logoPreview" alt="Site logo" />
            <Icon v-else name="mdi:storefront-outline" class="text-5xl text-[rgb(var(--muted))]" />
          </div>

          <label class="upload-zone">
            <input type="file" accept="image/*" class="hidden" @change="uploadLogo" />
            <Icon name="mdi:image-plus-outline" class="text-3xl text-[rgb(var(--primary))]" />
            <span>{{ uploading ? 'جاري رفع الشعار...' : 'اضغط لرفع شعار جديد' }}</span>
            <small>بعد الرفع اضغط حفظ الشعار حتى يثبت دائماً.</small>
          </label>

          <div class="grid gap-2">
            <label class="admin-label">رابط الشعار الحالي</label>
            <UiInput v-model="logoUrl" placeholder="https://..." dir="ltr" />
          </div>
        </div>
      </div>

      <div class="admin-panel identity-preview">
        <div class="preview-navbar">
          <div class="preview-logo">
            <img v-if="logoPreview" :src="logoPreview" alt="preview logo" />
            <Icon v-else name="mdi:storefront-outline" />
          </div>
          <div class="preview-search rtl-text">ابحث عن منتج...</div>
          <div class="preview-pill rtl-text">السلة</div>
          <div class="preview-pill rtl-text">لوحة التحكم</div>
        </div>

        <div class="preview-hero rtl-text">
          <span>Beauty Store</span>
          <h3>معاينة ظهور الشعار</h3>
          <p>هذه معاينة تقريبية لكيف سيظهر الشعار داخل واجهة المتجر بعد الحفظ.</p>
        </div>

        <div class="identity-note rtl-text">
          إذا تغيّر الشعار هنا ولم يظهر في المتجر مباشرة، اضغط تحديث للمتجر مرة واحدة. بعدها سيبقى ثابتاً لأنه محفوظ في قاعدة البيانات وليس فقط في الواجهة.
        </div>
      </div>
    </section>
  </div>
</template>

<script setup lang="ts">
definePageMeta({ layout: 'admin', middleware: ['admin'] })
import UiInput from '~/components/ui/UiInput.vue'

const store = useAppearanceStore()
const { buildAssetUrl } = useApi()
const toast = useToast()
if (!store.loaded) await store.fetchAdminAppearance()

const logoUrl = ref(store.data.siteLogoUrl || '')
const saving = ref(false)
const uploading = ref(false)
const logoPreview = computed(() => logoUrl.value ? buildAssetUrl(logoUrl.value) : '')

async function uploadFile(file: File) {
  const fd = new FormData()
  fd.append('file', file)
  const res: any = await $fetch('/api/bff/admin/appearance/upload', { method: 'POST', body: fd })
  return res?.url?.url || res?.url || ''
}
async function uploadLogo(e: Event) {
  const input = e.target as HTMLInputElement
  const file = input.files?.[0]
  if (!file) return
  uploading.value = true
  try {
    logoUrl.value = await uploadFile(file)
    toast.success('تم رفع الشعار، اضغط حفظ حتى يثبت')
  } catch {
    toast.error('تعذر رفع الشعار')
  } finally {
    uploading.value = false
    input.value = ''
  }
}
function resetLogo() {
  logoUrl.value = ''
}
async function saveIdentity() {
  saving.value = true
  try {
    await $fetch('/api/bff/admin/appearance', {
      method: 'POST',
      body: {
        isActive: true,
        enabledThemes: store.data.themes || [],
        enabledEffects: Object.entries(store.data.effects || {}).filter(([, v]) => !!v).map(([k]) => k),
        ads: [],
        siteLogoUrl: logoUrl.value || null,
        introEnabled: Boolean(store.data.intro?.enabled),
        introVideoUrl: store.data.intro?.videoUrl || null,
        introTitle: store.data.intro?.title || null,
        introSubtitle: store.data.intro?.subtitle || null,
        introButtonText: store.data.intro?.buttonText || 'ابدأ الآن',
        introButtonUrl: store.data.intro?.buttonUrl || '/products',
        introSecondaryButtonText: store.data.intro?.secondaryButtonText || 'تصفح البراندات',
        introSecondaryButtonUrl: store.data.intro?.secondaryButtonUrl || '/brands',
      },
    })
    await store.refresh()
    logoUrl.value = store.data.siteLogoUrl || ''
    if (process.client) window.dispatchEvent(new CustomEvent('appearance:changed'))
    toast.success('تم حفظ الشعار بشكل دائم')
  } catch {
    toast.error('تعذر حفظ الشعار')
  } finally {
    saving.value = false
  }
}
</script>

<style scoped>
.identity-hero{ display:flex; align-items:flex-start; justify-content:space-between; gap:1rem; padding:1.35rem; border-radius:30px; }
.identity-hero h1{ margin-top:.7rem; color:rgb(var(--text)); font-size:clamp(2rem,4vw,3.3rem); font-weight:1000; letter-spacing:-.05em; }
.identity-hero p{ margin-top:.55rem; color:rgb(var(--muted)); line-height:1.9; max-width:760px; }
.eyebrow{ display:inline-flex; border:1px solid rgba(var(--primary),.35); background:rgba(var(--primary),.08); color:rgb(var(--primary)); border-radius:999px; padding:.35rem .7rem; font-size:.72rem; font-weight:1000; }
.identity-card,.identity-preview{ padding:1.2rem; border-radius:30px; }
.identity-card h2{ color:rgb(var(--text)); font-size:1.3rem; font-weight:1000; }
.logo-workspace{ display:grid; gap:1rem; }
.logo-preview-xl{ width:160px; height:160px; border-radius:38px; border:1px solid rgba(var(--border),.95); background:linear-gradient(180deg, rgba(var(--surface-rgb),.96), rgba(var(--surface-2-rgb),.85)); display:grid; place-items:center; overflow:hidden; box-shadow:var(--shadow-soft); }
.logo-preview-xl img{ width:100%; height:100%; object-fit:cover; }
.upload-zone{ display:grid; place-items:center; gap:.35rem; min-height:132px; border:1px dashed rgba(var(--primary), .44); background:linear-gradient(180deg, rgba(var(--primary), .08), rgba(var(--surface-rgb), .45)); border-radius:24px; cursor:pointer; text-align:center; color:rgb(var(--text)); }
.upload-zone small{ color:rgb(var(--muted)); }
.preview-navbar{ display:flex; align-items:center; gap:.75rem; border:1px solid rgba(var(--border),.9); background:rgba(var(--surface-2-rgb),.55); border-radius:26px; padding:.75rem; }
.preview-logo{ width:58px; height:58px; border-radius:20px; overflow:hidden; background:rgb(var(--surface)); display:grid; place-items:center; border:1px solid rgba(var(--border),.9); flex:0 0 auto; }
.preview-logo img{ width:100%; height:100%; object-fit:cover; }
.preview-search{ flex:1; border:1px solid rgba(var(--border),.75); background:rgb(var(--surface)); color:rgb(var(--muted)); border-radius:999px; padding:.85rem 1rem; }
.preview-pill{ border:1px solid rgba(var(--border),.75); border-radius:999px; padding:.75rem 1rem; color:rgb(var(--text)); font-weight:900; }
.preview-hero{ margin-top:1rem; min-height:320px; border-radius:32px; border:1px solid rgba(var(--border),.9); background:radial-gradient(circle at 80% 10%, rgba(var(--primary),.22), transparent 32%), linear-gradient(135deg, rgba(var(--surface-2-rgb),.9), rgba(var(--surface-rgb),.98)); display:grid; align-content:center; padding:2rem; }
.preview-hero span{ color:rgb(var(--primary)); font-size:.8rem; font-weight:1000; }
.preview-hero h3{ margin-top:.75rem; color:rgb(var(--text)); font-size:clamp(2rem,5vw,4rem); font-weight:1000; letter-spacing:-.05em; }
.preview-hero p{ margin-top:.75rem; color:rgb(var(--muted)); max-width:560px; line-height:1.9; }
.identity-note{ margin-top:1rem; border:1px solid rgba(var(--primary),.25); background:rgba(var(--primary),.08); color:rgb(var(--muted)); border-radius:22px; padding:1rem; line-height:1.9; }
@media(max-width:900px){ .identity-hero,.preview-navbar{ flex-direction:column; align-items:stretch; } .logo-preview-xl{ width:132px; height:132px; } }
</style>
