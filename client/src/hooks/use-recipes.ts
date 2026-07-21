import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { ingredientsApi, recipesApi } from '@/lib/api'
import type { RecipeListParams, UpdateNutritionPayload, UpdateRecipePayload } from '@/lib/api'

export const recipeKeys = {
  all: ['recipes'] as const,
  list: (params: RecipeListParams) => ['recipes', 'list', params] as const,
  detail: (id: string) => ['recipes', 'detail', id] as const,
}

export function useRecipes(params: RecipeListParams) {
  return useQuery({
    queryKey: recipeKeys.list(params),
    queryFn: () => recipesApi.list(params),
    placeholderData: (prev) => prev, 
  })
}

export function useRecipe(id: string | null) {
  return useQuery({
    queryKey: recipeKeys.detail(id ?? ''),
    queryFn: () => recipesApi.getById(id as string),
    enabled: Boolean(id),
  })
}

export function useDeleteRecipe() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (id: string) => recipesApi.delete(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: recipeKeys.all }),
  })
}

export function useUpdateRecipe(id: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (body: UpdateRecipePayload) => recipesApi.update(id, body),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: recipeKeys.detail(id) })
      void queryClient.invalidateQueries({ queryKey: recipeKeys.all })
    },
  })
}

export function useUpdateNutrition(id: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (body: UpdateNutritionPayload) => recipesApi.updateNutrition(id, body),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: recipeKeys.detail(id) }),
  })
}

// Re-fetches nutrition for the given ingredients, then refreshes the recipe so the
// per-serving figures recalculate. Used to backfill ingredients that have no data yet.
export function useRefreshNutrition(recipeId: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (ingredientIds: string[]) =>
      Promise.all(ingredientIds.map((id) => ingredientsApi.refreshNutrition(id))),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: recipeKeys.detail(recipeId) }),
  })
}
