using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BackgroundController : MonoBehaviour
{
    //Singleton Instance
    public static BackgroundController main; 

    //Instances
    RectTransform backgroundPanel;
    Grapher grapher;

    [Header("Set Dimentions")]
    [SerializeField] Vector2 initialCanvasDimensions;
    [SerializeField] Vector2 minSize;
    Vector2 canvasDimensions;

    [Header("Grab Prefrences")]
    [SerializeField] float grabThreshold;
    [SerializeField] float edgeDragThreshold;

    //Mouse states
    [HideInInspector] public bool mouseOverBackground, mouseOverForeground;
    bool draging, moving;
    bool overY, cornerSelected;

    #region Enums
    enum Side
    {
        Top, Right, Bottom, Left
    }
    enum Corner
    {
        TR, TL, BR, BL
    }
    #endregion

    //Current hover selection
    Corner corner;
    Side side;

    //Mouse data
    Vector2 mouseDelta;
    Vector3 prevMousePos;


    [Header("Cursor Images")]
    [SerializeField] Texture2D mouseLeftRight;
    [SerializeField]Texture2D mouseTopBottom, mouseDiagonal, mouseDiagonalMirror, mouseMove;

    

    [Header("Mouse Size")]
    [SerializeField] Vector2 mouseDimensions;

    //Drag mouse data
    Texture2D currentCursor;
    Vector2 mouseImagePos;
    bool defaultMouse = true;
    
 

    void Awake()
    {
        //Assign Singleton Instance
        main = this; 
    }
    // Start is called before the first frame update
    void Start()
    {
        //Set intial values
        defaultMouse = true;
        moving = false;
        draging = false;
        canvasDimensions.y = Camera.main.pixelHeight;
        canvasDimensions.x = Camera.main.pixelWidth;

        //Assign instances
        grapher = transform.Find("Background").Find("Main Graph Area").GetComponent<Grapher>();
        backgroundPanel = transform.Find("Background").GetComponent<RectTransform>();

        //Set panel size and position
        backgroundPanel.sizeDelta = initialCanvasDimensions;
        backgroundPanel.anchoredPosition = Vector2.zero;
    }

    /// <summary>
    /// Find whether the mouse is over a side and set the mouse cursor to the right design
    /// </summary>
    public void CheckDrag()
    {
        //Set mousePos to where (0,0) is int he centre
        Vector2 mousePos = Input.mousePosition;
        mousePos -= new Vector2(canvasDimensions.x / 2, canvasDimensions.y / 2);

        //Cursor states
        bool overLeft, overRight, cornerSelected = false, overY = false;
        Corner corner = Corner.BL;

        //Runs if the mouse is on the gap between the grey and white part
        if (mouseOverBackground && !mouseOverForeground)
        {
            //Check whether the mouse is on the back panel and close to the edge
            overLeft = mousePos.x < backgroundPanel.anchoredPosition.x - backgroundPanel.sizeDelta.x / 2 + grabThreshold;
            overRight = mousePos.x > backgroundPanel.anchoredPosition.x + backgroundPanel.sizeDelta.x / 2 - grabThreshold;

            if ((overLeft || overRight) && !draging)
                currentCursor = mouseLeftRight;


            //Top
            if (mousePos.y > backgroundPanel.anchoredPosition.y + backgroundPanel.sizeDelta.y / 2 - grabThreshold)
            {
                if (!draging)
                    currentCursor = mouseTopBottom;
                overY = true;
                //overTop = true;
                if (overLeft)
                {
                    if (!draging)
                        currentCursor = mouseDiagonal;
                    cornerSelected = true;
                    corner = Corner.TL;
                }
                else if (overRight)
                {
                    if (!draging)
                        currentCursor = mouseDiagonalMirror;
                    cornerSelected = true;
                    corner = Corner.TR;
                }

            }
            //Bottom
            else if (mousePos.y < backgroundPanel.anchoredPosition.y - backgroundPanel.sizeDelta.y / 2 + grabThreshold)
            {
                if (!draging)
                    currentCursor = mouseTopBottom;
                overY = true;
                //overBottom = true;
                if (overLeft)
                {
                    if (!draging)
                        currentCursor = mouseDiagonalMirror;
                    cornerSelected = true;
                    corner = Corner.BL;
                }
                else if (overRight)
                {
                    if (!draging)
                        currentCursor = mouseDiagonal;
                    cornerSelected = true;
                    corner = Corner.BR;
                }
            };

            if (Input.GetMouseButtonDown(0))
            {
                print("Start Dragging");
                this.cornerSelected = cornerSelected;
                this.overY = overY;
                this.corner = corner;

                if(!cornerSelected)
                {
                    if(overY)
                    {
                        if(mousePos.y > backgroundPanel.anchoredPosition.y)
                        {
                            side = Side.Top;
                        }
                        else
                        {
                            side = Side.Bottom;
                        }
                    }
                    else if(overLeft || overRight)
                    {
                        if(overLeft)
                        {
                            side = Side.Left;
                        }
                        else
                        {
                            side = Side.Right;
                        }

                    }

                }

                
                draging = overLeft || overRight || overY;
                moving = !draging;
            }

            if (!(overLeft || overRight || overY) && !draging)
                currentCursor = mouseMove;

            defaultMouse = false; 

        }
        else
        {
            if (!draging && !moving)
                defaultMouse = true;
        }
        

    }

    void ResizePanel()
    {
        float widthDelta = 0, heightDelta = 0;
        int xMulti = 1, yMulti = 1;
        if (cornerSelected)
        {
            switch (corner)
            {
                case Corner.TR:
                    widthDelta = mouseDelta.x;
                    heightDelta = mouseDelta.y;
                    xMulti = -1;
                    xMulti = -1;
                    break;
                case Corner.TL:
                    widthDelta = -mouseDelta.x;
                    heightDelta = mouseDelta.y;
                    yMulti = -1;
                    break;
                case Corner.BR:
                    widthDelta = mouseDelta.x;
                    heightDelta = -mouseDelta.y;
                    xMulti = -1;
                    break;
                case Corner.BL:
                    widthDelta = -mouseDelta.x;
                    heightDelta = -mouseDelta.y;
                    break;
            }

        }
        else if(overY)
        {
            //Top
            if(side == Side.Top)
            {
                heightDelta = mouseDelta.y;
                yMulti = -1;
            }
            //Bottom
            else
            {
                heightDelta = -mouseDelta.y;
            }
            mouseDelta *= new Vector2(0, 1);
        }
        else
        {
            //Right
            if (side == Side.Right)
            {
                widthDelta = mouseDelta.x;
                xMulti = -1;
            }
            else
            {
                widthDelta = -mouseDelta.x;
            }
            mouseDelta *= new Vector2(1, 0);
        }

        Vector2 prevSizeDelta = backgroundPanel.sizeDelta;

        backgroundPanel.sizeDelta += new Vector2(widthDelta, heightDelta);


        if (backgroundPanel.sizeDelta.x < minSize.x)
        {
            backgroundPanel.sizeDelta = new Vector2(minSize.x, backgroundPanel.sizeDelta.y);
            mouseDelta.x = (prevSizeDelta.x - minSize.x) * xMulti;
        }
        if (backgroundPanel.sizeDelta.y < minSize.y)
        {

            backgroundPanel.sizeDelta = new Vector2(backgroundPanel.sizeDelta.x, minSize.y);
            mouseDelta.y = (prevSizeDelta.y - minSize.y) * yMulti;
        }

        backgroundPanel.anchoredPosition += mouseDelta / 2;

        //Debug.Log("Yes");

        grapher.AdjustGraph();
    }

    void MovePanel()
    {
        backgroundPanel.anchoredPosition += mouseDelta;
    }

    // Update is called once per frame
    void Update()
    {
        if (!defaultMouse)
            Cursor.visible = false;
        else
            Cursor.visible = true;

        mouseImagePos = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
        Vector3 mousePos = Input.mousePosition;

        if (mousePos.x > Screen.width - edgeDragThreshold)
            mousePos.x = Screen.width - edgeDragThreshold;
        else if (mousePos.x < edgeDragThreshold)
            mousePos.x = edgeDragThreshold;

        if (mousePos.y > Screen.height - edgeDragThreshold)
            mousePos.y = Screen.height - edgeDragThreshold;
        else if (mousePos.y < edgeDragThreshold)
            mousePos.y = edgeDragThreshold;

        mouseDelta = mousePos - prevMousePos;
        CheckDrag();

        if((moving || draging) && !Input.GetMouseButton(0))
        {
            draging = false;
            moving = false;
            defaultMouse = true;
        }
        else if(draging)
        {
            ResizePanel();
        }
        else if(moving)
        {
            MovePanel();
        }
        prevMousePos = mousePos;
    }



    //bool GetOver()
    //{
    //    Ray ray = Camera.main.ScreenPointToRay(mousePos);
    //    RaycastHit hit;
    //    if (Physics.Raycast(ray, out hit, 100))
    //    {
    //        if (hit.transform.gameObject == backgroundPanel.gameObject)
    //        {
    //            return true;
    //        }
    //        Debug.Log(hit.transform.gameObject);
    //    }
    //    else
    //    {
    //    }
    //    return false;
    //}

    public void MouseEnterBackground()
    {
        Debug.Log("Enter");
        mouseOverBackground = true;
    }

    public void MouseExitBackground()
    {
        Debug.Log("Exit");
        mouseOverBackground = false;
    }

    public void MouseEnterForeground()
    {
        mouseOverForeground = true;
    }
    public void MouseExitForeground()
    {
        mouseOverForeground = false;
    }

    public void Close()
    {
        Statistics.main.graphsOn[(int)transform.Find("Background").Find("Main Graph Area").GetComponent<Grapher>().representing] = false;
        Statistics.main.graphs[(int)transform.Find("Background").Find("Main Graph Area").GetComponent<Grapher>().representing] = null;
        defaultMouse = true;
        Cursor.visible = true;
        Destroy(transform.parent.gameObject);
        Destroy(this);
    }

    void OnGUI()
    {
        if (!defaultMouse)
        {
            GUI.DrawTexture(new Rect(mouseImagePos.x - (mouseDimensions.x / 2), mouseImagePos.y - (mouseDimensions.y / 2), mouseDimensions.x, mouseDimensions.y), currentCursor);
        }
    }

}
