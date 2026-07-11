import { useQuery } from '@tanstack/react-query'
import { recipesApi } from '@/lib/api'
import type { RecipeListParams } from '@/lib/api'

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
