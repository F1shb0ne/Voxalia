﻿using Voxalia.Shared;

namespace Voxalia.ServerGame.NetworkSystem.PacketsOut
{
    public class RemoveItemPacketOut: AbstractPacketOut
    {
        public RemoveItemPacketOut(int spot)
        {
            UsageType = NetUsageType.GENERAL;
            ID = ServerToClientPacket.REMOVE_ITEM;
            Data = Utilities.IntToBytes(spot);
        }
    }
}
