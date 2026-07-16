import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { usersApi } from '@/lib/api'

export function useProfile(enabled = true) {
  return useQuery({
    queryKey: ['profile'],
    queryFn: () => usersApi.me(),
    enabled,
    staleTime: 1000 * 60 * 5,
  })
}

export function useUploadAvatar() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (file: File) => usersApi.uploadAvatar(file),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['profile'] })
    },
  })
}
