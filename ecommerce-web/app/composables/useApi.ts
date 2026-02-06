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

export function useApi() {
  // كل الطلبات تمر عبر الـ BFF
  const base = '/api/bff'

  const config = useRuntimeConfig()
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

    // ✅ Client: إذا عندنا access token غير httpOnly، ارسله كـ Authorization
    // (لا تكتب Authorization إذا موجود أصلًا)
    if (process.client && !mergedHeaders.Authorization && !mergedHeaders.authorization) {
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

    // ✅ Client: Authorization من access
    if (process.client && !mergedHeaders.Authorization && !mergedHeaders.authorization) {
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

  // ✅ بناء رابط آمن للصور/الملفات
  const buildAssetUrl = (p?: string | null) => {
    if (!p) return ''

    const apiBase = (config.public.apiBase || '').toString()

    // إذا كان apiBase نسبي (مثل /api/bff)، نرجّع الملفات عبر الـ BFF نفسه
    // حتى يكون الرابط شغال على Vercel بدون الاعتماد على الدومين الخارجي.
    const isRelativeApiBase = apiBase.startsWith('/')

    // full url -> رجّعه مثل ما هو
    if (p.startsWith('http://') || p.startsWith('https://')) return p

    const path = p.startsWith('/') ? p : `/${p}`

    // أي ملف تحت /uploads نخليه يمر عبر /api/bff/uploads...
    if (path.startsWith('/uploads/')) {
      if (isRelativeApiBase && apiBase) return `${apiBase}${path}`

      // apiBase كامل (مثلاً https://host/api) -> حوّلها لـ origin + /uploads
      const apiOrigin = apiBase.replace(/\/api\/?$/, '')
      return apiOrigin ? `${apiOrigin}${path}` : path
    }

    // fallback
    return path
  }

  const upload = postForm

  return { request, get, post, put, del, postForm, upload, buildAssetUrl }
}
