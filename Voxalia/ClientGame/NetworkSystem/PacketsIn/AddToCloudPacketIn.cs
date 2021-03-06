//
// This file is part of the game Voxalia, created by Frenetic LLC.
// This code is Copyright (C) 2016-2017 Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.Shared;
using Voxalia.ClientGame.WorldSystem;
using FreneticGameCore;

namespace Voxalia.ClientGame.NetworkSystem.PacketsIn
{
    public class AddToCloudPacketIn: AbstractPacketIn
    {
        public override bool ParseBytesAndExecute(byte[] data)
        {
            if (data.Length != 24 + 4 + 4 + 8)
            {
                return false;
            }
            Location loc = Location.FromDoubleBytes(data, 0);
            float size = Utilities.BytesToFloat(Utilities.BytesPartial(data, 24, 4));
            float endsize = Utilities.BytesToFloat(Utilities.BytesPartial(data, 24 + 4, 4));
            long CID = Utilities.BytesToLong(Utilities.BytesPartial(data, 24 + 4 + 4, 8));
            for (int i = 0; i < TheClient.TheRegion.Clouds.Count; i++)
            {
                if (TheClient.TheRegion.Clouds[i].CID == CID)
                {
                    TheClient.TheRegion.Clouds[i].Points.Add(loc);
                    TheClient.TheRegion.Clouds[i].Sizes.Add(size);
                    TheClient.TheRegion.Clouds[i].EndSizes.Add(endsize);
                    return true;
                }
            }
            return false;
        }
    }
}
