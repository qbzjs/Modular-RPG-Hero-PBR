using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemController : MonoBehaviour
{
    private Rigidbody rb;

    private Transform ItemTransform;

    private Collider ItemCollider;

    private ItemData Item = new ItemData();

    private ItemDataManager IDM = new ItemDataManager();

    public string ItemSaveFileFolder = "Assets/ItemJson";

    public string ItemSaveFile = "Torch.json";

    // label stuff
    //public GameObject SpeechBubblePreFab;
    //public Collider PlayerInRangeForBubbleCollider;
    public bool isPickedUp = false;

    private float CooldownTimer = 0f;

    private float AnimationCooldownTimer = 0f;

    private float CurrentItemAction = 0.0f;

    public float CanDoAction = 0.0f; // which action it can do currently

    // 0 for none
    // 1 for primary
    // 2 for secondary
    // 3 for TODO
    public CharacterController ActionTargetCharacterController = null;

    Vector3 StandardHoldingPos = new Vector3(0.0f, 0.0f, 0.0f);

    float StandardHoldingRot = 0.0f;

    private GameObject HoldingCharacter;

    private bool CanBeDropped = true; // check that item can be dropped, for built tool use

    //When character comes online, set vars needed for init
    private void Awake()
    {
        // TODO have this copy the configs per unqiue torch item etc
        //cam = Camera.main;
        rb = gameObject.GetComponent<Rigidbody>();
        ItemTransform = gameObject.GetComponent<Transform>();
        ItemCollider = gameObject.GetComponent<Collider>();

        Debug.Log("Starting an item");
        IDM.Init (ItemSaveFileFolder, ItemSaveFile);
        Load();
        //Character = CDM.Load();
        //TODO load on init
    }

    private void FixedUpdate()
    {
        //happens on physics updates
        //saving etc
        //if (Input.GetKeyDown("i"))
        //{
        //    Load();
        //}
        //if (Input.GetKeyDown("o"))
        //{
        //    Save();
        //}
        // for debug
        //if (Input.GetKeyDown("k"))
        //{
        //    if (isPickedUp && Item.heldLocation == "Hand")
        //    {
        //        string summary = GetSummaryString();
        //        ActionTargetCharacterController
        //            .MakeSpeechBubble("Item is\n" + summary);
        //    }
        //}
    }

    private void Load()
    {
        Item = IDM.Load();

        Debug.Log("loading item" + Item.Name);
        /*
        // if picked up, go to holder, if not, go to last postion
        if (Item.holderUUID != "")
        {
            var characterControllersList = FindObjectsOfType<CharacterController>();
            string id;
            foreach (CharacterController controller in characterControllersList)
            {
                id = controller.GetUUID();
                if (id == Item.holderUUID)
                {

                    isPickedUp = true;
                    rb.useGravity = false;
                    rb.isKinematic = true;

                    rb.constraints = RigidbodyConstraints.None;

                    //TODO move relivant to character
                    Physics.IgnoreCollision(controller.gameObject.GetComponent<Collider>(), GetComponent<Collider>());

                    //ItemTransform.parent = collision.gameObject.GetComponent<CharacterController> ().GetCharacterTransform ();
                    HoldingCharacter = controller.gameObject;


                    //based on held location, change location
                    if (Item.heldLocation == "Hand")
                    {
                        ItemTransform.parent = HoldingCharacter.GetComponent<CharacterController>().GetHandTransform();
                    }
                    else if (Item.heldLocation == "Back")
                    {
                        ItemTransform.parent = HoldingCharacter.GetComponent<CharacterController>().GetBackTransform();
                    }
                    else if (Item.heldLocation == "Belt")
                    {
                        ItemTransform.parent = HoldingCharacter.GetComponent<CharacterController>().GetBeltTransform();
                    }

                    ItemTransform.localPosition = new Vector3(0, 0, 0);

                    break;
                }
            }
        }
        else
        {
            ItemTransform.position = new Vector3(Item.x_pos, Item.y_pos, Item.z_pos);
        }
        */
    }

    private void Save()
    {
        if (!isPickedUp)
        {
            Item.x_pos = ItemTransform.position.x;
            Item.y_pos = ItemTransform.position.y;
            Item.z_pos = ItemTransform.position.z;
        }
        IDM.Save (Item);
    }

    //TODO make update on frameupdate
    private void Update()
    {
        // ask parent if dropped
        if (HoldingCharacter != null)
        {
            if (AnimationCooldownTimer >= 0.0f)
            {
                AnimationCooldownTimer -= Time.deltaTime;

                if (AnimationCooldownTimer <= 0.0f)
                {
                    //Debug.Log("setting rotations back");
                    //Vector3 StandardHoldingPos = GetCurrentHoldingOffsetXYZ();
                    SetHoldingOffsetXYZ (StandardHoldingPos);
                    SetHoldingRotationOffset (StandardHoldingRot);
                }
            }

            if (ActionTargetCharacterController == null)
            {
                SetActionTargetCharacterController(HoldingCharacter
                    .GetComponent<CharacterController>());
            }

            string Status = ActionTargetCharacterController.GetItemStatus();

            //string Status = HoldingCharacter.GetComponent<CharacterController>().GetItemStatus();
            CurrentItemAction =
                ActionTargetCharacterController.GetItemActionFloat();

            //CurrentItemAction = HoldingCharacter.GetComponent<CharacterController>().GetItemActionFloat();
            if (
                (
                (Status == "Dropping" && Item.heldLocation == "Hand") ||
                CurrentItemAction == -1.0f
                ) &&
                CanBeDropped
            )
            {
                Debug.Log("parent id dropping me");
                EnableCollsion();

                Transform HoldingCharacterTransform =
                    HoldingCharacter.GetComponent<Transform>();

                Vector3 dropPoint =
                    HoldingCharacterTransform.position +
                    (HoldingCharacterTransform.forward * 1.5f) +
                    (HoldingCharacterTransform.up * 1.5f);

                //ItemTransform.localPosition = new Vector3 (Random.Range (-0.25f, 0.25f), Random.Range (0f, 0.5f), Random.Range (-0.25f, 0.25f));
                //ItemTransform.localPosition = ItemTransform.forward* new Vector3 (Random.Range (-0.25f, 0.25f), Random.Range (0f, 0.5f), Random.Range (-0.25f, 0.25f));
                //ItemTransform.position = ItemTransform.position + (ItemTransform.forward * 1.5f);
                ItemTransform.position = dropPoint;

                ItemTransform.parent = null;

                Physics
                    .IgnoreCollision(HoldingCharacter.GetComponent<Collider>(),
                    GetComponent<Collider>(),
                    false);

                Item.heldLocation = null;
                Item.holderUUID = null;

                HoldingCharacter = null;
                isPickedUp = false;
                rb.useGravity = true;
                rb.isKinematic = false;
                Status = "";

                if (CurrentItemAction == -1)
                {
                    Item.ownerUUID = "";
                }

                // get the lable text and enable text
                NearbyText label = this.gameObject.GetComponent<NearbyText>();
                if (label != null)
                {
                    label.EnableText();
                }
            }
            else if (Status == "SwapHandBack")
            {
                if (Item.heldLocation == "Hand")
                {
                    ItemTransform.parent =
                        ActionTargetCharacterController.GetBackTransform();

                    //ItemTransform.parent = HoldingCharacter.GetComponent<CharacterController>().GetBackTransform();
                    Item.heldLocation = "Back";
                }
                else if (Item.heldLocation == "Back")
                {
                    ItemTransform.parent =
                        ActionTargetCharacterController.GetHandTransform();

                    //ItemTransform.parent = HoldingCharacter.GetComponent<CharacterController>().GetHandTransform();
                    Item.heldLocation = "Hand";
                }

                Status = "";
            }
            else if (Status == "SwapHandBelt")
            {
                if (Item.heldLocation == "Hand")
                {
                    ItemTransform.parent =
                        ActionTargetCharacterController.GetBeltTransform();

                    //ItemTransform.parent = HoldingCharacter.GetComponent<CharacterController>().GetBeltTransform();
                    Item.heldLocation = "Belt";
                }
                else if (Item.heldLocation == "Belt")
                {
                    ItemTransform.parent =
                        ActionTargetCharacterController.GetHandTransform();

                    //ItemTransform.parent = HoldingCharacter.GetComponent<CharacterController>().GetHandTransform();
                    Item.heldLocation = "Hand";
                }

                Status = "";
            }
            else
            {
                //Keep the item above the player / at location
                // temp toggle if item needs to be moved diferently or not
                if (AnimationCooldownTimer <= 0.0f)
                {
                    ItemTransform.localRotation = Quaternion.identity;

                    if (Item.heldLocation == "Hand")
                    {
                        //ItemTransform.localPosition = new Vector3(Item.HoldingOffsetX, Item.HoldingOffsetY, Item.HoldingOffsetZ);
                        SetHoldingOffsetXYZ(new Vector3(Item.HoldingOffsetX,
                            Item.HoldingOffsetY,
                            Item.HoldingOffsetZ));
                        SetHoldingRotationOffset(Item.HoldingRotationOffset);
                    }
                    else
                    {
                        SetHoldingOffsetXYZ(new Vector3(0, 0, 0));
                        SetHoldingRotationOffset(0.0f);
                    }

                    //ItemTransform.localPosition = new Vector3(0, 1, 0);
                    //cooldown timer if needed
                    if (CooldownTimer > 0)
                    {
                        CooldownTimer -= Time.deltaTime;
                    }
                }

                if (Item.heldLocation == "Hand")
                {
                    if (CurrentItemAction > 0.0f)
                    {
                        // enable collision
                        EnableCollsion();
                    }
                    else
                    {
                        //disable collision
                        DisableCollsion();
                    }

                    //mouse click inputs
                    if (CurrentItemAction == 1.0f)
                    {
                        DoPrimaryAction();

                        //HoldingCharacter.GetComponent<CharacterController>().ResetItemActionFloat();
                        ActionTargetCharacterController.ResetItemActionFloat();
                    }
                    if (CurrentItemAction == 2.0f)
                    {
                        DoSecondaryAction();

                        //HoldingCharacter.GetComponent<CharacterController>().ResetItemActionFloat();
                        ActionTargetCharacterController.ResetItemActionFloat();
                    }
                    if (CurrentItemAction == 3.0f)
                    {
                        //TODO use action
                        DoSecondaryAction();

                        //HoldingCharacter.GetComponent<CharacterController>().ResetItemActionFloat();
                        ActionTargetCharacterController.ResetItemActionFloat();
                    }
                }
                //TODO else if heldlocation is belt, then do primary action on keypress etc

                //TODO if has status effect if held/on belt, put here
            }
        }
    }

    //have these do the item efect and trigger the holders animation
    private void DoPrimaryAction()
    {
        // do item with cooldown
        if (CooldownTimer <= 0.0f)
        {
            string ItemClass = Item.PrimaryActionClass;
            if (ItemClass == "SUMMON")
            {
                // doesnt need charactercorller target
                SetCanDoAction(1.0f);
                CooldownTimer += Item.Cooldown;

                // TODO this later like stamina potion
                /*

                // TODO move this to a per item thing that checks for current action
                Debug.Log("SUMMONIGNGGG");
                SummonPrefab Summoner = this.gameObject.GetComponent<SummonPrefab>();
                Debug.Log(Summoner);
                Summoner.Summon();
                */
            } //TODO else if basic etc
            else if (ItemClass == "POTION")
            {
                SetCanDoAction(1.0f);

                //SetActionTargetCharacterController(HoldingCharacter.GetComponent<CharacterController>());
                CooldownTimer += Item.Cooldown;
            }
            else if (ItemClass == "BASIC")
            {
                // do basic attack hit, if it hits then canDoAction is true
                float animationDuration = 1.0f;

                //AnimateHoldingCharacter("m_slash1", animationDuration);
                CooldownTimer += Item.Cooldown;

                // DO attack
                DoAttack();
            }
            else if (ItemClass == "SPEAR")
            {
                // do basic attack hit, if it hits then canDoAction is true
                float animationDuration = 1.0f;

                AnimationCooldownTimer = animationDuration;

                // TODO find new spear animation etc
                //Debug.Log("doing action as spears");
                StandardHoldingPos = GetCurrentHoldingOffsetXYZ();
                StandardHoldingRot = GetHoldingRotationOffset();

                SetHoldingRotationOffset(50.0f);
                SetHoldingOffsetXYZ(new Vector3(0.0f, 0.3f, 0.0f));

                // TODO held format must change
                AnimateHoldingCharacter("m_spear_stab2", animationDuration);
                CooldownTimer += Item.Cooldown;

                // DO attack
                DoAttack();
            }
            else if (ItemClass == "SPELL")
            {
                if (ActionTargetCharacterController.GetCurrentMana() > 0.0f)
                {
                    //SetActionTargetCharacterController(HoldingCharacter.GetComponent<CharacterController>());
                    //SetCanDoAction(1.0f);
                    CooldownTimer += Item.Cooldown;

                    float animationDuration = 2.4f;
                    AnimateHoldingCharacter("m_two_handed_cast2",
                    animationDuration);
                    ActionTargetCharacterController
                        .AddValueToMana(-1.0f * (Item.Damage / 2.0f));

                    // spells cost dmg/2 of mana base, and per spell can have added costs

                    // do nothing if class si none
                }
            }
            else if (ItemClass == "NONE")
            {
                // do nothing if class si none
            }
            else
            {
                // TODO rm this part
                // TODO move to attac secion maybe?
                //Debug.Log("doing basic attack 01");
                float animationDuration = 1.0f;
                AnimateHoldingCharacter("m_slash1", animationDuration);
            }
        }
    }

    private void DoSecondaryAction()
    {
        // TODO copy exact from primary but 1.0f to 2.0f
        // TODO base this on item json
        Debug.Log("Pressed Seconary button. TODO THIS");
        Debug.Log("doing basic attack 02");
        float animationDuration = 1.0f;
        AnimateHoldingCharacter("m_slash2", animationDuration);
    }

    // set canDoAction
    public void SetCanDoAction(float newStatus)
    {
        CanDoAction = newStatus;
    }

    public void SetActionTargetCharacterController(
        CharacterController newController
    )
    {
        ActionTargetCharacterController = newController;
    }

    public float GetDamage()
    {
        return Item.Damage;
    }

    private void AnimateHoldingCharacter(
        string animation,
        float overrideDuration
    )
    {
        //CharacterController controller = HoldingCharacter.GetComponent<CharacterController>();
        //controller.SetAnimation(animation, overrideDuration);
        ActionTargetCharacterController.SetAnimation (
            animation,
            overrideDuration
        );
    }

    private void SetTargetOnImpact(
        GameObject WhosTargetToSet,
        GameObject TargetToSet
    )
    {
        CharacterController controller =
            WhosTargetToSet.GetComponent<CharacterController>();
        controller.SetTarget (TargetToSet);
        controller.SetFighting(true);
    }

    private void EnableCollsion()
    {
        ItemCollider.enabled = true;
    }

    private void DisableCollsion()
    {
        ItemCollider.enabled = false;
    }

    private void DoAttack()
    {
        /*
        make points of 1m diamtter spheres in front of character
        if sphere hits target or new to target, then do hit

        */
        CharacterController HitCharacterController = null;
        CharacterController HoldingCharacterController =
            HoldingCharacter.GetComponent<CharacterController>();
        float i = 0.5f;
        Transform HoldingCharacterTransform =
            HoldingCharacter.GetComponent<Transform>();
        Vector3 center;
        bool hit = false;

        while (i < Item.Range)
        {
            //summon overlap sphere i meters out with diameter of 1 (radius of .5)
            // check hit target
            // if hit target, break
            // get overlap
            center =
                HoldingCharacterTransform.position +
                (HoldingCharacterTransform.forward * i);
            Collider[] hitColliders = Physics.OverlapSphere(center, 0.5f);
            int j = 0;
            while (j < hitColliders.Length)
            {
                HitCharacterController =
                    hitColliders[j]
                        .gameObject
                        .GetComponent<CharacterController>();
                if (HitCharacterController != null)
                {
                    Debug
                        .Log("hit controller is" +
                        HitCharacterController.GetCharacter().Name);
                    Debug
                        .Log("squad uuid is" +
                        HitCharacterController.GetSquadLeaderUUID());

                    //Debug.Log("my squad uuid is" + HoldingCharacterController.GetSquadLeaderUUID());
                    Debug
                        .Log("my squad uuid is" +
                        ActionTargetCharacterController.GetSquadLeaderUUID());

                    // if not targeting self or sqwuad
                    //if (HitCharacterController.GetUUID() != HoldingCharacterController.GetUUID() && HitCharacterController.GetSquadLeaderUUID() != HoldingCharacterController.GetSquadLeaderUUID())
                    if (
                        HitCharacterController.GetUUID() !=
                        ActionTargetCharacterController.GetUUID() &&
                        (
                        HitCharacterController.GetSquadLeaderUUID() !=
                        ActionTargetCharacterController.GetSquadLeaderUUID() ||
                        HitCharacterController.GetSquadLeaderUUID() == ""
                        )
                    )
                    {
                        hit = true;
                        break;
                    }
                }
                j += 1;
            }
            if (hit)
            {
                break;
            }
            i += 0.5f;

            //if(i >= 5.0f){
            //    break;
            //}
        }
        if (HitCharacterController != null && hit)
        {
            //do actual hit if got hit
            SetCanDoAction(1.0f);
            SetActionTargetCharacterController (HitCharacterController);

            // Set to target eachother
            SetTargetOnImpact(HoldingCharacter,
            HitCharacterController.gameObject);

            // if can fight and doesnt yet have a terget
            if (
                HitCharacterController.GetCanFight() &&
                !HitCharacterController.GetIsPlayer()
            )
            {
                SetTargetOnImpact(HitCharacterController.gameObject,
                HoldingCharacter);
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (Item.IsPickup && !isPickedUp)
        {
            foreach (ContactPoint contact in collision.contacts)
            {
                CharacterController CollidingCharacter =
                    collision.gameObject.GetComponent<CharacterController>();
                if (CollidingCharacter != null)
                {
                    //TODO remove this player only check
                    //if (CollidingCharacter.GetIsPlayer())
                    //{
                    if (!CollidingCharacter.GetHasItemInHand())
                    {
                        GetPickedUpBy (CollidingCharacter);

                        /*
                        //Debug.DrawRay(contact.point, contact.normal, Color.white);
                        //Debug.Log("I was hit by " + collision.gameObject.GetComponent<CharacterController>().GetCharacter().Name);
                        isPickedUp = true;
                        rb.useGravity = false;
                        rb.isKinematic = true;

                        rb.constraints = RigidbodyConstraints.None;

                        //TODO move relivant to character
                        Physics
                            .IgnoreCollision(collision
                                .gameObject
                                .GetComponent<Collider>(),
                            GetComponent<Collider>());

                        //ItemTransform.parent = collision.gameObject.GetComponent<CharacterController> ().GetCharacterTransform ();
                        HoldingCharacter = collision.gameObject;

                        SetActionTargetCharacterController(HoldingCharacter
                            .GetComponent<CharacterController>());

                        //ItemTransform.parent = HoldingCharacter.GetComponent<CharacterController>().GetHandTransform();
                        ItemTransform.parent =
                            ActionTargetCharacterController.GetHandTransform();

                        ItemTransform.localPosition = new Vector3(0, 0, 0);

                        Item.heldLocation = "Hand";
                        Item.holderUUID =
                            ActionTargetCharacterController.GetUUID();

                        //Item.holderUUID = HoldingCharacter.GetComponent<CharacterController>().GetUUID();
                        if (Item.ownerUUID == "")
                        {
                            Item.ownerUUID = Item.holderUUID;
                        }
                        */
                        break;
                    }
                    //}
                }
            }
            if (!isPickedUp)
            {
                //Debug.Log("Collision without picked up");
                // when item is dropped on ground
                if (collision.gameObject.tag == "Ground")
                {
                    ItemTransform.position += new Vector3(0.0f, 0.2f, 0.0f);
                    rb.constraints =
                        RigidbodyConstraints.FreezePositionX |
                        RigidbodyConstraints.FreezePositionY |
                        RigidbodyConstraints.FreezePositionZ |
                        RigidbodyConstraints.FreezeRotationX |
                        RigidbodyConstraints.FreezeRotationZ;
                    //rb.AddTorque(transform.up * 0.2f);
                }
                //else
                //{
                //    Debug.Log("Collision not on ground");
                //}
            }
        }
        /*
        else if (isPickedUp)
        {

            // TODO move this to attack
            if (Item.heldLocation == "Hand" && CurrentItemAction > 0.0f)// if item in hand, do dmg on impact
            {
                CharacterController CollidingCharacter = collision.gameObject.GetComponent<CharacterController>();
                if (CollidingCharacter != null)
                {
                    // cant hit self
                    if (CollidingCharacter.GetUUID() != HoldingCharacter.GetComponent<CharacterController>().GetUUID())
                    {

                        // cancle all physics
                        CollidingCharacter.SetVelocity(new Vector3(0, 0, 0));
                        HoldingCharacter.GetComponent<CharacterController>().SetVelocity(new Vector3(0, 0, 0));

                        //ignore interactions with squadmates
                        if (CollidingCharacter.GetSquadLeaderUUID() != HoldingCharacter.GetComponent<CharacterController>().GetSquadLeaderUUID())
                        {

                            SetCanDoAction(1.0f);
                            SetActionTargetCharacterController(HoldingCharacter.GetComponent<CharacterController>());


                            // move this to per item
                            //CollidingCharacter.AddValueToHealth(-1 * Item.Damage);


                            // Set to target eachother
                            SetTargetOnImpact(HoldingCharacter, collision.gameObject);
                            // if can fight
                            if (CollidingCharacter.GetCanFight())
                            {
                                SetTargetOnImpact(collision.gameObject, HoldingCharacter);
                            }
                        }
                        else
                        {
                            //Debug.Log("hit squadmate?");
                        }
                    }
                }
            }
        }
        */
    }

    public void GetPickedUpBy(CharacterController pickingUpCharacter)
    {
        isPickedUp = true;
        rb.useGravity = false;
        rb.isKinematic = true;

        rb.constraints = RigidbodyConstraints.None;

        Physics
            .IgnoreCollision(pickingUpCharacter
                .gameObject
                .GetComponent<Collider>(),
            GetComponent<Collider>());

        //ItemTransform.parent = collision.gameObject.GetComponent<CharacterController> ().GetCharacterTransform ();
        HoldingCharacter = pickingUpCharacter.gameObject;

        SetActionTargetCharacterController(HoldingCharacter
            .GetComponent<CharacterController>());

        //ItemTransform.parent = HoldingCharacter.GetComponent<CharacterController>().GetHandTransform();
        ItemTransform.parent =
            ActionTargetCharacterController.GetHandTransform();

        ItemTransform.localPosition = new Vector3(0, 0, 0);

        Item.heldLocation = "Hand";
        Item.holderUUID = ActionTargetCharacterController.GetUUID();

        //Item.holderUUID = HoldingCharacter.GetComponent<CharacterController>().GetUUID();
        if (Item.ownerUUID == "")
        {
            Item.ownerUUID = Item.holderUUID;
        }
        ActionTargetCharacterController.SetNeedsUIUpdate(true);
    }

    public void SetHeldLocation(
        string newHeldLocation,
        CharacterController ParentController
    )
    {
        //Debug.Log("I was set to be held by by " + ParentController.GetCharacter().Name);
        Item.heldLocation = newHeldLocation;

        isPickedUp = true;
        rb.useGravity = false;
        rb.isKinematic = true;

        rb.constraints = RigidbodyConstraints.None;

        //TODO move relivant to character
        Physics
            .IgnoreCollision(ParentController
                .gameObject
                .GetComponent<Collider>(),
            GetComponent<Collider>());

        //ItemTransform.parent = collision.gameObject.GetComponent<CharacterController> ().GetCharacterTransform ();
        HoldingCharacter = ParentController.gameObject;
        if (Item.heldLocation == "Hand")
        {
            ItemTransform.parent = ParentController.GetHandTransform();
        }
        else if (Item.heldLocation == "Back")
        {
            ItemTransform.parent = ParentController.GetBackTransform();
        }
        else if (Item.heldLocation == "Belt")
        {
            ItemTransform.parent = ParentController.GetBeltTransform();
        }
        else
        {
            Debug.Log("set held location to not valid location");
        }

        ItemTransform.localPosition = new Vector3(0, 0, 0);

        if (ActionTargetCharacterController == null)
        {
            SetActionTargetCharacterController(HoldingCharacter
                .GetComponent<CharacterController>());
        }

        Item.holderUUID = ActionTargetCharacterController.GetUUID();

        if (Item.heldLocation == "Hand")
        {
            //ItemTransform.localPosition = new Vector3(Item.HoldingOffsetX, Item.HoldingOffsetY, Item.HoldingOffsetZ);
            SetHoldingOffsetXYZ(new Vector3(Item.HoldingOffsetX,
                Item.HoldingOffsetY,
                Item.HoldingOffsetZ));
            //ItemTransform.localPosition = new Vector3(Item.HoldingOffsetX, Item.HoldingOffsetY, Item.HoldingOffsetZ);
        }
        else
        {
            ItemTransform.localPosition = new Vector3(0, 0, 0);
            SetHoldingOffsetXYZ(new Vector3(0, 0, 0));
        }
        //Debug.Log("held location updated postion:" + ItemTransform.position + Item.Name);
    }

    public string GetOwner()
    {
        return Item.ownerUUID;
    }

    public void SetOwner(string newOwner)
    {
        Item.ownerUUID = newOwner;
    }

    public float GetValue()
    {
        return Item.Value;
    }

    public CharacterController GetHoldingCharacterController()
    {
        return HoldingCharacter.gameObject.GetComponent<CharacterController>();
    }

    public string GetSummaryString()
    {
        /*
        gives summary string
        */
        int DamagePlusCount = (int)(Item.Damage / 10.0f);
        int ItemDollarSignCount = (int)(Item.Value / 10.0f);

        if (DamagePlusCount < 1)
        {
            DamagePlusCount = 1;
        }

        if (ItemDollarSignCount < 1)
        {
            ItemDollarSignCount = 1;
        }

        string valueString =
            "Value: " + new string('$', ItemDollarSignCount) + "\n";
        string damageString =
            "Damage: " + new string('+', DamagePlusCount) + "\n";

        if (
            Item.PrimaryActionClass == "POTION" ||
            Item.PrimaryActionClass == "SUMMON" ||
            Item.PrimaryActionClass == "NONE"
        )
        {
            damageString = "";
        }

        string summary =
            Item.Name +
            "\n" +
            Item.Description +
            "\n" +
            damageString +
            valueString;

        return summary;
    }

    public bool GetIsPickedUp()
    {
        return isPickedUp;
    }

    public bool GetCanGoOnBelt()
    {
        return Item.CanBePutOnBelt;
    }

    public Transform GetItemTransform()
    {
        return ItemTransform;
    }

    public Vector3 GetCurrentHoldingOffsetXYZ()
    {
        return new Vector3(ItemTransform.localPosition.x,
            ItemTransform.localPosition.y,
            ItemTransform.localPosition.z);
        //return new Vector3(Item.HoldingOffsetX, Item.HoldingOffsetY, Item.HoldingOffsetZ);
    }

    public void SetHoldingOffsetXYZ(Vector3 NewOffsetVector)
    {
        ItemTransform.localPosition =
            new Vector3(NewOffsetVector.x,
                NewOffsetVector.y,
                NewOffsetVector.z);
        //Debug.Log("set rotation to "+NewOffsetVector);
        //Debug.Log("set rotation to "+ItemTransform.localPosition);
    }

    public float GetHoldingRotationOffset()
    {
        return Item.HoldingRotationOffset;
    }

    public void SetHoldingRotationOffset(float NewRotationOffset)
    {
        ItemTransform.localRotation =
            Quaternion.Euler(NewRotationOffset, 0.0f, 0.0f);
    }

    public ItemData GetItem()
    {
        return Item;
    }

    public void SetCanBeDropped(bool newStatus)
    {
        CanBeDropped = newStatus;
    }

    public float GetCoolDown()
    {
        return CooldownTimer;
    }
}
