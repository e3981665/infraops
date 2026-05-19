import { PageState } from '@/components/ui/PageState'
import { useTranslation } from '@/shared/i18n/useTranslation'

export function RouteLoadingFallback() {
  const { t } = useTranslation()

  return <PageState className="status-panel--compact" title={t('common.loadingWorkspace')} />
}
