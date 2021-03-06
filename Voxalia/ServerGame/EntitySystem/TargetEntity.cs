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
using Voxalia.ServerGame.WorldSystem;
using Voxalia.Shared;
using Voxalia.ServerGame.ItemSystem;
using Voxalia.ServerGame.NetworkSystem.PacketsOut;
using LiteDB;
using FreneticGameCore;

namespace Voxalia.ServerGame.EntitySystem
{
    class TargetEntity: HumanoidEntity
    {
        public TargetEntity(Region tregion) :
            base (tregion)
        {
            model = "players/human_male_004";
            mod_zrot = 270;
            mod_scale = 1.5f;
            Damageable().SetMaxHealth(100);
            Damageable().SetHealth(100);
            SetMass(70);
            Items = new EntityInventory(tregion, this);
            // TODO: Better way to gather item details!
            Items.GiveItem(TheServer.Items.GetItem("weapons/rifles/m4"));
            Items.GiveItem(new ItemStack("bullet", "rifle_ammo", TheServer, 1000, "items/weapons/ammo/rifle_round_ico", "Assault Rifle Ammo", "Very rapid!", System.Drawing.Color.White, "items/weapons/ammo/rifle_round", false, 0));
            Items.cItem = 1;
            Items.Items[0].Info.PrepItem(this, Items.Items[0]);
            ShouldShine = true;
            Damageable().EffectiveDeathEvent.Add((e) =>
            {
                if (Removed)
                {
                    return;
                }
                TheRegion.Explode(GetPosition(), 5);
                RemoveMe();
            }, 0);
        }

        public override NetworkEntityType GetNetType()
        {
            return NetworkEntityType.CHARACTER;
        }

        public override byte[] GetNetData()
        {
            return GetCharacterNetData();
        }

        public override EntityType GetEntityType()
        {
            return EntityType.TARGET_ENTITY;
        }

        public double NextBoing = 0;

        public double NextAttack = 0;

        public override BsonDocument GetSaveData()
        {
            // TODO: Save properly!
            return null;
        }

        public override void Tick()
        {
            base.Tick();
            NextBoing -= TheRegion.Delta;
            if (NextBoing <= 0)
            {
                NextBoing = Utilities.UtilRandom.NextDouble() * 2 + 0.5;
                XMove = (double)Utilities.UtilRandom.NextDouble() * 2f - 1f;
                YMove = (double)Utilities.UtilRandom.NextDouble() * 2f - 1f;
                Upward = Utilities.UtilRandom.Next(100) > 75;
            }
            NextAttack -= TheRegion.Delta;
            if (NextAttack <= 0)
            {
                PlayerEntity player = NearestPlayer(out double distsq);
                if (distsq < 10 * 10)
                {
                    Location target = player.GetCenter();
                    Location pos = GetEyePosition();
                    Location rel = (target - pos).Normalize();
                    Direction = Utilities.VectorToAngles(rel);
                    Direction.Yaw += 180;
                    Items.Items[0].Info.Click(this, Items.Items[0]);
                    Items.Items[0].Info.ReleaseClick(this, Items.Items[0]);
                }
                NextAttack = Utilities.UtilRandom.NextDouble() * 2 + 0.5;
            }
        }

        public PlayerEntity NearestPlayer(out double distSquared)
        {
            PlayerEntity player = null;
            double distsq = double.MaxValue;
            Location p = GetCenter();
            foreach (PlayerEntity tester in TheRegion.Players)
            {
                double td = (tester.GetCenter() - p).LengthSquared();
                if (td < distsq)
                {
                    player = tester;
                    distsq = td;
                }
            }
            distSquared = distsq;
            return player;
        }
    }
}
