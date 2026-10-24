export type LedState = 'Idle' | 'EarlyWarning' | 'Alert' | 'Safe' | 'Manual'
export type AlertKind = 'Missiles' | 'Uav' | 'EarlyWarning' | 'Ended' | 'Unknown'
export type Effect = 'fill' | 'blink' | 'progress' | 'rainbow' | 'idle'

export interface Status {
  state: LedState
  stateSince: string
  countdownEndsAt: string | null
  tzofarConnected: boolean
  mqttConnected: boolean
  espOnline: boolean
}

export interface City {
  id: number
  name: string
  nameEn: string
}

export interface StateColors {
  earlyWarning: string
  alert: string
  safe: string
}

export interface Settings {
  cities: City[]
  colors: StateColors
  brightness: number
}

export interface AlertItem {
  time: string
  kind: AlertKind
  cities: string[]
}

export interface LedCommand {
  effect: Effect
  color?: string
  brightness?: number
  periodMs?: number
  from?: number
  to?: number
  durationMs?: number
  speedMs?: number
}

async function request<T>(method: string, url: string, body?: unknown): Promise<T> {
  const response = await fetch(url, {
    method,
    headers: body === undefined ? undefined : { 'Content-Type': 'application/json' },
    body: body === undefined ? undefined : JSON.stringify(body),
  })

  if (!response.ok) {
    const text = await response.text()
    throw new Error(text || `${method} ${url} failed (${response.status})`)
  }

  return response.status === 204 ? (undefined as T) : ((await response.json()) as T)
}

export const api = {
  status: () => request<Status>('GET', '/api/status'),
  searchCities: (q: string) => request<City[]>('GET', `/api/cities?q=${encodeURIComponent(q)}`),
  settings: () => request<Settings>('GET', '/api/settings'),
  saveSettings: (s: Settings) =>
    request<Settings>('PUT', '/api/settings', {
      cityIds: s.cities.map((c) => c.id),
      colors: s.colors,
      brightness: s.brightness,
    }),
  sendLed: (command: LedCommand) => request<void>('POST', '/api/led', command),
  forceState: (state: Exclude<LedState, 'Manual'>) => request<void>('POST', `/api/led/state/${state}`),
  alerts: () => request<AlertItem[]>('GET', '/api/alerts'),
}
