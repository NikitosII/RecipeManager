import { useState } from 'react'
import { MessageSquare, Pencil, Trash2 } from 'lucide-react'
import { useAddComment, useComments, useDeleteComment, useUpdateComment } from '@/hooks/use-comments'
import { resolveMediaUrl } from '@/config'
import type { Comment } from '@/types/api'

export function CommentsSection({ recipeId }: { recipeId: string }) {
  const { data: comments, isLoading } = useComments(recipeId)
  const addComment = useAddComment(recipeId)
  const [draft, setDraft] = useState('')

  const submit = () => {
    const body = draft.trim()
    if (!body) return
    addComment.mutate(body, { onSuccess: () => setDraft('') })
  }

  const count = comments?.length ?? 0

  return (
    <section className="mt-2">
      <h2
        className="flex items-center gap-2 text-xl font-semibold text-[#2C1A0E] mb-6"
        style={{ fontFamily: "'Playfair Display', serif" }}
      >
        <MessageSquare size={20} className="text-[#D94F3A]" />
        Comments{count > 0 && <span className="text-muted-foreground font-normal">({count})</span>}
      </h2>

      {/* New comment */}
      <div className="mb-8">
        <textarea
          value={draft}
          onChange={(e) => setDraft(e.target.value)}
          placeholder="Share your thoughts on this recipe…"
          rows={3}
          maxLength={2000}
          className="w-full resize-none rounded-2xl border border-border bg-white px-4 py-3 text-sm text-[#2C1A0E] placeholder:text-muted-foreground focus:border-[#D94F3A]/50 focus:outline-none focus:ring-2 focus:ring-[#D94F3A]/15"
        />
        <div className="mt-2 flex justify-end">
          <button
            onClick={submit}
            disabled={!draft.trim() || addComment.isPending}
            className="px-5 py-2 bg-[#D94F3A] text-white rounded-xl text-sm font-medium hover:bg-[#C0392B] transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
          >
            {addComment.isPending ? 'Posting…' : 'Post comment'}
          </button>
        </div>
      </div>

      {/* List */}
      {isLoading ? (
        <p className="text-sm text-muted-foreground">Loading comments…</p>
      ) : count === 0 ? (
        <p className="text-sm text-muted-foreground">No comments yet — be the first to share.</p>
      ) : (
        <ul className="space-y-4">
          {comments!.map((comment) => (
            <CommentItem key={comment.id} recipeId={recipeId} comment={comment} />
          ))}
        </ul>
      )}
    </section>
  )
}

function CommentItem({ recipeId, comment }: { recipeId: string; comment: Comment }) {
  const updateComment = useUpdateComment(recipeId)
  const deleteComment = useDeleteComment(recipeId)
  const [editing, setEditing] = useState(false)
  const [draft, setDraft] = useState(comment.body)
  const [confirmingDelete, setConfirmingDelete] = useState(false)

  const avatar = resolveMediaUrl(comment.authorAvatarUrl)
  const initials = comment.authorName
    .split(' ')
    .map((p) => p[0])
    .filter(Boolean)
    .slice(0, 2)
    .join('')
    .toUpperCase() || '?'
  const edited = comment.dateUpdated !== comment.dateCreated

  const saveEdit = () => {
    const body = draft.trim()
    if (!body || body === comment.body) {
      setEditing(false)
      setDraft(comment.body)
      return
    }
    updateComment.mutate({ id: comment.id, body }, { onSuccess: () => setEditing(false) })
  }

  return (
    <li className="flex gap-3">
      <div className="flex-shrink-0 w-9 h-9 rounded-full overflow-hidden bg-muted flex items-center justify-center text-xs font-semibold text-muted-foreground">
        {avatar ? (
          <img src={avatar} alt={comment.authorName} className="w-full h-full object-cover" />
        ) : (
          initials
        )}
      </div>

      <div className="flex-1 min-w-0">
        <div className="rounded-2xl border border-border bg-white px-4 py-3">
          <div className="flex items-center justify-between gap-2 mb-1">
            <span className="text-sm font-semibold text-[#2C1A0E] truncate">{comment.authorName}</span>
            <span className="text-xs text-muted-foreground flex-shrink-0">
              {formatDate(comment.dateCreated)}
              {edited && ' · edited'}
            </span>
          </div>

          {editing ? (
            <div>
              <textarea
                value={draft}
                onChange={(e) => setDraft(e.target.value)}
                rows={3}
                maxLength={2000}
                className="w-full resize-none rounded-xl border border-border bg-white px-3 py-2 text-sm text-[#2C1A0E] focus:border-[#D94F3A]/50 focus:outline-none focus:ring-2 focus:ring-[#D94F3A]/15"
              />
              <div className="mt-2 flex gap-2 justify-end">
                <button
                  onClick={() => {
                    setEditing(false)
                    setDraft(comment.body)
                  }}
                  className="px-3 py-1.5 text-sm text-muted-foreground hover:text-[#2C1A0E] transition-colors"
                >
                  Cancel
                </button>
                <button
                  onClick={saveEdit}
                  disabled={updateComment.isPending}
                  className="px-4 py-1.5 bg-[#D94F3A] text-white rounded-lg text-sm font-medium hover:bg-[#C0392B] transition-colors disabled:opacity-50"
                >
                  Save
                </button>
              </div>
            </div>
          ) : (
            <p className="text-sm text-[#2C1A0E] leading-relaxed whitespace-pre-wrap break-words">{comment.body}</p>
          )}
        </div>

        {comment.canEdit && !editing && (
          <div className="mt-1.5 flex items-center gap-3 px-1">
            <button
              onClick={() => {
                setEditing(true)
                setConfirmingDelete(false)
              }}
              className="flex items-center gap-1 text-xs text-muted-foreground hover:text-[#D94F3A] transition-colors"
            >
              <Pencil size={12} /> Edit
            </button>
            {confirmingDelete ? (
              <span className="flex items-center gap-2 text-xs">
                <span className="text-muted-foreground">Delete?</span>
                <button
                  onClick={() => deleteComment.mutate(comment.id)}
                  disabled={deleteComment.isPending}
                  className="text-red-600 font-medium hover:underline disabled:opacity-50"
                >
                  Yes
                </button>
                <button
                  onClick={() => setConfirmingDelete(false)}
                  className="text-muted-foreground hover:text-[#2C1A0E]"
                >
                  No
                </button>
              </span>
            ) : (
              <button
                onClick={() => setConfirmingDelete(true)}
                className="flex items-center gap-1 text-xs text-muted-foreground hover:text-red-600 transition-colors"
              >
                <Trash2 size={12} /> Delete
              </button>
            )}
          </div>
        )}
      </div>
    </li>
  )
}

function formatDate(iso: string): string {
  const date = new Date(iso)
  return date.toLocaleDateString(undefined, { year: 'numeric', month: 'short', day: 'numeric' })
}
