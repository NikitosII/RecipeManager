import axios from 'axios'
import type { AxiosError, InternalAxiosRequestConfig } from 'axios'
import { config } from '@/config'
import { tokenStorage } from '@/lib/token-storage'
import type { AuthResponse } from '@/types/api'

export const apiClient = axios.create({
  baseURL: config.apiUrl,
  headers: { 'Content-Type': 'application/json' },
})

apiClient.interceptors.request.use((cfg) => {
  const token = tokenStorage.getAccess()
  if (token) cfg.headers.Authorization = `Bearer ${token}`
  return cfg
})

// -- Refresh-token rotation -- //

let refreshPromise: Promise<string | null> | null = null

async function refreshAccessToken(): Promise<string | null> {
  const refreshToken = tokenStorage.getRefresh()
  if (!refreshToken) return null

  try {
    const { data } = await axios.post<AuthResponse>(`${config.apiUrl}/auth/refresh`, {
      refreshToken,
    })
    tokenStorage.setSession(data)
    return data.accessToken
  } catch {
    tokenStorage.clear()
    return null
  }
}

apiClient.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    const original = error.config as (InternalAxiosRequestConfig & { _retry?: boolean }) | undefined

    if (error.response?.status === 401 && original && !original._retry && tokenStorage.getRefresh()) {
      original._retry = true

      refreshPromise ??= refreshAccessToken().finally(() => {
        refreshPromise = null
      })
      const newToken = await refreshPromise

      if (newToken) {
        original.headers.Authorization = `Bearer ${newToken}`
        return apiClient(original)
      }

      window.dispatchEvent(new Event('auth:logout'))
    }

    return Promise.reject(error)
  },
)
