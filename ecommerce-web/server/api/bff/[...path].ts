import { defineEventHandler, getQuery, readBody, setResponseStatus, setHeader, getRouterParams } from 'h3'

/**
 * BFF Proxy:
 * - يوجّه أي طلب /api/bff/* إلى الـ API الحقيقي (مثلاً Fly.io)
 * - يدعم GET/POST/PUT/PATCH/DELETE
 * - يمنع مشكلة تكرار /api (إذا المستخدم حاط apiOrigin ينتهي بـ /api)
 * - يرجّع نفس Status/Headers من السيرفر الهدف بدل ما يحولها دائماً إلى 500
 */
export default defineEventHandler(async (event) => {
  const config = useRuntimeConfig()

  // apiOrigin يمكن يجي بصيغ مختلفة:
  // - https://domain
  // - https://domain/
  // - https://domain/api
  // نخليه دائماً base = https://domain/api بدون تكرار
  const rawOrigin = String(config.apiOrigin || config.public?.apiOrigin || '').trim()
  if (!rawOrigin) {
    setResponseStatus(event, 500)
    return { error: 'Missing apiOrigin. Set NUXT_API_ORIGIN in Vercel.' }
  }

  const origin = rawOrigin.replace(/\/+$/, '')
  const apiBase = origin.toLowerCase().endsWith('/api') ? origin : `${origin}/api`

  const method = (event.node.req.method || 'GET').toUpperCase()

  // Path بعد /api/bff/
  const params = getRouterParams(event) as { path?: string }
  const restPath = params.path || ''
  const targetUrl = `${apiBase}/${restPath}`

  const query = getQuery(event)

  // نفس الهيدرات اللي نحتاجها بالـ proxy (Authorization/Cookie ...إلخ)
  const headers: Record<string, string> = {}
  const auth = event.node.req.headers['authorization']
  if (auth) headers.authorization = String(auth)

  const cookie = event.node.req.headers['cookie']
  if (cookie) headers.cookie = String(cookie)

  // Forward content-type لو موجود
  const contentType = event.node.req.headers['content-type']
  if (contentType) headers['content-type'] = String(contentType)

  try {
    // إذا ميثود يسمح بالبودي اقرأه
    let body: any = undefined
    if (['POST', 'PUT', 'PATCH', 'DELETE'].includes(method)) {
      // readBody يرمي error لو ماكو body لبعض الحالات، نخليها safe
      try { body = await readBody(event) } catch { body = undefined }
    }

    // raw حتى نقدر نمرر status والـ headers مثل ما هي
    const res = await $fetch.raw(targetUrl, {
      method,
      query,
      body,
      headers,
      retry: 0,
    })

    // مرر status
    setResponseStatus(event, res.status)

    // مرر أهم الهيدرات (خصوصاً content-type)
    const ct = res.headers.get('content-type')
    if (ct) setHeader(event, 'content-type', ct)

    // مرر الباقي إذا تحب (ممكن تضيف headers أخرى عند الحاجة)
    // مثال: cache-control, etag ...
    const cache = res.headers.get('cache-control')
    if (cache) setHeader(event, 'cache-control', cache)

    return res._data
  } catch (err: any) {
    // إذا API رجّع status (مثلاً 401/404/500) نرجعه مثل ما هو بدل 500 ثابت
    const status = err?.response?.status || err?.statusCode || 500
    setResponseStatus(event, status)

    const msg =
      err?.data?.message ||
      err?.response?._data?.message ||
      err?.message ||
      'BFF proxy error'

    return {
      error: msg,
      targetUrl,
      status,
    }
  }
})
