using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonBlockerManager : MonoBehaviour
{
    public static ButtonBlockerManager Instance { get; private set; }

    public List<ButtonBlocker> coinTossBlock;
    public List<ButtonBlocker> cardPickBlock;
    public List<ButtonBlocker> trumphBlock;
    public List<ButtonBlocker> endTurnBlock;

    public void SetBlock(List<ButtonBlocker> blockers, bool isBlocked)
    {
        foreach (var blocker in blockers)
        {
            if (isBlocked) blocker.BlockButton();
            else blocker.ReleaseButton();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
