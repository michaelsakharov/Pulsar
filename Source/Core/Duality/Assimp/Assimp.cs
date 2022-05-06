/*
* Copyright (c) 2012-2018 AssimpNet - Nicholas Woodfield
* 
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in
* all copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
* THE SOFTWARE.
*/

/* Note 2019
* 
* This file is modified by Dongho Kang to distributed as a Unity package 2019.
*/ 

/* Note 2022
* 
* This file is modified by Michael Sakharov to be used inside Pulsar 3D Game Engine
*/ 

using Assimp.Unmanaged;
using Duality;
using Duality.IO;
using System.IO;

namespace Duality.Assimp
{
    /// <summary>
    /// AssimpNet This handles one-time initialization (before scene load) of the AssimpLibrary instance, setting DLL probing paths to load the correct native
    /// dependencies, if the current platform is supported.
    /// </summary>
    public static class AssimpHandler
    {
        private static bool s_triedLoading = false;
        private static bool s_assimpAvailable = false;

        /// <summary>
        /// Gets if the assimp library is available on this platform (e.g. the library can load native dependencies).
        /// </summary>
        public static bool IsAssimpAvailable
        {
            get
            {
                return s_assimpAvailable;
            }
        }

		public static void InitializeAssimp()
        {
            //Only try once during runtime
            if(s_triedLoading)
                return;

            UnmanagedLibrary libInstance = AssimpLibrary.Instance;

            //If already initialized, set flags and return
            if(libInstance.IsLibraryLoaded)
            {
                s_assimpAvailable = true;
                s_triedLoading = true;
                return;
            }

			//First time initialization, need to set a probing path (at least in editor) to resolve the native dependencies
			string native64LibPath = PathOp.GetFullPath(DualityApp.LibsDirectory) + "\\x86_64";
			string native32LibPath = PathOp.GetFullPath(DualityApp.LibsDirectory) + "\\x86";

			//Set if any platform needs to tweak the default name AssimpNet uses for the platform, null clears using an override at all
			string override64LibName = null;
            string override32LibName = null;

            //Set resolver properties, null will clear the property
            libInstance.Resolver.SetOverrideLibraryName64(override64LibName);
            libInstance.Resolver.SetOverrideLibraryName32(override32LibName);
            libInstance.Resolver.SetProbingPaths64(native64LibPath);
            libInstance.Resolver.SetProbingPaths32(native32LibPath);
            libInstance.ThrowOnLoadFailure = false;

            //Try and load the native library, if failed we won't get an exception
            bool success = libInstance.LoadLibrary();
            s_assimpAvailable = success;
            s_triedLoading = true;

            //Turn exceptions back on
            libInstance.ThrowOnLoadFailure = true;
        }
    }
}
