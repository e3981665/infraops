import { Component, type ErrorInfo, type PropsWithChildren, type ReactNode } from 'react'

interface AppErrorBoundaryState {
  hasError: boolean
}

export class AppErrorBoundary extends Component<PropsWithChildren, AppErrorBoundaryState> {
  public state: AppErrorBoundaryState = {
    hasError: false,
  }

  public static getDerivedStateFromError(): AppErrorBoundaryState {
    return { hasError: true }
  }

  public componentDidCatch(error: Error, errorInfo: ErrorInfo) {
    console.error('InfraOps route render failed.', error, errorInfo)
  }

  public render(): ReactNode {
    if (this.state.hasError) {
      return (
        <section className="status-panel">
          <p className="hero-panel__eyebrow">InfraOps</p>
          <h1>This workspace view could not be loaded.</h1>
          <p>Refresh the page and try again. If the issue persists, check the API and frontend logs.</p>
          <button className="button" type="button" onClick={() => window.location.reload()}>
            Reload view
          </button>
        </section>
      )
    }

    return this.props.children
  }
}
