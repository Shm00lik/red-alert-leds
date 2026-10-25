import { Lightbulb, Power, Send } from 'lucide-react'
import { useState, type ReactNode } from 'react'
import { api, type Effect, type LedCommand, type LedState, type StateColors } from '../api'
import { stateColor, stateLabels } from '../states'
import { Card } from './Card'
import { useToast } from './Toast'

const EFFECTS: { id: Effect; label: string }[] = [
  { id: 'fill', label: 'Fill' },
  { id: 'blink', label: 'Blink' },
  { id: 'progress', label: 'Progress' },
  { id: 'rainbow', label: 'Rainbow' },
  { id: 'idle', label: 'Off' },
]

const FORCE_STATES: Exclude<LedState, 'Manual'>[] = ['EarlyWarning', 'Alert', 'Safe', 'Idle']

const SWATCHES = ['#FF0000', '#FF4500', '#FFB000', '#00FF00', '#00C8FF', '#3B5BFF', '#B000FF', '#FFFFFF']

export function LedControl({ colors }: { colors?: StateColors }) {
  const toast = useToast()
  const [effect, setEffect] = useState<Effect>('fill')
  const [color, setColor] = useState('#00C8FF')
  const [periodMs, setPeriodMs] = useState(1000)
  const [percent, setPercent] = useState(100)
  const [durationSec, setDurationSec] = useState(30)
  const [speedMs, setSpeedMs] = useState(20)
  const [busy, setBusy] = useState(false)

  const run = async (action: () => Promise<void>, success: string) => {
    setBusy(true)
    try {
      await action()
      toast(success, 'success')
    } catch (e) {
      toast(e instanceof Error ? e.message : 'Request failed')
    } finally {
      setBusy(false)
    }
  }

  const buildCommand = (): LedCommand => {
    switch (effect) {
      case 'fill':
        return { effect, color }
      case 'blink':
        return { effect, color, periodMs }
      case 'progress':
        return durationSec > 0
          ? { effect, color, from: percent, to: 0, durationMs: durationSec * 1000 }
          : { effect, color, from: percent }
      case 'rainbow':
        return { effect, speedMs }
      case 'idle':
        return { effect }
    }
  }

  const usesColor = effect === 'fill' || effect === 'blink' || effect === 'progress'

  return (
    <Card title="Manual control" icon={Lightbulb}>
      <div className="grid grid-cols-5 gap-1 rounded-2xl bg-zinc-950/60 p-1">
        {EFFECTS.map((e) => (
          <button
            key={e.id}
            type="button"
            onClick={() => setEffect(e.id)}
            className={`rounded-xl px-1 py-2 text-xs font-medium transition-colors sm:text-sm ${
              effect === e.id ? 'bg-white/10 text-white shadow' : 'text-zinc-400 hover:text-zinc-200'
            }`}
          >
            {e.label}
          </button>
        ))}
      </div>

      <div className="mt-5 space-y-5">
        {usesColor && (
          <div>
            <FieldLabel>Color</FieldLabel>
            <div className="flex flex-wrap items-center gap-2">
              {SWATCHES.map((s) => (
                <button
                  key={s}
                  type="button"
                  onClick={() => setColor(s)}
                  aria-label={s}
                  className={`size-8 rounded-full border transition-transform hover:scale-110 ${
                    color.toUpperCase() === s ? 'border-white ring-2 ring-white/30' : 'border-white/10'
                  }`}
                  style={{ background: s }}
                />
              ))}
              <input type="color" value={color} onChange={(e) => setColor(e.target.value)} className="size-8" aria-label="Custom color" />
            </div>
          </div>
        )}

        {effect === 'blink' && (
          <Slider label="Blink period" value={periodMs} min={100} max={3000} step={25} unit="ms" onChange={setPeriodMs} />
        )}

        {effect === 'progress' && (
          <>
            <Slider label="Start at" value={percent} min={0} max={100} step={5} unit="%" onChange={setPercent} />
            <Slider label="Drain over" value={durationSec} min={0} max={600} step={5} unit="s" onChange={setDurationSec} hint={durationSec === 0 ? 'static' : undefined} />
          </>
        )}

        {effect === 'rainbow' && <Slider label="Speed" value={speedMs} min={2} max={100} step={1} unit="ms/step" onChange={setSpeedMs} />}

        {effect === 'rainbow' && (
          <div className="h-3 rounded-full" style={{ background: 'linear-gradient(90deg,#f00,#ff0,#0f0,#0ff,#00f,#f0f,#f00)' }} />
        )}
        {usesColor && (
          <div className="h-3 rounded-full transition-colors" style={{ background: color, boxShadow: `0 0 18px ${color}66` }} />
        )}

        <div className="flex gap-2">
          <button
            type="button"
            disabled={busy}
            onClick={() => run(() => api.sendLed(buildCommand()), 'Sent to the LEDs')}
            className="flex flex-1 items-center justify-center gap-2 rounded-2xl bg-white px-4 py-3 text-sm font-semibold text-zinc-950 transition hover:bg-zinc-200 disabled:opacity-50"
          >
            <Send className="size-4" />
            Send
          </button>
          <button
            type="button"
            disabled={busy}
            onClick={() => run(() => api.sendLed({ effect: 'idle' }), 'LEDs off')}
            aria-label="Turn off"
            className="flex items-center justify-center rounded-2xl border border-white/10 px-4 text-zinc-300 transition hover:bg-white/5 disabled:opacity-50"
          >
            <Power className="size-4" />
          </button>
        </div>
      </div>

      <div className="mt-6 border-t border-white/[0.06] pt-5">
        <FieldLabel>Simulate a state</FieldLabel>
        <div className="grid grid-cols-2 gap-2 sm:grid-cols-4">
          {FORCE_STATES.map((s) => {
            const c = stateColor(s, colors)
            return (
              <button
                key={s}
                type="button"
                disabled={busy}
                onClick={() => run(() => api.forceState(s), `${stateLabels[s]} simulated`)}
                className="flex items-center justify-center gap-2 rounded-xl border border-white/[0.08] bg-white/[0.03] px-3 py-2.5 text-xs font-medium text-zinc-300 transition hover:bg-white/[0.07] disabled:opacity-50"
              >
                <span className="size-2 rounded-full" style={{ background: c, boxShadow: `0 0 8px ${c}` }} />
                {stateLabels[s]}
              </button>
            )
          })}
        </div>
      </div>
    </Card>
  )
}

function FieldLabel({ children }: { children: ReactNode }) {
  return <p className="mb-2 text-xs font-medium uppercase tracking-wider text-zinc-500">{children}</p>
}

interface SliderProps {
  label: string
  value: number
  min: number
  max: number
  step: number
  unit: string
  hint?: string
  onChange: (value: number) => void
}

function Slider({ label, value, min, max, step, unit, hint, onChange }: SliderProps) {
  return (
    <div>
      <div className="mb-2 flex items-baseline justify-between">
        <FieldLabel>{label}</FieldLabel>
        <span className="font-mono text-sm tabular-nums text-zinc-300">{hint ?? `${value} ${unit}`}</span>
      </div>
      <input type="range" min={min} max={max} step={step} value={value} onChange={(e) => onChange(Number(e.target.value))} />
    </div>
  )
}
