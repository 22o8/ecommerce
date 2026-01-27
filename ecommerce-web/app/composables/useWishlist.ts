type WishlistState = {
  ids: Set<string>
}

const KEY = 'wishlist:v1'

export function useWishlist() {
  const state = useState<WishlistState>('wishlist', () => ({ ids: new Set<string>() }))

  const ready = useState<boolean>('wishlist_ready', () => false)

  function load() {
    if (process.server || ready.value) return
    try {
      const raw = localStorage.getItem(KEY)
      if (!raw) {
        ready.value = true
        return
      }
      const arr = JSON.parse(raw) as string[]
      state.value.ids = new Set(arr)
    } catch {
      // ignore
    } finally {
      ready.value = true
    }
  }

  function persist() {
    if (process.server) return
    try {
      localStorage.setItem(KEY, JSON.stringify(Array.from(state.value.ids)))
    } catch {
      // ignore
    }
  }

  function has(id: string) {
    load()
    return state.value.ids.has(id)
  }

  function toggle(id: string) {
    load()
    if (state.value.ids.has(id)) state.value.ids.delete(id)
    else state.value.ids.add(id)
    persist()
  }

  function list() {
    load()
    return Array.from(state.value.ids)
  }

  return { has, toggle, list, load }
}
