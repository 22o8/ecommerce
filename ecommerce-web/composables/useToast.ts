type ToastType = 'success' | 'error' | 'info'

export function useToast() {
  // Minimal toast composable so pages using it don't crash on SSR.
  // You can later replace this with a proper toast UI component.
  const toasts = useState<{ id: string; type: ToastType; message: string }[]>(
    'toasts',
    () => [],
  )

  const push = (type: ToastType, message: string) => {
    const id = `${Date.now()}-${Math.random().toString(16).slice(2)}`
    toasts.value.push({ id, type, message })

    // Client-only helper: show a basic alert for now (optional)
    if (process.client) {
      // eslint-disable-next-line no-console
      console.log(`[toast:${type}]`, message)
    }
  }

  return {
    toasts,
    success: (m: string) => push('success', m),
    error: (m: string) => push('error', m),
    info: (m: string) => push('info', m),
    clear: () => (toasts.value = []),
  }
}
