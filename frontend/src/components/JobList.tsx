import { useEffect, useRef, useState } from 'react'
import type { JobPosting } from '../types'
import { fetchJobs } from '../api'

interface Props {
  selectedJobId: string | undefined
  onSelect: (job: JobPosting) => void
}

export default function JobList({ selectedJobId, onSelect }: Props) {
  const [jobs, setJobs] = useState<JobPosting[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [search, setSearch] = useState('')
  const inputRef = useRef<HTMLInputElement>(null)

  async function load(term?: string) {
    setLoading(true)
    setError(null)
    try {
      const data = await fetchJobs(term || undefined, 100)
      setJobs(data)
    } catch (e) {
      setError((e as Error).message)
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => { load() }, [])

  function handleSearch(e: React.FormEvent) {
    e.preventDefault()
    load(search.trim())
  }

  function formatSalary(min?: string, max?: string) {
    if (!min && !max) return null
    if (min && max) return `£${min}–£${max}`
    return `£${min ?? max}`
  }

  return (
    <div className="job-list">
      <form className="job-list-search" onSubmit={handleSearch}>
        <input
          ref={inputRef}
          type="text"
          placeholder="Search jobs..."
          value={search}
          onChange={e => setSearch(e.target.value)}
        />
        <button type="submit">Search</button>
      </form>

      {!loading && !error && (
        <div className="job-list-count">{jobs.length} job{jobs.length !== 1 ? 's' : ''}</div>
      )}

      <div className="job-list-items">
        {loading && <div className="job-list-status">Loading jobs...</div>}
        {error && <div className="job-list-status" style={{ color: '#fca5a5' }}>{error}</div>}
        {!loading && !error && jobs.length === 0 && (
          <div className="job-list-status">No jobs found.</div>
        )}
        {jobs.map(job => (
          <div
            key={job.id}
            className={`job-item${job.id === selectedJobId ? ' selected' : ''}`}
            onClick={() => onSelect(job)}
          >
            <div className="job-item-title">{job.title}</div>
            <div className="job-item-meta">{job.company} · {job.location}</div>
            <div className="job-item-tags">
              {job.seniority && job.seniority !== 'Unknown' && <span className="tag">{job.seniority}</span>}
              {job.workMode && job.workMode !== 'unknown' && <span className="tag">{job.workMode}</span>}
              {job.contractType && <span className="tag">{job.contractType}</span>}
              {job.category && <span className="tag">{job.category}</span>}
              {formatSalary(job.salaryMin, job.salaryMax) && (
                <span className="tag">{formatSalary(job.salaryMin, job.salaryMax)}</span>
              )}
            </div>
            {job.skills.length > 0 && (
              <div className="job-item-tags" style={{ marginTop: 4 }}>
                {job.skills.slice(0, 5).map(s => (
                  <span key={s} className="tag" style={{ opacity: 0.7 }}>{s}</span>
                ))}
                {job.skills.length > 5 && (
                  <span className="tag" style={{ opacity: 0.5 }}>+{job.skills.length - 5}</span>
                )}
              </div>
            )}
          </div>
        ))}
      </div>
    </div>
  )
}
