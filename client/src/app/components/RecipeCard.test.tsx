import { fireEvent, render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'
import { RecipeCard } from './RecipeCard'
import type { RecipeSummary } from '@/types/api'

const recipe: RecipeSummary = {
  id: '019f4db1-1bc9-7228-9591-3516977526bd',
  title: 'Classic Pancakes',
  description: 'Fluffy',
  difficultyLevel: 1,
  prepTimeMinutes: 10,
  cookTimeMinutes: 15,
  servings: 4,
  imageUrl: null,
  categoryName: 'Breakfast',
  authorName: 'Ada Lovelace',
  isFavorite: false,
  averageRating: 4.5,
  ratingCount: 2,
  userRating: null,
  dateCreated: new Date().toISOString(),
}

describe('RecipeCard', () => {
  it('renders the recipe title and key stats', () => {
    render(<RecipeCard recipe={recipe} onOpen={() => {}} />)

    expect(screen.getByText('Classic Pancakes')).toBeInTheDocument()
    expect(screen.getByText(/by Ada Lovelace/)).toBeInTheDocument()
    expect(screen.getByText(/10 min prep/)).toBeInTheDocument()
    expect(screen.getByText(/4 servings/)).toBeInTheDocument()
  })

  it('shows the average rating and count', () => {
    render(<RecipeCard recipe={recipe} onOpen={() => {}} />)

    expect(screen.getByText(/4\.5/)).toBeInTheDocument()
    expect(screen.getByText(/\(2\)/)).toBeInTheDocument()
  })

  it('shows "New" when a recipe has no ratings', () => {
    render(<RecipeCard recipe={{ ...recipe, averageRating: 0, ratingCount: 0 }} onOpen={() => {}} />)

    expect(screen.getByText('New')).toBeInTheDocument()
  })

  it('calls onOpen when the card is clicked', () => {
    const onOpen = vi.fn()
    render(<RecipeCard recipe={recipe} onOpen={onOpen} />)

    fireEvent.click(screen.getByText('Classic Pancakes'))

    expect(onOpen).toHaveBeenCalledOnce()
  })

  it('toggles the favourite without triggering onOpen', () => {
    const onOpen = vi.fn()
    const onToggleFavorite = vi.fn()
    render(<RecipeCard recipe={recipe} onOpen={onOpen} onToggleFavorite={onToggleFavorite} />)

    fireEvent.click(screen.getByLabelText('Add to favourites'))

    expect(onToggleFavorite).toHaveBeenCalledWith(recipe)
    expect(onOpen).not.toHaveBeenCalled()
  })

  it('renders no favourite button when no toggle handler is provided', () => {
    const { container } = render(<RecipeCard recipe={recipe} onOpen={() => {}} />)

    expect(container.querySelector('button')).toBeNull()
  })
})
