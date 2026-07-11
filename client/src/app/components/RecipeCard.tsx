import { useState } from 'react'
import { Clock, Heart, Users } from 'lucide-react'
import type { RecipeSummary } from '@/types/api'
import { resolveMediaUrl } from '@/config'
import { decorationFor } from '../placeholders'
import { CategoryBadge, StarRating } from './ui-bits'

export function RecipeCard({ recipe, onOpen }: { recipe: RecipeSummary; onOpen: () => void }) {
  const deco = decorationFor(recipe.id)
  // Favourites have no backend — this toggle is local visual decoration only.
  const [favorite, setFavorite] = useState(deco.initialFavorite)
  const image = resolveMediaUrl(recipe.imageUrl) ?? deco.fallbackImage

  return (
    <div
      className="bg-white rounded-2xl overflow-hidden border border-border group cursor-pointer transition-all duration-300 hover:-translate-y-1 hover:shadow-xl hover:shadow-[rgba(44,26,14,0.1)]"
      onClick={onOpen}
    >
      <div className="relative h-44 bg-muted overflow-hidden">
        <img
          src={image}
          alt={recipe.title}
          className="w-full h-full object-cover transition-transform duration-500 group-hover:scale-105"
        />
        <button
          onClick={(e) => {
            e.stopPropagation()
            setFavorite((f) => !f)
          }}
          className={`absolute top-3 right-3 p-1.5 rounded-full backdrop-blur-sm transition-all duration-200 ${
            favorite ? 'bg-[#D94F3A] text-white' : 'bg-white/80 text-[#8C6F5E] hover:bg-white'
          }`}
        >
          <Heart size={14} fill={favorite ? 'currentColor' : 'none'} />
        </button>
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
          <StarRating rating={deco.rating} />
        </div>
        <p className="text-xs text-muted-foreground mb-3">by {deco.author.name}</p>
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
