import { useState } from 'react'
import type { ResumeResult } from '../types'
import { generateCustomResume } from '../api'
import ResumeOutput from './ResumeOutput'

interface Props {
  ethalonResume: string
  onSaveEthalon: (text: string) => void
}

export default function QuickGenerateForm({ ethalonResume, onSaveEthalon }: Props) {
  const [jobTitle, setJobTitle] = useState('')
  const [company, setCompany] = useState('')
  const [jobDescription, setJobDescription] = useState('')
  const [cvText, setCvText] = useState('')
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [result, setResult] = useState<ResumeResult | null>(null)

  function handleUseEthalon() {
    setCvText(ethalonResume)
  }

  function handleSaveEthalon() {
    if (cvText.trim()) onSaveEthalon(cvText.trim())
  }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    if (!jobDescription.trim() || !cvText.trim()) return
    setLoading(true)
    setError(null)
    try {
      const res = await generateCustomResume(jobTitle.trim(), company.trim(), jobDescription.trim(), cvText.trim())
      setResult(res)
    } catch (err) {
      setError((err as Error).message)
    } finally {
      setLoading(false)
    }
  }

  function handleBack() {
    setResult(null)
  }

  if (result) {
    return <ResumeOutput result={result} onBack={handleBack} />
  }

  return (
    <div className="quick-generate">
      <div className="quick-generate-header">
        <h2>Quick Generate</h2>
        <p>Paste a job description and your CV to generate a tailored resume instantly.</p>
      </div>

      <form onSubmit={handleSubmit}>
        <div className="quick-generate-meta">
          <div className="form-group">
            <label htmlFor="qg-title">Job title</label>
            <input
              id="qg-title"
              type="text"
              placeholder="e.g. Senior Software Engineer"
              value={jobTitle}
              onChange={e => setJobTitle(e.target.value)}
            />
          </div>
          <div className="form-group">
            <label htmlFor="qg-company">Company</label>
            <input
              id="qg-company"
              type="text"
              placeholder="e.g. Acme Corp"
              value={company}
              onChange={e => setCompany(e.target.value)}
            />
          </div>
        </div>

        <div className="quick-generate-fields">
          <div className="form-group quick-generate-col">
            <label htmlFor="qg-description">Job description</label>
            <textarea
              id="qg-description"
              rows={20}
              placeholder="Paste the full job posting here..."
              value={jobDescription}
              onChange={e => setJobDescription(e.target.value)}
              required
            />
          </div>

          <div className="form-group quick-generate-col">
            <div className="cv-label-row">
              <label htmlFor="qg-cv">Your CV / resume</label>
              <div className="cv-label-actions">
                {ethalonResume && (
                  <button type="button" className="btn-hint" onClick={handleUseEthalon}>
                    Use saved resume
                  </button>
                )}
                {cvText.trim() && (
                  <button type="button" className="btn-hint" onClick={handleSaveEthalon}>
                    Save as my resume
                  </button>
                )}
              </div>
            </div>
            <textarea
              id="qg-cv"
              rows={20}
              placeholder="Paste your full CV text here..."
              value={cvText}
              onChange={e => setCvText(e.target.value)}
              required
            />
          </div>
        </div>

        <div className="quick-generate-actions">
          <button
            type="submit"
            className="btn-primary"
            disabled={loading || !jobDescription.trim() || !cvText.trim()}
          >
            {loading ? 'Generating...' : 'Generate tailored resume'}
          </button>
          {error && <div className="error-msg" style={{ marginTop: 0 }}>{error}</div>}
        </div>
      </form>
    </div>
  )
}
