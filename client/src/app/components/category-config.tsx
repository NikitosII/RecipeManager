import type { ReactNode } from 'react'
import { Clock, Coffee, Flame, Leaf, Soup, Star, Utensils } from 'lucide-react'

export interface CategoryVisual {
  icon: ReactNode
  color: string
}

// Keyed by lowercased category name. Falls back to a neutral style for any
// category the design didn't anticipate (e.g. user-created categories).
const BY_NAME: Record<string, CategoryVisual> = {
  all: { icon: <Utensils size={15} />, color: 'bg-[#2C1A0E] text-[#FAF7F2]' },
  breakfast: { icon: <Coffee size={15} />, color: 'bg-amber-100 text-amber-800' },
  lunch: { icon: <Soup size={15} />, color: 'bg-lime-100 text-lime-800' },
  dinner: { icon: <Flame size={15} />, color: 'bg-orange-100 text-orange-800' },
  vegan: { icon: <Leaf size={15} />, color: 'bg-green-100 text-green-800' },
  dessert: { icon: <Star size={15} />, color: 'bg-rose-100 text-rose-800' },
  quick: { icon: <Clock size={15} />, color: 'bg-sky-100 text-sky-800' },
}

const DEFAULT: CategoryVisual = { icon: <Utensils size={15} />, color: 'bg-gray-100 text-gray-700' }

export function categoryVisual(name: string): CategoryVisual {
  return BY_NAME[name.toLowerCase()] ?? DEFAULT
}
