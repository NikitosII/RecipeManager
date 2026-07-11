import { create } from 'zustand'
import { tokenStorage } from '@/lib/token-storage'
import { authApi } from '@/lib/api'
import type { AuthResponse, AuthUser } from '@/types/api'

interface AuthState {
  user: AuthUser | null
  isAuthenticated: boolean
  setSession: (auth: AuthResponse) => void
  logout: () => Promise<void>
  clear: () => void
}

export const useAuthStore = create<AuthState>((set) => ({
  user: tokenStorage.getUser(),
  isAuthenticated: Boolean(tokenStorage.getAccess()),

  setSession: (auth) => {
    tokenStorage.setSession(auth)
    set({
      user: { userId: auth.userId, email: auth.email, firstName: auth.firstName, lastName: auth.lastName },
      isAuthenticated: true,
    })
  },

  logout: async () => {
    const refreshToken = tokenStorage.getRefresh()
    if (refreshToken) {
      try {
        await authApi.logout(refreshToken)
      } catch {
        /* ignore */
      }
    }
    tokenStorage.clear()
    set({ user: null, isAuthenticated: false })
  },

  clear: () => {
    tokenStorage.clear()
    set({ user: null, isAuthenticated: false })
  },
}))

// The axios client fires this when a refresh fails — drop the session app-wide.
if (typeof window !== 'undefined') {
  window.addEventListener('auth:logout', () => useAuthStore.getState().clear())
}
