using UnityEngine;
using System.Text;

namespace Utils {
    public class Enums {

         
        public enum MouseButtons {
            LeftButton = 0,
            RightButton = 1,
        }

        public enum ObjectLayers {
            Player = 3,
            Wall = 6,
            Enemy = 7,
        }
    }

    public class Const {   
        public const string PLAYER_TAG = "PlayerTag";
        public const string WEAPON_MANAGER_TAG = "WeaponManager";         
        public const string GLOBAL_WAYPOINTS_TAG = "GlobalWaypoints";   
    }

    public class Functions{
        public static string Vector2ArrayToString(Vector2[] vectors)
        {

            if (vectors == null) {
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

