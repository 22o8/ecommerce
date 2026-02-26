export const runtime = 'nodejs'

import { defineEventHandler, getHeader, getHeaders, getRequestURL, readRawBody, setResponseHeader, setResponseStatus } from 'h3'
import { parseCookies } from 'h3'

const HOP_BY_HOP = new Set([
  'connection','keep-alive','proxy-authenticate','proxy-authorization','te','trailer','transfer-encoding','upgrade'
])

// لا تمرّر هذه الرؤوس كما هي لأننا نقرأ/نعيد كتابة البودي داخل Nitro.
// تمرير content-length/content-encoding من الـ upstream يسبب عدم تطابق ويؤدي إلى
// net::ERR_HTTP2_PROTOCOL_ERROR في المتصفح.
const STRIP_RESPONSE_HEADERS = new Set([
  'content-length',
  'content-encoding',
  'transfer-encoding'
])

function sanitizeHeaders(headers: Record<string, string | string[] | undefined>) {
  const out: Record<string, string> = {}
  for (const [k, v] of Object.entries(headers)) {
    if (!v) continue
    const lk = k.toLowerCase()
    if (HOP_BY_HOP.has(lk)) continue
    if (Array.isArray(v)) out[k] = v.join(', ')
    else out[k] = v
  }
  // لا نرسل Host الخاص بـ Vercel إلى Fly
  delete out.host
  delete out.Host
  return out
}

export default defineEventHandler(async (event) => {
  const config = useRuntimeConfig()

  const apiOrigin =
    (config as any).apiOrigin ||
    process.env.NUXT_API_ORIGIN ||
    process.env.NUXT_PUBLIC_API_ORIGIN ||
    'https://ecommerce-api-22o8.fly.dev'

  const p = event.context.params?.path
  const parts = Array.isArray(p) ? p : [p]
  const joined = parts.filter(Boolean).join('/')

  const targetPath = `/api/${joined}`
  const targetUrl = new URL(targetPath, apiOrigin).toString()

  // Headers
  const headers = sanitizeHeaders(getHeaders(event) as any)

  // Auth: مرر Authorization إذا موجود، أو خذه من الكوكيز (access / token / access_token)
  const authHeader = getHeader(event, 'authorization')
  if (!authHeader) {
    const cookies = parseCookies(event)
    const token = cookies.access || cookies.token || cookies.access_token
    if (token) headers['authorization'] = token.startsWith('Bearer ') ? token : `Bearer ${token}`
  }

  // Body
  let body: any = undefined
  const method = event.method || 'GET'
  if (!['GET', 'HEAD'].includes(method.toUpperCase())) {
    body = await readRawBody(event)
  }

  try {
    const res = await fetch(targetUrl, {
      method,
      headers,
      body
    })

    // Pass-through status + headers (بشكل آمن)
    setResponseStatus(event, res.status)
    res.headers.forEach((v, k) => {
      const lk = k.toLowerCase()
      if (HOP_BY_HOP.has(lk)) return
      if (STRIP_RESPONSE_HEADERS.has(lk)) return
      // عادةً ما نترك set-cookie يمر، لأن بعض استجابات المصادقة قد تعتمد عليه
      setResponseHeader(event, k, v)
    })

    // رجّع البودي خام بدون إعادة تفسير، حتى نحافظ على التطابق بين الـ headers والبودي.
    // Nitro سيضع Content-Length الصحيح تلقائياً.
    const ab = await res.arrayBuffer()
    return Buffer.from(ab)
  } catch (err: any) {
    // رجّع تفاصيل كافية بدل 500 أعمى (يساعدنا نعرف وين المشكلة)
    setResponseStatus(event, 502)
    return {
      error: 'BFF proxy failed',
      message: err?.message || String(err),
      apiOrigin,
      targetUrl,
      path: getRequestURL(event).pathname,
      traceId: (event.node?.req as any)?.headers?.['x-vercel-id'] || undefined
    }
  }
})
