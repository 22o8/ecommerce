// app/middleware/admin.ts
export default defineNuxtRouteMiddleware(() => {
  const auth = useAuthStore()

  // auth.userData هو computed(ref) داخل store
  const u: any = (auth as any)?.userData?.value || (auth as any)?.user?.value || (auth as any)?.user || null

  const role = String(u?.role || '').toLowerCase()
  const isAdmin = !!u && (u?.isAdmin === true || role === 'admin')

  if (!isAdmin) return navigateTo('/login')
})
