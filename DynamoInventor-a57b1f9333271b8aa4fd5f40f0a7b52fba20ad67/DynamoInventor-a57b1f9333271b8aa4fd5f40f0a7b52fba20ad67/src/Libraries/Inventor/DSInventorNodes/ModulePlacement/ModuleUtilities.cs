using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InventorLibrary.ModulePlacement
{
    internal class ModuleUtilities
    {
        internal static List<Tuple<string, int, int, byte[]>> ReferenceKeysSorter(List<Tuple<string, int, int, byte[]>> allKeys, Tuple<string, int, int, byte[]> newKey)
        {
            if (allKeys !=null)
            {
                //A couple of scenarios could be possible if we are here.
                //1. An object at a particular moduleNumber/constrainIndex combination that existed before was 
                //   deleted.  In other words, there was a match for moduleNumber/constraintIndex and the object 
                //   types were the same, but the reference key failed to bind.  In this case we should replace
                //   the index that matches type,  moduleNumber, and constraintIndex with this newKey.  However, 
                //   if newKeys byte[] is the same as the old one, something is fucked.
                List<Tuple<string, int, int, byte[]>> matchedKeyAndTheRest = allKeys.SkipWhile(p => p.Item2 != newKey.Item2)
                                                                                    .SkipWhile(q => q.Item3 != newKey.Item3).ToList();
                int matchedKeyIndex = allKeys.Count - matchedKeyAndTheRest.Count;

                if (matchedKeyAndTheRest.Count > 0)//matchedKey.Item4 != newKey.Item4
                {
                    Tuple<string, int, int, byte[]> matchedKey = matchedKeyAndTheRest[0];
                    allKeys.RemoveAt(matchedKeyIndex);
                    allKeys.Insert(matchedKeyIndex, newKey);
                    return allKeys;
                }

                //2. The moduleNumber exists, but this is a new constraintIndex.
                else if (matchedKeyAndTheRest.Count == 0)
                {
                    List<Tuple<string, int, int, byte[]>> moduleKeysAndTheRest = allKeys.SkipWhile(p => p.Item2 != newKey.Item2).ToList();
                    int moduleStartIndex = allKeys.Count - moduleKeysAndTheRest.Count;
                    List<Tuple<string, int, int, byte[]>> moduleKeys = moduleKeysAndTheRest.Where(p => p.Item2 == newKey.Item2).ToList();
                    if (moduleKeys.Count == 0)
                    {
                        allKeys.Add(newKey);
                        return allKeys;
                    }
                    else if (newKey.Item3 - moduleKeys.Last().Item3 == 1)
                    {
                        allKeys.Insert(moduleStartIndex + moduleKeys.Count, newKey);
                        return allKeys;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }
    }
}
