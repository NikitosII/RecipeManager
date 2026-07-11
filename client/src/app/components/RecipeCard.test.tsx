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
  dateCreated: new Date().toISOString(),
}

describe('RecipeCard', () => {
  it('renders the recipe title and key stats', () => {
    render(<RecipeCard recipe={recipe} onOpen={() => {}} />)

    expect(screen.getByText('Classic Pancakes')).toBeInTheDocument()
    expect(screen.getByText(/10 min prep/)).toBeInTheDocument()
    expect(screen.getByText(/4 servings/)).toBeInTheDocument()
  })

  it('calls onOpen when the card is clicked', () => {
    const onOpen = vi.fn()
    render(<RecipeCard recipe={recipe} onOpen={onOpen} />)

    fireEvent.click(screen.getByText('Classic Pancakes'))

    expect(onOpen).toHaveBeenCalledOnce()
  })

  it('does not trigger onOpen when toggling the (decorative) favourite', () => {
    const onOpen = vi.fn()
    const { container } = render(<RecipeCard recipe={recipe} onOpen={onOpen} />)

    // The heart button is the only <button> in the card.
    fireEvent.click(container.querySelector('button')!)

    expect(onOpen).not.toHaveBeenCalled()
  })
})
