﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace MemTrick.CLR.Test
{
    [TestClass]
    public unsafe class UnmanagedSZArrayTest
    {
        [TestMethod]
        public void PrimitiveOperation()
        {
            Random rd = new Random();
            int size = rd.Next(1000, 2000);
            int[] original = new int[size];

            for (int idx = 0; idx < original.Length; idx++)
                original[idx] = rd.Next();

            using (UnmanagedSZArray<int> intSZArray = UnmanagedSZArray<int>.Create(size))
            {
                int[] arr = intSZArray.Array;

                for (int idx = 0; idx < arr.Length; idx++)
                    arr[idx] = original[idx];

                Assert.IsTrue(arr.SequenceEqual(arr));
            }
        }
    }
}
