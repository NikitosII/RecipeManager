// -- Enums --

export const DifficultyLevel = {
  Easy: 1,
  Medium: 2,
  Hard: 3,
  Expert: 4,
} as const
export type DifficultyLevel = (typeof DifficultyLevel)[keyof typeof DifficultyLevel]

export const DifficultyLabel: Record<number, string> = {
  1: 'Easy',
  2: 'Medium',
  3: 'Hard',
  4: 'Expert',
}

// Mirrors the backend RecipeSortBy enum; direction is passed separately.
export const RecipeSortBy = {
  DateCreated: 0,
  Name: 1,
  Rating: 2,
} as const
export type RecipeSortBy = (typeof RecipeSortBy)[keyof typeof RecipeSortBy]

export const MeasurementUnit = {
  Piece: 1,
  Gram: 2,
  Kilogram: 3,
  Milliliter: 4,
  Liter: 5,
  Teaspoon: 6,
  Tablespoon: 7,
  Cup: 8,
  Pinch: 9,
} as const
export type MeasurementUnit = (typeof MeasurementUnit)[keyof typeof MeasurementUnit]

// Short labels used in the ingredients table / detail list.
export const MeasurementUnitLabel: Record<number, string> = {
  1: 'pcs',
  2: 'g',
  3: 'kg',
  4: 'ml',
  5: 'L',
  6: 'tsp',
  7: 'tbsp',
  8: 'cup',
  9: 'pinch',
}

// Maps the free-text unit options in the Figma editor onto backend enum values.
export const UnitOptionToEnum: Record<string, MeasurementUnit> = {
  g: MeasurementUnit.Gram,
  kg: MeasurementUnit.Kilogram,
  ml: MeasurementUnit.Milliliter,
  L: MeasurementUnit.Liter,
  tsp: MeasurementUnit.Teaspoon,
  tbsp: MeasurementUnit.Tablespoon,
  cup: MeasurementUnit.Cup,
  pinch: MeasurementUnit.Pinch,
  whole: MeasurementUnit.Piece,
  pcs: MeasurementUnit.Piece,
}

// Mirrors the backend NutritionMode enum.
export const NutritionMode = {
  Auto: 1,
  Manual: 2,
} as const
export type NutritionMode = (typeof NutritionMode)[keyof typeof NutritionMode]

// Why an ingredient was left out of the automatic figures (backend UncountedReason).
export const UncountedReason = {
  MissingNutritionData: 1,
  UnconvertibleUnit: 2,
} as const
export type UncountedReason = (typeof UncountedReason)[keyof typeof UncountedReason]

export const UncountedReasonLabel: Record<number, string> = {
  1: 'no nutrition data',
  2: 'unit needs a weight',
}

// -- DTOs -- //

export interface RecipeSummary {
  id: string
  title: string
  description: string | null
  difficultyLevel: number
  prepTimeMinutes: number
  cookTimeMinutes: number
  servings: number
  imageUrl: string | null
  categoryName: string
  authorName: string
  isFavorite: boolean
  averageRating: number
  ratingCount: number
  userRating: number | null
  dateCreated: string
}

export interface Paginated<T> {
  items: T[]
  page: number
  pageSize: number
  totalCount: number
  totalPages: number
  hasNextPage: boolean
  hasPreviousPage: boolean
}

export interface RecipeStep {
  id: string
  stepNumber: number
  description: string
}

export interface RecipeIngredient {
  ingredientId: string
  ingredientName: string
  quantity: number
  unit: number
}

export interface UncountedIngredient {
  name: string
  reason: number
}

export interface Nutrition {
  mode: number
  calories: number
  protein: number
  fat: number
  carbohydrates: number
  fiber: number | null
  isComplete: boolean
  hasAnyData: boolean
  countedCount: number
  totalCount: number
  uncounted: UncountedIngredient[]
}

export interface RecipeDetail {
  id: string
  title: string
  description: string | null
  difficultyLevel: number
  prepTimeMinutes: number
  cookTimeMinutes: number
  servings: number
  imageUrl: string | null
  categoryId: string
  categoryName: string
  userId: string
  authorName: string
  isFavorite: boolean
  averageRating: number
  ratingCount: number
  userRating: number | null
  steps: RecipeStep[]
  ingredients: RecipeIngredient[]
  nutrition: Nutrition
  dateCreated: string
  dateUpdated: string
}

export interface Comment {
  id: string
  userId: string
  authorName: string
  authorAvatarUrl: string | null
  body: string
  canEdit: boolean
  dateCreated: string
  dateUpdated: string
}

export interface Category {
  id: string
  name: string
  slug: string
  recipeCount: number
}

export interface Ingredient {
  id: string
  name: string
}

export interface Collection {
  id: string
  name: string
  description: string | null
  recipeCount: number
  dateCreated: string
}

export interface CollectionDetail {
  id: string
  name: string
  description: string | null
  dateCreated: string
  recipes: RecipeSummary[]
}

export interface AuthResponse {
  accessToken: string
  accessTokenExpiry: string
  refreshToken: string
  userId: string
  email: string
  firstName: string
  lastName: string
}

export interface AuthUser {
  userId: string
  email: string
  firstName: string
  lastName: string
}

export interface UserProfile {
  userId: string
  email: string
  firstName: string
  lastName: string
  avatarUrl: string | null
  recipeCount: number
}
