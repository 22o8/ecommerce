import { defineEventHandler, readBody, getRequestURL, getRequestHeaders, setResponseStatus, setHeader, sendNoContent } from "h3";

/**
 * BFF proxy: forwards /api/bff/** to backend API.
 * - Works on Vercel/Nitro.
 * - Avoids throwing on 204 and empty bodies.
 * - Adds Authorization from cookie `token` if missing.
 */

const hopByHop = new Set([
  "connection",
  "keep-alive",
  "proxy-authenticate",
  "proxy-authorization",
  "te",
  "trailers",
  "transfer-encoding",
  "upgrade",
  "host",
  "content-length",
]);

function pickForwardHeaders(headers: Record<string, string | string[] | undefined>) {
  const out: Record<string, string> = {};
  for (const [k, v] of Object.entries(headers)) {
    if (!v) continue;
    const key = k.toLowerCase();
    if (hopByHop.has(key)) continue;
    if (Array.isArray(v)) out[key] = v.join(", ");
    else out[key] = String(v);
  }
  return out;
}

function parseCookie(cookieHeader?: string) {
  const map: Record<string, string> = {};
  if (!cookieHeader) return map;
  const parts = cookieHeader.split(";");
  for (const part of parts) {
    const [k, ...rest] = part.trim().split("=");
    if (!k) continue;
    map[k] = rest.join("=") ?? "";
  }
  return map;
}

function isJsonResponse(ct?: string | null) {
  return !!ct && ct.toLowerCase().includes("application/json");
}

export default defineEventHandler(async (event) => {
  const runtimeConfig = useRuntimeConfig();

  // Backend base (must end without trailing slash)
  const base = (runtimeConfig.public?.apiBase as string | undefined)
    || (runtimeConfig.apiBase as string | undefined)
    || process.env.API_BASE_URL
    || process.env.NUXT_PUBLIC_API_BASE_URL
    || "http://localhost:5010";

  // Incoming path after /api/bff/
  const url = getRequestURL(event);
  const fullPath = url.pathname || "/";
  const bffPrefix = "/api/bff/";
  const rest = fullPath.startsWith(bffPrefix) ? fullPath.slice(bffPrefix.length) : "";
  // If someone calls exactly /api/bff, just 404
  if (!rest) {
    setResponseStatus(event, 404);
    return { error: "BFF path missing" };
  }

  // Build backend URL
  const target = new URL(rest.replace(/^\//, "") , base.endsWith("/") ? base : base + "/");
  // Keep query string
  target.search = url.search;

  const incomingHeaders = getRequestHeaders(event);
  const forwardHeaders = pickForwardHeaders(incomingHeaders);

  // Ensure accept json by default
  if (!forwardHeaders["accept"]) forwardHeaders["accept"] = "application/json";

  // Attach Authorization from cookie token if not provided
  const cookies = parseCookie(incomingHeaders.cookie as string | undefined);
  if (!forwardHeaders["authorization"] && cookies.token) {
    forwardHeaders["authorization"] = `Bearer ${cookies.token}`;
  }

  // Read body for non-GET/HEAD
  const method = (event.node.req.method || "GET").toUpperCase();
  let body: any = undefined;
  if (method !== "GET" && method !== "HEAD") {
    const ct = String(incomingHeaders["content-type"] || "");
    if (ct.includes("application/json")) {
      body = await readBody(event);
    } else {
      // For formdata/others, readBody still works in h3 (string/object)
      body = await readBody(event);
    }
  }

  // Optional legacy fallback: if request is /Checkout/cart and payload items have brand missing,
  // don't mutate unless explicitly needed. (kept minimal)
  try {
    const res = await $fetch.raw(target.toString(), {
      method,
      headers: forwardHeaders,
      body: body as any,
      // Important on server: do NOT throw on non-2xx
      ignoreResponseError: true,
    });

    // Pass through status code
    if (res.status === 204) {
      // No content
      setResponseStatus(event, 204);
      return sendNoContent(event);
    }

    setResponseStatus(event, res.status);

    // Pass through selected headers
    const passthrough = ["content-type", "cache-control"];
    for (const h of passthrough) {
      const v = res.headers.get(h);
      if (v) setHeader(event, h, v);
    }

    // Pass Set-Cookie if backend sets any
    const setCookie = res.headers.get("set-cookie");
    if (setCookie) {
      setHeader(event, "set-cookie", setCookie);
    }

    // Read body safely
    const ct = res.headers.get("content-type");
    if (isJsonResponse(ct)) {
      // res._data is already parsed by $fetch when content-type json
      return res._data ?? {};
    }

    // Non-JSON: return as text/buffer
    // $fetch.raw provides _data as string/Buffer depending on response
    return res._data ?? null;

  } catch (err: any) {
    // Network / DNS / TLS errors => this is where "Failed to fetch" comes from in browser.
    setResponseStatus(event, 502);
    return {
      error: "BFF_PROXY_ERROR",
      message: err?.message || String(err),
      target: target.toString(),
    };
  }
});
