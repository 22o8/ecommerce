// ecommerce-web/server/api/bff/[...path].ts
export default defineEventHandler(async (event) => {
  try {
    const config = useRuntimeConfig()

    // لا تستخدم apiBase هنا إطلاقاً حتى لا يصير recursion
    const apiOrigin =
      (config as any).apiOrigin ||
      (config.public as any).apiOrigin ||
      process.env.NUXT_API_ORIGIN ||
      process.env.NUXT_PUBLIC_API_ORIGIN ||
      "https://ecommerce-api-22o8.fly.dev" // ✅ fallback آمن للإنتاج

    const method = (event.node.req.method || "GET").toUpperCase()
    const routePath = getRouterParam(event, "path") || ""

    const targetBase = String(apiOrigin).replace(/\/$/, "")
    const isUploads = routePath.startsWith("uploads/") || routePath === "uploads"
    const apiBase = targetBase.endsWith("/api") ? targetBase : `${targetBase}/api`
    const targetUrl = new URL(isUploads ? `${targetBase}/${routePath}` : `${apiBase}/${routePath}`)

    // Query params
    const incomingQuery = getQuery(event) as Record<string, any>

    // query=JSON => params
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

    // Headers (sanitize)
    const incomingHeaders = getRequestHeaders(event) as Record<string, any>
    const headers: Record<string, string> = {}
    for (const [k, v] of Object.entries(incomingHeaders)) {
      if (v === undefined || v === null) continue
      headers[k.toLowerCase()] = Array.isArray(v) ? String(v[0]) : String(v)
    }

    delete headers["host"]
    delete headers["connection"]
    delete headers["content-length"]

    // منع ضغط الاستجابة من المصدر (Edge/Serverless) لتفادي مشاكل parsing
    delete headers["accept-encoding"]
    headers["accept-encoding"] = "identity"

    // ✅ 1) Authorization إن وجد
    const authHeader = headers["authorization"]

    // ✅ 2) fallback من Cookie
    const tokenFromCookie =
      getCookie(event, "token") ||
      getCookie(event, "access_token") ||
      getCookie(event, "access")
    if (!authHeader && tokenFromCookie) {
      headers["authorization"] = `Bearer ${tokenFromCookie}`
    }

    // ✅ WhatsApp Checkout secret
    if (routePath.toLowerCase() === "checkout/cart/whatsapp") {
      const secret = process.env.CHECKOUT_SECRET
      if (secret) headers["x-checkout-secret"] = secret
    }

    // Body
    const body =
      method === "GET" || method === "HEAD" ? undefined : await readRawBody(event, false)

    const res = await fetch(targetUrl.toString(), {
      method,
      headers,
      body: body || undefined,
    })

    const isLogin = routePath.toLowerCase() === "auth/login" && method === "POST"
    const isLogout = routePath.toLowerCase() === "auth/logout"

    // ✅ login: خزن cookies
    if (isLogin) {
      const json = await res.json().catch(() => null)
      const token = (json as any)?.token
      const role = (json as any)?.user?.role || "User"

      if (token) {
        const secure = process.env.NODE_ENV === "production"

        setCookie(event, "token", token, {
          httpOnly: true,
          secure,
          sameSite: "lax",
          path: "/",
          maxAge: 60 * 60 * 24 * 30,
        })

        // access غير httpOnly (حل iOS/Telegram)
        setCookie(event, "access", token, {
          httpOnly: false,
          secure,
          sameSite: "lax",
          path: "/",
          maxAge: 60 * 60 * 24 * 30,
        })

        setCookie(event, "role", String(role), {
          httpOnly: false,
          secure,
          sameSite: "lax",
          path: "/",
          maxAge: 60 * 60 * 24 * 30,
        })

        setCookie(event, "auth", "1", {
          httpOnly: false,
          secure,
          sameSite: "lax",
          path: "/",
          maxAge: 60 * 60 * 24 * 30,
        })

        if ((json as any)?.user) {
          setCookie(event, "user", JSON.stringify((json as any).user), {
            httpOnly: false,
            secure,
            sameSite: "lax",
            path: "/",
            maxAge: 60 * 60 * 24 * 30,
          })
        }
      }

      setResponseStatus(event, res.status)
      return json
    }

    // ✅ logout: امسح cookies
    if (isLogout) {
      const json = await res.json().catch(() => null)
      const names = ["token", "access", "access_token", "role", "auth", "user"]
      for (const n of names) {
        try {
          deleteCookie(event, n, { path: "/" })
        } catch {
          /* ignore */
        }
      }
      setResponseStatus(event, res.status)
      return json ?? { ok: res.ok }
    }

    // ✅ مرّر status كما هو (لا ترمي 500)
    setResponseStatus(event, res.status)

    const ct = (res.headers.get("content-type") || "").toLowerCase()

    // إذا JSON
    if (ct.includes("application/json") || ct.includes("application/problem+json")) {
      const raw = await res.text().catch(() => "")
      if (!raw) return null
      try {
        return JSON.parse(raw)
      } catch {
        return {
          error: "Upstream returned invalid JSON.",
          status: res.status,
          contentType: ct,
          preview: raw.slice(0, 400),
          targetUrl: targetUrl.toString(),
        }
      }
    }

    // إذا صورة/ملف
    const isBinary =
      ct.startsWith("image/") ||
      ct.includes("application/octet-stream") ||
      ct.includes("application/pdf")

    if (isBinary) {
      setResponseHeader(event, "content-type", ct || "application/octet-stream")
      const buf = new Uint8Array(await res.arrayBuffer())
      return buf
    }

    // غير ذلك: رجع نص (حتى لو 401/403 بدون body، ما يكسر الفرونت)
    const text = await res.text().catch(() => "")
    if (ct) setResponseHeader(event, "content-type", ct)
    return text || null
  } catch (err: any) {
    console.error("BFF error:", err)
    setResponseStatus(event, 500)
    return { error: err?.message || "BFF failed" }
  }
})
