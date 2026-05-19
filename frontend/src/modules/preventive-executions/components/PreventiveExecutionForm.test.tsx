import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'
import { PreventiveExecutionForm } from '@/modules/preventive-executions/components/PreventiveExecutionForm'
import type { PreventiveExecutionTemplateSection } from '@/modules/preventive-executions/types/preventive-execution'

const sections: PreventiveExecutionTemplateSection[] = [
  {
    id: 'section-1',
    sourceTemplateSectionId: 'source-section-1',
    title: 'Visual Inspection',
    displayOrder: 1,
    checklistItems: [
      {
        id: 'item-1',
        sourceChecklistItemId: 'source-item-1',
        itemKey: 'equipmentClean',
        label: 'Equipment clean?',
        itemType: 'yesNo',
        displayOrder: 1,
        isRequired: true,
        helpText: null,
        isCritical: false,
        requiresCommentOnFailure: false,
        requiresPhotoOnFailure: false,
        minimumValue: null,
        maximumValue: null,
        options: [],
      },
      {
        id: 'item-2',
        sourceChecklistItemId: 'source-item-2',
        itemKey: 'activeAlarm',
        label: 'Any active alarm?',
        itemType: 'yesNo',
        displayOrder: 2,
        isRequired: true,
        helpText: null,
        isCritical: true,
        requiresCommentOnFailure: true,
        requiresPhotoOnFailure: false,
        minimumValue: null,
        maximumValue: null,
        options: [],
      },
    ],
  },
  {
    id: 'section-2',
    sourceTemplateSectionId: 'source-section-2',
    title: 'Electrical Measurements',
    displayOrder: 2,
    checklistItems: [
      {
        id: 'item-3',
        sourceChecklistItemId: 'source-item-3',
        itemKey: 'inputVoltage',
        label: 'Input voltage',
        itemType: 'numeric',
        displayOrder: 1,
        isRequired: true,
        helpText: null,
        isCritical: true,
        requiresCommentOnFailure: false,
        requiresPhotoOnFailure: false,
        minimumValue: 210,
        maximumValue: 240,
        options: [],
      },
      {
        id: 'item-4',
        sourceChecklistItemId: 'source-item-4',
        itemKey: 'batteryCondition',
        label: 'Battery condition',
        itemType: 'select',
        displayOrder: 2,
        isRequired: false,
        helpText: null,
        isCritical: false,
        requiresCommentOnFailure: false,
        requiresPhotoOnFailure: false,
        minimumValue: null,
        maximumValue: null,
        options: [
          { id: 'option-1', value: 'good', label: 'Good', displayOrder: 1 },
          { id: 'option-2', value: 'warning', label: 'Warning', displayOrder: 2 },
        ],
      },
    ],
  },
]

describe('PreventiveExecutionForm', () => {
  it('should render template sections and item controls from API data', () => {
    render(<PreventiveExecutionForm sections={sections} />)

    expect(screen.getByText('Visual Inspection')).toBeInTheDocument()
    expect(screen.getByText('Electrical Measurements')).toBeInTheDocument()
    expect(screen.getByLabelText(/Equipment clean/i)).toBeInTheDocument()
    expect(screen.getByLabelText(/Input voltage/i)).toBeInTheDocument()
  })

  it('should render select options and numeric hints', () => {
    render(<PreventiveExecutionForm sections={sections} />)

    expect(screen.getByRole('option', { name: 'Good' })).toBeInTheDocument()
    expect(screen.getByRole('option', { name: 'Warning' })).toBeInTheDocument()
    expect(screen.getByText('Expected range 210 to 240')).toBeInTheDocument()
  })

  it('should show conditional comment input when failure requires a comment', async () => {
    const user = userEvent.setup()

    render(<PreventiveExecutionForm sections={sections} />)

    await user.selectOptions(screen.getByLabelText(/Any active alarm/i), 'false')

    expect(screen.getByLabelText('Comment')).toBeInTheDocument()
  })

  it('should rehydrate saved draft answers when initial answers change', () => {
    const { rerender } = render(
      <PreventiveExecutionForm sections={sections} initialAnswers={[]} />,
    )

    rerender(
      <PreventiveExecutionForm
        sections={sections}
        initialAnswers={[
          { id: 'answer-1', itemKey: 'equipmentClean', value: 'true', comment: null },
          { id: 'answer-2', itemKey: 'inputVoltage', value: '220', comment: null },
          { id: 'answer-3', itemKey: 'batteryCondition', value: 'warning', comment: null },
        ]}
      />,
    )

    expect(screen.getByLabelText(/Equipment clean/i)).toHaveValue('true')
    expect(screen.getByLabelText(/Input voltage/i)).toHaveValue(220)
    expect(screen.getByLabelText(/Battery condition/i)).toHaveValue('warning')
  })

  it('should save incomplete drafts', async () => {
    const user = userEvent.setup()
    const onSaveDraft = vi.fn().mockResolvedValue(undefined)

    render(<PreventiveExecutionForm sections={sections} onSaveDraft={onSaveDraft} />)

    await user.selectOptions(screen.getByLabelText(/Equipment clean/i), 'true')
    await user.click(screen.getByRole('button', { name: /save draft/i }))

    await waitFor(() => {
      expect(onSaveDraft).toHaveBeenCalledWith([
        { itemKey: 'equipmentClean', value: 'true', comment: null },
      ])
    })
  })

  it('should block submit when required answers are missing', async () => {
    const user = userEvent.setup()
    const onSubmitExecution = vi.fn().mockResolvedValue(undefined)

    render(
      <PreventiveExecutionForm sections={sections} onSubmitExecution={onSubmitExecution} />,
    )

    await user.click(screen.getByRole('button', { name: /^submit$/i }))

    expect(await screen.findAllByText('This checklist item is required.')).toHaveLength(3)
    expect(onSubmitExecution).not.toHaveBeenCalled()
  })
})
