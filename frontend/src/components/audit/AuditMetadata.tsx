import { useTranslation } from '@/shared/i18n/useTranslation'

interface AuditMetadataProps {
  createdBy?: string | null
  updatedBy?: string | null
  submittedBy?: string | null
  createdAtUtc?: string | null
  updatedAtUtc?: string | null
  submittedAtUtc?: string | null
}

export function AuditMetadata({
  createdBy,
  updatedBy,
  submittedBy,
  createdAtUtc,
  updatedAtUtc,
  submittedAtUtc,
}: AuditMetadataProps) {
  const { t } = useTranslation()

  return (
    <article className="card">
      <h2>{t('audit.title')}</h2>
      <dl className="definition-list definition-list--compact">
        {createdBy ? (
          <div>
            <dt>{t('audit.createdBy')}</dt>
            <dd>{createdBy}</dd>
          </div>
        ) : null}
        {createdAtUtc ? (
          <div>
            <dt>{t('audit.createdAt')}</dt>
            <dd>{formatDate(createdAtUtc)}</dd>
          </div>
        ) : null}
        {updatedBy ? (
          <div>
            <dt>{t('audit.updatedBy')}</dt>
            <dd>{updatedBy}</dd>
          </div>
        ) : null}
        {updatedAtUtc ? (
          <div>
            <dt>{t('audit.updatedAt')}</dt>
            <dd>{formatDate(updatedAtUtc)}</dd>
          </div>
        ) : null}
        {submittedBy ? (
          <div>
            <dt>{t('audit.submittedBy')}</dt>
            <dd>{submittedBy}</dd>
          </div>
        ) : null}
        {submittedAtUtc ? (
          <div>
            <dt>{t('audit.submittedAt')}</dt>
            <dd>{formatDate(submittedAtUtc)}</dd>
          </div>
        ) : null}
      </dl>
    </article>
  )
}

function formatDate(value: string) {
  return new Date(value).toLocaleString()
}
