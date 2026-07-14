import { useEffect, useState } from 'react'
import { ArrowLeft, ChevronDown } from 'lucide-react'
import { useRecipe, useUpdateRecipe } from '@/hooks/use-recipes'
import { useAuthStore } from '@/stores/auth-store'
import { getApiErrorMessage } from '@/lib/api-error'
import { DifficultyLevel } from '@/types/api'

export function EditScreen({ recipeId, onBack }: { recipeId: string; onBack: () => void }) {
  const { data: recipe, isLoading, isError } = useRecipe(recipeId)
  const currentUser = useAuthStore((s) => s.user)
  const update = useUpdateRecipe(recipeId)

  const [title, setTitle] = useState('')
  const [description, setDescription] = useState('')
  const [difficulty, setDifficulty] = useState<number>(DifficultyLevel.Easy)
  const [prepTime, setPrepTime] = useState('0')
  const [cookTime, setCookTime] = useState('0')
  const [servings, setServings] = useState('1')
  const [error, setError] = useState<string | null>(null)

  // Prefill the form once the recipe loads.
  useEffect(() => {
    if (!recipe) return
    setTitle(recipe.title)
    setDescription(recipe.description ?? '')
    setDifficulty(recipe.difficultyLevel)
    setPrepTime(String(recipe.prepTimeMinutes))
    setCookTime(String(recipe.cookTimeMinutes))
    setServings(String(recipe.servings))
  }, [recipe])

  if (isLoading) {
    return (
      <Centered>
        <span className="inline-block w-6 h-6 border-2 border-[#D94F3A]/30 border-t-[#D94F3A] rounded-full animate-spin" />
      </Centered>
    )
  }

  if (isError || !recipe) {
    return (
      <Centered>
        <p className="text-[#2C1A0E] font-medium mb-3">This recipe could not be loaded.</p>
        <BackButton onBack={onBack} />
      </Centered>
    )
  }

  const isOwner = Boolean(currentUser) && currentUser!.userId === recipe.userId
  if (!isOwner) {
    return (
      <Centered>
        <p className="text-[#2C1A0E] font-medium mb-3">You can only edit recipes that you created.</p>
        <BackButton onBack={onBack} />
      </Centered>
    )
  }

  const save = () => {
    setError(null)
    const serv = Number(servings)
    if (!title.trim()) {
      setError('Please provide a title.')
      return
    }
    if (!Number.isFinite(serv) || serv <= 0) {
      setError('Please provide a valid number of servings.')
      return
    }
    const prep = Number(prepTime)
    const cook = Number(cookTime)
    update.mutate(
      {
        title: title.trim(),
        description: description.trim() || null,
        difficultyLevel: difficulty,
        prepTimeMinutes: Number.isFinite(prep) && prep >= 0 ? prep : 0,
        cookTimeMinutes: Number.isFinite(cook) && cook >= 0 ? cook : 0,
        servings: serv,
      },
      {
        onSuccess: onBack,
        onError: (err) => setError(getApiErrorMessage(err)),
      },
    )
  }

  return (
    <div className="min-h-screen bg-background" style={{ fontFamily: "'DM Sans', sans-serif" }}>
      <header className="sticky top-0 z-20 bg-background/90 backdrop-blur-sm border-b border-border px-6 py-3.5 flex items-center gap-4">
        <button
          onClick={onBack}
          className="flex items-center gap-1.5 text-sm text-muted-foreground hover:text-[#2C1A0E] transition-colors"
        >
          <ArrowLeft size={15} />
          Back
        </button>
        <div className="flex-1" />
        <h1 className="text-base font-semibold text-[#2C1A0E]" style={{ fontFamily: "'Playfair Display', serif" }}>
          Edit Recipe
        </h1>
        <div className="flex-1 flex justify-end">
          <button
            onClick={save}
            disabled={update.isPending}
            className="px-4 py-2 bg-[#D94F3A] text-white rounded-xl text-sm font-medium hover:bg-[#C0392B] transition-colors disabled:opacity-70 min-w-[90px] flex items-center justify-center"
          >
            {update.isPending ? (
              <span className="inline-block w-4 h-4 border-2 border-white/30 border-t-white rounded-full animate-spin" />
            ) : (
              'Save'
            )}
          </button>
        </div>
      </header>

      <div className="max-w-2xl mx-auto px-5 py-10 space-y-4">
        {error && (
          <div className="px-4 py-3 rounded-xl bg-rose-50 border border-rose-200 text-sm text-[#C0392B]">{error}</div>
        )}

        <Field label="Recipe Title">
          <input value={title} onChange={(e) => setTitle(e.target.value)} className={inputClass} />
        </Field>

        <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <Field label="Category">
            <input value={recipe.categoryName} disabled className={`${inputClass} opacity-60 cursor-not-allowed`} />
          </Field>
          <Field label="Difficulty">
            <div className="relative">
              <select
                value={difficulty}
                onChange={(e) => setDifficulty(Number(e.target.value))}
                className={`${inputClass} appearance-none cursor-pointer`}
              >
                <option value={DifficultyLevel.Easy}>Easy</option>
                <option value={DifficultyLevel.Medium}>Medium</option>
                <option value={DifficultyLevel.Hard}>Hard</option>
                <option value={DifficultyLevel.Expert}>Expert</option>
              </select>
              <ChevronDown
                size={14}
                className="absolute right-3 top-1/2 -translate-y-1/2 text-muted-foreground pointer-events-none"
              />
            </div>
          </Field>
        </div>

        <div className="grid grid-cols-3 gap-4">
          <Field label="Prep (min)">
            <input type="number" min={0} value={prepTime} onChange={(e) => setPrepTime(e.target.value)} className={inputClass} />
          </Field>
          <Field label="Cook (min)">
            <input type="number" min={0} value={cookTime} onChange={(e) => setCookTime(e.target.value)} className={inputClass} />
          </Field>
          <Field label="Servings">
            <input type="number" min={1} value={servings} onChange={(e) => setServings(e.target.value)} className={inputClass} />
          </Field>
        </div>

        <Field label="Description">
          <textarea
            value={description}
            onChange={(e) => setDescription(e.target.value)}
            rows={3}
            className={`${inputClass} resize-none`}
          />
        </Field>

        <p className="text-xs text-muted-foreground pt-2">
          Category, ingredients, cooking steps, and the cover image aren&apos;t editable here yet.
        </p>
      </div>
    </div>
  )
}

const inputClass =
  'w-full px-3.5 py-2.5 bg-input-background border border-border rounded-xl text-sm focus:outline-none focus:ring-2 focus:ring-[#D94F3A]/25 focus:border-[#D94F3A] transition-all'

function Field({ label, children }: { label: string; children: React.ReactNode }) {
  return (
    <div>
      <label className="block text-xs font-medium text-[#2C1A0E] mb-1.5">{label}</label>
      {children}
    </div>
  )
}

function Centered({ children }: { children: React.ReactNode }) {
  return (
    <div
      className="min-h-screen bg-background flex items-center justify-center text-center p-10"
      style={{ fontFamily: "'DM Sans', sans-serif" }}
    >
      <div>{children}</div>
    </div>
  )
}

function BackButton({ onBack }: { onBack: () => void }) {
  return (
    <button
      onClick={onBack}
      className="px-5 py-2.5 bg-[#D94F3A] text-white rounded-xl text-sm font-medium hover:bg-[#C0392B] transition-colors"
    >
      Back
    </button>
  )
}
