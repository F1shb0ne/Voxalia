//
// This file is part of the game Voxalia, created by Frenetic LLC.
// This code is Copyright (C) 2016-2017 Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Voxalia.Shared
{
    /// <summary>
    /// This class isn't actually entirely locked/thread-safe, or even entirely functional as a linked-list, only done sufficiently for its first immediate use-case.
    /// Use with caution. Probably don't use beyond as a starting point reference really.
    /// </summary>
    public class LockedLinkedList<T>
    {
        public class Node
        {
            public Node Previous;

            public Node Next;

            public T Data;
        }

        public Node First;

        public Node Last;

        private Object lastLock = new Object();

        /// <summary>
        /// Do NOT input null. There is no safety check, and that will produce unexpected results!
        /// </summary>
        public void Remove(Node itm)
        {
            // If the item or its follower is the last one, lock. Otherwise, execute as normal.
            // TODO: Evaluate this better. Is this actually guaranteed to not bork out on too much sudden usage?
            bool t = false;
            try
            {
                if (itm == Last || (itm.Next != null && itm.Next == Last))
                {
                    Monitor.Enter(lastLock, ref t);
                }
                if (itm.Previous != null)
                {
                    itm.Previous.Next = itm.Next;
                }
                if (itm.Next != null)
                {
                    itm.Next.Previous = itm.Previous;
                }
                if (itm == First)
                {
                    First = itm.Next;
                }
                if (itm == Last)
                {
                    Last = itm.Previous;
                }
            }
            finally
            {
                if (t)
                {
                    Monitor.Exit(lastLock);
                }
            }
        }

        public void AddAtEnd(T data)
        {
            // This always modifies the last entry, so lock on the last entry.
            lock (lastLock)
            {
                Node temp = new Node() { Data = data, Previous = Last };
                if (Last != null)
                {
                    Last.Next = temp;
                }
                Last = temp;
                if (First == null)
                {
                    First = temp;
                }
            }
        }

        public void Clear()
        {
            // This always modifies the last entry, so lock on the last entry.
            lock (lastLock)
            {
                First = null;
                Last = null;
            }
        }
    }
}
