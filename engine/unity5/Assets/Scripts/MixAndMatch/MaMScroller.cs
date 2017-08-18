using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Controls the scrolling of the panels
public class MaMScroller : MonoBehaviour {
    private GameObject mixAndMatchModeScript;

    private GameObject wheelRightScroll;
    private GameObject wheelLeftScroll;
    private List<GameObject> wheels;

    private GameObject driveBaseRightScroll;
    private GameObject driveBaseLeftScroll;
    private List<GameObject> driveBases;

    private GameObject presetRightScroll;
    private GameObject presetLeftScroll;
    private List<GameObject> presetClones;
	
    public void FindAllGameObjects()
    {
        mixAndMatchModeScript = GameObject.Find("MixAndMatchModeScript");        

        wheelRightScroll = GameObject.Find("WheelRightScroll");
        wheelLeftScroll = GameObject.Find("WheelLeftScroll");
        wheels = mixAndMatchModeScript.GetComponent<MixAndMatchMode>().wheels;

        driveBaseRightScroll = GameObject.Find("BaseRightScroll");
        driveBaseLeftScroll = GameObject.Find("BaseLeftScroll");
        driveBases = mixAndMatchModeScript.GetComponent<MixAndMatchMode>().bases;

        presetRightScroll = GameObject.Find("PresetRightScroll");
        presetLeftScroll = GameObject.Find("PresetLeftScroll");
        presetClones = mixAndMatchModeScript.GetComponent<MixAndMatchMode>().presetClones;
    }

    public bool Scroll(bool right, List<GameObject> objectList, int firstObject, Vector2[] positions, GameObject rightScroll, GameObject leftScroll)
    {
        int _firstObject = firstObject;

        //Makes sure that the list is not already scrolling
        if (objectList[_firstObject].GetComponent<MixAndMatchScroll>() != null) return false;
        if (objectList[_firstObject + 1].GetComponent<MixAndMatchScroll>() != null) return false;
        if (objectList[_firstObject + 2].GetComponent<MixAndMatchScroll>() != null) return false;

        //If the right scroll button is clicked, scroll to the right and activate the next object
        if (right && _firstObject + 3 < objectList.Count)
        {
            objectList[_firstObject].SetActive(false);
            objectList[_firstObject + 1].AddComponent<MixAndMatchScroll>().SetTargetPostion(positions[1]);
            objectList[_firstObject + 2].AddComponent<MixAndMatchScroll>().SetTargetPostion(positions[2]);
            objectList[_firstObject + 3].GetComponent<RectTransform>().anchoredPosition = positions[4];
            objectList[_firstObject + 3].SetActive(true);
            objectList[_firstObject + 3].AddComponent<MixAndMatchScroll>().SetTargetPostion(positions[3]);
            _firstObject++;
        }

        //If the left scroll button is clicked, scroll to the left and activate the previous object
        if (!right && _firstObject - 1 >= 0)
        {
            objectList[_firstObject - 1].GetComponent<RectTransform>().anchoredPosition = positions[0];
            objectList[_firstObject - 1].SetActive(true);
            objectList[_firstObject - 1].AddComponent<MixAndMatchScroll>().SetTargetPostion(positions[1]);
            objectList[_firstObject].AddComponent<MixAndMatchScroll>().SetTargetPostion(positions[2]);
            objectList[_firstObject + 1].AddComponent<MixAndMatchScroll>().SetTargetPostion(positions[3]);
            objectList[_firstObject + 2].SetActive(false);
            _firstObject--;
        }

        //Set both scroll buttons to active
        rightScroll.SetActive(true);
        leftScroll.SetActive(true);

        //If the last object showing is the last object in the list, deactivate the right scroll button
        if (_firstObject + 3 == objectList.Count)
        {
            rightScroll.SetActive(false);
        }

        //If the first object showing is the first object in the list, deactive the left scroll button
        if (_firstObject == 0)
        {
            leftScroll.SetActive(false);
        }

        return true;
    }

    int firstWheel = 0;
    public void ScrollWheels(bool right)
    {
        
        Vector2[] positions = { new Vector2(-290f, 7.5f), new Vector2(-90f, 7.5f), new Vector2(110f, 7.5f), new Vector2(310f, 7.5f), new Vector2(510f, 7.5f), };
        if (Scroll(right, wheels, firstWheel, positions, wheelRightScroll, wheelLeftScroll)) firstWheel = (right) ? firstWheel + 1 : firstWheel - 1;
    }

    int firstDriveBase = 0;
    public void ScrollDriveBase(bool right)
    {

        Vector2[] positions = { new Vector2(-290f, 8f), new Vector2(-90f, 8f), new Vector2(110f, 8f), new Vector2(310f, 8f), new Vector2(510f, 8f), };
        if (Scroll(right, driveBases, firstDriveBase, positions, driveBaseRightScroll, driveBaseLeftScroll)) firstDriveBase = (right) ? firstDriveBase + 1 : firstDriveBase - 1;
    }

    public static int firstPreset = 0;
    public void ScrollPreset(bool right)
    {
        presetClones = mixAndMatchModeScript.GetComponent<MixAndMatchMode>().presetClones;
        Debug.Log("First Preset" + firstPreset);
        Vector2[] positions = { new Vector2(-255, 0), new Vector2(-65, 0), new Vector2(125, 0), new Vector2(315, 0), new Vector2(505, 0), };
        if (Scroll(right, presetClones, firstPreset, positions, presetRightScroll, presetLeftScroll)) firstPreset = (right) ? firstPreset + 1 : firstPreset - 1;
    }

    public void ResetFirsts()
    {
        firstWheel = 0;
        firstDriveBase = 0;
        firstPreset = 0;
    }

}
