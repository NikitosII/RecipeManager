import { useQuery } from '@tanstack/react-query'
import { categoriesApi, ingredientsApi } from '@/lib/api'

export function useCategories() {
  return useQuery({
    queryKey: ['categories'],
    queryFn: () => categoriesApi.list(),
    staleTime: 1000 * 60 * 10, // categories rarely change
  })
}

export function useIngredients() {
  return useQuery({
    queryKey: ['ingredients'],
    queryFn: () => ingredientsApi.list(),
    staleTime: 1000 * 60 * 10, // ingredients rarely change
  })
}
