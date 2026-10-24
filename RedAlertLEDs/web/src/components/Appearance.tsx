import { Palette, Sun } from 'lucide-react'
import type { StateColors } from '../api'
import { Card } from './Card'

interface AppearanceProps {
  colors: StateColors
  brightness: number
  onColorsChange: (colors: StateColors) => void
  onBrightnessChange: (brightness: number) => void
}

const FIELDS: { key: keyof StateColors; label: string }[] = [
  { key: 'earlyWarning', label: 'Early warning' },
  { key: 'alert', label: 'Red alert' },
  { key: 'safe', label: 'All clear' },
]

export function Appearance({ colors, brightness, onColorsChange, onBrightnessChange }: AppearanceProps) {
  const percent = Math.round((brightness / 255) * 100)

  return (
    <Card title="Appearance" icon={Palette}>
      <div className="grid grid-cols-3 gap-3">
        {FIELDS.map(({ key, label }) => (
          <label
            key={key}
            className="flex cursor-pointer flex-col items-center gap-2.5 rounded-2xl border border-white/[0.06] bg-zinc-950/50 p-3 transition hover:border-white/15"
          >
            <span className="relative">
              <span className="absolute inset-0 rounded-xl blur-md" style={{ background: colors[key], opacity: 0.5 }} />
              <input
                type="color"
                value={colors[key]}
                onChange={(e) => onColorsChange({ ...colors, [key]: e.target.value.toUpperCase() })}
                className="relative size-12"
                aria-label={`${label} color`}
              />
            </span>
            <span className="text-center text-xs text-zinc-400">{label}</span>
          </label>
        ))}
      </div>

      <div className="mt-6">
        <div className="mb-2 flex items-baseline justify-between">
          <p className="flex items-center gap-1.5 text-xs font-medium uppercase tracking-wider text-zinc-500">
            <Sun className="size-3.5" />
            Brightness
          </p>
          <span className="font-mono text-sm tabular-nums text-zinc-300">{percent}%</span>
        </div>
        <input
          type="range"
          min={5}
          max={255}
          value={brightness}
          onChange={(e) => onBrightnessChange(Number(e.target.value))}
          aria-label="Brightness"
        />
        <p className="mt-2 text-xs text-zinc-600">On USB power the ESP also caps the strip's current, so bright colors may look dimmer.</p>
      </div>
    </Card>
  )
}
