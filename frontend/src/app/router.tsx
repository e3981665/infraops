import { lazy, Suspense, type ReactElement } from 'react'
import { createBrowserRouter, Navigate, RouterProvider } from 'react-router-dom'
import { AppErrorBoundary } from '@/app/components/AppErrorBoundary'
import { RouteLoadingFallback } from '@/app/components/RouteLoadingFallback'
import { ProtectedLayout } from '@/app/layouts/ProtectedLayout'
import { PublicLayout } from '@/app/layouts/PublicLayout'
import { permissionCodes, type PermissionCode } from '@/modules/auth/authorization/permission-codes'
import { ProtectedRoute } from '@/modules/auth/components/ProtectedRoute'
import { routePaths } from '@/shared/routing/route-paths'

const DashboardPage = lazy(() =>
  import('@/modules/dashboard/pages/DashboardPage').then((module) => ({
    default: module.DashboardPage,
  })),
)
const EntityTypeCreatePage = lazy(() =>
  import('@/modules/entity-types/pages/EntityTypeCreatePage').then((module) => ({
    default: module.EntityTypeCreatePage,
  })),
)
const EntityTypeDetailPage = lazy(() =>
  import('@/modules/entity-types/pages/EntityTypeDetailPage').then((module) => ({
    default: module.EntityTypeDetailPage,
  })),
)
const EntityTypeEditPage = lazy(() =>
  import('@/modules/entity-types/pages/EntityTypeEditPage').then((module) => ({
    default: module.EntityTypeEditPage,
  })),
)
const EntityTypeListPage = lazy(() =>
  import('@/modules/entity-types/pages/EntityTypeListPage').then((module) => ({
    default: module.EntityTypeListPage,
  })),
)
const HomePage = lazy(() =>
  import('@/modules/home/pages/HomePage').then((module) => ({ default: module.HomePage })),
)
const InventoryCreatePage = lazy(() =>
  import('@/modules/inventory/pages/InventoryCreatePage').then((module) => ({
    default: module.InventoryCreatePage,
  })),
)
const InventoryDetailPage = lazy(() =>
  import('@/modules/inventory/pages/InventoryDetailPage').then((module) => ({
    default: module.InventoryDetailPage,
  })),
)
const InventoryEditPage = lazy(() =>
  import('@/modules/inventory/pages/InventoryEditPage').then((module) => ({
    default: module.InventoryEditPage,
  })),
)
const InventoryListPage = lazy(() =>
  import('@/modules/inventory/pages/InventoryListPage').then((module) => ({
    default: module.InventoryListPage,
  })),
)
const LoginPage = lazy(() =>
  import('@/modules/auth/pages/LoginPage').then((module) => ({ default: module.LoginPage })),
)
const PreventiveExecutionDetailPage = lazy(() =>
  import('@/modules/preventive-executions/pages/PreventiveExecutionDetailPage').then((module) => ({
    default: module.PreventiveExecutionDetailPage,
  })),
)
const PreventiveExecutionEditPage = lazy(() =>
  import('@/modules/preventive-executions/pages/PreventiveExecutionEditPage').then((module) => ({
    default: module.PreventiveExecutionEditPage,
  })),
)
const PreventiveExecutionListPage = lazy(() =>
  import('@/modules/preventive-executions/pages/PreventiveExecutionListPage').then((module) => ({
    default: module.PreventiveExecutionListPage,
  })),
)
const PreventiveExecutionStartPage = lazy(() =>
  import('@/modules/preventive-executions/pages/PreventiveExecutionStartPage').then((module) => ({
    default: module.PreventiveExecutionStartPage,
  })),
)
const PreventiveValidationDetailPage = lazy(() =>
  import('@/modules/preventive-validations/pages/PreventiveValidationDetailPage').then((module) => ({
    default: module.PreventiveValidationDetailPage,
  })),
)
const PreventiveValidationQueuePage = lazy(() =>
  import('@/modules/preventive-validations/pages/PreventiveValidationQueuePage').then((module) => ({
    default: module.PreventiveValidationQueuePage,
  })),
)
const PreventiveTemplateCreatePage = lazy(() =>
  import('@/modules/preventive-templates/pages/PreventiveTemplateCreatePage').then((module) => ({
    default: module.PreventiveTemplateCreatePage,
  })),
)
const PreventiveTemplateDetailPage = lazy(() =>
  import('@/modules/preventive-templates/pages/PreventiveTemplateDetailPage').then((module) => ({
    default: module.PreventiveTemplateDetailPage,
  })),
)
const PreventiveTemplateEditPage = lazy(() =>
  import('@/modules/preventive-templates/pages/PreventiveTemplateEditPage').then((module) => ({
    default: module.PreventiveTemplateEditPage,
  })),
)
const PreventiveTemplateListPage = lazy(() =>
  import('@/modules/preventive-templates/pages/PreventiveTemplateListPage').then((module) => ({
    default: module.PreventiveTemplateListPage,
  })),
)

function withRouteFallback(element: ReactElement) {
  return (
    <AppErrorBoundary>
      <Suspense fallback={<RouteLoadingFallback />}>{element}</Suspense>
    </AppErrorBoundary>
  )
}

function protectedPage(requiredPermission: PermissionCode, element: ReactElement) {
  return withRouteFallback(
    <ProtectedRoute requiredPermission={requiredPermission}>{element}</ProtectedRoute>,
  )
}

const router = createBrowserRouter([
  {
    path: routePaths.home,
    element: <PublicLayout />,
    children: [
      {
        index: true,
        element: withRouteFallback(<HomePage />),
      },
      {
        path: routePaths.login.slice(1),
        element: withRouteFallback(<LoginPage />),
      },
    ],
  },
  {
    path: routePaths.app,
    element: <ProtectedLayout />,
    children: [
      {
        index: true,
        element: withRouteFallback(<DashboardPage />),
      },
      {
        path: routePaths.entityTypes.replace(`${routePaths.app}/`, ''),
        element: protectedPage(permissionCodes.entityManage, <EntityTypeListPage />),
      },
      {
        path: routePaths.entityTypeCreate.replace(`${routePaths.app}/`, ''),
        element: protectedPage(permissionCodes.entityManage, <EntityTypeCreatePage />),
      },
      {
        path: routePaths.entityTypeDetail.replace(`${routePaths.app}/`, ''),
        element: protectedPage(permissionCodes.entityManage, <EntityTypeDetailPage />),
      },
      {
        path: routePaths.entityTypeEdit.replace(`${routePaths.app}/`, ''),
        element: protectedPage(permissionCodes.entityManage, <EntityTypeEditPage />),
      },
      {
        path: routePaths.inventory.replace(`${routePaths.app}/`, ''),
        element: protectedPage(permissionCodes.inventoryRead, <InventoryListPage />),
      },
      {
        path: routePaths.inventoryCreate.replace(`${routePaths.app}/`, ''),
        element: protectedPage(permissionCodes.inventoryWrite, <InventoryCreatePage />),
      },
      {
        path: routePaths.inventoryDetail.replace(`${routePaths.app}/`, ''),
        element: protectedPage(permissionCodes.inventoryRead, <InventoryDetailPage />),
      },
      {
        path: routePaths.inventoryEdit.replace(`${routePaths.app}/`, ''),
        element: protectedPage(permissionCodes.inventoryWrite, <InventoryEditPage />),
      },
      {
        path: routePaths.preventiveTemplates.replace(`${routePaths.app}/`, ''),
        element: protectedPage(
          permissionCodes.preventiveTemplatesRead,
          <PreventiveTemplateListPage />,
        ),
      },
      {
        path: routePaths.preventiveTemplateCreate.replace(`${routePaths.app}/`, ''),
        element: protectedPage(
          permissionCodes.preventiveTemplatesWrite,
          <PreventiveTemplateCreatePage />,
        ),
      },
      {
        path: routePaths.preventiveTemplateDetail.replace(`${routePaths.app}/`, ''),
        element: protectedPage(
          permissionCodes.preventiveTemplatesRead,
          <PreventiveTemplateDetailPage />,
        ),
      },
      {
        path: routePaths.preventiveTemplateEdit.replace(`${routePaths.app}/`, ''),
        element: protectedPage(
          permissionCodes.preventiveTemplatesWrite,
          <PreventiveTemplateEditPage />,
        ),
      },
      {
        path: routePaths.preventiveExecutions.replace(`${routePaths.app}/`, ''),
        element: protectedPage(permissionCodes.preventiveRead, <PreventiveExecutionListPage />),
      },
      {
        path: routePaths.preventiveExecutionCreate.replace(`${routePaths.app}/`, ''),
        element: protectedPage(
          permissionCodes.preventiveExecute,
          <PreventiveExecutionStartPage />,
        ),
      },
      {
        path: routePaths.preventiveExecutionDetail.replace(`${routePaths.app}/`, ''),
        element: protectedPage(
          permissionCodes.preventiveRead,
          <PreventiveExecutionDetailPage />,
        ),
      },
      {
        path: routePaths.preventiveExecutionEdit.replace(`${routePaths.app}/`, ''),
        element: protectedPage(
          permissionCodes.preventiveExecute,
          <PreventiveExecutionEditPage />,
        ),
      },
      {
        path: routePaths.preventiveValidations.replace(`${routePaths.app}/`, ''),
        element: protectedPage(
          permissionCodes.preventiveValidate,
          <PreventiveValidationQueuePage />,
        ),
      },
      {
        path: routePaths.preventiveValidationDetail.replace(`${routePaths.app}/`, ''),
        element: protectedPage(
          permissionCodes.preventiveValidate,
          <PreventiveValidationDetailPage />,
        ),
      },
    ],
  },
  {
    path: '*',
    element: <Navigate to={routePaths.home} replace />,
  },
])

export function AppRouter() {
  return <RouterProvider router={router} />
}
