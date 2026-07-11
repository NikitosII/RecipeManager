import axios, { AxiosError } from 'axios'
import type { AxiosAdapter } from 'axios'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { apiClient } from './api-client'
import { tokenStorage } from './token-storage'
import type { AuthResponse } from '@/types/api'

const refreshedSession: AuthResponse = {
  accessToken: 'new-access',
  accessTokenExpiry: new Date().toISOString(),
  refreshToken: 'new-refresh',
  userId: 'u1',
  email: 'a@b.c',
  firstName: 'A',
  lastName: 'B',
}

function unauthorized(config: AxiosError['config']): AxiosError {
  return new AxiosError('Unauthorized', 'ERR_BAD_REQUEST', config, null, {
    status: 401,
    statusText: 'Unauthorized',
    data: {},
    headers: {},
    config: config!,
  })
}

describe('apiClient refresh interceptor', () => {
  beforeEach(() => {
    localStorage.clear()
    vi.restoreAllMocks()
  })

  afterEach(() => {
    apiClient.defaults.adapter = undefined
  })

  it('refreshes on 401, then replays the original request with the new token', async () => {
    tokenStorage.setSession({ ...refreshedSession, accessToken: 'old-access', refreshToken: 'old-refresh' })
    const postSpy = vi.spyOn(axios, 'post').mockResolvedValue({ data: refreshedSession })

    let retryAuthHeader: unknown
    const adapter: AxiosAdapter = async (cfg) => {
      const retried = (cfg as { _retry?: boolean })._retry === true
      if (!retried) throw unauthorized(cfg)
      retryAuthHeader = (cfg.headers as Record<string, unknown>).Authorization
      return { data: { ok: true }, status: 200, statusText: 'OK', headers: {}, config: cfg }
    }
    apiClient.defaults.adapter = adapter

    const res = await apiClient.get('/protected')

    expect(res.data).toEqual({ ok: true })
    expect(postSpy).toHaveBeenCalledOnce()
    expect(retryAuthHeader).toBe('Bearer new-access')
    expect(tokenStorage.getAccess()).toBe('new-access')
  })

  it('dispatches auth:logout and clears storage when the refresh fails', async () => {
    tokenStorage.setSession({ ...refreshedSession, accessToken: 'old', refreshToken: 'old-refresh' })
    vi.spyOn(axios, 'post').mockRejectedValue(new Error('refresh rejected'))

    apiClient.defaults.adapter = async (cfg) => {
      throw unauthorized(cfg)
    }

    const onLogout = vi.fn()
    window.addEventListener('auth:logout', onLogout)

    await expect(apiClient.get('/protected')).rejects.toBeInstanceOf(AxiosError)

    expect(onLogout).toHaveBeenCalledOnce()
    expect(tokenStorage.getRefresh()).toBeNull()
    window.removeEventListener('auth:logout', onLogout)
  })
})
