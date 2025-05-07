using Assets.Scripts.Common;
using UnityEngine;

namespace Assets.Scripts.Menu
{
    public class StartMainMenu : MonoBehaviour
    {

        private Repository rep;
        public static StartMainMenu Instance;
        private float startPlayTime;
        private bool deleteTableFirst = false;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            
            if (rep == null)
            {
                rep = new Repository(deleteTableFirst);
            }
        }

        public float GetStartPlayTime()
        {
            return startPlayTime;
        }

        public void BeginPlayTime()
        {
            this.startPlayTime = Time.time;
        }

        public Repository GetRepository(){
            return rep;
        }

        public CursorChanger GetCursorChangerScript()
        {
            return Instance.gameObject.GetComponent<CursorChanger>();
        }
    }
}