export default defineNuxtRouteMiddleware((to) => {
  if (!to.path.startsWith('/admin')) return

  const auth = useAuthStore()
  const role = auth.user?.role

  if (!auth.isAuthed) return navigateTo('/login')
  if (role !== 'Admin') return navigateTo('/') // ممنوع لغير الادمن
})
