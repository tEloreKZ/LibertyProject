import {
  Bell, BookOpen, CarFront, ChevronRight, CircleCheck, Clock3, Crosshair,
  FileCheck2, Medal, PackageOpen, Plus, Radio, ShieldCheck, Siren, Target,
  Trash2, Trophy, UserRound,
} from 'lucide-react'
import { useState } from 'react'
import type { CharacterCard } from '../types'
import type { Translate } from '../i18n'
import { sendAction } from '../bridge'
import { Logo } from './Logo'
import { Sidebar } from './Sidebar'

type DashboardProps = {
  characters: CharacterCard[]
  activeSlot: number | null
  language: string
  t: Translate
}

export function Dashboard({ characters, activeSlot, language, t }: DashboardProps) {
  const empty = characters.length === 0
  const now = new Date()
  return (
    <div className="screen dashboard-screen">
      <Sidebar mode="dashboard" t={t} />
      <main className="dashboard">
        <header className="dashboard__header">
          <div><h1>{t('ui.dashboard.title')}</h1><p>{t('ui.dashboard.welcome')}</p></div>
          <div className="header-widgets">
            {!empty && <><div className="duty-widget"><ShieldCheck/><span><b>{t('ui.dashboard.profile.rank')}</b><small>LSPD</small></span></div><div><Radio/><span><b>10-8</b><small>{language.toUpperCase()}</small></span></div></>}
            <div className="clock-widget"><Clock3/><span><b>{now.toLocaleTimeString(language, {hour:'2-digit',minute:'2-digit'})}</b><small>{now.toLocaleDateString(language)}</small></span></div>
          </div>
        </header>
        {empty ? <EmptyDashboard t={t} /> : <FilledDashboard characters={characters} activeSlot={activeSlot} t={t} />}
      </main>
    </div>
  )
}

function EmptyDashboard({ t }: { t: Translate }) {
  return (
    <section className="empty-state panel">
      <Logo compact />
      <p className="eyebrow">{t('ui.dashboard.empty.eyebrow')}</p>
      <h2>{t('ui.dashboard.empty.title')}</h2>
      <div className="ornament"><span/><ShieldCheck size={25}/><span/></div>
      <p className="empty-state__lead">{t('ui.dashboard.empty.lead')}</p>
      <p>{t('ui.dashboard.empty.description')}</p>
      <button data-interactive className="create-hero" onClick={() => sendAction('create-character', { slot: 1 })}>
        <span className="create-hero__icon"><UserRound/></span><strong>{t('ui.dashboard.empty.create')}</strong><ChevronRight/>
      </button>
    </section>
  )
}

function FilledDashboard({ characters, activeSlot, t }: Omit<DashboardProps, 'language'>) {
  const [deleteSlot, setDeleteSlot] = useState<number | null>(null)
  const active = characters.find(character => character.slot === activeSlot) ?? characters[0]
  const quick = [
    [PackageOpen, 'ui.dashboard.quick.equipment', 'ui.dashboard.quick.equipmentDescription'],
    [CarFront, 'ui.dashboard.quick.vehicle', 'ui.dashboard.quick.vehicleDescription'],
    [Siren, 'ui.dashboard.quick.lastCall', 'ui.dashboard.quick.lastCallDescription'],
    [BookOpen, 'ui.dashboard.quick.code', 'ui.dashboard.quick.codeDescription'],
  ] as const
  const notifications = [
    ['red', 'ui.dashboard.notifications.update', 'ui.dashboard.notifications.updateDescription', ''],
    ['yellow', 'ui.dashboard.notifications.daily', 'ui.dashboard.notifications.dailyDescription', '10:00'],
    ['yellow', 'ui.dashboard.notifications.training', 'ui.dashboard.notifications.trainingDescription', '09:15'],
    ['green', 'ui.dashboard.notifications.report', 'ui.dashboard.notifications.reportDescription', ''],
  ] as const

  return (
    <div className="dashboard-grid dashboard-grid--reference">
      <section className="panel profile-card">
        <span className="section-label">{t('ui.dashboard.profile.title')}</span>
        <div className="character-portrait">
          {active.portrait ? <img src={active.portrait} alt={active.fullName}/> : <UserRound aria-label={active.fullName}/>}
        </div>
        <div className="profile-card__data">
          <h2>{active.fullName}</h2><strong>{t('ui.dashboard.profile.rank')}</strong>
          <dl>
            <div><dt>{t('ui.dashboard.profile.badge')}</dt><dd>#{1240 + active.slot}</dd></div>
            <div><dt>{t('ui.dashboard.profile.department')}</dt><dd>{t('ui.dashboard.profile.departmentValue')}</dd></div>
            <div><dt>{t('ui.dashboard.profile.position')}</dt><dd>{t('ui.dashboard.profile.rank')}</dd></div>
            <div><dt>{t('ui.dashboard.profile.callsign')}</dt><dd>2-ADAM-{10 + active.slot}</dd></div>
            <div><dt>{t('ui.dashboard.profile.serviceTime')}</dt><dd>0 h. 00 min.</dd></div>
          </dl>
          <button data-interactive className="outline-button" onClick={() => sendAction('select-character', { slot: active.slot })}>{t('ui.dashboard.profile.enter')}<ChevronRight/></button>
        </div>
      </section>

      <section className="panel quick"><span className="section-label">{t('ui.dashboard.quick.title')}</span>{quick.map(([Icon,title,description])=><button data-interactive onClick={()=>sendAction('future')} key={title}><Icon/><span><b>{t(title)}</b><small>{t(description)}</small></span><ChevronRight/></button>)}</section>

      <section className="panel notifications"><span className="section-label">{t('ui.dashboard.notifications.title')}</span>{notifications.map(([color,title,description,time])=><div key={title}><i className={`notice-dot notice-dot--${color}`}/><span><b>{t(title)}</b><small>{t(description)}</small></span><time>{time}</time></div>)}<button data-interactive onClick={()=>sendAction('future')}>{t('ui.dashboard.notifications.all')}<em>4</em></button></section>

      <Statistics t={t}/>
      <Achievements t={t}/>

      <section className="panel character-slots"><span className="section-label">{t('ui.dashboard.characters.title')}</span><div className="slot-list">{[1,2,3].map(slot => {
        const item = characters.find(character => character.slot === slot)
        return <div className={`slot-card ${item?.active?'is-active':''}`} key={slot}>
          <button data-interactive className="slot-card__select" onClick={()=>sendAction(item?'select-character':'create-character',{slot})}>
            {item ? <>{item.portrait?<img src={item.portrait} alt=""/>:<UserRound/>}<span><b>{item.fullName}</b><small>{item.gender}</small></span><em>{item.active?t('ui.dashboard.characters.active'):t('ui.dashboard.characters.select')}</em></> : <><Plus/><span><b>{t('ui.dashboard.characters.create')}</b><small>{t('ui.dashboard.characters.freeSlot',slot)}</small></span></>}
          </button>
          {item && <button data-interactive className="slot-card__delete" aria-label={t('ui.dashboard.characters.delete')} title={t('ui.dashboard.characters.delete')} onClick={()=>setDeleteSlot(slot)}><Trash2/></button>}
        </div>
      })}</div></section>
      {deleteSlot !== null && <div className="confirm-backdrop" role="presentation" onMouseDown={()=>setDeleteSlot(null)}>
        <section className="confirm-dialog panel" role="dialog" aria-modal="true" aria-labelledby="delete-character-title" onMouseDown={event=>event.stopPropagation()}>
          <i><Trash2/></i><h2 id="delete-character-title">{t('ui.dashboard.characters.deleteTitle')}</h2>
          <p>{t('ui.dashboard.characters.deleteDescription', characters.find(character=>character.slot===deleteSlot)?.fullName ?? '')}</p>
          <div><button data-interactive onClick={()=>setDeleteSlot(null)}>{t('ui.dashboard.characters.cancel')}</button><button data-interactive className="is-danger" onClick={()=>{sendAction('delete-character',{slot:deleteSlot});setDeleteSlot(null)}}>{t('ui.dashboard.characters.delete')}</button></div>
        </section>
      </div>}
    </div>
  )
}

function Statistics({ t }: { t: Translate }) {
  const cards = [
    [Clock3, 'ui.dashboard.stats.playTime', '0ч. 00м.', 'ui.dashboard.stats.total'],
    [Radio, 'ui.dashboard.stats.calls', '0', 'ui.dashboard.stats.month'],
    [Target, 'ui.dashboard.stats.arrests', '0', 'ui.dashboard.stats.month'],
    [FileCheck2, 'ui.dashboard.stats.fines', '0', 'ui.dashboard.stats.month'],
    [Medal, 'ui.dashboard.stats.level', '1', '0 / 100 XP'],
  ] as const
  return <section className="panel statistics"><span className="section-label">{t('ui.dashboard.stats.title')}</span><div>{cards.map(([Icon,label,value,note])=><article key={label}><Icon/><small>{t(label)}</small><b>{value}</b><em>{note.startsWith('ui.')?t(note):note}</em></article>)}</div></section>
}

function Achievements({ t }: { t: Translate }) {
  const items = [
    [Trophy,'ui.dashboard.achievements.firstDay','ui.dashboard.achievements.firstDayDescription',t('ui.dashboard.achievements.completed')],
    [ShieldCheck,'ui.dashboard.achievements.order','ui.dashboard.achievements.orderDescription','0 / 10'],
    [Medal,'ui.dashboard.achievements.professional','ui.dashboard.achievements.professionalDescription','0 / 50'],
    [Crosshair,'ui.dashboard.achievements.marksman','ui.dashboard.achievements.marksmanDescription','0 / 100'],
  ] as const
  return <section className="panel achievements"><header><span className="section-label">{t('ui.dashboard.achievements.title')}</span><button data-interactive onClick={()=>sendAction('future')}>{t('ui.dashboard.achievements.all')}</button></header>{items.map(([Icon,title,description,progress],index)=><article key={title}><i><Icon/></i><span><b>{t(title)}</b><small>{t(description)}</small></span><em>{index===0&&<CircleCheck/>}{progress}</em></article>)}</section>
}
