'use client'

import React, { createContext, useState, useContext, useEffect } from 'react';
import { MailBox } from '@/models/mailBox';
import { useMailboxes } from '@/hooks/useMailboxes';

export type PersistentMailboxesState = {
  mailboxes: MailBox[],
  setMailboxes: React.Dispatch<React.SetStateAction<MailBox[]>>,
  refreshMailBoxes: () => Promise<void>,
}

const MailboxesContext = createContext<PersistentMailboxesState>({
  mailboxes: [],
  setMailboxes: () => {},
  refreshMailBoxes: async () => {},
});

const PersistentMailboxesProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [mailboxes, setMailboxes] = useState<MailBox[]>([]);
  const { getMailboxList } = useMailboxes();

  const refreshMailBoxes = async () => {
    const mailboxes = await getMailboxList();
    setMailboxes(mailboxes);
  };

  return (
    <MailboxesContext.Provider value={{ mailboxes, setMailboxes, refreshMailBoxes }}>
      {children}
    </MailboxesContext.Provider>
  );
};

export default PersistentMailboxesProvider;
export const usePersistentMailboxesState = () => useContext(MailboxesContext);