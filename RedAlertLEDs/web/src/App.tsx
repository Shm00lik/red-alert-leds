import { useCallback, useEffect, useRef, useState } from 'react'
import { api, type Settings } from './api'
import { Appearance } from './components/Appearance'
import { CityPicker } from './components/CityPicker'
import { LedControl } from './components/LedControl'
import { RecentAlerts } from './components/RecentAlerts'
import { StatusHero } from './components/StatusHero'
import { useToast } from './components/Toast'
import { usePolling } from './hooks'

export default function App() {
  const toast = useToast()
  const { data: status, error: statusError } = usePolling(api.status, 2000)
  const [settings, setSettings] = useState<Settings | null>(null)
  const saveTimer = useRef<ReturnType<typeof setTimeout>>(undefined)

  useEffect(() => {
    api
      .settings()
      .then(setSettings)
      .catch(() => toast("Couldn't load settings"))
  }, [toast])

  // Apply locally right away, save to the server shortly after (color pickers and sliders fire constantly).
  const update = useCallback(
    (next: Settings, delayMs = 400) => {
      setSettings(next)
      clearTimeout(saveTimer.current)
      saveTimer.current = setTimeout(() => {
        api
          .saveSettings(next)
          .then(setSettings)
          .catch((e) => toast(e instanceof Error ? e.message : "Couldn't save settings"))
      }, delayMs)
    },
    [toast],
  )

  return (
    <div className="mx-auto flex max-w-5xl flex-col gap-4 px-4 pb-16 pt-6 sm:gap-5 sm:px-6 sm:pt-10">
      <header className="flex items-center justify-between px-1">
        <div className="flex items-center gap-2.5">
          <span className="relative flex size-3">
            <span className="absolute inline-flex size-full animate-ping rounded-full bg-red-500 opacity-40" />
            <span className="relative inline-flex size-3 rounded-full bg-red-500" />
          </span>
          <span className="text-sm font-semibold tracking-wide text-zinc-300">Red Alert LEDs</span>
        </div>
      </header>

      <StatusHero status={status} colors={settings?.colors} offline={statusError} />

      <div className="grid gap-4 sm:gap-5 lg:grid-cols-2">
        <div className="flex flex-col gap-4 sm:gap-5">
          {settings && <CityPicker cities={settings.cities} onChange={(cities) => update({ ...settings, cities }, 0)} />}
          {settings && (
            <Appearance
              colors={settings.colors}
              brightness={settings.brightness}
              onColorsChange={(colors) => update({ ...settings, colors })}
              onBrightnessChange={(brightness) => update({ ...settings, brightness })}
            />
          )}
        </div>
        <div className="flex flex-col gap-4 sm:gap-5">
          <LedControl colors={settings?.colors} />
          <RecentAlerts colors={settings?.colors} />
        </div>
      </div>
    </div>
  )
}
