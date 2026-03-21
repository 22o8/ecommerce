<template>
  <header class="sticky top-0 z-50">
    <div class="bg-app/80 backdrop-blur supports-[backdrop-filter]:bg-app/70 border-b border-app">
      <div class="mx-auto max-w-7xl px-3 sm:px-4 py-3 flex items-center gap-2 sm:gap-3">
        <NuxtLink to="/" class="flex items-center min-w-0">
          <div class="h-9 w-9 sm:h-10 sm:w-10 rounded-2xl bg-[rgb(var(--primary))] animate-float text-black dark:text-[rgb(var(--bg))] grid place-items-center font-black overflow-hidden">
            <img v-if="siteLogoSrc !== '#'" :src="siteLogoSrc" alt="Site logo" class="h-full w-full object-cover" />
            <Icon v-else name="mdi:shopping-outline" class="text-xl animate-floaty" />
          </div>
        </NuxtLink>

        <div class="flex-1" />

        <!-- Search (desktop) -->
        <div class="hidden lg:flex items-center gap-2 w-[420px]">
          <div class="relative w-full">
            <div class="flex items-center gap-2 w-full rounded-2xl border border-app bg-surface px-3 py-2">
            <Icon name="mdi:magnify" class="text-lg opacity-70" />
            <input
              v-model="q"
              class="w-full bg-transparent outline-none text-sm rtl-text"
              :placeholder="t('productsPage.searchPlaceholder')"
              @keydown.enter="goSearch"
              @focus="openSearch = true"
            />
            </div>

            <!-- Live Search dropdown -->
            <div
              v-if="openSearch && liveItems.length"
              class="absolute top-[calc(100%+10px)] left-0 right-0 z-50 rounded-2xl border border-app bg-surface shadow-2xl overflow-hidden"
            >
              <button
                v-for="item in liveItems"
                :key="item.id"
                type="button"
                class="w-full px-3 py-3 text-left hover:bg-surface-2 transition flex items-center gap-3"
                @click="openLive(item)"
              >
                <img
                  class="h-10 w-10 rounded-xl object-cover border border-app"
                  :src="item.imageUrl || '/hero-placeholder.svg'"
                  :alt="item.name"
                />
                <div class="min-w-0 flex-1">
                  <div class="font-extrabold text-sm truncate keep-ltr">{{ item.name }}</div>
                  <div class="text-xs text-muted truncate">{{ item.brand }}</div>
                </div>
                <div class="text-sm font-black keep-ltr">{{ formatIqd(item.finalPriceIqd ?? item.priceIqd) }}</div>
              </button>

              <NuxtLink
                v-if="q"
                :to="{ path: '/products', query: { q } }"
                class="block px-4 py-3 text-sm font-bold text-[rgb(var(--primary))] bg-surface-2 hover:opacity-90"
                @click="openSearch = false"
              >
                {{ t('productsPage.viewAll') || 'عرض كل النتائج' }}
              </NuxtLink>
            </div>
          </div>
          <UiButton variant="secondary" @click="goSearch">
            <Icon name="mdi:arrow-right" class="keep-ltr" />
          </UiButton>
        </div>

        <!-- Actions -->
	        <div class="flex items-center gap-0.5 sm:gap-2 flex-shrink-0">
	          <!-- Brands -->
	          <NuxtLink to="/brands" class="hidden sm:block">
	            <UiButton variant="secondary" class="px-2 sm:px-3">
              <Icon name="mdi:storefront-outline" class="text-lg" />
              <span class="hidden md:inline rtl-text">{{ t('home.brands') }}</span>
            </UiButton>
          </NuxtLink>
	          <!-- Favorites -->
          <NuxtLink v-if="auth.isAuthed" to="/favorites" class="hidden sm:block">
            <UiButton variant="secondary" class="relative px-2 sm:px-3 shrink-0">
              <Icon name="mdi:heart-outline" class="text-lg" />
              <span class="hidden md:inline rtl-text">{{ t('nav.favorites') }}</span>
              <span
                v-if="fav.count"
                class="absolute -top-2 -right-2 h-5 min-w-[20px] px-1 rounded-full bg-[rgb(var(--primary))] text-black text-xs font-black grid place-items-center"
              >
                {{ fav.count }}
              </span>
            </UiButton>
          </NuxtLink>

          <!-- Cart: يظهر على الهاتف أيضاً (أيقونة فقط) -->
	          <NuxtLink to="/cart" class="block">
	            <UiButton variant="secondary" class="relative px-2 sm:px-3 shrink-0">
              <Icon name="mdi:cart-outline" class="text-lg" />
	              <span class="hidden sm:inline rtl-text">{{ t('nav.cart') }}</span>
              <span
                v-if="cart.count"
                class="absolute -top-2 -right-2 h-5 min-w-[20px] px-1 rounded-full bg-[rgb(var(--primary))] text-black text-xs font-black grid place-items-center"
              >
                {{ cart.count }}
              </span>
            </UiButton>
          </NuxtLink>
	          <UiButton variant="ghost" class="px-2 sm:px-3 shrink-0" @click="toggleLocale" :title="t('nav.language')">
            <Icon name="mdi:translate" class="text-lg" />
            <span class="hidden sm:inline keep-ltr">{{ ui.locale.toUpperCase() }}</span>
          </UiButton>

	          <UiButton variant="ghost" class="px-2 sm:px-3 shrink-0" @click="toggleTheme" :title="ui.theme === 'dark' ? t('ui.dark') : t('ui.light')">
            <Icon :name="ui.theme === 'dark' ? 'mdi:weather-night' : 'mdi:white-balance-sunny'" class="text-lg" />
          </UiButton>

	          <!-- Admin: نخليها فقط داخل زر المينيو على الهاتف -->
	          <NuxtLink v-if="isAdmin" to="/admin" class="hidden sm:block">
	            <UiButton variant="secondary">
              <Icon name="mdi:view-dashboard-outline" class="text-lg" />
              <span class="rtl-text">{{ t('home.dashboard') }}</span>
            </UiButton>
          </NuxtLink>

          <NuxtLink v-if="!auth.isAuthed" to="/login">
            <UiButton>
              <Icon name="mdi:login-variant" class="text-lg" />
              <span class="hidden sm:inline rtl-text">{{ t('nav.login') }}</span>
            </UiButton>
          </NuxtLink>
          <UiButton v-else variant="secondary" @click="logout">
            <Icon name="mdi:logout-variant" class="text-lg" />
            <span class="hidden sm:inline rtl-text">{{ t('nav.logout') }}</span>
          </UiButton>

	        <button class="md:hidden shrink-0 min-w-[40px] rounded-2xl border border-app bg-surface px-2 py-2" @click="open = !open">
            <Icon name="mdi:menu" class="text-xl" />
          </button>
        </div>
      </div>

      <!-- Mobile search bar -->
      <div class="lg:hidden border-t border-app/80 bg-app/90 backdrop-blur-sm">
        <div class="mx-auto max-w-7xl px-3 sm:px-4 py-3">
          <form class="mobile-search-shell" @submit.prevent="goSearch">
            <button type="submit" class="mobile-search-shell__icon" :aria-label="t('productsPage.searchPlaceholder')">
              <Icon name="mdi:magnify" class="text-[22px]" />
            </button>
            <input
              v-model="q"
              class="mobile-search-shell__input rtl-text"
              :placeholder="t('productsPage.searchPlaceholder')"
              @focus="openSearch = true"
            />
          </form>

          <div
            v-if="openSearch && liveItems.length"
            class="mt-2 overflow-hidden rounded-[22px] border border-app bg-surface shadow-2xl"
          >
            <button
              v-for="item in liveItems"
              :key="item.id"
              type="button"
              class="flex w-full items-center gap-3 px-3 py-3 text-left transition hover:bg-surface-2"
              @click="openLive(item)"
            >
              <img
                class="h-11 w-11 rounded-2xl object-cover border border-app"
                :src="item.imageUrl || '/hero-placeholder.svg'"
                :alt="item.name"
              />
              <div class="min-w-0 flex-1">
                <div class="truncate text-sm font-extrabold keep-ltr">{{ item.name }}</div>
                <div class="truncate text-xs text-muted">{{ item.brand }}</div>
              </div>
              <div class="text-sm font-black keep-ltr">{{ formatIqd(item.finalPriceIqd ?? item.priceIqd) }}</div>
            </button>
          </div>
        </div>
      </div>

      <!-- Mobile drawer -->
      <div v-if="open" class="md:hidden border-t border-app bg-surface">
        <div class="mx-auto max-w-7xl px-3 sm:px-4 py-4 grid gap-3">
          <div class="flex items-center gap-2 w-full rounded-2xl border border-app bg-surface px-3 py-2">
            <Icon name="mdi:magnify" class="text-lg opacity-70" />
            <input
              v-model="q"
              class="w-full bg-transparent outline-none text-sm rtl-text"
              :placeholder="t('productsPage.searchPlaceholder')"
              @keydown.enter="goSearch"
            />
          </div>

          <nav class="grid grid-cols-2 gap-2 text-sm">
            <NuxtLink to="/" class="rounded-2xl border border-app bg-surface-2 px-4 py-3">
              <div class="flex items-center gap-2">
                <Icon name="mdi:home-outline" class="text-lg" />
                <span class="rtl-text">{{ t('nav.home') }}</span>
              </div>
            </NuxtLink>
            <NuxtLink to="/products" class="rounded-2xl border border-app bg-surface-2 px-4 py-3">
              <div class="flex items-center gap-2">
                <Icon name="mdi:view-grid-outline" class="text-lg" />
                <span class="rtl-text">{{ t('nav.products') }}</span>
              </div>
            </NuxtLink>

	          <NuxtLink to="/brands" class="rounded-2xl border border-app bg-surface-2 px-4 py-3">
	            <div class="flex items-center gap-2">
	              <Icon name="mdi:storefront-outline" class="text-lg" />
	              <span class="rtl-text">{{ t('home.brands') }}</span>
	            </div>
	          </NuxtLink>
            <NuxtLink v-if="auth.isAuthed" to="/favorites" class="rounded-2xl border border-app bg-surface-2 px-4 py-3">
              <div class="flex items-center gap-2">
                <Icon name="mdi:heart-outline" class="text-lg" />
                <span class="rtl-text">{{ t('nav.favorites') }}</span>
                <span v-if="fav.count" class="keep-ltr text-xs text-muted">({{ fav.count }})</span>
              </div>
            </NuxtLink>

            <NuxtLink to="/cart" class="rounded-2xl border border-app bg-surface-2 px-4 py-3">
              <div class="flex items-center gap-2">
                <Icon name="mdi:cart-outline" class="text-lg" />
                <span class="rtl-text">{{ t('nav.cart') }}</span>
                <span v-if="cart.count" class="keep-ltr text-xs text-muted">({{ cart.count }})</span>
              </div>
            </NuxtLink>
            <NuxtLink v-if="auth.isAuthed" to="/orders" class="rounded-2xl border border-app bg-surface-2 px-4 py-3">
              <div class="flex items-center gap-2">
                <Icon name="mdi:receipt-text-outline" class="text-lg" />
                <span class="rtl-text">{{ t('myOrders') }}</span>
              </div>
            </NuxtLink>
	            <NuxtLink v-if="isAdmin" to="/admin" class="rounded-2xl border border-app bg-surface-2 px-4 py-3">
	              <div class="flex items-center gap-2">
	                <Icon name="mdi:view-dashboard-outline" class="text-lg" />
	                <span class="rtl-text">{{ $t('adminPanel') }}</span>
	              </div>
	            </NuxtLink>
            <NuxtLink to="/contact" class="rounded-2xl border border-app bg-surface-2 px-4 py-3">
              <div class="flex items-center gap-2">
                <Icon name="mdi:message-text-outline" class="text-lg" />
                <span class="rtl-text">{{ t('contact') }}</span>
              </div>
            </NuxtLink>
          </nav>
        </div>
      </div>
    </div>
  </header>
</template>

<script setup lang="ts">
import siteLogoSrc from '~/assets/img/site-logo.jpg'
import UiButton from '~/components/ui/UiButton.vue'
import { useFavoritesStore } from '~/stores/favorites'
import { useProductsStore } from '~/stores/products'
import { formatIqd } from '~/composables/useMoney'
const ui = useUiStore()
const auth = useAuthStore()
const cart = useCartStore()
const fav = useFavoritesStore()
const products = useProductsStore()
const { t } = useI18n()

const route = useRoute()

const router = useRouter()
const open = ref(false)
const q = ref(String(route.query.q || ''))
const openSearch = ref(false)
const liveItems = ref<any[]>([])
let liveTimer: any = null

const isAdmin = computed(() => auth.isAdmin)

function toggleTheme(){ ui.toggleTheme() }
function toggleLocale(){ ui.setLocale(ui.locale === 'ar' ? 'en' : 'ar') }

function goSearch(){
  router.push({ path: '/products', query: q.value ? { q: q.value } : {} })
  open.value = false
  openSearch.value = false
}

watch(q, (val) => {
  const v = String(val || '').trim()
  if (liveTimer) clearTimeout(liveTimer)
  if (!v || v.length < 2) {
    liveItems.value = []
    return
  }

  liveTimer = setTimeout(async () => {
    try {
      liveItems.value = await products.liveSearch(v, 8)
    } catch {
      liveItems.value = []
    }
  }, 180)
})

function openLive(item: any){
  openSearch.value = false
  q.value = ''
  navigateTo(`/product/${item.id}`)
}

const onDocClick = (e: any) => {
  const target = e?.target as HTMLElement | null
  if (!target) return
  // إذا ضغط داخل الهيدر نخليها مفتوحة
  if (target.closest('header')) return
  openSearch.value = false
}

onMounted(() => {
  document.addEventListener('click', onDocClick)
})

onBeforeUnmount(() => {
  document.removeEventListener('click', onDocClick)
})

async function logout(){
  auth.logout()
  if (route.path.startsWith('/admin')) router.push('/')
}
</script>


<style scoped>
.mobile-search-shell{
  display:flex;
  align-items:center;
  gap:.55rem;
  min-height:58px;
  border-radius:24px;
  border:1px solid rgba(var(--border), .92);
  background:linear-gradient(180deg, rgba(var(--surface-rgb), .98), rgba(var(--surface-2-rgb), .9));
  padding:.5rem .8rem;
  box-shadow:0 16px 38px rgba(0,0,0,.12), inset 0 1px 0 rgba(255,255,255,.05);
}
.mobile-search-shell__icon{
  display:grid;
  place-items:center;
  width:42px;
  height:42px;
  border-radius:50%;
  color:rgb(var(--primary));
  background:rgba(var(--primary), .08);
}
.mobile-search-shell__input{
  width:100%;
  background:transparent;
  outline:none;
  border:none;
  color:rgb(var(--text));
  font-size:.98rem;
  font-weight:700;
}
.mobile-search-shell__input::placeholder{ color:rgb(var(--muted)); opacity:.95; }
</style>
