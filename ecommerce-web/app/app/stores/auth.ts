/**
 * Compatibility shim:
 * Some pages import from "~/app/stores/auth" which resolves to /app/app/stores/auth
 * when using Nuxt appDir = "app".
 * This file re-exports the real store from /app/stores/auth.
 */
export * from '../../stores/auth'
