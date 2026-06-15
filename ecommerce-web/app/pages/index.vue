<script setup lang="ts">
import { computed, ref, onMounted, onBeforeUnmount, watch } from 'vue'
import { useAsyncData } from '#app'
import { useBrandsStore } from '~/stores/brands'
import { useProductsStore } from '~/stores/products'
import siteLogoSrc from '~/assets/img/site-logo.jpg'

const { t, locale } = useI18n()
const { categories, problemCategories, fetchCategories, fetchProblemChildren } = useCategories()

const brandsStore = useBrandsStore()
const productsStore = useProductsStore()
const appearance = useAppearanceStore()
if (!appearance.loaded) await appearance.refresh()

await useAsyncData(
  'home-prefetch',
  async () => {
    await Promise.allSettled([
      brandsStore.fetchPublic(1000),
      productsStore.fetchFeatured(8),
      productsStore.fetchDiscounts(8),
      productsStore.fetchTopRated(8),
      productsStore.fetch({ page: 1, pageSize: 8, sort: 'newest' }),
      fetchCategories(false, 'regular'),
      fetchCategories(false, 'problem'),
    ])
    return true
  },
  {
    server: false,
    default: () => true,
  }
)

const featured = computed(() => productsStore.featured)
const safeFeatured = computed(() => (featured.value ?? []).filter((p) => !!p && !!p.id))
const fallbackLatest = computed(() => productsStore.items?.slice(0, 8) ?? [])
const homeFeatured = computed(() => {
  if (safeFeatured.value.length) return safeFeatured.value
  return (fallbackLatest.value ?? []).filter((p) => !!p && !!p.id && !!p.isPublished).slice(0, 8)
})
const tab = ref<'featured' | 'discounts' | 'topRated'>('featured')
const displayedFeatured = computed(() => {
  if (tab.value === 'discounts') return (productsStore.discountItems ?? []).slice(0, 8)
  if (tab.value === 'topRated') return (productsStore.topRatedItems ?? []).slice(0, 8)
  return homeFeatured.value
})

const topRatedLoading = ref(false)
async function ensureTopRatedLoaded() {
  if (topRatedLoading.value) return
  if ((productsStore.topRatedItems ?? []).length > 0) return
  topRatedLoading.value = true
  try {
    await productsStore.fetchTopRated(8)
  } finally {
    topRatedLoading.value = false
  }
}
watch(tab, async (value) => {
  if (value === 'topRated') await ensureTopRatedLoaded()
})

const brands = computed(() => brandsStore.publicItems)
const topBrands = computed(() => {
  const seen = new Set<string>()
  const uniq = [] as typeof brands.value
  for (const b of (brands.value ?? [])) {
    if (!b) continue
    const key = (b.name ?? '').trim().toLowerCase()
    if (!key || seen.has(key)) continue
    seen.add(key)
    uniq.push(b)
  }
  return uniq
})

const brandOrbitIndex = ref(0)
let brandOrbitTimer: ReturnType<typeof setInterval> | null = null
const orbitBrands = computed(() => (topBrands.value ?? []).filter((b: any) => !!b && !!(b.logoUrl || b.imageUrl || b.logo || b.image)))
const orbitBrandTop = computed(() => {
  const list = orbitBrands.value
  if (!list.length) return null
  return list[brandOrbitIndex.value % list.length]
})
const orbitBrandBottom = computed(() => {
  const list = orbitBrands.value
  if (!list.length) return null
  return list[(brandOrbitIndex.value + 1) % list.length]
})
function brandOrbitSrc(brand: any) {
  return buildAssetUrl(brand?.logoUrl || brand?.imageUrl || brand?.logo || brand?.image || '')
}
const categoryCards = computed(() => {
  const accents = [
    'from-cyan-500/25 to-indigo-500/10',
    'from-fuchsia-500/20 to-rose-500/10',
    'from-amber-500/25 to-orange-500/10',
    'from-sky-500/20 to-violet-500/10',
    'from-blue-500/20 to-cyan-500/10',
    'from-pink-500/20 to-violet-500/10',
    'from-emerald-500/20 to-cyan-500/10',
  ]
  return (categories.value || []).map((c: any, idx: number) => ({
    key: String(c.key || '').toLowerCase(),
    title: locale.value === 'en' ? (c.nameEn || c.nameAr || c.key) : (c.nameAr || c.nameEn || c.key),
    subtitle: locale.value === 'en' ? (c.descriptionEn || c.descriptionAr || t('home.tapToExplore')) : (c.descriptionAr || c.descriptionEn || t('home.tapToExplore')),
    imageUrl: c.imageUrl || '',
    to: `/categories/${encodeURIComponent(String(c.key || '').toLowerCase())}`,
    accent: accents[idx % accents.length],
    index: idx + 1,
    id: String(c.id || ''),
    hasDetailSections: Boolean(c.hasDetailSections ?? false),
    childCount: Number(c.childCount ?? 0),
  }))
})

const activeCategoryKey = ref('')
const categoriesMenuOpen = ref(false)
const categoryChildrenMap = ref<Record<string, any[]>>({})
const activeCategory = computed(() => {
  const list = categoryCards.value || []
  if (!list.length) return null
  return list.find((item: any) => item.key === activeCategoryKey.value) || list[0]
})
const activeCategoryChildren = computed(() => {
  const key = activeCategoryKey.value
  return key ? (categoryChildrenMap.value[key] || []) : []
})
async function ensureCategoryChildren(item: any) {
  if (!(item?.hasDetailSections || item?.childCount > 0) || !item?.id) return []
  if (categoryChildrenMap.value[item.key]) return categoryChildrenMap.value[item.key]
  const children = await fetchProblemChildren(item.id, 'regular')
  categoryChildrenMap.value = { ...categoryChildrenMap.value, [item.key]: children }
  return children
}
async function openCategoriesMenu(key?: string) {
  const list = categoryCards.value || []
  const item = list.find((x: any) => x.key === (key || activeCategoryKey.value)) || list[0]
  if (!item) return
  activeCategoryKey.value = item.key
  if (item.hasDetailSections || item.childCount > 0) {
    await ensureCategoryChildren(item)
    categoriesMenuOpen.value = true
  } else {
    categoriesMenuOpen.value = false
  }
}
function closeCategoriesMenu() {
  categoriesMenuOpen.value = false
}
watch(categoryCards, (list) => {
  if (!list.length) {
    activeCategoryKey.value = ''
    return
  }
  if (!list.some((item: any) => item.key === activeCategoryKey.value)) {
    activeCategoryKey.value = list[0].key
  }
}, { immediate: true })

const problemCards = computed(() => {
  const accents = [
    'from-rose-500/25 to-fuchsia-500/10',
    'from-amber-500/25 to-orange-500/10',
    'from-sky-500/20 to-indigo-500/10',
    'from-emerald-500/20 to-cyan-500/10',
  ]
  return (problemCategories.value || []).map((c: any, idx: number) => ({
    key: String(c.key || '').toLowerCase(),
    title: locale.value === 'en' ? (c.nameEn || c.nameAr || c.key) : (c.nameAr || c.nameEn || c.key),
    subtitle: locale.value === 'en' ? (c.descriptionEn || c.descriptionAr || t('home.tapToExplore')) : (c.descriptionAr || c.descriptionEn || t('home.tapToExplore')),
    imageUrl: c.imageUrl || '',
    to: `/problems/${encodeURIComponent(String(c.key || '').toLowerCase())}`,
    accent: accents[idx % accents.length],
  }))
})

const { buildAssetUrl } = useApi()
const heroLogo = computed(() => appearance.data.siteLogoUrl ? buildAssetUrl(appearance.data.siteLogoUrl) : siteLogoSrc)

const homeAds = useState<any[]>('home-inline-ads', () => [])
const homeAdIndex = ref(0)
let homeAdTimer: ReturnType<typeof setInterval> | null = null

function normalizeAdValue(value: any) {
  return String(value ?? '').trim().toLowerCase()
}
function normalizeAdType(ad: any) {
  const raw = ad?.type ?? ad?.Type ?? ''
  if (typeof raw === 'number') return ['slider', 'banner', 'popup', 'product'][raw] || ''
  return normalizeAdValue(raw)
}
function normalizeAdPlacement(ad: any) {
  return normalizeAdValue(ad?.placement ?? ad?.Placement ?? '')
}
function adMediaList(ad: any) {
  const raw = ad?.imageUrls ?? ad?.ImageUrls
  const arr = Array.isArray(raw) ? raw : (Array.isArray(raw?.$values) ? raw.$values : [])
  const merged = arr.length ? arr : ((ad?.imageUrl || ad?.ImageUrl) ? [ad?.imageUrl || ad?.ImageUrl] : [])
  return merged.map((x: any) => String(x || '').trim()).filter(Boolean)
}
function firstAdMedia(ad: any) {
  return adMediaList(ad)[0] || ''
}
function adHasContent(ad: any) {
  return Boolean(firstAdMedia(ad) || String(ad?.title || ad?.Title || ad?.subtitle || ad?.Subtitle || '').trim())
}
function isAdEnabled(ad: any) {
  return (ad?.isEnabled ?? ad?.IsEnabled) !== false
}
const heroInlineAd = computed(() => {
  return (homeAds.value || [])
    .filter((ad: any) => isAdEnabled(ad) && ['banner', 'slider'].includes(normalizeAdType(ad)) && normalizeAdPlacement(ad) === 'home_hero_inline' && adHasContent(ad))
    .sort((a: any, b: any) => Number(a?.sortOrder ?? a?.SortOrder ?? 0) - Number(b?.sortOrder ?? b?.SortOrder ?? 0))[0] || null
})
const heroInlineMediaItems = computed(() => heroInlineAd.value ? adMediaList(heroInlineAd.value) : [])
const currentHeroInlineMedia = computed(() => heroInlineMediaItems.value[homeAdIndex.value] || heroInlineMediaItems.value[0] || '')
function isHeroVideo(url?: string) {
  return /\.(mp4|webm|ogg|mov)(\?|$)/i.test(String(url || ''))
}
function heroMediaAttrs(url?: string) {
  const src = buildAssetUrl(url || '')
  if (isHeroVideo(src)) return { src, autoplay: true, muted: true, loop: true, playsinline: true, preload: 'auto', controls: false }
  return { src, alt: heroInlineAd.value?.title || heroInlineAd.value?.Title || 'advertisement', loading: 'eager', decoding: 'async', fetchpriority: 'high' }
}
function safeAdLink(link?: string) {
  const value = String(link || '').trim()
  return value || '#'
}
async function loadHomeAds() {
  try {
    const res: any = await $fetch('/api/bff/ads/active', { query: { _t: Date.now() } })
    homeAds.value = Array.isArray(res) ? res : (Array.isArray(res?.items) ? res.items : (Array.isArray(res?.data) ? res.data : []))
  } catch {
    homeAds.value = []
  }
}
function startHomeAdTimer() {
  if (homeAdTimer) clearInterval(homeAdTimer)
  if (heroInlineMediaItems.value.length <= 1) return
  homeAdTimer = setInterval(() => {
    homeAdIndex.value = (homeAdIndex.value + 1) % heroInlineMediaItems.value.length
  }, 5200)
}
watch(heroInlineMediaItems, () => {
  homeAdIndex.value = 0
  startHomeAdTimer()
})
const categoryRail = ref<HTMLElement | null>(null)
const problemCategoryRail = ref<HTMLElement | null>(null)
const dragState = { active: false, moved: false, startX: 0, startScroll: 0, target: null as HTMLElement | null }

function onRailPointerDown(event: PointerEvent, rail: HTMLElement | null = categoryRail.value) {
  if (!rail || window.innerWidth < 768) return
  dragState.active = true
  dragState.moved = false
  dragState.target = rail
  dragState.startX = event.clientX
  dragState.startScroll = rail.scrollLeft
}

function onRailPointerMove(event: PointerEvent) {
  if (!dragState.active || !dragState.target) return
  const delta = event.clientX - dragState.startX
  if (Math.abs(delta) > 6) {
    dragState.moved = true
    dragState.target.classList.add('is-dragging')
  }
  if (!dragState.moved) return
  dragState.target.scrollLeft = dragState.startScroll - delta
}

function onRailLinkClick(event: MouseEvent) {
  if (!dragState.moved) return
  event.preventDefault()
  event.stopPropagation()
}

function endRailDrag() {
  dragState.active = false
  dragState.target?.classList.remove('is-dragging')
  setTimeout(() => {
    dragState.moved = false
    dragState.target = null
  }, 0)
}

function onRailWheel(event: WheelEvent) {
  const rail = event.currentTarget as HTMLElement | null
  if (!rail || window.innerWidth < 768) return
  if (Math.abs(event.deltaY) <= Math.abs(event.deltaX)) return
  event.preventDefault()
  rail.scrollLeft += event.deltaY
}

function scrollRail(direction: 'prev' | 'next', rail: HTMLElement | null) {
  if (!rail) return
  const amount = Math.max(rail.clientWidth * 0.72, 240)
  rail.scrollBy({ left: direction === 'next' ? amount : -amount, behavior: 'smooth' })
}

onMounted(() => {
  loadHomeAds()
  categoryRail.value?.addEventListener('wheel', onRailWheel, { passive: false })
  problemCategoryRail.value?.addEventListener('wheel', onRailWheel, { passive: false })
  ensureTopRatedLoaded()
  brandOrbitTimer = setInterval(() => {
    if (orbitBrands.value.length > 1) {
      brandOrbitIndex.value = (brandOrbitIndex.value + 1) % orbitBrands.value.length
    }
  }, 2600)
})

onBeforeUnmount(() => {
  categoryRail.value?.removeEventListener('wheel', onRailWheel as any)
  problemCategoryRail.value?.removeEventListener('wheel', onRailWheel as any)
  if (brandOrbitTimer) clearInterval(brandOrbitTimer)
  if (homeAdTimer) clearInterval(homeAdTimer)
})


useAdvancedSeo({
  title: 'DR SEOUL BEAUTY - متجر العناية الكورية الأصلي',
  description: 'تسوقي أفضل منتجات العناية بالبشرة والتجميل الكوري من DR SEOUL BEAUTY: تونرات، سيرومات، واقيات شمس، ماسكات وبراندات كورية مختارة بعناية.',
  keywords: ['DR SEOUL BEAUTY', 'korean skincare iraq', 'منتجات كورية للعناية بالبشرة', 'كوزمتك كوري', 'Anua', 'COSRX', 'Tenzero'],
  canonical: absoluteUrl('/'),
  image: heroLogo.value || '/og-image.png',
  schema: buildBreadcrumbSchema([{ name: 'Home', item: absoluteUrl('/') }]),
})

</script>

<template>
  <div class="min-h-screen home-page-shell">

    <section class="mx-auto max-w-[92rem] px-4 pt-8 pb-8 lg:px-6 lg:pt-12">
      <div class="home-luxury-hero">
        <div class="home-luxury-hero__glow home-luxury-hero__glow--one" />
        <div class="home-luxury-hero__glow home-luxury-hero__glow--two" />

        <div class="home-luxury-hero__content rtl-text">
          <div v-if="heroInlineAd" class="home-luxury-hero__inline-ad">
            <NuxtLink :to="safeAdLink(heroInlineAd.linkUrl || heroInlineAd.LinkUrl)" class="home-luxury-hero__inline-ad-link">
              <component
                v-if="currentHeroInlineMedia"
                :is="isHeroVideo(currentHeroInlineMedia) ? 'video' : 'img'"
                :key="currentHeroInlineMedia"
                v-bind="heroMediaAttrs(currentHeroInlineMedia)"
                class="home-luxury-hero__inline-ad-media"
              />
              <div v-if="heroInlineAd.title || heroInlineAd.Title || heroInlineAd.subtitle || heroInlineAd.Subtitle" class="home-luxury-hero__inline-ad-text">
                <span>إعلان</span>
                <b>{{ heroInlineAd.title || heroInlineAd.Title }}</b>
                <small v-if="heroInlineAd.subtitle || heroInlineAd.Subtitle">{{ heroInlineAd.subtitle || heroInlineAd.Subtitle }}</small>
              </div>
            </NuxtLink>
          </div>

          <div class="home-luxury-hero__actions home-luxury-hero__actions--moved">
            <NuxtLink to="/products" class="home-luxury-hero__primary">
              {{ t('homeHero.shopNow') }}
              <Icon name="mdi:arrow-left" class="text-lg" />
            </NuxtLink>
            <NuxtLink to="/referral" class="home-luxury-hero__secondary home-luxury-hero__share">
              <Icon name="mdi:share-variant-outline" class="text-lg" />
              <span>شارك لصديقك واربح نقاط</span>
            </NuxtLink>
            <NuxtLink to="/brands" class="home-luxury-hero__secondary">
              {{ t('nav.brands') }}
            </NuxtLink>
          </div>
        </div>

        <div class="home-luxury-hero__visual home-luxury-hero__visual--logo" aria-hidden="true">
          <div class="home-luxury-hero__logo-stage">
            <div class="home-luxury-hero__logo-ring home-luxury-hero__logo-ring--one" />
            <div class="home-luxury-hero__logo-ring home-luxury-hero__logo-ring--two" />
            <div class="home-luxury-hero__orb home-luxury-hero__orb--large home-luxury-hero__orb--logo">
              <img :src="heroLogo" alt="" />
            </div>
            <div class="home-luxury-hero__orb home-luxury-hero__orb--small home-luxury-hero__orb--top brand-orbit-card">
              <Transition name="brand-orbit-fade" mode="out-in">
                <img
                  v-if="orbitBrandTop"
                  :key="`top-${orbitBrandTop.id || orbitBrandTop.slug || brandOrbitIndex}`"
                  :src="brandOrbitSrc(orbitBrandTop)"
                  :alt="orbitBrandTop.name || 'Brand'"
                />
                <span v-else key="beauty">Beauty</span>
              </Transition>
            </div>
            <div class="home-luxury-hero__orb home-luxury-hero__orb--small home-luxury-hero__orb--bottom brand-orbit-card">
              <Transition name="brand-orbit-fade" mode="out-in">
                <img
                  v-if="orbitBrandBottom"
                  :key="`bottom-${orbitBrandBottom.id || orbitBrandBottom.slug || brandOrbitIndex}`"
                  :src="brandOrbitSrc(orbitBrandBottom)"
                  :alt="orbitBrandBottom.name || 'Brand'"
                />
                <span v-else key="store">Store</span>
              </Transition>
            </div>
          </div>
        </div>
      </div>
    </section>
    <section class="mx-auto max-w-[92rem] px-4 pt-3 pb-8 lg:px-6">
      <div class="home-section-panel home-section-panel--categories category-command-center category-command-center--raised">
        <div class="flex flex-col gap-4 lg:flex-row lg:items-end lg:justify-between">
          <div>
            <div class="inline-flex items-center gap-2 rounded-full border border-app bg-surface/80 px-3 py-1 text-[11px] font-bold text-[rgb(var(--muted))] backdrop-blur rtl-text">
              <span class="h-2 w-2 rounded-full bg-[rgb(var(--primary))]" />
              أقسام المتجر
            </div>
            <h2 class="mt-4 text-2xl font-extrabold tracking-tight text-[rgb(var(--text))] sm:text-4xl rtl-text">
              {{ t('home.spotlightTitle') }}
            </h2>
            <p class="mt-2 max-w-2xl text-sm text-[rgb(var(--muted))] sm:text-base rtl-text">
              تجربة أقرب للمتاجر العالمية: اختر التصنيف من الشريط، وعلى الحاسبة تظهر لك معاينة منظمة تساعدك تصل بسرعة.
            </p>
          </div>
        </div>

        <div class="category-navbar-shell" @mouseleave="closeCategoriesMenu()">
          <button
            type="button"
            class="category-navbar-arrow category-navbar-arrow--start"
            aria-label="السابق"
            @click="scrollRail('prev', categoryRail)"
          >
            <Icon name="mdi:chevron-right" class="text-xl" />
          </button>

          <div
            ref="categoryRail"
            class="category-secondary-bar__scroll category-secondary-bar__scroll--unified"
            @pointerdown="(e) => onRailPointerDown(e, categoryRail)"
            @pointermove="onRailPointerMove"
            @pointerup="endRailDrag"
            @pointercancel="endRailDrag"
            @pointerleave="endRailDrag"
          >
            <template v-for="c in categoryCards" :key="c.key">
              <button
                v-if="c.hasDetailSections || c.childCount > 0"
                type="button"
                class="category-secondary-bar__item"
                :class="activeCategory?.key === c.key && categoriesMenuOpen ? 'is-active' : ''"
                @mouseenter="openCategoriesMenu(c.key)"
                @focus="openCategoriesMenu(c.key)"
                @click="openCategoriesMenu(c.key)"
              >
                <span>{{ c.title }}</span>
                <Icon name="mdi:chevron-down" class="category-chevron" />
              </button>
              <NuxtLink
                v-else
                :to="c.to"
                class="category-secondary-bar__item"
                @click="onRailLinkClick"
              >
                <span>{{ c.title }}</span>
              </NuxtLink>
            </template>
          </div>

          <button
            type="button"
            class="category-navbar-arrow category-navbar-arrow--end"
            aria-label="التالي"
            @click="scrollRail('next', categoryRail)"
          >
            <Icon name="mdi:chevron-left" class="text-xl" />
          </button>

          <Transition name="fade-slide">
            <div
              v-if="categoriesMenuOpen && activeCategory && activeCategoryChildren.length"
              class="category-dropdown-panel"
              @mouseenter="openCategoriesMenu(activeCategory.key)"
            >
              <div class="category-dropdown-panel__head">
                <div>
                  <div class="text-xs font-bold uppercase tracking-[0.24em] text-[rgb(var(--muted))]">{{ activeCategory.title }}</div>
                  <div class="mt-2 text-lg font-extrabold text-[rgb(var(--text))] rtl-text">اختر التصنيف الدقيق</div>
                </div>
                <NuxtLink :to="activeCategory.to" class="category-dropdown-panel__all">عرض الكل</NuxtLink>
              </div>
              <div class="category-dropdown-panel__grid">
                <NuxtLink
                  v-for="child in activeCategoryChildren"
                  :key="child.id || child.key"
                  :to="`/categories/${encodeURIComponent(activeCategory.key)}/${encodeURIComponent(String(child.key || '').toLowerCase())}`"
                  class="category-dropdown-panel__link"
                >
                  <div class="category-dropdown-panel__icon">
                    <img v-if="child.imageUrl" :src="buildAssetUrl(child.imageUrl)" :alt="child.nameAr" class="h-full w-full object-cover" />
                    <span v-else>{{ child.nameAr?.slice(0, 1) }}</span>
                  </div>
                  <div class="min-w-0">
                    <div class="truncate text-sm font-extrabold text-[rgb(var(--text))] rtl-text">{{ child.nameAr }}</div>
                    <div class="mt-1 truncate text-xs text-[rgb(var(--muted))] rtl-text">{{ child.descriptionAr || 'عرض المنتجات' }}</div>
                  </div>
                  <Icon name="mdi:arrow-left" class="text-base text-[rgb(var(--muted))]" />
                </NuxtLink>
              </div>
            </div>
          </Transition>
        </div>
      </div>
    </section>

    <section class="mx-auto max-w-[92rem] px-4 pb-20 lg:px-6">
      <div class="home-section-panel home-section-panel--brands">
        <div class="flex flex-col items-start justify-between gap-4 sm:flex-row sm:items-end">
          <div>
            <h2 class="text-2xl font-extrabold tracking-tight text-[rgb(var(--text))] sm:text-4xl">{{ t('home.brands') }}</h2>
            <p class="mt-2 max-w-2xl text-sm text-[rgb(var(--muted))] sm:text-base">{{ t('home.brandsSubtitle') }}</p>
          </div>
          <NuxtLink
            to="/brands"
            class="btn inline-flex items-center gap-2 rounded-full px-4 py-2 text-sm font-semibold shadow-soft"
          >
            {{ t('nav.brands') }}
            <span aria-hidden="true">→</span>
          </NuxtLink>
        </div>


        <BrandMarquee :brands="topBrands" />
      </div>
    </section>

    <section v-if="problemCards.length" class="mx-auto max-w-[92rem] px-4 pb-16 lg:px-6">
      <div class="home-section-panel home-section-panel--categories">
        <div>
          <h2 class="text-2xl font-extrabold tracking-tight text-[rgb(var(--text))] sm:text-4xl">{{ t('home.problemCategoriesTitle') || 'حلول المشاكل' }}</h2>
          <p class="mt-2 max-w-2xl text-sm text-[rgb(var(--muted))] sm:text-base">{{ t('home.problemCategoriesSubtitle') || 'تسوق حسب المشكلة التي تريد حلها بسرعة.' }}</p>
        </div>
        <div class="rail-wrap mt-8">
          <button type="button" class="rail-arrow-btn rail-arrow-btn--prev hidden lg:inline-flex" @click="scrollRail('prev', problemCategoryRail)" aria-label="السابق">
            <Icon name="mdi:chevron-left" class="text-xl" />
          </button>
          <button type="button" class="rail-arrow-btn rail-arrow-btn--next hidden lg:inline-flex" @click="scrollRail('next', problemCategoryRail)" aria-label="التالي">
            <Icon name="mdi:chevron-right" class="text-xl" />
          </button>
          <div ref="problemCategoryRail" class="category-unified-rail" @pointerdown="(e) => onRailPointerDown(e, problemCategoryRail)" @pointermove="onRailPointerMove" @pointerup="endRailDrag" @pointercancel="endRailDrag" @pointerleave="endRailDrag">
            <NuxtLink v-for="c in problemCards" :key="c.key" :to="c.to" class="category-mobile-pill" @click="onRailLinkClick">
              <div class="category-mobile-pill__image-wrap" :class="`bg-gradient-to-br ${c.accent}`">
                <img v-if="c.imageUrl" :src="buildAssetUrl(c.imageUrl)" :alt="c.title" class="category-mobile-pill__image" />
                <div v-else class="category-mobile-pill__fallback">{{ c.title?.slice(0, 1) }}</div>
              </div>
              <div class="category-mobile-pill__title">{{ c.title }}</div>
            </NuxtLink>
          </div>
        </div>
      </div>
    </section>

    <section class="mx-auto max-w-[92rem] px-4 pb-16 pt-12 sm:pt-14 lg:px-6">
      <div class="home-section-panel">
        <div class="flex flex-col items-center justify-center gap-4 text-center">
          <div class="section-kicker" />
          <h2 class="text-2xl font-extrabold text-[rgb(var(--text))] sm:text-4xl">{{ t('homeHero.featuredProducts') }}</h2>

          <div class="inline-flex items-center rounded-full border border-app bg-surface p-1 shadow-soft flex-wrap justify-center gap-1">
            <button
              type="button"
              class="px-4 py-2 rounded-full text-sm font-bold transition"
              :class="tab === 'featured' ? 'bg-[rgb(var(--primary))] text-black shadow-[0_10px_24px_rgba(var(--primary),0.25)]' : 'text-[rgb(var(--text))] hover:bg-surface-2'"
              @click="tab = 'featured'"
            >
              {{ t('home.featuredTab') }}
            </button>
            <button
              type="button"
              class="px-4 py-2 rounded-full text-sm font-bold transition"
              :class="tab === 'discounts' ? 'bg-[rgb(var(--primary))] text-black shadow-[0_10px_24px_rgba(var(--primary),0.25)]' : 'text-[rgb(var(--text))] hover:bg-surface-2'"
              @click="tab = 'discounts'"
            >
              {{ t('home.discountsTab') }}
            </button>
            <button
              type="button"
              class="px-4 py-2 rounded-full text-sm font-bold transition"
              :class="tab === 'topRated' ? 'bg-[rgb(var(--primary))] text-black shadow-[0_10px_24px_rgba(var(--primary),0.25)]' : 'text-[rgb(var(--text))] hover:bg-surface-2'"
              @click="tab = 'topRated'"
            >
              {{ t('home.topRatedProducts') }}
            </button>
          </div>
        </div>

        <div v-if="displayedFeatured.length" class="product-grid-luxury mt-10 grid grid-cols-2 gap-3 sm:gap-5 lg:grid-cols-4">
          <RevealOnScroll
            v-for="(p, idx) in displayedFeatured"
            :key="p.id"
            :parity="idx % 2"
          >
            <ProductCard :p="p" />
          </RevealOnScroll>
        </div>
        <div v-else class="mt-10 rounded-[1.75rem] border border-app bg-surface p-8 text-center text-sm text-[rgb(var(--muted))]">
          {{ tab === 'topRated' ? t('home.noTopRatedProducts') : t('products.empty') }}
        </div>
      </div>
    </section>
  </div>
</template>
<style scoped>
.section-kicker{
  width:88px;
  height:6px;
  border-radius:999px;
  background:linear-gradient(90deg, rgba(var(--primary), .25), rgba(var(--primary), .88), rgba(var(--cta-glow-2), .35));
  box-shadow:0 8px 24px rgba(var(--primary), .25);
}
.shadow-soft{
  box-shadow:0 16px 38px rgba(0,0,0,.08);
}
.rail-wrap{
  position: relative;
}
.rail-arrow-btn{
  width: 3rem;
  height: 3rem;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  border-radius: 999px;
  border: 1px solid rgba(var(--border), .95);
  background: rgba(var(--surface), .96);
  color: rgb(var(--text));
  box-shadow: 0 16px 34px rgba(0,0,0,.18);
  transition: transform .18s ease, border-color .18s ease, background .18s ease, box-shadow .18s ease;
  position: absolute;
  top: 50%;
  z-index: 3;
  transform: translateY(-50%);
  backdrop-filter: blur(10px);
}
.rail-arrow-btn--prev{ left: .45rem; }
.rail-arrow-btn--next{ right: .45rem; }
.rail-arrow-btn:hover{
  transform: translateY(-50%) scale(1.04);
  border-color: rgba(var(--primary), .55);
  background: rgba(var(--surface-2), .98);
  box-shadow: 0 18px 38px rgba(0,0,0,.22);
}
.category-unified-rail{
  display:grid;
  grid-auto-flow:column;
  grid-auto-columns:120px;
  gap:1rem;
  overflow-x:auto;
  overflow-y:hidden;
  padding:.2rem 3.7rem .55rem;
  scroll-snap-type:x proximity;
  -webkit-overflow-scrolling:touch;
  scrollbar-width:none;

  cursor:grab;
  user-select:none;
}
.category-unified-rail.is-dragging{
  cursor:grabbing;
}
.category-unified-rail > *{
  user-select:none;
}
.category-unified-rail::-webkit-scrollbar{ display:none; }
.category-mobile-pill{
  display:flex;
  flex-direction:column;
  align-items:center;
  gap:.72rem;
  scroll-snap-align:start;
}
.category-mobile-pill__image-wrap{
  width:112px;
  height:112px;
  border-radius:999px;
  overflow:hidden;
  border:1px solid rgba(var(--border), .9);
  background:linear-gradient(180deg, rgba(var(--surface-rgb), .98), rgba(var(--surface-2-rgb), .92));
  box-shadow:0 18px 40px rgba(0,0,0,.14), inset 0 1px 0 rgba(255,255,255,.08);
  display:grid;
  place-items:center;
}
.category-mobile-pill__image{
  width:100%;
  height:100%;
  object-fit:cover;
}
.category-mobile-pill__fallback{
  font-size:2rem;
  font-weight:900;
  color:rgb(var(--text));
}
.category-mobile-pill__title{
  width:100%;
  color:rgb(var(--text));
  text-align:center;
  font-size:1rem;
  line-height:1.35;
  font-weight:900;
  display:-webkit-box;
  -webkit-line-clamp:2;
  -webkit-box-orient:vertical;
  overflow:hidden;
  min-height:2.7em;
}

.category-showcase{
  align-items:stretch;
}
.category-simple-card{
  position:relative;
  display:block;
  overflow:hidden;
  min-height:168px;
  border-radius:34px;
  border:1px solid rgba(var(--border), .92);
  background:linear-gradient(145deg, rgba(var(--surface-rgb), .96), rgba(var(--surface-2-rgb), .84));
  box-shadow:0 20px 48px rgba(5,8,20,.22), inset 0 1px 0 rgba(255,255,255,.08);
  transition:transform .24s ease, border-color .24s ease, box-shadow .24s ease;
  isolation:isolate;
}
.category-simple-card::before{
  content:'';
  position:absolute;
  inset:auto auto -70px -60px;
  width:170px;
  height:170px;
  border-radius:50%;
  background:radial-gradient(circle, rgba(var(--primary), .18), transparent 68%);
  filter:blur(4px);
  pointer-events:none;
}
.category-simple-card::after{
  content:'';
  position:absolute;
  inset:1px;
  border-radius:32px;
  background:linear-gradient(180deg, rgba(255,255,255,.035), transparent 24%, transparent 76%, rgba(255,255,255,.02));
  pointer-events:none;
  z-index:0;
}
.category-simple-card__inner{
  position:relative;
  z-index:1;
  display:grid;
  grid-template-columns:96px minmax(0,1fr) 44px;
  align-items:center;
  gap:1rem;
  min-height:168px;
  padding:1.1rem 1.05rem;
}
.category-simple-card__thumb{
  width:96px;
  height:96px;
  border-radius:28px;
  overflow:hidden;
  background:linear-gradient(180deg, rgba(255,255,255,.14), rgba(255,255,255,.05));
  border:1px solid rgba(255,255,255,.16);
  box-shadow:0 14px 30px rgba(0,0,0,.18), inset 0 1px 0 rgba(255,255,255,.18);
  display:grid;
  place-items:center;
}
.category-simple-card__img{
  width:100%;
  height:100%;
  object-fit:cover;
}
.category-simple-card__fallback{
  display:grid;
  place-items:center;
  width:100%;
  height:100%;
  font-size:2rem;
  font-weight:900;
  color:rgb(var(--text));
}
.category-simple-card__body{
  min-width:0;
}
.category-simple-card__title{
  color:rgb(var(--text));
  font-size:1.08rem;
  font-weight:900;
  line-height:1.2;
  margin-bottom:.4rem;
  display:-webkit-box;
  -webkit-line-clamp:2;
  -webkit-box-orient:vertical;
  overflow:hidden;
}
.category-simple-card__subtitle{
  color:rgb(var(--muted));
  font-size:.82rem;
  line-height:1.7;
  display:-webkit-box;
  -webkit-line-clamp:2;
  -webkit-box-orient:vertical;
  overflow:hidden;
  min-height:2.7em;
}
.category-simple-card__meta{
  display:inline-flex;
  margin-top:.7rem;
  padding:.32rem .64rem;
  border-radius:999px;
  border:1px solid rgba(var(--border), .85);
  background:rgba(255,255,255,.08);
  color:rgb(var(--muted));
  font-size:.68rem;
  font-weight:800;
  text-transform:uppercase;
  letter-spacing:.06em;
  max-width:max-content;
}
.category-simple-card__arrow{
  display:grid;
  place-items:center;
  width:44px;
  height:44px;
  border-radius:50%;
  border:1px solid rgba(var(--border), .9);
  background:rgba(255,255,255,.1);
  color:rgb(var(--text));
  font-size:1rem;
  box-shadow:0 10px 22px rgba(0,0,0,.14);
}
.category-simple-card:hover{
  transform:translateY(-6px) scale(1.01);
  border-color:rgba(var(--primary), .38);
  box-shadow:0 30px 68px rgba(6,10,24,.26), inset 0 1px 0 rgba(255,255,255,.12);
}
.category-simple-card:hover .category-simple-card__arrow{
  transform:translateX(-3px);
  background:rgba(var(--primary), .16);
  border-color:rgba(var(--primary), .28);
}
:global(html.theme-light) .home-section-panel{
  background:
    linear-gradient(180deg, rgba(255,255,255,.995), rgba(255,255,255,.985)),
    linear-gradient(135deg, rgba(236,72,153,.018), transparent 42%, rgba(244,114,182,.026) 100%);
}
:global(html.theme-light) .category-simple-card{
  background:linear-gradient(180deg, rgba(255,255,255,.995), rgba(255,255,255,.982));
  box-shadow:0 18px 44px rgba(232,91,154,.08), 0 10px 26px rgba(24,24,24,.05);
}
:global(html.theme-light) .category-simple-card:hover{
  background:linear-gradient(180deg, rgba(255,255,255,1), rgba(255,255,255,.99));
  box-shadow:0 18px 40px rgba(22,22,22,.06);
}
:global(html.theme-dark) .home-section-panel,
:global(html.theme-dark) .category-simple-card{
  box-shadow:0 18px 44px rgba(0,0,0,.24);
}
:global(html.theme-dark) .category-simple-card{
  background:linear-gradient(180deg, rgba(var(--surface-rgb), .98), rgba(var(--surface-2-rgb), .86));
}
@media (max-width: 1280px){
  .category-showcase{ grid-template-columns:repeat(3, minmax(0,1fr)); }
}
@media (max-width: 1024px){
  .category-showcase{ grid-template-columns:repeat(2, minmax(0,1fr)); }
}
@media (max-width: 768px){
  .category-simple-card{ min-height:148px; border-radius:28px; }
  .category-simple-card__inner{ grid-template-columns:84px minmax(0,1fr) 40px; min-height:148px; padding:1rem .95rem; gap:.85rem; }
  .category-simple-card__thumb{ width:84px; height:84px; border-radius:24px; }
  .category-simple-card__title{ font-size:1rem; }
  .category-simple-card__subtitle{ font-size:.78rem; }
  .category-simple-card__arrow{ width:40px; height:40px; }
}

.category-command-center--raised{padding-top:1rem}
.category-navbar-shell{position:relative;margin-top:1.15rem;border:1px solid rgba(var(--border),.95);border-radius:1.35rem;background:linear-gradient(180deg,rgba(var(--surface-rgb),.97),rgba(var(--surface-2-rgb),.9));box-shadow:0 18px 44px rgba(0,0,0,.15);padding:.55rem 3.15rem}
.category-secondary-bar__scroll{display:flex;align-items:center;gap:.55rem;overflow-x:auto;overflow-y:hidden;padding:.12rem;scrollbar-width:none;-webkit-overflow-scrolling:touch;scroll-snap-type:x proximity;cursor:grab}
.category-secondary-bar__scroll.is-dragging{cursor:grabbing}
.category-secondary-bar__scroll::-webkit-scrollbar{display:none}
.category-secondary-bar__scroll--unified{width:100%}
.category-secondary-bar__item{display:inline-flex;align-items:center;justify-content:center;gap:.45rem;white-space:nowrap;min-height:2.75rem;padding:0 1rem;border-radius:999px;border:1px solid rgba(var(--border),.75);background:rgba(var(--surface-rgb),.72);font-size:.9rem;font-weight:900;color:rgb(var(--text));transition:all .18s ease;scroll-snap-align:start;flex:0 0 auto}
.category-secondary-bar__item:hover,.category-secondary-bar__item.is-active{border-color:rgba(var(--primary),.45);background:rgba(var(--primary),.12);box-shadow:0 10px 24px rgba(var(--primary),.12)}
.category-chevron{font-size:1rem;opacity:.72;transition:transform .18s ease}.category-secondary-bar__item.is-active .category-chevron{transform:rotate(180deg)}
.category-navbar-arrow{position:absolute;top:50%;transform:translateY(-50%);z-index:4;width:2.35rem;height:2.35rem;border-radius:999px;border:1px solid rgba(var(--border),.95);background:rgba(var(--surface-rgb),.98);display:flex;align-items:center;justify-content:center;color:rgb(var(--text));box-shadow:0 12px 28px rgba(0,0,0,.16);transition:all .18s ease}
.category-navbar-arrow:hover{border-color:rgba(var(--primary),.48);background:rgba(var(--primary),.12)}
.category-navbar-arrow--start{right:.45rem}.category-navbar-arrow--end{left:.45rem}
.category-dropdown-panel{margin-top:.8rem;border:1px solid rgba(var(--border),.95);border-radius:1.6rem;background:linear-gradient(180deg,rgba(var(--surface-rgb),.98),rgba(var(--surface-2-rgb),.94));box-shadow:0 20px 44px rgba(0,0,0,.18);padding:1.1rem 1.1rem 1rem}
.category-dropdown-panel__head{display:flex;align-items:center;justify-content:space-between;gap:1rem;padding:.2rem .2rem .9rem}
.category-dropdown-panel__all{display:inline-flex;align-items:center;justify-content:center;min-height:2.7rem;padding:0 1rem;border-radius:999px;border:1px solid rgba(var(--primary),.35);background:rgba(var(--primary),.1);font-size:.85rem;font-weight:800;color:rgb(var(--text))}
.category-dropdown-panel__grid{display:grid;grid-template-columns:repeat(4,minmax(0,1fr));gap:.85rem}
.category-dropdown-panel__link{display:flex;align-items:center;gap:.85rem;min-height:5.3rem;padding:.85rem;border-radius:1.2rem;border:1px solid rgba(var(--border),.8);background:rgba(var(--surface-rgb),.72);transition:all .18s ease}
.category-dropdown-panel__link:hover{transform:translateY(-2px);border-color:rgba(var(--primary),.38);background:rgba(var(--surface-2-rgb),.95)}
.category-dropdown-panel__icon{flex:0 0 3.1rem;width:3.1rem;height:3.1rem;border-radius:1rem;overflow:hidden;border:1px solid rgba(var(--border),.8);display:flex;align-items:center;justify-content:center;background:rgba(var(--surface-2-rgb),.95);font-size:1.15rem;font-weight:900;color:rgb(var(--text))}
@media (max-width: 1279px){.category-dropdown-panel__grid{grid-template-columns:repeat(3,minmax(0,1fr))}}
@media (max-width: 768px){
  .category-command-center--raised{padding-top:.85rem}
  .category-navbar-shell{margin-top:1rem;border-radius:1.15rem;padding:.45rem 2.65rem}
  .category-navbar-arrow{width:2.05rem;height:2.05rem}
  .category-navbar-arrow--start{right:.35rem}.category-navbar-arrow--end{left:.35rem}
  .category-secondary-bar__scroll{gap:.45rem;padding:.05rem}
  .category-secondary-bar__item{min-height:2.45rem;padding:0 .82rem;font-size:.82rem}
  .category-dropdown-panel{border-radius:1.15rem;padding:.85rem;margin-top:.55rem}
  .category-dropdown-panel__grid{grid-template-columns:1fr;gap:.55rem;max-height:18rem;overflow:auto}
  .category-dropdown-panel__link{min-height:4.6rem;border-radius:1rem}
}


.home-page-shell{
  position:relative;
  overflow:hidden;
}
.home-page-shell::before{
  content:'';
  position:absolute;
  inset:0;
  pointer-events:none;
  background:
    radial-gradient(circle at 14% 12%, rgba(var(--primary), .12), transparent 30rem),
    radial-gradient(circle at 86% 18%, rgba(var(--cta-glow-2), .10), transparent 28rem),
    linear-gradient(180deg, rgba(var(--surface-2-rgb), .16), transparent 22rem);
  opacity:.9;
}
.home-page-shell > section{ position:relative; z-index:1; }
.home-luxury-hero{
  position:relative;
  display:grid;
  grid-template-columns:minmax(0, 1.05fr) minmax(320px, .72fr);
  gap:2rem;
  align-items:center;
  min-height:430px;
  overflow:hidden;
  border:1px solid rgba(var(--border), .88);
  border-radius:2.25rem;
  padding:2rem;
  background:
    linear-gradient(135deg, rgba(var(--surface-rgb), .98), rgba(var(--surface-2-rgb), .86)),
    radial-gradient(circle at 16% 18%, rgba(var(--primary), .16), transparent 28rem);
  box-shadow:0 28px 72px rgba(0,0,0,.24), inset 0 1px 0 rgba(255,255,255,.08);
  isolation:isolate;
}
.home-luxury-hero::after{
  content:'';
  position:absolute;
  inset:1px;
  border-radius:2.15rem;
  pointer-events:none;
  background:linear-gradient(180deg, rgba(255,255,255,.05), transparent 35%, rgba(255,255,255,.025));
  z-index:-1;
}
.home-luxury-hero__glow{
  position:absolute;
  border-radius:999px;
  filter:blur(4px);
  pointer-events:none;
  opacity:.72;
}
.home-luxury-hero__glow--one{
  width:26rem; height:26rem; right:-8rem; top:-11rem;
  background:radial-gradient(circle, rgba(var(--primary), .22), transparent 68%);
}
.home-luxury-hero__glow--two{
  width:24rem; height:24rem; left:-8rem; bottom:-12rem;
  background:radial-gradient(circle, rgba(var(--cta-glow-2), .18), transparent 68%);
}
.home-luxury-hero__content{
  position:relative;
  z-index:2;
  max-width:780px;
}
.home-luxury-hero__kicker{
  display:inline-flex;
  align-items:center;
  gap:.55rem;
  min-height:2.25rem;
  padding:0 .9rem;
  border-radius:999px;
  border:1px solid rgba(var(--primary), .28);
  background:rgba(var(--primary), .10);
  color:rgb(var(--text));
  font-size:.78rem;
  font-weight:900;
}
.home-luxury-hero__dot{
  width:.58rem;
  height:.58rem;
  border-radius:999px;
  background:rgb(var(--primary));
  box-shadow:0 0 0 6px rgba(var(--primary), .12);
}
.home-luxury-hero__title{
  margin-top:1.15rem;
  max-width:13ch;
  color:rgb(var(--text-strong));
  font-size:clamp(2.7rem, 7vw, 6.8rem);
  line-height:.95;
  letter-spacing:-.06em;
  font-weight:1000;
}
.home-luxury-hero__subtitle{
  margin-top:1.1rem;
  max-width:42rem;
  color:rgb(var(--text-soft));
  font-size:clamp(1rem, 1.45vw, 1.18rem);
  line-height:1.9;
}
.home-luxury-hero__actions{
  margin-top:1.6rem;
  display:flex;
  flex-wrap:wrap;
  gap:.8rem;
}
.home-luxury-hero__primary,.home-luxury-hero__secondary{
  min-height:3.15rem;
  display:inline-flex;
  align-items:center;
  justify-content:center;
  gap:.55rem;
  border-radius:999px;
  padding:0 1.25rem;
  font-weight:900;
  transition:transform .18s ease, box-shadow .18s ease, border-color .18s ease, background .18s ease;
}
.home-luxury-hero__primary{
  color:rgb(var(--text));
  background:linear-gradient(135deg, rgb(var(--primary)), rgba(var(--cta-glow-2), .92));
  box-shadow:0 18px 38px rgba(var(--primary), .26);
  text-shadow:0 1px 1px rgba(0,0,0,.22);
}
.home-luxury-hero__secondary{
  color:rgb(var(--text));
  border:1px solid rgba(var(--border), .88);
  background:rgba(var(--surface-rgb), .68);
}
.home-luxury-hero__primary:hover,.home-luxury-hero__secondary:hover{ transform:translateY(-2px); }

.home-luxury-hero__actions--moved{
  margin-top:1rem;
  padding-top:0;
}
.home-luxury-hero__inline-ad{
  margin-top:0;
  width:100%;
  max-width:44rem;
  border:1px solid rgba(var(--border), .82);
  border-radius:1.45rem;
  overflow:hidden;
  background:rgba(var(--surface-rgb), .66);
  box-shadow:0 18px 48px rgba(0,0,0,.16);
}
.home-luxury-hero__inline-ad-link{
  position:relative;
  display:block;
  min-height:14rem;
  color:rgb(var(--text));
}
.home-luxury-hero__inline-ad-media{
  width:100%;
  height:clamp(14rem, 22vw, 18rem);
  object-fit:contain;
  object-position:center;
  display:block;
}
.home-luxury-hero__inline-ad-text{
  position:absolute;
  inset:auto 1rem 1rem 1rem;
  display:grid;
  gap:.2rem;
  max-width:22rem;
  padding:.85rem 1rem;
  border-radius:1rem;
  background:rgba(var(--surface-rgb), .78);
  border:1px solid rgba(var(--border), .7);
  backdrop-filter:blur(16px);
}
.home-luxury-hero__inline-ad-text span{
  color:rgb(var(--primary));
  font-size:.72rem;
  font-weight:1000;
}
.home-luxury-hero__inline-ad-text b{
  color:rgb(var(--text-strong));
  font-size:1rem;
  font-weight:1000;
}
.home-luxury-hero__inline-ad-text small{
  color:rgb(var(--muted));
  font-size:.78rem;
  font-weight:800;
}
.home-luxury-hero__stats{
  margin-top:1.6rem;
  display:grid;
  grid-template-columns:repeat(3, minmax(0, 1fr));
  gap:.75rem;
  max-width:38rem;
}
.home-luxury-hero__stat{
  border:1px solid rgba(var(--border), .78);
  border-radius:1.2rem;
  background:rgba(var(--surface-rgb), .62);
  padding:.9rem 1rem;
}
.home-luxury-hero__stat strong{
  display:block;
  color:rgb(var(--text-strong));
  font-size:1.25rem;
  line-height:1;
  font-weight:1000;
}
.home-luxury-hero__stat span{
  display:block;
  margin-top:.35rem;
  color:rgb(var(--muted));
  font-size:.76rem;
  font-weight:800;
}
.home-luxury-hero__visual{
  position:relative;
  z-index:2;
  min-height:350px;
}
.home-luxury-hero__visual--logo{
  display:grid;
  place-items:center;
}
.home-luxury-hero__logo-stage{
  position:relative;
  width:min(24rem, 100%);
  aspect-ratio:1/1;
  display:grid;
  place-items:center;
}
.home-luxury-hero__logo-ring{
  position:absolute;
  border-radius:999px;
  pointer-events:none;
}
.home-luxury-hero__logo-ring--one{
  inset:-1.35rem;
  border:1px solid rgba(var(--primary), .22);
  background:radial-gradient(circle, rgba(var(--primary), .13), transparent 67%);
}
.home-luxury-hero__logo-ring--two{
  inset:2rem;
  border:1px dashed rgba(var(--border), .9);
  opacity:.7;
}
.home-luxury-hero__orb{
  position:absolute;
  display:grid;
  place-items:center;
  overflow:hidden;
  border:1px solid rgba(var(--border), .9);
  background:linear-gradient(180deg, rgba(255,255,255,.14), rgba(255,255,255,.04));
  box-shadow:0 28px 70px rgba(0,0,0,.26), inset 0 1px 0 rgba(255,255,255,.14);
  color:rgb(var(--text));
  font-weight:1000;
}
.home-luxury-hero__orb img{ width:100%; height:100%; object-fit:cover; }
.home-luxury-hero__orb--large{
  position:relative;
  width:min(20rem, 84%);
  aspect-ratio:1/1;
  border-radius:36% 64% 46% 54% / 45% 40% 60% 55%;
}
.home-luxury-hero__orb--logo{
  background:linear-gradient(145deg, rgba(var(--surface-rgb), .98), rgba(var(--surface-2-rgb), .88));
  padding:2.1rem;
}
.home-luxury-hero__orb--logo img{
  object-fit:contain;
  filter:drop-shadow(0 18px 28px rgba(0,0,0,.22));
}
.home-luxury-hero__orb--small{
  width:7.2rem;
  height:7.2rem;
  border-radius:2rem;
  backdrop-filter:blur(16px);
}
.home-luxury-hero__orb--top{ top:0; left:1rem; transform:rotate(-6deg); }
.home-luxury-hero__orb--bottom{ bottom:1rem; right:3rem; transform:rotate(7deg); }

.home-luxury-hero__primary{
  position:relative;
  overflow:hidden;
  border:1px solid rgba(var(--primary), .55);
}
.home-luxury-hero__primary::before{
  content:'';
  position:absolute;
  inset:0;
  background:linear-gradient(135deg, rgba(255,255,255,.18), transparent 42%, rgba(255,255,255,.10));
  opacity:.75;
  pointer-events:none;
}
.home-luxury-hero__primary > *{ position:relative; z-index:1; }
.brand-orbit-card{
  background:linear-gradient(180deg, rgba(var(--surface-rgb), .98), rgba(var(--surface-2-rgb), .84));
  padding:.55rem;
}
.brand-orbit-card img{
  width:100%;
  height:100%;
  object-fit:cover;
  border-radius:1.45rem;
  filter:none;
}
.brand-orbit-card span{
  display:grid;
  place-items:center;
  width:100%;
  height:100%;
  border-radius:1.45rem;
  background:rgba(var(--surface-rgb), .72);
}
.brand-orbit-fade-enter-active,
.brand-orbit-fade-leave-active{
  transition:opacity .35s ease, transform .35s ease;
}
.brand-orbit-fade-enter-from,
.brand-orbit-fade-leave-to{
  opacity:0;
  transform:scale(.92) rotate(-3deg);
}

.product-grid-luxury{ align-items:stretch; }
.product-grid-luxury > *{ min-width:0; }
:global(html.theme-light) .home-luxury-hero{
  background:
    linear-gradient(135deg, rgba(255,255,255,.995), rgba(255,255,255,.94)),
    radial-gradient(circle at 18% 18%, rgba(236,72,153,.12), transparent 28rem);
  box-shadow:0 24px 70px rgba(232,91,154,.08), 0 14px 34px rgba(20,20,20,.05);
}
:global(html.theme-light) .home-luxury-hero__primary{ color:#fff; background:linear-gradient(135deg, #111827, rgb(var(--primary))); box-shadow:0 18px 38px rgba(17,24,39,.16), 0 14px 34px rgba(var(--primary), .20); }
:global(html.theme-light) .home-luxury-hero__secondary,
:global(html.theme-light) .home-luxury-hero__stat{ background:rgba(255,255,255,.82); }
@media (max-width: 1024px){
  .home-luxury-hero{ grid-template-columns:1fr; padding:1.35rem; min-height:auto; }
  .home-luxury-hero__visual{ min-height:260px; order:-1; }
  .home-luxury-hero__logo-stage{ width:min(20rem, 82vw); }
  .home-luxury-hero__orb--large{ width:min(17rem, 78vw); }
  .home-luxury-hero__orb--top{ left:8%; top:1rem; }
  .home-luxury-hero__orb--bottom{ right:8%; bottom:.5rem; }
}
@media (max-width: 640px){
  .home-luxury-hero{ border-radius:1.6rem; padding:1rem; }
  .home-luxury-hero::after{ border-radius:1.5rem; }
  .home-luxury-hero__visual{ min-height:210px; }
  .home-luxury-hero__logo-stage{ width:17rem; }
  .home-luxury-hero__orb--large{ width:14.5rem; }
  .home-luxury-hero__orb--logo{ padding:1.55rem; }
  .home-luxury-hero__orb--small{ width:5.6rem; height:5.6rem; border-radius:1.45rem; }
  .home-luxury-hero__title{ font-size:3rem; max-width:12ch; }
  .home-luxury-hero__subtitle{ font-size:.95rem; line-height:1.75; }
  .home-luxury-hero__actions{ display:grid; grid-template-columns:1fr; }
  .home-luxury-hero__actions--moved{ margin-top:.85rem; padding-top:0; }
  .home-luxury-hero__inline-ad-link{ min-height:10rem; }
  .home-luxury-hero__inline-ad-media{ height:10rem; }
}

</style>
