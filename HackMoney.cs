using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Drawing;
using GTA;
using GTA.Math;

namespace HackMoney
{
    public class HackMoney : Script
    {
        int tickInterval = Game.GameTime + 1000; //script tick interval
        int markerDrawDistance = 10; // distance for the marker to be displayed
        readonly Random rand = new Random(); //rng
        bool useKey = false; //if the 'e' key is pressed
        bool isHacking = false; //if the player is currently hacking
        int hackWait = 18000; // time player has to wait to hack again
        bool firstHack = false; //has the player done the first hack

        int minCash = 25000; //minimum amount of cash

        Dictionary<Vector3, int> hackValues = new Dictionary<Vector3, int>()
        {
            {new Vector3(-1042f,-229f,38f), 750000 },//Life Invader
            {new Vector3(-4f,-1087f,25.5f), 250000 },//PDM
            {new Vector3(-467f,1129f,325f), 500000 },//observatory
            {new Vector3(3432f,3634f,39.5f), 1250000 }//Labs
        };

        Dictionary<Vector3, int> hackedTerminals = new Dictionary<Vector3, int>()
        {
            {new Vector3(-1042f,-229f,38f), 0},//Life Invader
            {new Vector3(-4f,-1087f,25.5f), 0},//PDM
            {new Vector3(-467f,1129f,325f), 0},//observatory
            {new Vector3(3432f,3634f,39.5f), 0}//Labs
        };

        readonly Vector3[] hackCoords =
        {
            new Vector3(-1042f,-229f,38f),//Life Invader
            new Vector3(-4f,-1087f,25.5f),//PDM
            new Vector3(-467f,1129f,325f),//observatory
            new Vector3(3432f,3634f,39.5f)//Labs
        };
        public HackMoney()
        {
            Tick += onTick;
            KeyDown += HackMoney_KeyDown;
            GTA.UI.Notification.Show("HackMoney Started");
            foreach (Vector3 i in hackCoords)
            {
                Blip newBlip = World.CreateBlip(i);
                newBlip.Sprite = BlipSprite.Laptop;
                newBlip.Name = "Money Hack Terminal";
                newBlip.DisplayType = BlipDisplayType.MainMapSelectable;
            }
        }

        private void HackMoney_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.E)
            {
                useKey = true;
            }
            else{ useKey = false; }
        }

        private void onTick(object sender, EventArgs e)
        {
            if (Game.GameTime > tickInterval)
            {
                //Resolve Hack Cooldown
                if(firstHack == true)
                {
                    if (isHacking == false)
                    {
                        Dictionary<Vector3, int> newHackedTerminals = new Dictionary<Vector3, int>() { };
                        foreach (KeyValuePair<Vector3, int> i in hackedTerminals)
                        {
                            newHackedTerminals.Add(i.Key, i.Value);
                            if(hackedTerminals[i.Key] != 0)
                            {
                                newHackedTerminals[i.Key] -= 1;
                            }
                        }
                        hackedTerminals = newHackedTerminals;
                    }
                }
                //Find nearby hack blips
                foreach (Vector3 i in hackCoords)
                {
                    
                    //if in range
                    if (World.GetDistance(Game.Player.Character.Position, i) <= markerDrawDistance)
                    {
                        //draw marker for coords
                        World.DrawMarker(MarkerType.VerticalCylinder, i, Vector3.Zero, Vector3.Zero, new Vector3(0.7f, 0.7f, 0.7f), System.Drawing.Color.Green);
                    }
                    //if not in vehicle
                    if (Game.Player.Character.CurrentVehicle == null)
                    {
                        //if in close range
                        if (World.GetDistance(Game.Player.Character.Position, i) <= 2)
                        {
                            if(hackedTerminals[i] == 0)
                            {
                                if (isHacking == false)
                                {
                                    GTA.UI.Screen.ShowHelpTextThisFrame("Press E start hack");
                                    //if key press
                                    if (useKey)
                                    {
                                        //Show ui
                                        startHack(i);
                                    }
                                }
                            }
                            else
                            {
                                GTA.UI.Screen.ShowHelpTextThisFrame("Cannot hack at this moment, please wait " + (hackedTerminals[i]/100).ToString() + " seconds to hack this terminal again");
                            }
                        }
                    }

                }
            }
        }

        public void startHack(Vector3 location)
        {
            isHacking = true;
            GTA.UI.Notification.Show("Hack Started");
            Wait(5000);
            GTA.UI.Notification.Show("Hack Finished");
            //add wanted level
            if(Game.Player.WantedLevel < 3)
            {
                Game.Player.WantedLevel = 3;
            }
            Game.Player.WantedCenterPosition = Game.Player.Character.Position;

            //Find max allowance of cash
            int maxCashTotal = hackValues[location];
            firstHack = true;
            //add money
            Game.Player.Money += rand.Next(minCash, maxCashTotal);
            //reset vars
            isHacking = false;
            hackedTerminals[location] = hackWait;
        }
    }
}
