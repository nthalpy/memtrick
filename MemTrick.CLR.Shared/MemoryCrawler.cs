﻿using System;
using System.Diagnostics;

namespace MemTrick.CLR
{
    /// <summary>
    /// Crawls memory to find methods.
    /// </summary>
    internal unsafe static class MemoryCrawler
    {
        private static readonly void* BaseAddress;
        private static readonly int ModuleMemorySize;

        private static CrawlResult AllocatorCache;

        static MemoryCrawler()
        {
            foreach (ProcessModule module in Process.GetCurrentProcess().Modules)
            {
                if (module.ModuleName == "clr.dll" || module.ModuleName == "coreclr.dll")
                {
                    BaseAddress = (void*)module.BaseAddress;
                    ModuleMemorySize = module.ModuleMemorySize;
                }
            }

            if (BaseAddress == null)
                throw new Exception("Unable to find proper runtime module (clr/coreclr).");
        }

        public static CrawlResult FindAllocator()
        {
            if (AllocatorCache != null)
                return AllocatorCache;

            if (AllocatorCache == null)
                AllocatorCache = CheckMatch(AssemblyPattern.CLR_X64_JIT_TrialAllocSFastMP_InlineGetThread);
            if (AllocatorCache == null)
                AllocatorCache = CheckMatch(AssemblyPattern.CLR_X86_JIT_TrialAllocSFastMP_InlineGetThread);
            
            if (AllocatorCache != null)
                return AllocatorCache;
            
            throw new NotSupportedException("Unable to find allocator.");
        }

        public static CrawlResult CheckMatch(AssemblyPattern pattern)
        {
            Byte* p = (Byte*)BaseAddress;

            for (int idx = 0; idx < ModuleMemorySize - pattern.Size; idx++)
            {
                if (pattern.IsMatches(p + idx))
                {
                    AllocatorCache = new CrawlResult(p + idx, pattern);
                    return AllocatorCache;
                }
            }

            return null;
        }
    }
}
