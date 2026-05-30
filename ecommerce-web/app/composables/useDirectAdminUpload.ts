export type DirectUploadOptions = {
  maxMb?: number
  fallbackToBff?: boolean
}

function apiOriginFromRuntime() {
  const config = useRuntimeConfig()
  const pub: any = config.public || {}
  const raw = String(pub.apiOrigin || pub.apiBase || '').trim()
  if (!raw || !raw.startsWith('http')) return ''
  return raw.replace(/\/$/, '').replace(/\/api$/i, '')
}

function authHeaders() {
  const access = useCookie<string | null>('access').value
  const token = useCookie<string | null>('token').value || useCookie<string | null>('access_token').value
  const bearer = (access || token || '').trim()
  return bearer ? { Authorization: `Bearer ${bearer}` } : {}
}

export function useDirectAdminUpload() {
  async function upload(path: string, file: File, options: DirectUploadOptions = {}) {
    const maxMb = options.maxMb ?? 120
    const sizeMb = file.size / 1024 / 1024
    if (sizeMb > maxMb) {
      throw new Error(`حجم الملف كبير. الحد الأقصى ${maxMb}MB`)
    }

    const fd = new FormData()
    fd.append('file', file)
    const normalizedPath = path.replace(/^\/+/, '')
    const origin = apiOriginFromRuntime()

    // رفع مباشر إلى الباك حتى لا يصطدم بحجم Vercel Function.
    if (origin) {
      try {
        const res: any = await $fetch(`${origin}/api/${normalizedPath}`, {
          method: 'POST',
          body: fd,
          headers: authHeaders(),
          credentials: 'include',
          timeout: 120000,
        })
        return res?.url?.url || res?.url || res?.imageUrl || res?.key || ''
      } catch (e) {
        if (!options.fallbackToBff) throw e
      }
    }

    // fallback للملفات الصغيرة فقط. لا تستخدمه للفيديوهات الكبيرة.
    const res: any = await $fetch(`/api/bff/${normalizedPath}`, {
      method: 'POST',
      body: fd,
      timeout: 120000,
    })
    return res?.url?.url || res?.url || res?.imageUrl || res?.key || ''
  }

  return { upload }
}
