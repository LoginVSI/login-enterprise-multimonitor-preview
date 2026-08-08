// TARGET:wordpad
// START_IN:
using System;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;
using LoginPI.Engine.ScriptBase;
using LoginPI.Engine.ScriptBase.Components;
using Interop.UIAutomationClient;
/*
This script demonstrates the use of Native UIAutomation in our engine scripting language
1- Cread the state of a radio button
*/
public class NativeAutomation : ScriptBase
{
    private readonly CUIAutomation8 automation = new CUIAutomation8();
    void Execute() 
    {
        START();
        Wait(2); 
        CheckRulerState();
        STOP();
    }
    
    void CheckRulerState()
    {
        Log("##### Checkbox state example");
        MainWindow.FindControl(className : "TabItem", title : "View").Click();
        Wait(1);
        // Move the mouse pointer out of the way, so we can see the checkbox toggles
        MainWindow.MoveMouseToCenter();
        var ruler = MainWindow.FindControl(className : "CheckBox", title : "Ruler");
        LogDetails(ruler);
        var checkBox = ruler.NativeAutomationElement.GetCurrentPattern(UIA_PatternIds.UIA_TogglePatternId) as IUIAutomationTogglePattern;
        if (checkBox == null)        
        {
            Log("This is not a checkbox");
            return;
        }
        Log($"Ruler is {checkBox.CurrentToggleState}");
        checkBox.Toggle();
        Wait(3);
        Log($"Ruler is {checkBox.CurrentToggleState}");
        checkBox.Toggle();
        Wait(3);
        Log($"Ruler is {checkBox.CurrentToggleState}");
        Wait(3);
        MainWindow.FindControl(className : "TabItem", title : "Home").Click();
        Wait(3);
        Log("##### Checkbox state example end");
        var copybox = MainWindow.FindControl(className : "Button", title : "Copy");
        LogDetails(copybox);
    }    
    
    void LogDetails(IWindow control)
     {
        Log("Checking patterns");
        // Example of how to inspect a control on a deepter level
        // First we check which patterns are available
        var patterns = EnumerateFields(typeof(UIA_PatternIds));
        foreach(var pattern in patterns)
        {
           var implementation = control.NativeAutomationElement.GetCurrentPattern(pattern.Value);
           if (implementation != null)
           {
             Log($"Pattern {pattern.Key} is supported");
           }
        }
        
        Log("Checking properties");
        // Example of how to inspect a control on a deepter level
        // First we check which patterns are available
        var properties = EnumerateFields(typeof(UIA_PropertyIds));
        foreach(var property in properties)
        {
            var value = control.NativeAutomationElement.GetCurrentPropertyValue(property.Value);
            if (value != null)
            {
                Log($"property {property.Key} has value : {value}");
            }
        }
    }

    private KeyValuePair<string, int>[] EnumerateFields(Type type)
    {
        FieldInfo[] fields = type.GetFields(
            BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

        List<KeyValuePair<string, int>> valuePairs = new List<KeyValuePair<string, int>>();

        foreach (FieldInfo field in fields)
        {
            if (!field.IsLiteral)
            {
               continue;
            }
            var obj = field.GetValue(null);
            if (obj.GetType().Name == "Int32")
            {
                valuePairs.Add(new KeyValuePair<string, int>(field.Name, (int)obj));
            }
            else
            {
                Log($"{field.Name} is {field.GetType().Name}");
            }
        }

        return valuePairs.ToArray();
    }

}


