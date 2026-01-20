// server/api/uploads/[...path].ts
import { defineEventHandler, getRouterParams, setHeader } from 'h3'

export default defineEventHandler(async (event) => {
  const config = useRuntimeConfig()
  const apiOrigin = String((config as any).apiOrigin || '').replace(/\/$/, '')

  // /uploads/* is served by ASP.NET static files (wwwroot/uploads)
  const params = getRouterParams(event)
  const rest = String(params.path || '')
  const target = `${apiOrigin}/uploads/${rest}`.replace(/([^:]\/)\/+?/g, '$1')

  const res = await fetch(target, {
    method: 'GET',
    headers: {
      ...(event.node.req.headers?.cookie
        ? { cookie: String(event.node.req.headers.cookie) }
        : {}),
    },
  })

  const contentType = res.headers.get('content-type')
  if (contentType) setHeader(event, 'content-type', contentType)

  const arrayBuffer = await res.arrayBuffer()
  return Buffer.from(arrayBuffer)
})
