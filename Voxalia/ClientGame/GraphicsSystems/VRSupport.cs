﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Valve.VR;
using Voxalia.Shared;
using Voxalia.ClientGame.ClientMainSystem;
using OpenTK;

namespace Voxalia.ClientGame.GraphicsSystems
{
    public class VRSupport
    {
        public CVRSystem VR = null;

        public Client TheClient;

        public CVRCompositor Compositor;

        public static bool Available()
        {
            return OpenVR.IsHmdPresent();
        }

        public static VRSupport TryInit(Client tclient)
        {
            if (!Available())
            {
                return null;
            }
            EVRInitError err = EVRInitError.None;
            VRSupport vrs = new VRSupport();
            vrs.TheClient = tclient;
            vrs.VR = OpenVR.Init(ref err);
            if (err != EVRInitError.None)
            {
                SysConsole.Output(OutputType.INFO, "VR error: " + err + ": " + OpenVR.GetStringForHmdError(err));
                return null;
            }
            vrs.Start();
            return vrs;
        }

        public void Start()
        {
            uint w = 0;
            uint h = 0;
            VR.GetRecommendedRenderTargetSize(ref w, ref h);
            if (w <= 0 || h <= 0)
            {
                throw new Exception("Failed to start VR: Invalid render target size!");
            }
            TheClient.MainWorldView.Generate(TheClient, (int)w, (int)h);
            TheClient.MainWorldView.GenerateFBO();
            SysConsole.Output(OutputType.INFO, "Switching to VR mode!");
            Compositor = OpenVR.Compositor;
            Compositor.SetTrackingSpace(ETrackingUniverseOrigin.TrackingUniverseStanding);
            Compositor.CompositorBringToFront();
        }

        public Matrix4 Eye(bool lefteye)
        {
            HmdMatrix34_t temp = VR.GetEyeToHeadTransform(lefteye ? EVREye.Eye_Left : EVREye.Eye_Right);
            return new Matrix4(temp.m0, temp.m1, temp.m2, temp.m3, temp.m4, temp.m5, temp.m6, temp.m7, temp.m8, temp.m9, temp.m10, temp.m11, 0, 0, 0, 1);
        }

        public Matrix4 GetProjection(bool lefteye, float znear, float zfar)
        {
            HmdMatrix44_t temp = VR.GetProjectionMatrix(lefteye ? EVREye.Eye_Left : EVREye.Eye_Right, znear, zfar, EGraphicsAPIConvention.API_OpenGL);
            return new Matrix4(temp.m0, temp.m1, temp.m2, temp.m3, temp.m4, temp.m5, temp.m6, temp.m7, temp.m8, temp.m9, temp.m10, temp.m11, temp.m12, temp.m13, temp.m14, temp.m15);
        }

        public void Stop()
        {
            OpenVR.Shutdown();
        }

        public void Submit()
        {
            VREvent_t evt = new VREvent_t();
            while (VR.PollNextEvent(ref evt, (uint)Marshal.SizeOf(typeof(VREvent_t))))
            {
                // No need to do anything here!
            }
            TrackedDevicePose_t[] rposes = new TrackedDevicePose_t[OpenVR.k_unMaxTrackedDeviceCount];
            TrackedDevicePose_t[] gposes = new TrackedDevicePose_t[OpenVR.k_unMaxTrackedDeviceCount];
            EVRCompositorError merr = Compositor.WaitGetPoses(rposes, gposes);
            if (merr != EVRCompositorError.None)
            {
                SysConsole.Output(OutputType.INFO, "Posing error: " + merr);
            }
            if (!Compositor.CanRenderScene())
            {
                SysConsole.Output(OutputType.INFO, "Can't render VR scene!");
            }
            Texture_t left = new Texture_t();
            left.eColorSpace = EColorSpace.Auto;
            left.eType = EGraphicsAPIConvention.API_OpenGL;
            left.handle = new IntPtr(TheClient.MainWorldView.CurrentFBOTexture);
            VRTextureBounds_t bounds = new VRTextureBounds_t();
            bounds.uMin = 0f;
            bounds.uMax = 0.5f;
            bounds.vMin = 0f;
            bounds.vMax = 1f;
            EVRCompositorError lerr = Compositor.Submit(EVREye.Eye_Left, ref left, ref bounds, EVRSubmitFlags.Submit_Default);
            if (lerr != EVRCompositorError.None)
            {
                SysConsole.Output(OutputType.INFO, "Left eye error: " + lerr);
            }
            Texture_t right = new Texture_t();
            right.eColorSpace = EColorSpace.Auto;
            right.eType = EGraphicsAPIConvention.API_OpenGL;
            right.handle = new IntPtr(TheClient.MainWorldView.CurrentFBOTexture);
            VRTextureBounds_t rbounds = new VRTextureBounds_t();
            rbounds.uMin = 0.5f;
            rbounds.uMax = 1f;
            rbounds.vMin = 0f;
            rbounds.vMax = 1f;
            EVRCompositorError rerr = Compositor.Submit(EVREye.Eye_Right, ref right, ref rbounds, EVRSubmitFlags.Submit_Default);
            if (rerr != EVRCompositorError.None)
            {
                SysConsole.Output(OutputType.INFO, "Right eye error: " + rerr);
            }
        }
    }
}
