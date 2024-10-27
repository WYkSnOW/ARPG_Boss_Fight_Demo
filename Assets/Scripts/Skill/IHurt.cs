
using UnityEngine;

public interface IHurt
{
    bool Hurt(Skill_HitData hitData, ISkillOwner hurtSource);
}
