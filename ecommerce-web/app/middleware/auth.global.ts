export default defineNuxtRouteMiddleware((to) => {
  const route = to ?? useRoute?.()
  const path = String(route?.path ?? "").toLowerCase()

  const protectedPaths = [
    "/account",
    "/orders",
    "/service-requests",
  ]

  const needsAuth = protectedPaths.some(p => path.startsWith(p))
  if (!needsAuth) return

  const auth = useAuthStore()
  if (!auth.isAuthed) {
    const redirect = encodeURIComponent(route?.fullPath || "/")
    return navigateTo(`/login?redirect=${redirect}`)
  }
})
