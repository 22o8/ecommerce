/// <reference lib="dom" />
// server/api/bff/[...path].ts
import {
  defineEventHandler,
  getRouterParams,
  getQuery,
  getRequestHeaders,
  getHeader,
  getCookie,
  readMultipartFormData,
  readBody,
  setHeader,
  setResponseStatus,
} from "h3"
import { $fetch } from "ofetch"

export default defineEventHandler(async (event) => {
  // ✅ SSL محلي للتطوير فقط
  if (process.env.NODE_ENV !== "production") {
    process.env.NODE_TLS_REJECT_UNAUTHORIZED = "0"
  }

  const config = useRuntimeConfig()

  // ✅ apiOrigin لازم يكون بدون /api
  // (إذا انحط بالغلط مثل: https://domain.com/api راح نسويه https://domain.com)
  let apiOrigin = String((config as any).apiOrigin || '').trim()
  apiOrigin = apiOrigin.replace(/\/+$/g, "")
  apiOrigin = apiOrigin.replace(/\/api$/i, "")

  const apiBase = `${apiOrigin}/api` // مثال: https://localhost:7043/api

  const method = (event.node.req.method || "GET").toUpperCase()
  const params = getRouterParams(event)
  const restPath = String(params.path || "")
  const query = getQuery(event)

  const targetUrl = `${apiBase}/${restPath}`.replace(/([^:]\/)\/+/g, "$1")

  // ✅ Forward headers (بدون host/content-length)
  const headers: Record<string, string> = {}
  for (const [k, v] of Object.entries(getRequestHeaders(event))) {
    if (!v) continue
    const key = k.toLowerCase()
    if (key === "host") continue
    if (key === "content-length") continue
    headers[key] = Array.isArray(v) ? v.join(",") : String(v)
  }

  // ✅ Authorization من الهيدر أو من cookie token
  const authHeader = getHeader(event, "authorization")
  if (authHeader) headers["authorization"] = authHeader
  else {
    const token = getCookie(event, "token")
    if (token) headers["authorization"] = `Bearer ${token}`
  }

  const contentType = (getHeader(event, "content-type") || "").toLowerCase()
  const hasBody = !["GET", "HEAD"].includes(method)

  let body: any = undefined

  // ✅ multipart/form-data (رفع صور / ملفات)
  if (hasBody && contentType.includes("multipart/form-data")) {
    const parts = await readMultipartFormData(event)
    const fd = new globalThis.FormData()

    for (const part of parts || []) {
      if (!part) continue

      // ملف
      if (part.filename) {
        const type = (part as any).type || "application/octet-stream"

        // ✅ تحويل مضمون إلى Buffer حتى نتجنب SharedArrayBuffer typing
        const buf = Buffer.isBuffer(part.data)
          ? part.data
          : Buffer.from(part.data as any)

        const blob = new globalThis.Blob([buf], { type })
        fd.append(part.name || "file", blob, part.filename || "file")
      } else {
        // حقل نصي
        fd.append(part.name || "field", part.data?.toString?.() ?? "")
      }
    }

    body = fd

    // مهم: لا ترسل content-type حتى الـ fetch يولّد boundary
    delete headers["content-type"]
  } else if (hasBody) {
    // ✅ JSON
    body = await readBody(event).catch(() => undefined)
    headers["accept"] = "application/json"
  } else {
    headers["accept"] = "application/json"
  }

  try {
    const res = await $fetch.raw(targetUrl, {
      method: method as any,
      headers,
      query,
      body,
      timeout: 20000,
    })

    const setCookie = res.headers.get("set-cookie")
    if (setCookie) setHeader(event, "set-cookie", setCookie)

    return res._data
  } catch (err: any) {
    const statusCode = err?.statusCode || err?.response?.status || 500
    const upstreamData = err?.data
    const message =
      upstreamData?.message ||
      upstreamData?.title ||
      err?.message ||
      "fetch failed"

    setResponseStatus(event, statusCode)

    // لا نعرض الـ targetUrl بالإنتاج
    const isProd = process.env.NODE_ENV === "production"

    return {
      message,
      ...(upstreamData && typeof upstreamData === "object" ? { upstream: upstreamData } : {}),
      ...(isProd ? {} : { targetUrl, method }),
    }
  }
})