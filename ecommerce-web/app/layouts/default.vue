<!-- app/layouts/default.vue -->
<template>
  <div class="min-h-screen bg-ambient">
    <header class="sticky top-0 z-50">
      <div class="mx-auto max-w-6xl px-4 pt-4">
        <div class="card px-4 py-3 flex items-center justify-between">
          <NuxtLink to="/" class="flex items-center gap-3">
            <div
              class="h-10 w-10 rounded-2xl flex items-center justify-center font-black text-white shadow-sm"
              :style="{ background: `rgb(var(--primary))` }"
            >
              E
            </div>

            <div class="leading-tight">
              <div class="font-extrabold tracking-wide">ECOMMERCE</div>
              <div class="text-xs muted -mt-0.5">{{ t('tagline') }}</div>
            </div>
          </NuxtLink>

          <!-- Desktop -->
          <nav class="hidden md:flex items-center gap-1 text-sm">
            <NuxtLink
              to="/"
              class="px-3 py-2 rounded-xl hover:bg-[rgba(var(--text),0.06)] transition"
              :class="isActive('/') ? 'nav-active' : ''"
            >
              {{ t('home') }}
            </NuxtLink>

            <NuxtLink
              to="/products"
              class="px-3 py-2 rounded-xl hover:bg-[rgba(var(--text),0.06)] transition"
              :class="isActive('/products') ? 'nav-active' : ''"
            >
              {{ t('products') }}
            </NuxtLink>

            <!-- تم حذف Services من الـ Navbar حسب الطلب (موجودة كزر داخل الصفحة الرئيسية) -->

            <NuxtLink
              v-if="auth.isAuthed"
              to="/orders"
              class="px-3 py-2 rounded-xl hover:bg-[rgba(var(--text),0.06)] transition"
              :class="isActive('/orders') ? 'nav-active' : ''"
            >
              {{ t('myOrders') }}
            </NuxtLink>

            <NuxtLink
              to="/contact"
              class="nav-link keep-ltr"
              :class="isActive('/contact') ? 'nav-active' : ''"
            >
              {{ t('contact') }}
            </NuxtLink>

            <!-- Services/Requests removed from navbar (not used in UI now) -->

            <NuxtLink
              v-if="isAdmin"
              to="/admin"
              class="px-3 py-2 rounded-xl hover:bg-[rgba(var(--text),0.06)] transition"
              :class="isActive('/admin') ? 'nav-active' : ''"
            >
              {{ t('admin') }}
            </NuxtLink>

            <div class="mx-2 h-6 w-px" style="background: rgba(var(--text),.12)" />

            <button
              type="button"
              class="px-3 py-2 rounded-xl border hover:opacity-90 transition"
              :style="{ borderColor: 'rgb(var(--border))', background: 'rgba(var(--text),.04)' }"
              @click="toggleLocale"
            >
              {{ ui.locale.toUpperCase() }}
            </button>

            <button
              type="button"
              class="px-3 py-2 rounded-xl border hover:opacity-90 transition"
              :style="{ borderColor: 'rgb(var(--border))', background: 'rgba(var(--text),.04)' }"
              @click="toggleTheme"
            >
              {{ ui.theme === 'dark' ? t('theme.dark') : t('theme.light') }}
            </button>

            <NuxtLink v-if="!auth.isAuthed" to="/login">
              <AppButton variant="primary">{{ t('login') }}</AppButton>
            </NuxtLink>
            <button v-else type="button" @click="logout" class="ml-2">
              <AppButton variant="ghost">{{ t('logout') }}</AppButton>
            </button>
          </nav>

          <!-- Mobile -->
          <div class="md:hidden flex items-center gap-2">
            <button
              type="button"
              class="px-3 py-2 rounded-xl border"
              :style="{ borderColor: 'rgb(var(--border))', background: 'rgba(var(--text),.04)' }"
              @click="toggleTheme"
            >
              {{ ui.theme === 'dark' ? '🌙' : '☀️' }}
            </button>

            <button
              type="button"
              class="px-3 py-2 rounded-xl border"
              :style="{ borderColor: 'rgb(var(--border))', background: 'rgba(var(--text),.04)' }"
              @click="toggleLocale"
            >
              {{ ui.locale.toUpperCase() }}
            </button>

            <button
              type="button"
              class="px-3 py-2 rounded-xl border"
              :style="{ borderColor: 'rgb(var(--border))', background: 'rgba(var(--text),.04)' }"
              @click="open = !open"
            >
              ☰
            </button>
          </div>
        </div>

        <!-- Mobile menu -->
        <div v-if="open" class="mt-2 card p-3 md:hidden">
          <NuxtLink to="/" class="block px-3 py-2 rounded-xl hover:bg-[rgba(var(--text),0.06)]" @click="open = false">
            {{ t('home') }}
          </NuxtLink>

          <NuxtLink to="/products" class="block px-3 py-2 rounded-xl hover:bg-[rgba(var(--text),0.06)]" @click="open = false">
            {{ t('products') }}
          </NuxtLink>



          <NuxtLink
            v-if="auth.isAuthed"
            to="/orders"
            class="block px-3 py-2 rounded-xl hover:bg-[rgba(var(--text),0.06)]"
            @click="open = false"
          >
            {{ t('myOrders') }}
          </NuxtLink>

          <NuxtLink
            to="/contact"
            class="block px-3 py-2 rounded-xl hover:bg-[rgba(var(--text),0.06)]"
            @click="open=false"
          >
            {{ t('contact') }}
          </NuxtLink>


          <NuxtLink
            v-if="isAdmin"
            to="/admin"
            class="block px-3 py-2 rounded-xl hover:bg-[rgba(var(--text),0.06)]"
            @click="open = false"
          >
            {{ t('admin') }}
          </NuxtLink>

          <div class="mt-3 flex gap-2">
            <NuxtLink v-if="!auth.isAuthed" to="/login" class="flex-1" @click="open = false">
              <AppButton variant="primary" class="w-full">{{ t('login') }}</AppButton>
            </NuxtLink>
            <button v-else type="button" class="flex-1" @click="logout">
              <AppButton variant="ghost" class="w-full">{{ t('logout') }}</AppButton>
            </button>
          </div>
        </div>
      </div>
    </header>

    <main class="mx-auto max-w-6xl px-4 py-10">
      <slot />
    </main>

    <footer class="py-10">
      <div class="mx-auto max-w-6xl px-4">
        <div class="card p-6 flex flex-col gap-4 md:flex-row md:items-center md:justify-between">
          <div class="text-sm muted">
            © {{ new Date().getFullYear() }} — {{ cfg.public.siteName || 'Ecommerce' }}
          </div>

          <div class="flex flex-wrap items-center gap-3 text-sm keep-ltr">
            <a v-if="supportPhone" class="footer-chip" :href="`tel:${supportPhone}`" rel="noopener">
              📞 <span class="font-extrabold">{{ supportPhone }}</span>
            </a>
            <a v-if="supportEmail" class="footer-chip" :href="`mailto:${supportEmail}`" rel="noopener">
              ✉️ <span class="font-extrabold">{{ supportEmail }}</span>
            </a>
            <a v-if="instagramUrl" class="footer-chip" :href="instagramUrl" target="_blank" rel="noopener" aria-label="Instagram">
              <span class="ig-dot" /> <span class="font-extrabold">Instagram</span>
            </a>
          </div>
        </div>
      </div>
    </footer>

    <!-- Floating quick actions (WhatsApp + back to top) -->
    <FloatingActions />
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue'
import { useRoute } from 'vue-router'
import { useRuntimeConfig } from '#imports'
import { useUiStore } from '~/stores/ui'
import { useAuthStore } from '~/stores/auth'
import { useI18n } from '~/composables/useI18n'
import FloatingActions from '~/components/FloatingActions.vue'

const ui = useUiStore()
const auth = useAuthStore()
const { t, setLocale } = useI18n()
const cfg = useRuntimeConfig()

const supportEmail = computed(() => String(cfg.public.supportEmail || ''))
const supportPhone = computed(() => String(cfg.public.supportPhone || ''))
const instagramUrl = computed(() => String(cfg.public.instagramUrl || ''))

const route = useRoute()
const open = ref(false)

const isAdmin = computed(() => {
  const u: any = auth.user
  return u?.role === 'Admin'
})

function isActive(path: string) {
  return route.path === path || (path !== '/' && route.path.startsWith(path))
}

function toggleTheme() {
  ui.toggleTheme()
}

function toggleLocale() {
  const next = ui.locale === 'en' ? 'ar' : 'en'
  setLocale(next)
  ui.setLocale(next)
}

async function logout() {
  await auth.logout()
  open.value = false
}

watch(
  () => route.fullPath,
  () => {
    open.value = false
  }
)

onMounted(() => {
  ui.initClient()
})
</script>
