<script setup lang="ts">
import { computed, ref, onMounted, onBeforeUnmount, watch } from 'vue'
import { useAsyncData } from '#app'
import { useBrandsStore } from '~/stores/brands'
import { useProductsStore } from '~/stores/products'

const { t, locale } = useI18n()
const { categories, problemCategories, fetchCategories } = useCategories()

const brandsStore = useBrandsStore()
const productsStore = useProductsStore()

await useAsyncData(
  'home-prefetch',
  async () => {
    await Promise.allSettled([
      brandsStore.fetchPublic(),
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
  return uniq.slice(0, 10)
})
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
  }))
})



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
    to: `/categories/${encodeURIComponent(String(c.key || '').toLowerCase())}`,
    accent: accents[idx % accents.length],
  }))
})

const { buildAssetUrl } = useApi()
const categoryRail = ref<HTMLElement | null>(null)
const problemCategoryRail = ref<HTMLElement | null>(null)
const dragState = { active: false, startX: 0, startScroll: 0, target: null as HTMLElement | null }

function onRailPointerDown(event: PointerEvent, rail: HTMLElement | null = categoryRail.value) {
  if (!rail || window.innerWidth < 768) return
  dragState.active = true
  dragState.target = rail
  dragState.startX = event.clientX
  dragState.startScroll = rail.scrollLeft
  rail.setPointerCapture?.(event.pointerId)
  rail.classList.add('is-dragging')
}

function onRailPointerMove(event: PointerEvent) {
  if (!dragState.active || !dragState.target) return
  const delta = event.clientX - dragState.startX
  dragState.target.scrollLeft = dragState.startScroll - delta
}

function endRailDrag() {
  dragState.active = false
  dragState.target?.classList.remove('is-dragging')
  dragState.target = null
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
  categoryRail.value?.addEventListener('wheel', onRailWheel, { passive: false })
  problemCategoryRail.value?.addEventListener('wheel', onRailWheel, { passive: false })
  ensureTopRatedLoaded()
})

onBeforeUnmount(() => {
  categoryRail.value?.removeEventListener('wheel', onRailWheel as any)
  problemCategoryRail.value?.removeEventListener('wheel', onRailWheel as any)
})

</script>

<template>
  <div class="min-h-screen home-page-shell">
    <section v-if="categoryCards.length" id="categories" class="mx-auto max-w-6xl px-4 pb-16 pt-8 scroll-mt-24">
      <div class="home-section-panel home-section-panel--categories">
        <div>
          <h2 class="text-2xl font-extrabold tracking-tight text-[rgb(var(--text))] sm:text-4xl">
            {{ t('home.spotlightTitle') }}
          </h2>
          <p class="mt-2 max-w-2xl text-sm text-[rgb(var(--muted))] sm:text-base">
            {{ t('home.spotlightSubtitle') }}
          </p>
        </div>

        <div class="rail-wrap mt-8">
          <button type="button" class="rail-arrow-btn rail-arrow-btn--prev hidden lg:inline-flex" @click="scrollRail('prev', categoryRail)" aria-label="السابق">
            <Icon name="mdi:chevron-right" class="text-xl" />
          </button>
          <button type="button" class="rail-arrow-btn rail-arrow-btn--next hidden lg:inline-flex" @click="scrollRail('next', categoryRail)" aria-label="التالي">
            <Icon name="mdi:chevron-left" class="text-xl" />
          </button>

          <div ref="categoryRail" class="category-unified-rail" @pointerdown="(e) => onRailPointerDown(e, categoryRail)" @pointermove="onRailPointerMove" @pointerup="endRailDrag" @pointercancel="endRailDrag" @pointerleave="endRailDrag">
            <NuxtLink
              v-for="c in categoryCards"
              :key="c.key"
              :to="c.to"
              class="category-mobile-pill"
            >
              <div class="category-mobile-pill__image-wrap" :class="`bg-gradient-to-br ${c.accent}`">
                <img v-if="c.imageUrl" :src="buildAssetUrl(c.imageUrl)" :alt="c.title" class="category-mobile-pill__image" />
                <div v-else class="category-mobile-pill__fallback">{{ c.title?.slice(0,1) }}</div>
              </div>
              <div class="category-mobile-pill__title">{{ c.title }}</div>
            </NuxtLink>
          </div>
        </div>
      </div>
    </section>

    <section class="mx-auto max-w-6xl px-4 pb-20">
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

    <section v-if="problemCards.length" class="mx-auto max-w-6xl px-4 pb-16">
      <div class="home-section-panel home-section-panel--categories">
        <div>
          <h2 class="text-2xl font-extrabold tracking-tight text-[rgb(var(--text))] sm:text-4xl">{{ t('home.problemCategoriesTitle') || 'حلول المشاكل' }}</h2>
          <p class="mt-2 max-w-2xl text-sm text-[rgb(var(--muted))] sm:text-base">{{ t('home.problemCategoriesSubtitle') || 'تسوق حسب المشكلة التي تريد حلها بسرعة.' }}</p>
        </div>
        <div class="rail-wrap mt-8">
          <button type="button" class="rail-arrow-btn rail-arrow-btn--prev hidden lg:inline-flex" @click="scrollRail('prev', problemCategoryRail)" aria-label="السابق">
            <Icon name="mdi:chevron-right" class="text-xl" />
          </button>
          <button type="button" class="rail-arrow-btn rail-arrow-btn--next hidden lg:inline-flex" @click="scrollRail('next', problemCategoryRail)" aria-label="التالي">
            <Icon name="mdi:chevron-left" class="text-xl" />
          </button>
          <div ref="problemCategoryRail" class="category-unified-rail" @pointerdown="(e) => onRailPointerDown(e, problemCategoryRail)" @pointermove="onRailPointerMove" @pointerup="endRailDrag" @pointercancel="endRailDrag" @pointerleave="endRailDrag">
            <NuxtLink v-for="c in problemCards" :key="c.key" :to="c.to" class="category-mobile-pill">
              <div class="category-mobile-pill__image-wrap" :class="`bg-gradient-to-br ${c.accent}`">
                <img v-if="c.imageUrl" :src="buildAssetUrl(c.imageUrl)" :alt="c.title" class="category-mobile-pill__image" />
                <div v-else class="category-mobile-pill__fallback">{{ c.title?.slice(0,1) }}</div>
              </div>
              <div class="category-mobile-pill__title">{{ c.title }}</div>
            </NuxtLink>
          </div>
        </div>
      </div>
    </section>
    <section class="mx-auto max-w-6xl px-4 pb-16 pt-12 sm:pt-14">
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

        <div v-if="displayedFeatured.length" class="mt-10 grid grid-cols-2 gap-3 sm:gap-5 lg:grid-cols-4">
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
</style>

@media (max-width: 1279px){
  .rail-arrow-btn{ display:none !important; }
}
