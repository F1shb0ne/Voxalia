﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cloo;
using Cloo.Bindings;
using System.Runtime.InteropServices;
using FreneticGameCore;
using Voxalia.Shared;
using Voxalia.ClientGame.ClientMainSystem;
using OpenTK;
using OpenTK.Graphics;

namespace Voxalia.ClientGame.ComputeSystem
{
    public class OpenCLTest
    {
        public Client TheClient;

        public void Test()
        {
            Setup();
        }

        public IntPtr GenStringSpace(int size)
        {
            return Marshal.AllocHGlobal(size);
        }

        public byte[] IntPtrToBytes(IntPtr ptr, int size)
        {
            byte[] chrs = new byte[size];
            Marshal.Copy(ptr, chrs, 0, size);
            return chrs;
        }

        public string IntPtrToString(IntPtr ptr, int size)
        {
            byte[] chrs = new byte[size];
            Marshal.Copy(ptr, chrs, 0, size);
            return Encoding.UTF8.GetString(chrs, 0, size - 1); // The string is null-terminated, so get rid of the null!
        }

        public void DeleteIntPtrString(IntPtr ptr)
        {
            Marshal.FreeHGlobal(ptr);
        }

        public void CheckEC(ComputeErrorCode ec)
        {
            if (ec != ComputeErrorCode.Success)
            {
                SysConsole.Output(OutputType.WARNING, "Compute error: " + ec);
            }
        }

        public string GetPlatformInfo(CLPlatformHandle handle, ComputePlatformInfo type)
        {
            CheckEC(CL11.GetPlatformInfo(handle, type, IntPtr.Zero, IntPtr.Zero, out IntPtr tester));
            int size = tester.ToInt32();
            IntPtr genned = GenStringSpace(size);
            CheckEC(CL11.GetPlatformInfo(handle, type, tester, genned, out IntPtr IGNORED));
            string str = IntPtrToString(genned, size);
            DeleteIntPtrString(genned);
            return str;
        }

        public void Setup()
        {
            CheckEC(CL11.GetPlatformIDs(0, null, out int numplat));
            CLPlatformHandle[] plats = new CLPlatformHandle[numplat];
            CheckEC(CL11.GetPlatformIDs(numplat, plats, out int IGNORED));
            CLPlatformHandle bestPlat = default(CLPlatformHandle);
            int bestVal = -1;
            int bestId = -1;
            for (int i = 0; i < plats.Length; i++)
            {
                if (!plats[i].IsValid)
                {
                    // Just in case
                    continue;
                }
                string name = GetPlatformInfo(plats[i], ComputePlatformInfo.Name);
                string profile = GetPlatformInfo(plats[i], ComputePlatformInfo.Profile);
                string vendor = GetPlatformInfo(plats[i], ComputePlatformInfo.Vendor);
                string version = GetPlatformInfo(plats[i], ComputePlatformInfo.Version);
                SysConsole.Output(OutputType.INIT, "Found CL profile: " + i + " -> " + name + ", " + profile + ", " + vendor + ", " + version);
                int cVal = 0;
                if (vendor.ToLowerFast().Contains("nvid") || name.ToLowerFast().Contains("nvid"))
                {
                    cVal = 10; // NVIDIA? YES PLEASE!
                }
                else if (vendor.ToLowerFast().Contains("amd") || name.ToLowerFast().Contains("amd"))
                {
                    cVal = 7; // AMD? That's cool too!
                }
                else if (vendor.ToLowerFast().Contains("gpu") || name.ToLowerFast().Contains("gpu"))
                {
                    cVal = 5; // Anything GPU-ish? Okay!
                }
                else if (vendor.ToLowerFast().Contains("grap") || name.ToLowerFast().Contains("grap"))
                {
                    cVal = 4; // Anything Graphical? Okay!
                }
                // Everything else shares a priority for now.
                // Last-most entry is the least likely to be the CPU though!
                if (cVal >= bestVal)
                {
                    // We DEMAND a GPU!
                    CheckEC(CL11.GetDeviceIDs(plats[i], ComputeDeviceTypes.Gpu, 0, null, out int num_gdev));
                    if (num_gdev == 0)
                    {
                        continue;
                    }
                    bestVal = cVal;
                    bestPlat = plats[i];
                    bestId = i;
                }
            }
            ComputeDeviceTypes wantedType = ComputeDeviceTypes.Gpu;
            if (bestVal == -1)
            {
                // No GPU? Backup plan!
                if (plats.Length > 0)
                {
                    wantedType = ComputeDeviceTypes.All;
                    bestPlat = plats[plats.Length - 1];
                    bestId = plats.Length - 1;
                    SysConsole.Output(OutputType.INIT, "No GPU device, using CPU-based CL...");
                }
                else
                {
                    // This is quite bad!
                    throw new Exception("Can't load OpenCL: no platforms!");
                }
            }
            SysConsole.Output(OutputType.INIT, "Choose CL context: " + bestId);
            CheckEC(CL11.GetDeviceIDs(bestPlat, wantedType, 0, null, out int num_dev));
            CLDeviceHandle[] devs = new CLDeviceHandle[num_dev];
            CheckEC(CL11.GetDeviceIDs(bestPlat, wantedType, num_dev, devs, out IGNORED));
            IntPtr[] cprops = new IntPtr[]
            {
                (IntPtr)ComputeContextPropertyName.Platform,
                bestPlat.Value,
                IntPtr.Zero,
                IntPtr.Zero
            };
            CLContextHandle context = CL11.CreateContext(cprops, num_dev, devs, null, IntPtr.Zero, out ComputeErrorCode errcode_ret);
            CheckEC(errcode_ret);
            CLContext = context;
        }

        public void FreeCL()
        {
            CL11.ReleaseContext(CLContext);
        }

        // TODO: Ensure accurate OpenGL connection magic. Cloo appears too overly restricted and outdated to be able to do this correctly?
        /*
        public void Ignored()
        {
            CLx clx = new CLx(ComputePlatform.GetByHandle(bestPlat.Value));
            IntPtr[] props = new IntPtr[]
            {
                (IntPtr)ComputeContextPropertyName.Platform,
                bestPlat.Value,
                (IntPtr)ComputeContextPropertyName.CL_GL_CONTEXT_KHR,
                (TheClient.Window.Context as IGraphicsContextInternal).Context.Handle,
                // TODO: WGL_HDC_KHR?
                IntPtr.Zero
            };
            CheckEC(clx.GetGLContextInfoKHR(props, ComputeGLContextInfo.CL_CURRENT_DEVICE_FOR_GL_CONTEXT_KHR, IntPtr.Zero, IntPtr.Zero, out IntPtr size_ret));
            int sized = size_ret.ToInt32();
            IntPtr buff = GenStringSpace(sized);
            CheckEC(clx.GetGLContextInfoKHR(props, ComputeGLContextInfo.CL_CURRENT_DEVICE_FOR_GL_CONTEXT_KHR, size_ret, buff, out IntPtr IGNOREDPTR));
            byte[] b = IntPtrToBytes(buff, sized);
            DeleteIntPtrString(buff);
            CLDeviceHandle[] devs = new CLDeviceHandle[b.Length / IntPtr.Size];
            for (int i = 0; i < devs.Length; i++)
            {
                // Excessively 64-bit capped!
                ulong tmp = 0;
                for (int x = 0; x < IntPtr.Size; x++)
                {
                    tmp |= (ulong)b[i * IntPtr.Size + x] << x;
                }
                devs[i] = new CLDeviceHandle() { Handle = new IntPtr(unchecked((long)tmp)) };
            }
        }*/

        public CLContextHandle CLContext;
    }
}
