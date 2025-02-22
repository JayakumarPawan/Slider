using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JungleGrid : SGrid
{
    public ChadRace chadRace;

    public List<GameObject> jungleBridgeRails;
    public List<GameObject> jungleBridges;
    public GameObject minecartProp;
    public GameObject factoryDoor;

    public override void Init() 
    {
        InitArea(Area.Jungle);
        base.Init();
    }
    
    protected override void Start()
    {
        base.Start();

        AudioManager.PlayMusic("Jungle");
    }

    public override void Save() 
    {
        base.Save();
    }

    public override void Load(SaveProfile profile)
    {
        base.Load(profile);
        

        if (profile.GetBool("jungleBridgeFixed"))
        {
            FixBridge();
        }
        else if (profile.GetBool("jungleTurnedInMinecart"))
        {
            minecartProp.SetActive(true);
        }
        else if (profile.GetBool("jungleTurnedInRail"))
        {
            foreach (GameObject g in jungleBridgeRails)
            {
                g.SetActive(true);
            }
        }

        if (profile.GetBool("jungleFactoryDoorOpened"))
        {
            OpenFactoryDoor();
        }
    }

    public override void EnableStile(STile stile, bool shouldFlicker=true)
    {
        if (GetNumTilesCollected() == 3)
        {
            string s = GetGridString(true);
            int location2 = s.IndexOf("2");
            int x2 = location2 % 3;
            int y2 = 2 - (location2 / 3);
            STile two = grid[x2, y2];

            bool doubleSwap = false;
            if (!CheckGrid.contains(s, "23"))
            {
                if (x2 == 2 && stile.x == 0) { 

                    doubleSwap = true;
                } else if (stile.x == 0)
                {
                    STile other = grid[x2 + 1, y2];
                    if (other.isTileActive)
                    {
                        doubleSwap = true;
                    }
                    else
                    {
                        SwapTiles(stile, other);
                    }
                } else
                {
                    STile other = grid[stile.x - 1, stile.y];
                    if (other.isTileActive)
                    {
                        doubleSwap = true;
                    }
                    else
                    {
                        SwapTiles(two, other);
                    }
                }
            } 

            if (doubleSwap)
            {
                List<STile> tiles = new List<STile>();
                //options for tile 2
                for (int i = 0; i < 2; i ++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        tiles.Add(grid[i, j]);
                    }
                }

                foreach (STile tile in tiles)
                {
                    if (tile.isTileActive)
                    {
                        continue;
                    }
                    if (grid[tile.x + 1, tile.y].isTileActive)
                    {
                        continue;
                    }

                    STile other2 = tile;
                    STile other3 = grid[tile.x + 1, tile.y];

                    SwapTiles(other2, two);
                    SwapTiles(other3, stile);
                    break;
                }
            }
        }
        base.EnableStile(stile, shouldFlicker);
        chadRace.CheckChad(this, null);
    }

    // === Jungle Puzzle Specific ===
    
    // Puzzle 5 - Chad Race
    

    public void SpawnChadRewards() {
        Collectible c = GetCollectible("Boots");
        if (!PlayerInventory.Contains(c))
        {
            c.gameObject.SetActive(true);
        }

        c = GetCollectible("Slider 6");
            
        if (!PlayerInventory.Contains(c))
        {
            c.gameObject.SetActive(true);
        }
    }

    // Broken bridge -- puzzle 7

    public void TurnInRail()
    {
        if (SaveSystem.Current.GetBool("jungleTurnedInRail"))
            return;

        SaveSystem.Current.SetBool("jungleTurnedInRail", true);
        AudioManager.Play("Puzzle Complete");

        if (SaveSystem.Current.GetBool("jungleTurnedInMinecart"))
        {
            FixBridge();
        }
        else
        {
            foreach (GameObject g in jungleBridgeRails)
            {
                g.SetActive(true);
            }
        }
    }

    public void TurnInMinecart()
    {
        if (SaveSystem.Current.GetBool("jungleTurnedInMinecart"))
            return;
            
        SaveSystem.Current.SetBool("jungleTurnedInMinecart", true);
        AudioManager.Play("Puzzle Complete");

        PlayerInventory.RemoveAndDestroyItem();

        minecartProp.SetActive(true);
        // particlemanager.poof

        if (SaveSystem.Current.GetBool("jungleTurnedInRail"))
        {
            FixBridge();
        }
    }

    public void FixBridge()
    {
        SaveSystem.Current.SetBool("jungleBridgeFixed", true);

        // animate minecart?
        
        foreach (GameObject g in jungleBridges)
        {
            g.SetActive(true);
        }
    }

    protected override void CheckForCompletionOnSetGrid()
    {
        CheckForJungleCompletion();
    }

    public void CheckForJungleCompletion() {
        if (IsJungleComplete()) 
        {
            StartCoroutine(ShowButtonAndMapCompletions());
            AchievementManager.SetAchievementStat("completedJungle", 1);
        }
    }

    public void IsJungleComplete(Condition c) => c.SetSpec(IsJungleComplete());
    public bool IsJungleComplete() => CheckGrid.contains(GetGridString(), "718_523_964");
    public void IsRailComplete(Condition c) => c.SetSpec(IsRailComplete());
    public bool IsRailComplete() => CheckGrid.contains(GetGridString(), "((718)|(781))_..._...");

    public void SetSecretaryCheckFlags()
    {
        if (IsJungleComplete())
        {
            SaveSystem.Current.SetBool("jungleSecretaryAllComplete", true);
        }
        if (IsRailComplete())
        {
            SaveSystem.Current.SetBool("jungleSecretaryRailComplete", true);
        }
    }

    public void OpenFactoryDoor()
    {
        factoryDoor.SetActive(false);

        if (SaveSystem.Current.GetBool("jungleTurnedInMinecart"))
            return;

        SaveSystem.Current.SetBool("jungleFactoryDoorOpened", true);

        for (int i = -2; i <= 2; i++)
        {
            ParticleManager.SpawnParticle(ParticleType.SmokePoof, factoryDoor.transform.position + new Vector3(0.5f, i), factoryDoor.transform);
        }
    }
}
