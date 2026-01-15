// server/api/product-images/[id].ts
// Public helper to fetch a product's images from the backend.
// Swagger shows public endpoints like GET /{id}/images (without /api prefix).

import { defineEventHandler, getRouterParams, setResponseStatus } from 'h3'

function getCookieValue(cookieHeader: string, name: string) {
  const parts = cookieHeader.split(';').map((p) => p.trim())
  for (const p of parts) {
    if (!p) continue
    const idx = p.indexOf('=')
    if (idx === -1) continue
    const k = p.slice(0, idx)
    if (k === name) return decodeURIComponent(p.slice(idx + 1))
  }
  return ''
}

export default defineEventHandler(async (event) => {
  // ✅ SSL محلي للتطوير فقط
  if (process.env.NODE_ENV !== 'production') {
    process.env.NODE_TLS_REJECT_UNAUTHORIZED = '0'
  }

  const config = useRuntimeConfig()
  // ✅ نفس مصدر الـ API المستخدم في باقي السيرفر routes
  const apiOrigin = String((config as any).public?.apiOrigin || (config as any).apiOrigin || 'http://localhost:7043')
    .replace(/\/$/, '')

  const { id } = getRouterParams(event)
  const productId = String(id || '')
  if (!productId) {
    setResponseStatus(event, 400)
    return { message: 'Missing id' }
  }

  // ✅ بعض الأحيان الـ Swagger يطلع المسار بدون /api، وأحياناً الصور تكون ضمن admin/products
  // نخليها مرنة: نجرب أكثر من مسار ونرجّع دائماً Array حتى الـ UI يشتغل بثبات.
  const candidates = [
    `${apiOrigin}/api/admin/products/${productId}/images`,
    `${apiOrigin}/api/Products/${productId}/images`,
    `${apiOrigin}/api/products/${productId}/images`,
    `${apiOrigin}/${productId}/images`,
  ].map((u) => u.replace(/([^:]\/)\/+?/g, '$1'))

  async function tryFetch(url: string) {
    const cookieHeader = String(event.node.req.headers?.cookie || '')
    // ✅ الباك يتوقع Bearer Token (مو Cookie) لواجهات admin، فنسحب التوكن من الكوكي ونمرّره.
    const token = cookieHeader ? getCookieValue(cookieHeader, 'token') : ''

    const res = await fetch(url, {
      method: 'GET',
      headers: {
        accept: 'application/json',
        ...(cookieHeader ? { cookie: cookieHeader } : {}),
        ...(token ? { authorization: `Bearer ${token}` } : {}),
      },
    })
    if (!res.ok) return null
    const text = await res.text()
    try {
      return JSON.parse(text)
    } catch {
      return null
    }
  }

  try {
    let data: any = null
    for (const url of candidates) {
      data = await tryFetch(url)
      if (data) break
    }

    // Normalize to array
    if (Array.isArray(data)) return data
    if (data && Array.isArray(data.items)) return data.items
    if (data && Array.isArray(data.images)) return data.images
    return []
  } catch (e: any) {
    setResponseStatus(event, 500)
    return { message: e?.message || 'Failed to fetch images' }
  }
})
