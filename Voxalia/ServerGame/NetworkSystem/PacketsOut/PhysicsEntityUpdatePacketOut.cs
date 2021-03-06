//
// This file is part of the game Voxalia, created by Frenetic LLC.
// This code is Copyright (C) 2016-2017 Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

using System;
using Voxalia.Shared;
using Voxalia.ServerGame.EntitySystem;
using FreneticGameCore;

namespace Voxalia.ServerGame.NetworkSystem.PacketsOut
{
    public class PhysicsEntityUpdatePacketOut: AbstractPacketOut
    {
        public PhysicsEntityUpdatePacketOut(PhysicsEntity e)
        {
            UsageType = NetUsageType.ENTITIES;
            ID = ServerToClientPacket.PHYSICS_ENTITY_UPDATE;
            Data = new byte[24 + 24 + 16 + 24 + 1 + 8];
            e.GetPosition().ToDoubleBytes().CopyTo(Data, 0);
            e.GetVelocity().ToDoubleBytes().CopyTo(Data, 24);
            Utilities.QuaternionToBytes(e.GetOrientation()).CopyTo(Data, 24 + 24);
            e.GetAngularVelocity().ToDoubleBytes().CopyTo(Data, 24 + 24 + 16);
            Data[24 + 24 + 16 + 24] = (byte)((e.Body != null && e.Body.ActivityInformation.IsActive) ? 1 : 0);
            Utilities.LongToBytes(e.EID).CopyTo(Data, 24 + 24 + 16 + 24 + 1);
        }
    }
}
