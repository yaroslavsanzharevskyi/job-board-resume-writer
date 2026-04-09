import Markdown from 'react-markdown'
import type { ResumeResult } from '../types'

interface Props {
  result: ResumeResult
  onBack: () => void
}

export default function ResumeOutput({ result, onBack }: Props) {
  function downloadMarkdown() {
    const blob = new Blob([result.resumeMarkdown], { type: 'text/markdown' })
    const url = URL.createObjectURL(blob)
    const a = document.createElement('a')
    a.href = url
    a.download = `resume-${result.company.replace(/\s+/g, '-').toLowerCase()}.md`
    a.click()
    URL.revokeObjectURL(url)
  }

  function printPdf() {
    window.print()
  }

  return (
    <div className="resume-output">
      <div className="resume-output-header">
        <div>
          <h2>Tailored resume for {result.jobTitle} @ {result.company}</h2>
        </div>
        <div className="resume-output-actions">
          <button className="btn-secondary" onClick={onBack}>← Back</button>
          <button className="btn-secondary" onClick={downloadMarkdown}>Download .md</button>
          <button className="btn-primary" onClick={printPdf}>Save as PDF</button>
        </div>
      </div>

      <div className="resume-meta">
        Generated {new Date(result.generatedAt).toLocaleString()} · {result.tokensUsed} tokens used
      </div>

      <div className="resume-content" id="resume-print-area">
        <Markdown>{result.resumeMarkdown}</Markdown>
      </div>
    </div>
  )
}
