using NUnit.Framework;
using UnityEngine;

public class Menu : MonoBehaviour
{
    public GameObject[] PanelList;
    
    
    public void Start()
    {
       
    }
    
    public void Reset()
    {
        
    }

    public void ShowLevelMenu()
    {
        ChangePanel.HideAllExeptFor(PanelList,new GameObject[]{PanelList[1]});
    }
    

}