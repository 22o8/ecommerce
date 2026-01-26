<template>
  <header class="sticky top-0 z-50">
    <div class="bg-app/80 backdrop-blur supports-[backdrop-filter]:bg-app/70 border-b border-app">
      <div class="mx-auto max-w-7xl px-4 py-3 flex items-center gap-3">
        <NuxtLink to="/" class="flex items-center gap-3">
          <div class="h-10 w-10 rounded-2xl bg-[rgb(var(--primary))] animate-float text-black dark:text-[rgb(var(--bg))] grid place-items-center font-black">
            <Icon name="mdi:shopping-outline" class="text-xl animate-floaty" />
          </div>
          <div class="leading-tight">
            <div class="font-extrabold tracking-wide">ECOMMERCE</div>
            <div class="text-xs text-muted -mt-0.5 rtl-text">{{ t('tagline') }}</div>
          </div>
        </NuxtLink>

        <div class="flex-1" />

        <!-- Search (desktop) -->
        <div class="hidden lg:flex items-center gap-2 w-[420px]">
          <div class="flex items-center gap-2 w-full rounded-2xl border border-app bg-surface px-3 py-2">
            <Icon name="mdi:magnify" class="text-lg opacity-70" />
            <input
              v-model="q"
              class="w-full bg-transparent outline-none text-sm rtl-text"
              :placeholder="t('productsPage.searchPlaceholder')"
              @keydown.enter="goSearch"
            />
          </div>
          <UiButton variant="secondary" @click="goSearch">
            <Icon name="mdi:arrow-right" class="keep-ltr" />
          </UiButton>
        </div>

        <!-- Actions -->
        <div class="flex items-center gap-2">
          <NuxtLink to="/cart" class="hidden sm:block">
            <UiButton variant="ghost" class="relative">
              <Icon name="mdi:cart-outline" class="text-lg" />
              <span class="rtl-text">{{ t('nav.cart') }}</span>
              <span
                v-if="cart.count"
                class="absolute -top-2 -right-2 h-5 min-w-[20px] px-1 rounded-full bg-[rgb(var(--primary))] text-black text-xs font-black grid place-items-center"
              >
                {{ cart.count }}
              </span>
            </UiButton>
          </NuxtLink>
          <UiButton variant="ghost" class="px-3" @click="toggleLocale" :title="t('nav.language')">
            <Icon name="mdi:translate" class="text-lg" />
            <span class="hidden sm:inline keep-ltr">{{ ui.locale.toUpperCase() }}</span>
          </UiButton>

          <UiButton variant="ghost" class="px-3" @click="toggleTheme" :title="ui.theme === 'dark' ? t('theme.dark') : t('theme.light')">
            <Icon :name="ui.theme === 'dark' ? 'mdi:weather-night' : 'mdi:white-balance-sunny'" class="text-lg" />
          </UiButton>

          <NuxtLink v-if="isAdmin" to="/admin" class="hidden sm:block">
            <UiButton variant="secondary">
              <Icon name="mdi:view-dashboard-outline" class="text-lg" />
              <span class="rtl-text">{{ t('dashboard') }}</span>
            </UiButton>
          </NuxtLink>

          <NuxtLink v-if="!auth.isAuthed" to="/login">
            <UiButton>
              <Icon name="mdi:login-variant" class="text-lg" />
              <span class="rtl-text">{{ t('nav.login') }}</span>
            </UiButton>
          </NuxtLink>
          <UiButton v-else variant="secondary" @click="logout">
            <Icon name="mdi:logout-variant" class="text-lg" />
            <span class="rtl-text">{{ t('nav.logout') }}</span>
          </UiButton>

          <button class="lg:hidden rounded-2xl border border-app bg-surface px-3 py-2" @click="open = !open">
            <Icon name="mdi:menu" class="text-xl" />
          </button>
        </div>
      </div>

      <!-- Mobile drawer -->
      <div v-if="open" class="lg:hidden border-t border-app bg-surface">
        <div class="mx-auto max-w-7xl px-4 py-4 grid gap-3">
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
                <Icon name="mdi:shopping-search-outline" class="text-lg" />
                <span class="rtl-text">{{ t('nav.products') }}</span>
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
import UiButton from '~/components/ui/UiButton.vue'
const ui = useUiStore()
const auth = useAuthStore()
const cart = useCartStore()
const { t } = useI18n()

const route = useRoute()
const router = useRouter()
const open = ref(false)
const q = ref(String(route.query.q || ''))

const isAdmin = computed(() => auth.user?.role?.toLowerCase?.() === 'admin')

function toggleTheme(){ ui.toggleTheme() }
function toggleLocale(){ ui.setLocale(ui.locale === 'ar' ? 'en' : 'ar') }

function goSearch(){
  router.push({ path: '/products', query: q.value ? { q: q.value } : {} })
  open.value = false
}

async function logout(){
  auth.logout()
  if (route.path.startsWith('/admin')) router.push('/')
}
</script>
