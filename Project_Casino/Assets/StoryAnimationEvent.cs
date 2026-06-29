using UnityEngine;

public class StoryAnimationEvent : MonoBehaviour
{
    [SerializeField]
    private StorySceneController controller;


    public void PlayCut1Bell()
    {
        controller.PlayCut1Bell();
    }

    public void PlayCut1MailBox()
    {
        controller.PlayCut1MailBox();
    }
    public void PlayCut3Letter()
    {
        controller.PlayCut3Letter();
    }
    public void PlayBoom()
    {
        controller.PlayCut4Boom();
    }
    public void PlayCut5Welcome()
    {
        controller.PlayCut5Welcome();
    }

}
