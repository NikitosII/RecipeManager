import { useMutation, useQueryClient } from '@tanstack/react-query'
import { recipesApi } from '@/lib/api'
import { recipeKeys } from './use-recipes'
import { favoriteKeys } from './use-favorites'
import { collectionKeys } from './use-collections'

function useRatingInvalidation() {
  const queryClient = useQueryClient()
  return () => {
    void queryClient.invalidateQueries({ queryKey: recipeKeys.all })
    void queryClient.invalidateQueries({ queryKey: favoriteKeys.all })
    void queryClient.invalidateQueries({ queryKey: collectionKeys.all })
  }
}

export function useRateRecipe() {
  const invalidate = useRatingInvalidation()
  return useMutation({
    mutationFn: ({ recipeId, value }: { recipeId: string; value: number }) => recipesApi.rate(recipeId, value),
    onSuccess: invalidate,
  })
}

export function useRemoveRating() {
  const invalidate = useRatingInvalidation()
  return useMutation({
    mutationFn: (recipeId: string) => recipesApi.removeRating(recipeId),
    onSuccess: invalidate,
  })
}
