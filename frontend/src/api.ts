import type { JobPosting, ResumeResult, StoredResume } from './types'
import { msalInstance, apiScopes } from './authConfig'

const API_BASE = import.meta.env.VITE_API_BASE_URL ?? ''

async function getAuthHeaders(): Promise<Record<string, string>> {
  const account = msalInstance.getActiveAccount() ?? msalInstance.getAllAccounts()[0]
  if (!account) return {}

  const result = await msalInstance.acquireTokenSilent({ scopes: apiScopes, account })
  return { Authorization: `Bearer ${result.accessToken}` }
}

async function apiFetch(path: string, init?: RequestInit): Promise<Response> {
  const authHeaders = await getAuthHeaders()
  return fetch(`${API_BASE}${path}`, {
    ...init,
    headers: { ...init?.headers, ...authHeaders },
  })
}

export async function fetchJobs(search?: string, limit = 50): Promise<JobPosting[]> {
  const params = new URLSearchParams({ limit: String(limit) })
  if (search) params.set('search', search)
  const res = await apiFetch(`/api/jobs?${params}`)
  if (!res.ok) throw new Error(`Failed to fetch jobs: ${res.statusText}`)
  const data = await res.json()
  return data.jobs as JobPosting[]
}

export async function generateResume(jobId: string, cvText: string): Promise<ResumeResult> {
  const res = await apiFetch('/api/resume/generate', {
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
  const res = await apiFetch('/api/resumes')
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
  const res = await apiFetch('/api/resume/generate-custom', {
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
  const res = await apiFetch(`/api/resumes/${id}`, { method: 'DELETE' })
  if (!res.ok) {
    const err = await res.json().catch(() => ({}))
    throw new Error((err as { error?: string }).error ?? `Delete failed: ${res.statusText}`)
  }
}
