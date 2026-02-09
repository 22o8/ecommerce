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
  const apiBase = (config.public as any)?.apiBase?.toString?.() || ''

  // إذا كان apiBase نسبي (مثل /api/bff)، نرجّع الملفات عبر الـ BFF نفسه
  // حتى يكون الرابط شغال على Vercel بدون الاعتماد على الدومين الخارجي.
  const isRelativeApiBase = apiBase.startsWith('/')

  // full url -> إذا كان رابط رفع (uploads) حوّله إلى رابط الـ BFF حتى ما ينكسر بين الدومينات
  if (p.startsWith('http://') || p.startsWith('https://')) {
    try {
      const u = new URL(p)
      if (u.pathname.startsWith('/uploads/')) {
        // ✅ الأفضل دائماً: نخدم الصور عبر الـ BFF على نفس دومين الموقع
        const bff = (isRelativeApiBase && apiBase) ? apiBase : '/api/bff'
        return `${bff}${u.pathname}`
      }
    } catch {
      // ignore
    }
    return p
  }

  const path = p.startsWith('/') ? p : `/${p}`

  // أي ملف تحت /uploads نخليه يمر عبر /api/bff/uploads...
  if (path.startsWith('/uploads/')) {
    const bff = (isRelativeApiBase && apiBase) ? apiBase : '/api/bff'
    return `${bff}${path}`
  }

  // fallback
  return path
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
