export const entityTypeQueryKeys = {
  all: ['entity-types'] as const,
  detail: (entityTypeId: string) => ['entity-types', entityTypeId] as const,
}
