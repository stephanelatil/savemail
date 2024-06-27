

interface AppUser {
  id: string,
  email: string,
  emailConfirmed: boolean,
  twoFactorEnabled: boolean,
  firstName?: string,
  lastName?: string
}

interface EditAppUser{
  id: string,
  firstName: string|null,
  lastName: string|null
}

export default AppUser