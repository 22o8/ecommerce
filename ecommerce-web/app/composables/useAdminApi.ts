// ecommerce-web/app/composables/useAdminApi.ts

export type AdminPaged<T> = {
  items: T[]
  page: number
  pageSize: number
  total: number
}

export function useAdminApi() {
  const api = useApi()

  // =========================
  // Brands
  // =========================
  const listBrands = <T>(query?: any) => api.get<T>('/admin/brands', query)
  const getBrand = <T>(id: string) => api.get<T>(`/admin/brands/${id}`)
  const createBrand = <T>(body: any) => api.post<T>('/admin/brands', body)
  const updateBrand = <T>(id: string, body: any) => api.put<T>(`/admin/brands/${id}` , body)
  const deleteBrand = <T>(id: string) => api.del<T>(`/admin/brands/${id}`)

  // =========================
  // Users
  // =========================
  const listUsers = <T>(query?: any) => api.get<T>('/admin/users', query)
  const getUser = <T>(id: string) => api.get<T>(`/admin/users/${id}`)
  const createUser = <T>(body: any) => api.post<T>('/admin/users', body)
  const updateUser = <T>(id: string, body: any) => api.put<T>(`/admin/users/${id}` , body)
  const deleteUser = <T>(id: string) => api.del<T>(`/admin/users/${id}`)

  // =========================
  // Orders
  // =========================
  const listOrders = <T>(query?: any) => api.get<T>('/admin/orders', query)
  const getOrder = <T>(id: string) => api.get<T>(`/admin/orders/${id}`)
  const updateOrderStatus = <T>(id: string, body: any) => api.patch<T>(`/admin/orders/${id}/status`, body)

  // =========================
  // Products
  // =========================
  const listProducts = <T>(query?: any) => api.get<T>('/admin/products', query)
  const getProduct = <T>(id: string) => api.get<T>(`/admin/products/${id}`)
  const createProduct = <T>(body: any) => api.post<T>('/admin/products', body)
  const updateProduct = <T>(id: string, body: any) => api.put<T>(`/admin/products/${id}` , body)
  const deleteProduct = <T>(id: string) => api.del<T>(`/admin/products/${id}`)

  // Product images (assets)
  const listProductImages = <T>(productId: string) => api.get<T>(`/admin/products/${productId}/images`)

  const uploadProductImages = async <T>(productId: string, files: File[]) => {
    const fd = new FormData()
    files.forEach(f => fd.append('files', f))
    return await api.postForm<T>(`/admin/products/${productId}/images`, fd)
  }

  const deleteProductImage = <T>(productId: string, imageId: string) =>
    api.del<T>(`/admin/products/${productId}/images/${imageId}`)

  // ✅ Legacy names used by some older pages
  const getAdminProduct = getProduct
  const deleteAdminProduct = deleteProduct
  const listProductAssets = listProductImages
  const uploadProductAssets = uploadProductImages
  const deleteProductAsset = deleteProductImage

  return {
    listBrands,
    getBrand,
    createBrand,
    updateBrand,
    deleteBrand,

    listUsers,
    getUser,
    createUser,
    updateUser,
    deleteUser,

    listOrders,
    getOrder,
    updateOrderStatus,

    listProducts,
    getProduct,
    createProduct,
    updateProduct,
    deleteProduct,

    listProductImages,
    uploadProductImages,
    deleteProductImage,

    // legacy
    getAdminProduct,
    deleteAdminProduct,
    listProductAssets,
    uploadProductAssets,
    deleteProductAsset,
  }
}
