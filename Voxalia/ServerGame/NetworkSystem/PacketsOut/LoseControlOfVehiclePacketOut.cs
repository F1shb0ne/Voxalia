//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016-2017 FreneticXYZ under the terms of a strict license.
// See README.md or LICENSE.txt in the source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voxalia.ServerGame.EntitySystem;
using Voxalia.Shared;

namespace Voxalia.ServerGame.NetworkSystem.PacketsOut
{
    public class LoseControlOfVehiclePacketOut : AbstractPacketOut
    {
        public LoseControlOfVehiclePacketOut(CharacterEntity driver, VehicleEntity vehicle)
        {
            ID = ServerToClientPacket.LOSE_CONTROL_OF_VEHICLE;
            Data = new byte[8 + 8];
            Utilities.LongToBytes(driver.EID).CopyTo(Data, 0);
            Utilities.LongToBytes(vehicle.EID).CopyTo(Data, 8);
        }
    }
}
