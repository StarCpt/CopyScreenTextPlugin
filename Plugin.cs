using HarmonyLib;
using Sandbox.Game.Gui;
using Sandbox.Graphics.GUI;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VRage.Game.Components;
using VRage.Plugins;
using VRage.Utils;
using VRageMath;

namespace CopyScreenTextPlugin
{
    public class Plugin : IPlugin
    {
        public void Dispose()
        {

        }

        public void Init(object gameInstance)
        {
            new Harmony("CopyScreenTextPlugin").PatchAll(Assembly.GetExecutingAssembly());
        }

        public void Update()
        {

        }
    }

    [HarmonyPatch(typeof(MyGuiScreenText), "RecreateControls")]
    public class MyGuiScreenTextPostfix
    {
        private static MyGuiControlButton copyBtn;
        [HarmonyPostfix]
        public static void Postfix(MyGuiScreenText __instance, MyGuiControlButton ___m_okButton)
        {
            if (__instance is MyGuiScreenTextPanel)
                return;

            copyBtn = new MyGuiControlButton(
                new Vector2(___m_okButton.PositionX + 0.1f, ___m_okButton.PositionY),
                size: ___m_okButton.Size,
                text: new StringBuilder("Copy Text"),
                onButtonClick: action => CopyText(__instance));

            __instance.Controls.Add(copyBtn);

            //Move the ok button to the left a little for symmetry
            ___m_okButton.PositionX -= 0.1f;
        }

        public static void CopyText(MyGuiScreenText __instance)
        {
            try
            {
                AccessTools.Field(typeof(MyGuiScreenText), "m_enableEdit").SetValue(__instance, true);

                FieldInfo descriptionBoxField = AccessTools.Field(typeof(MyGuiScreenText), "m_descriptionBox");
                MyGuiControlMultilineText m_descriptionBox = __instance.Description;
                __instance.Controls.Remove(m_descriptionBox);
                Vector2 descSize = (Vector2)AccessTools.Field(typeof(MyGuiScreenText), "m_descSize").GetValue(__instance);

                Vector2 vector2_1 = new Vector2(0.0f, -0.3f);
                Vector2 position1 = new Vector2((float)(-descSize.X / 2.0), vector2_1.Y + 0.1f);
                Vector2 vector2_4 = new Vector2(0.005f, 0.0f);
                Vector2? offset = new Vector2?(position1 + vector2_4);
                m_descriptionBox = (MyGuiControlMultilineText)AccessTools.Method(typeof(MyGuiScreenText), "AddMultilineText").Invoke(__instance,
                    new object[] {
                    new Vector2?(descSize),
                    offset,
                    1f,
                    false,
                    MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP,
                    MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP });
                m_descriptionBox.Text = __instance.Description.Text;

                descriptionBoxField.SetValue(__instance, m_descriptionBox);

                copyBtn.Enabled = false;
            }
            catch (Exception e)
            {
                MyLog.Default.WriteLine(e);
                MyGuiSandbox.Show(new StringBuilder("Tell the plugin creator so he can fix it. Thanks. Send me the log too if possible."), MyStringId.GetOrCompute("Plugin Error!"));
            }
        }
    }
}
