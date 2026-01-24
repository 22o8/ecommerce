export default defineNuxtRouteMiddleware((to) => {
  if (!to.path.startsWith('/admin')) return

  const auth = useAuthStore()
  // auth.user هو Ref (useCookie) لذلك لازم نقرأ .value
  const role = (auth.user as any)?.value?.role || (auth.userData as any)?.value?.role

  if (!auth.isAuthed) return navigateTo('/login')
  if (role !== 'Admin') return navigateTo('/') // ممنوع لغير الادمن
})
