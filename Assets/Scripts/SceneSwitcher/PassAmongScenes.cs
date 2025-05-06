using UnityEngine;

public class PassAmongScenes : MonoBehaviour
{
    private SwitchScene switcher;
    private LogicManager logic;

    void Start()
    {
        if (GameObject.FindGameObjectWithTag(Utils.Const.SCENE_SWITCHER_TAG) is GameObject scene && scene != null){
            switcher = scene.GetComponent<SwitchScene>();
        }
        logic = GameObject.FindGameObjectWithTag(Utils.Const.LOGIC_MANAGER_TAG).GetComponent<LogicManager>();
    }

    public void OnTriggerEnter2D(Collider2D collision){
        if (collision.gameObject.layer == (int)Utils.Enums.ObjectLayers.Player
            && logic.IsTheRoomClear()){ 
            switcher.SaveAndGoNext();
        }
    }
}
