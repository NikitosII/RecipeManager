import { useMemo, useState } from 'react'
import {
  Bell,
  Bookmark,
  ChevronDown,
  ChevronLeft,
  ChevronRight,
  Heart,
  LogOut,
  Plus,
  Search,
  Settings,
  SlidersHorizontal,
  User,
  Utensils,
} from 'lucide-react'
import { useCategories } from '@/hooks/use-catalog'
import { useRecipes } from '@/hooks/use-recipes'
import { useDebouncedValue } from '@/hooks/use-debounced-value'
import { useAuthStore } from '@/stores/auth-store'
import { CURRENT_USER_AVATAR } from '../placeholders'
import { categoryVisual } from '../components/category-config'
import { RecipeCard } from '../components/RecipeCard'
import { SkeletonCard } from '../components/ui-bits'

const PAGE_SIZE = 9

export function DashboardScreen({ onOpen, onCreate }: { onOpen: (id: string) => void; onCreate: () => void }) {
  const logout = useAuthStore((s) => s.logout)

  const [activeCategoryId, setActiveCategoryId] = useState<string | null>(null)
  const [search, setSearch] = useState('')
  const [page, setPage] = useState(1)
  const [userMenuOpen, setUserMenuOpen] = useState(false)

  const debouncedSearch = useDebouncedValue(search)

  const { data: categories = [] } = useCategories()
  const { data, isFetching } = useRecipes({
    page,
    pageSize: PAGE_SIZE,
    search: debouncedSearch.trim() || undefined,
    categoryId: activeCategoryId ?? undefined,
  })

  const recipes = data?.items ?? []
  const totalPages = data?.totalPages ?? 1
  const totalCount = data?.totalCount ?? 0
  const allCount = useMemo(() => categories.reduce((sum, c) => sum + c.recipeCount, 0), [categories])

  const activeCategoryName = activeCategoryId
    ? (categories.find((c) => c.id === activeCategoryId)?.name ?? 'Recipes')
    : 'All'

  const selectCategory = (id: string | null) => {
    setActiveCategoryId(id)
    setPage(1)
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
            active={activeCategoryId === null}
            count={allCount}
            onClick={() => selectCategory(null)}
          />
          {categories.map((cat) => (
            <CategoryButton
              key={cat.id}
              label={cat.name}
              active={activeCategoryId === cat.id}
              count={cat.recipeCount}
              onClick={() => selectCategory(cat.id)}
            />
          ))}

          <p className="text-[10px] font-semibold uppercase tracking-widest text-muted-foreground px-2 pt-5 pb-1">
            Library
          </p>
          {[
            { icon: <Heart size={15} />, label: 'Favourites', count: 0 },
            { icon: <Bookmark size={15} />, label: 'Collections', count: 3 },
          ].map((item) => (
            <button
              key={item.label}
              className="w-full flex items-center gap-2.5 px-3 py-2.5 rounded-xl text-sm font-medium text-[#2C1A0E] hover:bg-sidebar-accent transition-all"
            >
              <span className="text-muted-foreground">{item.icon}</span>
              {item.label}
              <span className="ml-auto text-xs bg-muted text-muted-foreground rounded-full px-1.5 py-0.5">
                {item.count}
              </span>
            </button>
          ))}
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
                <img src={CURRENT_USER_AVATAR} alt="User" className="w-7 h-7 rounded-full object-cover" />
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
          <div className="mb-6 flex items-end justify-between">
            <div>
              <h1
                className="text-2xl font-semibold text-[#2C1A0E]"
                style={{ fontFamily: "'Playfair Display', serif" }}
              >
                {activeCategoryId === null ? 'All Recipes' : activeCategoryName}
              </h1>
              <p className="text-sm text-muted-foreground mt-0.5">
                {totalCount} {totalCount === 1 ? 'recipe' : 'recipes'} found
              </p>
            </div>
            <div className="flex items-center gap-2 text-muted-foreground">
              <SlidersHorizontal size={14} />
              <span className="text-xs">Filter</span>
            </div>
          </div>

          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-5">
            {isFetching && recipes.length === 0 ? (
              Array.from({ length: 6 }).map((_, i) => <SkeletonCard key={i} />)
            ) : recipes.length > 0 ? (
              recipes.map((recipe) => <RecipeCard key={recipe.id} recipe={recipe} onOpen={() => onOpen(recipe.id)} />)
            ) : (
              <div className="col-span-full text-center py-20 text-muted-foreground">
                <Utensils size={40} className="mx-auto mb-3 opacity-30" />
                <p className="font-medium">No recipes found</p>
                <p className="text-sm mt-1">Try a different search or category, or create the first one</p>
              </div>
            )}
          </div>

          {totalPages > 1 && (
            <div className="flex items-center justify-center gap-2 mt-10">
              <button
                onClick={() => setPage((p) => Math.max(1, p - 1))}
                disabled={page === 1}
                className="p-2 rounded-lg border border-border text-muted-foreground hover:bg-muted disabled:opacity-40 disabled:cursor-not-allowed transition-colors"
              >
                <ChevronLeft size={15} />
              </button>
              {Array.from({ length: totalPages }, (_, i) => i + 1).map((p) => (
                <button
                  key={p}
                  onClick={() => setPage(p)}
                  className={`w-8 h-8 rounded-lg text-sm font-medium transition-colors ${
                    p === page ? 'bg-[#D94F3A] text-white' : 'border border-border text-muted-foreground hover:bg-muted'
                  }`}
                >
                  {p}
                </button>
              ))}
              <button
                onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
                disabled={page === totalPages}
                className="p-2 rounded-lg border border-border text-muted-foreground hover:bg-muted disabled:opacity-40 disabled:cursor-not-allowed transition-colors"
              >
                <ChevronRight size={15} />
              </button>
            </div>
          )}
        </main>
      </div>
    </div>
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
