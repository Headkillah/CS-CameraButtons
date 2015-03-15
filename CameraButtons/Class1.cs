using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using ICities;
using ColossalFramework.UI;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace CameraButtons
{
   public class CameraButtonsMod : IUserMod
   { 
      public string Description
      {
         get { return "Camera Buttons"; }
      }

      public string Name
      {
         get { return "Adds buttons on the screen to move and rotate the camera."; }
      }
   }
   
   public class CameraButtonLoadingExtension : LoadingExtensionBase
   {
      public static UIPanel ThePanel;

      public static int ScaleX(int x)
      {
         int scaledX = x;

         // we intentionally use height here to retain aspect ratio
         scaledX = (int)((float)x * (float)Screen.width / 3440f);
         
         return scaledX;
      }

      public static int ScaleY(int y)
      {
         int scaledY = y;

         scaledY = (int)((float)y * (float)Screen.width / 3440);

         return scaledY;
      }

      public override void OnLevelLoaded(LoadMode mode)
      {
         DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Message, "Loading CameraButtons Extension");
         UIView uiv =  GameObject.FindObjectOfType<UIView>();

         try
         {
            CameraButtonBehaviour cbb = uiv.gameObject.AddComponent<CameraButtonBehaviour>();
            cbb.transform.parent = uiv.transform;

            // create a panel to hold buttons
            UIComponent panel_uic = uiv.AddUIComponent(typeof(UIPanel));
            panel_uic.transform.parent = uiv.transform;
            UIPanel panel = panel_uic.GetComponent<UIPanel>();

            // All sizes and positions use pixel screen point values that we scale ourselves (I tried using viewport and couldn't get it to translate to worldpoint properly)
            // values used are relative to my 3440x1440 res
            int butWid = 30;
            int butHt = 30;

            panel.width = ScaleX(7 * butWid);
            panel.height = ScaleY(3 * butHt);

            // transformposition wants a worldpoint
            GameObject gameObject = GameObject.FindGameObjectWithTag("MainCamera");
            if (gameObject != null)
            {
               Camera cam = gameObject.GetComponent<Camera>();

               if (cam != null)
               {
                  //Vector3 pos_sp = new Vector3(ScaleX((int)((-2 * Screen.width) - (6 * panel.width))), (-ScaleY(Screen.height)) + panel.height);
                  //Vector3 pos_wp = cam.ScreenToViewportPoint(pos_sp);

                  Vector3 pos = new Vector3();

                  if (cam.aspect < 1.35f) // 4:3
                  {
                     pos.x = -1.0f;
                     pos.y = -0.8f;
                  }
                  else if (cam.aspect < 1.62f) // 16:10
                  {
                     pos.x = -1.3f;
                     pos.y = -0.8f;
                  }
                  else if (cam.aspect < 1.9f) // 16:9
                  {
                     pos.x = -1.4f;
                     pos.y = -0.8f;
                  }
                  else // 21:9 or other
                  {
                     pos.x = -2.0f;
                     pos.y = -0.7f;
                  }

                  panel.transformPosition = pos;
               }
            }

            panel.backgroundSprite = "ButtonMenu";

            int buttonw = ScaleX(butWid);
            int buttonh = ScaleY(butHt);

            UIButton zoominbutton = CreateButton(panel, "\u25A1+", new Vector3(3f * buttonw, 0f * buttonh));
            UIButton zoomoutbutton = CreateButton(panel, "\u25A1-", new Vector3(3f * buttonw, 2f * buttonh));
            UIButton leftbutton = CreateButton(panel, "\u2190", new Vector3(0f * buttonw, 1f * buttonh));
            UIButton rightbutton = CreateButton(panel, "\u2192", new Vector3(2f * buttonw, 1f * buttonh));
            UIButton forwardbutton = CreateButton(panel, "\u2191", new Vector3(1f * buttonw, 0f * buttonh));
            UIButton backbutton = CreateButton(panel, "\u2193", new Vector3(1f * buttonw, 2f * buttonh));
            UIButton rotateleft = CreateButton(panel, "\u21B6", new Vector3(4f * buttonw, 1f * buttonh));
            UIButton rotateright = CreateButton(panel, "\u21B7", new Vector3(6f * buttonw, 1f * buttonh));
            UIButton rotateup = CreateButton(panel, "\u21E1", new Vector3(5f * buttonw, 0f * buttonh));
            UIButton rotatedown = CreateButton(panel, "\u21E3", new Vector3(5f * buttonw, 2f * buttonh));

            zoominbutton.eventMouseUp += ZoominButtonMouseUp;
            zoominbutton.eventMouseDown += ZoominButtonMouseDown;

            zoomoutbutton.eventMouseUp += ZoomoutButtonMouseUp;
            zoomoutbutton.eventMouseDown += ZoomoutButtonMouseDown;

            leftbutton.eventMouseUp += LeftButtonMouseUp;
            leftbutton.eventMouseDown += LeftButtonMouseDown;

            rightbutton.eventMouseUp += RightButtonMouseUp;
            rightbutton.eventMouseDown += RightButtonMouseDown;

            forwardbutton.eventMouseUp += ForwardButtonMouseUp;
            forwardbutton.eventMouseDown += ForwardButtonMouseDown;

            backbutton.eventMouseUp += BackButtonMouseUp;
            backbutton.eventMouseDown += BackButtonMouseDown;

            rotateleft.eventMouseUp += RotateLeftMouseUp;
            rotateleft.eventMouseDown += RotateLeftMouseDown;

            rotateright.eventMouseUp += RotateRightMouseUp;
            rotateright.eventMouseDown += RotateRightMouseDown;

            rotateup.eventMouseUp += RotateUpMouseUp;
            rotateup.eventMouseDown += RotateUpMouseDown;

            rotatedown.eventMouseUp += RotateDownMouseUp;
            rotatedown.eventMouseDown += RotateDownMouseDown;

            DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Message, "done onlevelloaded");

            ThePanel = panel;

            // call up
            base.OnLevelLoaded(mode);
         }
         catch(Exception ex)
         {
            DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Message, string.Format("CameraButtons exception: {0}", ex.Message));
         }
      }

      private UIButton CreateButton(UIPanel panel, string text, Vector3 pos)
      {
         UIComponent zo_uic = panel.AddUIComponent(typeof(UIButton));
         zo_uic.transform.parent = panel.transform;

         UIButton button = zo_uic.GetComponent<UIButton>();
         button.text = text;
         button.width = ScaleX(30);
         button.height = ScaleY(30);
         button.normalBgSprite = "ButtonMenu";
         button.disabledBgSprite = "ButtonMenuDisabled";
         button.hoveredBgSprite = "ButtonMenuHovered";
         button.focusedBgSprite = "ButtonMenuFocused";
         button.pressedBgSprite = "ButtonMenuPressed";
         button.textColor = new Color32(255, 255, 255, 255);
         button.disabledTextColor = new Color32(7, 7, 7, 255);
         button.hoveredTextColor = new Color32(7, 132, 255, 255);
         button.focusedTextColor = new Color32(255, 255, 255, 255);
         button.pressedTextColor = new Color32(30, 30, 44, 255);

         button.relativePosition = pos;

         return button;
      }

      private void ZoominButtonMouseDown(UIComponent component, UIMouseEventParameter eventParam)
      {
         //DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Message, "buttonmousedown");
         CameraButtonBehaviour.ZoomInActive = true;
      }

      private void ZoominButtonMouseUp(UIComponent component, UIMouseEventParameter eventParam)
      {
         //DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Message, "buttonmouseup");
         CameraButtonBehaviour.ZoomInActive = false;
      }

      private void ZoomoutButtonMouseDown(UIComponent component, UIMouseEventParameter eventParam)
      {
         //DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Message, "buttonmousedown");
         CameraButtonBehaviour.ZoomOutActive = true;
      }

      private void ZoomoutButtonMouseUp(UIComponent component, UIMouseEventParameter eventParam)
      {
         //DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Message, "buttonmouseup");
         CameraButtonBehaviour.ZoomOutActive = false;
      }

      private void LeftButtonMouseDown(UIComponent component, UIMouseEventParameter eventParam)
      {
         //DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Message, "buttonmousedown");
         CameraButtonBehaviour.LeftActive = true;
      }

      private void LeftButtonMouseUp(UIComponent component, UIMouseEventParameter eventParam)
      {
         //DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Message, "buttonmouseup");
         CameraButtonBehaviour.LeftActive = false;
      }

      private void RightButtonMouseDown(UIComponent component, UIMouseEventParameter eventParam)
      {
         //DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Message, "buttonmousedown");
         CameraButtonBehaviour.RightActive = true;
      }

      private void RightButtonMouseUp(UIComponent component, UIMouseEventParameter eventParam)
      {
         //DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Message, "buttonmouseup");
         CameraButtonBehaviour.RightActive = false;
      }

      private void ForwardButtonMouseDown(UIComponent component, UIMouseEventParameter eventParam)
      {
         //DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Message, "buttonmousedown");
         CameraButtonBehaviour.ForwardActive = true;
      }

      private void ForwardButtonMouseUp(UIComponent component, UIMouseEventParameter eventParam)
      {
         //DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Message, "buttonmouseup");
         CameraButtonBehaviour.ForwardActive = false;
      }

      private void BackButtonMouseDown(UIComponent component, UIMouseEventParameter eventParam)
      {
         //DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Message, "buttonmousedown");
         CameraButtonBehaviour.BackActive = true;
      }

      private void BackButtonMouseUp(UIComponent component, UIMouseEventParameter eventParam)
      {
         //DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Message, "buttonmouseup");
         CameraButtonBehaviour.BackActive = false;
      }

      private void RotateLeftMouseDown(UIComponent component, UIMouseEventParameter eventParam)
      {
         //DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Message, "buttonmousedown");
         CameraButtonBehaviour.RotateLeftActive = true;
      }

      private void RotateLeftMouseUp(UIComponent component, UIMouseEventParameter eventParam)
      {
         //DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Message, "buttonmouseup");
         CameraButtonBehaviour.RotateLeftActive = false;
      }

      private void RotateRightMouseDown(UIComponent component, UIMouseEventParameter eventParam)
      {
         //DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Message, "buttonmousedown");
         CameraButtonBehaviour.RotateRightActive = true;
      }

      private void RotateRightMouseUp(UIComponent component, UIMouseEventParameter eventParam)
      {
         //DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Message, "buttonmouseup");
         CameraButtonBehaviour.RotateRightActive = false;
      }

      private void RotateUpMouseDown(UIComponent component, UIMouseEventParameter eventParam)
      {
         //DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Message, "buttonmousedown");
         CameraButtonBehaviour.RotateUpActive = true;
      }

      private void RotateUpMouseUp(UIComponent component, UIMouseEventParameter eventParam)
      {
         //DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Message, "buttonmouseup");
         CameraButtonBehaviour.RotateUpActive = false;
      }

      private void RotateDownMouseDown(UIComponent component, UIMouseEventParameter eventParam)
      {
         //DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Message, "buttonmousedown");
         CameraButtonBehaviour.RotateDownActive = true;
      }

      private void RotateDownMouseUp(UIComponent component, UIMouseEventParameter eventParam)
      {
         //DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Message, "buttonmouseup");
         CameraButtonBehaviour.RotateDownActive = false;
      }

   }

   public class CameraButtonBehaviour : MonoBehaviour
   {
      public static bool ZoomInActive = false;
      public static bool ZoomOutActive = false;
      public static bool LeftActive = false;
      public static bool RightActive = false;
      public static bool ForwardActive = false;
      public static bool BackActive = false;
      public static bool RotateLeftActive = false;
      public static bool RotateRightActive = false;
      public static bool RotateUpActive = false;
      public static bool RotateDownActive = false;

      private static float zoomstep = 40.0f;
      private static float movestep = 40.0f;
      private static float rotatestep = 5.0f;
      private static float rotateupdownstep = 5.0f;
      private static float maxup = 0.0f;
      private static float maxdown = 90.0f;

      public void LateUpdate()
      {
         if (ZoomInActive)
         {
            GameObject gameObject = GameObject.FindGameObjectWithTag("MainCamera");
            if (gameObject != null)
            {
               CameraController cameraController = gameObject.GetComponent<CameraController>();

               if ((cameraController.m_currentSize - zoomstep) <= 40)
                  cameraController.m_targetSize = 40;
               else
                  cameraController.m_targetSize = cameraController.m_currentSize - zoomstep;
            }
         }
         else if (ZoomOutActive)
         {
            GameObject gameObject = GameObject.FindGameObjectWithTag("MainCamera");
            if (gameObject != null)
            {
               CameraController cameraController = gameObject.GetComponent<CameraController>();

               if (cameraController.m_currentSize + zoomstep >= 4000)
                  cameraController.m_targetSize = 4000;
               else
                  cameraController.m_targetSize = cameraController.m_currentSize + zoomstep;
            }
         }
         else if (LeftActive)
         {
            if (CameraButtonLoadingExtension.ThePanel != null && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
            {
               // move the panel
               Vector3 pos = CameraButtonLoadingExtension.ThePanel.transformPosition;
               Vector3 apos = CameraButtonLoadingExtension.ThePanel.absolutePosition;

               pos.x -= .005f;

               CameraButtonLoadingExtension.ThePanel.transformPosition = pos;

               return;
            }

            GameObject gameObject = GameObject.FindGameObjectWithTag("MainCamera");
            if (gameObject != null)
            {
               CameraController cameraController = gameObject.GetComponent<CameraController>();

               Vector3 currentPos = cameraController.m_currentPosition;
               float cameraAngle = cameraController.m_currentAngle.x * Mathf.PI / 180f;

               Vector2 vector, vectorWithAngle;
               vector.x = movestep;
               vector.y = 0;

               vectorWithAngle.x = vector.x * Mathf.Cos(cameraAngle) - vector.y * Mathf.Sin(cameraAngle);
               vectorWithAngle.y = vector.x * Mathf.Sin(cameraAngle) + vector.y * Mathf.Cos(cameraAngle);

               Vector3 targetPos = currentPos;
               targetPos.x -= vectorWithAngle.x;
               targetPos.z += vectorWithAngle.y;

               cameraController.m_targetPosition = targetPos;
            }
         }
         else if (RightActive)
         {
            if (CameraButtonLoadingExtension.ThePanel != null && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
            {
               // move the panel
               Vector3 pos = CameraButtonLoadingExtension.ThePanel.transformPosition;
               Vector3 apos = CameraButtonLoadingExtension.ThePanel.absolutePosition;

               pos.x += .005f;

               CameraButtonLoadingExtension.ThePanel.transformPosition = pos;

               return;
            }

            GameObject gameObject = GameObject.FindGameObjectWithTag("MainCamera");
            if (gameObject != null)
            {
               CameraController cameraController = gameObject.GetComponent<CameraController>();

               Vector3 currentPos = cameraController.m_currentPosition;
               float cameraAngle = cameraController.m_currentAngle.x * Mathf.PI / 180f;

               Vector2 vector, vectorWithAngle;
               vector.x = -movestep;
               vector.y = 0;

               vectorWithAngle.x = vector.x * Mathf.Cos(cameraAngle) - vector.y * Mathf.Sin(cameraAngle);
               vectorWithAngle.y = vector.x * Mathf.Sin(cameraAngle) + vector.y * Mathf.Cos(cameraAngle);

               Vector3 targetPos = currentPos;
               targetPos.x -= vectorWithAngle.x;
               targetPos.z += vectorWithAngle.y;

               cameraController.m_targetPosition = targetPos;
            }
         }
         else if (ForwardActive)
         {
            if (CameraButtonLoadingExtension.ThePanel != null && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
            {
               // move the panel
               Vector3 pos = CameraButtonLoadingExtension.ThePanel.transformPosition;
               Vector3 apos = CameraButtonLoadingExtension.ThePanel.absolutePosition;

               pos.y += .005f;

               CameraButtonLoadingExtension.ThePanel.transformPosition = pos;

               return;
            }

            GameObject gameObject = GameObject.FindGameObjectWithTag("MainCamera");
            if (gameObject != null)
            {
               CameraController cameraController = gameObject.GetComponent<CameraController>();

               Vector3 currentPos = cameraController.m_currentPosition;
               float cameraAngle = cameraController.m_currentAngle.x * Mathf.PI / 180f;

               Vector2 vector, vectorWithAngle;
               vector.x = 0;
               vector.y = movestep;

               vectorWithAngle.x = vector.x * Mathf.Cos(cameraAngle) - vector.y * Mathf.Sin(cameraAngle);
               vectorWithAngle.y = vector.x * Mathf.Sin(cameraAngle) + vector.y * Mathf.Cos(cameraAngle);

               Vector3 targetPos = currentPos;
               targetPos.x -= vectorWithAngle.x;
               targetPos.z += vectorWithAngle.y;

               cameraController.m_targetPosition = targetPos;
            }
         }
         else if (BackActive)
         {
            if (CameraButtonLoadingExtension.ThePanel != null && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
            {
               // move the panel
               Vector3 pos = CameraButtonLoadingExtension.ThePanel.transformPosition;
               Vector3 apos = CameraButtonLoadingExtension.ThePanel.absolutePosition;

               pos.y -= .005f;

               CameraButtonLoadingExtension.ThePanel.transformPosition = pos;

               return;
            }

            GameObject gameObject = GameObject.FindGameObjectWithTag("MainCamera");
            if (gameObject != null)
            {
               CameraController cameraController = gameObject.GetComponent<CameraController>();

               Vector3 currentPos = cameraController.m_currentPosition;
               float cameraAngle = cameraController.m_currentAngle.x * Mathf.PI / 180f;

               Vector2 vector, vectorWithAngle;
               vector.x = 0;
               vector.y = -movestep;

               vectorWithAngle.x = vector.x * Mathf.Cos(cameraAngle) - vector.y * Mathf.Sin(cameraAngle);
               vectorWithAngle.y = vector.x * Mathf.Sin(cameraAngle) + vector.y * Mathf.Cos(cameraAngle);

               Vector3 targetPos = currentPos;
               targetPos.x -= vectorWithAngle.x;
               targetPos.z += vectorWithAngle.y;

               cameraController.m_targetPosition = targetPos;
            }
         }
         else if (RotateLeftActive)
         {
            GameObject gameObject = GameObject.FindGameObjectWithTag("MainCamera");
            if (gameObject != null)
            {
               CameraController cameraController = gameObject.GetComponent<CameraController>();
               Vector2 currentAngle = cameraController.m_currentAngle;
               cameraController.m_targetAngle = new Vector2(currentAngle.x - rotatestep, 0);
            }
         }
         else if (RotateRightActive)
         {
            GameObject gameObject = GameObject.FindGameObjectWithTag("MainCamera");
            if (gameObject != null)
            {
               CameraController cameraController = gameObject.GetComponent<CameraController>();
               Vector2 currentAngle = cameraController.m_currentAngle;
               cameraController.m_targetAngle = new Vector2(currentAngle.x + rotatestep, 0);
            }
         }
         else if (RotateUpActive)
         {
            GameObject gameObject = GameObject.FindGameObjectWithTag("MainCamera");
            if (gameObject != null)
            {
               CameraController cameraController = gameObject.GetComponent<CameraController>();

               if (cameraController.m_currentAngle.y <= maxup)
               {
                  return;
               }
               if (cameraController.m_currentAngle.y >= maxdown)
               {
                  cameraController.m_targetAngle = new Vector2(cameraController.m_currentAngle.x, maxdown);
               }

               // What a COLOSSAL(tm) PITA this has been... still not working right
               // game is doing something nasty. if zoomed way out and you try to rotate camera up beyond some value it messes with it.  The 'some value' changes accoring to your current zoom factor.
               // zoomed in (m_currentSize ~40) no problems
               // zoomed out (m_currentSize approaching 4000, max currentAngle allowed is ~55
               // 1% degradation per 100 units of size
               float degredation_percent = ((cameraController.m_currentSize - 40) / 65);  // approx 0% deg at 40, 36% deg at 4000... seems right
               float fudged = maxdown - (maxdown * (1f-(degredation_percent / 100f))); // this is as far towards zero as we can go safely

               //DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Message, string.Format("degrper: {0} fudged: {1} curry: {2}", degredation_percent, fudged, cameraController.m_currentAngle.y));

               //if (cameraController.m_currentAngle.y < fudged)
               //{
               //   cameraController.m_targetAngle = new Vector2(cameraController.m_currentAngle.x, fudged); // jump right to it
               //}

               if (cameraController.m_currentAngle.y > fudged) 
               {
                  //DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Message, string.Format("current size: {0}", cameraController.m_currentSize.ToString()));
                  cameraController.m_targetAngle = new Vector2(cameraController.m_currentAngle.x, cameraController.m_currentAngle.y - rotateupdownstep);
               }
            }
         }
         else if (RotateDownActive)
         {
            GameObject gameObject = GameObject.FindGameObjectWithTag("MainCamera");
            if (gameObject != null)
            {
               CameraController cameraController = gameObject.GetComponent<CameraController>();

               if (cameraController.m_currentAngle.y <= maxup)
               {
                  cameraController.m_targetAngle = new Vector2(cameraController.m_currentAngle.x, maxup);
               }
               if (cameraController.m_currentAngle.y >= maxdown)
               {
                  return;
               }

               //DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Message, string.Format("RotateDown: {0}", currentAngle.ToString()));

               if (cameraController.m_currentAngle.y < maxdown)
               {
                  //DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Message, string.Format("current size: {0}", cameraController.m_currentSize.ToString()));

                  cameraController.m_targetAngle = new Vector2(cameraController.m_currentAngle.x, cameraController.m_currentAngle.y + rotateupdownstep);
               }
            }
         }
      }
   }
}
