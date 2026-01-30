// server/api/uploads/[...path].ts
import { defineEventHandler, getRouterParams, setHeader, setResponseStatus } from 'h3'

export default defineEventHandler(async (event) => {
  const config = useRuntimeConfig()

  const rawOrigin = String((config as any).apiOrigin || (config as any).public?.apiOrigin || '').trim()
  if (!rawOrigin) {
    setResponseStatus(event, 500)
    return Buffer.from('')
  }

  // uploads تكون على جذر الدومين: https://domain/uploads/...
  // لذلك إذا المستخدم حاط apiOrigin ينتهي بـ /api، نشيله
  const origin0 = rawOrigin.replace(/\/+$/, '')
  const origin = origin0.toLowerCase().endsWith('/api') ? origin0.slice(0, -4) : origin0

  const params = getRouterParams(event)
  const rest = String((params as any).path || '')

  const target = `${origin}/uploads/${rest}`.replace(/([^:]\/)\/+?/g, '$1')

  const res = await fetch(target, {
    method: 'GET',
    headers: {
      ...(event.node.req.headers?.cookie ? { cookie: String(event.node.req.headers.cookie) } : {}),
    },
  })

  setResponseStatus(event, res.status)

  const contentType = res.headers.get('content-type')
  if (contentType) setHeader(event, 'content-type', contentType)

  const arrayBuffer = await res.arrayBuffer()
  return Buffer.from(arrayBuffer)
})
