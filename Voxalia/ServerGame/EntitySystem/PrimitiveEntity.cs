//
// This file is part of the game Voxalia, created by Frenetic LLC.
// This code is Copyright (C) 2016-2017 Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

using System;
using System.Collections.Generic;
using Voxalia.Shared;
using Voxalia.ServerGame.NetworkSystem.PacketsOut;
using Voxalia.ServerGame.WorldSystem;
using Voxalia.Shared.Collision;
using Voxalia.ServerGame.NetworkSystem;
using LiteDB;
using FreneticGameCore;
using FreneticGameCore.Collision;

namespace Voxalia.ServerGame.EntitySystem
{
    public class CollisionEventArgs : EventArgs
    {
        public CollisionEventArgs(CollisionResult cr)
        {
            Info = cr;
        }

        public CollisionResult Info;
    }

    public abstract class PrimitiveEntity: Entity
    {
        public Location Gravity = Location.Zero;

        public HashSet<long> NoCollide = new HashSet<long>();

        public abstract string GetModel();

        public override long GetRAMUsage()
        {
            return base.GetRAMUsage() + 50 + NoCollide.Count * 16;
        }

        public PrimitiveEntity(Region tregion)
            : base(tregion, true)
        {
        }

        public const int PrimitiveNetDataLength = 24 + 24 + 16 + 24 + 24 + 4;

        public byte[] GetPrimitiveNetData()
        {
            byte[] Data = new byte[PrimitiveNetDataLength];
            GetPosition().ToDoubleBytes().CopyTo(Data, 0);
            GetVelocity().ToDoubleBytes().CopyTo(Data, 24);
            Utilities.QuaternionToBytes(Angles).CopyTo(Data, 24 + 24);
            Scale.ToDoubleBytes().CopyTo(Data, 24 + 24 + 16);
            Gravity.ToDoubleBytes().CopyTo(Data, 24 + 24 + 16 + 24);
            Utilities.IntToBytes(TheServer.Networking.Strings.IndexForString(GetModel())).CopyTo(Data, 24 + 24 + 16 + 24 + 24);
            return Data;
        }

        public override void PotentialActivate()
        {
        }

        public bool network = true;

        public bool FilterHandle(BEPUphysics.BroadPhaseEntries.BroadPhaseEntry entry)
        {
            if (entry is BEPUphysics.BroadPhaseEntries.MobileCollidables.EntityCollidable)
            {
                long eid = ((PhysicsEntity)((BEPUphysics.BroadPhaseEntries.MobileCollidables.EntityCollidable)entry).Entity.Tag).EID;
                if (NoCollide.Contains(eid))
                {
                    return false;
                }
            }
            return CollisionUtil.ShouldCollide(entry);
        }
        
        public double netdeltat = 0;

        public override void Tick()
        {
            if (TheRegion.IsVisible(GetPosition()))
            {
                bool sme = false;
                SetVelocity(GetVelocity() * 0.99f + Gravity * TheRegion.Delta);
                if (GetVelocity().LengthSquared() > 0)
                {
                    CollisionResult cr = TheRegion.Collision.CuboidLineTrace(Scale, GetPosition(), GetPosition() + GetVelocity() * TheRegion.Delta, FilterHandle);
                    Location vel = GetVelocity();
                    if (cr.Hit && Collide != null)
                    {
                        Collide(this, new CollisionEventArgs(cr));
                    }
                    if (!IsSpawned || Removed)
                    {
                        return;
                    }
                    if (vel == GetVelocity())
                    {
                        SetVelocity((cr.Position - GetPosition()) / TheRegion.Delta);
                    }
                    SetPosition(cr.Position);
                    // TODO: Timer
                    if (network)
                    {
                        sme = true;
                    }
                    netdeltat = 2;
                }
                else
                {
                    netdeltat += TheRegion.Delta;
                    if (netdeltat > 2.0)
                    {
                        netdeltat -= 2.0;
                        sme = true;
                    }
                }
                Location pos = GetPosition();
                PrimitiveEntityUpdatePacketOut primupd = sme ? new PrimitiveEntityUpdatePacketOut(this) : null;
                foreach (PlayerEntity player in TheRegion.Players)
                {
                    bool shouldseec = player.ShouldSeePosition(pos);
                    bool shouldseel = player.ShouldSeePositionPreviously(lPos);
                    if (shouldseec && !shouldseel)
                    {
                        player.Network.SendPacket(GetSpawnPacket());
                    }
                    if (shouldseel && !shouldseec)
                    {
                        player.Network.SendPacket(new DespawnEntityPacketOut(EID));
                    }
                    if (sme && shouldseec)
                    {
                        player.Network.SendPacket(primupd);
                    }
                }
            }
            lPos = GetPosition();
            Vector3i cpos = TheRegion.ChunkLocFor(lPos);
            if (CanSave && !TheRegion.LoadedChunks.ContainsKey(cpos))
            {
                TheRegion.LoadChunk(cpos);
            }
        }
        
        public Location lPos = Location.NaN;

        public EventHandler<CollisionEventArgs> Collide;

        public virtual void Spawn()
        {
            NoCollide.Add(EID);
        }

        public virtual void Destroy()
        {
            NoCollide.Remove(EID);
        }

        public Location Position;

        public Location Velocity;

        public Location Scale;

        public BEPUutilities.Quaternion Angles;

        public override Location GetPosition()
        {
            return Position;
        }

        public override void SetPosition(Location pos)
        {
            Position = pos;
        }

        public Location GetVelocity()
        {
            return Velocity;
        }

        public virtual void SetVelocity(Location vel)
        {
            Velocity = vel;
        }


        public override BEPUutilities.Quaternion GetOrientation()
        {
            return Angles;
        }

        public override void SetOrientation(BEPUutilities.Quaternion quat)
        {
            Angles = quat;
        }

        public void ApplyPrimitiveSaveData(BsonDocument doc)
        {
            Position = Location.FromDoubleBytes(doc["prim_pos"].AsBinary, 0);
            Velocity = Location.FromDoubleBytes(doc["prim_vel"].AsBinary, 0);
            Scale = Location.FromDoubleBytes(doc["prim_scale"].AsBinary, 0);
            Gravity = Location.FromDoubleBytes(doc["prim_grav"].AsBinary, 0);
            double x = doc["prim_ang_x"].AsDouble;
            double y = doc["prim_ang_y"].AsDouble;
            double z = doc["prim_ang_z"].AsDouble;
            double w = doc["prim_ang_w"].AsDouble;
            SetOrientation(new BEPUutilities.Quaternion(x, y, z, w));
            foreach (BsonValue bsv in doc["prim_noco"].AsArray)
            {
                NoCollide.Add(bsv.AsInt64);
            }
        }

        public override BsonDocument GetSaveData()
        {
            BsonDocument doc = new BsonDocument();
            byte[] pos = Position.ToDoubleBytes();
            doc["prim_pos"] = pos;
            doc["prim_vel"] = Velocity.ToDoubleBytes();
            doc["prim_scale"] = Scale.ToDoubleBytes();
            doc["prim_grav"] = Gravity.ToDoubleBytes();
            BEPUutilities.Quaternion quat = GetOrientation();
            doc["prim_ang_x"] = (double)quat.X;
            doc["prim_ang_y"] = (double)quat.Y;
            doc["prim_ang_z"] = (double)quat.Z;
            doc["prim_ang_w"] = (double)quat.W;
            BsonArray b = new BsonArray();
            foreach (long l in NoCollide)
            {
                b.Add(l);
            }
            doc["prim_noco"] = b;
            return doc;
        }
    }
}
