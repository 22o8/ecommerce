// app/composables/useWishlist.ts
// Back-compat wrapper: القديم كان LocalStorage، الآن المفضلة على السيرفر مرتبطة بالحساب.
import { useFavoritesStore } from '~/stores/favorites'

export function useWishlist() {
  const fav = useFavoritesStore()

  function has(id: string) { return fav.isFavorite(id) }
  function isInWishlist(id: string) { return fav.isFavorite(id) }
  async function toggle(id: string) { await fav.toggle(id) }
  async function add(id: string) { if (!fav.isFavorite(id)) await fav.toggle(id) }
  async function remove(id: string) { if (fav.isFavorite(id)) await fav.toggle(id) }
  async function clear() { /* not implemented server-side */ await fav.load() }
  function list() { return fav.items.map((p: any) => p.id) }
  async function load() { await fav.load() }

  return { has, isInWishlist, toggle, add, remove, clear, list, load }
}
