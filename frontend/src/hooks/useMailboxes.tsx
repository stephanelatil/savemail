'use client'

import { getAllMailboxes, getMailbox as getMailboxService, createMailBox, editMailBox as editMailBoxService, synchronizeMailBox as synchronizeMailBoxService, deleteMailBox as deleteMailBoxService } from '@/services/mailboxService'
import { useState } from 'react'
import { useNotification } from './useNotification'
import { useRouter } from 'next/navigation'
import { EditMailBox, MailBox } from '@/models/mailBox'
import { FetchError } from '@/services/fetchService'

export const useMailboxes = () => {
  const showNotification = useNotification();
  const [loading, setLoading] = useState(false);
  const router = useRouter();

  const getMailboxList = async (): Promise<MailBox[]> => {
    try {
      setLoading(true);
      return await getAllMailboxes();
    } catch (error: unknown) {
      if (error instanceof FetchError){
        if (error.statusCode >= 500)
          showNotification("Backend issue: "+error.message, 'error');

        else if (error.statusCode == 401)
          showNotification(error.message, 'error');
      }
      return [];
    } finally {
      setLoading(false);
    }
  }

  const getMailbox = async (id:number):Promise<MailBox|null> => {
    try {
      setLoading(true);
      return await getMailboxService(id);
    } catch (error: unknown) {
      if (error instanceof FetchError){
        if (error.statusCode >= 500)
          showNotification("Backend issue: "+error.message, 'error');

        else if (error.statusCode == 401)
          showNotification(error.message, 'error');
      }
      return null;
    } finally {
      setLoading(false);
    }
  }

  const createNewMailbox  = async (newMb:EditMailBox): Promise<MailBox|null> => {
    try{
      setLoading(true);
      return await createMailBox(newMb);
    }catch (error: unknown){
      if (error instanceof FetchError){
        if (error.statusCode == 500){
          showNotification("Backend Error", 'error');
          console.error(error.message, error)
        }
        else if (error.statusCode == 400)
          showNotification(error.message, 'warning');
      }
      return null;
    }
    finally{
      setLoading(false);
    }
  }

  const editMailBox = async (edits:EditMailBox):Promise<void> => {
    try{
      setLoading(true);
      await editMailBoxService(edits);

    }catch (error:unknown){
      if (error instanceof FetchError)
        if (error.statusCode == 400)
          showNotification(error.message, 'warning');
        else if (error.statusCode == 404)
          showNotification("Mailbox does not exist", 'error');
        else
          showNotification('This mailbox does not belong to you!', 'warning');
      else{
        showNotification("Backend error", 'error');
        console.error(error);
      }
    }finally{
      setLoading(false);
    }
  }

  const synchronizeMailbox = async (id:number) => {
    try{
      setLoading(true);
      await synchronizeMailBoxService(id);
      showNotification("Mailbox queued to sync", 'success');
    }catch (error:unknown){
      if (error instanceof FetchError)
        if (error.statusCode == 400)
          showNotification(error.message, 'warning');
        else if (error.statusCode == 404)
          showNotification("Mailbox does not exist", 'error');
        else
          showNotification('This mailbox does not belong to you!', 'warning');
      else{
        showNotification("Backend error", 'error');
        console.error(error);
      }
    }finally{
      setLoading(false);
    }
  }

  const deleteMailBox = async (id:number) => {
    try{
      setLoading(true);
      await deleteMailBoxService(id);
    }catch (error:unknown){
      if (error instanceof FetchError)
        if (error.statusCode == 400)
          showNotification(error.message, 'warning');
        else if (error.statusCode == 404)
          showNotification("Mailbox does not exist", 'error');
        else
          showNotification('This mailbox does not belong to you!', 'warning');
      else{
        showNotification("Backend error", 'error');
        console.error(error);
      }
    }finally{
      setLoading(false);
    }
  }

  return {
    loading,
    getMailbox,
    getMailboxList,
    createNewMailbox,
    editMailBox,
    synchronizeMailbox,
    deleteMailBox
  };
}
