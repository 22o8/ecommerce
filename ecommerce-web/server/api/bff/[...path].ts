export const runtime = 'nodejs'

import { proxyRequest } from 'h3'

export default defineEventHandler(async (event) => {
  const config = useRuntimeConfig()

  const apiOrigin =
    (config as any).apiOrigin ||
    process.env.NUXT_API_ORIGIN ||
    'https://ecommerce-api-22o8.fly.dev'

  const path = event.context.params?.path
  const target = `${apiOrigin}/api/${path}`

  return proxyRequest(event, target)
})
