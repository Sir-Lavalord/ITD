using ReLogic.Peripherals.RGB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace ITD.Systems.Recruitment
{
    public abstract class RecruitBehavior
    {
        public abstract int NPCType { get; }
        public int attackTime { get { return _attackTime; } set { _attackDuration = value; _attackTime = value; } }
        private int _attackTime;
        private int _attackDuration;
        private float _attackProgress;
        public bool attacking = false;
        public float AITimer = 0f;
        public virtual void SetStaticDefaults()
        {
            attackTime = 16;
        }
        public abstract void Run(NPC npc, Player player);
        public abstract void Attack(NPC npc, NPC other);
        public virtual void UpdateAttack(NPC npc)
        {
            if (!attacking)
                return;
            _attackProgress = (_attackDuration - _attackTime) / (float)_attackDuration;
            _attackTime--;
            if (_attackProgress >= 1)
            {
                _attackProgress = 0;
                _attackTime = _attackDuration;
                attacking = false;
                return;
            }
        }
        public virtual void FindFrame(NPC npc, int frameHeight)
        {
            if (!attacking)
                return;
            int attackAnimation = 21 + (int)(_attackProgress * 4f);
            npc.frame.Y = attackAnimation * frameHeight;
        }
    }
}
