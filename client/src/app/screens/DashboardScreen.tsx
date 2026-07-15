import { useMemo, useState } from 'react'
import {
  Bell,
  Bookmark,
  ChevronDown,
  ChevronLeft,
  ChevronRight,
  FolderPlus,
  Heart,
  LogOut,
  Plus,
  Search,
  Settings,
  SlidersHorizontal,
  Trash2,
  User,
  Utensils,
  X,
} from 'lucide-react'
import { useCategories, useIngredients } from '@/hooks/use-catalog'
import { useRecipes } from '@/hooks/use-recipes'
import { useFavorites, useToggleFavorite } from '@/hooks/use-favorites'
import {
  useCollection,
  useCollections,
  useCreateCollection,
  useDeleteCollection,
  useRemoveRecipeFromCollection,
} from '@/hooks/use-collections'
import { useDebouncedValue } from '@/hooks/use-debounced-value'
import { useAuthStore } from '@/stores/auth-store'
import { DifficultyLabel, RecipeSortBy } from '@/types/api'
import type { RecipeSummary } from '@/types/api'
import { categoryVisual } from '../components/category-config'
import { RecipeCard } from '../components/RecipeCard'
import { SkeletonCard } from '../components/ui-bits'

const PAGE_SIZE = 9

type View =
  | { kind: 'recipes' }
  | { kind: 'favorites' }
  | { kind: 'collections' }
  | { kind: 'collection'; id: string; name: string }

interface Filters {
  difficulty: number | null
  maxPrepTime: number | null
  maxCookTime: number | null
  minServings: number | null
  ingredientIds: string[]
}

const EMPTY_FILTERS: Filters = {
  difficulty: null,
  maxPrepTime: null,
  maxCookTime: null,
  minServings: null,
  ingredientIds: [],
}

function countActiveFilters(f: Filters): number {
  return (
    (f.difficulty !== null ? 1 : 0) +
    (f.maxPrepTime !== null ? 1 : 0) +
    (f.maxCookTime !== null ? 1 : 0) +
    (f.minServings !== null ? 1 : 0) +
    f.ingredientIds.length
  )
}

const SORT_OPTIONS = [
  { key: 'newest', label: 'Newest', sortBy: RecipeSortBy.DateCreated, sortDescending: true },
  { key: 'name', label: 'Name (A–Z)', sortBy: RecipeSortBy.Name, sortDescending: false },
  { key: 'rating', label: 'Top rated', sortBy: RecipeSortBy.Rating, sortDescending: true },
] as const

type SortKey = (typeof SORT_OPTIONS)[number]['key']
const DEFAULT_SORT: SortKey = 'newest'

export function DashboardScreen({ onOpen, onCreate }: { onOpen: (id: string) => void; onCreate: () => void }) {
  const logout = useAuthStore((s) => s.logout)
  const user = useAuthStore((s) => s.user)
  const initials =
    (user ? `${user.firstName?.[0] ?? ''}${user.lastName?.[0] ?? ''}`.toUpperCase() : '') || '?'

  const [view, setView] = useState<View>({ kind: 'recipes' })
  const [activeCategoryId, setActiveCategoryId] = useState<string | null>(null)
  const [search, setSearch] = useState('')
  const [page, setPage] = useState(1)
  const [favPage, setFavPage] = useState(1)
  const [userMenuOpen, setUserMenuOpen] = useState(false)
  const [filterOpen, setFilterOpen] = useState(false)
  const [filters, setFilters] = useState<Filters>(EMPTY_FILTERS)
  const [sort, setSort] = useState<SortKey>(DEFAULT_SORT)

  const activeFilterCount = countActiveFilters(filters)
  const activeSort = SORT_OPTIONS.find((o) => o.key === sort) ?? SORT_OPTIONS[0]
  const applyFilters = (next: Filters) => {
    setFilters(next)
    setPage(1)
  }
  const applySort = (next: SortKey) => {
    setSort(next)
    setPage(1)
  }

  const debouncedSearch = useDebouncedValue(search)
  const toggleFavorite = useToggleFavorite()
  const onToggleFavorite = (recipe: RecipeSummary) =>
    toggleFavorite.mutate({ recipeId: recipe.id, isFavorite: recipe.isFavorite })

  const { data: categories = [] } = useCategories()
  const { data: collections = [] } = useCollections()
  const { data, isFetching } = useRecipes({
    page,
    pageSize: PAGE_SIZE,
    search: debouncedSearch.trim() || undefined,
    categoryId: activeCategoryId ?? undefined,
    difficulty: filters.difficulty ?? undefined,
    maxPrepTime: filters.maxPrepTime ?? undefined,
    maxCookTime: filters.maxCookTime ?? undefined,
    minServings: filters.minServings ?? undefined,
    ingredientIds: filters.ingredientIds.length ? filters.ingredientIds : undefined,
    sortBy: activeSort.sortBy,
    sortDescending: activeSort.sortDescending,
  })
  const { data: favData, isFetching: favFetching } = useFavorites({ page: favPage, pageSize: PAGE_SIZE })

  const recipes = data?.items ?? []
  const totalPages = data?.totalPages ?? 1
  const totalCount = data?.totalCount ?? 0
  const favCount = favData?.totalCount ?? 0
  const allCount = useMemo(() => categories.reduce((sum, c) => sum + c.recipeCount, 0), [categories])

  const activeCategoryName = activeCategoryId
    ? (categories.find((c) => c.id === activeCategoryId)?.name ?? 'Recipes')
    : 'All'

  const selectCategory = (id: string | null) => {
    setActiveCategoryId(id)
    setPage(1)
    setView({ kind: 'recipes' })
  }

  return (
    <div className="min-h-screen bg-background flex" style={{ fontFamily: "'DM Sans', sans-serif" }}>
      {/* Sidebar */}
      <aside className="hidden md:flex w-60 flex-shrink-0 flex-col bg-sidebar border-r border-sidebar-border min-h-screen sticky top-0 h-screen">
        <div className="p-5 border-b border-sidebar-border">
          <div className="flex items-center gap-2.5">
            <div className="w-7 h-7 bg-[#D94F3A] rounded-lg flex items-center justify-center">
              <Utensils size={14} className="text-white" />
            </div>
            <span
              className="font-semibold text-[#2C1A0E] text-base"
              style={{ fontFamily: "'Playfair Display', serif" }}
            >
              Mealcraft
            </span>
          </div>
        </div>

        <nav className="flex-1 p-4 space-y-1 overflow-y-auto">
          <p className="text-[10px] font-semibold uppercase tracking-widest text-muted-foreground px-2 pt-2 pb-1">
            Categories
          </p>

          <CategoryButton
            label="All"
            active={view.kind === 'recipes' && activeCategoryId === null}
            count={allCount}
            onClick={() => selectCategory(null)}
          />
          {categories.map((cat) => (
            <CategoryButton
              key={cat.id}
              label={cat.name}
              active={view.kind === 'recipes' && activeCategoryId === cat.id}
              count={cat.recipeCount}
              onClick={() => selectCategory(cat.id)}
            />
          ))}

          <p className="text-[10px] font-semibold uppercase tracking-widest text-muted-foreground px-2 pt-5 pb-1">
            Library
          </p>
          <LibraryButton
            icon={<Heart size={15} />}
            label="Favourites"
            count={favCount}
            active={view.kind === 'favorites'}
            onClick={() => setView({ kind: 'favorites' })}
          />
          <LibraryButton
            icon={<Bookmark size={15} />}
            label="Collections"
            count={collections.length}
            active={view.kind === 'collections' || view.kind === 'collection'}
            onClick={() => setView({ kind: 'collections' })}
          />
        </nav>

        <div className="p-4 border-t border-sidebar-border">
          <button
            onClick={() => void logout()}
            className="w-full flex items-center gap-2 px-3 py-2 rounded-xl text-sm text-muted-foreground hover:text-[#D94F3A] hover:bg-rose-50 transition-all"
          >
            <LogOut size={14} />
            Sign out
          </button>
        </div>
      </aside>

      {/* Main content */}
      <div className="flex-1 flex flex-col min-w-0">
        <header className="sticky top-0 z-20 bg-background/90 backdrop-blur-sm border-b border-border px-6 py-3.5 flex items-center gap-4">
          {view.kind === 'recipes' ? (
            <div className="flex-1 relative max-w-sm">
              <Search size={15} className="absolute left-3 top-1/2 -translate-y-1/2 text-muted-foreground" />
              <input
                value={search}
                onChange={(e) => {
                  setSearch(e.target.value)
                  setPage(1)
                }}
                placeholder="Search recipes…"
                className="w-full pl-9 pr-4 py-2 bg-input-background border border-border rounded-xl text-sm focus:outline-none focus:ring-2 focus:ring-[#D94F3A]/25 focus:border-[#D94F3A] transition-all"
              />
            </div>
          ) : (
            <div className="flex-1" />
          )}

          <div className="flex items-center gap-2 ml-auto">
            <button
              onClick={onCreate}
              className="flex items-center gap-1.5 px-4 py-2 bg-[#D94F3A] text-white rounded-xl text-sm font-medium hover:bg-[#C0392B] transition-colors"
            >
              <Plus size={15} />
              <span className="hidden sm:inline">Create Recipe</span>
            </button>

            <button className="relative p-2 rounded-xl hover:bg-muted transition-colors text-muted-foreground">
              <Bell size={17} />
              <span className="absolute top-1.5 right-1.5 w-1.5 h-1.5 bg-[#D94F3A] rounded-full" />
            </button>

            <div className="relative">
              <button
                onClick={() => setUserMenuOpen(!userMenuOpen)}
                className="flex items-center gap-2 pl-1 pr-2 py-1 rounded-xl hover:bg-muted transition-colors"
              >
                <span className="w-7 h-7 rounded-full bg-[#D94F3A] text-white flex items-center justify-center text-xs font-semibold">
                  {initials}
                </span>
                <ChevronDown size={13} className="text-muted-foreground hidden sm:block" />
              </button>
              {userMenuOpen && (
                <div className="absolute right-0 top-full mt-2 w-44 bg-white rounded-xl border border-border shadow-lg py-1 z-30">
                  {[
                    { icon: <User size={13} />, label: 'My Profile', red: false },
                    { icon: <Settings size={13} />, label: 'Settings', red: false },
                    { icon: <LogOut size={13} />, label: 'Sign out', red: true },
                  ].map((item) => (
                    <button
                      key={item.label}
                      onClick={() => {
                        setUserMenuOpen(false)
                        if (item.label === 'Sign out') void logout()
                      }}
                      className={`w-full flex items-center gap-2.5 px-3.5 py-2 text-sm transition-colors ${
                        item.red ? 'text-[#D94F3A] hover:bg-rose-50' : 'text-[#2C1A0E] hover:bg-muted'
                      }`}
                    >
                      {item.icon}
                      {item.label}
                    </button>
                  ))}
                </div>
              )}
            </div>
          </div>
        </header>

        <main className="flex-1 p-6">
          {view.kind === 'recipes' && (
            <>
              <SectionHeading
                title={activeCategoryId === null ? 'All Recipes' : activeCategoryName}
                subtitle={`${totalCount} ${totalCount === 1 ? 'recipe' : 'recipes'} found`}
                filter={{
                  open: filterOpen,
                  activeCount: activeFilterCount,
                  onToggle: () => setFilterOpen((o) => !o),
                }}
              />
              {filterOpen && (
                <FilterPanel
                  filters={filters}
                  activeCount={activeFilterCount}
                  onChange={applyFilters}
                  sort={sort}
                  onSortChange={applySort}
                  onClear={() => {
                    applyFilters(EMPTY_FILTERS)
                    applySort(DEFAULT_SORT)
                  }}
                />
              )}
              <RecipeGrid
                recipes={recipes}
                isFetching={isFetching}
                onOpen={onOpen}
                onToggleFavorite={onToggleFavorite}
                emptyTitle="No recipes found"
                emptySubtitle="Try a different search or category, or create the first one"
              />
              <Pager page={page} totalPages={totalPages} onPage={setPage} />
            </>
          )}

          {view.kind === 'favorites' && (
            <>
              <SectionHeading
                title="Favourites"
                subtitle={`${favCount} ${favCount === 1 ? 'recipe' : 'recipes'} you love`}
              />
              <RecipeGrid
                recipes={favData?.items ?? []}
                isFetching={favFetching}
                onOpen={onOpen}
                onToggleFavorite={onToggleFavorite}
                emptyIcon={<Heart size={40} className="mx-auto mb-3 opacity-30" />}
                emptyTitle="No favourites yet"
                emptySubtitle="Tap the heart on any recipe to save it here"
              />
              <Pager page={favPage} totalPages={favData?.totalPages ?? 1} onPage={setFavPage} />
            </>
          )}

          {view.kind === 'collections' && (
            <CollectionsView onOpenCollection={(id, name) => setView({ kind: 'collection', id, name })} />
          )}

          {view.kind === 'collection' && (
            <CollectionDetailView
              collectionId={view.id}
              onBack={() => setView({ kind: 'collections' })}
              onOpenRecipe={onOpen}
              onToggleFavorite={onToggleFavorite}
            />
          )}
        </main>
      </div>
    </div>
  )
}

function SectionHeading({
  title,
  subtitle,
  filter,
}: {
  title: string
  subtitle: string
  filter?: { open: boolean; activeCount: number; onToggle: () => void }
}) {
  return (
    <div className="mb-6 flex items-end justify-between">
      <div>
        <h1 className="text-2xl font-semibold text-[#2C1A0E]" style={{ fontFamily: "'Playfair Display', serif" }}>
          {title}
        </h1>
        <p className="text-sm text-muted-foreground mt-0.5">{subtitle}</p>
      </div>
      {filter && (
        <button
          onClick={filter.onToggle}
          className={`flex items-center gap-2 px-3 py-2 rounded-xl border text-xs font-medium transition-colors ${
            filter.open || filter.activeCount > 0
              ? 'bg-rose-50 border-[#D94F3A]/30 text-[#D94F3A]'
              : 'border-border text-muted-foreground hover:bg-muted'
          }`}
        >
          <SlidersHorizontal size={14} />
          <span>Filter</span>
          {filter.activeCount > 0 && (
            <span className="min-w-4 h-4 px-1 rounded-full bg-[#D94F3A] text-white text-[10px] font-semibold flex items-center justify-center">
              {filter.activeCount}
            </span>
          )}
        </button>
      )}
    </div>
  )
}

const filterPillClass = (active: boolean) =>
  `px-3 py-1.5 rounded-full text-sm font-medium border transition-colors ${
    active
      ? 'bg-[#D94F3A] text-white border-[#D94F3A]'
      : 'bg-white text-[#2C1A0E] border-border hover:border-[#D94F3A]/40'
  }`

const filterNumberInputClass =
  'w-full px-3 py-2 bg-input-background border border-border rounded-xl text-sm focus:outline-none focus:ring-2 focus:ring-[#D94F3A]/25 focus:border-[#D94F3A] transition-all'

function FilterFieldLabel({ children }: { children: React.ReactNode }) {
  return <p className="text-[11px] font-semibold uppercase tracking-wider text-muted-foreground mb-2">{children}</p>
}

function FilterPanel({
  filters,
  activeCount,
  onChange,
  sort,
  onSortChange,
  onClear,
}: {
  filters: Filters
  activeCount: number
  onChange: (next: Filters) => void
  sort: SortKey
  onSortChange: (next: SortKey) => void
  onClear: () => void
}) {
  const { data: ingredients = [] } = useIngredients()
  const [ingredientSearch, setIngredientSearch] = useState('')

  const set = <K extends keyof Filters>(key: K, value: Filters[K]) => onChange({ ...filters, [key]: value })

  const parseNumber = (raw: string): number | null => {
    if (raw.trim() === '') return null
    const n = Number(raw)
    return Number.isNaN(n) || n < 0 ? null : n
  }
  const numberValue = (v: number | null) => (v === null ? '' : String(v))

  const toggleIngredient = (id: string) =>
    set(
      'ingredientIds',
      filters.ingredientIds.includes(id)
        ? filters.ingredientIds.filter((x) => x !== id)
        : [...filters.ingredientIds, id],
    )

  const visibleIngredients = useMemo(() => {
    const q = ingredientSearch.trim().toLowerCase()
    const list = q ? ingredients.filter((i) => i.name.toLowerCase().includes(q)) : ingredients
    // Always keep already-selected ingredients visible, then fill up to a cap.
    const selected = ingredients.filter((i) => filters.ingredientIds.includes(i.id))
    const rest = list.filter((i) => !filters.ingredientIds.includes(i.id))
    return [...selected, ...rest].slice(0, 30)
  }, [ingredients, ingredientSearch, filters.ingredientIds])

  return (
    <div className="mb-6 rounded-2xl border border-border bg-white p-5 space-y-5">
      {/* Sort */}
      <div>
        <FilterFieldLabel>Sort by</FilterFieldLabel>
        <div className="flex flex-wrap gap-2">
          {SORT_OPTIONS.map((option) => (
            <button
              key={option.key}
              onClick={() => onSortChange(option.key)}
              className={filterPillClass(sort === option.key)}
            >
              {option.label}
            </button>
          ))}
        </div>
      </div>

      {/* Difficulty */}
      <div>
        <FilterFieldLabel>Difficulty</FilterFieldLabel>
        <div className="flex flex-wrap gap-2">
          {[1, 2, 3, 4].map((level) => {
            const active = filters.difficulty === level
            return (
              <button
                key={level}
                onClick={() => set('difficulty', active ? null : level)}
                className={filterPillClass(active)}
              >
                {DifficultyLabel[level]}
              </button>
            )
          })}
        </div>
      </div>

      {/* Numeric bounds */}
      <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
        <div>
          <FilterFieldLabel>Max prep (min)</FilterFieldLabel>
          <input
            type="number"
            min={0}
            placeholder="Any"
            value={numberValue(filters.maxPrepTime)}
            onChange={(e) => set('maxPrepTime', parseNumber(e.target.value))}
            className={filterNumberInputClass}
          />
        </div>
        <div>
          <FilterFieldLabel>Max cook (min)</FilterFieldLabel>
          <input
            type="number"
            min={0}
            placeholder="Any"
            value={numberValue(filters.maxCookTime)}
            onChange={(e) => set('maxCookTime', parseNumber(e.target.value))}
            className={filterNumberInputClass}
          />
        </div>
        <div>
          <FilterFieldLabel>Min servings</FilterFieldLabel>
          <input
            type="number"
            min={1}
            placeholder="Any"
            value={numberValue(filters.minServings)}
            onChange={(e) => set('minServings', parseNumber(e.target.value))}
            className={filterNumberInputClass}
          />
        </div>
      </div>

      {/* Ingredients */}
      <div>
        <FilterFieldLabel>Ingredients (recipe must contain all selected)</FilterFieldLabel>
        <div className="relative max-w-xs mb-3">
          <Search size={14} className="absolute left-3 top-1/2 -translate-y-1/2 text-muted-foreground" />
          <input
            value={ingredientSearch}
            onChange={(e) => setIngredientSearch(e.target.value)}
            placeholder="Search ingredients…"
            className="w-full pl-8 pr-3 py-2 bg-input-background border border-border rounded-xl text-sm focus:outline-none focus:ring-2 focus:ring-[#D94F3A]/25 focus:border-[#D94F3A] transition-all"
          />
        </div>
        <div className="flex flex-wrap gap-2 max-h-44 overflow-y-auto">
          {visibleIngredients.length > 0 ? (
            visibleIngredients.map((ing) => (
              <button
                key={ing.id}
                onClick={() => toggleIngredient(ing.id)}
                className={filterPillClass(filters.ingredientIds.includes(ing.id))}
              >
                {ing.name}
              </button>
            ))
          ) : (
            <p className="text-xs text-muted-foreground">No ingredients match your search.</p>
          )}
        </div>
      </div>

      {/* Footer */}
      <div className="flex items-center justify-between border-t border-border pt-4">
        <span className="text-xs text-muted-foreground">
          {activeCount === 0 ? 'No filters applied' : `${activeCount} ${activeCount === 1 ? 'filter' : 'filters'} applied`}
        </span>
        <button
          onClick={onClear}
          disabled={activeCount === 0}
          className="flex items-center gap-1.5 text-sm font-medium text-muted-foreground hover:text-[#D94F3A] transition-colors disabled:opacity-40 disabled:cursor-not-allowed"
        >
          <X size={14} />
          Clear all
        </button>
      </div>
    </div>
  )
}

function RecipeGrid({
  recipes,
  isFetching,
  onOpen,
  onToggleFavorite,
  emptyTitle,
  emptySubtitle,
  emptyIcon,
}: {
  recipes: RecipeSummary[]
  isFetching: boolean
  onOpen: (id: string) => void
  onToggleFavorite: (recipe: RecipeSummary) => void
  emptyTitle: string
  emptySubtitle: string
  emptyIcon?: React.ReactNode
}) {
  return (
    <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-5">
      {isFetching && recipes.length === 0 ? (
        Array.from({ length: 6 }).map((_, i) => <SkeletonCard key={i} />)
      ) : recipes.length > 0 ? (
        recipes.map((recipe) => (
          <RecipeCard
            key={recipe.id}
            recipe={recipe}
            onOpen={() => onOpen(recipe.id)}
            onToggleFavorite={onToggleFavorite}
          />
        ))
      ) : (
        <div className="col-span-full text-center py-20 text-muted-foreground">
          {emptyIcon ?? <Utensils size={40} className="mx-auto mb-3 opacity-30" />}
          <p className="font-medium">{emptyTitle}</p>
          <p className="text-sm mt-1">{emptySubtitle}</p>
        </div>
      )}
    </div>
  )
}

function Pager({ page, totalPages, onPage }: { page: number; totalPages: number; onPage: (p: number) => void }) {
  if (totalPages <= 1) return null
  return (
    <div className="flex items-center justify-center gap-2 mt-10">
      <button
        onClick={() => onPage(Math.max(1, page - 1))}
        disabled={page === 1}
        className="p-2 rounded-lg border border-border text-muted-foreground hover:bg-muted disabled:opacity-40 disabled:cursor-not-allowed transition-colors"
      >
        <ChevronLeft size={15} />
      </button>
      {Array.from({ length: totalPages }, (_, i) => i + 1).map((p) => (
        <button
          key={p}
          onClick={() => onPage(p)}
          className={`w-8 h-8 rounded-lg text-sm font-medium transition-colors ${
            p === page ? 'bg-[#D94F3A] text-white' : 'border border-border text-muted-foreground hover:bg-muted'
          }`}
        >
          {p}
        </button>
      ))}
      <button
        onClick={() => onPage(Math.min(totalPages, page + 1))}
        disabled={page === totalPages}
        className="p-2 rounded-lg border border-border text-muted-foreground hover:bg-muted disabled:opacity-40 disabled:cursor-not-allowed transition-colors"
      >
        <ChevronRight size={15} />
      </button>
    </div>
  )
}

function CollectionsView({ onOpenCollection }: { onOpenCollection: (id: string, name: string) => void }) {
  const { data: collections = [], isLoading } = useCollections()
  const createCollection = useCreateCollection()
  const deleteCollection = useDeleteCollection()

  const [creating, setCreating] = useState(false)
  const [name, setName] = useState('')

  const handleCreate = () => {
    const trimmed = name.trim()
    if (!trimmed) return
    createCollection.mutate(
      { name: trimmed, description: null },
      {
        onSuccess: () => {
          setName('')
          setCreating(false)
        },
      },
    )
  }

  const handleDelete = (id: string, collectionName: string) => {
    if (!window.confirm(`Delete the collection “${collectionName}”? The recipes themselves are not deleted.`)) return
    deleteCollection.mutate(id)
  }

  return (
    <>
      <div className="mb-6 flex items-end justify-between">
        <div>
          <h1 className="text-2xl font-semibold text-[#2C1A0E]" style={{ fontFamily: "'Playfair Display', serif" }}>
            Collections
          </h1>
          <p className="text-sm text-muted-foreground mt-0.5">
            {collections.length} {collections.length === 1 ? 'collection' : 'collections'}
          </p>
        </div>
        <button
          onClick={() => setCreating((c) => !c)}
          className="flex items-center gap-1.5 px-4 py-2 bg-[#D94F3A] text-white rounded-xl text-sm font-medium hover:bg-[#C0392B] transition-colors"
        >
          <FolderPlus size={15} />
          New collection
        </button>
      </div>

      {creating && (
        <div className="mb-6 flex items-center gap-2 max-w-md">
          <input
            autoFocus
            value={name}
            onChange={(e) => setName(e.target.value)}
            onKeyDown={(e) => {
              if (e.key === 'Enter') handleCreate()
              if (e.key === 'Escape') setCreating(false)
            }}
            placeholder="Collection name"
            className="flex-1 px-3.5 py-2 bg-input-background border border-border rounded-xl text-sm focus:outline-none focus:ring-2 focus:ring-[#D94F3A]/25 focus:border-[#D94F3A]"
          />
          <button
            onClick={handleCreate}
            disabled={!name.trim() || createCollection.isPending}
            className="px-4 py-2 rounded-xl bg-[#D94F3A] text-white text-sm font-medium hover:bg-[#C0392B] transition-colors disabled:opacity-50"
          >
            Create
          </button>
        </div>
      )}

      {isLoading ? (
        <p className="text-sm text-muted-foreground">Loading…</p>
      ) : collections.length > 0 ? (
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-5">
          {collections.map((c) => (
            <div
              key={c.id}
              onClick={() => onOpenCollection(c.id, c.name)}
              className="bg-white rounded-2xl border border-border p-5 cursor-pointer group transition-all duration-200 hover:-translate-y-1 hover:shadow-lg hover:shadow-[rgba(44,26,14,0.08)]"
            >
              <div className="flex items-start justify-between gap-2">
                <div className="w-10 h-10 rounded-xl bg-rose-50 flex items-center justify-center text-[#D94F3A]">
                  <Bookmark size={18} />
                </div>
                <button
                  aria-label="Delete collection"
                  onClick={(e) => {
                    e.stopPropagation()
                    handleDelete(c.id, c.name)
                  }}
                  className="p-1.5 rounded-lg text-muted-foreground opacity-0 group-hover:opacity-100 hover:text-red-600 hover:bg-red-50 transition-all"
                >
                  <Trash2 size={15} />
                </button>
              </div>
              <h3
                className="font-semibold text-[#2C1A0E] text-base mt-3 leading-snug"
                style={{ fontFamily: "'Playfair Display', serif" }}
              >
                {c.name}
              </h3>
              {c.description && <p className="text-xs text-muted-foreground mt-1 line-clamp-2">{c.description}</p>}
              <p className="text-xs text-muted-foreground mt-3">
                {c.recipeCount} {c.recipeCount === 1 ? 'recipe' : 'recipes'}
              </p>
            </div>
          ))}
        </div>
      ) : (
        <div className="text-center py-20 text-muted-foreground">
          <Bookmark size={40} className="mx-auto mb-3 opacity-30" />
          <p className="font-medium">No collections yet</p>
          <p className="text-sm mt-1">Create a collection to group recipes however you like</p>
        </div>
      )}
    </>
  )
}

function CollectionDetailView({
  collectionId,
  onBack,
  onOpenRecipe,
  onToggleFavorite,
}: {
  collectionId: string
  onBack: () => void
  onOpenRecipe: (id: string) => void
  onToggleFavorite: (recipe: RecipeSummary) => void
}) {
  const { data: collection, isLoading, isError } = useCollection(collectionId)
  const removeRecipe = useRemoveRecipeFromCollection()

  if (isLoading) return <p className="text-sm text-muted-foreground">Loading…</p>
  if (isError || !collection)
    return (
      <div className="text-center py-20 text-muted-foreground">
        <p className="font-medium text-[#2C1A0E]">This collection could not be loaded.</p>
        <button onClick={onBack} className="mt-3 text-sm text-[#D94F3A] font-medium hover:underline">
          Back to collections
        </button>
      </div>
    )

  const recipes = collection.recipes

  return (
    <>
      <button
        onClick={onBack}
        className="flex items-center gap-1.5 text-sm text-muted-foreground hover:text-[#2C1A0E] transition-colors mb-4"
      >
        <ChevronLeft size={15} />
        Collections
      </button>

      <div className="mb-6">
        <h1 className="text-2xl font-semibold text-[#2C1A0E]" style={{ fontFamily: "'Playfair Display', serif" }}>
          {collection.name}
        </h1>
        {collection.description && <p className="text-sm text-muted-foreground mt-1">{collection.description}</p>}
        <p className="text-sm text-muted-foreground mt-0.5">
          {recipes.length} {recipes.length === 1 ? 'recipe' : 'recipes'}
        </p>
      </div>

      {recipes.length > 0 ? (
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-5">
          {recipes.map((recipe) => (
            <div key={recipe.id} className="relative group">
              <RecipeCard
                recipe={recipe}
                onOpen={() => onOpenRecipe(recipe.id)}
                onToggleFavorite={onToggleFavorite}
              />
              <button
                aria-label="Remove from collection"
                onClick={() => removeRecipe.mutate({ collectionId, recipeId: recipe.id })}
                disabled={removeRecipe.isPending}
                className="absolute top-3 left-3 p-1.5 rounded-full bg-white/90 text-[#8C6F5E] backdrop-blur-sm opacity-0 group-hover:opacity-100 hover:text-red-600 transition-all disabled:opacity-50"
              >
                <Trash2 size={14} />
              </button>
            </div>
          ))}
        </div>
      ) : (
        <div className="text-center py-20 text-muted-foreground">
          <Bookmark size={40} className="mx-auto mb-3 opacity-30" />
          <p className="font-medium">This collection is empty</p>
          <p className="text-sm mt-1">Open a recipe and use “Save to Collection” to add it here</p>
        </div>
      )}
    </>
  )
}

function CategoryButton({
  label,
  active,
  count,
  onClick,
}: {
  label: string
  active: boolean
  count: number
  onClick: () => void
}) {
  const { icon } = categoryVisual(label)
  return (
    <button
      onClick={onClick}
      className={`w-full flex items-center gap-2.5 px-3 py-2.5 rounded-xl text-sm font-medium transition-all duration-150 ${
        active ? 'bg-[#D94F3A] text-white shadow-sm' : 'text-[#2C1A0E] hover:bg-sidebar-accent'
      }`}
    >
      <span className={active ? 'text-white' : 'text-muted-foreground'}>{icon}</span>
      {label}
      <span
        className={`ml-auto text-xs rounded-full px-1.5 py-0.5 ${
          active ? 'bg-white/20 text-white' : 'bg-muted text-muted-foreground'
        }`}
      >
        {count}
      </span>
    </button>
  )
}

function LibraryButton({
  icon,
  label,
  count,
  active,
  onClick,
}: {
  icon: React.ReactNode
  label: string
  count: number
  active: boolean
  onClick: () => void
}) {
  return (
    <button
      onClick={onClick}
      className={`w-full flex items-center gap-2.5 px-3 py-2.5 rounded-xl text-sm font-medium transition-all ${
        active ? 'bg-[#D94F3A] text-white shadow-sm' : 'text-[#2C1A0E] hover:bg-sidebar-accent'
      }`}
    >
      <span className={active ? 'text-white' : 'text-muted-foreground'}>{icon}</span>
      {label}
      <span
        className={`ml-auto text-xs rounded-full px-1.5 py-0.5 ${
          active ? 'bg-white/20 text-white' : 'bg-muted text-muted-foreground'
        }`}
      >
        {count}
      </span>
    </button>
  )
}
