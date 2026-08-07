import { Bell, ChevronRight, Clock3, Plus, ShieldCheck, Trophy, UserRound } from 'lucide-react'
import type { CharacterCard } from '../types'
import { sendAction } from '../bridge'
import { Logo } from './Logo'
import { Sidebar } from './Sidebar'

export function Dashboard({ characters, activeSlot }: { characters: CharacterCard[]; activeSlot: number | null }) {
  const empty = characters.length === 0
  return (
    <div className="screen dashboard-screen">
      <Sidebar mode="dashboard" />
      <main className="dashboard">
        <header className="dashboard__header">
          <div><h1>ГЛАВНАЯ</h1><p>Добро пожаловать в Project Liberty</p></div>
          <div className="header-widgets"><div><Trophy size={19}/></div><div><UserRound size={19}/></div><div><Clock3 size={19}/><b>{new Date().toLocaleTimeString('ru-RU', {hour:'2-digit',minute:'2-digit'})}</b></div></div>
        </header>

        {empty ? <EmptyDashboard /> : <FilledDashboard characters={characters} activeSlot={activeSlot} />}
      </main>
    </div>
  )
}

function EmptyDashboard() {
  return (
    <section className="empty-state panel">
      <div className="empty-state__shade" />
      <Logo compact />
      <p className="eyebrow">С О З Д А Й Т Е&nbsp;&nbsp; С В О Е Г О</p>
      <h2>П Е Р С О Н А Ж А</h2>
      <div className="ornament"><span/><ShieldCheck size={25}/><span/></div>
      <p className="empty-state__lead">Начните свою историю в Лос-Сантосе.</p>
      <p>Создайте уникального персонажа и станьте частью Project Liberty.</p>
      <button data-interactive className="create-hero" onClick={() => sendAction('create-character', { slot: 1 })}>
        <span className="create-hero__icon"><UserRound/></span><strong>С О З Д А Т Ь&nbsp;&nbsp; П Е Р С О Н А Ж А</strong><ChevronRight/>
      </button>
    </section>
  )
}

function FilledDashboard({ characters, activeSlot }: { characters: CharacterCard[]; activeSlot: number | null }) {
  const active = characters.find(character => character.slot === activeSlot) ?? characters[0]
  return (
    <div className="dashboard-grid">
      <section className="panel profile-card">
        <span className="section-label">МОЙ ПЕРСОНАЖ</span>
        <div className="avatar-placeholder"><UserRound size={68}/></div>
        <div className="profile-card__data"><h2>{active.fullName}</h2><strong>Персонаж {active.slot} / 3</strong><p>{active.gender}</p>
          <button data-interactive className="outline-button" onClick={() => sendAction('select-character', { slot: active.slot })}>ПЕРЕЙТИ К ПЕРСОНАЖУ <ChevronRight size={17}/></button>
        </div>
      </section>
      <section className="panel quick"><span className="section-label">БЫСТРЫЙ ДОСТУП</span>{['Взять снаряжение','Служебный транспорт','Последний вызов','Кодекс и правила'].map(x=><button data-interactive onClick={()=>sendAction('future')} key={x}><ShieldCheck/><span>{x}<small>Функция следующей фазы</small></span><ChevronRight/></button>)}</section>
      <section className="panel notifications"><span className="section-label">УВЕДОМЛЕНИЯ</span>{['Обновление проекта','Ежедневное задание','Персонаж сохранён'].map((x,i)=><div key={x}><Bell size={15}/><span>{x}<small>{i?'Сегодня':'Новая версия интерфейса'}</small></span></div>)}</section>
      <section className="panel character-slots"><span className="section-label">ПЕРСОНАЖИ</span><div className="slot-list">{[1,2,3].map(slot => {
        const item = characters.find(character => character.slot === slot)
        return <button data-interactive className={`slot-card ${item?.active?'is-active':''}`} key={slot} onClick={()=>sendAction(item?'select-character':'create-character',{slot})}>
          {item ? <><UserRound/><span><b>{item.fullName}</b><small>Персонаж {slot} / 3</small></span><em>{item.active?'АКТИВЕН':'ВЫБРАТЬ'}</em></> : <><Plus/><span><b>Создать персонажа</b><small>Свободный слот {slot}</small></span></>}
        </button>
      })}</div></section>
    </div>
  )
}
