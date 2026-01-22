// app/composables/useApi.ts
type HttpMethod =
  | 'GET' | 'POST' | 'PUT' | 'PATCH' | 'DELETE'
  | 'get' | 'post' | 'put' | 'patch' | 'delete'

export type FetchOpts = {
  method?: HttpMethod
  query?: Record<string, any>
  headers?: Record<string, string>
  body?: any
}

export function useApi() {
  // كل الطلبات تمر عبر الـ BFF
  const base = '/api/bff'

  const config = useRuntimeConfig()
  // رابط الباك (للتطوير) مثل: https://localhost:7043
  const apiOrigin = String((config.public as any)?.apiOrigin || (config as any)?.apiOrigin || '').replace(/\/$/, '')

  const request = async <T>(path: string, opts: FetchOpts = {}): Promise<T> => {
    const url = `${base}${path.startsWith('/') ? path : `/${path}`}`

    // ✅ حل مشكلة TypedInternalResponse في TS
    return await $fetch(url, {
      method: opts.method as any,
      query: opts.query,
      headers: opts.headers,
      body: opts.body,
    }) as unknown as T
  }

  const get = <T>(path: string, query?: any, headers?: Record<string, string>) =>
    request<T>(path, { method: 'GET', query, headers })

  const post = <T>(path: string, body?: any, query?: any, headers?: Record<string, string>) =>
    request<T>(path, { method: 'POST', body, query, headers })

  const put = <T>(path: string, body?: any, query?: any, headers?: Record<string, string>) =>
    request<T>(path, { method: 'PUT', body, query, headers })

  const del = <T>(path: string, query?: any, headers?: Record<string, string>) =>
    request<T>(path, { method: 'DELETE', query, headers })

  // ✅ رفع FormData (صور/ملفات)
  const postForm = async <T>(
    path: string,
    formData: FormData,
    query?: any,
    headers?: Record<string, string>
  ): Promise<T> => {
    const url = `${base}${path.startsWith('/') ? path : `/${path}`}`
    return await $fetch(url, {
      method: 'POST',
      query,
      headers, // لا تضيف content-type
      body: formData,
    }) as unknown as T
  }

  // ✅ بناء رابط آمن للصور/الملفات
  // - لو الباك يرجّع /uploads/... نخليه يمر عبر proxy داخلي: /api/uploads/...
  // - لو رجّع URL كامل https://localhost:7043/uploads/... نقتطع المسار ونفس الشي
  // - أي مسار نسبي آخر نرجّعه absolute على نفس الدومين
  const buildAssetUrl = (p?: string | null) => {
    if (!p) return ''

    // full url
    if (p.startsWith('http://') || p.startsWith('https://')) {
      try {
        const u = new URL(p)
        // لو هو /uploads خلّيه يمر عبر proxy
        if (u.pathname.startsWith('/uploads/')) {
          const rest = u.pathname.replace(/^\/uploads\//, '')
          return `/api/uploads/${rest}`
        }
        return p
      } catch {
        return p
      }
    }

    // normalize
    const path = p.startsWith('/') ? p : `/${p}`

    // لو /uploads/... مرره عبر proxy
    if (path.startsWith('/uploads/')) {
      const rest = path.replace(/^\/uploads\//, '')
      return `/api/uploads/${rest}`
    }

    // لو الباك رجّع uploads/... داخل سترنغ
    const idx = path.indexOf('/uploads/')
    if (idx !== -1) {
      const rest = path.slice(idx).replace(/^\/uploads\//, '')
      return `/api/uploads/${rest}`
    }

    // fallback: خليها absolute على apiOrigin إذا أحبّيت (مفيد للروابط اللي مو proxy)
    // لكن افتراضيًا نخليها relative حتى تشتغل على نفس الدومين
    return path
  }

  // ✅ Alias حتى بعض الكومبوننتات القديمة تشتغل
  const upload = postForm

  
  async function uploadFiles(files: File[], fieldName = 'files') {
    const fd = new FormData()
    for (const f of files) fd.append(fieldName, f)
    // يمر عبر BFF حتى يعمل CORS والكوكيز بشكل صحيح
    return await postForm('/uploads', fd)
  }

  return { request, get, post, put, del, postForm, upload, buildAssetUrl }
}
