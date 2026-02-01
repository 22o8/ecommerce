// ecommerce-web/server/api/uploads/[...path].ts
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
      return { error: "Missing API origin. Set NUXT_API_ORIGIN / NUXT_PUBLIC_API_ORIGIN." }
    }

    const routePath = getRouterParam(event, "path") || ""
    const targetBase = apiOrigin.replace(/\/$/, "")

    // لاحظ: uploads بدون /api
    const targetUrl = new URL(`${targetBase}/uploads/${routePath}`)

    // مرر query params إذا موجودة
    const q = getQuery(event) as Record<string, any>
    for (const [k, v] of Object.entries(q)) {
      if (v === undefined || v === null) continue
      if (Array.isArray(v)) v.forEach((vv) => targetUrl.searchParams.append(k, String(vv)))
      else targetUrl.searchParams.set(k, String(v))
    }

    const res = await fetch(targetUrl.toString(), { method: "GET" })

    setResponseStatus(event, res.status)

    // مرر نوع المحتوى وكاش
    const ct = res.headers.get("content-type") || "application/octet-stream"
    setResponseHeader(event, "content-type", ct)

    const cache = res.headers.get("cache-control")
    if (cache) setResponseHeader(event, "cache-control", cache)

    // رجع bytes (مهم للصور)
    const buf = Buffer.from(await res.arrayBuffer())
    return buf
  } catch (err: any) {
    console.error("Uploads proxy error:", err)
    setResponseStatus(event, 500)
    return { error: err?.message || "Uploads proxy failed" }
  }
})
