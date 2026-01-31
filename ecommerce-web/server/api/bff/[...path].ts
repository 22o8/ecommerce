// ecommerce-web/server/api/bff/[...path].ts
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
    const routePath = getRouterParam(event, "path") || ""

    const targetBase = apiOrigin.replace(/\/$/, "")

    // ✅ إذا apiOrigin ينتهي بـ /api لا نكررها، وإلا نضيف /api
    const apiBase = targetBase.endsWith("/api") ? targetBase : `${targetBase}/api`

    const targetUrl = new URL(`${apiBase}/${routePath}`)

    // Query params
    const incomingQuery = getQuery(event) as Record<string, any>

    // ✅ إذا اكو query=JSON حوله لباراميترات منفصلة
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

    // Headers
    const headers = getRequestHeaders(event) as Record<string, any>

    // لا تمرر host/connection/content-length… الخ
    delete headers.host
    delete headers.connection
    delete headers["content-length"]

    // ✅ الحل الجذري: خذ التوكن من أي مكان منطقي
    // 1) من Authorization header (إذا موجود)
    // 2) أو من Cookie token
    // 3) أو من Cookie access_token
    const tokenFromCookie =
      getCookie(event, "token") || getCookie(event, "access_token")

    const authHeader =
      headers.authorization || headers.Authorization

    // إذا ماكو Authorization، نبنيه من الكوكي
    if (!authHeader && tokenFromCookie) {
      headers.authorization = `Bearer ${tokenFromCookie}`
    }

    // Body
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
    if (ct.includes("application/json")) return await res.json()
    return await res.text()
  } catch (err: any) {
    console.error("BFF error:", err)
    setResponseStatus(event, 500)
    return { error: err?.message || "BFF failed" }
  }
})
