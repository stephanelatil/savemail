

export interface AppUser {
  id: string,
  email: string,
  emailConfirmed: boolean,
  twoFactorEnabled: boolean,
  firstName?: string,
  lastName?: string
}

export interface EditAppUser{
  id: string,
  firstName: string|null,
  lastName: string|null
}