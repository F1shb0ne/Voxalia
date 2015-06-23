﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.ServerGame.ServerMainSystem;
using Voxalia.ServerGame.WorldSystem;

namespace Voxalia.ServerGame.EntitySystem
{
    public abstract class EntityLiving: PhysicsEntity, EntityDamageable
    {
        public EntityLiving(World tworld, bool ticks, float maxhealth)
            : base(tworld, ticks)
        {
            MaxHealth = maxhealth;
            Health = maxhealth;
        }

        public float Health = 100;

        public float MaxHealth = 100;

        public virtual float GetHealth()
        {
            return Health;
        }

        public virtual float GetMaxHealth()
        {
            return MaxHealth;
        }

        public virtual void SetHealth(float health)
        {
            Health = health;
            if (MaxHealth != 0 && Health <= 0)
            {
                Die();
            }
        }

        public virtual void Damage(float amount)
        {
            SetHealth(GetHealth() - amount);
        }

        public virtual void SetMaxHealth(float maxhealth)
        {
            MaxHealth = maxhealth;
        }

        public abstract void Die();
    }
}
