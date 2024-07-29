import Folder from "./folder"

enum SecureSockerOptions {
  None = 0,
  Auto = 1,
  SslOnConnect = 2,
  StartTls = 3,
  StartTlsIfAvailable = 4
}

enum ImapProvider {
  Simple=0,
  Plain=1,
  SaslLogin=2,
  Cram_MD5=3,
  Gmail=4
}

interface MailBox {
  id: number,
  imapDomain:string,
  imapPort:number,
  username:string,
  secureSocketOptions:SecureSockerOptions,
  provider:ImapProvider,
  folders:Folder[]
}

interface EditMailBox{
  id: number,
  imapDomain:string,
  imapPort:number,
  username:string,
  password:string,
  secureSocketOptions:SecureSockerOptions,
  provider:ImapProvider
}

export default MailBox