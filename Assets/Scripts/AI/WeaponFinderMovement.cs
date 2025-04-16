using UnityEngine;

public class WeaponFinderMovement : MonoBehaviour, IMovement
{
    private WeaponSpawner spawner;

    public IMovement New(){
        this.spawner = GameObject.FindGameObjectWithTag(Utils.Const.WEAPON_SPAWNER).GetComponent<WeaponSpawner>();
        return this;
    }

    public void CustomSetter<T>(T varToSet)
    {
        return;
    }

    public void Move(Rigidbody2D enemyTransform)
    {
        throw new System.NotImplementedException();
    }
}