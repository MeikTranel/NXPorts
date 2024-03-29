﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Runtime.InteropServices;

namespace NXPorts.Tests.Infrastructure
{
    public static class PInvokeAssertions
    {
        internal static class UnsafeNativeMethods
        {

            [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
            public static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

            [DllImport("kernel32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool FreeLibrary(IntPtr hModule);

            [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            public static extern IntPtr LoadLibrary(string lpFileName);
        }

#pragma warning disable IDE0060 // Remove unused parameter
        public static void RunsWithoutError<TDelegate>(this Assert assert, string filePath, string expectedFunctionAlias, Action<TDelegate> action)
#pragma warning restore IDE0060 // Remove unused parameter
            where TDelegate : Delegate
        {
            if (action is null)
                throw new ArgumentNullException(nameof(action));
            var dllHandle = UnsafeNativeMethods.LoadLibrary(filePath);
            if (dllHandle == IntPtr.Zero)
                throw new AssertFailedException("Could not load library");
            var procedureAddress = UnsafeNativeMethods.GetProcAddress(dllHandle, expectedFunctionAlias);
            if (procedureAddress == IntPtr.Zero)
                throw new AssertFailedException("Could not load export.");

            try
            {
                var pInvokeDelegate = Marshal.GetDelegateForFunctionPointer<TDelegate>(procedureAddress);
                action(pInvokeDelegate);
            }
            catch (Exception e)
            {
                throw new AssertFailedException("Invoking the delegate ran with errors.", e);
            }
            finally
            {
                UnsafeNativeMethods.FreeLibrary(dllHandle);
            }
        }
    }
}
