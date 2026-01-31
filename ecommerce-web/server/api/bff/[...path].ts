import {
  defineEventHandler,
  getCookie,
  getQuery,
  getRequestHeaders,
  getRouterParam,
  readRawBody,
  setCookie,
  setResponseHeader,
  setResponseStatus,
} from "h3"

export default defineEventHandler(async (event) => {
  try {
    const config = useRuntimeConfig()

    // Source of API origin (Vercel)
    const apiOrigin =
      (config.public as any).apiOrigin ||
      (config.public as any).apiBase ||
      process.env.NUXT_API_ORIGIN ||
      process.env.NUXT_PUBLIC_API_ORIGIN

    if (!apiOrigin) {
      setResponseStatus(event, 500)
      return { error: "Missing API origin. Set NUXT_API_ORIGIN / NUXT_PUBLIC_API_ORIGIN." }
    }

    const method = (event.node.req.method || "GET").toUpperCase()
    const routePath = (getRouterParam(event, "path") || "").replace(/^\/+/, "")

    const targetBase = apiOrigin.replace(/\/$/, "")
    // إذا apiOrigin ينتهي بـ /api لا نكررها، وإلا نضيف /api
    const apiBase = targetBase.endsWith("/api") ? targetBase : `${targetBase}/api`

    const targetUrl = new URL(`${apiBase}/${routePath}`)

    // -----------------------
    // Query params (يدعم query كـ JSON)
    // -----------------------
    const incomingQuery = getQuery(event) as Record<string, any>

    if (typeof incomingQuery.query === "string" && incomingQuery.query.trim()) {
      try {
        const obj = JSON.parse(incomingQuery.query)
        if (obj && typeof obj === "object") {
          for (const [k, v] of Object.entries(obj)) {
            if (v === undefined || v === null) continue
            if (Array.isArray(v)) v.forEach((vv) => targetUrl.searchParams.append(k, String(vv)))
            else targetUrl.searchParams.set(k, String(v))
          }
        }
      } catch {
        targetUrl.searchParams.set("query", incomingQuery.query)
      }
      delete incomingQuery.query
    }

    for (const [k, v] of Object.entries(incomingQuery)) {
      if (v === undefined || v === null) continue
      if (Array.isArray(v)) v.forEach((vv) => targetUrl.searchParams.append(k, String(vv)))
      else targetUrl.searchParams.set(k, String(v))
    }

    // -----------------------
    // Headers
    // -----------------------
    const headers = getRequestHeaders(event) as Record<string, any>

    // لا تمرر host/connection/content-length
    delete headers.host
    delete headers.connection
    delete headers["content-length"]

    // ✅ الحل الجذري للـ 401 بالفرونت:
    // خذ التوكن من:
    // 1) Authorization header (إذا موجود)
    // 2) أو Cookie token / access_token
    const tokenFromCookie = getCookie(event, "token") || getCookie(event, "access_token")
    const authHeader = headers.authorization || headers.Authorization

    if (!authHeader && tokenFromCookie) {
      headers.authorization = `Bearer ${tokenFromCookie}`
    }

    // -----------------------
    // Body
    // -----------------------
    const body =
      method === "GET" || method === "HEAD" ? undefined : await readRawBody(event)

    const res = await fetch(targetUrl.toString(), {
      method,
      headers: {
        ...headers,
        ...(body ? { "content-type": headers["content-type"] || "application/json" } : {}),
      },
      body: body || undefined,
    })

    setResponseStatus(event, res.status)

    const ct = res.headers.get("content-type") || ""
    const isJson = ct.includes("application/json") || ct.includes("application/problem+json")

    // -----------------------
    // ✅ إذا هذا Login: خزّن التوكن كـ Cookie على دومين Vercel
    // حتى كل طلبات admin تصير Authorized تلقائياً
    // -----------------------
    const isLogin =
      method === "POST" &&
      (routePath.toLowerCase() === "auth/login" || routePath.toLowerCase().endsWith("/auth/login"))

    if (isJson) {
      const data = await res.json()

      if (isLogin && res.ok && data?.token) {
        const secure = process.env.NODE_ENV === "production"

        setCookie(event, "token", String(data.token), {
          httpOnly: true,
          secure,
          sameSite: "lax",
          path: "/",
          maxAge: 60 * 60 * 24 * 30, // 30 يوم
        })

        // مفيد للفرونت إذا تحب تقرأ role بدون فك JWT
        if (data?.user?.role) {
          setCookie(event, "role", String(data.user.role), {
            httpOnly: false,
            secure,
            sameSite: "lax",
            path: "/",
            maxAge: 60 * 60 * 24 * 30,
          })
        }
      }

      return data
    }

    // رجّع نص إذا مو JSON
    const text = await res.text()
    // بعض الأحيان Vercel يرجّع text/html للـ 401
    setResponseHeader(event, "content-type", ct || "text/plain; charset=utf-8")
    return text
  } catch (err: any) {
    console.error("BFF error:", err)
    setResponseStatus(event, 500)
    return { error: err?.message || "BFF failed" }
  }
})
