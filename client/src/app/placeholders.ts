function hash(input: string): number {
  let h = 0
  for (let i = 0; i < input.length; i++) {
    h = (h << 5) - h + input.charCodeAt(i)
    h |= 0
  }
  return Math.abs(h)
}

export interface Decoration {
  rating: number
  initialFavorite: boolean
}

export function decorationFor(id: string): Decoration {
  const h = hash(id)
  return {
    rating: Math.round((4.3 + (h % 7) / 10) * 10) / 10, // 4.3 – 4.9
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
