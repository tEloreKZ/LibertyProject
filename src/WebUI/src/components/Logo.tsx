export function Logo({ compact = false }: { compact?: boolean }) {
  return (
    <div className={`brand ${compact ? 'brand--compact' : ''}`}>
      <img src="./project-liberty-logo.png" alt="Project Liberty" />
      {!compact && <div className="brand__text"><strong>PROJECT</strong><strong>LIBERTY</strong><span>Hard RP Mod</span></div>}
    </div>
  )
}
