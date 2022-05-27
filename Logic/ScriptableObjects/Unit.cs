using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Unit", menuName = "Unit", order = 1)]
public class Unit : ScriptableObject
{
    [Header("—сылки на файлы юнита")]
    [SerializeField] Object unit_file;
    [SerializeField] string unit_ui_type = "DefaultUI";
    [Header("јнимации юнита")]
    [SerializeField] string[] idle_animations;
    [SerializeField] string[] attack_animations;
    [SerializeField] string[] hurt_animations;

    public GameObject InstantiateObject()
    {
        return GameObject.Instantiate(unit_file) as GameObject;
    }
    public GameObject InstantiateUIObject()
    {
        GameObject g = GameObject.Instantiate(Resources.Load(@"UI\" + unit_ui_type)) as GameObject;
        g.transform.SetParent(GameObject.FindObjectOfType<Canvas>().transform);
        return g;
    }
    public string[] A_Idle
    {
        get
        {
            return idle_animations;
        }
    }
    public string[] A_Attack
    {
        get
        {
            return attack_animations;
        }
    }
    public string[] A_Hurt
    {
        get
        {
            return hurt_animations;
        }
    }
}
