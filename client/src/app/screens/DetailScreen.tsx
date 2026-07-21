import { useMemo, useState } from 'react'
import {
  ArrowLeft,
  Bookmark,
  Check,
  CircleCheck,
  Clock,
  Flame,
  FolderPlus,
  Heart,
  Pencil,
  Plus,
  RefreshCw,
  Sparkles,
  Trash2,
  User,
  Users,
  Utensils,
  X,
} from 'lucide-react'
import { useDeleteRecipe, useRecipe, useRefreshNutrition, useUpdateNutrition } from '@/hooks/use-recipes'
import { useToggleFavorite } from '@/hooks/use-favorites'
import { useAddRecipeToCollection, useCollections, useCreateCollection } from '@/hooks/use-collections'
import { useRateRecipe, useRemoveRating } from '@/hooks/use-ratings'
import { useAuthStore } from '@/stores/auth-store'
import { resolveMediaUrl } from '@/config'
import { MeasurementUnitLabel, NutritionMode, UncountedReasonLabel } from '@/types/api'
import type { RecipeDetail } from '@/types/api'
import { CategoryBadge, RatingStars, RatingSummary } from '../components/ui-bits'

export function DetailScreen({
  recipeId,
  onBack,
  onEdit,
}: {
  recipeId: string
  onBack: () => void
  onEdit: (id: string) => void
}) {
  const { data: recipe, isLoading, isError } = useRecipe(recipeId)
  const currentUser = useAuthStore((s) => s.user)
  const deleteRecipe = useDeleteRecipe()
  const toggleFavorite = useToggleFavorite()
  const rateRecipe = useRateRecipe()
  const removeRating = useRemoveRating()

  const [checkedIngredients, setCheckedIngredients] = useState<Set<string>>(new Set())
  const [activeStep, setActiveStep] = useState(0)

  const steps = useMemo(
    () => (recipe ? [...recipe.steps].sort((a, b) => a.stepNumber - b.stepNumber) : []),
    [recipe],
  )

  if (isLoading) {
    return (
      <CenteredMessage>
        <span className="inline-block w-6 h-6 border-2 border-[#D94F3A]/30 border-t-[#D94F3A] rounded-full animate-spin" />
      </CenteredMessage>
    )
  }

  if (isError || !recipe) {
    return (
      <CenteredMessage>
        <p className="text-[#2C1A0E] font-medium mb-3">This recipe could not be loaded.</p>
        <button
          onClick={onBack}
          className="px-5 py-2.5 bg-[#D94F3A] text-white rounded-xl text-sm font-medium hover:bg-[#C0392B] transition-colors"
        >
          Back to Dashboard
        </button>
      </CenteredMessage>
    )
  }

  const image = resolveMediaUrl(recipe.imageUrl)
  const isOwner = Boolean(currentUser) && currentUser!.userId === recipe.userId

  const handleDelete = () => {
    if (!window.confirm('Delete this recipe? This cannot be undone.')) return
    deleteRecipe.mutate(recipe.id, { onSuccess: onBack })
  }

  const toggleIngredient = (id: string) => {
    setCheckedIngredients((prev) => {
      const next = new Set(prev)
      if (next.has(id)) next.delete(id)
      else next.add(id)
      return next
    })
  }

  return (
    <div className="min-h-screen bg-background" style={{ fontFamily: "'DM Sans', sans-serif" }}>
      {/* Hero */}
      <div className="relative h-[60vh] min-h-[380px] overflow-hidden bg-muted">
        {image ? (
          <img src={image} alt={recipe.title} className="w-full h-full object-cover" />
        ) : (
          <div className="w-full h-full flex items-center justify-center text-muted-foreground/30">
            <Utensils size={72} />
          </div>
        )}
        <div className="absolute inset-0 bg-gradient-to-t from-[#2C1A0E]/90 via-[#2C1A0E]/30 to-transparent" />

        <button
          onClick={onBack}
          className="absolute top-5 left-5 flex items-center gap-1.5 px-3 py-2 bg-white/15 backdrop-blur-sm rounded-xl text-white text-sm font-medium hover:bg-white/25 transition-colors"
        >
          <ArrowLeft size={15} />
          Back
        </button>

        <button
          aria-label={recipe.isFavorite ? 'Remove from favourites' : 'Add to favourites'}
          onClick={() => toggleFavorite.mutate({ recipeId: recipe.id, isFavorite: recipe.isFavorite })}
          disabled={toggleFavorite.isPending}
          className={`absolute top-5 right-5 p-2.5 rounded-xl backdrop-blur-sm transition-colors disabled:opacity-60 ${
            recipe.isFavorite ? 'bg-[#D94F3A] text-white' : 'bg-white/15 text-white hover:bg-white/25'
          }`}
        >
          <Heart size={16} fill={recipe.isFavorite ? 'currentColor' : 'none'} />
        </button>

        <div className="absolute bottom-0 left-0 right-0 p-7">
          <CategoryBadge category={recipe.categoryName} />
          <h1
            className="text-3xl md:text-4xl font-bold text-white mt-3 mb-3 leading-tight max-w-2xl"
            style={{ fontFamily: "'Playfair Display', serif" }}
          >
            {recipe.title}
          </h1>
          <div className="flex flex-wrap items-center gap-4 text-white/80 text-sm">
            <span className="flex items-center gap-1.5">
              <span className="w-5 h-5 rounded-full bg-white/20 flex items-center justify-center">
                <User size={11} />
              </span>
              {recipe.authorName}
            </span>
            <span className="flex items-center gap-1">
              <Clock size={13} /> {recipe.prepTimeMinutes} min prep
            </span>
            <span className="flex items-center gap-1">
              <Flame size={13} /> {recipe.cookTimeMinutes} min cook
            </span>
            <span className="flex items-center gap-1">
              <Users size={13} /> {recipe.servings} servings
            </span>
            <RatingSummary average={recipe.averageRating} count={recipe.ratingCount} light />
          </div>
        </div>
      </div>

      {/* Body */}
      <div className="max-w-6xl mx-auto px-5 py-10 grid grid-cols-1 lg:grid-cols-[1fr_320px] gap-10">
        {/* Left — steps */}
        <div>
          {recipe.description && (
            <p className="text-muted-foreground text-sm leading-relaxed mb-8">{recipe.description}</p>
          )}

          <h2
            className="text-xl font-semibold text-[#2C1A0E] mb-6"
            style={{ fontFamily: "'Playfair Display', serif" }}
          >
            Cooking Steps
          </h2>

          {steps.length > 0 ? (
            <>
              <div className="mb-6 bg-muted rounded-full h-1.5 overflow-hidden">
                <div
                  className="h-full bg-[#D94F3A] rounded-full transition-all duration-500"
                  style={{ width: `${(activeStep / steps.length) * 100}%` }}
                />
              </div>
              <p className="text-xs text-muted-foreground mb-6">
                {activeStep} of {steps.length} steps completed
              </p>

              <div className="space-y-4">
                {steps.map((step, i) => {
                  const done = i < activeStep
                  const active = i === activeStep
                  return (
                    <div
                      key={step.id}
                      onClick={() => setActiveStep(done ? i : active ? i + 1 : activeStep)}
                      className={`flex gap-4 p-5 rounded-2xl border cursor-pointer transition-all duration-200 ${
                        done
                          ? 'border-border bg-muted/40 opacity-60'
                          : active
                            ? 'border-[#D94F3A]/40 bg-rose-50/50 shadow-sm'
                            : 'border-border bg-white hover:border-muted-foreground/30'
                      }`}
                    >
                      <div
                        className={`flex-shrink-0 w-8 h-8 rounded-full flex items-center justify-center text-sm font-bold ${
                          done ? 'bg-[#2C1A0E] text-white' : active ? 'bg-[#D94F3A] text-white' : 'bg-muted text-muted-foreground'
                        }`}
                      >
                        {done ? <Check size={14} /> : i + 1}
                      </div>
                      <p
                        className={`text-sm leading-relaxed pt-1 ${
                          done ? 'line-through text-muted-foreground' : 'text-[#2C1A0E]'
                        }`}
                      >
                        {step.description}
                      </p>
                    </div>
                  )
                })}
              </div>

              {activeStep === steps.length && (
                <div className="mt-6 p-5 bg-green-50 border border-green-200 rounded-2xl flex items-center gap-3">
                  <CircleCheck size={20} className="text-green-600 flex-shrink-0" />
                  <div>
                    <p className="text-sm font-semibold text-green-800">Recipe complete!</p>
                    <p className="text-xs text-green-600 mt-0.5">Enjoy your {recipe.title}.</p>
                  </div>
                </div>
              )}
            </>
          ) : (
            <p className="text-sm text-muted-foreground">No steps have been added to this recipe yet.</p>
          )}
        </div>

        {/* Right — ingredients + nutrition */}
        <div className="space-y-6 lg:sticky lg:top-6 self-start">
          <AddToCollectionMenu recipe={recipe} />

          {/* Ratings */}
          <div className="bg-white rounded-2xl border border-border p-5">
            <h3
              className="font-semibold text-[#2C1A0E] mb-3 text-base"
              style={{ fontFamily: "'Playfair Display', serif" }}
            >
              Rating
            </h3>
            <div className="flex items-baseline gap-2">
              <span className="text-3xl font-bold text-[#2C1A0E]" style={{ fontFamily: "'Playfair Display', serif" }}>
                {recipe.ratingCount > 0 ? recipe.averageRating.toFixed(1) : '—'}
              </span>
              <span className="text-sm text-muted-foreground">/ 5</span>
            </div>
            <p className="text-xs text-muted-foreground mt-0.5 mb-4">
              {recipe.ratingCount} {recipe.ratingCount === 1 ? 'rating' : 'ratings'}
            </p>

            <p className="text-xs font-medium text-muted-foreground mb-1.5">
              {recipe.userRating ? 'Your rating' : 'Rate this recipe'}
            </p>
            <div className="flex items-center justify-between">
              <RatingStars
                value={recipe.userRating}
                onRate={(value) => rateRecipe.mutate({ recipeId: recipe.id, value })}
                disabled={rateRecipe.isPending || removeRating.isPending}
              />
              {recipe.userRating != null && (
                <button
                  onClick={() => removeRating.mutate(recipe.id)}
                  disabled={removeRating.isPending}
                  className="text-xs text-muted-foreground hover:text-[#D94F3A] transition-colors disabled:opacity-50"
                >
                  Clear
                </button>
              )}
            </div>
          </div>

          {isOwner && (
            <>
              <button
                onClick={() => onEdit(recipe.id)}
                className="w-full flex items-center justify-center gap-2 py-3 rounded-2xl font-semibold text-sm border border-border text-[#2C1A0E] hover:bg-muted transition-colors"
              >
                <Pencil size={16} />
                Edit recipe
              </button>
              <button
                onClick={handleDelete}
                disabled={deleteRecipe.isPending}
                className="w-full flex items-center justify-center gap-2 py-3 rounded-2xl font-semibold text-sm border border-red-200 text-red-600 hover:bg-red-50 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
              >
                <Trash2 size={16} />
                {deleteRecipe.isPending ? 'Deleting…' : 'Delete recipe'}
              </button>
            </>
          )}

          <div className="bg-white rounded-2xl border border-border p-5">
            <h3
              className="font-semibold text-[#2C1A0E] mb-4 text-base"
              style={{ fontFamily: "'Playfair Display', serif" }}
            >
              Ingredients
            </h3>
            {recipe.ingredients.length > 0 ? (
              <div className="space-y-2.5">
                {recipe.ingredients.map((ing) => {
                  const checked = checkedIngredients.has(ing.ingredientId)
                  return (
                    <button
                      key={ing.ingredientId}
                      onClick={() => toggleIngredient(ing.ingredientId)}
                      className={`w-full flex items-center gap-3 p-2.5 rounded-xl text-left transition-all ${
                        checked ? 'opacity-50' : 'hover:bg-muted/50'
                      }`}
                    >
                      <div
                        className={`flex-shrink-0 w-4.5 h-4.5 rounded border flex items-center justify-center transition-colors ${
                          checked ? 'bg-[#D94F3A] border-[#D94F3A]' : 'border-border bg-input-background'
                        }`}
                      >
                        {checked && <Check size={9} className="text-white" />}
                      </div>
                      <span
                        className={`text-sm flex-1 ${checked ? 'line-through text-muted-foreground' : 'text-[#2C1A0E]'}`}
                      >
                        {ing.ingredientName}
                      </span>
                      <span className="text-xs text-muted-foreground font-medium whitespace-nowrap">
                        {ing.quantity} {MeasurementUnitLabel[ing.unit] ?? ''}
                      </span>
                    </button>
                  )
                })}
              </div>
            ) : (
              <p className="text-sm text-muted-foreground">No ingredients listed.</p>
            )}
          </div>

          {/* Nutrition — calculated from the ingredients, or entered by the author */}
          <NutritionPanel recipe={recipe} isOwner={isOwner} canRefresh={Boolean(currentUser)} />
        </div>
      </div>
    </div>
  )
}

function NutritionPanel({
  recipe,
  isOwner,
  canRefresh,
}: {
  recipe: RecipeDetail
  isOwner: boolean
  canRefresh: boolean
}) {
  const n = recipe.nutrition
  const [editing, setEditing] = useState(false)
  const refresh = useRefreshNutrition(recipe.id)

  const isManual = n.mode === NutritionMode.Manual
  const hasValues = isManual || n.hasAnyData

  const caption = isManual
    ? 'Per serving · entered by the author'
    : n.isComplete
      ? 'Per serving · calculated from all ingredients'
      : `Per serving · based on ${n.countedCount} of ${n.totalCount} ingredients`

  // The ids of the ingredients that were left out, so we can try to backfill just those.
  const uncountedNames = new Set(n.uncounted.map((u) => u.name))
  const idsToRefresh = recipe.ingredients
    .filter((i) => uncountedNames.has(i.ingredientName))
    .map((i) => i.ingredientId)
  const showRefresh = canRefresh && !isManual && idsToRefresh.length > 0

  const refreshButton = showRefresh ? (
    <button
      onClick={() => refresh.mutate(idsToRefresh)}
      disabled={refresh.isPending}
      className="mt-3 flex items-center gap-1.5 text-xs font-medium text-[#D94F3A] hover:text-[#C0392B] disabled:opacity-60"
    >
      <RefreshCw size={12} className={refresh.isPending ? 'animate-spin' : ''} />
      {refresh.isPending ? 'Fetching nutrition data…' : 'Fetch nutrition data'}
    </button>
  ) : null

  const rows = [
    { label: 'Calories', value: `${n.calories} kcal`, highlight: true },
    { label: 'Protein', value: `${n.protein} g`, highlight: false },
    { label: 'Carbohydrates', value: `${n.carbohydrates} g`, highlight: false },
    { label: 'Fat', value: `${n.fat} g`, highlight: false },
    ...(n.fiber != null ? [{ label: 'Fiber', value: `${n.fiber} g`, highlight: false }] : []),
  ]

  return (
    <div className="bg-white rounded-2xl border border-border p-5">
      <div className="flex items-center justify-between mb-2">
        <h3
          className="font-semibold text-[#2C1A0E] text-base"
          style={{ fontFamily: "'Playfair Display', serif" }}
        >
          Nutrition
        </h3>
        {isOwner && !editing && (
          <button
            onClick={() => setEditing(true)}
            className="flex items-center gap-1 text-xs font-medium text-[#D94F3A] hover:text-[#C0392B]"
          >
            <Pencil size={12} /> Edit
          </button>
        )}
      </div>

      {editing ? (
        <NutritionEditor recipe={recipe} onDone={() => setEditing(false)} />
      ) : hasValues ? (
        <>
          <p className="text-xs text-muted-foreground mb-4">{caption}</p>
          <div className="grid grid-cols-2 gap-3">
            {rows.map((r) => (
              <div
                key={r.label}
                className={`rounded-xl p-3 ${
                  r.highlight ? 'bg-rose-50 col-span-2 flex items-center justify-between' : 'bg-muted/50'
                }`}
              >
                <p className="text-xs text-muted-foreground">{r.label}</p>
                <p className={`font-semibold text-[#2C1A0E] ${r.highlight ? 'text-lg' : 'text-sm mt-0.5'}`}>
                  {r.value}
                </p>
              </div>
            ))}
          </div>
          {!isManual && n.uncounted.length > 0 && (
            <p className="text-[11px] text-muted-foreground mt-3">
              Not included:{' '}
              {n.uncounted
                .map((u) => `${u.name} (${UncountedReasonLabel[u.reason] ?? 'unknown'})`)
                .join(', ')}
              .
            </p>
          )}
          {refreshButton}
        </>
      ) : (
        <>
          <p className="text-sm text-muted-foreground">
            No nutrition yet — these ingredients don't have nutrition data.
            {isOwner ? ' Use Edit to enter values by hand.' : ''}
          </p>
          {refreshButton}
        </>
      )}
    </div>
  )
}

function NutritionEditor({ recipe, onDone }: { recipe: RecipeDetail; onDone: () => void }) {
  const n = recipe.nutrition
  const update = useUpdateNutrition(recipe.id)
  const [error, setError] = useState<string | null>(null)
  const [form, setForm] = useState({
    calories: String(n.calories ?? ''),
    protein: String(n.protein ?? ''),
    carbohydrates: String(n.carbohydrates ?? ''),
    fat: String(n.fat ?? ''),
    fiber: n.fiber != null ? String(n.fiber) : '',
  })

  const fields: { key: keyof typeof form; label: string; suffix: string }[] = [
    { key: 'calories', label: 'Calories', suffix: 'kcal' },
    { key: 'protein', label: 'Protein', suffix: 'g' },
    { key: 'carbohydrates', label: 'Carbohydrates', suffix: 'g' },
    { key: 'fat', label: 'Fat', suffix: 'g' },
    { key: 'fiber', label: 'Fiber (optional)', suffix: 'g' },
  ]

  const saveManual = () => {
    const core = {
      calories: Number(form.calories),
      protein: Number(form.protein),
      fat: Number(form.fat),
      carbohydrates: Number(form.carbohydrates),
    }
    if (Object.values(core).some((v) => !Number.isFinite(v) || v < 0)) {
      setError('Calories, protein, fat and carbohydrates are required and must be 0 or more.')
      return
    }
    const fiber = form.fiber.trim() === '' ? null : Number(form.fiber)
    if (fiber != null && (!Number.isFinite(fiber) || fiber < 0)) {
      setError('Fiber must be a number of 0 or more.')
      return
    }
    setError(null)
    update.mutate({ mode: NutritionMode.Manual, ...core, fiber }, { onSuccess: onDone })
  }

  const useAutomatic = () => {
    setError(null)
    update.mutate({ mode: NutritionMode.Auto }, { onSuccess: onDone })
  }

  return (
    <div>
      <p className="text-xs text-muted-foreground mb-3">
        Enter per-serving values, or switch back to automatic calculation.
      </p>

      <div className="space-y-2.5">
        {fields.map((f) => (
          <label key={f.key} className="flex items-center gap-2 text-sm">
            <span className="w-28 text-muted-foreground">{f.label}</span>
            <input
              type="number"
              min="0"
              step="0.1"
              inputMode="decimal"
              value={form[f.key]}
              onChange={(e) => setForm((prev) => ({ ...prev, [f.key]: e.target.value }))}
              className="flex-1 min-w-0 rounded-lg border border-border bg-input-background px-3 py-1.5 text-sm"
            />
            <span className="w-8 text-xs text-muted-foreground">{f.suffix}</span>
          </label>
        ))}
      </div>

      {error && <p className="text-xs text-red-600 mt-2">{error}</p>}

      <div className="flex items-center gap-2 mt-4">
        <button
          onClick={saveManual}
          disabled={update.isPending}
          className="flex items-center gap-1 px-3 py-1.5 rounded-lg bg-[#D94F3A] text-white text-sm font-medium hover:bg-[#C0392B] disabled:opacity-60"
        >
          <Check size={14} /> Save
        </button>
        <button
          onClick={onDone}
          disabled={update.isPending}
          className="flex items-center gap-1 px-3 py-1.5 rounded-lg border border-border text-sm text-muted-foreground hover:bg-muted/50"
        >
          <X size={14} /> Cancel
        </button>
        <button
          onClick={useAutomatic}
          disabled={update.isPending}
          className="flex items-center gap-1 px-3 py-1.5 rounded-lg text-sm text-[#D94F3A] hover:bg-rose-50 ml-auto"
        >
          <Sparkles size={14} /> Use automatic
        </button>
      </div>
    </div>
  )
}

function AddToCollectionMenu({ recipe }: { recipe: RecipeDetail }) {
  const { data: collections = [], isLoading } = useCollections()
  const addRecipe = useAddRecipeToCollection()
  const createCollection = useCreateCollection()

  const [open, setOpen] = useState(false)
  const [added, setAdded] = useState<Set<string>>(new Set())
  const [creating, setCreating] = useState(false)
  const [newName, setNewName] = useState('')

  const markAdded = (collectionId: string) =>
    setAdded((prev) => new Set(prev).add(collectionId))

  const handleAdd = (collectionId: string) => {
    if (added.has(collectionId)) return
    addRecipe.mutate(
      { collectionId, recipeId: recipe.id },
      { onSuccess: () => markAdded(collectionId) },
    )
  }

  const handleCreate = async () => {
    const name = newName.trim()
    if (!name) return
    const id = await createCollection.mutateAsync({ name, description: null })
    await addRecipe.mutateAsync({ collectionId: id, recipeId: recipe.id })
    markAdded(id)
    setNewName('')
    setCreating(false)
  }

  return (
    <div className="relative">
      <button
        onClick={() => setOpen((o) => !o)}
        className="w-full flex items-center justify-center gap-2 py-3.5 rounded-2xl font-semibold text-sm bg-[#D94F3A] text-white hover:bg-[#C0392B] transition-colors"
      >
        <Bookmark size={16} /> Save to Collection
      </button>

      {open && (
        <div className="absolute z-30 mt-2 w-full bg-white rounded-2xl border border-border shadow-lg p-2">
          <p className="text-[10px] font-semibold uppercase tracking-widest text-muted-foreground px-2 pt-1 pb-1.5">
            Your collections
          </p>

          {isLoading ? (
            <p className="px-2 py-2 text-sm text-muted-foreground">Loading…</p>
          ) : collections.length > 0 ? (
            <div className="max-h-52 overflow-y-auto space-y-0.5">
              {collections.map((c) => {
                const isAdded = added.has(c.id)
                return (
                  <button
                    key={c.id}
                    onClick={() => handleAdd(c.id)}
                    disabled={isAdded || addRecipe.isPending}
                    className="w-full flex items-center gap-2.5 px-2.5 py-2 rounded-xl text-sm text-left text-[#2C1A0E] hover:bg-muted transition-colors disabled:cursor-default"
                  >
                    <Bookmark size={14} className="text-muted-foreground flex-shrink-0" />
                    <span className="flex-1 truncate">{c.name}</span>
                    {isAdded ? (
                      <Check size={14} className="text-green-600" />
                    ) : (
                      <Plus size={14} className="text-muted-foreground" />
                    )}
                  </button>
                )
              })}
            </div>
          ) : (
            <p className="px-2 py-2 text-sm text-muted-foreground">No collections yet.</p>
          )}

          <div className="border-t border-border mt-1.5 pt-1.5">
            {creating ? (
              <div className="flex items-center gap-1.5 px-1">
                <input
                  autoFocus
                  value={newName}
                  onChange={(e) => setNewName(e.target.value)}
                  onKeyDown={(e) => {
                    if (e.key === 'Enter') void handleCreate()
                    if (e.key === 'Escape') setCreating(false)
                  }}
                  placeholder="Collection name"
                  className="flex-1 min-w-0 px-2.5 py-1.5 bg-input-background border border-border rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-[#D94F3A]/25"
                />
                <button
                  onClick={() => void handleCreate()}
                  disabled={!newName.trim() || createCollection.isPending}
                  className="px-3 py-1.5 rounded-lg bg-[#D94F3A] text-white text-sm font-medium hover:bg-[#C0392B] transition-colors disabled:opacity-50"
                >
                  Add
                </button>
              </div>
            ) : (
              <button
                onClick={() => setCreating(true)}
                className="w-full flex items-center gap-2.5 px-2.5 py-2 rounded-xl text-sm text-left font-medium text-[#D94F3A] hover:bg-rose-50 transition-colors"
              >
                <FolderPlus size={15} />
                New collection
              </button>
            )}
          </div>
        </div>
      )}
    </div>
  )
}

function CenteredMessage({ children }: { children: React.ReactNode }) {
  return (
    <div
      className="min-h-screen bg-background flex items-center justify-center text-center p-10"
      style={{ fontFamily: "'DM Sans', sans-serif" }}
    >
      <div>{children}</div>
    </div>
  )
}
