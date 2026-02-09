// ecommerce-web/app/composables/useApi.ts
type HttpMethod =
  | 'GET' | 'POST' | 'PUT' | 'PATCH' | 'DELETE'
  | 'get' | 'post' | 'put' | 'patch' | 'delete'

export type FetchOpts = {
  method?: HttpMethod
  query?: Record<string, any>
  headers?: Record<string, string>
  body?: any
}

// ✅ بناء رابط آمن للصور/الملفات
// بعض الصفحات تستورده كـ named export:
// import { buildAssetUrl } from '~/composables/useApi'
export function buildAssetUrl(p?: string | null) {
  if (!p) return ''

  const config = useRuntimeConfig()
  const apiBase = String((config.public as any)?.apiBase || '').replace(/\/$/, '')
  const apiOrigin = String((config.public as any)?.apiOrigin || '').replace(/\/$/, '')

  // 1) absolute URL: لو كان http نحوله إلى https (خصوصاً Fly) لتجنب mixed content
  if (p.startsWith('http://') || p.startsWith('https://')) {
    try {
      const u = new URL(p)
      if (u.protocol === 'http:') {
        const looksLikeFly = u.hostname.endsWith('fly.dev') || u.hostname.endsWith('fly.io')
        const looksLikeSameHost = u.hostname === new URL(apiBase).hostname
        if (looksLikeFly || looksLikeSameHost) {
          u.protocol = 'https:'
          return u.toString()
        }
      }
    } catch {
      // ignore
    }
    return p
  }

  const path = p.startsWith('/') ? p : `/${p}`

  // 2) uploads relative path: serve from backend origin directly
  if (path.startsWith('/uploads/')) {
    // if apiOrigin exists -> https://host + /uploads/...
    if (apiOrigin) return `${apiOrigin}${path}`
    // fallback to relative
    return path
  }

  // 3) any other relative: return as-is
  return path
}


export function useApi() {
  // كل الطلبات تمر عبر الـ BFF
  const config = useRuntimeConfig()
  const publicApiBase = String((config.public as any)?.apiBase || '').replace(/\/$/, '')
  const base = publicApiBase.startsWith('http') ? publicApiBase : '/api/bff' 
  // رابط الباك (للتطوير) مثل: https://localhost:7043
  const apiOrigin = String(
    (config.public as any)?.apiOrigin ||
    (config.public as any)?.apiBase ||
    (config as any)?.apiOrigin ||
    ''
  ).replace(/\/$/, '')

  // ✅ توكن عميل (غير HttpOnly) — مهم للموبايل/Telegram
  // إذا HttpOnly token ما ينحفظ/ينرسل على iOS، هذا ينقذك.
  const access = useCookie<string | null>('access', {
    default: () => null,
    path: '/',
    sameSite: 'lax',
    secure: process.env.NODE_ENV === 'production',
    maxAge: 60 * 60 * 24 * 30,
  })

  const request = async <T>(path: string, opts: FetchOpts = {}): Promise<T> => {
    const url = `${base}${path.startsWith('/') ? path : `/${path}`}`

    // ✅ SSR: انقل Cookie header من ريكوست المستخدم إلى ريكوست الـ BFF
    const ssrCookieHeaders = process.server
      ? (useRequestHeaders(['cookie']) as Record<string, string>)
      : {}

    const mergedHeaders: Record<string, string> = {
      ...ssrCookieHeaders,
      ...(opts.headers || {}),
    }

    // ✅ إذا عندنا access token (غير HttpOnly) ارسله كـ Authorization
    // مهم خصوصاً على SSR لأن الـ cookie لوحده قد لا يكون كافي للمصادقة.
    // (لا تكتب Authorization إذا موجود أصلًا)
    if (!mergedHeaders.Authorization && !mergedHeaders.authorization) {
      const t = (access.value || '').trim()
      if (t) mergedHeaders.Authorization = `Bearer ${t}`
    }

    return (await $fetch(url, {
      method: opts.method as any,
      query: opts.query,
      headers: mergedHeaders,
      // على المتصفح: خليه يرسل الكوكيز دائماً
      credentials: 'include',
      body: opts.body,
    })) as unknown as T
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

    const ssrCookieHeaders = process.server
      ? (useRequestHeaders(['cookie']) as Record<string, string>)
      : {}

    const mergedHeaders: Record<string, string> = {
      ...ssrCookieHeaders,
      ...(headers || {}),
    }

    // ✅ Authorization من access (SSR + Client)
    if (!mergedHeaders.Authorization && !mergedHeaders.authorization) {
      const t = (access.value || '').trim()
      if (t) mergedHeaders.Authorization = `Bearer ${t}`
    }

    return (await $fetch(url, {
      method: 'POST',
      query,
      headers: mergedHeaders, // لا تضيف content-type
      credentials: 'include',
      body: formData,
    })) as unknown as T
  }

  // ✅ نفس الدالة لكن متاحة أيضاً كـ named export بالأعلى
  // حتى لا ينكسر build عند الاستيراد المباشر.
  const buildAssetUrlLocal = buildAssetUrl

  const upload = postForm

  return { request, get, post, put, del, postForm, upload, buildAssetUrl: buildAssetUrlLocal }
}
