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
using BEPUphysics.Constraints;
using Voxalia.Shared.Collision;

namespace Voxalia.ServerGame.JointSystem
{
    public class JointFlyingDisc : BaseJoint
    {
        public JointFlyingDisc(Entity e)
        {
            One = e;
            Two = e;
        }

        public bool IsAPlane = false;

        public override SolverUpdateable GetBaseJoint()
        {
            return new FlyingDiscConstraint(Ent1.Body) { IsAPlane = IsAPlane };
        }
    }
}
