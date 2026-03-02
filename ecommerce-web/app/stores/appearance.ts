import { defineStore } from 'pinia'

export type AppearanceAd = {
  id: string
  title?: string
  imageUrl: string
  linkUrl?: string
  isActive: boolean
  position?: 'hero' | 'popup'
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
  },
  ads: [],
}

export const useAppearanceStore = defineStore('appearance', {
  state: () => ({
    loaded: false as boolean,
    data: DEFAULT as AppearanceState,
  }),
  actions: {
    async refresh() {
      try {
        const res = await $fetch<AppearanceState>('/api/appearance', { timeout: 8000 })
        this.data = { ...DEFAULT, ...(res as any) }
        this.loaded = true
      } catch {
        this.data = DEFAULT
        this.loaded = true
      }
    },
  },
})
