<template>
  <div class="space-y-6">
    <div class="flex flex-wrap items-center justify-between gap-3">
      <div>
        <h1 class="text-2xl font-extrabold text-[rgb(var(--text))] rtl-text">الثيمات وهوية الموقع</h1>
        <p class="mt-1 text-sm text-[rgb(var(--muted))] rtl-text">غيّر شعار الموقع، صفحة البداية، والتأثيرات العامة من مكان واحد.</p>
      </div>
      <button class="admin-primary px-5 py-3" :disabled="saving || uploadingLogo || uploadingIntro" @click="save">
        {{ saving ? 'جارِ الحفظ...' : 'حفظ التغييرات' }}
      </button>
    </div>

    <div class="grid gap-6 xl:grid-cols-[420px_1fr]">
      <section class="admin-panel p-5">
        <div class="flex items-start justify-between gap-3">
          <div>
            <h2 class="text-lg font-extrabold rtl-text">شعار الموقع</h2>
            <p class="mt-1 text-sm text-[rgb(var(--muted))] rtl-text">ارفع Logo جديد يظهر في الهيدر بدل الشعار الافتراضي.</p>
          </div>
          <div class="grid h-16 w-16 place-items-center overflow-hidden rounded-3xl border border-app bg-surface-2">
            <img v-if="draft.siteLogoUrl" :src="buildAssetUrl(draft.siteLogoUrl)" class="h-full w-full object-cover" />
            <Icon v-else name="mdi:shopping-outline" class="text-2xl text-[rgb(var(--primary))]" />
          </div>
        </div>

        <label class="mt-5 upload-zone">
          <input type="file" accept="image/*" class="hidden" @change="onLogoFile" />
          <span>{{ uploadingLogo ? 'جاري رفع الشعار...' : 'رفع شعار جديد' }}</span>
          <small>يفضل صورة مربعة PNG/JPG</small>
        </label>
        <UiInput v-model="draft.siteLogoUrl" class="mt-3" placeholder="https://..." dir="ltr" />
      </section>

      <section class="admin-panel p-5">
        <div class="flex flex-wrap items-start justify-between gap-3">
          <div>
            <h2 class="text-lg font-extrabold rtl-text">استفتاحية الموقع</h2>
            <p class="mt-1 text-sm text-[rgb(var(--muted))] rtl-text">صفحة بداية اختيارية بفيديو ونص وزر "ابدأ الآن"، ويمكن تعطيلها بأي وقت.</p>
          </div>
          <label class="flex items-center gap-2 rounded-2xl border border-app bg-surface-2 px-3 py-2 font-bold">
            <input v-model="draft.intro.enabled" type="checkbox" class="h-4 w-4" /> تفعيل
          </label>
        </div>

        <div class="mt-5 grid gap-4 lg:grid-cols-2">
          <div class="grid gap-3">
            <label class="admin-label">العنوان</label>
            <UiInput v-model="draft.intro.title" placeholder="اكتشفي جمالك بثقة" />
            <label class="admin-label">الوصف</label>
            <textarea v-model="draft.intro.subtitle" class="admin-input min-h-[112px] p-3" placeholder="نص قصير يظهر في صفحة البداية"></textarea>
            <label class="admin-label">نص الزر</label>
            <UiInput v-model="draft.intro.buttonText" placeholder="ابدأ الآن" />
          </div>
          <div class="grid gap-3">
            <label class="admin-label">فيديو البداية</label>
            <label class="upload-zone min-h-[150px]">
              <input type="file" accept="video/*" class="hidden" @change="onIntroVideo" />
              <span>{{ uploadingIntro ? 'جاري رفع الفيديو...' : 'رفع فيديو' }}</span>
              <small>يمكن أيضاً وضع رابط الفيديو يدوياً</small>
            </label>
            <UiInput v-model="draft.intro.videoUrl" placeholder="https://..." dir="ltr" />
            <video v-if="draft.intro.videoUrl" :src="buildAssetUrl(draft.intro.videoUrl)" class="h-40 w-full rounded-3xl border border-app object-cover" muted controls />
          </div>
        </div>
      </section>
    </div>

    <div class="grid gap-6 lg:grid-cols-2">
      <section class="admin-panel p-5">
        <h2 class="font-extrabold text-[rgb(var(--text))] rtl-text">الثيمات الموسمية</h2>
        <p class="mt-1 text-sm text-[rgb(var(--muted))] rtl-text">تقدر تفعل أكثر من ثيم بنفس الوقت.</p>
        <div class="mt-4 grid sm:grid-cols-2 gap-3">
          <label v-for="opt in themeOptions" :key="opt.key" class="setting-tile">
            <input type="checkbox" class="h-5 w-5" v-model="draft.themes" :value="opt.key" />
            <div><b>{{ t(opt.labelKey) }}</b><small>{{ t(opt.hintKey) }}</small></div>
          </label>
        </div>
      </section>

      <section class="admin-panel p-5">
        <h2 class="font-extrabold text-[rgb(var(--text))] rtl-text">تأثيرات الخلفية</h2>
        <p class="mt-1 text-sm text-[rgb(var(--muted))] rtl-text">تأثيرات مرئية خفيفة تظهر للزبائن.</p>
        <div class="mt-4 grid sm:grid-cols-2 gap-3">
          <label v-for="e in effectOptions" :key="e.key" class="setting-tile">
            <input type="checkbox" class="h-5 w-5" v-model="draft.effects[e.key]" />
            <div><b>{{ e.label || t(e.labelKey) }}</b><small>{{ e.hint || t(e.hintKey) }}</small></div>
          </label>
        </div>
      </section>
    </div>
  </div>
</template>

<script setup lang="ts">
definePageMeta({ layout: 'admin', middleware: ['admin'] })
import UiInput from '~/components/ui/UiInput.vue'

const { t } = useI18n()
const store = useAppearanceStore()
const toast = useToast()
const { buildAssetUrl } = useApi()
if (!store.loaded) await store.refresh()

const saving = ref(false)
const uploadingLogo = ref(false)
const uploadingIntro = ref(false)

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
  { key: 'rosesEdge', label: 'ورود على الأطراف', hint: 'ورود وردية ثابتة على أطراف الصفحة.' },
]

const draft = reactive({
  themes: [...(store.data.themes || [])],
  effects: { ...(store.data.effects || {}) } as Record<string, boolean>,
  siteLogoUrl: store.data.siteLogoUrl || '',
  intro: {
    enabled: !!store.data.intro?.enabled,
    title: store.data.intro?.title || 'اكتشفي جمالك بثقة',
    subtitle: store.data.intro?.subtitle || 'منتجات كوزمتك مختارة بعناية لتوصلك للروتين المناسب بسرعة.',
    videoUrl: store.data.intro?.videoUrl || '',
    buttonText: store.data.intro?.buttonText || 'ابدأ الآن',
  },
})

async function uploadFile(file: File, target: 'logo' | 'intro') {
  const fd = new FormData()
  fd.append('file', file)
  const res: any = await $fetch('/api/bff/admin/appearance/upload', { method: 'POST', body: fd })
  return res?.url?.url || res?.url || ''
}

async function onLogoFile(e: Event) {
  const file = (e.target as HTMLInputElement).files?.[0]
  if (!file) return
  uploadingLogo.value = true
  try { draft.siteLogoUrl = await uploadFile(file, 'logo'); toast.success('تم رفع الشعار') }
  catch { toast.error('تعذر رفع الشعار') }
  finally { uploadingLogo.value = false; (e.target as HTMLInputElement).value = '' }
}

async function onIntroVideo(e: Event) {
  const file = (e.target as HTMLInputElement).files?.[0]
  if (!file) return
  uploadingIntro.value = true
  try { draft.intro.videoUrl = await uploadFile(file, 'intro'); toast.success('تم رفع الفيديو') }
  catch { toast.error('تعذر رفع الفيديو') }
  finally { uploadingIntro.value = false; (e.target as HTMLInputElement).value = '' }
}

async function save() {
  saving.value = true
  try {
    const enabledThemes = draft.themes
    const enabledEffects = Object.entries(draft.effects).filter(([, v]) => !!v).map(([k]) => k)
    await $fetch('/api/bff/admin/appearance', {
      method: 'POST',
      body: {
        isActive: true,
        enabledThemes,
        enabledEffects,
        ads: [],
        siteLogoUrl: draft.siteLogoUrl || null,
        introEnabled: draft.intro.enabled,
        introTitle: draft.intro.title || null,
        introSubtitle: draft.intro.subtitle || null,
        introVideoUrl: draft.intro.videoUrl || null,
        introButtonText: draft.intro.buttonText || 'ابدأ الآن',
      },
    })
    await store.refresh()
    toast.success('تم الحفظ')
  } catch { toast.error('فشل الحفظ') }
  finally { saving.value = false }
}
</script>

<style scoped>
.upload-zone{ display:grid; place-items:center; gap:6px; min-height:112px; border:1px dashed rgba(var(--primary), .42); background:rgba(var(--primary), .07); border-radius:24px; cursor:pointer; text-align:center; color:rgb(var(--text)); }
.upload-zone span{ font-weight:1000; } .upload-zone small{ color:rgb(var(--muted)); }
.setting-tile{ display:flex; align-items:flex-start; gap:12px; cursor:pointer; border:1px solid rgba(var(--border), .9); background:rgba(var(--surface-2-rgb), .7); border-radius:22px; padding:15px; transition:.2s ease; }
.setting-tile:hover{ transform:translateY(-2px); border-color:rgba(var(--primary), .32); }
.setting-tile b{ display:block; color:rgb(var(--text)); } .setting-tile small{ display:block; margin-top:4px; color:rgb(var(--muted)); line-height:1.6; }
</style>
