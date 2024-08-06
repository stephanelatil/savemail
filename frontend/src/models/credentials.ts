export interface Credentials {
    username: string,
    password: string,
    passwordRepeat?:string,
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

export interface Init2FA{}

export interface Enable2FA{
    enable: boolean,
    twoFactorCode?: string
}

export interface Response2FA{
    sharedKey:string,
    recoveryCodesLeft:number
    recoveryCodes:string[],
    isTwoFactorEnabled:boolean,
    isMachineRemembered:boolean
}