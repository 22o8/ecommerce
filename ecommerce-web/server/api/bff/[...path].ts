// ecommerce-web/server/api/bff/[...path].ts
export default defineEventHandler(async (event) => {
  try {
    const config = useRuntimeConfig()

    // IMPORTANT:
    // لا تستخدم apiBase هنا إطلاقاً.
    // إذا apiOrigin صار فاضي/غلط، كان الكود ياخذ "/api/bff" (apiBase)
    // ويسوي recursion على نفسه -> 500 على Vercel.
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

    // ✅ المسار الافتراضي للـ API يكون تحت /api
    // لكن مسارات الملفات المرفوعة تكون عادةً تحت /uploads (بدون /api)
    // إذا عملنا لها proxy إلى /api/uploads راح نحصل 404.
    const isUploads = routePath.startsWith('uploads/') || routePath === 'uploads'
    const apiBase = targetBase.endsWith('/api') ? targetBase : `${targetBase}/api`
    const targetUrl = new URL(isUploads ? `${targetBase}/${routePath}` : `${apiBase}/${routePath}`)

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
    // ✅ منع ضغط الاستجابة من المصدر لتفادي JSON مضغوط/غير قابل للقراءة على بعض الشبكات/VPN
    delete headers['accept-encoding']
    delete headers['Accept-Encoding']
    headers['accept-encoding'] = 'identity'

    // ✅ 1) إذا الفرونت مرسل Authorization خليّه مثل ما هو
    const authHeader = headers.authorization || headers.Authorization

    // ✅ 2) fallback: خذ التوكن من Cookie إذا ماكو Authorization
    const tokenFromCookie =
      getCookie(event, 'token') ||
      getCookie(event, 'access_token') ||
      getCookie(event, 'access') // ✅ نسمح لـ access هم
    if (!authHeader && tokenFromCookie) {
      headers.authorization = `Bearer ${tokenFromCookie}`
    }

    // Body
    const body = method === 'GET' || method === 'HEAD'
      ? undefined
      : await readRawBody(event, false)

    const res = await fetch(targetUrl.toString(), {
      method,
      headers: { ...headers },
      body: body || undefined,
    })

    const isLogin = routePath.toLowerCase() === 'auth/login' && method === 'POST'
    const isLogout = routePath.toLowerCase() === 'auth/logout'

    // ✅ login: خزن cookies
    if (isLogin) {
      const json = await res.json().catch(() => null)

      const token = json?.token
      const role = json?.user?.role || 'User'

      if (token) {
        const secure = process.env.NODE_ENV === 'production'

        // token httpOnly (أمني)
        setCookie(event, 'token', token, {
          httpOnly: true,
          secure,
          sameSite: 'lax',
          path: '/',
          maxAge: 60 * 60 * 24 * 30,
        })

        // ✅ access غير httpOnly (حل iOS/Telegram 401)
        setCookie(event, 'access', token, {
          httpOnly: false,
          secure,
          sameSite: 'lax',
          path: '/',
          maxAge: 60 * 60 * 24 * 30,
        })

        // role غير httpOnly
        setCookie(event, 'role', String(role), {
          httpOnly: false,
          secure,
          sameSite: 'lax',
          path: '/',
          maxAge: 60 * 60 * 24 * 30,
        })

        // auth flag غير httpOnly
        setCookie(event, 'auth', '1', {
          httpOnly: false,
          secure,
          sameSite: 'lax',
          path: '/',
          maxAge: 60 * 60 * 24 * 30,
        })

        // (اختياري) خزن user كـ JSON
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

    // ✅ logout: امسح الكوكيز
    if (isLogout) {
      deleteCookie(event, 'token', { path: '/' })
      deleteCookie(event, 'access', { path: '/' })
      deleteCookie(event, 'role', { path: '/' })
      deleteCookie(event, 'auth', { path: '/' })
      deleteCookie(event, 'user', { path: '/' })

      setResponseStatus(event, 200)
      return { ok: true }
    }

    setResponseStatus(event, res.status)

const ct = res.headers.get('content-type') || ''
const ce = (res.headers.get('content-encoding') || '').toLowerCase()

// اقرأ البودي كباينري دائماً حتى نقدر نعالج حالات (JSON مضغوط/رد HTML/حماية) بدون ما ينكسر
let buf = Buffer.from(await res.arrayBuffer())

// ✅ إذا أكو ضغط مُعلن نفكّه
try {
  if (ce.includes('gzip')) {
    const { gunzipSync } = await import('node:zlib')
    buf = gunzipSync(buf)
  } else if (ce.includes('br')) {
    const { brotliDecompressSync } = await import('node:zlib')
    buf = brotliDecompressSync(buf)
  } else if (ce.includes('deflate')) {
    const { inflateSync } = await import('node:zlib')
    buf = inflateSync(buf)
  } else {
    // ✅ أحياناً يرجع باينري مضغوط بدون هيدر صحيح (يصير مع بعض الـ proxies/VPN)
    // نحاول نفك gzip كـ محاولة أخيرة (وإذا فشل نتجاهل)
    const { gunzipSync } = await import('node:zlib')
    try { buf = gunzipSync(buf) } catch {}
  }
} catch {}

// ✅ إذا مو JSON رجّعه كباينري (صور/ملفات)
if (!ct.includes('application/json')) {
  if (ct) setResponseHeader(event, 'content-type', ct)
  const cl = res.headers.get('content-length')
  if (cl) setResponseHeader(event, 'content-length', cl)
  const cc = res.headers.get('cache-control')
  if (cc) setResponseHeader(event, 'cache-control', cc)
  return buf
}

// ✅ حاول parse JSON بأمان
const text = buf.toString('utf-8')
try {
  return JSON.parse(text)
} catch (e: any) {
  // رجّع خطأ مفهوم بدل "Unexpected token" + جزء من الرد حتى نعرف شنو صار (Block/HTML)
  return {
    error: 'Upstream returned invalid JSON (maybe blocked/challenge/compressed response).',
    status: res.status,
    contentType: ct,
    contentEncoding: ce,
    preview: text.slice(0, 400),
  }
}
  } catch (err: any) {
    console.error('BFF error:', err)
    setResponseStatus(event, 500)
    return { error: err?.message || 'BFF failed' }
  }
})
