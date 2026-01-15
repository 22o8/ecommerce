// app/middleware/auth.global.ts
export default defineNuxtRouteMiddleware((to) => {
  const auth = useAuthStore()

  const protectedPaths = ['/account', '/orders', '/service-requests', '/admin']
  const needsAuth = protectedPaths.some((p) => to.path.startsWith(p))

  if (needsAuth && !auth.isAuthed) {
    return navigateTo('/login')
  }
})
