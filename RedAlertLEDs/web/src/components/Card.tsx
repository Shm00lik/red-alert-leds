import type { LucideIcon } from 'lucide-react'
import type { ReactNode } from 'react'

interface CardProps {
  title: string
  icon: LucideIcon
  action?: ReactNode
  children: ReactNode
  className?: string
}

export function Card({ title, icon: Icon, action, children, className = '' }: CardProps) {
  return (
    <section
      className={`rounded-3xl border border-white/[0.06] bg-zinc-900/60 p-5 shadow-xl shadow-black/20 backdrop-blur sm:p-6 ${className}`}
    >
      <header className="mb-4 flex items-center justify-between gap-3">
        <h2 className="flex items-center gap-2 text-sm font-semibold tracking-wide text-zinc-300">
          <Icon className="size-4 text-zinc-500" strokeWidth={2.25} />
          {title}
        </h2>
        {action}
      </header>
      {children}
    </section>
  )
}
