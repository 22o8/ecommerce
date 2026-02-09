// app/composables/useAdminApi.ts
import { useApi } from '~/composables/useApi'

export function useAdminApi() {
  const api = useApi()

  return {
    getDashboardStats: <T>() => api.get<T>('/admin/dashboard/stats'),

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
      // Swagger: field name "images" (array). نرسل ملف واحد ضمنها.
      fd.append('images', file)
      if (alt) fd.append('alt', alt)
      return await api.postForm<T>(`/admin/products/${productId}/images`, fd)
    },

    /**
     * Upload multiple images sequentially (stable on serverless + avoids timeouts).
     */
    uploadProductImages: async <T>(productId: string, files: File[], altPrefix: string = '') => {
      const results: T[] = []
      for (let i = 0; i < files.length; i++) {
        const f = files[i]
        const alt = altPrefix ? `${altPrefix} ${i + 1}` : ''
        // reuse single-upload endpoint
        const r = await (api.postForm<T>(`/admin/products/${productId}/images`, (() => {
          const fd = new FormData()
          fd.append('images', f)
          if (alt) fd.append('alt', alt)
          return fd
        })()))
        results.push(r)
      }
      return results
    },
    deleteProductImage: <T>(productId: string, imageId: string) =>
      api.del<T>(`/admin/products/${productId}/images/${imageId}`),

    reorderProductImages: <T>(productId: string, imageIds: string[]) =>
      api.put<T>(`/admin/products/${productId}/images/reorder`, { imageIds }),

    // ✅ Aliases for images
    listProductImages: <T>(productId: string) => api.get<T>(`/admin/products/${productId}/images`),
    addProductImage: async <T>(productId: string, file: File, alt?: string) => {
      const fd = new FormData()
      fd.append('images', file)
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

    // Admin Brands
    listBrands: <T>(query?: any) => api.get<T>('/admin/brands', query),
    getBrand: <T>(id: string) => api.get<T>(`/admin/brands/${id}`),
    createBrand: <T>(payload: any) => api.post<T>('/admin/brands', payload),
    updateBrand: <T>(id: string, payload: any) => api.put<T>(`/admin/brands/${id}`, payload),
    deleteBrand: <T>(id: string) => api.del<T>(`/admin/brands/${id}`),
    uploadBrandLogo: async <T>(id: string, file: File) => {
      const fd = new FormData()
      // Swagger: field name "logo"
      fd.append('logo', file)
      return await api.postForm<T>(`/admin/brands/${id}/logo`, fd)
    },
  }
}
