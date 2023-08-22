using UnityEngine;
using UnityEngine.UI;

// The minecraft hotbar / toolbar
public class Toolbar : MonoBehaviour
{
    World world;
    public Player player;

    public RectTransform highlight; // the highlight
    public ItemSlot[] itemSlots; // the itemslot array

    int slotIndex = 0;

    private void Start()
    {
        // getting the world object and generate the items within the tool bar
        world = GameObject.Find("World").GetComponent<World>();

        foreach (ItemSlot slot in itemSlots)
        {
            slot.icon.sprite = world.blocktypes[slot.itemID].icon;
            slot.icon.enabled = true;
        }
        player.selectedBlockIndex = itemSlots[slotIndex].itemID;
    }

    private void Update()
    {
        // getting the input of the mouse scrollwheel and move the highlight accordingly
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll != 0)
        {
            if (scroll > 0)
                slotIndex--;
            else
                slotIndex++;

            if (slotIndex > itemSlots.Length - 1)
                slotIndex = 0;
            if (slotIndex < 0)
                slotIndex = itemSlots.Length - 1;

            highlight.position = itemSlots[slotIndex].icon.transform.position; // change the highlight position
            player.selectedBlockIndex = itemSlots[slotIndex].itemID; // change which block the player can place
        }
    }
}

// a class for the item slot, this class will hold the id of the block and the sprite of the block
[System.Serializable]
public class ItemSlot
{
    public byte itemID;
    public Image icon;
}