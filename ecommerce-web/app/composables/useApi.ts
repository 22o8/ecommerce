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
  const apiOrigin = String((config.public as any)?.apiOrigin || (config as any)?.apiOrigin || '').replace(/\/$/, '')

  const request = async <T>(path: string, opts: FetchOpts = {}): Promise<T> => {
    const url = `${base}${path.startsWith('/') ? path : `/${path}`}`

    // ✅ SSR: انقل cookie header من المستخدم إلى /api/bff حتى لا يصير 401 على Vercel
    const ssrCookieHeaders = process.server ? (useRequestHeaders(['cookie']) as Record<string, string>) : {}

    const mergedHeaders: Record<string, string> = {
      ...ssrCookieHeaders,
      ...(opts.headers || {}),
    }

    return (await $fetch(url, {
      method: opts.method as any,
      query: opts.query,
      headers: mergedHeaders,
      // ✅ ضروري جداً للموبايل حتى يرسل الكوكيز
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

    const ssrCookieHeaders = process.server ? (useRequestHeaders(['cookie']) as Record<string, string>) : {}
    const mergedHeaders: Record<string, string> = {
      ...ssrCookieHeaders,
      ...(headers || {}),
    }

    // لا تضيف content-type حتى المتصفح يضيف boundary
    return (await $fetch(url, {
      method: 'POST',
      query,
      headers: mergedHeaders,
      credentials: 'include',
      body: formData,
    })) as unknown as T
  }

  // ✅ بناء رابط آمن للصور/الملفات
  const buildAssetUrl = (p?: string | null) => {
    if (!p) return ''

    // full url
    if (p.startsWith('http://') || p.startsWith('https://')) {
      try {
        const u = new URL(p)
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

    // لو داخل سترنغ أكو /uploads/
    const idx = path.indexOf('/uploads/')
    if (idx !== -1) {
      const rest = path.slice(idx).replace(/^\/uploads\//, '')
      return `/api/uploads/${rest}`
    }

    // fallback: رجع path نسبي على نفس الدومين
    return path
  }

  // ✅ Alias للتوافق
  const upload = postForm

  return { request, get, post, put, del, postForm, upload, buildAssetUrl, apiOrigin }
}
