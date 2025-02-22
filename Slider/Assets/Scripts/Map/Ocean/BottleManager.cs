using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BottleManager : MonoBehaviour
{
    private const string validTiles = "1367"; //these tiles have a water path for the bottle
    
    public GameObject bottlePrefab;
    public bool puzzleSolved = false;

    public Item romeosBottle; // for the spawn animation
    private Transform romeosBottleHolder;
    
    private int turncounter = 0;
    private bool puzzleActive = false;

    private GameObject bottle = null;
    private bool bottleIsInWater = false;
    private STile bottleParentStile = null;
    private UITracker bottleUITracker;
    public GameObject uiTrackerSinkingBottlePrefab;
    
    [SerializeField] private Vector3 bottleInitialLocation = new Vector3(-7.5f,41.5f);
    private List<Vector3> positions = new List<Vector3> { 
        new Vector3(-4.5f,7.5f), 
        new Vector3(0,0), 
        new Vector3(4.5f,-7.5f)
    };

    
    private void Awake() 
    {
        romeosBottleHolder = romeosBottle.transform.parent;
    }

    public void OnEnable()
    {
        //subscribe stile move end to update counter
        UIArtifact.OnButtonInteract += UpdateBottleLocation;
    }


    public void OnDisable()
    {
        //unsubscribe stile move end to update counter
        UIArtifact.OnButtonInteract -= UpdateBottleLocation;
    }

    private IEnumerator StartBottleMovementAnimation(Vector3 start, Vector3 end, float moveDuration)
    {
        if(start == end)
            yield break;
        float t = 0;
        while(t < moveDuration)
        {
            t += Time.deltaTime;

            if (bottle == null)
                yield break;

            bottle.transform.localPosition = Vector3.Lerp(start, end, t/moveDuration);
            yield return null;
        }
        bottle.transform.localPosition = end;
    }

    private void UpdateBottleLocation(object sender, System.EventArgs e){
        if(puzzleActive)
        {
            turncounter+=1;
            if (turncounter >2)
            {
                DestroyBottle();
            }
            else
                StartCoroutine(StartBottleMovementAnimation(positions[turncounter-1] ,positions[turncounter], 1));
        }
    }

    private IEnumerator DestroyBottleExecutor()
    {
        //Debug.Log("deleting bottle");
        if (bottle != null)
            bottle.GetComponent<Animator>().SetBool("IsSinking", true);

        GameObject tempBottleGO = this.bottle;
        this.bottle = null;
        bottleIsInWater = false;
        puzzleActive = false;
        bottleParentStile = null;

        bottleUITracker.GetComponent<Animator>().SetBool("sink", true);
        bottleUITracker = null;
        
        romeosBottle.spriteRenderer.enabled = true;

        yield return new WaitForSeconds(0.5f);

        AudioManager.Play("Bottle Sink");

        yield return new WaitForSeconds(1.5f);

        UITrackerManager.RemoveTracker(tempBottleGO);
    }

    //so that juliet can call it
    public void DestroyBottle()
    {
        StartCoroutine(DestroyBottleExecutor());
        // DestroyBottleExecutor();
    }

    public void CreateNewBottle()
    {
        //create new bottle after talking to romeo and puzzle is not solved
        if(bottle == null && !puzzleSolved && CheckGrid.contains(SGrid.GetGridString(),$"[{validTiles}].._..._..."))
        {
            puzzleActive = true;

            bottle = GameObject.Instantiate(bottlePrefab, bottleInitialLocation, Quaternion.identity);

            bottleParentStile = SGrid.Current.GetGrid()[0, 2];
            bottle.transform.SetParent(bottleParentStile.transform);

            turncounter = 0;
            bottle.transform.localPosition = positions[(int)(turncounter)];

            GameObject bottleTrackerGO = Instantiate(uiTrackerSinkingBottlePrefab);
            bottleUITracker = bottleTrackerGO.GetComponent<UITracker>();
            UITrackerManager.AddNewCustomTracker(bottleUITracker, bottle);


            bottleIsInWater = false;
            RomeoBottleSpawnAnimation(bottle);
            
        }
    }

    private void RomeoBottleSpawnAnimation(GameObject actualBottle)
    {
        actualBottle.GetComponent<SpriteRenderer>().enabled = false;

        romeosBottle.DropItem(bottle.transform.position, () => {

            ParticleManager.SpawnParticle(ParticleType.SmokePoof, bottle.transform.position, actualBottle.transform);
            actualBottle.GetComponent<SpriteRenderer>().enabled = true;
            bottleIsInWater = true;

            romeosBottle.spriteRenderer.enabled = false;
            romeosBottle.transform.SetParent(romeosBottleHolder);
            romeosBottle.transform.localPosition = Vector3.zero;
        });
        // yield return new WaitUntil(() => true) ;
    }

    public void InvalidTile(Condition c)
    {
       //only tiles 1,3, and 8 have a water path for the bottle to go through
       if( CheckGrid.contains(SGrid.GetGridString(),$"[{validTiles}].._..._..."))
        {
            c.SetSpec(false);
        }
        else
        {
            c.SetSpec(true);
        }
    }

    public void BottleIsInWater(Condition c)
    {
        c.SetSpec(bottleIsInWater);
    }

    public void MessageDelivered(Condition c)
    {
        if ((turncounter < 3 && bottleParentStile != null && bottleParentStile.x == 2 && bottleParentStile.y == 0) || puzzleSolved)
        {
            c.SetSpec(true);
            puzzleSolved = true;

        }
        else
        {
            c.SetSpec(false);
        }
    }
}


