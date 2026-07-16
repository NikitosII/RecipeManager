import { useRef } from 'react'
import { Camera, Mail, UtensilsCrossed, X } from 'lucide-react'
import { useProfile, useUploadAvatar } from '@/hooks/use-profile'
import { resolveMediaUrl } from '@/config'

export function ProfileModal({ open, onClose }: { open: boolean; onClose: () => void }) {
  const { data: profile, isLoading } = useProfile(open)
  const uploadAvatar = useUploadAvatar()
  const fileInputRef = useRef<HTMLInputElement>(null)

  if (!open) return null

  const initials =
    (profile ? `${profile.firstName?.[0] ?? ''}${profile.lastName?.[0] ?? ''}`.toUpperCase() : '') || '?'
  const avatar = resolveMediaUrl(profile?.avatarUrl)
  const fullName = profile ? `${profile.firstName} ${profile.lastName}`.trim() : ''

  const onPickFile = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0]
    e.target.value = '' // allow re-selecting the same file
    if (file) uploadAvatar.mutate(file)
  }

  return (
    <div
      className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 backdrop-blur-sm p-4"
      onClick={onClose}
    >
      <div
        role="dialog"
        aria-modal="true"
        aria-label="My profile"
        className="w-full max-w-sm bg-white rounded-2xl border border-border shadow-xl overflow-hidden"
        onClick={(e) => e.stopPropagation()}
      >
        <div className="flex items-center justify-between px-5 py-3.5 border-b border-border">
          <h2 className="text-base font-semibold text-[#2C1A0E]" style={{ fontFamily: "'Playfair Display', serif" }}>
            My Profile
          </h2>
          <button
            onClick={onClose}
            aria-label="Close"
            className="p-1.5 rounded-lg text-muted-foreground hover:bg-muted transition-colors"
          >
            <X size={16} />
          </button>
        </div>

        {isLoading || !profile ? (
          <div className="p-8 text-center text-sm text-muted-foreground">Loading…</div>
        ) : (
          <div className="p-6 flex flex-col items-center">
            {/* Avatar with upload overlay */}
            <div className="relative">
              {avatar ? (
                <img
                  src={avatar}
                  alt={fullName}
                  className="w-24 h-24 rounded-full object-cover border-2 border-border"
                />
              ) : (
                <span className="w-24 h-24 rounded-full bg-[#D94F3A] text-white flex items-center justify-center text-2xl font-semibold">
                  {initials}
                </span>
              )}
              <button
                onClick={() => fileInputRef.current?.click()}
                disabled={uploadAvatar.isPending}
                aria-label="Upload new avatar"
                className="absolute bottom-0 right-0 p-2 rounded-full bg-[#D94F3A] text-white border-2 border-white shadow hover:bg-[#C0392B] transition-colors disabled:opacity-60"
              >
                <Camera size={14} />
              </button>
              <input
                ref={fileInputRef}
                type="file"
                accept="image/png,image/jpeg,image/webp,image/gif"
                className="hidden"
                onChange={onPickFile}
              />
            </div>

            <h3 className="mt-4 text-lg font-semibold text-[#2C1A0E]">{fullName || 'Unnamed cook'}</h3>

            <div className="mt-1 flex items-center gap-1.5 text-sm text-muted-foreground">
              <Mail size={13} />
              <span className="truncate max-w-[15rem]">{profile.email}</span>
            </div>

            {uploadAvatar.isPending && (
              <p className="mt-3 text-xs text-muted-foreground">Uploading avatar…</p>
            )}
            {uploadAvatar.isError && (
              <p className="mt-3 text-xs text-[#D94F3A]">Upload failed. Use an image under 5 MB.</p>
            )}

            <div className="mt-6 w-full rounded-xl bg-muted/60 border border-border px-4 py-3 flex items-center gap-3">
              <span className="p-2 rounded-lg bg-[#D94F3A]/10 text-[#D94F3A]">
                <UtensilsCrossed size={16} />
              </span>
              <div>
                <p className="text-lg font-semibold text-[#2C1A0E] leading-none">{profile.recipeCount}</p>
                <p className="text-xs text-muted-foreground mt-0.5">
                  {profile.recipeCount === 1 ? 'recipe created' : 'recipes created'}
                </p>
              </div>
            </div>
          </div>
        )}
      </div>
    </div>
  )
}
