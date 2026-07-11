import { useMemo, useState } from 'react'
import { ArrowLeft, Bookmark, Check, CircleCheck, Clock, Flame, Heart, Users } from 'lucide-react'
import { useRecipe } from '@/hooks/use-recipes'
import { resolveMediaUrl } from '@/config'
import { MeasurementUnitLabel } from '@/types/api'
import { decorationFor, PLACEHOLDER_NUTRITION } from '../placeholders'
import { CategoryBadge, StarRating } from '../components/ui-bits'

export function DetailScreen({ recipeId, onBack }: { recipeId: string; onBack: () => void }) {
  const { data: recipe, isLoading, isError } = useRecipe(recipeId)

  const [checkedIngredients, setCheckedIngredients] = useState<Set<string>>(new Set())
  const [activeStep, setActiveStep] = useState(0)
  const [saved, setSaved] = useState(false)
  const [favorite, setFavorite] = useState(false)

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

  const deco = decorationFor(recipe.id)
  const image = resolveMediaUrl(recipe.imageUrl) ?? deco.fallbackImage

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
        <img src={image} alt={recipe.title} className="w-full h-full object-cover" />
        <div className="absolute inset-0 bg-gradient-to-t from-[#2C1A0E]/90 via-[#2C1A0E]/30 to-transparent" />

        <button
          onClick={onBack}
          className="absolute top-5 left-5 flex items-center gap-1.5 px-3 py-2 bg-white/15 backdrop-blur-sm rounded-xl text-white text-sm font-medium hover:bg-white/25 transition-colors"
        >
          <ArrowLeft size={15} />
          Back
        </button>

        <button
          onClick={() => setFavorite((f) => !f)}
          className={`absolute top-5 right-5 p-2.5 rounded-xl backdrop-blur-sm transition-colors ${
            favorite ? 'bg-[#D94F3A] text-white' : 'bg-white/15 text-white hover:bg-white/25'
          }`}
        >
          <Heart size={16} fill={favorite ? 'currentColor' : 'none'} />
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
              <img src={deco.author.avatar} alt={deco.author.name} className="w-5 h-5 rounded-full object-cover" />
              {deco.author.name}
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
            <StarRating rating={deco.rating} />
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
          <button
            onClick={() => setSaved(!saved)}
            className={`w-full flex items-center justify-center gap-2 py-3.5 rounded-2xl font-semibold text-sm transition-all duration-200 ${
              saved ? 'bg-[#2C1A0E] text-white' : 'bg-[#D94F3A] text-white hover:bg-[#C0392B]'
            }`}
          >
            {saved ? (
              <>
                <Bookmark size={16} fill="currentColor" /> Saved to Collection
              </>
            ) : (
              <>
                <Bookmark size={16} /> Save to Collection
              </>
            )}
          </button>

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

          {/* Nutrition — static decoration (not stored by the backend) */}
          <div className="bg-white rounded-2xl border border-border p-5">
            <h3
              className="font-semibold text-[#2C1A0E] mb-4 text-base"
              style={{ fontFamily: "'Playfair Display', serif" }}
            >
              Nutrition per serving
            </h3>
            <div className="grid grid-cols-2 gap-3">
              {[
                { label: 'Calories', value: `${PLACEHOLDER_NUTRITION.calories} kcal`, highlight: true },
                { label: 'Protein', value: PLACEHOLDER_NUTRITION.protein, highlight: false },
                { label: 'Carbohydrates', value: PLACEHOLDER_NUTRITION.carbs, highlight: false },
                { label: 'Fat', value: PLACEHOLDER_NUTRITION.fat, highlight: false },
                { label: 'Fiber', value: PLACEHOLDER_NUTRITION.fiber, highlight: false },
              ].map((n) => (
                <div
                  key={n.label}
                  className={`rounded-xl p-3 ${
                    n.highlight ? 'bg-rose-50 col-span-2 flex items-center justify-between' : 'bg-muted/50'
                  }`}
                >
                  <p className="text-xs text-muted-foreground">{n.label}</p>
                  <p className={`font-semibold text-[#2C1A0E] ${n.highlight ? 'text-lg' : 'text-sm mt-0.5'}`}>
                    {n.value}
                  </p>
                </div>
              ))}
            </div>
          </div>
        </div>
      </div>
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
