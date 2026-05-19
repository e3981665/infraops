import { Component, type ErrorInfo, type PropsWithChildren, type ReactNode } from 'react'
import { PageState } from '@/components/ui/PageState'
import { useTranslation } from '@/shared/i18n/useTranslation'

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
      return <AppErrorFallback />
    }

    return this.props.children
  }
}

function AppErrorFallback() {
  const { t } = useTranslation()

  return (
    <PageState
      title={t('common.viewFailed')}
      message={t('common.viewFailedMessage')}
      action={
        <button className="button" type="button" onClick={() => window.location.reload()}>
          {t('common.reloadView')}
        </button>
      }
    />
  )
}
