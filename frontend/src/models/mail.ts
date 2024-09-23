import { Attachment }  from "./attachment"
import { EmailAddress } from "./emailAddress"


export interface Mail {
  id: number,
  repliedFromId: number|null,
  repliedFrom: Mail|null,
  replyId:number|null,
  reply:Mail|null,
  sender:EmailAddress,
  recipients:EmailAddress[],
  recipientsCc:EmailAddress[],
  subject:string,
  body:string,
  attachments:Attachment[],
  dateSent:Date
}