export type ViewName = 'hidden' | 'dashboard' | 'editor'

export interface CharacterCard {
  slot: number
  fullName: string
  gender: string
  active: boolean
}

export interface EditorRow {
  index: number
  label: string
  value: string
  secondaryValue?: string | null
  kind?: 'value' | 'text'
}

export interface EditorState {
  slot: number
  section: number
  sectionTitle: string
  fullName: string
  gender: string
  rows: EditorRow[]
  canScrollUp: boolean
  canScrollDown: boolean
}

export interface UiState {
  type: 'state'
  view: ViewName
  characters: CharacterCard[]
  activeSlot: number | null
  editor?: EditorState
}

export interface PointerMessage {
  type: 'pointer'
  x: number
  y: number
  down: boolean
  pressed: boolean
  released: boolean
  wheel: number
}

export type HostMessage = UiState | PointerMessage
