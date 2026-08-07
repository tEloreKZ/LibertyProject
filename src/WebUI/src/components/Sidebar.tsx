import { BarChart3, CircleHelp, FileClock, House, LogOut, Settings, ShieldCheck, Shirt, UserRound, UsersRound } from 'lucide-react'
import type { Translate } from '../i18n'
import { Logo } from './Logo'
import { sendAction } from '../bridge'

const dashboardItems = [
  [House, 'ui.nav.home'], [UserRound, 'ui.nav.character'], [BarChart3, 'ui.nav.statistics'],
  [Settings, 'ui.nav.settings'], [UsersRound, 'ui.nav.squad'], [ShieldCheck, 'ui.nav.achievements'],
  [FileClock, 'ui.nav.journal'],
] as const
const editorItems = [
  [UserRound, 'ui.editor.step.appearance'], [Shirt, 'ui.editor.step.clothes'],
  [FileClock, 'ui.editor.step.identity'], [ShieldCheck, 'ui.editor.step.preview'],
] as const

export function Sidebar({ mode, t, active = 0 }: { mode: 'dashboard' | 'editor'; t: Translate; active?: number }) {
  const items = mode === 'dashboard' ? dashboardItems : editorItems
  return (
    <aside className="sidebar glass-strong">
      <Logo />
      <nav className="sidebar__nav">
        {items.map(([Icon, key], index) => (
          <button key={key} data-interactive className={`nav-item ${active === index ? 'is-active' : ''}`}
            onClick={() => mode === 'editor' ? sendAction('editor-section', { section: index }) : index > 0 && sendAction('future')}>
            <Icon size={19} strokeWidth={1.7} /><span>{t(key)}</span>
          </button>
        ))}
      </nav>
      <div className="sidebar__bottom">
        <button data-interactive className="nav-item" onClick={() => sendAction('future')}><CircleHelp size={19}/>{t('ui.nav.help')}</button>
        <button data-interactive className="nav-item" onClick={() => sendAction(mode === 'editor' ? 'editor-cancel' : 'close')}><LogOut size={19}/>{t(mode === 'editor' ? 'ui.nav.back' : 'ui.nav.exit')}</button>
        <div className="version"><span>PROJECT LIBERTY v1.0.0</span><strong>Nothing Is Scripted</strong></div>
      </div>
    </aside>
  )
}
