import type { SupportedLocale } from '@/shared/i18n/translations'

const ptBrDemoText: Record<string, string> = {
  'InfraOps Administrator': 'Administrador InfraOps',
  'InfraOps Technician': 'Técnico InfraOps',
  'InfraOps Validator': 'Validador InfraOps',
  Admin: 'Administrador',
  Technician: 'Técnico',
  Validator: 'Validador',
  Viewer: 'Visualizador',
  Operational: 'Operacional',
  'Pending validation': 'Pendente de validação',
  Approved: 'Aprovado',
  Rejected: 'Rejeitado',
  'Rework requested': 'Refação solicitada',
  Generator: 'Gerador',
  'North Region': 'Região Norte',
  'South Region': 'Região Sul',
  'North Hub': 'Hub Norte',
  'South Yard': 'Pátio Sul',
  'Riverside Station': 'Estação Riverside',
  'Serial Number': 'Número de série',
  Manufacturer: 'Fabricante',
  Model: 'Modelo',
  'Manufacturer serial number.': 'Número de série do fabricante.',
  'Quarterly UPS Inspection': 'Inspeção trimestral de UPS',
  'Monthly Generator Run': 'Teste mensal do gerador',
  'Monthly HVAC Inspection': 'Inspeção mensal de HVAC',
  'Power quality and battery inspection.': 'Inspeção de qualidade de energia e bateria.',
  'Readiness run and visual inspection.': 'Teste de prontidão e inspeção visual.',
  'Cooling performance and equipment condition.':
    'Desempenho de refrigeração e condição do equipamento.',
  'Visual Inspection': 'Inspeção visual',
  'Operational Readings': 'Leituras operacionais',
  'Equipment clean?': 'Equipamento limpo?',
  'Any active alarm?': 'Algum alarme ativo?',
  'Primary operating reading': 'Leitura operacional principal',
  'Overall condition': 'Condição geral',
  'Use the normal operating unit for this asset.':
    'Use a unidade operacional normal para este ativo.',
  Good: 'Bom',
  Warning: 'Atenção',
  Critical: 'Crítico',
  'Readings accepted.': 'Leituras aceitas.',
  'Generator failed load-transfer criteria.':
    'O gerador falhou nos critérios de transferência de carga.',
  'Clarify alarm comment and retake readings.':
    'Esclareça o comentário do alarme e refaça as leituras.',
  'Cooling checks reviewed.': 'Verificações de refrigeração revisadas.',
}

const checklistItemTypeLabels: Record<SupportedLocale, Record<string, string>> = {
  'en-US': {
    yesNo: 'Yes / No',
    text: 'Text',
    numeric: 'Numeric',
    select: 'Select',
  },
  'pt-BR': {
    yesNo: 'Sim / Não',
    text: 'Texto',
    numeric: 'Numérico',
    select: 'Seleção',
  },
}

export function localizeDemoText(value: string | null | undefined, locale: SupportedLocale) {
  if (!value) {
    return value ?? ''
  }

  if (locale !== 'pt-BR') {
    return value
  }

  return ptBrDemoText[value] ?? value
}

export function localizeChecklistItemType(value: string, locale: SupportedLocale) {
  return checklistItemTypeLabels[locale][value] ?? value
}
