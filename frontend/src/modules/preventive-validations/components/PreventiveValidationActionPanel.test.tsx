import { screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'
import { renderWithProviders } from '@/app/test/render-with-providers'
import { PreventiveValidationActionPanel } from '@/modules/preventive-validations/components/PreventiveValidationActionPanel'

describe('PreventiveValidationActionPanel', () => {
  it('should require rejection reason', async () => {
    const user = userEvent.setup()
    const onReject = vi.fn().mockResolvedValue(undefined)

    renderWithProviders(
      <PreventiveValidationActionPanel
        status="submitted"
        onApprove={vi.fn().mockResolvedValue(undefined)}
        onReject={onReject}
        onRequestRework={vi.fn().mockResolvedValue(undefined)}
      />,
    )

    await user.click(screen.getByRole('button', { name: /reject/i }))

    expect(await screen.findByText('Rejection reason is required.')).toBeInTheDocument()
    expect(onReject).not.toHaveBeenCalled()
  })

  it('should require rework reason', async () => {
    const user = userEvent.setup()
    const onRequestRework = vi.fn().mockResolvedValue(undefined)

    renderWithProviders(
      <PreventiveValidationActionPanel
        status="submitted"
        onApprove={vi.fn().mockResolvedValue(undefined)}
        onReject={vi.fn().mockResolvedValue(undefined)}
        onRequestRework={onRequestRework}
      />,
    )

    await user.click(screen.getByRole('button', { name: /request rework/i }))

    expect(await screen.findByText('Rework reason is required.')).toBeInTheDocument()
    expect(onRequestRework).not.toHaveBeenCalled()
  })

  it('should approve without comment', async () => {
    const user = userEvent.setup()
    const onApprove = vi.fn().mockResolvedValue(undefined)

    renderWithProviders(
      <PreventiveValidationActionPanel
        status="submitted"
        onApprove={onApprove}
        onReject={vi.fn().mockResolvedValue(undefined)}
        onRequestRework={vi.fn().mockResolvedValue(undefined)}
      />,
    )

    await user.click(screen.getByRole('button', { name: /approve/i }))

    await waitFor(() => {
      expect(onApprove).toHaveBeenCalledWith(null)
    })
  })
})
