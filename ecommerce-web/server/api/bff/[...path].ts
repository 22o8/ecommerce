// ecommerce-web/server/api/bff/[...path].ts
import { defineEventHandler, proxyRequest, getRequestURL, getRouterParam, getRequestHeaders, readRawBody, setCookie, setResponseStatus, setResponseHeader, getCookie, deleteCookie } from 'h3'

export default defineEventHandler(async (event) => {
  try {
    const config = useRuntimeConfig()

    // لا تعتمد على apiBase حتى لا يصير recursion على نفسه
    const apiOrigin =
      (config as any).apiOrigin ||
      (config.public as any).apiOrigin ||
      process.env.NUXT_API_ORIGIN ||
      process.env.NUXT_PUBLIC_API_ORIGIN

    if (!apiOrigin) {
      setResponseStatus(event, 500)
      return { error: 'Missing API origin. Set NUXT_API_ORIGIN / NUXT_PUBLIC_API_ORIGIN.' }
    }

    const method = (event.node.req.method || 'GET').toUpperCase()
    const routePath = getRouterParam(event, 'path') || ''

    const targetBase = String(apiOrigin).replace(/\/$/, '')

    // uploads بدون /api
    const isUploads = routePath.startsWith('uploads/') || routePath === 'uploads'
    const apiBase = targetBase.endsWith('/api') ? targetBase : `${targetBase}/api`

    const incomingUrl = getRequestURL(event)
    const search = incomingUrl.search || ''

    const target = isUploads
      ? `${targetBase}/${routePath}${search}`
      : `${apiBase}/${routePath}${search}`

    // Headers (sanitized)
    const headers = { ...(getRequestHeaders(event) as Record<string, any>) }
    delete headers.host
    delete headers.connection
    delete headers['content-length']
    delete headers['accept-encoding']
    delete headers['Accept-Encoding']

    // ✅ مرّر Authorization إذا موجود
    const authHeader = headers.authorization || headers.Authorization

    // ✅ fallback: خذ التوكن من Cookie إذا ماكو Authorization
    const tokenFromCookie =
      getCookie(event, 'token') ||
      getCookie(event, 'access_token') ||
      getCookie(event, 'access')
    if (!authHeader && tokenFromCookie) {
      headers.authorization = `Bearer ${tokenFromCookie}`
    }

    // ✅ WhatsApp Checkout secret
    if (routePath.toLowerCase() === 'checkout/cart/whatsapp') {
      const secret = process.env.CHECKOUT_SECRET
      if (secret) headers['X-Checkout-Secret'] = secret
    }

    const isLogin = routePath.toLowerCase() === 'auth/login' && method === 'POST'
    const isLogout = routePath.toLowerCase() === 'auth/logout'

    // ====== LOGIN / LOGOUT بحاجة معالجة خاصة (Cookies) ======
    if (isLogin) {
      const body = await readRawBody(event, false).catch(() => undefined)

      const res = await fetch(target, {
        method,
        headers: { ...headers },
        body: body || undefined,
      })

      const json = await res.json().catch(() => null)

      const token = json?.token
      const role = json?.user?.role || 'User'

      if (token) {
        const secure = process.env.NODE_ENV === 'production'

        setCookie(event, 'token', token, {
          httpOnly: true,
          secure,
          sameSite: 'lax',
          path: '/',
          maxAge: 60 * 60 * 24 * 30,
        })

        // access غير httpOnly (مفيد لبعض البيئات/المتصفحات)
        setCookie(event, 'access', token, {
          httpOnly: false,
          secure,
          sameSite: 'lax',
          path: '/',
          maxAge: 60 * 60 * 24 * 30,
        })

        setCookie(event, 'role', String(role), {
          httpOnly: false,
          secure,
          sameSite: 'lax',
          path: '/',
          maxAge: 60 * 60 * 24 * 30,
        })

        setCookie(event, 'auth', '1', {
          httpOnly: false,
          secure,
          sameSite: 'lax',
          path: '/',
          maxAge: 60 * 60 * 24 * 30,
        })

        if (json?.user) {
          setCookie(event, 'user', JSON.stringify(json.user), {
            httpOnly: false,
            secure,
            sameSite: 'lax',
            path: '/',
            maxAge: 60 * 60 * 24 * 30,
          })
        }
      }

      setResponseStatus(event, res.status)
      return json
    }

    if (isLogout) {
      const res = await fetch(target, { method, headers: { ...headers } })
      const json = await res.json().catch(() => null)

      const names = ['token','access','access_token','role','auth','user']
      for (const n of names) {
        try { deleteCookie(event, n, { path: '/' }) } catch { /* ignore */ }
      }

      setResponseStatus(event, res.status)
      return json ?? { ok: res.ok }
    }

    // ====== باقي المسارات: Proxy مباشر (جذري) ======
    // هذا يحل مشكلة 500 على Vercel بسبب Body فارغ/Content-Type فارغ أو binary responses.
    return await proxyRequest(event, target, {
      headers,
    })

  } catch (err: any) {
    console.error('BFF error:', err)
    setResponseStatus(event, 502)
    return { error: err?.message || 'BFF failed' }
  }
})
