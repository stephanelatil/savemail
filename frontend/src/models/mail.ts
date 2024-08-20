import { Attachment }  from "./attachment"
import { EmailAddress } from "./emailAddress"


export interface Mail {
  id: number,
  replyTo: number|null,
  replies:Mail[],
  sender:EmailAddress,
  recipients:EmailAddress[],
  recipientsCc:EmailAddress[],
  subject:string,
  body:string,
  attachments:Attachment[],
  dateSent:Date
}