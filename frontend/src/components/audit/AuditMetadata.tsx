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
  return (
    <article className="card">
      <h2>Audit</h2>
      <dl className="definition-list definition-list--compact">
        {createdBy ? (
          <div>
            <dt>Created by</dt>
            <dd>{createdBy}</dd>
          </div>
        ) : null}
        {createdAtUtc ? (
          <div>
            <dt>Created at</dt>
            <dd>{formatDate(createdAtUtc)}</dd>
          </div>
        ) : null}
        {updatedBy ? (
          <div>
            <dt>Updated by</dt>
            <dd>{updatedBy}</dd>
          </div>
        ) : null}
        {updatedAtUtc ? (
          <div>
            <dt>Updated at</dt>
            <dd>{formatDate(updatedAtUtc)}</dd>
          </div>
        ) : null}
        {submittedBy ? (
          <div>
            <dt>Submitted by</dt>
            <dd>{submittedBy}</dd>
          </div>
        ) : null}
        {submittedAtUtc ? (
          <div>
            <dt>Submitted at</dt>
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
