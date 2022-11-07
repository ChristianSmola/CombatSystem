using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.AI;

public class Combat : MonoBehaviour
{
    public class Soldier
    {
        public GameObject SoldierGO;
        public Animation Anim;
        public NavMeshAgent Agent;
        public Vector3 PositionInFormation;
        public Vector3 CurrentPosition;
        public Trigger trigger;
        public Rank rank;

        public Soldier (GameObject GO, /*Animation anim,*/ NavMeshAgent agent, Rank _rank)
        {
            SoldierGO = GO;
            //Anim = anim;
            Agent = agent;
            rank = _rank;
        }
    }

    public class Division
    {
        public string Owner;
        public int Width;
        public GameObject DivisionGO;
        public List<Soldier> SoldierList = new List<Soldier>();
        public Side _Side;
        public Formation formation;
        public bool IsInMotion = false;

        public Division(string owner, Side side, Formation form, GameObject GO, List<Soldier> Soldiers, int width)
        {
            Owner = owner;
            _Side = side;
            formation = form;
            DivisionGO = GO;
            SoldierList = Soldiers;
            Width = width;
        }
    }

    public enum Side
    {
        Friendly, Hostile, Neurtal
    }

    public enum Formation
    {
        Rect, Wedge
    }

    public enum Rank
    {
        Legionary, Centurion
    }

    private bool ShutDownChildThread = false;
    private Thread ChildThread;
    public List<Action> FunctionsToRunInChildThread = new List<Action>();

    public List<Division> DivisionList = new List<Division>();

    public System.Diagnostics.Stopwatch UpdateLogicTimer;

    // Start is called before the first frame update
    void Start()
    {
        int DivisionWidth = 12;

        for (int x = 0; x < 3; x++)
        {
            GameObject GO = new GameObject();
            GO.transform.position = new Vector3(500f, 0f, 515 + (x * 15f));

            GO.name = "Roman Division " + x.ToString();

            //Add the CreateNewSoldierList Function 
            Division div = new Division("Rome", Side.Friendly, Formation.Rect, GO, CreateNewSoldierList(45, 12, GO), DivisionWidth);

            div.IsInMotion = true;

            //CalculateMinMaxWidth(div);
            //FunctionsToRunInChildThread.Add(() => AssignSoldiersToTheirDiv(div));

            GO.transform.eulerAngles = new Vector3(0f, 0f, 0f);

            //EstablishDivisionBorders(div);
            DivisionList.Add(div);
        }

        ChildThread = new Thread(ChildThreadFunction);
        ChildThread.Start();

        //SelectionBox.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ChildThreadFunction()
    {
        UpdateLogicTimer = System.Diagnostics.Stopwatch.StartNew();

        while (true)
        {
            if (ShutDownChildThread)
                ChildThread.Abort();

            List<Action> ActionList = new List<Action>();

            if (UpdateLogicTimer.ElapsedMilliseconds > 100f)
            {
                //ActionList.Add(() => UpdateLogic());
                UpdateLogicTimer.Restart();
            }

            //Copy the actions and clear the list
            lock (FunctionsToRunInChildThread)
            {
                if (FunctionsToRunInChildThread.Count > 0)
                    foreach (Action act in FunctionsToRunInChildThread)
                        ActionList.Add(act);

                FunctionsToRunInChildThread.Clear();
            }

            if (ActionList.Count > 0)
            {
                foreach (Action act in ActionList)
                    act();
            }
            else
            {
                //sleep for a tenth of a second
                Thread.Sleep(100);
            }
        }
    }

    public List<Soldier> CreateNewSoldierList(int NumberOfSoldiers, int width, GameObject GO/*, State EquipState, Military.UnitSubType sub, Military.UnitType Type = Military.UnitType.HeavyInfantry*/)
    {
        try
        {
            List<Soldier> soldiers = new List<Soldier>();

            int TotalNumberOfColumns = NumberOfSoldiers / width;
            int CurrentRowIsEven = 0;

            //if Total # of Soldiers is not divisible by the width, add 1
            if (NumberOfSoldiers % width != 0)
                TotalNumberOfColumns++;

            if (width % 2 == 0)
                CurrentRowIsEven = 1;

            for (int RowIndex = 0; RowIndex < TotalNumberOfColumns; RowIndex++)
            {
                int CurrentRowWidth = 0;

                if (soldiers.Count + width < NumberOfSoldiers)
                    CurrentRowWidth = width;
                else
                    CurrentRowWidth = NumberOfSoldiers - soldiers.Count;

                int WidthOffset = 0;
                float xPositionOffset = 0f;

                if (CurrentRowWidth % 2 != 0)
                    WidthOffset = 1;
                else
                    xPositionOffset = 0.5f;

                for (int x = -(CurrentRowWidth / 2) + CurrentRowIsEven; x < (CurrentRowWidth / 2) + WidthOffset + CurrentRowIsEven; x++)
                {
                    Rank rank = Rank.Legionary;

                    //This offset bit is convoluted, basically it'll be -1 when width is even and zero when width is odd
                    if (RowIndex == 0 && x == (width / 2) /*- 1 + WidthOffset*/)
                        rank = Rank.Centurion;

                    //GameObject[] Cunts = Resources.LoadAll<GameObject>("Updated Units/");

                    //GameObject Prefab = Resources.Load<GameObject>("New Export Folder/" + rank.ToString() + " " + EquipState.ToString());
                    GameObject Prefab = Resources.Load<GameObject>("Updated Units/" + rank.ToString());

                    GameObject UnitGO = Instantiate(Prefab, new Vector3(GO.transform.position.x + x + xPositionOffset, 0f, GO.transform.position.z - (RowIndex * 1f)), GO.transform.rotation, GO.transform);

                    UnitGO.transform.Find(rank.ToString() + " Idle").gameObject.SetActive(true);

                    UnitGO.transform.tag = "Division";

                    soldiers.Add(new Soldier(UnitGO, UnitGO.GetComponent<NavMeshAgent>(), rank));

                    soldiers[soldiers.Count - 1].PositionInFormation = new Vector2(x, RowIndex);
                }
            }


            foreach (Soldier sol in soldiers)
                sol.trigger = sol.SoldierGO.transform.Find("Cone").GetComponent<Trigger>();

            foreach (Soldier sol in soldiers)
                sol.CurrentPosition = sol.SoldierGO.transform.position;

            return soldiers;
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
            return null;
        }
    }
}
