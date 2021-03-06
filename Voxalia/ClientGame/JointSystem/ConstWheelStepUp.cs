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
using Voxalia.ClientGame.EntitySystem;
using BEPUphysics.Constraints;
using Voxalia.Shared.Collision;

namespace Voxalia.ClientGame.JointSystem
{
    public class ConstWheelStepUp : BaseJoint
    {
        public float Height;

        public ConstWheelStepUp(PhysicsEntity ent, float height)
        {
            One = ent;
            Two = ent;
            Height = height;
        }

        public override SolverUpdateable GetBaseJoint()
        {
            return new WheelStepUpConstraint(Ent1.Body, Ent1.TheRegion.Collision, Height);
        }
    }
}
