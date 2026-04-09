import { useState, useEffect, useCallback } from 'react'
import type { StoredResume } from '../types'
import { fetchStoredResumes, deleteResume } from '../api'

export default function ResumeList() {
  const [resumes, setResumes] = useState<StoredResume[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [deleting, setDeleting] = useState<string | null>(null)

  const handleDelete = useCallback(async (id: string) => {
    setDeleting(id)
    try {
      await deleteResume(id)
      setResumes(prev => prev.filter(r => r.id !== id))
    } catch (e) {
      setError((e as Error).message)
    } finally {
      setDeleting(null)
    }
  }, [])

  const load = useCallback(async () => {
    setLoading(true)
    setError(null)
    try {
      setResumes(await fetchStoredResumes())
    } catch (e) {
      setError((e as Error).message)
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => { load() }, [load])

  if (loading) {
    return <div className="job-list-status">Loading saved resumes…</div>
  }

  if (error) {
    return (
      <div style={{ padding: '16px' }}>
        <div className="error-msg">{error}</div>
        <button className="btn-secondary" style={{ marginTop: 10 }} onClick={load}>Retry</button>
      </div>
    )
  }

  if (resumes.length === 0) {
    return (
      <div className="empty-state" style={{ padding: '40px 16px' }}>
        <div className="empty-state-icon">📄</div>
        <p>No saved resumes yet.<br />Generate one to see it here.</p>
      </div>
    )
  }

  return (
    <div className="resume-list-items">
      {resumes.map(r => (
        <div key={r.id} className="resume-list-item">
          <div className="resume-list-title">{r.jobTitle}</div>
          <div className="resume-list-meta">
            <span>{r.company}</span>
            <span>{new Date(r.generatedAt).toLocaleDateString()}</span>
            <span>{r.tokensUsed} tokens</span>
          </div>
          <div style={{ display: 'flex', gap: 8 }}>
            <a
              className="btn-secondary resume-list-download"
              href={r.downloadUrl}
              target="_blank"
              rel="noreferrer"
              download
            >
              Download PDF
            </a>
            <button
              className="btn-secondary"
              onClick={() => handleDelete(r.id)}
              disabled={deleting === r.id}
            >
              {deleting === r.id ? 'Deleting…' : 'Delete'}
            </button>
          </div>
        </div>
      ))}
    </div>
  )
}
