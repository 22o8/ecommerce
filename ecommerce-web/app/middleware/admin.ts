export default defineNuxtRouteMiddleware((to) => {
  // حماية من أي undefined
  if (!to?.path) return

  // إذا ماكو توكن -> لوجن
  const token = useCookie<string | null>('token').value
  if (!token) {
    return navigateTo(`/login?redirect=${encodeURIComponent(to.fullPath || '/admin')}`)
  }
})
