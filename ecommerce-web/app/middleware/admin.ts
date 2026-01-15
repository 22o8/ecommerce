// app/middleware/Admin.ts
export default defineNuxtRouteMiddleware(() => {
  const auth = useAuthStore();

  // إذا user عندك object مباشر (مو ref)
  const user = auth.user;

  // عدّل شرط الأدمن حسب موديلك: isAdmin / role / permissions
  const isAdmin =
    !!user && (
      (user as any).isAdmin === true ||
      (user as any).role === 'Admin' ||
      (user as any).role === 'admin'
    );

  if (!isAdmin) {
    return navigateTo('/login');
  }
});
