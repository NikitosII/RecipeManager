import { apiClient } from '@/lib/api-client'
import type {
  AuthResponse,
  Category,
  Ingredient,
  MeasurementUnit,
  Paginated,
  RecipeDetail,
  RecipeStep,
  RecipeSummary,
} from '@/types/api'

// -- Auth -- //

export const authApi = {
  register: (body: { firstName: string; lastName: string; email: string; password: string }) =>
    apiClient.post<AuthResponse>('/auth/register', body).then((r) => r.data),

  login: (body: { email: string; password: string }) =>
    apiClient.post<AuthResponse>('/auth/login', body).then((r) => r.data),

  logout: (refreshToken: string) => apiClient.post('/auth/logout', { refreshToken }).then(() => undefined),
}

// -- Categories & ingredients -- //

export const categoriesApi = {
  list: () => apiClient.get<Category[]>('/categories').then((r) => r.data),
}

export const ingredientsApi = {
  list: () => apiClient.get<Ingredient[]>('/ingredients').then((r) => r.data),
}

// -- Recipes -- //

export interface RecipeListParams {
  page?: number
  pageSize?: number
  search?: string
  categoryId?: string
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
      .get<Paginated<RecipeSummary>>('/recipes', { params })
      .then((r) => r.data),

  getById: (id: string) => apiClient.get<RecipeDetail>(`/recipes/${id}`).then((r) => r.data),

  create: (body: CreateRecipePayload) =>
    apiClient.post<{ id: string }>('/recipes', body).then((r) => r.data.id),

  update: (id: string, body: UpdateRecipePayload) =>
    apiClient.put(`/recipes/${id}`, body).then(() => undefined),

  delete: (id: string) => apiClient.delete(`/recipes/${id}`).then(() => undefined),

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
