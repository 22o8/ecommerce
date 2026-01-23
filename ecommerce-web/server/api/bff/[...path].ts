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
  const apiOrigin = String((config as any).apiOrigin || '').replace(/\/$/, '')
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

  // =========================
  // ✅ Fixes لمسارات الواجهة العامة
  // =========================
  const isGuid = (s: string) =>
    /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i.test(s)

  // 1) by-slug: بعض الصفحات كانت ترسل GUID بدل slug
  if (event.method === "GET" && restPath === "Products/by-slug") {
    const slugRaw = String((query as any)?.slug ?? "").trim()

    // جرّب المسار الأصلي أولاً
    try {
      return await $fetch(`${apiBase}/Products/by-slug`, {
        method: "GET",
        headers,
        query: { slug: slugRaw },
      })
    } catch {
      // fallback: جلب قائمة المنتجات والبحث محلياً
      const list: any = await $fetch(`${apiBase}/Products`, {
        method: "GET",
        headers,
        query,
      })

      const items = Array.isArray(list?.items) ? list.items : Array.isArray(list) ? list : []

      const normalizeSlug = (v: any) =>
        String(v ?? "")
          .trim()
          .toLowerCase()
          .replace(/\s+/g, "-")
          .replace(/[^a-z0-9\-\u0600-\u06FF]+/g, "")

      const found = items.find((p: any) => {
        if (isGuid(slugRaw)) return String(p?.id) === slugRaw
        return normalizeSlug(p?.slug ?? p?.title) === normalizeSlug(slugRaw)
      })

      if (!found) {
        setResponseStatus(event, 404)
        return {
          message: `Product not found (slug=${slugRaw})`,
          targetUrl: `${apiBase}/Products/by-slug`,
          method: "GET",
        }
      }

      return found
    }
  }

  // 2) صور المنتج العامة: جرّب endpoint العام ثم fallback إلى admin (مع التوكن من الكوكي)
  if (event.method === "GET") {
    const m = restPath.match(/^Products\/([^/]+)\/images$/)
    if (m?.[1]) {
      const id = m[1]
      try {
        return await $fetch(`${apiBase}/Products/${id}/images`, { method: "GET", headers, query })
      } catch (e: any) {
        // إذا فشل العام (404/401/403) جرّب admin
        try {
          return await $fetch(`${apiBase}/admin/products/${id}/images`, { method: "GET", headers, query })
        } catch {
          throw e
        }
      }
    }
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
    })

    const setCookie = res.headers.get("set-cookie")
    if (setCookie) setHeader(event, "set-cookie", setCookie)

    return res._data
  } catch (err: any) {
    const statusCode = err?.statusCode || err?.response?.status || 500
    const message = err?.data?.message || err?.message || "fetch failed"
    setResponseStatus(event, statusCode)
    return { message, targetUrl, method }
  }
})