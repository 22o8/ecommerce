// ecommerce-web/app/composables/useApi.ts
import { useUiStore } from '~/stores/ui'
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

  // ✅ iOS Safari وبعض المتصفحات تكون حساسة لمسارات تحتوي مسافات أو backslashes
  const safeUrl = (s: string) => {
    const fixed = String(s).replace(/\\/g, '/')
    try {
      return encodeURI(fixed)
    } catch {
      return fixed
    }
  }

  const config = useRuntimeConfig()
  const apiBase = String((config.public as any)?.apiBase || '').replace(/\/$/, '')
  const apiOrigin = String((config.public as any)?.apiOrigin || '').replace(/\/$/, '')

  // 1) absolute URL
  // - لو كان http نحوله إلى https (خصوصاً Fly) لتجنب mixed content
  // - ولو كان يشير إلى /uploads على نفس apiOrigin نحوله إلى مسار يمر عبر الـ BFF
  //   حتى نتفادى مشاكل CORS/SSR/Refresh وتكون الروابط ثابتة على دومين الواجهة.
  if (p.startsWith('http://') || p.startsWith('https://')) {
    try {
      const u = new URL(p)

      // proxy uploads عبر الـ BFF إذا كان نفس apiOrigin
      if (u.pathname.startsWith('/uploads/') && apiBase && apiOrigin) {
        try {
          const o = new URL(apiOrigin)
          if (u.hostname === o.hostname) {
            const b = apiBase.replace(/\/$/, '')
            return safeUrl(`${b}${u.pathname}`)
          }
        } catch {
          // ignore
        }
      }

      if (u.protocol === 'http:') {
        const looksLikeFly = u.hostname.endsWith('fly.dev') || u.hostname.endsWith('fly.io')
        // apiBase قد يكون مسار نسبي مثل /api/bff على Vercel
        let looksLikeSameHost = false
        try {
          if (apiBase.startsWith('http')) looksLikeSameHost = u.hostname === new URL(apiBase).hostname
        } catch {
          // ignore
        }
        if (looksLikeFly || looksLikeSameHost) {
          u.protocol = 'https:'
          return safeUrl(u.toString())
        }
      }
    } catch {
      // ignore
    }
    return safeUrl(p)
  }

  const path = p.startsWith('/') ? p : `/${p}`

  // 2) uploads relative path:
  // الأفضل نخليها تمر عبر الـ BFF (/api/bff/uploads/...) حتى تبقى ثابتة على نفس الدومين
  // وتتفادى مشاكل SSR/Refresh/CORS أو غياب apiOrigin على Vercel.
  if (path.startsWith('/uploads/')) {
    if (apiBase) {
      const b = apiBase.replace(/\/$/, '')
      return safeUrl(`${b}${path}`)
    }
    // fallback: لو apiBase غير متوفر
    if (apiOrigin) return safeUrl(`${apiOrigin}${path}`)
    return safeUrl(path)
  }

  // 3) any other relative: return as-is
  return safeUrl(path)
}


export function useApi() {
  // كل الطلبات تمر عبر الـ BFF
  const config = useRuntimeConfig()
  const publicApiBaseRaw = String((config.public as any)?.apiBase || '').trim()

  // ✅ حل جذري لمشكلة 404:
  // إذا المستخدم حط الدومين فقط (بدون /api)، نضيف /api تلقائياً.
  // وإذا حط /api مسبقاً ما نضاعفها.
  const normalizeApiBase = (v: string) => {
    const cleaned = v.replace(/\/$/, '')
    if (!cleaned.startsWith('http')) return cleaned
    return /\/api$/i.test(cleaned) ? cleaned : `${cleaned}/api`
  }

  const publicApiBase = normalizeApiBase(publicApiBaseRaw)

  // ✅ كل الطلبات لازم تمر عبر الـ BFF حتى تشتغل بنفس الطريقة على كل الأجهزة والمتصفحات
  // حتى لو أحد حط الدومين الكامل بالـ env (https://...)
  const base = publicApiBaseRaw && !publicApiBaseRaw.startsWith('http')
    ? publicApiBaseRaw
    : '/api/bff'
  // رابط الباك (بدون /api) — يستخدم لبناء روابط الصور والملفات
  // إذا apiBase يحتوي /api، نرجّع الأصل تلقائياً حتى ما يصير /api/uploads...
  const apiOriginRaw = String(
    (config.public as any)?.apiOrigin ||
    (config.public as any)?.apiBase ||
    (config as any)?.apiOrigin ||
    ''
  ).trim()

  const apiOrigin = apiOriginRaw
    .replace(/\/$/, '')
    .replace(/\/api$/i, '')

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

    try {
      return (await $fetch(url, {
        method: opts.method as any,
        query: opts.query,
        headers: mergedHeaders,
        // على المتصفح: خليه يرسل الكوكيز دائماً
        credentials: 'include',
        body: opts.body,
      })) as unknown as T
    } catch (e: any) {
      // سجل آخر خطأ API (للتشخيص على الأجهزة التي تفشل بصمت)
      try {
        const ui = useUiStore()
        const status = e?.statusCode ?? e?.response?.status ?? e?.status
        const msg = String(e?.message || 'API Error')
        ui.setLastApiError({
          at: new Date().toISOString(),
          url,
          method: String(opts.method || 'GET'),
          status: typeof status === 'number' ? status : undefined,
          message: msg,
          data: e?.data ?? e?.response?._data ?? undefined,
        } as any)
      } catch {
        // ignore
      }
      throw e
    }
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
