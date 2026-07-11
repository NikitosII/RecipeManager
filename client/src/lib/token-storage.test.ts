import { beforeEach, describe, expect, it } from 'vitest'
import { tokenStorage } from './token-storage'
import type { AuthResponse } from '@/types/api'

const auth: AuthResponse = {
  accessToken: 'access-123',
  accessTokenExpiry: new Date().toISOString(),
  refreshToken: 'refresh-456',
  userId: 'user-1',
  email: 'cook@example.com',
  firstName: 'Cook',
  lastName: 'Book',
}

describe('tokenStorage', () => {
  beforeEach(() => localStorage.clear())

  it('round-trips a session', () => {
    tokenStorage.setSession(auth)

    expect(tokenStorage.getAccess()).toBe('access-123')
    expect(tokenStorage.getRefresh()).toBe('refresh-456')
    expect(tokenStorage.getUser()).toEqual({
      userId: 'user-1',
      email: 'cook@example.com',
      firstName: 'Cook',
      lastName: 'Book',
    })
  })

  it('clear() removes everything', () => {
    tokenStorage.setSession(auth)
    tokenStorage.clear()

    expect(tokenStorage.getAccess()).toBeNull()
    expect(tokenStorage.getRefresh()).toBeNull()
    expect(tokenStorage.getUser()).toBeNull()
  })

  it('returns null for corrupt user JSON instead of throwing', () => {
    localStorage.setItem('mc_user', '{not valid json')

    expect(tokenStorage.getUser()).toBeNull()
  })
})
