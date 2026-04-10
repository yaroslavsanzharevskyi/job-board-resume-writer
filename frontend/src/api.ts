import type { JobPosting, ResumeResult, StoredResume } from './types'

const API_BASE = import.meta.env.VITE_API_BASE_URL ?? ''

export async function fetchJobs(search?: string, limit = 50): Promise<JobPosting[]> {
  const params = new URLSearchParams({ limit: String(limit) })
  if (search) params.set('search', search)
  const res = await fetch(`${API_BASE}/api/jobs?${params}`)
  if (!res.ok) throw new Error(`Failed to fetch jobs: ${res.statusText}`)
  const data = await res.json()
  return data.jobs as JobPosting[]
}

export async function generateResume(jobId: string, cvText: string): Promise<ResumeResult> {
  const res = await fetch(`${API_BASE}/api/resume/generate`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ jobId, cvText }),
  })
  if (!res.ok) {
    const err = await res.json().catch(() => ({}))
    throw new Error((err as { error?: string }).error ?? `Request failed: ${res.statusText}`)
  }
  return res.json() as Promise<ResumeResult>
}

export async function fetchStoredResumes(): Promise<StoredResume[]> {
  const res = await fetch(`${API_BASE}/api/resumes`)
  if (!res.ok) throw new Error(`Failed to fetch resumes: ${res.statusText}`)
  const data = await res.json()
  return data.resumes as StoredResume[]
}

export async function generateCustomResume(
  jobTitle: string,
  company: string,
  jobDescription: string,
  cvText: string
): Promise<ResumeResult> {
  const res = await fetch(`${API_BASE}/api/resume/generate-custom`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ jobTitle, company, jobDescription, cvText }),
  })
  if (!res.ok) {
    const err = await res.json().catch(() => ({}))
    throw new Error((err as { error?: string }).error ?? `Request failed: ${res.statusText}`)
  }
  return res.json() as Promise<ResumeResult>
}

export async function deleteResume(id: string): Promise<void> {
  const res = await fetch(`${API_BASE}/api/resumes/${id}`, { method: 'DELETE' })
  if (!res.ok) {
    const err = await res.json().catch(() => ({}))
    throw new Error((err as { error?: string }).error ?? `Delete failed: ${res.statusText}`)
  }
}
