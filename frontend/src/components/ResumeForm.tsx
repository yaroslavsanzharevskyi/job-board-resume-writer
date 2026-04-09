import { useState } from 'react'
import type { JobPosting, ResumeResult } from '../types'
import { generateResume } from '../api'

interface Props {
  job: JobPosting | null
  onGenerated: (result: ResumeResult) => void
  ethalonResume: string
  onSaveEthalon: (text: string) => void
}

export default function ResumeForm({ job, onGenerated, ethalonResume, onSaveEthalon }: Props) {
  const [cvText, setCvText] = useState('')
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  if (!job) {
    return (
      <div className="empty-state" style={{ height: '100%', padding: '24px' }}>
        <div className="empty-state-icon">←</div>
        <p>Select a job from the list to get started.</p>
      </div>
    )
  }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    if (!cvText.trim()) return
    setLoading(true)
    setError(null)
    try {
      const result = await generateResume(job!.id, cvText)
      onGenerated(result)
    } catch (e) {
      setError((e as Error).message)
    } finally {
      setLoading(false)
    }
  }

  const salaryDisplay = job.salaryMid
    ? `£${job.salaryMid} mid`
    : [job.salaryMin, job.salaryMax].filter(Boolean).join('–')
      ? `£${[job.salaryMin, job.salaryMax].filter(Boolean).join('–')}`
      : null

  return (
    <div className="resume-form">
      <div className="job-card">
        <div className="job-card-title">{job.title}</div>
        <div className="job-card-meta">
          <span>{job.company}</span>
          <span>{job.location}</span>
          {salaryDisplay && <span>{salaryDisplay}</span>}
          {job.seniority && job.seniority !== 'Unknown' && <span>{job.seniority}</span>}
          {job.workMode && job.workMode !== 'unknown' && <span>{job.workMode}</span>}
          {job.contractType && <span>{job.contractType}</span>}
        </div>
        {job.skills.length > 0 && (
          <div className="job-item-tags" style={{ marginTop: 8 }}>
            {job.skills.map(s => <span key={s} className="tag">{s}</span>)}
          </div>
        )}
        {job.description && (
          <div className="job-card-description">{job.description}</div>
        )}
        {job.redirectUrl && (
          <div style={{ marginTop: 8 }}>
            <a href={job.redirectUrl} target="_blank" rel="noreferrer"
               style={{ fontSize: 12, color: 'var(--accent)' }}>View original posting ↗</a>
          </div>
        )}
      </div>

      <form onSubmit={handleSubmit}>
        <div className="form-group">
          <div className="cv-label-row">
            <label htmlFor="cv-text">Your current CV / resume</label>
            <div className="cv-label-actions">
              {ethalonResume && (
                <button type="button" className="btn-hint" onClick={() => setCvText(ethalonResume)}>
                  Use saved resume
                </button>
              )}
              {cvText.trim() && (
                <button type="button" className="btn-hint" onClick={() => onSaveEthalon(cvText.trim())}>
                  Save as my resume
                </button>
              )}
            </div>
          </div>
          <textarea
            id="cv-text"
            rows={18}
            placeholder="Paste your full CV text here..."
            value={cvText}
            onChange={e => setCvText(e.target.value)}
            required
          />
        </div>

        <button
          type="submit"
          className="btn-primary"
          disabled={loading || !cvText.trim()}
        >
          {loading ? 'Generating...' : 'Generate tailored resume'}
        </button>

        {error && <div className="error-msg">{error}</div>}
      </form>
    </div>
  )
}
