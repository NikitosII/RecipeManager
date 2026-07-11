import { useEffect, useState } from 'react'
import { useAuthStore } from '@/stores/auth-store'
import { AuthScreen } from './screens/AuthScreen'
import { DashboardScreen } from './screens/DashboardScreen'
import { DetailScreen } from './screens/DetailScreen'
import { CreateScreen } from './screens/CreateScreen'

type Screen = { name: 'dashboard' } | { name: 'detail'; id: string } | { name: 'create' }

export default function App() {
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated)
  const [screen, setScreen] = useState<Screen>({ name: 'dashboard' })

  useEffect(() => {
    if (isAuthenticated) setScreen({ name: 'dashboard' })
  }, [isAuthenticated])

  if (!isAuthenticated) return <AuthScreen />

  if (screen.name === 'detail') {
    return <DetailScreen recipeId={screen.id} onBack={() => setScreen({ name: 'dashboard' })} />
  }

  if (screen.name === 'create') {
    return <CreateScreen onBack={() => setScreen({ name: 'dashboard' })} />
  }

  return (
    <DashboardScreen
      onOpen={(id) => setScreen({ name: 'detail', id })}
      onCreate={() => setScreen({ name: 'create' })}
    />
  )
}
