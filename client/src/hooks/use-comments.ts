import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { commentsApi } from '@/lib/api'

export const commentKeys = {
  all: ['comments'] as const,
  list: (recipeId: string) => ['comments', recipeId] as const,
}

export function useComments(recipeId: string) {
  return useQuery({
    queryKey: commentKeys.list(recipeId),
    queryFn: () => commentsApi.list(recipeId),
    enabled: Boolean(recipeId),
  })
}

export function useAddComment(recipeId: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (body: string) => commentsApi.add(recipeId, body),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: commentKeys.list(recipeId) }),
  })
}

export function useUpdateComment(recipeId: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({ id, body }: { id: string; body: string }) => commentsApi.update(id, body),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: commentKeys.list(recipeId) }),
  })
}

export function useDeleteComment(recipeId: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (id: string) => commentsApi.remove(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: commentKeys.list(recipeId) }),
  })
}
