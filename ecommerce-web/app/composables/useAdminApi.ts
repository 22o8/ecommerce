// app/composables/useAdminApi.ts
import { useApi } from '~/composables/useApi'

export function useAdminApi() {
  const api = useApi()

  return {
    // generic (تحتاجها بالصفحات)
    get: api.get,
    post: api.post,
    put: api.put,
    del: api.del,
    postForm: api.postForm,

    // Admin Products
    listAdminProducts: <T>(query?: any) => api.get<T>('/admin/products', query),
    createAdminProduct: <T>(payload: any) => api.post<T>('/admin/products', payload),
    updateAdminProduct: <T>(id: string, payload: any) => api.put<T>(`/admin/products/${id}`, payload),
    deleteAdminProduct: <T>(id: string) => api.del<T>(`/admin/products/${id}`),

    // ✅ Aliases used by some pages/components (back-compat)
    listProducts: <T>(query?: any) => api.get<T>('/admin/products', query),
    getProduct: <T>(id: string) => api.get<T>(`/admin/products/${id}`),
    createProduct: <T>(payload: any) => api.post<T>('/admin/products', payload),
    updateProduct: <T>(id: string, payload: any) => api.put<T>(`/admin/products/${id}`, payload),
    deleteProduct: <T>(id: string) => api.del<T>(`/admin/products/${id}`),

    // Product Images
    getProductImages: <T>(productId: string) => api.get<T>(`/admin/products/${productId}/images`),
    uploadProductImage: async <T>(productId: string, file: File, alt?: string) => {
      const fd = new FormData()
      // Swagger غالباً يتوقع الحقل بإسم "file"
      fd.append('file', file)
      if (alt) fd.append('alt', alt)
      return await api.postForm<T>(`/admin/products/${productId}/images`, fd)
    },
    deleteProductImage: <T>(productId: string, imageId: string) =>
      api.del<T>(`/admin/products/${productId}/images/${imageId}`),

    reorderProductImages: <T>(productId: string, imageIds: string[]) =>
      api.put<T>(`/admin/products/${productId}/images/reorder`, { imageIds }),

    // ✅ Aliases for images
    listProductImages: <T>(productId: string) => api.get<T>(`/admin/products/${productId}/images`),
    addProductImage: async <T>(productId: string, file: File, alt?: string) => {
      const fd = new FormData()
      fd.append('file', file)
      if (alt) fd.append('alt', alt)
      return await api.postForm<T>(`/admin/products/${productId}/images`, fd)
    },
    removeProductImage: <T>(productId: string, imageId: string) =>
      api.del<T>(`/admin/products/${productId}/images/${imageId}`),
    saveProductImageOrder: <T>(productId: string, imageIds: string[]) =>
      api.put<T>(`/admin/products/${productId}/images/reorder`, { imageIds }),

    // Admin Services
    // Swagger: GET/POST /api/admin/services
    listServices: <T>(query?: any) => api.get<T>('/admin/services', query),
    createService: <T>(payload: any) => api.post<T>('/admin/services', payload),
    updateService: <T>(id: string, payload: any) => api.put<T>(`/admin/services/${id}`, payload),
    deleteService: <T>(id: string) => api.del<T>(`/admin/services/${id}`),
  }
}
