import { defineStore } from 'pinia'

export type AppearanceAd = {
  id: string
  title?: string
  subtitle?: string
  imageUrl: string
  linkUrl?: string
  isActive: boolean
  position?: 'hero' | 'popup'
  sortOrder?: number
}

export type AppearanceState = {
  version: number
  updatedAt?: string
  // allow multiple themes enabled
  themes: string[]
  // allow multiple effects enabled
  effects: Record<string, boolean>
  ads: AppearanceAd[]
}

const DEFAULT: AppearanceState = {
  version: 1,
  themes: [],
  effects: {
    snow: false,
    ramadan: false,
    eid: false,
    christmas: false,
    valentines: false,
    blackFriday: false,
  },
  ads: [],
}

function normalizeUrl(v: any): string {
  if (!v) return ''
  if (typeof v === 'string') return v
  if (typeof v?.url === 'string') return v.url
  if (typeof v?.Url === 'string') return v.Url
  return ''
}

export const useAppearanceStore = defineStore('appearance', {
  state: () => ({
    loaded: false as boolean,
    data: DEFAULT as AppearanceState,
  }),
  actions: {
    mapFromApi(res: any) {
      const enabledThemes: string[] = res?.enabledThemes ?? res?.EnabledThemes ?? res?.themes ?? []
      const enabledEffects: string[] = res?.enabledEffects ?? res?.EnabledEffects ?? []
      const ads: any[] = res?.ads ?? res?.Ads ?? []

      const effects = { ...DEFAULT.effects }
      for (const k of Object.keys(effects)) {
        ;(effects as any)[k] = enabledEffects.includes(k)
      }

      this.data = {
        ...DEFAULT,
        version: res?.version ?? 1,
        updatedAt: res?.updatedAt ?? res?.UpdatedAt,
        themes: enabledThemes,
        effects,
        ads: ads.map((a: any) => ({
          id: a.id ?? a.Id,
          title: a.title ?? a.Title,
          subtitle: a.subtitle ?? a.Subtitle,
          imageUrl: normalizeUrl(a.imageUrl ?? a.ImageUrl),
          linkUrl: a.linkUrl ?? a.LinkUrl,
          isActive: a.isEnabled ?? a.IsEnabled ?? a.isActive ?? a.IsActive ?? true,
          sortOrder: a.sortOrder ?? a.SortOrder ?? 0,
        })),
      }
    },

    async refresh() {
      try {
        const res = await $fetch<any>('/api/bff/appearance', { timeout: 8000 })
        this.mapFromApi(res)
        this.loaded = true
      } catch {
        this.data = DEFAULT
        this.loaded = true
      }
    },

    async fetchAdminAppearance() {
      try {
        const res = await $fetch<any>('/api/bff/admin/appearance', { timeout: 8000 })
        this.mapFromApi(res)
        this.loaded = true
      } catch {
        this.loaded = true
      }
    },
  },
})
