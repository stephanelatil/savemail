import { Folder } from "./folder"

export enum SecureSocketOptions {
  None = 0,
  Auto = 1,
  SslOnConnect = 2,
  StartTls = 3,
  StartTlsIfAvailable = 4
}

export enum ImapProvider {
  Simple=0,
  Plain=1,
  SaslLogin=2,
  Cram_MD5=3,
  Gmail=4
}

export interface MailBox {
  id: number,
  imapDomain:string,
  imapPort:number,
  username:string,
  secureSocketOptions:SecureSocketOptions,
  provider:ImapProvider,
  folders:Folder[]
}

export interface EditMailBox{
  id: number,
  imapDomain:string,
  imapPort:number,
  username:string,
  password:string,
  secureSocketOptions:SecureSocketOptions,
  provider:ImapProvider
}