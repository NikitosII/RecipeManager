import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { recipesApi } from '@/lib/api'
import type { RecipeListParams, UpdateRecipePayload } from '@/lib/api'

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
