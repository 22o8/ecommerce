// app/composables/useWishlist.ts
// Back-compat wrapper: القديم كان LocalStorage، الآن المفضلة على السيرفر مرتبطة بالحساب.
import { useFavoritesStore } from '~/stores/favorites'

export function useWishlist() {
  const fav = useFavoritesStore()

  function has(id: string) { return fav.has(id) }
  function isInWishlist(id: string) { return fav.has(id) }
  async function toggle(id: string) { await fav.toggle(id) }
  async function add(id: string) { if (!fav.has(id)) await fav.toggle(id) }
  async function remove(id: string) { if (fav.has(id)) await fav.toggle(id) }
  async function clear() { /* not implemented server-side */ await fav.refresh() }
  function list() { return Array.from(fav.ids) }
  async function load() { await fav.refresh() }

  return { has, isInWishlist, toggle, add, remove, clear, list, load }
}
