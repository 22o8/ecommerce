export type QuickPreviewProduct = any

export function useQuickPreview() {
  const open = useState<boolean>('qp_open', () => false)
  const product = useState<QuickPreviewProduct | null>('qp_product', () => null)

  function show(p: QuickPreviewProduct) {
    product.value = p
    open.value = true
  }

  function close() {
    open.value = false
    // keep product for a moment; no animation framework here
    setTimeout(() => {
      if (!open.value) product.value = null
    }, 50)
  }

  return { open, product, show, close }
}
