export interface JobPosting {
  id: string
  title: string
  company: string
  location: string
  description: string
  salaryMin?: string
  salaryMax?: string
  salaryMid?: string
  contractType?: string
  contractTime?: string
  category?: string
  categoryTag?: string
  createdAt: string
  skills: string[]
  seniority?: string
  workMode?: string
  redirectUrl?: string
  sourceCountry?: string
}

export interface ResumeResult {
  resumeId: string
  jobId: string
  jobTitle: string
  company: string
  resumeMarkdown: string
  blobUrl: string
  tokensUsed: number
  generatedAt: string
}

export interface StoredResume {
  id: string
  jobId: string
  jobTitle: string
  company: string
  downloadUrl: string
  tokensUsed: number
  generatedAt: string
}
