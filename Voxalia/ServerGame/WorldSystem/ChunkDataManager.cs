//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of a strict license.
// See README.md or LICENSE.txt in the source root for contents of the license.
// If neither of these are not available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using LiteDB;
using Voxalia.Shared;
using Voxalia.Shared.Collision;
using Voxalia.Shared.Files;

namespace Voxalia.ServerGame.WorldSystem
{
    public class ChunkDataManager
    {
        public Region TheRegion;

        LiteDatabase Database;

        LiteCollection<BsonDocument> DBChunks;

        LiteCollection<BsonDocument> DBTops;

        LiteCollection<BsonDocument> DBTopsHigher;

        LiteCollection<BsonDocument> DBMins;

        LiteDatabase LODsDatabase;

        LiteCollection<BsonDocument> DBLODs;

        LiteDatabase EntsDatabase;

        LiteCollection<BsonDocument> DBEnts;
        
        public void Init(Region tregion)
        {
            TheRegion = tregion;
            string dir = "/saves/" + TheRegion.TheWorld.Name + "/";
            TheRegion.TheServer.Files.CreateDirectory(dir);
            dir = TheRegion.TheServer.Files.BaseDirectory + dir;
            Database = new LiteDatabase("filename=" + dir + "chunks.ldb");
            DBChunks = Database.GetCollection<BsonDocument>("chunks");
            DBTops = Database.GetCollection<BsonDocument>("tops");
            DBTopsHigher = Database.GetCollection<BsonDocument>("topshigh");
            DBMins = Database.GetCollection<BsonDocument>("mins");
            LODsDatabase = new LiteDatabase("filename=" + dir + "lod_chunks.ldb");
            DBLODs = LODsDatabase.GetCollection<BsonDocument>("lodchunks");
            EntsDatabase = new LiteDatabase("filename=" + dir + "ents.ldb");
            DBEnts = EntsDatabase.GetCollection<BsonDocument>("ents");
        }
        
        public void Shutdown()
        {
            Database.Dispose();
            LODsDatabase.Dispose();
            EntsDatabase.Dispose();
        }

        public BsonValue GetIDFor(int x, int y, int z)
        {
            byte[] array = new byte[12];
            Utilities.IntToBytes(x).CopyTo(array, 0);
            Utilities.IntToBytes(y).CopyTo(array, 4);
            Utilities.IntToBytes(z).CopyTo(array, 8);
            return new BsonValue(array);
        }

        public byte[] GetLODChunkDetails(int x, int y, int z)
        {
            BsonDocument doc;
            doc = DBLODs.FindById(GetIDFor(x, y, z));
            if (doc == null)
            {
                return null;
            }
            return FileHandler.Uncompress(doc["blocks"].AsBinary);
        }

        public void WriteLODChunkDetails(int x, int y, int z, byte[] LOD)
        {
            BsonValue id = GetIDFor(x, y, z);
            BsonDocument newdoc = new BsonDocument();
            Dictionary<string, BsonValue> tbs = newdoc.RawValue;
            tbs["_id"] = id;
            tbs["blocks"] = new BsonValue(FileHandler.Compress(LOD));
            DBLODs.Upsert(newdoc);
        }

        public ChunkDetails GetChunkEntities(int x, int y, int z)
        {
            BsonDocument doc;
            doc = DBEnts.FindById(GetIDFor(x, y, z));
            if (doc == null)
            {
                return null;
            }
            ChunkDetails det = new ChunkDetails();
            det.X = x;
            det.Y = y;
            det.Z = z;
            det.Version = doc["version"].AsInt32;
            det.Blocks = /*FileHandler.UnGZip(*/doc["entities"].AsBinary/*)*/;
            return det;
        }

        public void WriteChunkEntities(ChunkDetails details)
        {
            BsonValue id = GetIDFor(details.X, details.Y, details.Z);
            BsonDocument newdoc = new BsonDocument();
            Dictionary<string, BsonValue> tbs = newdoc.RawValue;
            tbs["_id"] = id;
            tbs["version"] = new BsonValue(details.Version);
            tbs["entities"] = new BsonValue(/*FileHandler.GZip(*/details.Blocks/*)*/);
            DBEnts.Upsert(newdoc);
        }

        public ChunkDetails GetChunkDetails(int x, int y, int z)
        {
            BsonDocument doc;
            doc = DBChunks.FindById(GetIDFor(x, y, z));
            if (doc == null)
            {
                return null;
            }
            ChunkDetails det = new ChunkDetails();
            det.X = x;
            det.Y = y;
            det.Z = z;
            det.Version = doc["version"].AsInt32;
            det.Flags = (ChunkFlags)doc["flags"].AsInt32;
            det.Blocks = FileHandler.Uncompress(doc["blocks"].AsBinary);
            det.Reachables = doc["reach"].AsBinary;
            return det;
        }

        public void WriteChunkDetails(ChunkDetails details)
        {
            BsonValue id = GetIDFor(details.X, details.Y, details.Z);
            BsonDocument newdoc = new BsonDocument();
            Dictionary<string, BsonValue> tbs = newdoc.RawValue;
            tbs["_id"] = id;
            tbs["version"] = new BsonValue(details.Version);
            tbs["flags"] = new BsonValue((int)details.Flags);
            tbs["blocks"] = new BsonValue(FileHandler.Compress(details.Blocks));
            tbs["reach"] = new BsonValue(details.Reachables);
            DBChunks.Upsert(newdoc);
        }

        public void ClearChunkDetails(Vector3i details)
        {
            BsonValue id = GetIDFor(details.X, details.Y, details.Z);
            DBChunks.Delete(id);
        }

        public void WriteTopsHigher(int x, int y, int z, byte[] tops)
        {
            BsonValue id = GetIDFor(x, y, z);
            BsonDocument newdoc = new BsonDocument();
            Dictionary<string, BsonValue> tbs = newdoc.RawValue;
            tbs["_id"] = id;
            tbs["tops"] = new BsonValue(FileHandler.Compress(tops));
            DBTopsHigher.Upsert(newdoc);
        }

        public byte[] GetTopsHigher(int x, int y, int z)
        {
            BsonDocument doc;
            doc = DBTopsHigher.FindById(GetIDFor(x, y, z));
            if (doc == null)
            {
                return null;
            }
            return FileHandler.Uncompress(doc["tops"].AsBinary);
        }

        public void WriteTops(int x, int y, byte[] tops)
        {
            BsonValue id = GetIDFor(x, y, 0);
            BsonDocument newdoc = new BsonDocument();
            Dictionary<string, BsonValue> tbs = newdoc.RawValue;
            tbs["_id"] = id;
            tbs["tops"] = new BsonValue(FileHandler.Compress(tops));
            DBTops.Upsert(newdoc);
        }

        public byte[] GetTops(int x, int y)
        {
            BsonDocument doc;
            doc = DBTops.FindById(GetIDFor(x, y, 0));
            if (doc == null)
            {
                return null;
            }
            return FileHandler.Uncompress(doc["tops"].AsBinary);
        }

        /// <summary>
        /// TODO: Probably clear this occasionally!
        /// </summary>
        public ConcurrentDictionary<Vector2i, int> Mins = new ConcurrentDictionary<Vector2i, int>();
        
        public int GetMins(int x, int y)
        {
            Vector2i input = new Vector2i(x, y);
            int output;
            if (Mins.TryGetValue(input, out output))
            {
                return output;
            }
            BsonDocument doc;
            doc = DBMins.FindById(GetIDFor(x, y, 0));
            if (doc == null)
            {
                return 0;
            }
            return doc["min"].AsInt32;
        }

        public void SetMins(int x, int y, int min)
        {
            BsonValue id = GetIDFor(x, y, 0);
            BsonDocument newdoc = new BsonDocument();
            Dictionary<string, BsonValue> tbs = newdoc.RawValue;
            tbs["_id"] = id;
            tbs["min"] = new BsonValue(min);
            Mins[new Vector2i(x, y)] = min;
            DBMins.Upsert(newdoc);
        }

    }
}
