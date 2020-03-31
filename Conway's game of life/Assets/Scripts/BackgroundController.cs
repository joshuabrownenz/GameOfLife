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
    /// Find whether the mouse is over a side and set the mouse cursor to the right design, as well as activating dragging or moving
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
                //Set to vertical cursor
                if (!draging)
                    currentCursor = mouseTopBottom;

                overY = true;


                //CHecks if it is over a corner
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
                //Sets to vertical cursor
                if (!draging)
                    currentCursor = mouseTopBottom;

                overY = true;

                //Checks if over corner
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

            //If mouse button is pressed, set current selection states to the global rather than glocal varibles
            if (Input.GetMouseButtonDown(0))
            {
                //Set varibles 
                this.cornerSelected = cornerSelected;
                this.overY = overY;
                this.corner = corner;


                if(!cornerSelected)
                {
                    //Sets the active side
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

                //dragging refers to resizing
                draging = overLeft || overRight || overY;

                //If it is not dragging then it is moving the graph
                moving = !draging;
            }

            //SIf not over edge then set cursur to the move icon
            if (!(overLeft || overRight || overY) && !draging)
                currentCursor = mouseMove;

            //Turn off normal mouse
            defaultMouse = false; 

        }
        //Else if not over a spot which allows dragging set it back to normal mouse
        else
        {
            if (!draging && !moving)
                defaultMouse = true;
        }
        

    }

    /// <summary>
    /// Resize panel based of drag and side/corner data
    /// </summary>
    void ResizePanel()
    {
        //Change to width and height
        float widthDelta = 0, heightDelta = 0;

        //Multipliers determine the direction the position change
        int xMulti = 1, yMulti = 1;

        //Set mulpliers for corners
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

        //If over top or bottom adjust multpliers
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

            //Cancel out x change
            mouseDelta *= new Vector2(0, 1);
        }

        //If over left or right adjust multipliers
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

            //Cancel out y change
            mouseDelta *= new Vector2(1, 0);
        }


        //Save for comparsion later
        Vector2 prevSizeDelta = backgroundPanel.sizeDelta;

        //Change by height and width deltas
        backgroundPanel.sizeDelta += new Vector2(widthDelta, heightDelta);

        //If smaller than min size, adjust to min size
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

        //Adjust position
        backgroundPanel.anchoredPosition += mouseDelta / 2;

        //Adjust ponts and text and dividers of graph
        grapher.AdjustGraph();
    }

    void Update()
    {
        //Hide the default cursor to been seen otherwise allow it to be seen
        if (!defaultMouse)
            Cursor.visible = false;
        else
            Cursor.visible = true;

        //Get mouse pos
        Vector3 mousePos = Input.mousePosition;

        //Set the postion of ther cursor
        mouseImagePos = new Vector2(mousePos.x, Screen.height - mousePos.y);

        //Keeps the graph on the screen - x axis
        if (mousePos.x > Screen.width - edgeDragThreshold)
            mousePos.x = Screen.width - edgeDragThreshold;
        else if (mousePos.x < edgeDragThreshold)
            mousePos.x = edgeDragThreshold;

        //Keeps the graph on the screen - x axis
        if (mousePos.y > Screen.height - edgeDragThreshold)
            mousePos.y = Screen.height - edgeDragThreshold;
        else if (mousePos.y < edgeDragThreshold)
            mousePos.y = edgeDragThreshold;

        //Calculate change in mouse position
        mouseDelta = mousePos - prevMousePos;

        //Check if graph and prepare to drag
        CheckDrag();

        //If draging or moving and has stopped
        if((moving || draging) && !Input.GetMouseButton(0))
        {
            //Go back to deafult settings
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
            //Move panel based of mouse delta
            backgroundPanel.anchoredPosition += mouseDelta;
        }
        prevMousePos = mousePos;
    }


    //Change states based of mouse entering or exiting background or foreground
    public void MouseEnterBackground()
    {
        mouseOverBackground = true;
    }
    public void MouseExitBackground()
    {
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

    //Closing a graph
    public void Close()
    {
        //Turn off graphs in the statistics object
        Statistics.main.graphsOn[(int)transform.Find("Background").Find("Main Graph Area").GetComponent<Grapher>().representing] = false;
        Statistics.main.graphs[(int)transform.Find("Background").Find("Main Graph Area").GetComponent<Grapher>().representing] = null;

        //Set mouse back to deafult
        defaultMouse = true;
        Cursor.visible = true;

        //Destroy the graph and this instance
        Destroy(transform.parent.gameObject);
        Destroy(this);
    }

    //Adjust changed mouse cursor position
    void OnGUI()
    {
        if (!defaultMouse)
        {
            GUI.DrawTexture(new Rect(mouseImagePos.x - (mouseDimensions.x / 2), mouseImagePos.y - (mouseDimensions.y / 2), mouseDimensions.x, mouseDimensions.y), currentCursor);
        }
    }

}
