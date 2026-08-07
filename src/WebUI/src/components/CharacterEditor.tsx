import { ChevronLeft, ChevronRight, FilePenLine, MousePointer2, RotateCw, ScanFace, Search, Shirt, UserRound } from 'lucide-react'
import type { EditorRow, EditorState } from '../types'
import type { Translate } from '../i18n'
import { sendAction } from '../bridge'
import { Logo } from './Logo'
import { Sidebar } from './Sidebar'

const stepIcons = [ScanFace, Shirt, FilePenLine, Search] as const
const stepKeys = ['ui.editor.step.appearance','ui.editor.step.clothes','ui.editor.step.identity','ui.editor.step.preview'] as const

export function CharacterEditor({ editor, t }: { editor: EditorState; t: Translate }) {
  return (
    <div className="screen editor-screen">
      <Sidebar mode="editor" active={editor.section} t={t}/>
      <main className="editor-stage" data-interactive
        onPointerMove={event => event.buttons === 1 && Math.abs(event.movementX) > 0 && sendAction('editor-rotate', { delta: event.movementX })}
        onWheel={event => sendAction('editor-zoom', { delta: Math.sign(event.deltaY) })}>
        <div className="editor-steps">{stepIcons.map((Icon,index)=><div className={index===editor.section?'is-active':''} key={stepKeys[index]}><span><Icon size={20}/></span><b>{t(stepKeys[index])}</b>{index<3&&<i/>}</div>)}</div>
        <div className="stage-help glass"><MousePointer2 size={18}/>{t('ui.editor.rotate')}<RotateCw size={18}/>{t('ui.editor.zoom')}</div>
      </main>
      <div className="slot-indicator glass">
        <Logo compact/>
        <span><small>{t('ui.editor.slot')}</small><b>{t('ui.editor.character',editor.slot)}</b><em>{t('ui.editor.slotOf',editor.slot)}</em></span>
        <i aria-hidden="true"/>
      </div>
      <EditorPanel editor={editor} t={t}/>
    </div>
  )
}

function EditorPanel({ editor, t }: { editor: EditorState; t: Translate }) {
  const preview = editor.section === 3
  return (
    <aside className="editor-panel panel" data-interactive onWheel={event=>sendAction('editor-scroll',{delta:Math.sign(event.deltaY)})}>
      <h2>{editor.sectionTitle}</h2>
      <div className="editor-panel__accent"/>
      {preview ? <div className="preview-copy"><UserRound size={46}/><h3>{editor.fullName}</h3><strong>{editor.gender}</strong><p>{t('ui.editor.preview.description')}</p></div> :
        <div className="editor-rows">{editor.rows.map(row=><EditorRowControl row={row} key={row.index}/>)}</div>}
      <div className="scroll-hint">{editor.canScrollUp && '↑ '}{(editor.canScrollUp||editor.canScrollDown) && t('ui.editor.scroll')}{editor.canScrollDown && ' ↓'}</div>
      <button data-interactive className="primary-button" onClick={()=>sendAction(preview?'editor-save':'editor-next')}>{t(preview?'ui.editor.save':'ui.editor.next')}<ChevronRight/></button>
    </aside>
  )
}

function EditorRowControl({ row }: { row: EditorRow }) {
  return <div className={`editor-row editor-row--${row.kind ?? 'value'}`}>
    <span>{row.label}</span>
    {row.kind === 'text' ? <button data-interactive onClick={()=>sendAction('editor-edit-name',{index:row.index})}>{row.value}<FilePenLine size={15}/></button> :
      row.kind === 'slider' ? <div className="slider-control">
        <input data-interactive type="range" min={row.min ?? 0} max={row.max ?? 1} step={row.step ?? .01}
          value={row.numericValue ?? 0} onChange={event=>sendAction('editor-set',{index:row.index,value:Number(event.currentTarget.value)})}/>
        <b>{row.value}</b>
      </div> : row.secondaryValue != null ? <div className="dual-stepper">
        <Stepper value={row.value} onDecrease={()=>sendAction('editor-adjust',{index:row.index,delta:-1,texture:false})} onIncrease={()=>sendAction('editor-adjust',{index:row.index,delta:1,texture:false})}/>
        <Stepper value={row.secondaryValue} onDecrease={()=>sendAction('editor-adjust',{index:row.index,delta:-1,texture:true})} onIncrease={()=>sendAction('editor-adjust',{index:row.index,delta:1,texture:true})}/>
      </div> : <Stepper value={row.value} onDecrease={()=>sendAction('editor-adjust',{index:row.index,delta:-1})} onIncrease={()=>sendAction('editor-adjust',{index:row.index,delta:1})}/>
    }
  </div>
}

function Stepper({ value, onDecrease, onIncrease }: { value: string; onDecrease: () => void; onIncrease: () => void }) {
  return <div className="stepper"><button data-interactive onClick={onDecrease}><ChevronLeft/></button><b>{value}</b><button data-interactive onClick={onIncrease}><ChevronRight/></button></div>
}
