import { History } from 'lucide-react'
import { api, type StateColors } from '../api'
import { usePolling } from '../hooks'
import { kindColor, kindLabels } from '../states'
import { Card } from './Card'

export function RecentAlerts({ colors }: { colors?: StateColors }) {
  const { data: alerts } = usePolling(api.alerts, 5000)

  return (
    <Card title="Recent alerts" icon={History}>
      {!alerts || alerts.length === 0 ? (
        <p className="rounded-2xl border border-dashed border-white/10 px-4 py-6 text-center text-sm text-zinc-500">
          No alerts for your cities since the server started.
        </p>
      ) : (
        <ul className="divide-y divide-white/[0.05]">
          {alerts.map((a, i) => {
            const color = kindColor(a.kind, colors)
            return (
              <li key={`${a.time}-${a.kind}-${i}`} className="flex items-start gap-3 py-3 first:pt-0 last:pb-0">
                <span
                  className="mt-0.5 shrink-0 rounded-full px-2.5 py-1 text-[11px] font-semibold"
                  style={{ background: `${color}22`, color }}
                >
                  {kindLabels[a.kind]}
                </span>
                <div className="min-w-0 flex-1">
                  <p dir="rtl" className="truncate text-right text-sm text-zinc-200" title={a.cities.join(', ')}>
                    {a.cities.length > 0 ? a.cities.join(', ') : '—'}
                  </p>
                </div>
                <time className="shrink-0 font-mono text-xs tabular-nums text-zinc-500" dateTime={a.time}>
                  {formatTime(a.time)}
                </time>
              </li>
            )
          })}
        </ul>
      )}
    </Card>
  )
}

function formatTime(iso: string) {
  const d = new Date(iso)
  const sameDay = d.toDateString() === new Date().toDateString()
  return sameDay
    ? d.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })
    : d.toLocaleString([], { day: '2-digit', month: '2-digit', hour: '2-digit', minute: '2-digit' })
}
