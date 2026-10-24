import type { AlertKind, LedState, StateColors } from './api'

export const IDLE_COLOR = '#71717a'
export const MANUAL_COLOR = '#a78bfa'

export const stateLabels: Record<LedState, string> = {
  Idle: 'All quiet',
  EarlyWarning: 'Early warning',
  Alert: 'Red alert',
  Safe: 'All clear',
  Manual: 'Manual control',
}

export const stateHints: Record<LedState, string> = {
  Idle: 'No active alerts in your cities',
  EarlyWarning: 'Alerts expected soon — stay near a shelter',
  Alert: 'Enter the protected space',
  Safe: 'The incident has ended',
  Manual: 'Showing your own effect until the next alert',
}

export function stateColor(state: LedState, colors?: StateColors) {
  switch (state) {
    case 'EarlyWarning':
      return colors?.earlyWarning ?? '#FF4500'
    case 'Alert':
      return colors?.alert ?? '#FF0000'
    case 'Safe':
      return colors?.safe ?? '#00FF00'
    case 'Manual':
      return MANUAL_COLOR
    default:
      return IDLE_COLOR
  }
}

export const kindLabels: Record<AlertKind, string> = {
  Missiles: 'Missiles',
  Uav: 'Hostile aircraft',
  EarlyWarning: 'Early warning',
  Ended: 'Ended',
  Unknown: 'Other',
}

export function kindColor(kind: AlertKind, colors?: StateColors) {
  switch (kind) {
    case 'Missiles':
    case 'Uav':
      return stateColor('Alert', colors)
    case 'EarlyWarning':
      return stateColor('EarlyWarning', colors)
    case 'Ended':
      return stateColor('Safe', colors)
    default:
      return IDLE_COLOR
  }
}
