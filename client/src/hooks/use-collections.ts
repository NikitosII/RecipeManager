import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { collectionsApi } from '@/lib/api'

export const collectionKeys = {
  all: ['collections'] as const,
  detail: (id: string) => ['collections', 'detail', id] as const,
}

export function useCollections() {
  return useQuery({
    queryKey: collectionKeys.all,
    queryFn: () => collectionsApi.list(),
  })
}

export function useCollection(id: string | null) {
  return useQuery({
    queryKey: collectionKeys.detail(id ?? ''),
    queryFn: () => collectionsApi.getById(id as string),
    enabled: Boolean(id),
  })
}

export function useCreateCollection() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (body: { name: string; description: string | null }) => collectionsApi.create(body),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: collectionKeys.all }),
  })
}

export function useUpdateCollection(id: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (body: { name: string; description: string | null }) => collectionsApi.update(id, body),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: collectionKeys.all }),
  })
}

export function useDeleteCollection() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (id: string) => collectionsApi.delete(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: collectionKeys.all }),
  })
}

export function useAddRecipeToCollection() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({ collectionId, recipeId }: { collectionId: string; recipeId: string }) =>
      collectionsApi.addRecipe(collectionId, recipeId),
    // Invalidating the root key also refreshes any open collection detail (prefix match).
    onSuccess: () => queryClient.invalidateQueries({ queryKey: collectionKeys.all }),
  })
}

export function useRemoveRecipeFromCollection() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({ collectionId, recipeId }: { collectionId: string; recipeId: string }) =>
      collectionsApi.removeRecipe(collectionId, recipeId),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: collectionKeys.all }),
  })
}
