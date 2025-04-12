using System.Collections.Generic;

// This files defines connections between exitWaypoints 
// WARNING! The same namber of exitWaypoints and the SAME EXACT CONNECTIONS MUST BE USED ALSO FOR OTHER ENEMIES WICH RESIDES
// IN THE SAME ROOM!
// Within the same scene a merge between all exitWaypoints of all enemies may be needed
// syntax: { 0, new List<int> { 1, 3 } } means waypoint 0 connected to 1 and 3

// TODO: at the moment a new script GoonAI must be created for each enemy GoonAI of different rooms to redefine this structure.
// Of course I need to serialize this in a common file and take them dinamically through a serialize field integer which 
// indicates the specific map to get for example. 

// indexes correspond to indexes of Vector2[] instances
namespace Utils
{
    public class Level1
    {
        public class Scene1
        {
            public static Dictionary<int, List<int>> GOON_1_CONNECTIONS = new Dictionary<int, List<int>>
            {
                { 0, new List<int> { 1 } },
                { 1, new List<int> { 9, 10, 0 } },
                { 2, new List<int> { 7, 10 } },
                { 3, new List<int> { 6, 7 } },
                { 4, new List<int> { 6, 8 } },
                { 5, new List<int> { 8, 9 } },
                { 6, new List<int> { 3, 4 } },
                { 7, new List<int> { 2, 3 } },
                { 8, new List<int> { 4, 5 } },
                { 9, new List<int> { 5, 1 } },
                { 10, new List<int> { 1, 2 } },
            };
        }
    }
}