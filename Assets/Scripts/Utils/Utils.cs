using UnityEngine;
using System.Text;
using System.Collections.Generic;
using System.Linq;

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
        }
    }

    public class Const
    {
        public const string PLAYER_TAG = "PlayerTag";
        public const string WEAPON_MANAGER_TAG = "WeaponManager";
        public const string GLOBAL_WAYPOINTS_TAG = "GlobalWaypoints";
    }

    public class Functions
    {

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

        // This function defines connections between Vector2 points. 
        /// The connection pattern is:
        /// - Index 0: connected to index 1.
        /// - Index 1: connected to indices 0, 2, and the last index (n - 1).
        /// - For other elements (indexes 2 to n - 2): connected to the previous and next indexes.
        /// - The last element (index n - 1): connected to the previous index (n - 2) and index 1.
        // indexes correspond to indexes of Vector2[] instances
        public static Dictionary<int, List<int>> GenerateConnections(Vector2[] points)
        {
            Dictionary<int, List<int>> connections = new Dictionary<int, List<int>>();
            int n = points.Length;

            if (n == 0)
            {
                return connections;
            }
            else if (n == 1)
            {
                // For a single element, we could return an empty connections list.
                connections.Add(0, new List<int>());
                return connections;
            }

            for (int i = 0; i < n; i++)
            {
                List<int> neighbors = new List<int>();

                if (i == 0)
                {
                    // Element 0 is connected to element 1.
                    neighbors.Add(1);
                }
                else if (i == 1)
                {
                    // Element 1 is connected to 0, (if exists, 2), and the last index.
                    neighbors.Add(0);
                    if (n > 2)
                    {
                        neighbors.Add(2);
                    }
                    // Always add the last index for element 1.
                    neighbors.Add(n - 1);
                }
                else if (i == n - 1)
                {
                    // Last element is connected to its predecessor and index 1.
                    neighbors.Add(n - 2);
                    // Avoid duplicate if n - 1 is 1.
                    if (1 != n - 2)
                    {
                        neighbors.Add(1);
                    }
                }
                else
                {
                    // All other elements (indexes 2 to n - 2) are connected to the previous and next elements.
                    neighbors.Add(i - 1);
                    neighbors.Add(i + 1);
                }

                connections.Add(i, neighbors);
            }

            return connections;
        }
    }
}

