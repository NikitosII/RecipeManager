/*
 * Static decoration for data the backend does not store (author, avatar, rating, nutrition, cover-image fallback). 
 */

function hash(input: string): number {
  let h = 0
  for (let i = 0; i < input.length; i++) {
    h = (h << 5) - h + input.charCodeAt(i)
    h |= 0
  }
  return Math.abs(h)
}

const AUTHORS = [
  { name: 'Elena Morin', avatar: 'https://images.unsplash.com/photo-1438761681033-6461ffad8d80?w=80&h=80&fit=crop&auto=format' },
  { name: 'Marco Vitelli', avatar: 'https://images.unsplash.com/photo-1472099645785-5658abf4ff4e?w=80&h=80&fit=crop&auto=format' },
  { name: 'Priya Nair', avatar: 'https://images.unsplash.com/photo-1494790108377-be9c29b29330?w=80&h=80&fit=crop&auto=format' },
  { name: 'Sophie Blanchard', avatar: 'https://images.unsplash.com/photo-1544005313-94ddf0286df2?w=80&h=80&fit=crop&auto=format' },
  { name: 'Jake Osei', avatar: 'https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=80&h=80&fit=crop&auto=format' },
  { name: 'Yuki Tanaka', avatar: 'https://images.unsplash.com/photo-1527980965255-d3b416303d12?w=80&h=80&fit=crop&auto=format' },
]

const FALLBACK_IMAGES = [
  'https://images.unsplash.com/photo-1547592180-85f173990554?w=700&h=460&fit=crop&auto=format',
  'https://images.unsplash.com/photo-1476224203421-9ac39bcb3327?w=700&h=460&fit=crop&auto=format',
  'https://images.unsplash.com/photo-1512621776951-a57141f2eefd?w=700&h=460&fit=crop&auto=format',
  'https://images.unsplash.com/photo-1470124182917-cc6e71b22ecc?w=700&h=460&fit=crop&auto=format',
  'https://images.unsplash.com/photo-1484723091739-30a097e8f929?w=700&h=460&fit=crop&auto=format',
  'https://images.unsplash.com/photo-1569718212165-3a8278d5f624?w=700&h=460&fit=crop&auto=format',
]

export interface Decoration {
  author: { name: string; avatar: string }
  rating: number
  fallbackImage: string
  initialFavorite: boolean
}

export function decorationFor(id: string): Decoration {
  const h = hash(id)
  return {
    author: AUTHORS[h % AUTHORS.length],
    rating: Math.round((4.3 + (h % 7) / 10) * 10) / 10, // 4.3 – 4.9
    fallbackImage: FALLBACK_IMAGES[h % FALLBACK_IMAGES.length],
    initialFavorite: h % 3 === 0,
  }
}

export const PLACEHOLDER_NUTRITION = {
  calories: 420,
  protein: '18g',
  carbs: '32g',
  fat: '20g',
  fiber: '5g',
}

export const CURRENT_USER_AVATAR =
  'https://images.unsplash.com/photo-1438761681033-6461ffad8d80?w=80&h=80&fit=crop&auto=format'
