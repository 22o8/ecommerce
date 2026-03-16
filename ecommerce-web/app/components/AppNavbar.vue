<template>
  <header class="sticky top-0 z-50 px-2 pt-2 sm:px-3 sm:pt-3">
    <div class="nav-shell mx-auto max-w-7xl overflow-hidden rounded-[28px] border border-app">
      <div class="nav-shell__aurora nav-shell__aurora--one" />
      <div class="nav-shell__aurora nav-shell__aurora--two" />

      <div class="relative z-[1] mx-auto max-w-7xl px-3 py-3 sm:px-4">
        <div class="flex items-center gap-2 sm:gap-3">
          <NuxtLink to="/" class="brand-lockup min-w-0 shrink-0">
            <div class="brand-lockup__logo">
              <img :src="siteLogoSrc" alt="Site logo" class="h-full w-full object-cover" />
            </div>
            <div class="brand-lockup__text min-w-0">
              <div class="truncate text-sm font-extrabold tracking-[0.04em] text-[rgb(var(--text))] sm:text-base">
                ECOMMERCE
              </div>
              <div class="truncate text-xs text-[rgb(var(--muted))] rtl-text">
                {{ t('tagline') }}
              </div>
            </div>
          </NuxtLink>

          <div class="hidden xl:flex flex-1 items-center justify-center px-2">
            <div class="nav-search-wrap relative w-full max-w-[420px]">
              <div class="nav-search-bar">
                <Icon name="mdi:magnify" class="text-lg opacity-70" />
                <input
                  v-model="q"
                  class="w-full bg-transparent outline-none text-sm rtl-text"
                  :placeholder="t('productsPage.searchPlaceholder')"
                  @keydown.enter="goSearch"
                  @focus="openSearch = true"
                />
                <button type="button" class="nav-search-go" @click="goSearch">
                  <Icon name="mdi:arrow-right" class="keep-ltr" />
                </button>
              </div>

              <div
                v-if="openSearch && liveItems.length"
                class="absolute top-[calc(100%+12px)] left-0 right-0 z-50 overflow-hidden rounded-[24px] border border-app bg-surface shadow-2xl"
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
          </div>

          <div class="actions-strip ms-auto flex items-center gap-1.5 sm:gap-2 flex-shrink-0">
            <NuxtLink to="/brands" class="hidden md:block">
              <UiButton variant="secondary" class="nav-action-btn">
                <Icon name="mdi:storefront-outline" class="text-lg" />
                <span class="hidden lg:inline rtl-text">{{ t('home.brands') }}</span>
              </UiButton>
            </NuxtLink>

            <NuxtLink v-if="auth.isAuthed" to="/favorites" class="hidden sm:block">
              <UiButton variant="secondary" class="nav-action-btn relative">
                <Icon name="mdi:heart-outline" class="text-lg" />
                <span class="hidden lg:inline rtl-text">{{ t('nav.favorites') }}</span>
                <span v-if="fav.count" class="nav-badge">{{ fav.count }}</span>
              </UiButton>
            </NuxtLink>

            <NuxtLink to="/cart" class="block">
              <UiButton variant="secondary" class="nav-action-btn relative">
                <Icon name="mdi:cart-outline" class="text-lg" />
                <span class="hidden md:inline rtl-text">{{ t('nav.cart') }}</span>
                <span v-if="cart.count" class="nav-badge">{{ cart.count }}</span>
              </UiButton>
            </NuxtLink>

            <UiButton variant="ghost" class="nav-icon-btn" @click="toggleLocale" :title="t('nav.language')">
              <Icon name="mdi:translate" class="text-lg" />
              <span class="hidden sm:inline keep-ltr">{{ ui.locale.toUpperCase() }}</span>
            </UiButton>

            <UiButton variant="ghost" class="nav-icon-btn" @click="toggleTheme" :title="ui.theme === 'dark' ? t('ui.dark') : t('ui.light')">
              <Icon :name="ui.theme === 'dark' ? 'mdi:weather-night' : 'mdi:white-balance-sunny'" class="text-lg" />
            </UiButton>

            <NuxtLink v-if="isAdmin" to="/admin" class="hidden sm:block">
              <UiButton variant="secondary" class="nav-action-btn nav-action-btn--highlight">
                <Icon name="mdi:view-dashboard-outline" class="text-lg" />
                <span class="hidden lg:inline rtl-text">{{ t('home.dashboard') }}</span>
              </UiButton>
            </NuxtLink>

            <NuxtLink v-if="!auth.isAuthed" to="/login">
              <UiButton class="nav-action-btn nav-action-btn--solid">
                <Icon name="mdi:login-variant" class="text-lg" />
                <span class="hidden md:inline rtl-text">{{ t('nav.login') }}</span>
              </UiButton>
            </NuxtLink>
            <UiButton v-else variant="secondary" class="nav-action-btn" @click="logout">
              <Icon name="mdi:logout-variant" class="text-lg" />
              <span class="hidden md:inline rtl-text">{{ t('nav.logout') }}</span>
            </UiButton>

            <button class="mobile-menu-btn lg:hidden" @click="open = !open">
              <Icon name="mdi:menu" class="text-xl" />
            </button>
          </div>
        </div>
      </div>

      <div v-if="open" class="relative z-[1] border-t border-app bg-surface/90 px-3 py-4 lg:hidden sm:px-4">
        <div class="grid gap-3">
          <div class="nav-search-bar">
            <Icon name="mdi:magnify" class="text-lg opacity-70" />
            <input
              v-model="q"
              class="w-full bg-transparent outline-none text-sm rtl-text"
              :placeholder="t('productsPage.searchPlaceholder')"
              @keydown.enter="goSearch"
            />
            <button type="button" class="nav-search-go" @click="goSearch">
              <Icon name="mdi:arrow-right" class="keep-ltr" />
            </button>
          </div>

          <nav class="grid grid-cols-2 gap-2 text-sm">
            <NuxtLink to="/" class="drawer-link">
              <Icon name="mdi:home-outline" class="text-lg" />
              <span class="rtl-text">{{ t('nav.home') }}</span>
            </NuxtLink>
            <NuxtLink to="/brands" class="drawer-link">
              <Icon name="mdi:storefront-outline" class="text-lg" />
              <span class="rtl-text">{{ t('home.brands') }}</span>
            </NuxtLink>
            <NuxtLink v-if="auth.isAuthed" to="/favorites" class="drawer-link">
              <Icon name="mdi:heart-outline" class="text-lg" />
              <span class="rtl-text">{{ t('nav.favorites') }}</span>
              <span v-if="fav.count" class="keep-ltr text-xs text-muted">({{ fav.count }})</span>
            </NuxtLink>
            <NuxtLink to="/cart" class="drawer-link">
              <Icon name="mdi:cart-outline" class="text-lg" />
              <span class="rtl-text">{{ t('nav.cart') }}</span>
              <span v-if="cart.count" class="keep-ltr text-xs text-muted">({{ cart.count }})</span>
            </NuxtLink>
            <NuxtLink v-if="auth.isAuthed" to="/orders" class="drawer-link">
              <Icon name="mdi:receipt-text-outline" class="text-lg" />
              <span class="rtl-text">{{ t('myOrders') }}</span>
            </NuxtLink>
            <NuxtLink v-if="isAdmin" to="/admin" class="drawer-link">
              <Icon name="mdi:view-dashboard-outline" class="text-lg" />
              <span class="rtl-text">{{ t('adminPanel') }}</span>
            </NuxtLink>
            <NuxtLink to="/contact" class="drawer-link col-span-2">
              <Icon name="mdi:message-text-outline" class="text-lg" />
              <span class="rtl-text">{{ t('contact') }}</span>
            </NuxtLink>
          </nav>
        </div>
      </div>
    </div>
  </header>
</template>

<script setup lang="ts">
import UiButton from '~/components/ui/UiButton.vue'
import siteLogoSrc from '~/assets/img/site-logo.jpg'
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
let liveTimer: ReturnType<typeof setTimeout> | null = null

const isAdmin = computed(() => auth.isAdmin)

function toggleTheme() { ui.toggleTheme() }
function toggleLocale() { ui.setLocale(ui.locale === 'ar' ? 'en' : 'ar') }

function goSearch() {
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

function openLive(item: any) {
  openSearch.value = false
  q.value = ''
  navigateTo(`/product/${item.id}`)
}

const onDocClick = (e: Event) => {
  const target = e?.target as HTMLElement | null
  if (!target) return
  if (target.closest('header')) return
  openSearch.value = false
}

onMounted(() => {
  document.addEventListener('click', onDocClick)
})

onBeforeUnmount(() => {
  document.removeEventListener('click', onDocClick)
})

async function logout() {
  auth.logout()
  if (route.path.startsWith('/admin')) router.push('/')
}
</script>

<style scoped>
.nav-shell{
  position: relative;
  background:
    linear-gradient(180deg, rgba(var(--surface-rgb), .92), rgba(var(--surface-2-rgb), .78)),
    linear-gradient(135deg, rgba(var(--primary), .08), transparent 35%, rgba(var(--cta-glow-2), .08) 100%);
  box-shadow: 0 18px 44px rgba(3, 8, 20, .18);
  backdrop-filter: blur(18px);
}
.nav-shell__aurora{
  position: absolute;
  border-radius: 999px;
  pointer-events: none;
  filter: blur(20px);
}
.nav-shell__aurora--one{
  inset-inline-start: -40px;
  top: -40px;
  width: 180px;
  height: 120px;
  background: radial-gradient(circle, rgba(var(--primary), .16), transparent 72%);
}
.nav-shell__aurora--two{
  inset-inline-end: 12%;
  bottom: -46px;
  width: 240px;
  height: 100px;
  background: radial-gradient(circle, rgba(var(--cta-glow-2), .12), transparent 72%);
}
.brand-lockup{
  display:flex;
  align-items:center;
  gap:.8rem;
  min-width:0;
  padding:.4rem .5rem .4rem .35rem;
  border-radius:22px;
  background:linear-gradient(180deg, rgba(var(--surface-rgb), .82), rgba(var(--surface-2-rgb), .72));
  border:1px solid rgba(var(--border), .92);
  box-shadow: inset 0 1px 0 rgba(255,255,255,.08);
}
.brand-lockup__logo{
  width:48px;
  height:48px;
  overflow:hidden;
  border-radius:18px;
  border:1px solid rgba(255,255,255,.14);
  box-shadow: 0 12px 24px rgba(0,0,0,.18);
  flex:0 0 auto;
}
.nav-search-wrap{ min-width: 0; }
.nav-search-bar{
  display:flex;
  align-items:center;
  gap:.75rem;
  min-height:54px;
  padding:.55rem .7rem .55rem .9rem;
  border-radius:22px;
  border:1px solid rgba(var(--border), .92);
  background:linear-gradient(180deg, rgba(var(--surface-rgb), .88), rgba(var(--surface-2-rgb), .74));
  box-shadow: inset 0 1px 0 rgba(255,255,255,.06), 0 12px 28px rgba(0,0,0,.14);
}
.nav-search-go{
  display:grid;
  place-items:center;
  width:38px;
  height:38px;
  border-radius:999px;
  border:1px solid rgba(var(--border), .9);
  background: rgba(var(--primary), .12);
  color: rgb(var(--text));
}
.actions-strip{ min-width: 0; }
.nav-action-btn{
  min-height:48px;
  border-radius:18px;
}
.nav-action-btn--highlight{
  background: linear-gradient(135deg, rgba(var(--primary), .18), rgba(var(--cta-glow-2), .12));
}
.nav-action-btn--solid{
  box-shadow: 0 12px 24px rgba(var(--primary), .18);
}
.nav-icon-btn{
  min-height:48px;
  border-radius:18px;
}
.nav-badge{
  position:absolute;
  top:-8px;
  inset-inline-end:-8px;
  display:grid;
  place-items:center;
  min-width:20px;
  height:20px;
  padding-inline:.3rem;
  border-radius:999px;
  background:rgb(var(--primary));
  color:black;
  font-size:.72rem;
  font-weight:900;
}
.mobile-menu-btn{
  display:grid;
  place-items:center;
  width:46px;
  height:46px;
  border-radius:18px;
  border:1px solid rgba(var(--border), .92);
  background:linear-gradient(180deg, rgba(var(--surface-rgb), .88), rgba(var(--surface-2-rgb), .76));
}
.drawer-link{
  display:flex;
  align-items:center;
  gap:.6rem;
  min-height:52px;
  padding:.85rem 1rem;
  border-radius:18px;
  border:1px solid rgba(var(--border), .9);
  background:linear-gradient(180deg, rgba(var(--surface-rgb), .84), rgba(var(--surface-2-rgb), .74));
}
:global(html.theme-light) .nav-shell{
  background:
    linear-gradient(180deg, rgba(255,255,255,.94), rgba(255,247,252,.9)),
    linear-gradient(135deg, rgba(232,91,154,.08), transparent 35%, rgba(246,180,212,.08) 100%);
  box-shadow: 0 18px 40px rgba(232, 91, 154, .10), 0 8px 24px rgba(18,18,18,.05);
}
:global(html.theme-light) .brand-lockup,
:global(html.theme-light) .nav-search-bar,
:global(html.theme-light) .mobile-menu-btn,
:global(html.theme-light) .drawer-link{
  box-shadow: 0 12px 28px rgba(232, 91, 154, .08), inset 0 1px 0 rgba(255,255,255,.7);
}
@media (max-width: 1024px){
  .brand-lockup__logo{ width:44px; height:44px; }
  .nav-action-btn, .nav-icon-btn{ min-height:44px; }
}
@media (max-width: 640px){
  .nav-shell{ border-radius:24px; }
  .brand-lockup{ padding:.35rem .45rem .35rem .3rem; }
  .brand-lockup__logo{ width:40px; height:40px; border-radius:14px; }
}
</style>
