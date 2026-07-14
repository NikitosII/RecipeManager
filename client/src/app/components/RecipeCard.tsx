import { Clock, Heart, Users, Utensils } from 'lucide-react'
import type { RecipeSummary } from '@/types/api'
import { resolveMediaUrl } from '@/config'
import { CategoryBadge, RatingSummary } from './ui-bits'

export function RecipeCard({
  recipe,
  onOpen,
  onToggleFavorite,
}: {
  recipe: RecipeSummary
  onOpen: () => void
  onToggleFavorite?: (recipe: RecipeSummary) => void
}) {
  const favorite = recipe.isFavorite
  const image = resolveMediaUrl(recipe.imageUrl)

  return (
    <div
      className="bg-white rounded-2xl overflow-hidden border border-border group cursor-pointer transition-all duration-300 hover:-translate-y-1 hover:shadow-xl hover:shadow-[rgba(44,26,14,0.1)]"
      onClick={onOpen}
    >
      <div className="relative h-44 bg-muted overflow-hidden">
        {image ? (
          <img
            src={image}
            alt={recipe.title}
            className="w-full h-full object-cover transition-transform duration-500 group-hover:scale-105"
          />
        ) : (
          <div className="w-full h-full flex items-center justify-center text-muted-foreground/40">
            <Utensils size={32} />
          </div>
        )}
        {onToggleFavorite && (
          <button
            aria-label={favorite ? 'Remove from favourites' : 'Add to favourites'}
            onClick={(e) => {
              e.stopPropagation()
              onToggleFavorite(recipe)
            }}
            className={`absolute top-3 right-3 p-1.5 rounded-full backdrop-blur-sm transition-all duration-200 ${
              favorite ? 'bg-[#D94F3A] text-white' : 'bg-white/80 text-[#8C6F5E] hover:bg-white'
            }`}
          >
            <Heart size={14} fill={favorite ? 'currentColor' : 'none'} />
          </button>
        )}
        <div className="absolute top-3 left-3">
          <CategoryBadge category={recipe.categoryName} small />
        </div>
      </div>
      <div className="p-4">
        <div className="flex items-start justify-between gap-2 mb-2">
          <h3
            className="font-semibold text-[#2C1A0E] text-sm leading-snug line-clamp-2 flex-1"
            style={{ fontFamily: "'Playfair Display', serif" }}
          >
            {recipe.title}
          </h3>
          <RatingSummary average={recipe.averageRating} count={recipe.ratingCount} />
        </div>
        <p className="text-xs text-muted-foreground mb-3">by {recipe.authorName}</p>
        <div className="flex items-center gap-3 text-xs text-muted-foreground">
          <span className="flex items-center gap-1">
            <Clock size={11} /> {recipe.prepTimeMinutes} min prep
          </span>
          <span className="flex items-center gap-1">
            <Users size={11} /> {recipe.servings} servings
          </span>
        </div>
      </div>
    </div>
  )
}
