using System.IO;
using Inventor;
using System.Diagnostics;
using Asset = Inventor.Asset;
using Color = Inventor.Color;

namespace SynthesisInventorGltfExporter.Unused
{
    public class AppearanceDebugging
    {
        private static void PrintAssetLibrary(Application application)
        { // TODO: Move debugging methods to separate class
            using (StreamWriter file = new StreamWriter(@"C:\temp\InvAppearances.csv"))
            {
                file.Write("Appearance,");
                file.Write("DisplayName,");
                file.Write("HasTexture,");
                file.Write("IsReadOnly,");
                file.Write("Name,");
                file.WriteLine();

                foreach (AssetLibrary assetLib in application.AssetLibraries)
                {
                    // file.Write("Library" + assetLib.DisplayName);
                    // file.Write("  DisplayName: " + assetLib.DisplayName);
                    // file.Write("  FullFileName: " + assetLib.FullFileName);
                    // file.Write("  InternalName: " + assetLib.InternalName);
                    // file.Write("  IsReadOnly: " + assetLib.IsReadOnly);

                    foreach (Asset appearance in assetLib.AppearanceAssets)
                    {
                        file.Write(appearance.DisplayName + ",");
                        file.Write(appearance.HasTexture + ",");
                        file.Write(appearance.IsReadOnly + ",");
                        file.Write(appearance.Name + ",");
                        file.WriteLine();
                        PrintAsset(appearance, file);
                    }
                }
            }
        }

        private static void PrintAsset(Asset appearance, StreamWriter file)
        {
            // file.Write("Appearance,");
            // file.Write("DisplayName,");
            // file.Write("Name,");
            file.Write(appearance.DisplayName+",");
            // file.Write("      HasTexture: " + appearance.HasTexture);
            // file.Write("      IsReadOnly: " + appearance.IsReadOnly);
            file.Write(appearance.Name+",");
            file.WriteLine();
            foreach (AssetValue o in appearance)
            {
                PrintAssetValue(o, file);
            }
            file.WriteLine();
            file.WriteLine();
        }

        private static void PrintAssetValue(AssetValue InValue, StreamWriter file)
        {
            
            file.Write(InValue.Name+",");
            file.Write(InValue.IsReadOnly+",");

            switch (InValue.ValueType)
            {
                case AssetValueTypeEnum.kAssetValueTypeBoolean:
                    file.Write("Boolean,");

                    var booleanValue = InValue as BooleanAssetValue;

                    file.Write(booleanValue.Value+",");
                    break;
                case AssetValueTypeEnum.kAssetValueTypeChoice:
                    file.Write("Choice,");

                    var choiceValue = InValue as ChoiceAssetValue;

                    file.Write(choiceValue.Value + ",");

                    string[] names = new string[]{};
                    string[] choices = new string[]{};
                    choiceValue.GetChoices(out names, out choices);
                    for (int i = 0; i < names.Length; i++)
                    {
                        file.Write(" " + names[i] + ":" + choices[i] + ",");
                    }
                    break;
                case AssetValueTypeEnum.kAssetValueTypeColor:
                    file.Write("Color,");

                    var colorValue = InValue as ColorAssetValue;

                    file.Write(colorValue.HasConnectedTexture + ",");
                    file.Write(colorValue.HasMultipleValues + ",");

                    if (!colorValue.HasMultipleValues)
                    {
                        file.Write(ColorString(colorValue.Value, file));
                    }
                    else
                    {
                        file.Write("Colors,");

                        var colors = colorValue.get_Values();

                        foreach (var color in colors)
                        {
                            file.Write(ColorString(color, file));
                        }
                    }
                    break;

                case AssetValueTypeEnum.kAssetValueTypeFilename:
                    file.Write("Filename,");

                    var filenameValue = InValue as FilenameAssetValue;

                    file.Write(filenameValue.Value + ",");
                    break;

                case AssetValueTypeEnum.kAssetValueTypeFloat:
                    file.Write("Float,");

                    var floatValue = InValue as FloatAssetValue;

                    file.Write(floatValue.Value + ",");
                    break;

                case AssetValueTypeEnum.kAssetValueTypeInteger:
                    file.Write("Integer,");

                    var integerValue = InValue as IntegerAssetValue;

                    file.Write(integerValue.Value + ",");
                    break;

                case AssetValueTypeEnum.kAssetValueTypeReference:
                    file.Write("Reference,");

                    var refType = InValue as ReferenceAssetValue;
                    break;

                case AssetValueTypeEnum.kAssetValueTypeString:
                    file.Write("String,");

                    var stringValue = InValue as StringAssetValue;

                    file.Write(stringValue.Value + ",");
                    break;

            }
            file.WriteLine();
        }

        private static string ColorString(Color color, StreamWriter file)
        {
            return "("+color.Red + " " + color.Green + " " + color.Blue + " " + color.Opacity+"),";
        }
        
        
        private void DebugPrintAppearance(Asset appearance)
        {
            Debug.Print("=========================+");
            Debug.Print(appearance.Name);
            Debug.Print(appearance.DisplayName);
            Debug.Print(appearance.CategoryName);
            
            foreach (AssetValue assetValue in appearance)
            {
                Debug.Print("--------------");
                Debug.Print(assetValue.Name);
                Debug.Print(assetValue.DisplayName);
                switch (assetValue.ValueType)
                {
                    case AssetValueTypeEnum.kAssetValueTypeColor:
                        var colorAssetValue = (ColorAssetValue) assetValue;
                        if (colorAssetValue.HasMultipleValues)
                        {
                            foreach (var value in colorAssetValue.get_Values())
                            {
                                Debug.Print(value.Red.ToString());
                                Debug.Print(value.Green.ToString());
                                Debug.Print(value.Blue.ToString());
                                Debug.Print(value.Opacity.ToString());
                            }
                        }
                        else
                        {
                            Debug.Print(colorAssetValue.Value.Red.ToString());
                            Debug.Print(colorAssetValue.Value.Green.ToString());
                            Debug.Print(colorAssetValue.Value.Blue.ToString());
                            Debug.Print(colorAssetValue.Value.Opacity.ToString());
                        }
                        break;
                    case AssetValueTypeEnum.kAssetValueTypeBoolean:
                        var booleanAssetValue = (BooleanAssetValue) assetValue;
                        Debug.Print(booleanAssetValue.Value.ToString());
                        break;
                    case AssetValueTypeEnum.kAssetValueTypeChoice:
                        var choiceAssetValue = (ChoiceAssetValue) assetValue;
                        Debug.Print(choiceAssetValue.Value);
                        break;
                    case AssetValueTypeEnum.kAssetValueTypeFloat:
                        var floatAssetValue = (FloatAssetValue) assetValue;
                        if (floatAssetValue.HasMultipleValues)
                        {
                            foreach (var value in floatAssetValue.get_Values())
                            {
                                Debug.Print(value.ToString());
                            }
                        }
                        else
                        {
                            Debug.Print(floatAssetValue.Value.ToString());
                        }
                        break;
                    case AssetValueTypeEnum.kAssetValueTypeInteger:
                        var integerAssetValue = (IntegerAssetValue) assetValue;
                        if (integerAssetValue.HasMultipleValues)
                        {
                            foreach (var value in integerAssetValue.get_Values())
                            {
                                Debug.Print(value.ToString());
                            }
                        }
                        else
                        {
                            Debug.Print(integerAssetValue.Value.ToString());
                        }
                        break;
                    case AssetValueTypeEnum.kAssetValueTypeString:
                        var stringAssetValue = (StringAssetValue) assetValue;
                        Debug.Print(stringAssetValue.Value);
                        break;
                }
            }
        }
    }
}