import { useState } from 'react'
import type { JobPosting, ResumeResult } from './types'
import JobList from './components/JobList'
import ResumeForm from './components/ResumeForm'
import ResumeOutput from './components/ResumeOutput'
import ResumeList from './components/ResumeList'
import QuickGenerateForm from './components/QuickGenerateForm'

type GlobalTab = 'jobs' | 'quick' | 'resumes'

export default function App() {
  const [tab, setTab] = useState<GlobalTab>('jobs')
  const [selectedJob, setSelectedJob] = useState<JobPosting | null>(null)
  const [result, setResult] = useState<ResumeResult | null>(null)
  const [ethalonResume, setEthalonResume] = useState(() =>
    localStorage.getItem('ethalon-resume') ?? ''
  )

  function saveEthalonResume(text: string) {
    setEthalonResume(text)
    localStorage.setItem('ethalon-resume', text)
  }

  function handleJobSelect(job: JobPosting) {
    setSelectedJob(job)
    setResult(null)
  }

  function handleTabChange(next: GlobalTab) {
    setTab(next)
    setResult(null)
  }

  return (
    <div className="app">
      <header className="app-header">
        <div className="app-header-top">
          <div>
            <h1>AI Resume Generator</h1>
            <p>Select a job posting or paste a description, then generate a tailored resume.</p>
          </div>
          {ethalonResume && (
            <div className="ethalon-badge" title="You have a saved resume">
              Resume saved
            </div>
          )}
        </div>
        <nav className="global-tabs">
          <button
            className={`global-tab${tab === 'jobs' ? ' global-tab-active' : ''}`}
            onClick={() => handleTabChange('jobs')}
          >
            Jobs
          </button>
          <button
            className={`global-tab${tab === 'quick' ? ' global-tab-active' : ''}`}
            onClick={() => handleTabChange('quick')}
          >
            Quick Generate
          </button>
          <button
            className={`global-tab${tab === 'resumes' ? ' global-tab-active' : ''}`}
            onClick={() => handleTabChange('resumes')}
          >
            Saved Resumes
          </button>
        </nav>
      </header>

      <main className="app-body">
        {tab === 'jobs' && (
          <>
            <aside className="panel panel-left">
              <JobList selectedJobId={selectedJob?.id} onSelect={handleJobSelect} />
            </aside>
            <section className="panel panel-right">
              {result ? (
                <ResumeOutput result={result} onBack={() => setResult(null)} />
              ) : (
                <ResumeForm
                  job={selectedJob}
                  onGenerated={setResult}
                  ethalonResume={ethalonResume}
                  onSaveEthalon={saveEthalonResume}
                />
              )}
            </section>
          </>
        )}

        {tab === 'quick' && (
          <div className="panel panel-full">
            <QuickGenerateForm
              ethalonResume={ethalonResume}
              onSaveEthalon={saveEthalonResume}
            />
          </div>
        )}

        {tab === 'resumes' && (
          <div className="panel panel-full">
            <ResumeList />
          </div>
        )}
      </main>
    </div>
  )
}
