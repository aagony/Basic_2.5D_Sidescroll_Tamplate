using UnityEngine;

public readonly struct MonsterDeadEvent
{
    public readonly GameObject MonsterObject;

    public MonsterDeadEvent(GameObject monsterObject)
    {
        MonsterObject = monsterObject;
    }
}
