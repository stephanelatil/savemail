

interface AppUser {
  id: string,
  email: string,
  emailConfirmed: boolean,
  twoFactorEnabled: boolean,
  firstName?: string,
  lastName?: string
}

export default AppUser