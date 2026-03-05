type ToastType = 'success' | 'error' | 'info'

export type ToastItem = {
  id: string
  type: ToastType
  title?: string
  message: string
  ttlMs: number
}

function uid() {
  return Math.random().toString(36).slice(2, 10) + Date.now().toString(36)
}

export function useToast() {
  const toasts = useState<ToastItem[]>('toasts', () => [])

  function push(partial: Omit<ToastItem, 'id'>) {
    const id = uid()
    const item: ToastItem = { id, ...partial }
    toasts.value = [...toasts.value, item]
    if (process.client) {
      window.setTimeout(() => remove(id), item.ttlMs)
    }
    return id
  }

  function remove(id: string) {
    toasts.value = toasts.value.filter((t) => t.id !== id)
  }

  // Helpers
  function success(message: string, title?: string) {
    return push({ type: 'success', message, title, ttlMs: 2400 })
  }
  function error(message: string, title?: string) {
    return push({ type: 'error', message, title, ttlMs: 3200 })
  }
  function info(message: string, title?: string) {
    return push({ type: 'info', message, title, ttlMs: 2400 })
  }

  return { toasts, push, remove, success, error, info }
}
