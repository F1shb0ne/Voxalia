﻿using System;
using System.Collections.Generic;
using Voxalia.ClientGame.GraphicsSystems;
using Voxalia.Shared;
using OpenTK;
using Voxalia.ClientGame.OtherSystems;
using Voxalia.Shared.Collision;

namespace Voxalia.ClientGame.WorldSystem
{
    public partial class Chunk
    {
        public VBO _VBO = null;
        
        public void CreateVBO()
        {
            OwningRegion.NeedToRender(this);
        }

        /// <summary>
        /// Internal region call only.
        /// </summary>
        public void MakeVBONow()
        {
            if (rendering != null)
            {
                ASyncScheduleItem item = OwningRegion.TheClient.Schedule.AddASyncTask(() => VBOHInternal());
                rendering = rendering.ReplaceOrFollowWith(item);
            }
            else
            {
                rendering = OwningRegion.TheClient.Schedule.StartASyncTask(() => VBOHInternal());
            }
        }

        public ASyncScheduleItem rendering = null;
        
        public static Vector3i[] dirs = new Vector3i[] { new Vector3i(1, 0, 0), new Vector3i(0, 1, 0), new Vector3i(0, 0, 1), new Vector3i(1, 1, 0), new Vector3i(0, 1, 1), new Vector3i(1, 0, 1),
        new Vector3i(-1, 1, 0), new Vector3i(0, -1, 1), new Vector3i(-1, 0, 1), new Vector3i(1, 1, 1), new Vector3i(-1, 1, 1), new Vector3i(1, -1, 1), new Vector3i(1, 1, -1), new Vector3i(-1, -1, 1),
        new Vector3i(-1, 1, -1), new Vector3i(1, -1, -1) };

        BlockInternal GetMostSolid(Chunk c, int x, int y, int z)
        {
            if (c.PosMultiplier == PosMultiplier)
            {
                return c.GetBlockAt(x, y, z);
            }
            // TODO: better method here...
            if (c.PosMultiplier > PosMultiplier)
            {
                return new BlockInternal((ushort)Material.STONE, 0, 0, 0);
            }
            return BlockInternal.AIR;
        }

        public List<Vector3i> PlantsSpawned = null;

        void VBOHInternal()
        {
            try
            {
                bool shaped = OwningRegion.TheClient.CVars.r_noblockshapes.ValueB;
                Object locky = new Object();
                ChunkRenderHelper rh;
                lock (locky)
                {
                    rh = new ChunkRenderHelper();
                }
                if (DENIED)
                {
                    return;
                }
                List<Tuple<Vector3i, Matrix4, Model, Model, float>> PlantsToSpawn = new List<Tuple<Vector3i, Matrix4, Model, Model, float>>();
                Vector3 ppos = ClientUtilities.Convert(WorldPosition.ToLocation() * CHUNK_SIZE);
                //bool light = OwningRegion.TheClient.CVars.r_fallbacklighting.ValueB;
                Chunk c_zp = OwningRegion.GetChunk(WorldPosition + new Vector3i(0, 0, 1));
                Chunk c_zm = OwningRegion.GetChunk(WorldPosition + new Vector3i(0, 0, -1));
                Chunk c_yp = OwningRegion.GetChunk(WorldPosition + new Vector3i(0, 1, 0));
                Chunk c_ym = OwningRegion.GetChunk(WorldPosition + new Vector3i(0, -1, 0));
                Chunk c_xp = OwningRegion.GetChunk(WorldPosition + new Vector3i(1, 0, 0));
                Chunk c_xm = OwningRegion.GetChunk(WorldPosition + new Vector3i(-1, 0, 0));
                BlockInternal t_air = new BlockInternal((ushort)Material.STONE, 0, 0, 0);
                for (int x = 0; x < CSize; x++)
                {
                    for (int y = 0; y < CSize; y++)
                    {
                        for (int z = 0; z < CSize; z++)
                        {
                            BlockInternal c = GetBlockAt(x, y, z);
                            if ((c.Material).RendersAtAll())
                            {
                                BlockInternal zp = z + 1 < CSize ? GetBlockAt(x, y, z + 1) : (c_zp == null ? t_air : GetMostSolid(c_zp, x, y, z + 1 - CSize));
                                BlockInternal zm = z > 0 ? GetBlockAt(x, y, z - 1) : (c_zm == null ? t_air : GetMostSolid(c_zm, x, y, z - 1 + CSize));
                                BlockInternal yp = y + 1 < CSize ? GetBlockAt(x, y + 1, z) : (c_yp == null ? t_air : GetMostSolid(c_yp, x, y + 1 - CSize, z));
                                BlockInternal ym = y > 0 ? GetBlockAt(x, y - 1, z) : (c_ym == null ? t_air : GetMostSolid(c_ym, x, y - 1 + CSize, z));
                                BlockInternal xp = x + 1 < CSize ? GetBlockAt(x + 1, y, z) : (c_xp == null ? t_air : GetMostSolid(c_xp, x + 1 - CSize, y, z));
                                BlockInternal xm = x > 0 ? GetBlockAt(x - 1, y, z) : (c_xm == null ? t_air : GetMostSolid(c_xm, x - 1 + CSize, y, z));
                                bool rAS = !((Material)c.BlockMaterial).GetCanRenderAgainstSelf();
                                bool pMatters = !c.IsOpaque();
                                bool zps = (zp.IsOpaque() || (rAS && (zp.BlockMaterial == c.BlockMaterial && (pMatters || zp.BlockPaint == c.BlockPaint)))) && BlockShapeRegistry.BSD[shaped ? 0 : zp.BlockData].OccupiesBOTTOM();
                                bool zms = (zm.IsOpaque() || (rAS && (zm.BlockMaterial == c.BlockMaterial && (pMatters || zm.BlockPaint == c.BlockPaint)))) && BlockShapeRegistry.BSD[shaped ? 0 : zm.BlockData].OccupiesTOP();
                                bool xps = (xp.IsOpaque() || (rAS && (xp.BlockMaterial == c.BlockMaterial && (pMatters || xp.BlockPaint == c.BlockPaint)))) && BlockShapeRegistry.BSD[shaped ? 0 : xp.BlockData].OccupiesXM();
                                bool xms = (xm.IsOpaque() || (rAS && (xm.BlockMaterial == c.BlockMaterial && (pMatters || xm.BlockPaint == c.BlockPaint)))) && BlockShapeRegistry.BSD[shaped ? 0 : xm.BlockData].OccupiesXP();
                                bool yps = (yp.IsOpaque() || (rAS && (yp.BlockMaterial == c.BlockMaterial && (pMatters || yp.BlockPaint == c.BlockPaint)))) && BlockShapeRegistry.BSD[shaped ? 0 : yp.BlockData].OccupiesYM();
                                bool yms = (ym.IsOpaque() || (rAS && (ym.BlockMaterial == c.BlockMaterial && (pMatters || ym.BlockPaint == c.BlockPaint)))) && BlockShapeRegistry.BSD[shaped ? 0 : ym.BlockData].OccupiesYP();
                                if (zps && zms && xps && xms && yps && yms)
                                {
                                    continue;
                                }
                                BEPUutilities.Vector3 pos = new BEPUutilities.Vector3(x, y, z);
                                List<BEPUutilities.Vector3> vecsi = BlockShapeRegistry.BSD[shaped ? 0 : c.BlockData].GetVertices(pos, xps, xms, yps, yms, zps, zms);
                                List<BEPUutilities.Vector3> normsi = BlockShapeRegistry.BSD[shaped ? 0 : c.BlockData].GetNormals(pos, xps, xms, yps, yms, zps, zms);
                                List<BEPUutilities.Vector3> tci = BlockShapeRegistry.BSD[shaped ? 0 : c.BlockData].GetTCoords(pos, (Material)c.BlockMaterial, xps, xms, yps, yms, zps, zms);
                                KeyValuePair<List<BEPUutilities.Vector4>, List<BEPUutilities.Vector4>> ths = !c.BlockShareTex ? default(KeyValuePair<List<BEPUutilities.Vector4>, List<BEPUutilities.Vector4>>) :
                                    BlockShapeRegistry.BSD[shaped ? 0 : c.BlockData].GetStretchData(pos, vecsi, xp, xm, yp, ym, zp, zm, xps, xms, yps, yms, zps, zms);
                                for (int i = 0; i < vecsi.Count; i++)
                                {
                                    Vector3 vt = new Vector3((float)vecsi[i].X * PosMultiplier + ppos.X, (float)vecsi[i].Y * PosMultiplier + ppos.Y, (float)vecsi[i].Z * PosMultiplier + ppos.Z);
                                    rh.Vertices.Add(vt);
                                    Vector3 nt = new Vector3((float)normsi[i].X, (float)normsi[i].Y, (float)normsi[i].Z);
                                    rh.Norms.Add(nt);
                                    rh.TCoords.Add(new Vector3((float)tci[i].X, (float)tci[i].Y, (float)tci[i].Z));
                                    Location lcol = OwningRegion.GetLightAmount(ClientUtilities.Convert(vt), ClientUtilities.Convert(nt));
                                    rh.Cols.Add(new Vector4((float)lcol.X, (float)lcol.Y, (float)lcol.Z, 1));
                                    rh.TCols.Add(OwningRegion.TheClient.Rendering.AdaptColor(vt, Colors.ForByte(c.BlockPaint)));
                                    if (ths.Key != null)
                                    {
                                        rh.THVs.Add(new Vector4((float)ths.Key[i].X, (float)ths.Key[i].Y, (float)ths.Key[i].Z, (float)ths.Key[i].W));
                                        rh.THWs.Add(new Vector4((float)ths.Value[i].X, (float)ths.Value[i].Y, (float)ths.Value[i].Z, (float)ths.Value[i].W));
                                    }
                                    else
                                    {
                                        rh.THVs.Add(new Vector4(0, 0, 0, 0));
                                        rh.THWs.Add(new Vector4(0, 0, 0, 0));
                                    }
                                }
                                if (!c.IsOpaque() && BlockShapeRegistry.BSD[shaped ? 0 : c.BlockData].BackTextureAllowed)
                                {
                                    int tf = rh.Cols.Count - vecsi.Count;
                                    for (int i = vecsi.Count - 1; i >= 0; i--)
                                    {
                                        rh.Vertices.Add(new Vector3((float)vecsi[i].X * PosMultiplier + ppos.X, (float)vecsi[i].Y * PosMultiplier + ppos.Y, (float)vecsi[i].Z * PosMultiplier + ppos.Z));
                                        int tx = tf + i;
                                        rh.Cols.Add(rh.Cols[tx]);
                                        rh.TCols.Add(rh.TCols[tx]);
                                        rh.Norms.Add(new Vector3((float)-normsi[i].X, (float)-normsi[i].Y, (float)-normsi[i].Z));
                                        rh.TCoords.Add(new Vector3((float)tci[i].X, (float)tci[i].Y, (float)tci[i].Z));
                                        if (ths.Key != null)
                                        {
                                            rh.THVs.Add(new Vector4((float)ths.Key[i].X, (float)ths.Key[i].Y, (float)ths.Key[i].Z, (float)ths.Key[i].W));
                                            rh.THWs.Add(new Vector4((float)ths.Value[i].X, (float)ths.Value[i].Y, (float)ths.Value[i].Z, (float)ths.Value[i].W));
                                        }
                                        else
                                        {
                                            rh.THVs.Add(new Vector4(0, 0, 0, 0));
                                            rh.THWs.Add(new Vector4(0, 0, 0, 0));
                                        }
                                    }
                                }
                                if (c.Material.GetPlant() != null && !zp.Material.RendersAtAll() && zp.Material.GetSolidity() == MaterialSolidity.NONSOLID)
                                {
                                    Location offset;
                                    BEPUphysics.CollisionShapes.EntityShape es = BlockShapeRegistry.BSD[c.BlockData].GetShape(c.Damage, out offset, false);
                                    BEPUutilities.RayHit rayhit;
                                    es.GetCollidableInstance().RayCast(new BEPUutilities.Ray(new BEPUutilities.Vector3(0, 0, 2), new BEPUutilities.Vector3(0, 0, -1)), 3, out rayhit);
                                    Model m = OwningRegion.TheClient.Models.GetModel(c.Material.GetPlant() + "_hd");
                                    Model m2 = OwningRegion.TheClient.Models.GetModel(c.Material.GetPlant());
                                    Vector3 trans = new Vector3(WorldPosition.X * CHUNK_SIZE + x + 0.5f, WorldPosition.Y* CHUNK_SIZE +y + 0.5f, WorldPosition.Z* CHUNK_SIZE +z + 1);
                                    Matrix4 tmat = Matrix4.CreateTranslation(trans);
                                    if (rayhit.Normal.LengthSquared() > 0)
                                    {
                                        BEPUutilities.Vector3 plantalign = new BEPUutilities.Vector3(0, 0, 1);
                                        BEPUutilities.Quaternion orient;
                                        BEPUutilities.Quaternion.GetQuaternionBetweenNormalizedVectors(ref plantalign, ref rayhit.Normal, out orient);
                                        tmat = Matrix4.CreateFromQuaternion(new Quaternion((float)orient.X, (float)orient.Y, (float)orient.Z, (float)orient.W)) * tmat;
                                    }
                                    Location skylight = OwningRegion.GetLightAmount(ClientUtilities.Convert(trans), Location.UnitZ);
                                    PlantsToSpawn.Add(new Tuple<Vector3i, Matrix4, Model, Model, float>(WorldPosition * CHUNK_SIZE + new Vector3i(x, y, z + 1), tmat, m, m2, (float)skylight.X));
                                }
                            }
                        }
                    }
                }
                for (int i = 0; i < rh.Vertices.Count; i += 3)
                {
                    Vector3 v1 = rh.Vertices[i];
                    Vector3 dv1 = rh.Vertices[i + 1] - v1;
                    Vector3 dv2 = rh.Vertices[i + 2] - v1;
                    Vector3 t1 = rh.TCoords[i];
                    Vector3 dt1 = rh.TCoords[i + 1] - t1;
                    Vector3 dt2 = rh.TCoords[i + 2] - t1;
                    Vector3 tangent = (dv1 * dt2.Y - dv2 * dt1.Y) / (dt1.X * dt2.Y - dt1.Y * dt2.X);
                    Vector3 normal = rh.Norms[i];
                    tangent = (tangent - normal * Vector3.Dot(normal, tangent)).Normalized(); // TODO: Necessity of this correction?
                    rh.Tangs.Add(tangent);
                    rh.Tangs.Add(tangent);
                    rh.Tangs.Add(tangent);
                }
                if (rh.Vertices.Count == 0)
                {
                    OwningRegion.TheClient.Schedule.ScheduleSyncTask(() =>
                    {
                        if (_VBO != null)
                        {
                            VBO tV = _VBO;
                            lock (OwningRegion.TheClient.vbos)
                            {
                                if (OwningRegion.TheClient.vbos.Count < 40)
                                {
                                    OwningRegion.TheClient.vbos.Push(tV);
                                }
                                else
                                {
                                    tV.Destroy();
                                }
                            }
                        }
                        IsAir = true;
                        _VBO = null;
                    });
                    OwningRegion.DoneRendering(this);
                    return;
                }
                uint[] inds = new uint[rh.Vertices.Count];
                for (uint i = 0; i < rh.Vertices.Count; i++)
                {
                    inds[i] = i;
                }
                VBO tVBO;
                lock (OwningRegion.TheClient.vbos)
                {
                    if (OwningRegion.TheClient.vbos.Count > 0)
                    {
                        tVBO = OwningRegion.TheClient.vbos.Pop();
                    }
                    else
                    {
                        tVBO = new VBO();
                        tVBO.BufferMode = OpenTK.Graphics.OpenGL4.BufferUsageHint.StreamDraw;
                    }
                }
                lock (locky)
                {
                    tVBO.indices = inds;
                    tVBO.Vertices = rh.Vertices;
                    tVBO.Normals = rh.Norms;
                    tVBO.TexCoords = rh.TCoords;
                    tVBO.Colors = rh.Cols;
                    tVBO.TCOLs = rh.TCols;
                    tVBO.THVs = rh.THVs;
                    tVBO.THWs = rh.THWs;
                    tVBO.Tangents = rh.Tangs;
                    tVBO.BoneWeights = null;
                    tVBO.BoneIDs = null;
                    tVBO.BoneWeights2 = null;
                    tVBO.BoneIDs2 = null;
                    tVBO.oldvert();
                }
                OwningRegion.TheClient.Schedule.ScheduleSyncTask(() =>
                {
                    if (DENIED)
                    {
                        if (tVBO.generated)
                        {
                            tVBO.Destroy();
                        }
                        return;
                    }
                    if (_VBO != null)
                    {
                        VBO tV = _VBO;
                        lock (OwningRegion.TheClient.vbos)
                        {
                            if (OwningRegion.TheClient.vbos.Count < 40)
                            {
                                OwningRegion.TheClient.vbos.Push(tV);
                            }
                            else
                            {
                                tV.Destroy();
                            }
                        }
                    }
                    if (DENIED)
                    {
                        if (tVBO.generated)
                        {
                            tVBO.Destroy();
                        }
                        return;
                    }
                    _VBO = tVBO;
                    lock (locky)
                    {
                        tVBO.GenerateOrUpdate();
                        tVBO.CleanLists();
                    }
                    DestroyPlants();
                    PlantsSpawned = new List<Vector3i>();
                    foreach (Tuple<Vector3i, Matrix4, Model, Model, float> plant in PlantsToSpawn)
                    {
                        OwningRegion.AxisAlignedModels[plant.Item1] = new Tuple<Matrix4, Model, Model, float>(plant.Item2, plant.Item3, plant.Item4, plant.Item5);
                        PlantsSpawned.Add(plant.Item1);
                    }
                });
                OwningRegion.DoneRendering(this);
            }
            catch (Exception ex)
            {
                SysConsole.Output(OutputType.ERROR, "Generating ChunkVBO...: " + ex.ToString());
                OwningRegion.DoneRendering(this);
            }
        }

        public bool IsAir = false;
        
        public void Render()
        {
            if (_VBO != null && _VBO.generated)
            {
                _VBO.Render(OwningRegion.TheClient.RenderTextures);
            }
        }
    }

    public class ChunkRenderHelper
    {
        const int CSize = Chunk.CHUNK_SIZE;

        public ChunkRenderHelper()
        {
            Vertices = new List<Vector3>(CSize * CSize * CSize * 6);
            TCoords = new List<Vector3>(CSize * CSize * CSize * 6);
            Norms = new List<Vector3>(CSize * CSize * CSize * 6);
            Cols = new List<Vector4>(CSize * CSize * CSize * 6);
            TCols = new List<Vector4>(CSize * CSize * CSize * 6);
            THVs = new List<Vector4>(CSize * CSize * CSize * 6);
            THWs = new List<Vector4>(CSize * CSize * CSize * 6);
            Tangs = new List<Vector3>(CSize * CSize * CSize * 6);
    }
        public List<Vector3> Vertices;
        public List<Vector3> TCoords;
        public List<Vector3> Norms;
        public List<Vector4> Cols;
        public List<Vector4> TCols;
        public List<Vector4> THVs;
        public List<Vector4> THWs;
        public List<Vector3> Tangs;
    }
}
