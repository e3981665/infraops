// Intentional local/demo credentials for seeded Docker E2E workflows only.
export const demoUsers = {
  admin: {
    email: 'admin@infraops.local',
    password: 'DemoOnly-Admin-Local',
  },
  technician: {
    email: 'technician@infraops.local',
    password: 'DemoOnly-Tech-Local',
  },
  validator: {
    email: 'validator@infraops.local',
    password: 'DemoOnly-Validator-Local',
  },
} as const
