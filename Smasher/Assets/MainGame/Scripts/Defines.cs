using UnityEngine;
using System.Collections.Generic;

namespace Defines
{
    static class Config
    {
        public static float BODY_DISTANCE = 0.75f;
    }

    static class Tag
    {
        public static string PLAYER = "Player";
        public static string ENEMY = "Enemy";
        public static string COLLECTIBLE = "Collectible";
    }

    public class QuestInfo
    {
        public QuestType Type;
        public HeadStyle Head;
        public BodyStyle Body;
        public string Description;
        public int Count;
    }

    public enum HeadStyle
    {
        CLAW01,
        CLAW02,
        CLAW03,
        CLAW04,
        GUN
    }

    public enum BodyStyle
    {
        NORMAL,
        GUN,
        GUN_ELECTRIC,
        FOOD,
    }

    public enum CollectibleType
    {
        HEAD,
        BODY,
        FOOD
    }

    public enum CollisionType
    {
        BODY = 8,
        CLAW = 9,
        BULLET = 10,
        SKELETON = 11,
        COLLECTIBLE = 12,
        CHOCOLATE = 13,
        ZONE = 14,
    }

    public enum QuestType
    {
        // COLLECT_HEAD,
        // COLLECT_BODY,
        // DESTROY_HEAD,
        DESTROY_BODY,
    }

    public enum EmojiType
    {
        ANGRY,
        TEETH,
        COOL,
        CRY,
        DISAPPOINTED,
        DROOL,
        KISS,
        LAUNCH_CRY,
        POOP,
        PUKE,
        EVIL_LAUNCH,
        SCARE,
    }

    public enum BulletType
    {
        NORMAL,
        ELECTRIC
    }
}
