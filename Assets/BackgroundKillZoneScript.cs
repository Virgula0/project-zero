using UnityEngine;

public class BackgroundKillZoneScript : MonoBehaviour
{
    private void OnTriggerStay2D(Collider2D collision)
    {
        LogicManager sup = GameObject.FindGameObjectWithTag(Utils.Const.LOGIC_MANAGER_TAG).GetComponent<LogicManager>();
        sup.GameOverAmbient();
    }
}
