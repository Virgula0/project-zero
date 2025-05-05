using UnityEngine;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Utils
{
    public class Enums
    {


        public enum MouseButtons
        {
            LeftButton = 0,
            RightButton = 1,
        }

        public enum ObjectLayers
        {
            Player = 3,
            Wall = 6,
            Enemy = 7,
            BulletByPlayer = 8,
            SwingByPlayer = 9,
            ParriableLayer = 10,
            BodyOnTheGround = 11,
            KillZone = 12
        }
    }

    public class Animations
    {
        public const string PLAYER_SWORD_ATTACK = "PlayerSwordAttackAnimation";
        public const string ENEMY_SWORD_ATTACK = "EnemySwordAttackAnimation";
        public const string TELEPORT_ANIMATION = "PlayerTeleportInAnimation";

        public class Triggers
        {
            public const string TELEPORTING = "teleporting";
        }
    }


    public class Const
    {
        public const string HORIZONTAL = "Horizontal";
        public const string VERTICAL = "Vertical";
        public const string WALKING_ANIM_VAR = "isWalking";
        public const string PLAYER_TAG = "PlayerTag";
        public const string GOON_TAG = "GoonEnemyTag";
        public const string WEAPON_MANAGER_TAG = "WeaponManager";
        public const string GLOBAL_WAYPOINTS_TAG = "GlobalWaypoints";
        public const string GUN_ON_THE_GROUND_TAG = "GunOnTheGround";
        public const string WEAPON_SPAWNER_TAG = "WeaponSpawner";
        public const string CURSOR_CHANGER_TAG = "CursorChanger";
        public const string LOGIC_MANAGER_TAG = "LogicManager";
        public const string UI_MANAGER_TAG = "UIManager";
        public const string SCENE_SWITCHER_TAG = "SceneSwitcher";
    }

    public class Functions
    {
        public static void SetLayerRecursively(GameObject obj, int newLayer)
        {
            obj.layer = newLayer;
            foreach (Transform child in obj.transform)
            {
                SetLayerRecursively(child.gameObject, newLayer);
            }
        }

        public static void PrintDictionary(Dictionary<int, List<int>> dict)
        {
            Debug.Log(string.Join(", ", dict.Select(kv => $"{kv.Key}: [{string.Join(", ", kv.Value)}]")));
        }

        public static Vector2[] AddToVector2Array(Vector2[] array, Vector2 newElement, out int addedIndex)
        {
            Vector2[] newArray = new Vector2[array.Length + 1];
            for (int i = 0; i < array.Length; i++)
            {
                newArray[i] = array[i];
            }
            newArray[array.Length] = newElement;
            addedIndex = array.Length;
            return newArray;
        }
        /// Returns a new array containing only those points in <paramref name="source"/>
        /// that are not exactly equal to any point in <paramref name="toRemove"/>.
        public static Vector2[] RemoveAll(Vector2[] source, Vector2[] toRemove)
        {
            // build a HashSet for O(1) lookups
            var removeSet = new HashSet<Vector2>(toRemove);
            return source
                .Where(v => !removeSet.Contains(v))
                .ToArray();
        }

        public static Vector2[] RemoveAtIndex(Vector2[] source, int index)
        {
            // Defensive check
            if (index < 0 || index >= source.Length)
                throw new System.ArgumentOutOfRangeException(nameof(index));

            // Convert to List, remove, convert back
            var list = source.ToList();       // O(n)
            list.RemoveAt(index);             // O(n) shift
            return list.ToArray();            // O(n)
        }

        public static string Vector2ArrayToString(Vector2[] vectors)
        {

            if (vectors == null)
            {
                return "";
            }

            StringBuilder sb = new StringBuilder();
            foreach (Vector2 vector in vectors)
            {
                // Append each vector in the format (x,y)
                sb.AppendFormat("({0},{1}) ", vector.x, vector.y);
            }
            // Remove trailing space and return the string.
            return sb.ToString().TrimEnd();
        }
    }
}

