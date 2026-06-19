export type ProductImage = {
  id: string
  url: string
  alt?: string | null
  sortOrder?: number | null
}

export type AdminProduct = {
  id: string
  title: string
  slug: string
  description: string
  priceUsd: number
  rating?: number | null
  isPublished: boolean
  createdAt: string
  images?: ProductImage[]
  coverImageUrl?: string | null
}
