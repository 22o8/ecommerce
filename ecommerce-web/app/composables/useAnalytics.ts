type AnalyticsItem = {
  item_id?: string | number
  item_name?: string
  item_brand?: string
  item_category?: string
  price?: number
  quantity?: number
}

function gtagSafe(eventName: string, payload: Record<string, any> = {}) {
  if (!import.meta.client) return
  const w = window as any
  if (typeof w.gtag !== 'function') return
  w.gtag('event', eventName, {
    currency: 'IQD',
    market: 'Iraq',
    ...payload,
  })
}

function itemFromProduct(product: any, quantity = 1): AnalyticsItem {
  return {
    item_id: product?.id || product?.Id || product?.slug || product?.Slug,
    item_name: product?.title || product?.Title || product?.name || product?.Name,
    item_brand: product?.brand || product?.Brand || 'DR SEOUL BEAUTY',
    item_category: product?.category || product?.Category || product?.subCategory || product?.SubCategory || 'Korean Skincare Iraq',
    price: Number(product?.finalPriceIqd ?? product?.priceIqd ?? product?.PriceIqd ?? 0),
    quantity,
  }
}

export function useAnalytics() {
  return {
    event: gtagSafe,
    viewItem(product: any) {
      const item = itemFromProduct(product)
      gtagSafe('view_item', {
        value: Number(item.price || 0),
        items: [item],
      })
    },
    addToCart(product: any, quantity = 1) {
      const item = itemFromProduct(product, quantity)
      gtagSafe('add_to_cart', {
        value: Number(item.price || 0) * quantity,
        items: [item],
      })
    },
    beginCheckout(items: any[] = [], value = 0) {
      gtagSafe('begin_checkout', {
        value,
        items: items.map((x: any) => itemFromProduct(x.product || x, Number(x.quantity || 1))),
      })
    },
    purchase(order: any) {
      const items = Array.isArray(order?.items) ? order.items : []
      gtagSafe('purchase', {
        transaction_id: order?.id || order?.Id || String(Date.now()),
        value: Number(order?.totalIqd ?? order?.TotalIqd ?? 0),
        items: items.map((x: any) => itemFromProduct(x.product || x, Number(x.quantity || 1))),
      })
    },
    search(term: string) {
      if (!term?.trim()) return
      gtagSafe('search', { search_term: term.trim() })
    },
    selectCity(city: string) {
      gtagSafe('select_content', { content_type: 'iraq_city_landing', item_id: city })
    },
  }
}
