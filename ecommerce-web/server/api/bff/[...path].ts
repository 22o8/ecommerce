// ecommerce-web/server/api/bff/[...path].ts
export default defineEventHandler(async (event) => {
  try {
    const config = useRuntimeConfig()

    // لازم يكون مضبوط على Vercel (Production)
    const apiOrigin =
      (config.public as any).apiOrigin ||
      (config.public as any).apiBase ||
      process.env.NUXT_API_ORIGIN ||
      process.env.NUXT_PUBLIC_API_ORIGIN

    if (!apiOrigin) {
      setResponseStatus(event, 500)
      return {
        error: "Missing API origin. Set NUXT_API_ORIGIN on Vercel.",
      }
    }

    const method = (event.node.req.method || "GET").toUpperCase()
    const routePath = getRouterParam(event, "path") || ""
    const targetBase = apiOrigin.replace(/\/$/, "")
    const targetUrl = new URL(`${targetBase}/api/${routePath}`)

    // انقل كل query params عادي
    const incomingQuery = getQuery(event) as Record<string, any>

    // ✅ إذا اكو query=JSON حوله لباراميترات منفصلة
    if (typeof incomingQuery.query === "string" && incomingQuery.query.trim()) {
      try {
        const obj = JSON.parse(incomingQuery.query)
        if (obj && typeof obj === "object") {
          for (const [k, v] of Object.entries(obj)) {
            if (v === undefined || v === null) continue
            targetUrl.searchParams.set(k, String(v))
          }
        }
      } catch {
        // إذا JSON مو صالح، خليه مثل ما هو حتى ما يطيح السيرفر
        targetUrl.searchParams.set("query", incomingQuery.query)
      }
      delete incomingQuery.query
    }

    // باقي الباراميترات
    for (const [k, v] of Object.entries(incomingQuery)) {
      if (v === undefined || v === null) continue
      if (Array.isArray(v)) {
        v.forEach((vv) => targetUrl.searchParams.append(k, String(vv)))
      } else {
        targetUrl.searchParams.set(k, String(v))
      }
    }

    const headers = getRequestHeaders(event)
    // لا تمرر host/connection… الخ
    delete (headers as any).host
    delete (headers as any).connection
    delete (headers as any)["content-length"]

    const body =
      method === "GET" || method === "HEAD" ? undefined : await readRawBody(event)

    const res = await fetch(targetUrl.toString(), {
      method,
      headers: {
        ...headers,
        // ضمان نوع محتوى JSON إذا اكو body
        ...(body ? { "content-type": headers["content-type"] || "application/json" } : {}),
      },
      body: body || undefined,
    })

    // رجّع نفس الستاتس
    setResponseStatus(event, res.status)

    const ct = res.headers.get("content-type") || ""
    if (ct.includes("application/json")) return await res.json()
    return await res.text()
  } catch (err: any) {
    // ✅ لا تخليها UnhandledRejection
    console.error("BFF error:", err)
    setResponseStatus(event, 500)
    return { error: err?.message || "BFF failed" }
  }
})
