﻿using MemTrick.RumtimeSpecific;
using System;
using System.Reflection;

namespace MemTrick
{
    public static class UnmanagedHeapAllocator
    {
        /// <summary>
        /// Similar with FormatterServices.GetUninitializedObject. Allocate and return zeroed T-typed object.
        /// </summary>
        public static UnmanagedHeapDisposeHandle UninitializedAllocation<T>(out T result) where T : class
        {
            UnmanagedHeapDisposeHandle handle = UninitializedAllocation(typeof(T), out Object obj);
            result = obj as T;

            return handle;
        }

        /// <summary>
        /// Similar with FormatterServices.GetUninitializedObject. Allocate and return zeroed T-typed object.
        /// </summary>
        public static UnmanagedHeapDisposeHandle UninitializedAllocation(Type t, out Object result)
        {
            unsafe
            {
                MethodTable* mt = MethodTable.GetMethodTable(t);
                int size = mt->BaseSize;

                ObjectHeader* objHeader = (ObjectHeader*)RawMemoryAllocator.Allocate(size);

                objHeader->SyncBlock = 0;
                objHeader->MethodTable = mt;
                RawMemoryAllocator.FillMemory(objHeader + 1, 0, size - sizeof(ObjectHeader));

                result = TypedReferenceHelper.PointerToObject<Object>(objHeader);
                return new UnmanagedHeapDisposeHandle(objHeader);
            }
        }

        /// <summary>
        /// Similar with new T();
        /// </summary>
        public static UnmanagedHeapDisposeHandle Allocate<T>(out T result) where T : class
        {
            UnmanagedHeapDisposeHandle handle = UninitializedAllocation<T>(out result);

            unsafe
            {
                ConstructorInfo ci = typeof(T).GetConstructor(
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.CreateInstance,
                    null,
                    CallingConventions.Any,
                    Type.EmptyTypes,
                    null);
                ArbitaryMethodInvoker.InvokeAction(ci.MethodHandle.GetFunctionPointer(), result);
            }

            return handle;
        }

        /// <summary>
        /// Similar with new T(arg0);
        /// </summary>
        public static UnmanagedHeapDisposeHandle Allocate<T, TArg0>(out T result, TArg0 arg0) where T : class
        {
            UnmanagedHeapDisposeHandle handle = UninitializedAllocation<T>(out result);

            unsafe
            {
                ConstructorInfo ci = typeof(T).GetConstructor(
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.CreateInstance,
                    null,
                    CallingConventions.Any,
                    new Type[] { typeof(TArg0) },
                    null);
                ArbitaryMethodInvoker.InvokeAction(ci.MethodHandle.GetFunctionPointer(), result, arg0);
            }

            return handle;
        }

        public static UnmanagedHeapDisposeHandle Box<T>(T val, out Object boxed) where T : struct
        {
            unsafe
            {
                MethodTable* mt = MethodTable.GetMethodTable<T>();
                int size = mt->BaseSize;

                ObjectHeader* objHeader = (ObjectHeader*)RawMemoryAllocator.Allocate(size);

                void* src = TypedReferenceHelper.StructToPointer(ref val);

                objHeader->SyncBlock = 0;
                objHeader->MethodTable = mt;
                RawMemoryAllocator.MemCpy(objHeader + 1, src, mt->DataSize);

                boxed = TypedReferenceHelper.PointerToObject<Object>(objHeader);
                return new UnmanagedHeapDisposeHandle(objHeader);
            }
        }
    }
}
