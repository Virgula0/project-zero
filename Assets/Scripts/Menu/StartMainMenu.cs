using Assets.Scripts.Common;
using UnityEngine;

namespace Assets.Scripts.Menu
{
    public class StartMainMenu : MonoBehaviour
    {

        private Repository rep;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            if (rep == null)
            {
                rep = new Repository();
            }
        }
    }
}