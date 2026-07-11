import { useState } from 'react'
import type { FormEvent } from 'react'
import { Check, Eye, EyeOff, Utensils } from 'lucide-react'
import { authApi } from '@/lib/api'
import { getApiErrorMessage } from '@/lib/api-error'
import { useAuthStore } from '@/stores/auth-store'

type AuthTab = 'login' | 'signup'

export function AuthScreen() {
  const setSession = useAuthStore((s) => s.setSession)

  const [tab, setTab] = useState<AuthTab>('login')
  const [showPw, setShowPw] = useState(false)
  const [showConfirm, setShowConfirm] = useState(false)
  const [remember, setRemember] = useState(false)

  const [fullName, setFullName] = useState('')
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [confirm, setConfirm] = useState('')

  const [submitting, setSubmitting] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const switchTab = (t: AuthTab) => {
    setTab(t)
    setError(null)
  }

  async function handleSubmit(e: FormEvent) {
    e.preventDefault()
    setError(null)

    if (tab === 'signup' && password !== confirm) {
      setError('Passwords do not match.')
      return
    }

    setSubmitting(true)
    try {
      if (tab === 'login') {
        const auth = await authApi.login({ email, password })
        setSession(auth)
      } else {
        const trimmed = fullName.trim()
        const firstSpace = trimmed.indexOf(' ')
        const firstName = firstSpace === -1 ? trimmed : trimmed.slice(0, firstSpace)
        const lastName = firstSpace === -1 ? '' : trimmed.slice(firstSpace + 1)
        const auth = await authApi.register({ firstName, lastName, email, password })
        setSession(auth)
      }
    } catch (err) {
      setError(getApiErrorMessage(err))
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <div className="min-h-screen flex" style={{ fontFamily: "'DM Sans', sans-serif" }}>
      {/* Left — photo panel */}
      <div className="hidden lg:flex lg:w-[55%] relative flex-col justify-between overflow-hidden">
        <img
          src="https://images.unsplash.com/photo-1414235077428-338989a2e8c0?w=1200&h=1000&fit=crop&auto=format"
          alt="Fine dining food spread"
          className="absolute inset-0 w-full h-full object-cover"
        />
        <div className="absolute inset-0 bg-gradient-to-br from-[#2C1A0E]/80 via-[#2C1A0E]/50 to-transparent" />

        <div className="relative z-10 p-10">
          <div className="flex items-center gap-2.5">
            <div className="w-8 h-8 bg-[#D94F3A] rounded-lg flex items-center justify-center">
              <Utensils size={16} className="text-white" />
            </div>
            <span
              className="text-white font-semibold text-lg tracking-wide"
              style={{ fontFamily: "'Playfair Display', serif" }}
            >
              Mealcraft
            </span>
          </div>
        </div>

        <div className="relative z-10 p-10 pb-12">
          <blockquote
            className="text-white/90 text-3xl leading-tight mb-4"
            style={{ fontFamily: "'Playfair Display', serif", fontStyle: 'italic' }}
          >
            "Great cooking is about ingredients and how you combine them."
          </blockquote>
          <p className="text-white/60 text-sm">— Wolfgang Puck</p>
        </div>
      </div>

      {/* Right — form panel */}
      <div className="flex-1 flex items-center justify-center bg-background px-6 py-12">
        <div className="w-full max-w-[400px]">
          <div className="flex items-center gap-2 mb-8 lg:hidden">
            <div className="w-7 h-7 bg-[#D94F3A] rounded-md flex items-center justify-center">
              <Utensils size={14} className="text-white" />
            </div>
            <span className="font-semibold text-[#2C1A0E]" style={{ fontFamily: "'Playfair Display', serif" }}>
              Mealcraft
            </span>
          </div>

          <h1
            className="text-2xl font-semibold text-[#2C1A0E] mb-1"
            style={{ fontFamily: "'Playfair Display', serif" }}
          >
            {tab === 'login' ? 'Welcome back' : 'Create your account'}
          </h1>
          <p className="text-sm text-muted-foreground mb-7">
            {tab === 'login'
              ? 'Sign in to access your recipes and collections.'
              : 'Join thousands of home cooks sharing their passion.'}
          </p>

          <div className="flex bg-muted rounded-xl p-1 mb-7">
            {(['login', 'signup'] as AuthTab[]).map((t) => (
              <button
                key={t}
                type="button"
                onClick={() => switchTab(t)}
                className={`flex-1 py-2 rounded-lg text-sm font-medium transition-all duration-200 ${
                  tab === t ? 'bg-white text-[#2C1A0E] shadow-sm' : 'text-muted-foreground hover:text-[#2C1A0E]'
                }`}
              >
                {t === 'login' ? 'Log In' : 'Sign Up'}
              </button>
            ))}
          </div>

          <form className="space-y-4" onSubmit={handleSubmit}>
            {tab === 'signup' && (
              <div>
                <label className="block text-xs font-medium text-[#2C1A0E] mb-1.5">Full Name</label>
                <input
                  type="text"
                  required
                  value={fullName}
                  onChange={(e) => setFullName(e.target.value)}
                  placeholder="Emma Thompson"
                  className="w-full px-3.5 py-2.5 bg-input-background border border-border rounded-xl text-sm focus:outline-none focus:ring-2 focus:ring-[#D94F3A]/30 focus:border-[#D94F3A] transition-all"
                />
              </div>
            )}

            <div>
              <label className="block text-xs font-medium text-[#2C1A0E] mb-1.5">Email Address</label>
              <input
                type="email"
                required
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                placeholder="emma@example.com"
                className="w-full px-3.5 py-2.5 bg-input-background border border-border rounded-xl text-sm focus:outline-none focus:ring-2 focus:ring-[#D94F3A]/30 focus:border-[#D94F3A] transition-all"
              />
            </div>

            <div>
              <label className="block text-xs font-medium text-[#2C1A0E] mb-1.5">Password</label>
              <div className="relative">
                <input
                  type={showPw ? 'text' : 'password'}
                  required
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  placeholder="••••••••"
                  className="w-full px-3.5 py-2.5 pr-10 bg-input-background border border-border rounded-xl text-sm focus:outline-none focus:ring-2 focus:ring-[#D94F3A]/30 focus:border-[#D94F3A] transition-all"
                />
                <button
                  type="button"
                  onClick={() => setShowPw(!showPw)}
                  className="absolute right-3 top-1/2 -translate-y-1/2 text-muted-foreground hover:text-[#2C1A0E] transition-colors"
                >
                  {showPw ? <EyeOff size={15} /> : <Eye size={15} />}
                </button>
              </div>
              {tab === 'signup' && (
                <p className="text-[11px] text-muted-foreground mt-1.5">At least 8 characters.</p>
              )}
            </div>

            {tab === 'signup' && (
              <div>
                <label className="block text-xs font-medium text-[#2C1A0E] mb-1.5">Confirm Password</label>
                <div className="relative">
                  <input
                    type={showConfirm ? 'text' : 'password'}
                    required
                    value={confirm}
                    onChange={(e) => setConfirm(e.target.value)}
                    placeholder="••••••••"
                    className="w-full px-3.5 py-2.5 pr-10 bg-input-background border border-border rounded-xl text-sm focus:outline-none focus:ring-2 focus:ring-[#D94F3A]/30 focus:border-[#D94F3A] transition-all"
                  />
                  <button
                    type="button"
                    onClick={() => setShowConfirm(!showConfirm)}
                    className="absolute right-3 top-1/2 -translate-y-1/2 text-muted-foreground hover:text-[#2C1A0E] transition-colors"
                  >
                    {showConfirm ? <EyeOff size={15} /> : <Eye size={15} />}
                  </button>
                </div>
              </div>
            )}

            {tab === 'login' && (
              <div className="flex items-center justify-between">
                <label className="flex items-center gap-2 cursor-pointer select-none">
                  <button
                    type="button"
                    onClick={() => setRemember(!remember)}
                    className={`w-4 h-4 rounded border flex items-center justify-center transition-colors ${
                      remember ? 'bg-[#D94F3A] border-[#D94F3A]' : 'border-border bg-input-background'
                    }`}
                  >
                    {remember && <Check size={10} className="text-white" />}
                  </button>
                  <span className="text-xs text-muted-foreground">Remember me</span>
                </label>
                <button type="button" className="text-xs text-[#D94F3A] hover:underline font-medium">
                  Forgot password?
                </button>
              </div>
            )}

            {error && (
              <div className="px-3.5 py-2.5 rounded-xl bg-rose-50 border border-rose-200 text-sm text-[#C0392B]">
                {error}
              </div>
            )}

            <button
              type="submit"
              disabled={submitting}
              className="w-full py-3 bg-[#D94F3A] text-white rounded-xl font-semibold text-sm hover:bg-[#C0392B] transition-colors duration-200 mt-2 disabled:opacity-70 flex items-center justify-center gap-2"
            >
              {submitting && (
                <span className="inline-block w-4 h-4 border-2 border-white/30 border-t-white rounded-full animate-spin" />
              )}
              {tab === 'login' ? 'Sign In' : 'Create Account'}
            </button>
          </form>

          <div className="relative my-6">
            <div className="absolute inset-0 flex items-center">
              <div className="w-full border-t border-border" />
            </div>
            <div className="relative flex justify-center">
              <span className="bg-background px-3 text-xs text-muted-foreground">or continue with</span>
            </div>
          </div>

          <div className="grid grid-cols-2 gap-3">
            {[
              { name: 'Google', bg: 'bg-white border border-border hover:bg-secondary', logo: 'G' },
              { name: 'Apple', bg: 'bg-[#2C1A0E] text-white hover:bg-[#1a0f08]', logo: '' },
            ].map((s) => (
              <button
                key={s.name}
                type="button"
                className={`flex items-center justify-center gap-2 py-2.5 rounded-xl text-sm font-medium transition-colors ${s.bg}`}
              >
                <span className="font-bold text-sm">{s.logo || '🍎'}</span>
                {s.name}
              </button>
            ))}
          </div>
        </div>
      </div>
    </div>
  )
}
