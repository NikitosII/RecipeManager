const API_URL = import.meta.env.VITE_API_URL ?? 'http://localhost:8080/api/v1'
const API_ORIGIN = (() => {
  try {
    return new URL(API_URL).origin
  } catch {
    return ''
  }
})()

export const config = {
  apiUrl: API_URL,
  apiOrigin: API_ORIGIN,
} as const

export function resolveMediaUrl(path: string | null | undefined): string | undefined {
  if (!path) return undefined
  if (/^https?:\/\//i.test(path) || path.startsWith('data:')) return path
  return `${API_ORIGIN}${path}`
}
