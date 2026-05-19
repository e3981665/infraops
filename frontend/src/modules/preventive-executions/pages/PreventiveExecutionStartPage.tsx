import { useState } from 'react'
import { useMutation, useQuery } from '@tanstack/react-query'
import { useNavigate } from 'react-router-dom'
import { useAuthSession } from '@/modules/auth/hooks/useAuthSession'
import { inventoryClient } from '@/modules/inventory/api/inventory-client'
import { inventoryQueryKeys } from '@/modules/inventory/api/inventory-query-keys'
import { PreventiveExecutionForm } from '@/modules/preventive-executions/components/PreventiveExecutionForm'
import { preventiveExecutionsClient } from '@/modules/preventive-executions/api/preventive-executions-client'
import { preventiveExecutionQueryKeys } from '@/modules/preventive-executions/api/preventive-execution-query-keys'
import type { PreventiveExecutionAnswerInput } from '@/modules/preventive-executions/types/preventive-execution'
import { buildPreventiveExecutionEditPath } from '@/shared/routing/route-paths'

export function PreventiveExecutionStartPage() {
  const navigate = useNavigate()
  const { session } = useAuthSession()
  const accessToken = session?.tokens.accessToken
  const [inventoryItemId, setInventoryItemId] = useState('')
  const [startedExecutionId, setStartedExecutionId] = useState<string | null>(null)
  const [error, setError] = useState<string | null>(null)

  const inventoryQuery = useQuery({
    queryKey: inventoryQueryKeys.list({
      entityTypeId: '',
      status: '',
      siteId: '',
      regionId: '',
      isActive: 'true',
      search: '',
    }),
    queryFn: () =>
      inventoryClient.list(
        {
          entityTypeId: '',
          status: '',
          siteId: '',
          regionId: '',
          isActive: 'true',
          search: '',
        },
        accessToken!,
      ),
    enabled: Boolean(accessToken),
  })

  const formDefinitionQuery = useQuery({
    queryKey: preventiveExecutionQueryKeys.formDefinition(inventoryItemId),
    queryFn: () => preventiveExecutionsClient.getFormDefinition(inventoryItemId, accessToken!),
    enabled: Boolean(accessToken && inventoryItemId),
  })

  const startMutation = useMutation({
    mutationFn: () => preventiveExecutionsClient.start(inventoryItemId, accessToken!),
    onSuccess: (execution) => {
      setStartedExecutionId(execution.id)
    },
  })

  const saveDraftMutation = useMutation({
    mutationFn: ({
      answers,
      executionId,
    }: {
      answers: PreventiveExecutionAnswerInput[]
      executionId: string
    }) => preventiveExecutionsClient.saveDraft(executionId, answers, accessToken!),
    onSuccess: (execution) => {
      navigate(buildPreventiveExecutionEditPath(execution.id))
    },
  })

  const submitMutation = useMutation({
    mutationFn: ({
      answers,
      executionId,
    }: {
      answers: PreventiveExecutionAnswerInput[]
      executionId: string
    }) => preventiveExecutionsClient.submit(executionId, answers, accessToken!),
    onSuccess: (execution) => {
      navigate(buildPreventiveExecutionEditPath(execution.id))
    },
  })

  async function ensureStarted() {
    if (startedExecutionId) {
      return startedExecutionId
    }

    if (!inventoryItemId) {
      throw new Error('Select an inventory item before saving.')
    }

    const execution = await startMutation.mutateAsync()
    setStartedExecutionId(execution.id)

    return execution.id
  }

  async function handleSaveDraft(answers: PreventiveExecutionAnswerInput[]) {
    setError(null)
    const executionId = await ensureStarted()
    await saveDraftMutation.mutateAsync({ executionId, answers })
  }

  async function handleSubmit(answers: PreventiveExecutionAnswerInput[]) {
    setError(null)
    const executionId = await ensureStarted()
    await submitMutation.mutateAsync({ executionId, answers })
  }

  const formDefinition = formDefinitionQuery.data

  return (
    <section className="module-panel">
      <div className="module-panel__header">
        <div>
          <p className="hero-panel__eyebrow">Start execution</p>
          <h1>Create a preventive execution.</h1>
        </div>
        <p>Select an active inventory item to load its active preventive template.</p>
      </div>

      <div className="field">
        <label htmlFor="executionInventoryItem">Inventory item</label>
        <select
          id="executionInventoryItem"
          value={inventoryItemId}
          onChange={(event) => {
            setInventoryItemId(event.target.value)
            setStartedExecutionId(null)
          }}
        >
          <option value="">Select an inventory item</option>
          {(inventoryQuery.data ?? []).map((item) => (
            <option key={item.id} value={item.id}>
              {item.displayName} - {item.entityTypeName}
            </option>
          ))}
        </select>
      </div>

      {formDefinitionQuery.isError ? <p className="form-error">{formDefinitionQuery.error.message}</p> : null}
      {error ? <p className="form-error">{error}</p> : null}

      {formDefinition ? (
        <PreventiveExecutionForm
          sections={formDefinition.sections}
          onSaveDraft={(answers) => handleSaveDraft(answers).catch((caught) => setError(String(caught)))}
          onSubmitExecution={(answers) => handleSubmit(answers).catch((caught) => setError(String(caught)))}
        />
      ) : null}
    </section>
  )
}
