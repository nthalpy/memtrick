﻿using System;
using System.Runtime.InteropServices;

namespace MemTrick.CLR.RumtimeSpecific
{
    /// <summary>
    /// Pointer to sync block.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct ObjectHeader
    {
        public static MethodTable** GetMethodTablePointer(ObjectHeader* headerPtr)
        {
            return (MethodTable**)((Int32*)headerPtr + 1);
        }

        public Int32 SyncBlock;

#if _TARGET_64BIT_
        private Int32 Pad;
#endif

        public MethodTable* MethodTable;
    }
}
