﻿
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;

public class CameraFollow : MonoBehaviour
{

    public float CameraMoveSpeed = 120.0f;
    public GameObject CameraFollowObj;
    Vector3 FollowPOS;
    public float clampAngle = 80.0f;
    public float inputSensitivity = 150.0f;
    public GameObject CameraObj;
    public GameObject PlayerObj;
    public float camDistanceXToPlayer;
    public float camDistanceYToPlayer;
    public float camDistanceZToPlayer;
    public float mouseX;
    public float mouseY;
    public float finalInputX;
    public float finalInputZ;
    public float smoothX;
    public float smoothY;
    private float rotY = 0.0f;
    private float rotX = 0.0f;


    private CharacterController Player = null;

    private CharacterData Character = null;
    private CharacterData TargetCharacter = null;

    // ui stuff
    [SerializeField] GameObject ManaUI;
    [SerializeField] GameObject HealthUI;
    [SerializeField] GameObject StaminaUI;
    [SerializeField] GameObject TargetUI;//healthbar for target
    [SerializeField] GameObject TargetName;//name of target
    [SerializeField] GameObject SquadList;//name of target

    private bool hasTarget = false;
    private string BaseSquadListText = "[Squad List]";
    private string SquadListText = "[Squad List]";

    private List<CharacterController> SquadCharacterControllers = null;



    // Use this for initialization
    void Start()
    {
        Vector3 rot = transform.localRotation.eulerAngles;
        rotY = rot.y;
        rotX = rot.x;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // TODO call on switch too
        GetPlayer();
        GetPlayerCharacter();
        SquadListText = GenerateSquadList();

    }

    // Update is called once per frame
    void Update()
    {

        // We setup the rotation of the sticks here
        float inputX = Input.GetAxis("RightStickHorizontal");
        float inputZ = Input.GetAxis("RightStickVertical");
        mouseX = Input.GetAxis("Mouse X");
        mouseY = Input.GetAxis("Mouse Y");
        finalInputX = inputX + mouseX;
        finalInputZ = inputZ + mouseY;

        rotY += finalInputX * inputSensitivity * Time.deltaTime;
        rotX += finalInputZ * inputSensitivity * Time.deltaTime;

        rotX = Mathf.Clamp(rotX, -clampAngle, clampAngle);

        Quaternion localRotation = Quaternion.Euler(rotX, rotY, 0.0f);
        transform.rotation = localRotation;

        // get stuff from current player and do ui

        if (Player == null)
        {
            GetPlayer();
        }

        GetPlayersTargetCharacter();
        DoUI();



        // TODO move this to camera
        if (Input.GetKeyDown("y"))
        {
            Debug.Log("swapping into target");
            //SwitchToTarget();
        }

        // get numbers 1-9 todo
        if (Input.GetKeyDown("1"))
        {
            Debug.Log("pushed 1");
            Debug.Log(SquadCharacterControllers[0]);
            SwitchToTarget(SquadCharacterControllers[0]);
        }
        if (Input.GetKeyDown("2"))
        {
            Debug.Log("pushed 2");
            SwitchToTarget(SquadCharacterControllers[1]);
        }



    }

    void LateUpdate()
    {
        CameraUpdater();
    }

    void CameraUpdater()
    {
        // set the target object to follow
        Transform target = CameraFollowObj.transform;

        //move towards the game object that is the target
        float step = CameraMoveSpeed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, target.position, step);
    }


    private void SwitchToTarget(CharacterController SwitchTargetCharacterController)
    {
        CameraFollowObj = SwitchTargetCharacterController.GetCameraTarget();
        Player.SwapIntoTarget(SwitchTargetCharacterController);

        GetPlayer();
        GetPlayerCharacter();
        SquadListText = GenerateSquadList();
    }


    private void GetPlayer()
    {
        Player = CameraFollowObj.gameObject.GetComponentInParent<CharacterController>();
    }

    private void GetPlayerCharacter()
    {
        Character = Player.GetCharacter();
    }


    private void GetPlayersTargetCharacter()
    {
        hasTarget = Player.GetHasTarget();
        TargetCharacter = Player.GetTargetCharacter();
    }


    private void DoUI()
    {
        DoHealthUI();
        DoStaminaUI();
        DoManaUI();
        DoTargetHealtBarUI();
        DoSquadUI();
    }

    private void DoHealthUI()
    {
        HealthUI.GetComponent<FillUI>().SetTo(Character.CurrentHealth / Character.MaxHealth);

    }
    private void DoStaminaUI()
    {
        StaminaUI.GetComponent<FillUI>().SetTo(Character.CurrentStamina / Character.MaxStamina);
    }

    private void DoManaUI()
    {
        ManaUI.GetComponent<FillUI>().SetTo(Character.CurrentMana / Character.MaxMana);

    }

    private void DoTargetHealtBarUI()
    {
        if (hasTarget)
        //check if freindly, if so show only name, else show health bar
        {
            TargetName.GetComponent<Text>().text = TargetCharacter.Name;
            if (!TargetCharacter.IsFollower)
            {
                //TargetUI.GetComponent<FillUI>().SetTo(TargetCharacter.CurrentHealth);
                //float targetsHealth = CombatTarget.GetComponent<CharacterController>().GetCurrentHealth();
                TargetUI.GetComponent<FillUI>().SetTo(TargetCharacter.CurrentHealth / TargetCharacter.MaxHealth);
            }

        }
        else// if no target, hide UI
        {
            TargetUI.GetComponent<FillUI>().SetTo(0.0f);
            TargetName.GetComponent<Text>().text = "";
        }
    }


    private void DoSquadUI()
    {


        SquadList.GetComponent<Text>().text = SquadListText;

    }

    private string GenerateSquadList()
    {
        // clear squadlist
        SquadCharacterControllers = new List<CharacterController>();

        string ListString = "\n";
        int counter = 1;
        string myId = Player.GetUUID();
        // get all characters who have same squad leader
        var characterControllersList = FindObjectsOfType<CharacterController>();
        string id;
        foreach (CharacterController controller in characterControllersList)
        {
            id = controller.GetUUID();
            Debug.Log("found id " + id + " im " + myId);
            Debug.Log("char in my squad " + IsCharacterInMySquad(controller));
            if (IsCharacterInMySquad(controller))
            {
                ListString = ListString + AddCharatcerNameToList(controller, counter, (id == myId));
                SquadCharacterControllers.Add(controller);
                counter = counter + 1;
            }

        }

        return BaseSquadListText + ListString;
    }

    private bool IsCharacterInMySquad(CharacterController targetCharacterController)
    {
        string mySquadLeader = Player.GetSquadLeaderUUID();
        string theirSquadLeader = targetCharacterController.GetSquadLeaderUUID();
        if (mySquadLeader == theirSquadLeader)
        {
            return true;
        }
        return false;
    }

    private string AddCharatcerNameToList(CharacterController targetCharacterController, int ListCounter, bool isMe)
    {
        string lineString = ListCounter.ToString() + " : " + targetCharacterController.GetCharacter().Name;
        if (isMe)
        {
            lineString = "*" + lineString;

        }
        if (targetCharacterController.GetSquadLeaderUUID() == targetCharacterController.GetUUID())
        {

            lineString = "#" + lineString;
        }
        return lineString + "\n";
    }

    private void SelectSquadMemberByNumber(int selectedMember)
    {


    }


    private void SetFollowObject(GameObject newFollowObj)
    {
        CameraFollowObj = newFollowObj;
    }




}