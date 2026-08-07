import { ChevronLeft, ChevronRight, MousePointer2, RotateCw, Search, Shirt, UserRound, FilePenLine, ScanFace } from 'lucide-react'
import type { EditorState } from '../types'
import { sendAction } from '../bridge'
import { Logo } from './Logo'
import { Sidebar } from './Sidebar'

const steps = [[ScanFace,'ВНЕШНОСТЬ'],[Shirt,'ОДЕЖДА'],[FilePenLine,'ИМЯ И ДАННЫЕ'],[Search,'ПРОСМОТР']] as const

export function CharacterEditor({ editor }: { editor: EditorState }) {
  return (
    <div className="screen editor-screen">
      <Sidebar mode="editor" active={editor.section}/>
      <main className="editor-stage" data-interactive
        onPointerMove={event => event.buttons === 1 && Math.abs(event.movementX) > 0 && sendAction('editor-rotate', { delta: event.movementX })}
        onWheel={event => sendAction('editor-zoom', { delta: Math.sign(event.deltaY) })}>
        <div className="editor-steps">{steps.map(([Icon,label],index)=><div className={index===editor.section?'is-active':''} key={label}><span><Icon size={20}/></span><b>{label}</b>{index<3&&<i/>}</div>)}</div>
        <div className="stage-help glass"><MousePointer2 size={18}/> Зажмите ЛКМ и двигайте мышь — поворот <RotateCw size={18}/> Колесо — масштаб</div>
      </main>
      <div className="slot-indicator glass">
        <Logo compact/>
        <span><small>СЛОТ ПЕРСОНАЖА</small><b>ПЕРСОНАЖ {editor.slot}</b><em>{editor.slot} ИЗ 3</em></span>
        <i aria-hidden="true"/>
      </div>
      <EditorPanel editor={editor}/>
    </div>
  )
}

function EditorPanel({ editor }: { editor: EditorState }) {
  const preview = editor.section === 3
  return (
    <aside className="editor-panel panel" data-interactive onWheel={event=>sendAction('editor-scroll',{delta:Math.sign(event.deltaY)})}>
      <h2>{editor.sectionTitle}</h2>
      <div className="editor-panel__accent"/>
      {preview ? <div className="preview-copy"><UserRound size={46}/><h3>{editor.fullName}</h3><strong>{editor.gender}</strong><p>Проверьте внешность персонажа и сохраните его в выбранный слот.</p></div> :
        <div className="editor-rows">{editor.rows.map(row=><div className="editor-row" key={row.index}>
          <span>{row.label}</span>
          {row.kind === 'text' ? <button data-interactive onClick={()=>sendAction('editor-edit-name',{index:row.index})}>{row.value}<FilePenLine size={15}/></button> :
          row.secondaryValue != null ? <div className="dual-stepper">
            <div className="stepper"><button data-interactive onClick={()=>sendAction('editor-adjust',{index:row.index,delta:-1,texture:false})}><ChevronLeft/></button><b>{row.value}</b><button data-interactive onClick={()=>sendAction('editor-adjust',{index:row.index,delta:1,texture:false})}><ChevronRight/></button></div>
            <div className="stepper"><button data-interactive onClick={()=>sendAction('editor-adjust',{index:row.index,delta:-1,texture:true})}><ChevronLeft/></button><b>{row.secondaryValue}</b><button data-interactive onClick={()=>sendAction('editor-adjust',{index:row.index,delta:1,texture:true})}><ChevronRight/></button></div>
          </div> : <div className="stepper"><button data-interactive onClick={()=>sendAction('editor-adjust',{index:row.index,delta:-1})}><ChevronLeft/></button><b>{row.value}</b><button data-interactive onClick={()=>sendAction('editor-adjust',{index:row.index,delta:1})}><ChevronRight/></button></div>}
        </div>)}</div>}
      <div className="scroll-hint">{editor.canScrollUp && '↑ '}{(editor.canScrollUp||editor.canScrollDown) && 'Колесо: другие параметры'}{editor.canScrollDown && ' ↓'}</div>
      <button data-interactive className="primary-button" onClick={()=>sendAction(preview?'editor-save':'editor-next')}>{preview?'СОХРАНИТЬ':'ДАЛЕЕ'}<ChevronRight/></button>
    </aside>
  )
}
