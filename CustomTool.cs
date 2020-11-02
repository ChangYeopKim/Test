using UnityEngine;

[System.Serializable]
public class CustomTool
{
    public Sprite img;
    public string name;

    // 코스튬 가격
    public int price;

    // 장착 여부
    // UNLOCK = 0, UNEQUIP = 1, EQUIP = 2
    public int state_dir;
}
