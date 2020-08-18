// Instantiates 10 copies of Prefab each 2 units apart from each other

using System.Collections;
using UnityEngine;

public class SummonNPCWithItems : MonoBehaviour
{

    public GameObject NPC;
    public GameObject HandItem;
    public GameObject BackItem;
    public GameObject BeltItem;


    //TODO make have a bool for "on awake" vs "when player in range"
    private Vector3 SummonPositon;

    void Start()
    {
        SummonPositon = this.transform.position;

        Summon();
    }

    public void Update()
    {
    }

    public void Summon()
    {
        // make npc
        GameObject SpawnedNPC = Instantiate(NPC, SummonPositon, Quaternion.identity);
        CharacterController SpawnedNPCController = SpawnedNPC.GetComponent<CharacterController>();

        Transform HandTransform = SpawnedNPCController.GetHandTransform();
        Transform BackTransform = SpawnedNPCController.GetBackTransform();
        Transform BeltTransform = SpawnedNPCController.GetBeltTransform();

        // if each item exsits, summon it and give to npc
        if (HandItem != null)
        {
            GameObject SpawnedHandItem = Instantiate(HandItem, SummonPositon, Quaternion.identity);
            SpawnedHandItem.GetComponent<ItemController>().SetHeldLocation("Hand", SpawnedNPCController);
        }
        if (BackItem != null)
        {
            GameObject SpawnedBackItem = Instantiate(BackItem, SummonPositon, Quaternion.identity);
            SpawnedBackItem.GetComponent<ItemController>().SetHeldLocation("Back", SpawnedNPCController);
        }
        if (BeltItem != null)
        {
            GameObject SpawnedBeltItem = Instantiate(BeltItem, SummonPositon, Quaternion.identity);
            SpawnedBeltItem.GetComponent<ItemController>().SetHeldLocation("Belt", SpawnedNPCController);
        }

    }
}