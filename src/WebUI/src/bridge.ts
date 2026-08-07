import type { HostMessage, PointerMessage, UiState } from './types'

type StateListener = (state: UiState) => void

declare global {
  interface Window {
    liberty?: {
      emit(eventName: string, data?: unknown): void
      on(eventName: string, callback: (data: any) => void): void
      off(eventName: string, callback?: (data: any) => void): void
    }
    chrome?: {
      webview?: {
        postMessage(message: unknown): void
        addEventListener(type: 'message', listener: (event: MessageEvent<HostMessage>) => void): void
      }
    }
  }
}

let stateListener: StateListener | undefined
let pressedElement: HTMLElement | null = null
let hoveredElement: HTMLElement | null = null
let previousX = 0
let previousY = 0

export function subscribeToState(listener: StateListener): () => void {
  stateListener = listener
  const nativeListener = (state: UiState) => listener(state)
  if (window.liberty) {
    window.liberty.on('ui:state', nativeListener)
    window.liberty.emit('ui:ready', {})
  }
  return () => {
    if (stateListener === listener) stateListener = undefined
    window.liberty?.off('ui:state', nativeListener)
  }
}

export function sendAction(action: string, payload: Record<string, unknown> = {}): void {
  const message = { type: 'action', action, ...payload }
  if (window.liberty) window.liberty.emit('ui:action', message)
  else window.chrome?.webview?.postMessage(message)
}

function dispatchPointer(message: PointerMessage): void {
  const x = Math.max(0, Math.min(window.innerWidth - 1, message.x * window.innerWidth))
  const y = Math.max(0, Math.min(window.innerHeight - 1, message.y * window.innerHeight))
  const cursor = document.getElementById('liberty-cursor')
  if (cursor) cursor.style.transform = `translate3d(${x}px, ${y}px, 0)`

  const target = document.elementFromPoint(x, y)
  const interactiveTarget = target?.closest<HTMLElement>('[data-interactive]') ?? null
  const hover = interactiveTarget
  if (hoveredElement !== hover) {
    hoveredElement?.removeAttribute('data-sim-hover')
    hover?.setAttribute('data-sim-hover', 'true')
    hoveredElement = hover
  }

  const movementX = x - previousX
  const movementY = y - previousY
  previousX = x
  previousY = y

  const eventTarget = pressedElement ?? interactiveTarget ?? target
  eventTarget?.dispatchEvent(new PointerEvent('pointermove', {
    bubbles: true, clientX: x, clientY: y, movementX, movementY,
    buttons: message.down ? 1 : 0, pointerId: 1, pointerType: 'mouse', isPrimary: true,
  }))

  if (message.pressed && interactiveTarget) {
    pressedElement = interactiveTarget
    interactiveTarget.dispatchEvent(new PointerEvent('pointerdown', {
      bubbles: true, clientX: x, clientY: y, button: 0, buttons: 1,
      pointerId: 1, pointerType: 'mouse', isPrimary: true,
    }))
  }

  if (message.released) {
    const releaseTarget = pressedElement ?? interactiveTarget ?? target
    releaseTarget?.dispatchEvent(new PointerEvent('pointerup', {
      bubbles: true, clientX: x, clientY: y, button: 0, buttons: 0,
      pointerId: 1, pointerType: 'mouse', isPrimary: true,
    }))
    if (interactiveTarget && pressedElement === interactiveTarget)
      interactiveTarget.dispatchEvent(new MouseEvent('click', { bubbles: true, clientX: x, clientY: y, button: 0 }))
    pressedElement = null
  }

  if (message.wheel !== 0 && (interactiveTarget ?? target))
    (interactiveTarget ?? target)?.dispatchEvent(new WheelEvent('wheel', { bubbles: true, clientX: x, clientY: y, deltaY: message.wheel * 100 }))
}

window.chrome?.webview?.addEventListener('message', event => {
  const message = event.data
  if (message.type === 'state') stateListener?.(message)
  else if (message.type === 'pointer') dispatchPointer(message)
})

if (window.liberty) {
  window.addEventListener('pointermove', event => {
    const cursor = document.getElementById('liberty-cursor')
    if (cursor) cursor.style.transform = `translate3d(${event.clientX}px, ${event.clientY}px, 0)`
  }, { passive: true })
}
