export type ViewName = 'hidden' | 'dashboard' | 'editor'

export interface CharacterCard {
  slot: number
  fullName: string
  gender: string
  active: boolean
  portrait?: string | null
}

export interface EditorRow {
  index: number
  label: string
  value: string
  secondaryValue?: string | null
  kind?: 'value' | 'text' | 'slider'
  numericValue?: number | null
  min?: number
  max?: number
  step?: number
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
  language: string
  translations: Record<string, string>
  characters: CharacterCard[]
  activeSlot: number | null
  editor?: EditorState
}
