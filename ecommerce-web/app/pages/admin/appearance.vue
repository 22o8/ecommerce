<template>
  <div class="appearance-admin space-y-6">
    <section class="admin-panel appearance-hero">
      <div>
        <div class="eyebrow rtl-text">هوية المتجر</div>
        <h1 class="rtl-text">الثيمات، الشعار، واستفتاحية الموقع</h1>
        <p class="rtl-text">غيّر شكل المتجر من مكان واحد: شعار الموقع، فيديو البداية، العنوان، زر ابدأ الآن، والثيمات الموسمية.</p>
      </div>
      <button class="admin-primary px-5 py-3" @click="save" :disabled="saving">
        {{ saving ? 'جارِ الحفظ...' : 'حفظ كل التغييرات' }}
      </button>
    </section>

    <section class="grid gap-6 xl:grid-cols-[minmax(420px,560px)_1fr]">
      <div class="space-y-6">
        <section class="admin-panel card-section">
          <div class="section-head">
            <div>
              <h2 class="rtl-text">شعار الموقع</h2>
              <p class="rtl-text">الشعار يظهر في الهيدر والفوتر. استخدم صورة مربعة أو شفافة.</p>
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
              <UiInput v-model="draft.siteLogoUrl" placeholder="https://..." dir="ltr" />
              <button type="button" class="admin-secondary w-max px-3 py-2" @click="draft.siteLogoUrl = ''">استخدام الشعار الافتراضي</button>
            </div>
          </div>
        </section>

        <section class="admin-panel card-section">
          <div class="section-head">
            <div>
              <h2 class="rtl-text">استفتاحية الموقع</h2>
              <p class="rtl-text">صفحة بداية اختيارية تظهر للزائر بفيديو وعنوان وزر ابدأ الآن. يمكن تعطيلها بأي وقت.</p>
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
              <small>يفضل فيديو قصير ومضغوط حتى لا يبطئ الموقع.</small>
            </label>
            <UiInput v-model="draft.intro.videoUrl" placeholder="رابط الفيديو أو ارفعه من الأعلى" dir="ltr" />
            <UiInput v-model="draft.intro.title" placeholder="العنوان الرئيسي مثل: اكتشفي جمالك بثقة" />
            <UiInput v-model="draft.intro.subtitle" placeholder="وصف قصير يظهر تحت العنوان" />
            <div class="grid gap-3 sm:grid-cols-2">
              <UiInput v-model="draft.intro.buttonText" placeholder="نص الزر: ابدأ الآن" />
              <UiInput v-model="draft.intro.buttonUrl" placeholder="/products" dir="ltr" />
            </div>
          </div>
        </section>
      </div>

      <div class="space-y-6">
        <section class="admin-panel card-section">
          <div class="section-head">
            <div>
              <h2 class="rtl-text">معاينة الاستفتاحية</h2>
              <p class="rtl-text">هذه معاينة تقريبية لما سيظهر للزائر عند التفعيل.</p>
            </div>
          </div>
          <div class="intro-preview">
            <video v-if="introVideoPreview" :src="introVideoPreview" autoplay muted loop playsinline />
            <div class="intro-preview__overlay" />
            <div class="intro-preview__content">
              <span>Beauty Store</span>
              <h3>{{ draft.intro.title || 'اكتشفي جمالك بثقة' }}</h3>
              <p>{{ draft.intro.subtitle || 'منتجات مختارة بعناية لتجربة كوزمتك أنيقة وسريعة.' }}</p>
              <button>{{ draft.intro.buttonText || 'ابدأ الآن' }}</button>
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
  { key: 'rosesEdge', label: 'الشكل الثاني', hint: 'ورود وردية ثابتة على أطراف الصفحة في الثيمين.' },
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
  },
})

const logoPreview = computed(() => draft.siteLogoUrl ? buildAssetUrl(draft.siteLogoUrl) : '')
const introVideoPreview = computed(() => draft.intro.videoUrl ? buildAssetUrl(draft.intro.videoUrl) : '')

async function uploadFile(file: File) {
  const fd = new FormData()
  fd.append('file', file)
  const res: any = await $fetch('/api/bff/admin/appearance/upload', { method: 'POST', body: fd })
  return res?.url?.url || res?.url || ''
}

async function uploadLogo(e: Event) {
  const file = (e.target as HTMLInputElement)?.files?.[0]
  if (!file) return
  uploadingLogo.value = true
  try {
    draft.siteLogoUrl = await uploadFile(file)
    toast.success('تم رفع الشعار')
  } catch { toast.error('تعذر رفع الشعار') }
  finally { uploadingLogo.value = false; (e.target as HTMLInputElement).value = '' }
}

async function uploadIntroVideo(e: Event) {
  const file = (e.target as HTMLInputElement)?.files?.[0]
  if (!file) return
  uploadingVideo.value = true
  try {
    draft.intro.videoUrl = await uploadFile(file)
    toast.success('تم رفع الفيديو')
  } catch { toast.error('تعذر رفع الفيديو') }
  finally { uploadingVideo.value = false; (e.target as HTMLInputElement).value = '' }
}

async function save() {
  saving.value = true
  try {
    const enabledEffects = Object.entries(draft.effects)
      .filter(([, v]) => !!v)
      .map(([k]) => k)

    const payload = {
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
    }

    await $fetch('/api/bff/admin/appearance', { method: 'POST', body: payload })
    await store.refresh()
    toast.success('تم حفظ الهوية')
  } catch {
    toast.error('فشل الحفظ')
  } finally {
    saving.value = false
  }
}
</script>

<style scoped>
.appearance-hero{ display:flex; align-items:flex-start; justify-content:space-between; gap:1rem; padding:1.35rem; border-radius:30px; }
.eyebrow{ display:inline-flex; border:1px solid rgba(var(--primary),.35); background:rgba(var(--primary),.08); color:rgb(var(--primary)); border-radius:999px; padding:.35rem .7rem; font-size:.72rem; font-weight:1000; }
.appearance-hero h1{ margin-top:.8rem; font-size:clamp(1.7rem,3vw,2.5rem); font-weight:1000; color:rgb(var(--text)); letter-spacing:-.04em; }
.appearance-hero p{ margin-top:.4rem; max-width:760px; color:rgb(var(--muted)); line-height:1.9; }
.card-section{ border-radius:30px; padding:1.15rem; }
.section-head{ display:flex; align-items:flex-start; justify-content:space-between; gap:1rem; margin-bottom:1rem; }
.section-head h2,.card-section h2{ font-size:1.18rem; font-weight:1000; color:rgb(var(--text)); }
.section-head p{ color:rgb(var(--muted)); font-size:.86rem; line-height:1.8; margin-top:.2rem; }
.logo-editor{ display:flex; gap:1rem; align-items:center; }
.logo-preview{ width:112px; height:112px; border-radius:30px; border:1px solid rgba(var(--border),.9); background:linear-gradient(180deg, rgba(var(--surface-rgb),.96), rgba(var(--surface-2-rgb),.82)); display:grid; place-items:center; overflow:hidden; flex:0 0 auto; box-shadow:var(--shadow-soft); }
.logo-preview img{ width:100%; height:100%; object-fit:cover; }
.upload-zone{ display:grid; place-items:center; gap:.35rem; min-height:126px; border:1px dashed rgba(var(--primary), .44); background:linear-gradient(180deg, rgba(var(--primary), .08), rgba(var(--surface-rgb), .45)); border-radius:24px; cursor:pointer; text-align:center; color:rgb(var(--text)); }
.upload-zone.compact{ min-height:78px; }
.upload-zone span{ font-weight:1000; } .upload-zone small{ color:rgb(var(--muted)); }
.switch-row{ display:flex; align-items:center; gap:.55rem; border:1px solid rgba(var(--border),.9); border-radius:999px; background:rgba(var(--surface-2-rgb),.7); padding:.65rem .85rem; color:rgb(var(--text)); font-weight:900; }
.intro-preview{ position:relative; min-height:360px; overflow:hidden; border-radius:32px; border:1px solid rgba(var(--border),.9); background:#050509; display:grid; place-items:center; }
.intro-preview video{ position:absolute; inset:0; width:100%; height:100%; object-fit:cover; opacity:.72; }
.intro-preview__overlay{ position:absolute; inset:0; background:radial-gradient(circle at 70% 20%, rgba(var(--primary),.32), transparent 34%), linear-gradient(90deg, rgba(0,0,0,.82), rgba(0,0,0,.42), rgba(0,0,0,.74)); }
.intro-preview__content{ position:relative; width:min(88%,620px); color:white; border:1px solid rgba(255,255,255,.14); border-radius:30px; padding:2rem; background:rgba(8,8,14,.46); backdrop-filter:blur(14px); }
.intro-preview__content span{ display:inline-flex; border:1px solid rgba(255,255,255,.18); border-radius:999px; padding:.35rem .7rem; color:rgba(255,255,255,.78); font-size:.72rem; font-weight:900; }
.intro-preview__content h3{ margin-top:1rem; font-size:clamp(1.9rem,4vw,4rem); line-height:1; font-weight:1000; }
.intro-preview__content p{ margin-top:1rem; color:rgba(255,255,255,.78); line-height:1.9; }
.intro-preview__content button{ margin-top:1.4rem; min-height:48px; border-radius:999px; background:rgb(var(--primary)); color:#050509; font-weight:1000; padding:0 1.2rem; }
.option-card{ display:flex; gap:.7rem; align-items:flex-start; border:1px solid rgba(var(--border),.86); background:rgba(var(--surface-2-rgb),.55); border-radius:20px; padding:.85rem; cursor:pointer; }
.option-card b{ display:block; color:rgb(var(--text)); } .option-card small{ color:rgb(var(--muted)); display:block; margin-top:.15rem; line-height:1.5; }
@media(max-width:780px){ .appearance-hero,.section-head,.logo-editor{ flex-direction:column; } .logo-preview{ width:96px; height:96px; } }
</style>
