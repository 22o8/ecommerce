<!-- components/UploadImages.vue -->
<template>
  <div class="upload-box">
    <div class="top">
      <label class="btn">
        <input
          type="file"
          accept="image/*"
          multiple
          class="hidden"
          @change="onPick"
        />
        اختيار صور
      </label>

      <button class="btn ghost" :disabled="isUploading || !files.length" @click="uploadNow">
        {{ isUploading ? 'جاري الرفع...' : 'رفع الصور' }}
      </button>

      <button class="btn danger" :disabled="isUploading || (!files.length && !modelValue.length)" @click="clearAll">
        حذف الكل
      </button>
    </div>

    <p class="hint">
      - تظهر معاينة مباشرة بعد الاختيار. <br />
      - بعد الرفع، تتحول الروابط إلى روابط من السيرفر وتظهر بالفرونت.
    </p>

    <div v-if="errors.length" class="errors">
      <div v-for="(e, i) in errors" :key="i" class="err">{{ e }}</div>
    </div>

    <div class="grid">
      <!-- صور السيرفر (modelValue) -->
      <div v-for="(img, idx) in modelValue" :key="'server-'+idx" class="card">
        <img :src="assetUrl(img)" class="img" />
        <div class="actions">
          <button class="mini danger" :disabled="isUploading" @click="removeServer(idx)">حذف</button>
          <button class="mini" @click="copy(img)">نسخ الرابط</button>
        </div>
        <div class="meta">
          <span class="tag">Server</span>
          <span class="path">{{ img }}</span>
        </div>
      </div>

      <!-- معاينات محلية قبل الرفع -->
      <div v-for="(f, idx) in files" :key="'local-'+idx" class="card">
        <img :src="f.preview" class="img" />
        <div class="actions">
          <button class="mini danger" :disabled="isUploading" @click="removeLocal(idx)">إزالة</button>
        </div>
        <div class="meta">
          <span class="tag">Local</span>
          <span class="path">{{ f.file.name }}</span>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
type UploadResponse =
  | { urls: string[] }              // مثال: { urls: ["/uploads/a.jpg", ...] }
  | { images: string[] }            // مثال: { images: [...] }
  | { files: string[] }             // مثال: { files: [...] }
  | { path: string }                // مثال: { path: "/uploads/a.jpg" }
  | { url: string };                // مثال: { url: "/uploads/a.jpg" }

const props = defineProps<{
  modelValue: string[];
  productId?: number | string; // إذا endpoint يحتاج id
}>();

const emit = defineEmits<{
  (e: 'update:modelValue', v: string[]): void;
}>();

const { upload, buildAssetUrl } = useApi();

const errors = ref<string[]>([]);
const isUploading = ref(false);

const files = ref<{ file: File; preview: string }[]>([]);

// ✅ عدّل هذا حسب الباك عندك
// إذا عندك رفع مرتبط بالمنتج: `/Products/${props.productId}/images`
// إذا endpoint عام: `/Upload/images`
const UPLOAD_ENDPOINT = computed(() => {
  // مثال افتراضي:
  // POST https://localhost:7043/api/Uploads/images
  return props.productId ? `/admin/products/${String(props.productId)}/images` : '/Upload/images'
});

// هذا هو prefix لمجلد الصور على السيرفر (بدون /api)
// إذا سيرفرك يعرض الصور على /uploads
const UPLOADS_PREFIX = '/uploads';

const assetUrl = (p: string) => {
  // إذا الباك يرجع "a.jpg" أو "uploads/a.jpg" نخليه يصير "/uploads/a.jpg"
  if (!p) return '';
  if (/^https?:\/\//i.test(p)) return p;

  let normalized = p.replace(/\\/g, '/'); // لو رجع \ من ويندوز
  if (!normalized.startsWith('/')) normalized = '/' + normalized;

  // إذا رجع "/api/..." غلط للعرض—نشيله
  normalized = normalized.replace(/^\/api\//, '/');

  // إذا رجع "uploads/..." نخليه "/uploads/..."
  if (!normalized.startsWith(UPLOADS_PREFIX) && normalized.includes('/uploads/')) {
    const idx = normalized.indexOf('/uploads/');
    normalized = normalized.slice(idx);
  }

  // إذا رجع اسم ملف فقط "a.jpg" نخليه "/uploads/a.jpg"
  if (!normalized.includes('/')) {
    normalized = `${UPLOADS_PREFIX}/${normalized}`;
  } else if (normalized.split('/').length === 2 && normalized.startsWith('/')) {
    // "/a.jpg" => "/uploads/a.jpg"
    normalized = `${UPLOADS_PREFIX}${normalized}`;
  }

  return buildAssetUrl(normalized);
};

const onPick = (e: Event) => {
  errors.value = [];
  const input = e.target as HTMLInputElement;
  if (!input.files || input.files.length === 0) return;

  for (const f of Array.from(input.files)) {
    if (!f.type.startsWith('image/')) {
      errors.value.push(`الملف ليس صورة: ${f.name}`);
      continue;
    }
    const preview = URL.createObjectURL(f);
    files.value.push({ file: f, preview });
  }

  // reset input حتى تقدر تختار نفس الملف مرة ثانية
  input.value = '';
};

const removeLocal = (idx: number) => {
  const item = files.value[idx];
  if (item?.preview) URL.revokeObjectURL(item.preview);
  files.value.splice(idx, 1);
};

const removeServer = (idx: number) => {
  const next = [...props.modelValue];
  next.splice(idx, 1);
  emit('update:modelValue', next);
};

const clearAll = () => {
  for (const it of files.value) {
    if (it.preview) URL.revokeObjectURL(it.preview);
  }
  files.value = [];
  emit('update:modelValue', []);
  errors.value = [];
};

const normalizeResponseToUrls = (res: UploadResponse): string[] => {
  // نحاول نقرأ بأي شكل يرجعه الباك
  if ((res as any).urls?.length) return (res as any).urls;
  if ((res as any).images?.length) return (res as any).images;
  if ((res as any).files?.length) return (res as any).files;
  if ((res as any).url) return [(res as any).url];
  if ((res as any).path) return [(res as any).path];
  return [];
};

const uploadNow = async () => {
  errors.value = [];

  if (!files.value.length) {
    errors.value.push('اختر صور أولاً.');
    return;
  }

  // إذا endpoint يحتاج productId وانت ما مررته
  // (خليتها مرنة)
  // if (!props.productId) { ... }

  isUploading.value = true;
  try {
    const fd = new FormData();

    // ✅ أهم نقطة: اسم الحقل لازم يطابق اللي بالباك
    // Backend يعتمد 'files' للرفع.
    for (const it of files.value) {
      fd.append('files', it.file)
    }

    // إذا تحتاج تربط الرفع بمنتج
    if (props.productId !== undefined && props.productId !== null) {
      fd.append('productId', String(props.productId));
    }

    const res = await upload<UploadResponse>(UPLOAD_ENDPOINT.value, fd);
    const urls = normalizeResponseToUrls(res);

    if (!urls.length) {
      errors.value.push('الرفع تم لكن السيرفر لم يرجّع روابط صور قابلة للعرض. لازم يرجّع urls/images/files/url/path.');
      return;
    }

    // دمج صور السيرفر الحالية + الجديدة
    emit('update:modelValue', [...props.modelValue, ...urls]);

    // تفريغ الملفات المحلية
    for (const it of files.value) {
      if (it.preview) URL.revokeObjectURL(it.preview);
    }
    files.value = [];
  } catch (err: any) {
    errors.value.push(err?.message || 'فشل رفع الصور.');
  } finally {
    isUploading.value = false;
  }
};

const copy = async (p: string) => {
  const full = assetUrl(p);
  try {
    await navigator.clipboard.writeText(full);
  } catch {
    // ignore
  }
};
</script>

<style scoped>
.upload-box { border: 1px solid #eee; border-radius: 14px; padding: 14px; }
.top { display: flex; gap: 10px; align-items: center; flex-wrap: wrap; }
.btn {
  display: inline-flex; align-items: center; justify-content: center;
  border-radius: 12px; padding: 10px 12px; border: 1px solid #ddd;
  background: #fff; cursor: pointer; font-weight: 600;
}
.btn.ghost { background: #f7f7f7; }
.btn.danger { background: #fff; border-color: #ffb6b6; }
.btn:disabled { opacity: .6; cursor: not-allowed; }
.hidden { display: none; }
.hint { margin: 10px 0 0; font-size: 13px; opacity: .8; line-height: 1.6; }
.errors { margin-top: 10px; }
.err { background: #fff3f3; border: 1px solid #ffd0d0; padding: 8px 10px; border-radius: 10px; margin-bottom: 8px; font-size: 13px; }
.grid {
  margin-top: 12px;
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(180px, 1fr));
  gap: 12px;
}
.card { border: 1px solid #eee; border-radius: 14px; overflow: hidden; background: #fff; }
.img { width: 100%; height: 160px; object-fit: cover; display: block; }
.actions { display: flex; gap: 8px; padding: 10px; }
.mini {
  flex: 1;
  border-radius: 10px; padding: 8px 10px; border: 1px solid #ddd;
  background: #fff; cursor: pointer; font-weight: 600; font-size: 12px;
}
.mini.danger { border-color: #ffb6b6; }
.meta { padding: 0 10px 12px; font-size: 12px; opacity: .85; }
.tag { display: inline-block; font-weight: 700; margin-bottom: 6px; }
.path { display: block; word-break: break-all; }
</style>
