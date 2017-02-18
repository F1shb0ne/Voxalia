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
using FreneticScript.TagHandlers;
using FreneticScript.TagHandlers.Objects;
using Voxalia.ServerGame.TagSystem.TagObjects;
using Voxalia.ServerGame.ServerMainSystem;
using Voxalia.ServerGame.EntitySystem;
using Voxalia.ServerGame.WorldSystem;
using FreneticScript;
using Voxalia.Shared;

namespace Voxalia.ServerGame.TagSystem.TagBases
{
    class PrimitiveEntityTagBase : TemplateTagBase
    {
        // <--[tagbase]
        // @Base primitive_entity[<PrimitiveEntityTag>]
        // @Group Entities
        // @ReturnType PrimitiveEntityTag
        // @Returns the primitive entity with the given entity ID.
        // -->
        Server TheServer;

        public PrimitiveEntityTagBase(Server tserver)
        {
            Name = "primitive_entity";
            TheServer = tserver;
        }

        public override TemplateObject Handle(TagData data)
        {
            long eid;
            string input = data.GetModifier(0).ToLowerFast();
            if (long.TryParse(input, out eid))
            {
                Entity e = TheServer.GetEntity(eid);
                if (e != null && e is PrimitiveEntity)
                {
                    return new PrimitiveEntityTag((PrimitiveEntity)e).Handle(data.Shrink());
                }
            }
            data.Error("Invalid primitive entity '" + TagParser.Escape(input) + "'!");
            return new NullTag();
        }
    }
}
