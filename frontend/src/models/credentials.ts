export interface Credentials {
    username: string,
    password: string,
    twoFactorCode?:string,
    twoFactorRecoveryCode?: string
}

export interface PasswordReset{
    email:string,
    resetCode: string,
    newPassword: string
}

export interface ChangePassword{
    newPassword:string,
    oldPassword:string
}

export interface Edit2FA{
    enable: boolean,
    twoFactorCode: string,
    resetSharedKey: boolean,
    resetRecoveryCodes: boolean,
    forgetMachine: boolean
}