import { Star } from 'lucide-react'
import { categoryVisual } from './category-config'

export function CategoryBadge({ category, small = false }: { category: string; small?: boolean }) {
  const { icon, color } = categoryVisual(category)
  return (
    <span
      className={`inline-flex items-center gap-1 rounded-full font-medium ${
        small ? 'px-2 py-0.5 text-xs' : 'px-2.5 py-1 text-xs'
      } ${color}`}
    >
      {icon}
      {category}
    </span>
  )
}

export function StarRating({ rating }: { rating: number }) {
  return (
    <span className="flex items-center gap-1 text-amber-500">
      <Star size={13} fill="currentColor" />
      <span className="text-xs font-medium text-[#2C1A0E]">{rating.toFixed(1)}</span>
    </span>
  )
}

export function SkeletonCard() {
  return (
    <div className="bg-white rounded-2xl overflow-hidden border border-border animate-pulse">
      <div className="h-44 bg-muted" />
      <div className="p-4 space-y-3">
        <div className="h-3 bg-muted rounded-full w-1/3" />
        <div className="h-4 bg-muted rounded-full w-4/5" />
        <div className="h-4 bg-muted rounded-full w-3/5" />
        <div className="flex gap-3 pt-1">
          <div className="h-3 bg-muted rounded-full w-16" />
          <div className="h-3 bg-muted rounded-full w-16" />
        </div>
      </div>
    </div>
  )
}
