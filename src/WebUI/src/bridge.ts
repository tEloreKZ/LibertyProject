import type { UiState } from './types'

type StateListener = (state: UiState) => void

declare global {
  interface Window {
    liberty?: {
      emit(eventName: string, data?: unknown): void
      on(eventName: string, callback: (data: unknown) => void): void
      off(eventName: string, callback?: (data: unknown) => void): void
    }
  }
}

export function subscribeToState(listener: StateListener): () => void {
  const nativeListener = (state: unknown) => listener(state as UiState)
  window.liberty?.on('ui:state', nativeListener)
  window.liberty?.emit('ui:ready', {})
  return () => window.liberty?.off('ui:state', nativeListener)
}

export function sendAction(action: string, payload: Record<string, unknown> = {}): void {
  window.liberty?.emit('ui:action', { type: 'action', action, ...payload })
}

window.addEventListener('pointermove', event => {
  const cursor = document.getElementById('liberty-cursor')
  if (cursor) cursor.style.transform = `translate3d(${event.clientX}px, ${event.clientY}px, 0)`
}, { passive: true })
