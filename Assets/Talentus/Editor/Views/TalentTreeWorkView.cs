using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace com.cygnusprojects.TalentTree.Editor
{
	public class TalentTreeWorkView : TalentTreeViewBase
    {
        #region Variables
        Vector2 mousePos;
        int deleteNodeID = -1;
        bool isPanYset = false;
        Rect origRect;
        public Rect WorkSpace;
		#endregion

		#region Implementation
		public TalentTreeWorkView() : base("Work View") { }

        public override void UpdateView(Rect editorRect, Rect percentageRect, Event e, TalentTreeGraph curTree)
        {
            Color requiredConnectionColor = TalentTreeUtils.requiredConnectionColor;
            Color optionalConnectionColor = TalentTreeUtils.optionalConnectionColor;

            base.UpdateView(editorRect, percentageRect, e, curTree);
            origRect = viewRect;

            if (!isPanYset)
            {
                isPanYset = true;
                if (curTree != null)
                {
                    curTree.PanY = toolBarHeight;
                }
            }

            if (curTree != null)
            {
                GUI.BeginGroup(new Rect(curTree.PanX, curTree.PanY, 100000, 100000));
            }

            //GUI.Box(viewRect, viewTitle, viewSkin.GetStyle("ViewBG"));
            viewRect = new Rect(0, 0, 100000, 100000);
            GUI.Box(viewRect, "", viewSkin.GetStyle("ViewBG"));

            // Draw grid         
            Color gridColor = new Color(25f / 255f, 25f / 255f, 25f / 255f);
            TalentTreeUtils.DrawGrid(viewRect, 10f, 0.25f, gridColor);
            gridColor = new Color(34f / 255f, 34f / 255f, 34f / 255f);
            TalentTreeUtils.DrawGrid(viewRect, 50f, 0.5f, gridColor);

            GUILayout.BeginArea(viewRect);
            if (curTree != null)
            {
                curTree.UpdateGraphGUI(e, viewRect, origRect, viewSkin, requiredConnectionColor, optionalConnectionColor);
            }
            GUILayout.EndArea();

            if (curTree != null)
            {
                GUI.EndGroup();
            }
            viewRect = origRect;

            ProcessEvents(e);
        }

        public override void ProcessEvents(Event e)
        {
            base.ProcessEvents(e);

            //if (viewRect.Contains(e.mousePosition))
            if (origRect.Contains(e.mousePosition))
            {
                //Debug.Log("Inside " + viewTitle);
                if (e.button == 0)
                {
                    if (e.type == EventType.MouseDown)
                    {
                        //Debug.Log("Left clicked in " + viewTitle);
                        if (curTree != null)
                        {
                            curTree.selectedConnection = null;
                            curTree.selectedNode = null;
                            curTree.showProperties = false;
                            if (curTree.connections.Count > 0)
                            {
                                for (int i = 0; i < curTree.connections.Count; i++)
                                {
                                    if (curTree.connections[i].isSelected)
                                    {
                                        curTree.selectedConnection = curTree.connections[i];
                                        curTree.showProperties = true;
                                        break;
                                    }
                                }
                            }

                            if (curTree.selectedConnection == null)
                            {
                                curTree.selectedNode = null;
                                curTree.showProperties = false;
                                if (curTree.talents.Count > 0)
                                {
                                    for (int i = 0; i < curTree.talents.Count; i++)
                                    {
                                        if (curTree.talents[i].isSelected)
                                        {
                                            GUI.FocusControl(null);
                                            curTree.selectedNode = curTree.talents[i];
                                            curTree.showProperties = true;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (e.type == EventType.MouseDrag)
                    {
                        if (curTree != null) // Make sure we have a tree
                        {
                            if (curTree.selectedNode == null) // and we are not having a node selected
                            {
                                if (curTree != null)
                                {
                                    curTree.PanX += e.delta.x;
                                    curTree.PanY += e.delta.y;

                                    if (curTree.PanX > 0) curTree.PanX = 0;
                                    if (curTree.PanY > toolBarHeight) curTree.PanY = toolBarHeight;

                                    //Debug.Log(string.Format("PanX {0} - PanY {1}", panX, panY));
                                }
                            }
                        }
                    }
                    if (e.type == EventType.MouseUp)
                    {
                        //Debug.Log("Mouse up in " + viewTitle);
                    }
                }
                if (e.button == 1)
                {
                    if (e.type == EventType.MouseDown)
                    {
                        mousePos = e.mousePosition;

                        // Correct with panning values
                        if (curTree != null)
                        {
                            mousePos.x = mousePos.x - curTree.PanX;
                            mousePos.y = mousePos.y - curTree.PanY;
                        }

                        bool overNode = false;
                        deleteNodeID = -1;
                        if (curTree != null)
                        {
                            if (curTree.talents.Count > 0)
                            {
                                for (int i = 0; i < curTree.talents.Count; i++)
                                {
                                    if (curTree.talents[i].nodeRect.Contains(mousePos))
                                    {
                                        overNode = true;
                                        deleteNodeID = i;
                                    }
                                }
                            }
                        }
                        if (!overNode)
                        {
                            ProcessContextMenu(e, 0);
                        }
                        else
                        {
                            ProcessContextMenu(e, 1);
                        }
                        
                    }
                }
            }
        }

        #endregion

        #region Utilities
        private void ProcessContextMenu(Event e, int contextID)
        {
            GenericMenu menu = new GenericMenu();
            if (contextID == 0)
            {
                menu.AddItem(new GUIContent("Create Tree"), false, ContextCallback, "0");
                menu.AddItem(new GUIContent("Load Tree"), false, ContextCallback, "1");
                if (curTree != null)
                {
                    menu.AddSeparator("");
                    menu.AddItem(new GUIContent("Unload Tree"), false, ContextCallback, "2");
                    menu.AddSeparator("");
                    menu.AddItem(new GUIContent("Add Talent (Skill)"), false, ContextCallback, "3");
                }
            }

            if (contextID == 1)
            {
                if (curTree != null)
                {
                    menu.AddItem(new GUIContent("Delete Node"), false, ContextCallback, "4");
                }
            }
            menu.ShowAsContext();
            e.Use();
        }

        private void ContextCallback(object obj)
        {
            switch (obj.ToString())
            {
                case "0":
                    TalentTreePopupWindow.InitNodePopup();
                    break;
                case "1":
                    TalentTreeUtils.LoadTree();
                    break;
                case "2":
                    TalentTreeUtils.UnloadTree();
                    break;
                case "3":
                    TalentTreeUtils.CreateNode(curTree, NodeType.Talent, mousePos);
                    break;
                case "4":
                    curTree.selectedNode = null;
                    TalentTreeConnectionUtils.DeleteConnectionsForNode(curTree, deleteNodeID);
                    TalentTreeUtils.DeleteNode(deleteNodeID, curTree);
                    break;
                default:
                    break;
            }
        }
        #endregion
    }
}
