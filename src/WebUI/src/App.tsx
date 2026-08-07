import { useEffect, useState } from 'react'
import { Dashboard } from './components/Dashboard'
import { CharacterEditor } from './components/CharacterEditor'
import { sendAction, subscribeToState } from './bridge'
import type { UiState } from './types'

const initialState: UiState = { type: 'state', view: 'hidden', characters: [], activeSlot: null }

export function App() {
  const [state, setState] = useState(initialState)
  useEffect(() => subscribeToState(setState), [])
  useEffect(() => {
    const handleKeyDown = (event: KeyboardEvent) => {
      if (event.key !== 'Escape' || event.repeat || state.view === 'hidden') return
      event.preventDefault()
      event.stopPropagation()
      sendAction(state.view === 'editor' ? 'editor-cancel' : 'close')
    }
    window.addEventListener('keydown', handleKeyDown, true)
    return () => window.removeEventListener('keydown', handleKeyDown, true)
  }, [state.view])

  if (state.view === 'hidden') return null
  if (state.view === 'editor' && state.editor) return <CharacterEditor editor={state.editor} />
  return <Dashboard characters={state.characters} activeSlot={state.activeSlot} />
}
