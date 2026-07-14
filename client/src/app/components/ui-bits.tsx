import { useState } from 'react'
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

/** Read-only average rating with its count (shows "New" when nothing is rated yet). */
export function RatingSummary({
  average,
  count,
  light = false,
}: {
  average: number
  count: number
  light?: boolean
}) {
  return (
    <span className="flex items-center gap-1 text-amber-500">
      <Star size={13} fill="currentColor" />
      {count > 0 ? (
        <span className={`text-xs font-medium ${light ? 'text-white' : 'text-[#2C1A0E]'}`}>
          {average.toFixed(1)}
          <span className={light ? 'text-white/70' : 'text-muted-foreground'}> ({count})</span>
        </span>
      ) : (
        <span className={`text-xs font-medium ${light ? 'text-white/70' : 'text-muted-foreground'}`}>New</span>
      )}
    </span>
  )
}

/** Interactive 1–5 star input. `value` is the current user's rating (null if none). */
export function RatingStars({
  value,
  onRate,
  disabled = false,
  size = 22,
}: {
  value: number | null
  onRate: (value: number) => void
  disabled?: boolean
  size?: number
}) {
  const [hover, setHover] = useState<number | null>(null)
  const active = hover ?? value ?? 0

  return (
    <div className="flex items-center gap-0.5" onMouseLeave={() => setHover(null)}>
      {[1, 2, 3, 4, 5].map((n) => (
        <button
          key={n}
          type="button"
          disabled={disabled}
          aria-label={`Rate ${n} star${n > 1 ? 's' : ''}`}
          onMouseEnter={() => setHover(n)}
          onClick={() => onRate(n)}
          className="p-0.5 transition-transform hover:scale-110 disabled:cursor-not-allowed disabled:opacity-60"
        >
          <Star
            size={size}
            className={n <= active ? 'text-amber-500' : 'text-muted-foreground/30'}
            fill={n <= active ? 'currentColor' : 'none'}
          />
        </button>
      ))}
    </div>
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
