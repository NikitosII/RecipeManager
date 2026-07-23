import { type ReactNode } from 'react'
import { Navigate, Route, Routes, useNavigate, useParams } from 'react-router-dom'
import { useAuthStore } from '@/stores/auth-store'
import { AuthScreen } from './screens/AuthScreen'
import { DashboardScreen } from './screens/DashboardScreen'
import { DetailScreen } from './screens/DetailScreen'
import { CreateScreen } from './screens/CreateScreen'
import { EditScreen } from './screens/EditScreen'

export default function App() {
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated)

  return (
    <Routes>
      <Route path="/login" element={isAuthenticated ? <Navigate to="/" replace /> : <AuthScreen />} />
      <Route path="/" element={<RequireAuth><DashboardRoute /></RequireAuth>} />
      <Route path="/recipes/new" element={<RequireAuth><CreateRoute /></RequireAuth>} />
      <Route path="/recipes/:id" element={<RequireAuth><DetailRoute /></RequireAuth>} />
      <Route path="/recipes/:id/edit" element={<RequireAuth><EditRoute /></RequireAuth>} />
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  )
}
function RequireAuth({ children }: { children: ReactNode }) {
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated)
  if (!isAuthenticated) return <Navigate to="/login" replace />
  return <>{children}</>
}

// -- Route adapters -- //

function DashboardRoute() {
  const navigate = useNavigate()
  return (
    <DashboardScreen
      onOpen={(id) => navigate(`/recipes/${id}`)}
      onCreate={() => navigate('/recipes/new')}
    />
  )
}

function DetailRoute() {
  const navigate = useNavigate()
  const { id } = useParams()
  if (!id) return <Navigate to="/" replace />
  return (
    <DetailScreen
      recipeId={id}
      onBack={() => navigate('/')}
      onEdit={(recipeId) => navigate(`/recipes/${recipeId}/edit`)}
    />
  )
}

function CreateRoute() {
  const navigate = useNavigate()
  return <CreateScreen onBack={() => navigate('/')} />
}

function EditRoute() {
  const navigate = useNavigate()
  const { id } = useParams()
  if (!id) return <Navigate to="/" replace />
  return <EditScreen recipeId={id} onBack={() => navigate(`/recipes/${id}`)} />
}
