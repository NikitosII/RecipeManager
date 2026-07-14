import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { favoritesApi } from '@/lib/api'
import { recipeKeys } from './use-recipes'

export const favoriteKeys = {
  all: ['favorites'] as const,
  list: (params: { page: number; pageSize: number }) => ['favorites', 'list', params] as const,
}

export function useFavorites(params: { page: number; pageSize: number }) {
  return useQuery({
    queryKey: favoriteKeys.list(params),
    queryFn: () => favoritesApi.list(params),
    placeholderData: (prev) => prev,
  })
}
export function useToggleFavorite() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({ recipeId, isFavorite }: { recipeId: string; isFavorite: boolean }) =>
      isFavorite ? favoritesApi.remove(recipeId) : favoritesApi.add(recipeId),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: recipeKeys.all })
      void queryClient.invalidateQueries({ queryKey: favoriteKeys.all })
    },
  })
}
