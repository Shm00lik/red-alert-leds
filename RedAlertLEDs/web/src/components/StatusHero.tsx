import { Cpu, Radio, Wifi } from 'lucide-react'
import type { StateColors, Status } from '../api'
import { useNow } from '../hooks'
import { stateColor, stateHints, stateLabels } from '../states'

const RADIUS = 88
const CIRCUMFERENCE = 2 * Math.PI * RADIUS

interface StatusHeroProps {
  status: Status | null
  colors?: StateColors
  offline: boolean
}

export function StatusHero({ status, colors, offline }: StatusHeroProps) {
  const now = useNow(250)
  const state = status?.state ?? 'Idle'
  const color = stateColor(state, colors)

  let fraction = state === 'Idle' ? 0 : 1
  let remaining: string | null = null

  if (status?.countdownEndsAt) {
    const start = Date.parse(status.stateSince)
    const end = Date.parse(status.countdownEndsAt)
    const left = Math.max(0, end - now)
    fraction = end > start ? left / (end - start) : 0
    remaining = formatDuration(left)
  }

  const isAlert = state === 'Alert'

  return (
    <section
      className="relative overflow-hidden rounded-[2rem] border border-white/[0.06] bg-zinc-900/60 p-6 shadow-2xl shadow-black/30 sm:p-8"
      style={{ ['--accent' as string]: color }}
    >
      <div
        className="pointer-events-none absolute -top-24 left-1/2 size-96 -translate-x-1/2 rounded-full blur-3xl transition-colors duration-700"
        style={{ background: color, opacity: state === 'Idle' ? 0.08 : 0.22 }}
      />

      <div className="relative flex flex-col items-center gap-6 sm:flex-row sm:gap-10">
        <div className="relative size-52 shrink-0">
          <svg viewBox="0 0 200 200" className="size-full -rotate-90">
            <circle cx="100" cy="100" r={RADIUS} fill="none" stroke="rgb(255 255 255 / 0.06)" strokeWidth="12" />
            <circle
              cx="100"
              cy="100"
              r={RADIUS}
              fill="none"
              stroke={color}
              strokeWidth="12"
              strokeLinecap="round"
              strokeDasharray={CIRCUMFERENCE}
              strokeDashoffset={CIRCUMFERENCE * (1 - fraction)}
              className={`transition-[stroke-dashoffset,stroke] duration-300 ease-linear ${isAlert ? 'animate-pulse-ring' : ''}`}
              style={{ filter: `drop-shadow(0 0 10px ${color}88)` }}
            />
          </svg>
          <div className="absolute inset-0 flex flex-col items-center justify-center">
            {remaining ? (
              <>
                <span className="font-mono text-4xl font-semibold tabular-nums tracking-tight">{remaining}</span>
                <span className="mt-1 text-xs uppercase tracking-[0.2em] text-zinc-500">remaining</span>
              </>
            ) : (
              <span className="size-4 rounded-full transition-colors duration-700" style={{ background: color, boxShadow: `0 0 24px ${color}` }} />
            )}
          </div>
        </div>

        <div className="flex flex-1 flex-col items-center gap-4 text-center sm:items-start sm:text-left">
          <div>
            <p className="text-xs font-medium uppercase tracking-[0.25em] text-zinc-500">Current state</p>
            <h1 className="mt-2 text-4xl font-bold tracking-tight transition-colors duration-700 sm:text-5xl" style={{ color: state === 'Idle' ? undefined : color }}>
              {status ? stateLabels[state] : 'Connecting…'}
            </h1>
            <p className="mt-2 text-zinc-400">{status ? stateHints[state] : 'Waiting for the server'}</p>
          </div>

          <div className="flex flex-wrap justify-center gap-2 sm:justify-start">
            <Pill icon={Radio} label="Tzofar" ok={!offline && !!status?.tzofarConnected} />
            <Pill icon={Wifi} label="MQTT" ok={!offline && !!status?.mqttConnected} />
            <Pill icon={Cpu} label="ESP" ok={!offline && !!status?.espOnline} />
          </div>
          {offline && <p className="text-sm text-red-400">Can't reach the server</p>}
        </div>
      </div>
    </section>
  )
}

function Pill({ icon: Icon, label, ok }: { icon: typeof Radio; label: string; ok: boolean }) {
  return (
    <span
      className={`inline-flex items-center gap-1.5 rounded-full border px-3 py-1.5 text-xs font-medium transition-colors ${
        ok ? 'border-emerald-500/20 bg-emerald-500/10 text-emerald-300' : 'border-red-500/20 bg-red-500/10 text-red-300'
      }`}
    >
      <Icon className="size-3.5" />
      {label}
      <span className={`size-1.5 rounded-full ${ok ? 'bg-emerald-400' : 'bg-red-400'}`} />
    </span>
  )
}

function formatDuration(ms: number) {
  const total = Math.ceil(ms / 1000)
  const minutes = Math.floor(total / 60)
  const seconds = total % 60
  return `${minutes}:${seconds.toString().padStart(2, '0')}`
}
