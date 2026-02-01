// ecommerce-web/server/api/bff/[...path].ts

export default defineEventHandler(async (event) => {
  try {
    const config = useRuntimeConfig()

    const apiOrigin =
      (config.public as any).apiOrigin ||
      (config.public as any).apiBase ||
      process.env.NUXT_API_ORIGIN ||
      process.env.NUXT_PUBLIC_API_ORIGIN

    if (!apiOrigin) {
      setResponseStatus(event, 500)
      return { error: 'Missing API origin. Set NUXT_API_ORIGIN / NUXT_PUBLIC_API_ORIGIN.' }
    }

    const method = (event.node.req.method || 'GET').toUpperCase()
    const routePath = getRouterParam(event, 'path') || ''

    const targetBase = String(apiOrigin).replace(/\/$/, '')
    const apiBase = targetBase.endsWith('/api') ? targetBase : `${targetBase}/api`
    const targetUrl = new URL(`${apiBase}/${routePath}`)

    // Query params
    const incomingQuery = getQuery(event) as Record<string, any>

    // ✅ query=JSON => params
    if (typeof incomingQuery.query === 'string' && incomingQuery.query.trim()) {
      try {
        const obj = JSON.parse(incomingQuery.query)
        if (obj && typeof obj === 'object') {
          for (const [k, v] of Object.entries(obj)) {
            if (v === undefined || v === null) continue
            if (Array.isArray(v)) v.forEach((vv) => targetUrl.searchParams.append(k, String(vv)))
            else targetUrl.searchParams.set(k, String(v))
          }
        }
      } catch {
        targetUrl.searchParams.set('query', incomingQuery.query)
      }
      delete incomingQuery.query
    }

    for (const [k, v] of Object.entries(incomingQuery)) {
      if (v === undefined || v === null) continue
      if (Array.isArray(v)) v.forEach((vv) => targetUrl.searchParams.append(k, String(vv)))
      else targetUrl.searchParams.set(k, String(v))
    }

    // Headers
    const headers = getRequestHeaders(event) as Record<string, any>
    delete headers.host
    delete headers.connection
    delete headers['content-length']

    // ✅ إذا ماكو Authorization وخليته بالكوكي، سوّله Bearer (لـ SSR/أحياناً)
    const tokenFromCookie = getCookie(event, 'token') || getCookie(event, 'access_token')
    const authHeader = headers.authorization || headers.Authorization
    if (!authHeader && tokenFromCookie) headers.authorization = `Bearer ${tokenFromCookie}`

    // Body (raw للحفاظ على multipart)
    const body =
      method === 'GET' || method === 'HEAD' ? undefined : await readRawBody(event, false)

    const res = await fetch(targetUrl.toString(), {
      method,
      headers: { ...headers },
      body: body || undefined,
    })

    const lower = routePath.toLowerCase()

    const isLogin = lower === 'auth/login' && method === 'POST'
    const isRegister = lower === 'auth/register' && method === 'POST'
    const isLogout = lower === 'auth/logout'

    // ✅ Helpers for cookies options
    const isProd = process.env.NODE_ENV === 'production'

    const commonCookie = {
      secure: isProd, // مهم: محلياً لازم false
      sameSite: 'lax' as const,
      path: '/',
      maxAge: 60 * 60 * 24 * 30, // 30d
    }

    // ✅ login/register: خزّن token + role + auth + user
    if (isLogin || isRegister) {
      const json = await res.json().catch(() => null)

      const token = json?.token
      const role = String(json?.user?.role || 'User')
      const user = json?.user || null

      if (token) {
        // token httpOnly (أمني)
        setCookie(event, 'token', token, {
          ...commonCookie,
          httpOnly: true,
        })

        // role غير httpOnly حتى الفرونت يقراه للميدلوير
        setCookie(event, 'role', role, {
          ...commonCookie,
          httpOnly: false,
        })

        // auth flag
        setCookie(event, 'auth', '1', {
          ...commonCookie,
          httpOnly: false,
        })

        // user (مفيد للواجهة)
        setCookie(event, 'user', JSON.stringify(user), {
          ...commonCookie,
          httpOnly: false,
        })
      }

      setResponseStatus(event, res.status)
      return json
    }

    // ✅ logout: امسح الكوكيز
    if (isLogout) {
      deleteCookie(event, 'token', { path: '/' })
      deleteCookie(event, 'role', { path: '/' })
      deleteCookie(event, 'auth', { path: '/' })
      deleteCookie(event, 'user', { path: '/' })

      setResponseStatus(event, 200)
      return { ok: true }
    }

    setResponseStatus(event, res.status)

    // مرر نوع الاستجابة
    const ct = res.headers.get('content-type') || ''
    if (ct.includes('application/json')) return await res.json()
    return await res.text()
  } catch (err: any) {
    console.error('BFF error:', err)
    setResponseStatus(event, 500)
    return { error: err?.message || 'BFF failed' }
  }
})
