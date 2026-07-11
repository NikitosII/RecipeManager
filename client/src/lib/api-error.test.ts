import { AxiosError, AxiosHeaders } from 'axios'
import { describe, expect, it } from 'vitest'
import { getApiErrorMessage } from './api-error'

function axiosErrorWith(data: unknown): AxiosError {
  const error = new AxiosError('Request failed', 'ERR_BAD_REQUEST')
  error.response = {
    data,
    status: 400,
    statusText: 'Bad Request',
    headers: {},
    config: { headers: new AxiosHeaders() },
  }
  return error
}

describe('getApiErrorMessage', () => {
  it('prefers the first validation error', () => {
    const message = getApiErrorMessage(
      axiosErrorWith({ title: 'Bad', detail: 'nope', errors: ['Name is required.', 'Second'] }),
    )
    expect(message).toBe('Name is required.')
  })

  it('falls back to detail then title', () => {
    expect(getApiErrorMessage(axiosErrorWith({ detail: 'Detailed reason' }))).toBe('Detailed reason')
    expect(getApiErrorMessage(axiosErrorWith({ title: 'Just a title' }))).toBe('Just a title')
  })

  it('uses the provided fallback for non-API errors', () => {
    expect(getApiErrorMessage('some string', 'fallback!')).toBe('fallback!')
  })

  it('reads the message from a plain Error', () => {
    expect(getApiErrorMessage(new Error('boom'))).toBe('boom')
  })
})
