// ecommerce-web/server/api/bff/[...path].ts
import { readMultipartFormData, readRawBody } from 'h3'

export default defineEventHandler(async (event) => {
  try {
    const config = useRuntimeConfig()

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
    const isUploads = routePath.startsWith('uploads/') || routePath === 'uploads'
    const apiBase = targetBase.endsWith('/api') ? targetBase : `${targetBase}/api`
    const targetUrl = new URL(isUploads ? `${targetBase}/${routePath}` : `${apiBase}/${routePath}`)

    const incomingQuery = getQuery(event) as Record<string, any>
    if (typeof incomingQuery.query === 'string' && incomingQuery.query.trim()) {
      try {
        const obj = JSON.parse(incomingQuery.query)
        if (obj && typeof obj === 'object') {
          for (const [k, v] of Object.entries(obj)) {
            if (v == null) continue
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
      if (v == null) continue
      if (Array.isArray(v)) v.forEach((vv) => targetUrl.searchParams.append(k, String(vv)))
      else targetUrl.searchParams.set(k, String(v))
    }

    const headers = getRequestHeaders(event) as Record<string, any>
    delete headers.host
    delete headers.connection
    delete headers['content-length']
    delete headers['accept-encoding']
    delete headers['Accept-Encoding']
    headers['accept-encoding'] = 'identity'

    const authHeader = headers.authorization || headers.Authorization
    const tokenFromCookie =
      getCookie(event, 'token') ||
      getCookie(event, 'access_token') ||
      getCookie(event, 'access')

    if (!authHeader && tokenFromCookie) {
      headers.authorization = `Bearer ${tokenFromCookie}`
    }

    if (routePath.toLowerCase() === 'checkout/cart/whatsapp') {
      const secret = process.env.CHECKOUT_SECRET
      if (secret) headers['X-Checkout-Secret'] = secret
    }

    let body: any = undefined
    const requestCt = String(headers['content-type'] || headers['Content-Type'] || '')

    if (method !== 'GET' && method !== 'HEAD') {
      if (requestCt.includes('multipart/form-data')) {
        const parts = await readMultipartFormData(event)
        const fd = new FormData()

        for (const part of parts || []) {
          const name = (part as any)?.name || 'file'
          const filename = (part as any)?.filename
          const mime = (part as any)?.type || 'application/octet-stream'
          const data = (part as any)?.data

          if (filename) {
            const bytes = data instanceof Uint8Array ? data : new Uint8Array(data || [])
            const blob = new Blob([bytes], { type: mime })
            fd.append(name, blob, filename)
          } else {
            const value = data instanceof Uint8Array
              ? new TextDecoder().decode(data)
              : (data?.toString?.() ?? String(data ?? ''))
            fd.append(name, value)
          }
        }

        body = fd
        delete headers['content-type']
        delete headers['Content-Type']
      } else {
        body = await readRawBody(event, false)
      }
    }

    const res = await fetch(targetUrl.toString(), {
      method,
      headers: { ...headers },
      body: body || undefined,
    })

    const isLogin = routePath.toLowerCase() === 'auth/login' && method === 'POST'
    const isLogout = routePath.toLowerCase() === 'auth/logout'

    if (isLogin) {
      const json = await res.json().catch(() => null)
      const token = json?.token
      const role = json?.user?.role || 'User'

      if (token) {
        const secure = process.env.NODE_ENV === 'production'
        setCookie(event, 'token', token, { httpOnly: true, secure, sameSite: 'lax', path: '/', maxAge: 60 * 60 * 24 * 30 })
        setCookie(event, 'access', token, { httpOnly: false, secure, sameSite: 'lax', path: '/', maxAge: 60 * 60 * 24 * 30 })
        setCookie(event, 'role', String(role), { httpOnly: false, secure, sameSite: 'lax', path: '/', maxAge: 60 * 60 * 24 * 30 })
        setCookie(event, 'auth', '1', { httpOnly: false, secure, sameSite: 'lax', path: '/', maxAge: 60 * 60 * 24 * 30 })
        if (json?.user) {
          setCookie(event, 'user', JSON.stringify(json.user), { httpOnly: false, secure, sameSite: 'lax', path: '/', maxAge: 60 * 60 * 24 * 30 })
        }
      }

      setResponseStatus(event, res.status)
      return json
    }

    if (isLogout) {
      const json = await res.json().catch(() => null)
      const names = ['token', 'access', 'access_token', 'role', 'auth', 'user']
      for (const n of names) {
        try { deleteCookie(event, n, { path: '/' }) } catch {}
      }
      setResponseStatus(event, res.status)
      return json ?? { ok: res.ok }
    }

    setResponseStatus(event, res.status)

    const responseCt = String(res.headers.get('content-type') || '').toLowerCase()
    if (responseCt) setResponseHeader(event, 'content-type', responseCt)

    if (responseCt.includes('application/json') || responseCt.includes('application/problem+json')) {
      const raw = await res.text().catch(() => '')
      try {
        return raw ? JSON.parse(raw) : null
      } catch {
        return {
          message: res.ok ? 'Upstream returned invalid JSON.' : 'تعذر إكمال الطلب من الخادم.',
          status: res.status,
          contentType: responseCt,
          preview: raw.slice(0, 400),
        }
      }
    }

    if (responseCt.startsWith('text/')) {
      const text = await res.text().catch(() => '')
      if (!res.ok) {
        return {
          message: text?.trim() || 'تعذر إكمال الطلب من الخادم.',
          status: res.status,
        }
      }
      return text
    }

    const buf = new Uint8Array(await res.arrayBuffer())
    return buf
  } catch (err: any) {
    console.error('BFF error:', err)
    setResponseStatus(event, 500)
    return { error: err?.message || 'BFF failed' }
  }
})
