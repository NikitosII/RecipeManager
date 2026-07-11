import { useCallback, useEffect, useRef, useState } from 'react'
import type { ChangeEvent, DragEvent } from 'react'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import {
  ArrowLeft,
  ChevronDown,
  CircleCheck,
  GripVertical,
  ImageIcon,
  MoveDown,
  MoveUp,
  PlusCircle,
  Trash2,
  Upload,
  X,
} from 'lucide-react'
import { useCategories } from '@/hooks/use-catalog'
import { createRecipeFull } from '@/lib/api'
import { recipeKeys } from '@/hooks/use-recipes'
import { getApiErrorMessage } from '@/lib/api-error'
import { DifficultyLevel, UnitOptionToEnum } from '@/types/api'
import type { MeasurementUnit } from '@/types/api'

interface IngredientRow {
  id: number
  name: string
  quantity: string
  unit: string
}
interface StepRow {
  id: number
  text: string
}

const UNIT_OPTIONS = ['g', 'kg', 'ml', 'L', 'tsp', 'tbsp', 'cup', 'pinch', 'whole']
const ALLOWED_IMAGE_TYPES = ['image/jpeg', 'image/png', 'image/webp', 'image/gif']

let _uid = 1000
const uid = () => ++_uid

export function CreateScreen({ onBack }: { onBack: () => void }) {
  const queryClient = useQueryClient()
  const { data: categories = [] } = useCategories()

  const [title, setTitle] = useState('')
  const [categoryId, setCategoryId] = useState('')
  const [difficulty, setDifficulty] = useState<number>(DifficultyLevel.Easy)
  const [prepTime, setPrepTime] = useState('10')
  const [cookTime, setCookTime] = useState('20')
  const [servings, setServings] = useState('2')
  const [description, setDescription] = useState('')

  const [isDragging, setIsDragging] = useState(false)
  const [imageFile, setImageFile] = useState<File | null>(null)
  const [imagePreview, setImagePreview] = useState<string | null>(null)
  const fileInputRef = useRef<HTMLInputElement>(null)

  const [ingredients, setIngredients] = useState<IngredientRow[]>([
    { id: uid(), name: '', quantity: '', unit: 'g' },
  ])
  const [steps, setSteps] = useState<StepRow[]>([{ id: uid(), text: '' }])

  const [error, setError] = useState<string | null>(null)

  // Default the category select to the first available category.
  useEffect(() => {
    if (!categoryId && categories.length > 0) setCategoryId(categories[0].id)
  }, [categories, categoryId])

  // ── Media (single image only — the API stores one cover image) ──
  const selectImage = useCallback((file: File | undefined) => {
    if (!file) return
    if (!ALLOWED_IMAGE_TYPES.includes(file.type)) {
      setError('Only image files are supported (JPG, PNG, WEBP, GIF).')
      return
    }
    if (file.size > 10 * 1024 * 1024) {
      setError('Image exceeds the maximum size of 10 MB.')
      return
    }
    setError(null)
    setImageFile(file)
    setImagePreview((prev) => {
      if (prev) URL.revokeObjectURL(prev)
      return URL.createObjectURL(file)
    })
  }, [])

  const handleDrop = useCallback(
    (e: DragEvent<HTMLDivElement>) => {
      e.preventDefault()
      setIsDragging(false)
      selectImage(e.dataTransfer.files[0])
    },
    [selectImage],
  )

  const handleFileInput = (e: ChangeEvent<HTMLInputElement>) => selectImage(e.target.files?.[0])

  const clearImage = () => {
    if (imagePreview) URL.revokeObjectURL(imagePreview)
    setImageFile(null)
    setImagePreview(null)
  }

  useEffect(() => () => {
    if (imagePreview) URL.revokeObjectURL(imagePreview)
  }, [imagePreview])

  // ── Ingredients ──
  const addIngredient = () =>
    setIngredients((prev) => [...prev, { id: uid(), name: '', quantity: '', unit: 'g' }])
  const removeIngredient = (id: number) => setIngredients((prev) => prev.filter((i) => i.id !== id))
  const updateIngredient = (id: number, field: keyof IngredientRow, val: string) =>
    setIngredients((prev) => prev.map((i) => (i.id === id ? { ...i, [field]: val } : i)))

  // ── Steps ──
  const addStep = () => setSteps((prev) => [...prev, { id: uid(), text: '' }])
  const insertStepAt = (index: number) =>
    setSteps((prev) => [...prev.slice(0, index), { id: uid(), text: '' }, ...prev.slice(index)])
  const removeStep = (id: number) => setSteps((prev) => prev.filter((s) => s.id !== id))
  const updateStep = (id: number, text: string) =>
    setSteps((prev) => prev.map((s) => (s.id === id ? { ...s, text } : s)))
  const moveStep = (index: number, dir: -1 | 1) => {
    const next = index + dir
    if (next < 0 || next >= steps.length) return
    setSteps((prev) => {
      const arr = [...prev]
      ;[arr[index], arr[next]] = [arr[next], arr[index]]
      return arr
    })
  }

  // ── Publish ──
  const publish = useMutation({
    mutationFn: () => {
      const prep = Number(prepTime)
      const cook = Number(cookTime)
      const serv = Number(servings)

      const problems: string[] = []
      if (!title.trim()) problems.push('a title')
      if (!categoryId) problems.push('a category')
      if (!Number.isFinite(serv) || serv <= 0) problems.push('a valid number of servings')
      if (problems.length > 0) throw new Error(`Please provide ${problems.join(', ')}.`)

      return createRecipeFull({
        recipe: {
          title: title.trim(),
          description: description.trim() || null,
          difficultyLevel: difficulty,
          prepTimeMinutes: Number.isFinite(prep) && prep >= 0 ? prep : 0,
          cookTimeMinutes: Number.isFinite(cook) && cook >= 0 ? cook : 0,
          servings: serv,
          categoryId,
        },
        steps: steps.map((s) => ({ description: s.text })),
        ingredients: ingredients.map((i) => ({
          name: i.name,
          quantity: parseFloat(i.quantity) || 0,
          unit: (UnitOptionToEnum[i.unit] ?? UnitOptionToEnum.whole) as MeasurementUnit,
        })),
        image: imageFile,
      })
    },
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: recipeKeys.all })
    },
    onError: (err) => setError(getApiErrorMessage(err)),
  })

  const handlePublish = () => {
    setError(null)
    publish.mutate()
  }

  if (publish.isSuccess) {
    return (
      <div
        className="min-h-screen bg-background flex items-center justify-center"
        style={{ fontFamily: "'DM Sans', sans-serif" }}
      >
        <div className="text-center p-10">
          <div className="w-16 h-16 bg-green-100 rounded-full flex items-center justify-center mx-auto mb-5">
            <CircleCheck size={32} className="text-green-600" />
          </div>
          <h2
            className="text-2xl font-semibold text-[#2C1A0E] mb-2"
            style={{ fontFamily: "'Playfair Display', serif" }}
          >
            Recipe published!
          </h2>
          <p className="text-muted-foreground text-sm mb-7">Your recipe is now live and discoverable by others.</p>
          <button
            onClick={onBack}
            className="px-6 py-2.5 bg-[#D94F3A] text-white rounded-xl text-sm font-medium hover:bg-[#C0392B] transition-colors"
          >
            Back to Dashboard
          </button>
        </div>
      </div>
    )
  }

  const publishing = publish.isPending

  return (
    <div className="min-h-screen bg-background" style={{ fontFamily: "'DM Sans', sans-serif" }}>
      {/* Top bar */}
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
          Create Recipe
        </h1>
        <div className="flex-1 flex justify-end gap-2">
          <button
            onClick={handlePublish}
            disabled={publishing}
            className="px-4 py-2 bg-[#D94F3A] text-white rounded-xl text-sm font-medium hover:bg-[#C0392B] transition-colors disabled:opacity-70 min-w-[90px] flex items-center justify-center"
          >
            {publishing ? (
              <span className="inline-block w-4 h-4 border-2 border-white/30 border-t-white rounded-full animate-spin" />
            ) : (
              'Publish'
            )}
          </button>
        </div>
      </header>

      <div className="max-w-3xl mx-auto px-5 py-10 space-y-10">
        {error && (
          <div className="px-4 py-3 rounded-xl bg-rose-50 border border-rose-200 text-sm text-[#C0392B]">{error}</div>
        )}

        {/* Section 1 — Basic Info */}
        <section>
          <SectionHeading step={1} title="Basic Info" />
          <div className="space-y-4">
            <Field label="Recipe Title">
              <input
                value={title}
                onChange={(e) => setTitle(e.target.value)}
                placeholder="e.g. Rustic Tomato Galette with Gruyère"
                className={inputClass}
              />
            </Field>

            <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
              <Field label="Category">
                <div className="relative">
                  <select
                    value={categoryId}
                    onChange={(e) => setCategoryId(e.target.value)}
                    className={`${inputClass} appearance-none cursor-pointer`}
                  >
                    {categories.length === 0 && <option value="">No categories</option>}
                    {categories.map((c) => (
                      <option key={c.id} value={c.id}>
                        {c.name}
                      </option>
                    ))}
                  </select>
                  <ChevronDown
                    size={14}
                    className="absolute right-3 top-1/2 -translate-y-1/2 text-muted-foreground pointer-events-none"
                  />
                </div>
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
                placeholder="A short description of the dish — what makes it special, flavour notes, occasion…"
                rows={3}
                className={`${inputClass} resize-none`}
              />
            </Field>
          </div>
        </section>

        {/* Section 2 — Media */}
        <section>
          <SectionHeading step={2} title="Cover Image" />
          {imagePreview ? (
            <div className="relative rounded-2xl overflow-hidden border border-border">
              <img src={imagePreview} alt="Selected cover" className="w-full h-56 object-cover" />
              <button
                onClick={clearImage}
                className="absolute top-3 right-3 p-2 rounded-xl bg-black/50 text-white hover:bg-black/70 transition-colors"
              >
                <X size={15} />
              </button>
              <div className="absolute bottom-0 left-0 right-0 px-4 py-2 bg-black/40 text-white text-xs truncate">
                {imageFile?.name}
              </div>
            </div>
          ) : (
            <div
              onDragOver={(e) => {
                e.preventDefault()
                setIsDragging(true)
              }}
              onDragLeave={() => setIsDragging(false)}
              onDrop={handleDrop}
              onClick={() => fileInputRef.current?.click()}
              className={`relative border-2 border-dashed rounded-2xl p-10 text-center cursor-pointer transition-all duration-200 ${
                isDragging
                  ? 'border-[#D94F3A] bg-rose-50'
                  : 'border-border bg-input-background hover:border-muted-foreground/40 hover:bg-muted/50'
              }`}
            >
              <input
                ref={fileInputRef}
                type="file"
                accept="image/jpeg,image/png,image/webp,image/gif"
                className="hidden"
                onChange={handleFileInput}
              />
              <div
                className={`w-12 h-12 rounded-2xl mx-auto mb-4 flex items-center justify-center transition-colors ${
                  isDragging ? 'bg-[#D94F3A]/10 text-[#D94F3A]' : 'bg-muted text-muted-foreground'
                }`}
              >
                <Upload size={22} />
              </div>
              <p className="text-sm font-medium text-[#2C1A0E] mb-1">
                {isDragging ? 'Drop image here' : 'Drop a cover image here'}
              </p>
              <p className="text-xs text-muted-foreground">
                or <span className="text-[#D94F3A] font-medium">browse files</span> — JPG, PNG, WEBP, GIF up to 10MB
              </p>
              <div className="flex items-center justify-center gap-4 mt-4 text-muted-foreground/50">
                <ImageIcon size={18} />
              </div>
            </div>
          )}
        </section>

        {/* Section 3 — Ingredients */}
        <section>
          <SectionHeading step={3} title="Ingredients" />
          <div className="space-y-2">
            <div className="grid grid-cols-[1fr_80px_90px_32px] gap-2 px-1 mb-1">
              {['Ingredient', 'Qty', 'Unit', ''].map((h, i) => (
                <span key={i} className="text-[10px] font-semibold uppercase tracking-wider text-muted-foreground">
                  {h}
                </span>
              ))}
            </div>
            {ingredients.map((ing) => (
              <div key={ing.id} className="grid grid-cols-[1fr_80px_90px_32px] gap-2">
                <input
                  value={ing.name}
                  onChange={(e) => updateIngredient(ing.id, 'name', e.target.value)}
                  placeholder="e.g. Cherry tomatoes"
                  className={smallInputClass}
                />
                <input
                  value={ing.quantity}
                  onChange={(e) => updateIngredient(ing.id, 'quantity', e.target.value)}
                  placeholder="200"
                  className={smallInputClass}
                />
                <div className="relative">
                  <select
                    value={ing.unit}
                    onChange={(e) => updateIngredient(ing.id, 'unit', e.target.value)}
                    className={`${smallInputClass} w-full appearance-none cursor-pointer`}
                  >
                    {UNIT_OPTIONS.map((u) => (
                      <option key={u} value={u}>
                        {u}
                      </option>
                    ))}
                  </select>
                </div>
                <button
                  onClick={() => removeIngredient(ing.id)}
                  disabled={ingredients.length === 1}
                  className="flex items-center justify-center text-muted-foreground hover:text-[#D94F3A] disabled:opacity-30 disabled:cursor-not-allowed transition-colors"
                >
                  <Trash2 size={14} />
                </button>
              </div>
            ))}
          </div>
          <button
            onClick={addIngredient}
            className="mt-3 flex items-center gap-1.5 text-sm text-[#D94F3A] hover:text-[#C0392B] font-medium transition-colors"
          >
            <PlusCircle size={15} />
            Add Ingredient
          </button>
        </section>

        {/* Section 4 — Steps */}
        <section>
          <SectionHeading step={4} title="Cooking Steps" trailing="Reorder · Insert anywhere" />
          <button
            onClick={() => insertStepAt(0)}
            className="w-full flex items-center gap-2 py-1.5 text-xs text-muted-foreground hover:text-[#D94F3A] transition-colors group mb-1"
          >
            <div className="flex-1 h-px bg-border group-hover:bg-[#D94F3A]/30 transition-colors" />
            <PlusCircle size={12} className="flex-shrink-0" />
            <span>Insert step here</span>
            <div className="flex-1 h-px bg-border group-hover:bg-[#D94F3A]/30 transition-colors" />
          </button>

          <div className="space-y-1">
            {steps.map((step, i) => (
              <div key={step.id}>
                <div className="flex gap-3 bg-white border border-border rounded-2xl p-4 group hover:border-muted-foreground/30 transition-colors">
                  <div className="flex flex-col items-center gap-2 flex-shrink-0">
                    <div className="w-6 h-6 bg-[#2C1A0E] text-white rounded-full flex items-center justify-center text-xs font-bold">
                      {i + 1}
                    </div>
                    <GripVertical size={14} className="text-muted-foreground/40 cursor-grab mt-1" />
                    <div className="flex flex-col gap-1 mt-auto">
                      <button
                        onClick={() => moveStep(i, -1)}
                        disabled={i === 0}
                        className="text-muted-foreground hover:text-[#2C1A0E] disabled:opacity-20 transition-colors"
                      >
                        <MoveUp size={13} />
                      </button>
                      <button
                        onClick={() => moveStep(i, 1)}
                        disabled={i === steps.length - 1}
                        className="text-muted-foreground hover:text-[#2C1A0E] disabled:opacity-20 transition-colors"
                      >
                        <MoveDown size={13} />
                      </button>
                    </div>
                  </div>

                  <textarea
                    value={step.text}
                    onChange={(e) => updateStep(step.id, e.target.value)}
                    placeholder={`Describe step ${i + 1}…`}
                    rows={2}
                    className="flex-1 bg-transparent text-sm text-[#2C1A0E] resize-none focus:outline-none placeholder:text-muted-foreground/50 leading-relaxed"
                  />

                  <button
                    onClick={() => removeStep(step.id)}
                    disabled={steps.length === 1}
                    className="self-start flex-shrink-0 text-muted-foreground/40 hover:text-[#D94F3A] disabled:opacity-0 transition-colors"
                  >
                    <X size={14} />
                  </button>
                </div>

                <button
                  onClick={() => insertStepAt(i + 1)}
                  className="w-full flex items-center gap-2 py-1.5 text-xs text-muted-foreground hover:text-[#D94F3A] transition-colors group my-1"
                >
                  <div className="flex-1 h-px bg-border group-hover:bg-[#D94F3A]/30 transition-colors" />
                  <PlusCircle size={12} className="flex-shrink-0" />
                  <span>Insert step here</span>
                  <div className="flex-1 h-px bg-border group-hover:bg-[#D94F3A]/30 transition-colors" />
                </button>
              </div>
            ))}
          </div>

          <button
            onClick={addStep}
            className="mt-2 flex items-center gap-1.5 text-sm text-[#D94F3A] hover:text-[#C0392B] font-medium transition-colors"
          >
            <PlusCircle size={15} />
            Add Step
          </button>
        </section>

        {/* Footer */}
        <div className="flex items-center justify-end pt-4 border-t border-border">
          <button
            onClick={handlePublish}
            disabled={publishing}
            className="px-7 py-2.5 bg-[#D94F3A] text-white rounded-xl text-sm font-semibold hover:bg-[#C0392B] transition-colors disabled:opacity-70 min-w-[110px] flex items-center justify-center gap-2"
          >
            {publishing ? (
              <>
                <span className="inline-block w-4 h-4 border-2 border-white/30 border-t-white rounded-full animate-spin" />{' '}
                Publishing…
              </>
            ) : (
              'Publish Recipe'
            )}
          </button>
        </div>
      </div>
    </div>
  )
}

const inputClass =
  'w-full px-3.5 py-2.5 bg-input-background border border-border rounded-xl text-sm focus:outline-none focus:ring-2 focus:ring-[#D94F3A]/25 focus:border-[#D94F3A] transition-all'
const smallInputClass =
  'px-3 py-2 bg-input-background border border-border rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-[#D94F3A]/25 focus:border-[#D94F3A] transition-all'

function SectionHeading({ step, title, trailing }: { step: number; title: string; trailing?: string }) {
  return (
    <div className="flex items-center gap-2.5 mb-5">
      <span className="w-6 h-6 bg-[#D94F3A] text-white rounded-lg flex items-center justify-center text-xs font-bold flex-shrink-0">
        {step}
      </span>
      <h2 className="text-lg font-semibold text-[#2C1A0E]" style={{ fontFamily: "'Playfair Display', serif" }}>
        {title}
      </h2>
      {trailing && <span className="text-xs text-muted-foreground ml-auto">{trailing}</span>}
    </div>
  )
}

function Field({ label, children }: { label: string; children: React.ReactNode }) {
  return (
    <div>
      <label className="block text-xs font-medium text-[#2C1A0E] mb-1.5">{label}</label>
      {children}
    </div>
  )
}
