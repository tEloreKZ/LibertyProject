import { CircleHelp, FileClock, House, LogOut, Settings, ShieldCheck, Shirt, UserRound, UsersRound } from 'lucide-react'
import { Logo } from './Logo'
import { sendAction } from '../bridge'

const dashboardItems = [
  [House, 'Главная'], [UserRound, 'Персонаж'], [Settings, 'Настройки мода'],
  [UsersRound, 'Управление отрядом'], [FileClock, 'Журнал событий'], [ShieldCheck, 'Достижения'],
]
const editorItems = [[UserRound, 'Внешность'], [Shirt, 'Одежда'], [FileClock, 'Имя и данные'], [ShieldCheck, 'Предпросмотр']]

export function Sidebar({ mode, active = 0 }: { mode: 'dashboard' | 'editor'; active?: number }) {
  const items = mode === 'dashboard' ? dashboardItems : editorItems
  return (
    <aside className="sidebar glass-strong">
      <Logo />
      <nav className="sidebar__nav">
        {items.map(([Icon, label], index) => (
          <button key={label as string} data-interactive className={`nav-item ${active === index ? 'is-active' : ''}`}
            onClick={() => mode === 'editor' ? sendAction('editor-section', { section: index }) : index > 0 && sendAction('future')}>
            <Icon size={19} strokeWidth={1.7} /> <span>{label as string}</span>
          </button>
        ))}
      </nav>
      <div className="sidebar__bottom">
        <button data-interactive className="nav-item" onClick={() => sendAction('future')}><CircleHelp size={19}/>Помощь</button>
        <button data-interactive className="nav-item" onClick={() => sendAction(mode === 'editor' ? 'editor-cancel' : 'close')}><LogOut size={19}/>Вернуться</button>
        <div className="version"><span>PROJECT LIBERTY v1.0.0</span><strong>Nothing Is Scripted</strong></div>
      </div>
    </aside>
  )
}
