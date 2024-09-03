import { Folder } from "./folder"

export enum ImapProvider {
  Simple=0,
  Google=1
}

export interface MailBox {
  id: number,
  imapDomain:string,
  imapPort:number,
  provider:ImapProvider,
  username:string,
  folders:Folder[]
}

export interface EditMailBox{
  id: number,
  imapDomain:string,
  imapPort:number,
  username:string,
  password:string
}