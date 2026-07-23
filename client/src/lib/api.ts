import { apiClient } from '@/lib/api-client'
import type {
  AuthResponse,
  Category,
  Collection,
  CollectionDetail,
  Comment,
  Ingredient,
  MeasurementUnit,
  Nutrition,
  Paginated,
  RecipeDetail,
  RecipeStep,
  RecipeSummary,
  UserProfile,
} from '@/types/api'

// -- Auth -- //

export const authApi = {
  register: (body: { firstName: string; lastName: string; email: string; password: string }) =>
    apiClient.post<AuthResponse>('/auth/register', body).then((r) => r.data),

  login: (body: { email: string; password: string }) =>
    apiClient.post<AuthResponse>('/auth/login', body).then((r) => r.data),

  logout: (refreshToken: string) => apiClient.post('/auth/logout', { refreshToken }).then(() => undefined),
}

// -- Users -- //

export const usersApi = {
  me: () => apiClient.get<UserProfile>('/users/me').then((r) => r.data),

  uploadAvatar: (file: File) => {
    const form = new FormData()
    form.append('file', file)
    return apiClient
      .post<{ avatarUrl: string }>('/users/me/avatar', form, {
        headers: { 'Content-Type': 'multipart/form-data' },
      })
      .then((r) => r.data.avatarUrl)
  },
}

// -- Categories & ingredients -- //

export const categoriesApi = {
  list: () => apiClient.get<Category[]>('/categories').then((r) => r.data),
}

export const ingredientsApi = {
  list: () => apiClient.get<Ingredient[]>('/ingredients').then((r) => r.data),

  // Re-fetches an ingredient's per-100g macros from the nutrition source (backfill).
  refreshNutrition: (id: string) =>
    apiClient.post(`/ingredients/${id}/nutrition/refresh`).then((r) => r.data),
}

// -- Recipes -- //

export interface RecipeListParams {
  page?: number
  pageSize?: number
  search?: string
  categoryId?: string
  difficulty?: number
  maxPrepTime?: number
  maxCookTime?: number
  minServings?: number
  ingredientIds?: string[]
  sortBy?: number
  sortDescending?: boolean
}

export interface CreateRecipePayload {
  title: string
  description: string | null
  difficultyLevel: number
  prepTimeMinutes: number
  cookTimeMinutes: number
  servings: number
  categoryId: string
}

export interface UpdateRecipePayload {
  title: string
  description: string | null
  difficultyLevel: number
  prepTimeMinutes: number
  cookTimeMinutes: number
  servings: number
}

// Mode 1 = automatic (macros are ignored); mode 2 = manual (values are required).
export interface UpdateNutritionPayload {
  mode: number
  calories?: number | null
  protein?: number | null
  fat?: number | null
  carbohydrates?: number | null
  fiber?: number | null
}

export interface CreateRecipeStepInput {
  description: string
}

export interface CreateRecipeIngredientInput {
  name: string
  quantity: number
  unit: MeasurementUnit
}

export const recipesApi = {
  list: (params: RecipeListParams) =>
    apiClient
      .get<Paginated<RecipeSummary>>('/recipes', { params, paramsSerializer: { indexes: null } })
      .then((r) => r.data),

  getById: (id: string) => apiClient.get<RecipeDetail>(`/recipes/${id}`).then((r) => r.data),

  create: (body: CreateRecipePayload) =>
    apiClient.post<{ id: string }>('/recipes', body).then((r) => r.data.id),

  update: (id: string, body: UpdateRecipePayload) =>
    apiClient.put(`/recipes/${id}`, body).then(() => undefined),

  delete: (id: string) => apiClient.delete(`/recipes/${id}`).then(() => undefined),

  rate: (id: string, value: number) =>
    apiClient.put(`/recipes/${id}/rating`, { value }).then(() => undefined),

  removeRating: (id: string) => apiClient.delete(`/recipes/${id}/rating`).then(() => undefined),

  updateNutrition: (id: string, body: UpdateNutritionPayload) =>
    apiClient.put<Nutrition>(`/recipes/${id}/nutrition`, body).then((r) => r.data),

  appendStep: (id: string, description: string) =>
    apiClient.post<RecipeStep>(`/recipes/${id}/steps`, { description }).then((r) => r.data),

  addIngredient: (id: string, body: CreateRecipeIngredientInput) =>
    apiClient.post(`/recipes/${id}/ingredients`, body).then((r) => r.data),

  uploadImage: (id: string, file: File) => {
    const form = new FormData()
    form.append('file', file)
    return apiClient
      .post<{ imageUrl: string }>(`/recipes/${id}/image`, form, {
        headers: { 'Content-Type': 'multipart/form-data' },
      })
      .then((r) => r.data.imageUrl)
  },
}

// -- Favourites -- //

export const favoritesApi = {
  list: (params: { page?: number; pageSize?: number }) =>
    apiClient.get<Paginated<RecipeSummary>>('/favorites', { params }).then((r) => r.data),

  add: (recipeId: string) => apiClient.put(`/favorites/${recipeId}`).then(() => undefined),

  remove: (recipeId: string) => apiClient.delete(`/favorites/${recipeId}`).then(() => undefined),
}

// -- Comments -- //

export const commentsApi = {
  list: (recipeId: string) =>
    apiClient.get<Comment[]>(`/recipes/${recipeId}/comments`).then((r) => r.data),

  add: (recipeId: string, body: string) =>
    apiClient.post<Comment>(`/recipes/${recipeId}/comments`, { body }).then((r) => r.data),

  update: (id: string, body: string) =>
    apiClient.put<Comment>(`/comments/${id}`, { body }).then((r) => r.data),

  remove: (id: string) => apiClient.delete(`/comments/${id}`).then(() => undefined),
}

// -- Collections -- //

export const collectionsApi = {
  list: () => apiClient.get<Collection[]>('/collections').then((r) => r.data),

  getById: (id: string) => apiClient.get<CollectionDetail>(`/collections/${id}`).then((r) => r.data),

  create: (body: { name: string; description: string | null }) =>
    apiClient.post<{ id: string }>('/collections', body).then((r) => r.data.id),

  update: (id: string, body: { name: string; description: string | null }) =>
    apiClient.put(`/collections/${id}`, body).then(() => undefined),

  delete: (id: string) => apiClient.delete(`/collections/${id}`).then(() => undefined),

  addRecipe: (collectionId: string, recipeId: string) =>
    apiClient.put(`/collections/${collectionId}/recipes/${recipeId}`).then(() => undefined),

  removeRecipe: (collectionId: string, recipeId: string) =>
    apiClient.delete(`/collections/${collectionId}/recipes/${recipeId}`).then(() => undefined),
}

export async function createRecipeFull(input: {
  recipe: CreateRecipePayload
  steps: CreateRecipeStepInput[]
  ingredients: CreateRecipeIngredientInput[]
  image?: File | null
}): Promise<string> {
  const recipeId = await recipesApi.create(input.recipe)

  for (const step of input.steps) {
    if (step.description.trim()) await recipesApi.appendStep(recipeId, step.description.trim())
  }

  for (const ingredient of input.ingredients) {
    if (ingredient.name.trim() && ingredient.quantity > 0) {
      await recipesApi.addIngredient(recipeId, ingredient)
    }
  }

  if (input.image) await recipesApi.uploadImage(recipeId, input.image)

  return recipeId
}
