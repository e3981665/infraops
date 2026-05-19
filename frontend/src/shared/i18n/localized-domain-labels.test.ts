import { describe, expect, it } from 'vitest'
import {
  localizeChecklistItemType,
  localizeDemoText,
} from '@/shared/i18n/localized-domain-labels'

describe('localized domain labels', () => {
  it('should keep API content unchanged for English', () => {
    expect(localizeDemoText('Visual Inspection', 'en-US')).toBe('Visual Inspection')
    expect(localizeDemoText('Good', 'en-US')).toBe('Good')
  })

  it('should localize known demo checklist content for Portuguese', () => {
    expect(localizeDemoText('Visual Inspection', 'pt-BR')).toBe('Inspeção visual')
    expect(localizeDemoText('Equipment clean?', 'pt-BR')).toBe('Equipamento limpo?')
    expect(localizeDemoText('Good', 'pt-BR')).toBe('Bom')
  })

  it('should localize dashboard status labels for Portuguese', () => {
    expect(localizeDemoText('Pending validation', 'pt-BR')).toBe('Pendente de validação')
    expect(localizeDemoText('Approved', 'pt-BR')).toBe('Aprovado')
    expect(localizeDemoText('Rejected', 'pt-BR')).toBe('Rejeitado')
    expect(localizeDemoText('Rework requested', 'pt-BR')).toBe('Refação solicitada')
  })

  it('should localize checklist item type codes', () => {
    expect(localizeChecklistItemType('yesNo', 'pt-BR')).toBe('Sim / Não')
    expect(localizeChecklistItemType('numeric', 'en-US')).toBe('Numeric')
  })
})
