import { isAxiosError } from 'axios'

interface ProblemDetails {
  title?: string
  detail?: string
  errors?: string[]
}

/**
 * Extracts a human-readable message from an RFC 9457 ProblemDetails response
 * (the shape produced by the API's GlobalExceptionHandler).
 */
export function getApiErrorMessage(error: unknown, fallback = 'Something went wrong. Please try again.'): string {
  if (isAxiosError(error)) {
    const data = error.response?.data as ProblemDetails | undefined
    if (data) {
      if (Array.isArray(data.errors) && data.errors.length > 0) return data.errors[0]
      if (data.detail) return data.detail
      if (data.title) return data.title
    }
    if (error.message) return error.message
  }
  if (error instanceof Error && error.message) return error.message
  return fallback
}
