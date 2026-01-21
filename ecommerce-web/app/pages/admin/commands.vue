<script setup lang="ts">
definePageMeta({ layout: 'admin', middleware: ['auth', 'admin'] })

import { ref, onMounted } from 'vue'
import { useApi } from '~/composables/useApi'

const api = useApi()

// token مخزّن بالكوكي حسب اللي باين عندك بالـ Network (cookie: token=...)
const token = useCookie<string | null>('token')

const igUserId = ref('')
const accessToken = ref('')
const limit = ref(80)

const loading = ref(false)
const message = ref<string | null>(null)
const error = ref<string | null>(null)

onMounted(() => {
  igUserId.value = localStorage.getItem('ig_user_id') || ''
  accessToken.value = localStorage.getItem('ig_access_token') || ''
})

function saveLocal() {
  localStorage.setItem('ig_user_id', igUserId.value.trim())
  localStorage.setItem('ig_access_token', accessToken.value.trim())
}

function authHeaders() {
  // إذا ماكو توكن، رجّع هيدرز فارغ
  if (!token.value) return {}
  return { Authorization: `Bearer ${token.value}` }
}

function normalizeError(e: any, fallback: string) {
  const status = e?.status || e?.response?.status || e?.data?.statusCode
  const msg = e?.data?.error || e?.data?.message || e?.message || fallback

  if (status === 401) return 'غير مسموح (401) — لازم تسجّل دخول أدمن والتوكن يكون صحيح.'
  if (status === 403) return 'ممنوع (403) — حسابك مو أدمن أو ما عندك صلاحية.'
  return msg
}

async function runImport() {
  error.value = null
  message.value = null
  loading.value = true

  try {
    saveLocal()
    const res = await api.post(
      '/admin/instagram/import',
      {
        igUserId: igUserId.value.trim(),
        accessToken: accessToken.value.trim(),
        limit: Number(limit.value || 80),
      },
      {
        headers: authHeaders(),
      }
    )

    message.value = `Imported: ${res.imported}, Skipped: ${res.skipped}`
  } catch (e: any) {
    error.value = normalizeError(e, 'Import failed')
  } finally {
    loading.value = false
  }
}

async function deleteImported() {
  if (!confirm('Delete all Instagram imported products (slug starts with ig-)?')) return

  error.value = null
  message.value = null
  loading.value = true

  try {
    // ✅ هذا هو المسار الصحيح حسب الـ API: DELETE /api/admin/instagram/imported
    const res = await api.del('/admin/instagram/imported', {
      headers: authHeaders(),
    })

    message.value = `Deleted: ${res.deleted ?? 'OK'}`
  } catch (e: any) {
    error.value = normalizeError(e, 'Delete failed')
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <div class="adm-page">
    <div class="adm-header">
      <div>
        <h1 class="adm-title">Commands</h1>
        <p class="adm-sub">أوامر قوية لإدارة الموقع (Import من Instagram + حذف جماعي).</p>
      </div>
    </div>

    <div v-if="error" class="adm-alert adm-alert-err">{{ error }}</div>
    <div v-if="message" class="adm-alert adm-alert-ok">{{ message }}</div>

    <div class="adm-grid">
      <section class="adm-card">
        <h2 class="adm-card-title">Instagram Import</h2>
        <p class="adm-card-hint">
          يستورد آخر البوستات ويحوّلها إلى منتجات. إذا ماكو سعر بالكابشن → السعر يظهر "-" بالموقع.
        </p>

        <div class="adm-form">
          <label class="adm-label">IG User Id</label>
          <input v-model="igUserId" class="adm-input" placeholder="مثال: 1784..." />

          <label class="adm-label">Access Token</label>
          <textarea v-model="accessToken" class="adm-input" rows="3" placeholder="IG Graph API token"></textarea>

          <label class="adm-label">Limit</label>
          <input v-model.number="limit" type="number" class="adm-input" min="1" max="500" />

          <div class="adm-actions">
            <button class="adm-btn" :disabled="loading" @click="runImport">
              {{ loading ? 'Working...' : 'Run Import' }}
            </button>
            <button class="adm-btn adm-btn-danger" :disabled="loading" @click="deleteImported">
              Delete Imported
            </button>
          </div>
        </div>
      </section>

      <section class="adm-card">
        <h2 class="adm-card-title">ملاحظات مهمّة</h2>
        <ul class="adm-list">
          <li>هذا الحل يستخدم Instagram Graph API (توكن)، لأن تسجيل الدخول بيوزر/باسورد مو آمن وغير مدعوم.</li>
          <li>الصور يتم تنزيلها إلى السيرفر (uploads) حتى لا تختفي بعد دقائق.</li>
          <li>المنتجات المستوردة يكون الـ slug مالها يبدأ بـ <b>ig-</b> حتى تقدر تحذفها دفعة وحدة.</li>
        </ul>
      </section>
    </div>
  </div>
</template>

<style scoped>
.adm-page{padding:20px;}
.adm-header{display:flex;align-items:flex-end;justify-content:space-between;margin-bottom:14px}
.adm-title{font-size:24px;font-weight:800;margin:0;color:rgb(var(--adm-text));}
.adm-sub{margin:6px 0 0;color:rgba(var(--adm-text),.75)}
.adm-grid{display:grid;grid-template-columns:1fr 1fr;gap:14px}
.adm-card{background:rgba(var(--adm-panel),var(--adm-panel-alpha));border:1px solid rgba(var(--adm-border),var(--adm-border-alpha));border-radius:16px;padding:16px;box-shadow:0 18px 50px rgba(0,0,0,.15)}
.adm-card-title{margin:0 0 8px;font-weight:800}
.adm-card-hint{margin:0 0 12px;color:rgba(var(--adm-text),.75);font-size:13px}
.adm-form{display:grid;gap:10px}
.adm-label{font-size:12px;color:rgba(var(--adm-text),.75)}
.adm-input{width:100%;border-radius:12px;border:1px solid rgba(var(--adm-border),var(--adm-border-alpha));padding:10px 12px;background:rgba(255,255,255,.9);color:rgb(var(--adm-text));outline:none}
html.theme-dark .adm-input{background:rgba(0,0,0,.22);color:rgb(var(--adm-text))}
.adm-actions{display:flex;gap:10px;margin-top:6px;flex-wrap:wrap}
.adm-btn{background:rgb(var(--adm-primary));color:white;border:none;border-radius:12px;padding:10px 14px;font-weight:700;cursor:pointer}
.adm-btn:disabled{opacity:.6;cursor:not-allowed}
.adm-btn-danger{background:#b42318}
.adm-alert{border-radius:12px;padding:10px 12px;margin-bottom:12px;border:1px solid transparent}
.adm-alert-ok{background:rgba(34,197,94,.12);border-color:rgba(34,197,94,.35)}
.adm-alert-err{background:rgba(239,68,68,.12);border-color:rgba(239,68,68,.35)}
.adm-list{margin:0;padding-left:18px;color:rgba(var(--adm-text),.8);display:grid;gap:8px}
@media (max-width: 980px){.adm-grid{grid-template-columns:1fr}}
</style>
