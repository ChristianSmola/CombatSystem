using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using UnityEngine;

public class Combat : MonoBehaviour
{
    public class Soldier
    {
        public GameObject SoldierGO;
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

    private bool ShutDownChildThread = false;
    private Thread ChildThread;
    public List<Action> FunctionsToRunInChildThread = new List<Action>();

    public List<Division> DivisionList = new List<Division>();

    public Stopwatch UpdateLogicTimer;

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
            Division div = new Division("Rome", Side.Friendly, Formation.Rect, GO, new List<Soldier>(), DivisionWidth);

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
        UpdateLogicTimer = Stopwatch.StartNew();

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

    public List<Soldier> CreateNewSoldierList(int NumberOfSoldiers, int width, GameObject GO)
    {
        try
        {
            List<Soldier> Soldiers = new List<Soldier>();

            int TotalNumberOfColumns = NumberOfSoldiers / width;

            if (NumberOfSoldiers % width != 0)
                TotalNumberOfColumns++;

            int CurrentRowIsEven = 0;

            if (width % 2 == 0)
                CurrentRowIsEven = 1;

            for (int RowIndex = 0; RowIndex < TotalNumberOfColumns; RowIndex++)
            {
                int CurrentRowWidth = 0;

                if (Soldiers.Count + width < NumberOfSoldiers)
                    CurrentRowWidth = width;
                else
                    CurrentRowWidth = NumberOfSoldiers - Soldiers.Count;

                int WidthOffset = 0;
                float xPositionOffset = 0f;

                if (CurrentRowWidth % 2 != 0)
                    WidthOffset = 1;
                else
                    xPositionOffset = 0.5f;

                for (int x = -(CurrentRowWidth / 2) + CurrentRowIsEven; x < (CurrentRowWidth / 2) + WidthOffset + CurrentRowIsEven; x++)
                {
                    //Rank rank = Rank.Legionary;

                    //GameObject Prefab = Resources.Load<GameObject>()
                }
            }

            return Soldiers;
        }
        catch(Exception ex)
        {
            UnityEngine.Debug.Log(ex.Message);
            return null;
        }
    }
}
