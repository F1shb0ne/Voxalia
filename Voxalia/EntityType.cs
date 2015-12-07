﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Voxalia
{
    public enum EntityType: int
    {
        PLAYER = 1,
        ARROW = 2,
        BLOCK_GROUP = 3,
        BLOCK_ITEM = 4,
        BULLET = 5,
        GLOWSTICK = 6,
        SMOKE_GRENADE = 7,
        MODEL = 8,
        ITEM = 9,
        VEHICLE = 10,
        LEGACY_SPAWNPOINT = 100
    }
}
