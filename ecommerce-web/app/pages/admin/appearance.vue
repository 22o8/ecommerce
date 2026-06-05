<template>
  <div class="appearance-admin space-y-6">
    <section class="admin-panel appearance-hero">
      <div>
        <div class="eyebrow rtl-text">هوية المتجر</div>
        <h1 class="rtl-text">الشعار، الاستفتاحية، والثيمات</h1>
        <p class="rtl-text">قسّمنا الصفحة حتى يكون الشعار دائم ومستقل، والاستفتاحية اختيارية يمكن تفعيلها أو حذفها بأي وقت.</p>
      </div>
      <div class="flex flex-wrap gap-2 justify-end">
        <NuxtLink to="/admin/identity" class="admin-secondary px-5 py-3">تغيير الشعار</NuxtLink>
        <button class="admin-primary px-5 py-3" @click="save" :disabled="saving">
          {{ saving ? 'جارِ الحفظ...' : 'حفظ كل التغييرات' }}
        </button>
      </div>
    </section>

    <section class="grid gap-6 xl:grid-cols-[minmax(420px,600px)_1fr]">
      <div class="space-y-6">
        <section class="admin-panel card-section identity-card">
          <div class="section-head">
            <div>
              <span class="mini-label">قسم مستقل</span>
              <h2 class="rtl-text">شعار الموقع الدائم</h2>
              <p class="rtl-text">هذا الشعار يظهر في الهيدر والفوتر دائماً ولا علاقة له بالاستفتاحية.</p>
            </div>
          </div>
          <div class="logo-editor">
            <div class="logo-preview">
              <img v-if="logoPreview" :src="logoPreview" alt="logo" />
              <Icon v-else name="mdi:storefront-outline" class="text-4xl text-[rgb(var(--muted))]" />
            </div>
            <div class="grid flex-1 gap-3">
              <label class="upload-zone compact">
                <input type="file" accept="image/*" class="hidden" @change="uploadLogo" />
                <Icon name="mdi:image-plus-outline" class="text-2xl text-[rgb(var(--primary))]" />
                <span>{{ uploadingLogo ? 'جاري رفع الشعار...' : 'رفع شعار جديد' }}</span>
              </label>
              <UiInput v-model="draft.siteLogoUrl" placeholder="رابط الشعار الدائم" dir="ltr" />
              <div class="flex flex-wrap gap-2">
                <button type="button" class="admin-secondary px-3 py-2" @click="draft.siteLogoUrl = ''">الشعار الافتراضي</button>
                <button type="button" class="admin-primary px-3 py-2" @click="save" :disabled="saving">حفظ الشعار</button>
              </div>
            </div>
          </div>
        </section>

        <section class="admin-panel card-section intro-builder">
          <div class="section-head">
            <div>
              <span class="mini-label">Intro Builder</span>
              <h2 class="rtl-text">استفتاحية الموقع</h2>
              <p class="rtl-text">صفحة بداية احترافية تظهر مرة للزائر بفيديو وخطاب تسويقي وزرين.</p>
            </div>
            <label class="switch-row">
              <input type="checkbox" v-model="draft.intro.enabled" />
              <span>{{ draft.intro.enabled ? 'مفعلة' : 'معطلة' }}</span>
            </label>
          </div>

          <div class="grid gap-4">
            <label class="upload-zone">
              <input type="file" accept="video/*" class="hidden" @change="uploadIntroVideo" />
              <Icon name="mdi:video-plus-outline" class="text-3xl text-[rgb(var(--primary))]" />
              <span>{{ uploadingVideo ? 'جاري رفع الفيديو...' : 'رفع فيديو الاستفتاحية' }}</span>
              <small>رفع مباشر إلى الباك/R2 لتجاوز حد Vercel. يقبل MP4، أو ضع رابط فيديو مباشر.</small>
            </label>
            <UiInput v-model="draft.intro.videoUrl" placeholder="رابط MP4 مباشر أو YouTube/Instagram/TikTok" dir="ltr" />
            <div class="intro-note rtl-text">ملاحظة: روابط Instagram/Reels ليست ملف فيديو مباشر، لذلك نعرضها كـ iframe إن أمكن. أفضل حل للفيديو التجاري هو رفع MP4 مباشر إلى R2 أو استخدام رابط MP4.</div>
            <UiInput v-model="draft.intro.title" placeholder="العنوان الرئيسي مثل: Dr.Seoul Beauty" />
            <UiInput v-model="draft.intro.subtitle" placeholder="وصف قصير:Korean Skincare Iraq🇰🇷" />
            <div class="grid gap-3 sm:grid-cols-2">
              <UiInput v-model="draft.intro.buttonText" placeholder="زر رئيسي: ابدأ الآن" />
              <UiInput v-model="draft.intro.buttonUrl" placeholder="/products" dir="ltr" />
            </div>
            <div class="grid gap-3 sm:grid-cols-2">
              <UiInput v-model="draft.intro.secondaryButtonText" placeholder="زر ثاني: تصفح البراندات" />
              <UiInput v-model="draft.intro.secondaryButtonUrl" placeholder="/brands" dir="ltr" />
            </div>
            <div class="intro-note rtl-text">
              يمكنك تعطيل الاستفتاحية بأي وقت بدون حذف الفيديو أو النصوص، وستبقى الإعدادات محفوظة للعودة لها لاحقاً.
            </div>
            <div class="intro-link-box">
              <div class="min-w-0">
                <b class="rtl-text">رابط الاستفتاحية للنشر</b>
                <p class="keep-ltr">{{ introPublicUrl }}</p>
              </div>
              <button type="button" class="admin-secondary px-4 py-3" @click="copyIntroLink">نسخ الرابط</button>
            </div>
          </div>
        </section>
      </div>

      <div class="space-y-6">
        <section class="admin-panel card-section">
          <div class="section-head">
            <div>
              <h2 class="rtl-text">معاينة الاستفتاحية</h2>
              <p class="rtl-text">المعاينة تعطيك شكل قريب من تجربة الزائر.</p>
            </div>
          </div>
          <div class="intro-preview" :class="draft.intro.enabled ? 'is-enabled' : 'is-disabled'">
            <iframe
              v-if="introEmbedUrl"
              class="intro-preview__media"
              :src="introEmbedUrl"
              allow="autoplay; encrypted-media; picture-in-picture"
              allowfullscreen
            />
            <video v-else-if="introVideoPreview" class="intro-preview__media" :src="introVideoPreview" autoplay muted loop playsinline />
            <div v-else class="intro-preview__fallback" />
            <div class="intro-preview__overlay" />
            <div class="intro-preview__content rtl-text">
              <span>Beauty Store</span>
              <h3>{{ draft.intro.title || 'Dr.Seoul Beauty' }}</h3>
              <p>{{ draft.intro.subtitle || 'منتجات مختارة بعناية لتجربة كوزمتك أنيقة ' }}</p>
              <div class="intro-preview__actions">
                <button>{{ draft.intro.buttonText || 'ابدأ الآن' }}</button>
                <button class="ghost">{{ draft.intro.secondaryButtonText || 'تصفح البراندات' }}</button>
              </div>
            </div>
          </div>
        </section>

        <section class="grid gap-6 lg:grid-cols-2">
          <div class="admin-panel card-section">
            <h2 class="rtl-text">الثيمات</h2>
            <p class="mt-1 text-sm text-[rgb(var(--muted))] rtl-text">تقدر تفعل أكثر من ثيم بنفس الوقت.</p>
            <div class="mt-4 grid gap-3">
              <label v-for="opt in themeOptions" :key="opt.key" class="option-card">
                <input type="checkbox" v-model="draft.themes" :value="opt.key" />
                <div>
                  <b>{{ t(opt.labelKey) }}</b>
                  <small>{{ t(opt.hintKey) }}</small>
                </div>
              </label>
            </div>
          </div>

          <div class="admin-panel card-section">
            <h2 class="rtl-text">تأثيرات الخلفية</h2>
            <p class="mt-1 text-sm text-[rgb(var(--muted))] rtl-text">تأثيرات اختيارية تظهر على المتجر.</p>
            <div class="mt-4 grid gap-3">
              <label v-for="e in effectOptions" :key="e.key" class="option-card">
                <input type="checkbox" v-model="draft.effects[e.key]" />
                <div>
                  <b>{{ e.label || t(e.labelKey) }}</b>
                  <small>{{ e.hint || t(e.hintKey) }}</small>
                </div>
              </label>
            </div>
          </div>
        </section>
      </div>
    </section>
  </div>
</template>

<script setup lang="ts">
definePageMeta({ layout: 'admin', middleware: ['admin'] })
import UiInput from '~/components/ui/UiInput.vue'

const { t } = useI18n()
const store = useAppearanceStore()
const { buildAssetUrl } = useApi()
if (!store.loaded) await store.fetchAdminAppearance()

const saving = ref(false)
const uploadingLogo = ref(false)
const uploadingVideo = ref(false)
const toast = useToast()
const directUpload = useDirectAdminUpload()

const themeOptions = [
  { key: 'ramadan', labelKey: 'season.ramadan', hintKey: 'seasonHints.ramadan' },
  { key: 'eid', labelKey: 'season.eid', hintKey: 'seasonHints.eid' },
  { key: 'christmas', labelKey: 'season.christmas', hintKey: 'seasonHints.christmas' },
  { key: 'valentines', labelKey: 'season.valentines', hintKey: 'seasonHints.valentines' },
  { key: 'blackFriday', labelKey: 'season.blackFriday', hintKey: 'seasonHints.blackFridayTheme' },
]
const effectOptions = [
  { key: 'snow', labelKey: 'season.snow', hintKey: 'seasonHints.snow' },
  { key: 'ramadan', labelKey: 'season.ramadan', hintKey: 'seasonHints.ramadanEffect' },
  { key: 'eid', labelKey: 'season.eid', hintKey: 'seasonHints.eidEffect' },
  { key: 'christmas', labelKey: 'season.christmas', hintKey: 'seasonHints.christmasEffect' },
  { key: 'valentines', labelKey: 'season.valentines', hintKey: 'seasonHints.valentinesEffect' },
  { key: 'blackFriday', labelKey: 'season.blackFriday', hintKey: 'seasonHints.blackFridayEffect' },
  { key: 'rosesEdge', label: 'الشكل الثاني', hint: 'ورود وردية ثابتة على أطراف الصفحة.' },
]

type Draft = {
  themes: string[]
  effects: Record<string, boolean>
  siteLogoUrl: string
  intro: {
    enabled: boolean
    videoUrl: string
    title: string
    subtitle: string
    buttonText: string
    buttonUrl: string
    secondaryButtonText: string
    secondaryButtonUrl: string
  }
}

const draft = reactive<Draft>({
  themes: [...(store.data.themes || [])],
  effects: { ...(store.data.effects || {}) },
  siteLogoUrl: store.data.siteLogoUrl || '',
  intro: {
    enabled: Boolean(store.data.intro?.enabled),
    videoUrl: store.data.intro?.videoUrl || '',
    title: store.data.intro?.title || '',
    subtitle: store.data.intro?.subtitle || '',
    buttonText: store.data.intro?.buttonText || 'ابدأ الآن',
    buttonUrl: store.data.intro?.buttonUrl || '/products',
    secondaryButtonText: (store.data.intro as any)?.secondaryButtonText || 'تصفح البراندات',
    secondaryButtonUrl: (store.data.intro as any)?.secondaryButtonUrl || '/brands',
  },
})

const logoPreview = computed(() => draft.siteLogoUrl ? buildAssetUrl(draft.siteLogoUrl) : '')
function directVideoUrl(raw?: string) {
  const v = String(raw || '').trim()
  if (!v) return ''
  if (isEmbeddableVideo(v)) return ''
  return buildAssetUrl(v)
}
function isEmbeddableVideo(v: string) {
  return /youtube\.com|youtu\.be|instagram\.com|tiktok\.com/i.test(v)
}
function toEmbedUrl(raw?: string) {
  const v = String(raw || '').trim()
  if (!v) return ''
  try {
    const u = new URL(v)
    const host = u.hostname.replace(/^www\./, '')
    if (host.includes('youtu.be')) return `https://www.youtube.com/embed/${u.pathname.replace('/', '')}?autoplay=1&mute=1&loop=1&playsinline=1`
    if (host.includes('youtube.com')) {
      const id = u.searchParams.get('v') || u.pathname.split('/').filter(Boolean).pop()
      return id ? `https://www.youtube.com/embed/${id}?autoplay=1&mute=1&loop=1&playsinline=1` : ''
    }
    if (host.includes('instagram.com')) {
      const parts = u.pathname.split('/').filter(Boolean)
      const type = parts[0] || 'reel'
      const code = parts[1]
      return code ? `https://www.instagram.com/${type}/${code}/embed` : ''
    }
    if (host.includes('tiktok.com')) return v
  } catch {}
  return ''
}
const introVideoPreview = computed(() => directVideoUrl(draft.intro.videoUrl))
const introEmbedUrl = computed(() => toEmbedUrl(draft.intro.videoUrl))
const introPublicUrl = computed(() => process.client ? `${window.location.origin}/intro` : '/intro')

async function copyIntroLink() {
  try {
    if (process.client) await navigator.clipboard.writeText(introPublicUrl.value)
    toast.success('تم نسخ رابط الاستفتاحية')
  } catch {
    toast.error('تعذر نسخ الرابط')
  }
}

async function uploadSmallAppearanceFile(file: File) {
  const fd = new FormData()
  fd.append('file', file)
  const res: any = await $fetch('/api/bff/admin/appearance/upload', { method: 'POST', body: fd })
  return res?.url?.url || res?.url || ''
}
async function uploadLargeAppearanceFile(file: File) {
  return await directUpload.upload('admin/appearance/upload', file, { maxMb: 150, fallbackToBff: false })
}
async function uploadLogo(e: Event) {
  const input = e.target as HTMLInputElement
  const file = input.files?.[0]
  if (!file) return
  uploadingLogo.value = true
  try { draft.siteLogoUrl = await uploadSmallAppearanceFile(file); toast.success('تم رفع الشعار') }
  catch (e:any) { toast.error(e?.data?.message || e?.message || 'تعذر رفع الشعار') }
  finally { uploadingLogo.value = false; input.value = '' }
}
async function uploadIntroVideo(e: Event) {
  const input = e.target as HTMLInputElement
  const file = input.files?.[0]
  if (!file) return
  uploadingVideo.value = true
  try { draft.intro.videoUrl = await uploadLargeAppearanceFile(file); toast.success('تم رفع الفيديو مباشرة إلى التخزين') }
  catch (e:any) { toast.error(e?.data?.message || e?.message || 'تعذر رفع الفيديو. استخدم ملف MP4 أو رابط مباشر') }
  finally { uploadingVideo.value = false; input.value = '' }
}
async function save() {
  saving.value = true
  try {
    const enabledEffects = Object.entries(draft.effects).filter(([, v]) => !!v).map(([k]) => k)
    await $fetch('/api/bff/admin/appearance', {
      method: 'POST',
      body: {
        isActive: true,
        enabledThemes: draft.themes,
        enabledEffects,
        ads: [],
        siteLogoUrl: draft.siteLogoUrl || null,
        introEnabled: draft.intro.enabled,
        introVideoUrl: draft.intro.videoUrl || null,
        introTitle: draft.intro.title || null,
        introSubtitle: draft.intro.subtitle || null,
        introButtonText: draft.intro.buttonText || 'ابدأ الآن',
        introButtonUrl: draft.intro.buttonUrl || '/products',
        introSecondaryButtonText: draft.intro.secondaryButtonText || 'تصفح البراندات',
        introSecondaryButtonUrl: draft.intro.secondaryButtonUrl || '/brands',
      },
    })
    await store.refresh()
    toast.success('تم حفظ إعدادات الهوية والاستفتاحية')
  } catch { toast.error('تعذر حفظ الإعدادات') }
  finally { saving.value = false }
}
</script>

<style scoped>
.appearance-hero{ display:flex; align-items:flex-start; justify-content:space-between; gap:1rem; padding:1.45rem; border-radius:30px; }
.appearance-hero h1{ margin-top:.7rem; font-size:clamp(1.9rem,4vw,3.4rem); font-weight:1000; letter-spacing:-.05em; color:rgb(var(--text)); }
.appearance-hero p{ margin-top:.55rem; color:rgb(var(--muted)); line-height:1.9; max-width:820px; }
.eyebrow,.mini-label{ display:inline-flex; border:1px solid rgba(var(--primary),.35); background:rgba(var(--primary),.08); color:rgb(var(--primary)); border-radius:999px; padding:.35rem .7rem; font-size:.72rem; font-weight:1000; }
.card-section{ padding:1.15rem; border-radius:30px; }
.section-head{ display:flex; align-items:flex-start; justify-content:space-between; gap:1rem; margin-bottom:1rem; }
.section-head h2{ margin-top:.45rem; color:rgb(var(--text)); font-size:1.25rem; font-weight:1000; }
.section-head p{ margin-top:.25rem; color:rgb(var(--muted)); font-size:.88rem; line-height:1.8; }
.logo-editor{ display:flex; gap:1rem; align-items:center; }
.logo-preview{ width:128px; height:128px; border-radius:32px; border:1px solid rgba(var(--border),.9); background:rgb(var(--surface-2)); display:grid; place-items:center; overflow:hidden; flex:0 0 auto; box-shadow:var(--shadow-soft); }
.logo-preview img{ width:100%; height:100%; object-fit:cover; }
.upload-zone{ display:grid; place-items:center; gap:.35rem; min-height:132px; border:1px dashed rgba(var(--primary), .44); background:linear-gradient(180deg, rgba(var(--primary), .08), rgba(var(--surface-rgb), .45)); border-radius:24px; cursor:pointer; text-align:center; color:rgb(var(--text)); }
.upload-zone.compact{ min-height:78px; }
.upload-zone small{ color:rgb(var(--muted)); }
.switch-row{ display:inline-flex; align-items:center; gap:.55rem; border:1px solid rgba(var(--border),.9); background:rgba(var(--surface-rgb),.72); border-radius:999px; padding:.65rem .9rem; font-weight:900; color:rgb(var(--text)); }
.intro-note{ border:1px solid rgba(var(--primary),.22); background:rgba(var(--primary),.08); color:rgb(var(--muted)); border-radius:20px; padding:.8rem 1rem; line-height:1.8; font-size:.86rem; }
.intro-link-box{ display:flex; align-items:center; justify-content:space-between; gap:1rem; border:1px solid rgba(var(--border),.9); background:rgba(var(--surface-2-rgb),.56); border-radius:22px; padding:.9rem 1rem; }
.intro-link-box b{ display:block; color:rgb(var(--text)); font-weight:1000; }
.intro-link-box p{ margin-top:.25rem; color:rgb(var(--muted)); font-size:.82rem; overflow:hidden; text-overflow:ellipsis; white-space:nowrap; }
.intro-preview{ position:relative; min-height:430px; overflow:hidden; border-radius:34px; border:1px solid rgba(var(--border),.9); background:#050509; display:grid; place-items:center; box-shadow:var(--shadow-soft); }
.intro-preview video,.intro-preview iframe,.intro-preview__media,.intro-preview__fallback{ position:absolute; inset:0; width:100%; height:100%; object-fit:cover; border:0; }
.intro-preview__fallback{ background:radial-gradient(circle at 70% 20%, rgba(var(--primary),.30), transparent 35%), radial-gradient(circle at 20% 80%, rgba(236,72,153,.22), transparent 42%), #050509; }
.intro-preview__overlay{ position:absolute; inset:0; background:linear-gradient(90deg, rgba(0,0,0,.86), rgba(0,0,0,.42), rgba(0,0,0,.72)); }
.intro-preview__content{ position:relative; width:min(88%,680px); color:white; border:1px solid rgba(255,255,255,.14); border-radius:32px; padding:2.2rem; background:rgba(8,8,14,.48); backdrop-filter:blur(16px); }
.intro-preview__content span{ display:inline-flex; border:1px solid rgba(255,255,255,.18); border-radius:999px; padding:.35rem .7rem; color:rgba(255,255,255,.78); font-size:.72rem; font-weight:900; }
.intro-preview__content h3{ margin-top:1rem; font-size:clamp(2rem,4vw,4.35rem); line-height:.98; font-weight:1000; letter-spacing:-.05em; }
.intro-preview__content p{ margin-top:1rem; color:rgba(255,255,255,.8); line-height:1.9; max-width:560px; }
.intro-preview__actions{ display:flex; flex-wrap:wrap; gap:.8rem; margin-top:1.5rem; }
.intro-preview__actions button{ min-height:48px; border-radius:999px; background:rgb(var(--primary)); color:#050509; font-weight:1000; padding:0 1.2rem; }
.intro-preview__actions .ghost{ background:rgba(255,255,255,.08); color:white; border:1px solid rgba(255,255,255,.18); }
.option-card{ display:flex; align-items:center; gap:.8rem; border:1px solid rgba(var(--border),.9); background:rgba(var(--surface-2-rgb),.52); border-radius:22px; padding:.85rem; color:rgb(var(--text)); }
.option-card b{ display:block; font-weight:1000; } .option-card small{ color:rgb(var(--muted)); line-height:1.6; }
@media(max-width:900px){ .appearance-hero,.section-head,.logo-editor{ flex-direction:column; } .logo-preview{ width:104px; height:104px; } .intro-preview{ min-height:360px; } }
</style>
