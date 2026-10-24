import { Globe, MapPin, Plus, Search, X } from 'lucide-react'
import { useEffect, useRef, useState } from 'react'
import { api, type City } from '../api'
import { useDebounced } from '../hooks'
import { Card } from './Card'
import { useToast } from './Toast'

interface CityPickerProps {
  cities: City[]
  onChange: (cities: City[]) => void
}

export function CityPicker({ cities, onChange }: CityPickerProps) {
  const toast = useToast()
  const [query, setQuery] = useState('')
  const [results, setResults] = useState<City[]>([])
  const [open, setOpen] = useState(false)
  const [highlight, setHighlight] = useState(0)
  const debounced = useDebounced(query, 200)
  const boxRef = useRef<HTMLDivElement>(null)

  useEffect(() => {
    if (!debounced.trim()) {
      setResults([])
      return
    }

    let cancelled = false
    api
      .searchCities(debounced)
      .then((r) => {
        if (!cancelled) {
          setResults(r)
          setHighlight(0)
        }
      })
      .catch(() => toast('City search failed'))
    return () => {
      cancelled = true
    }
  }, [debounced, toast])

  useEffect(() => {
    const onClick = (e: MouseEvent) => {
      if (!boxRef.current?.contains(e.target as Node)) setOpen(false)
    }
    document.addEventListener('mousedown', onClick)
    return () => document.removeEventListener('mousedown', onClick)
  }, [])

  const selectedIds = new Set(cities.map((c) => c.id))
  const options = results.filter((c) => !selectedIds.has(c.id))

  const add = (city: City) => {
    onChange([...cities, city])
    setQuery('')
    setResults([])
  }

  const remove = (id: number) => onChange(cities.filter((c) => c.id !== id))

  return (
    <Card title="Your cities" icon={MapPin} className="relative z-10">
      <div ref={boxRef} className="relative">
        <label className="flex items-center gap-2 rounded-2xl border border-white/[0.08] bg-zinc-950/60 px-4 py-3 focus-within:border-white/20">
          <Search className="size-4 text-zinc-500" />
          <input
            value={query}
            onChange={(e) => {
              setQuery(e.target.value)
              setOpen(true)
            }}
            onFocus={() => setOpen(true)}
            onKeyDown={(e) => {
              if (e.key === 'ArrowDown') {
                e.preventDefault()
                setHighlight((h) => Math.min(h + 1, options.length - 1))
              } else if (e.key === 'ArrowUp') {
                e.preventDefault()
                setHighlight((h) => Math.max(h - 1, 0))
              } else if (e.key === 'Enter' && options[highlight]) {
                e.preventDefault()
                add(options[highlight])
              } else if (e.key === 'Escape') {
                setOpen(false)
              }
            }}
            dir="auto"
            placeholder="Search a city — עברית or English"
            className="w-full bg-transparent text-sm outline-none placeholder:text-zinc-600"
          />
        </label>

        {open && options.length > 0 && (
          <ul className="absolute inset-x-0 top-full z-20 mt-2 max-h-72 overflow-auto rounded-2xl border border-white/10 bg-zinc-900 p-1.5 shadow-2xl shadow-black/50">
            {options.map((city, i) => (
              <li key={city.id}>
                <button
                  type="button"
                  onMouseEnter={() => setHighlight(i)}
                  onClick={() => add(city)}
                  className={`flex w-full items-center justify-between gap-3 rounded-xl px-3 py-2.5 text-left text-sm ${
                    i === highlight ? 'bg-white/[0.06]' : ''
                  }`}
                >
                  <span>
                    <span dir="rtl" className="font-medium">
                      {city.name}
                    </span>
                    <span className="ml-2 text-zinc-500">{city.nameEn}</span>
                  </span>
                  <Plus className="size-4 text-zinc-500" />
                </button>
              </li>
            ))}
          </ul>
        )}
      </div>

      <div className="mt-4">
        {cities.length === 0 ? (
          <div className="flex items-center gap-3 rounded-2xl border border-dashed border-white/10 px-4 py-3.5 text-sm text-zinc-400">
            <Globe className="size-5 shrink-0 text-zinc-500" />
            <span>
              No cities selected — <span className="font-medium text-zinc-200">all of Israel</span> triggers the LEDs.
            </span>
          </div>
        ) : (
          <ul className="flex flex-wrap gap-2">
            {cities.map((city) => (
              <li
                key={city.id}
                className="group flex items-center gap-1.5 rounded-full border border-white/[0.08] bg-white/[0.04] py-1.5 pl-3.5 pr-1.5 text-sm"
              >
                <span dir="rtl">{city.name}</span>
                <button
                  type="button"
                  onClick={() => remove(city.id)}
                  aria-label={`Remove ${city.nameEn || city.name}`}
                  className="rounded-full p-1 text-zinc-500 hover:bg-white/10 hover:text-zinc-200"
                >
                  <X className="size-3.5" />
                </button>
              </li>
            ))}
          </ul>
        )}
      </div>
    </Card>
  )
}
