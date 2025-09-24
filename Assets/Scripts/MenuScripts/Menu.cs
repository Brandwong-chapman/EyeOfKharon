using NUnit.Framework;
using UnityEngine;

public class Menu : ChangePanel
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
        HideAllExeptFor(PanelList,new GameObject[]{PanelList[1]});
    }
    

}