import { CircleAlert, CircleCheck } from 'lucide-react'
import { createContext, useCallback, useContext, useRef, useState, type ReactNode } from 'react'

type ToastKind = 'error' | 'success'

interface ToastItem {
  id: number
  kind: ToastKind
  message: string
}

const ToastContext = createContext<(message: string, kind?: ToastKind) => void>(() => {})

export function useToast() {
  return useContext(ToastContext)
}

export function ToastProvider({ children }: { children: ReactNode }) {
  const [toasts, setToasts] = useState<ToastItem[]>([])
  const nextId = useRef(0)

  const show = useCallback((message: string, kind: ToastKind = 'error') => {
    const id = nextId.current++
    setToasts((t) => [...t.slice(-2), { id, kind, message }])
    setTimeout(() => setToasts((t) => t.filter((x) => x.id !== id)), 3500)
  }, [])

  return (
    <ToastContext.Provider value={show}>
      {children}
      <div className="pointer-events-none fixed inset-x-0 bottom-4 z-50 flex flex-col items-center gap-2 px-4">
        {toasts.map((t) => (
          <div
            key={t.id}
            role="status"
            className="flex max-w-md items-center gap-2 rounded-2xl border border-white/10 bg-zinc-900/95 px-4 py-3 text-sm shadow-2xl shadow-black/40 backdrop-blur"
          >
            {t.kind === 'error' ? (
              <CircleAlert className="size-4 shrink-0 text-red-400" />
            ) : (
              <CircleCheck className="size-4 shrink-0 text-emerald-400" />
            )}
            <span className="text-zinc-200">{t.message}</span>
          </div>
        ))}
      </div>
    </ToastContext.Provider>
  )
}
