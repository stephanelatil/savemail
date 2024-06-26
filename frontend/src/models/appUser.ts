

interface AppUser {
  id: string,
  email: string,
  normalizedEmail?: string,
  emailConfirmed: boolean,
  twoFactorEnabled: boolean,
  firstName?: string,
  lastName?: string
}

export default AppUser