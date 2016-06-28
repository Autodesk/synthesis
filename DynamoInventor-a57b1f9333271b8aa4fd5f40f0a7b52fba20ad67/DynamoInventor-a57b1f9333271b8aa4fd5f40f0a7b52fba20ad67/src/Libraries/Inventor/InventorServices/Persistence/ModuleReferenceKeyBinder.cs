using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace InventorServices.Persistence
{
    public class ModuleReferenceKeyBinder : IModuleBinder
    {
        //private const string INVENTOR_TRACE_ID = "{097338D8-7FD3-42c5-9905-272147594D38}-INVENTOR";
        private const string INVENTOR_TRACE_ID = "{0459D869-0C72-447F-96D8-08A7FB92214B}-REVIT";
        private ISerializableModuleIdManager _idManager;
        private ISerializableId<List<Tuple<string, int, int, byte[]>>> _id;
        private IContextData _contextData;
        private IContextManager _contextManager;

        public ModuleReferenceKeyBinder(ISerializableModuleIdManager idManager, 
                                        ISerializableId<List<Tuple<string, int, int, byte[]>>> id,
                                        IContextData contextData,
                                        IContextManager contextManager)
        {
            _idManager = idManager;
            _id = id;
            _contextData = contextData;
            _contextManager = contextManager;
        }

        public IContextData ContextData
        {
            get { return _contextData; }
        }

        public IContextManager ContextManager
        {
            get { return _contextManager; }
        }

        public ISerializableId<T> GetObjectKey<T>()
        {
            throw new NotImplementedException();
        }

        public bool GetObjectFromTrace<T>(out T e)
        {
            ISerializableId<List<Tuple<string, int, int, byte[]>>> refKeys;
            if (_idManager.GetTraceData(INVENTOR_TRACE_ID, out refKeys) && refKeys != null)
            {
                var matchedData = refKeys.Id.Where(p => p.Item1 == typeof(T).ToString())
                                                                        .Where(q => q.Item2 == _contextData.Context.Item1)
                                                                        .Where(r => r.Item3 == _contextData.Context.Item2)
                                                                        .FirstOrDefault();
                if (matchedData != null && TryBindReferenceKey<T>(matchedData.Item4, out e))
                {
                    return true;
                }
                else
                {
                    e = default(T);
                    return false;
                }
            }
            else
            {
                e = default(T);
                return false;
            }
        }

        public void SetObjectForTrace<T>(dynamic inventorObject,
                                         Func<List<Tuple<string, int, int, byte[]>>,
                                                         Tuple<string, int, int, byte[]>,
                                                         List<Tuple<string, int, int, byte[]>>> referenceKeysEvaluator)
        {
            byte[] refKey = new byte[] { };

            //We need to check if there is anything it the slot.
            ISerializableId<List<Tuple<string, int, int, byte[]>>> refKeys;
            if (_idManager.GetTraceData(INVENTOR_TRACE_ID, out refKeys) && refKeys.Id.Count > 0)
            {
                inventorObject.GetReferenceKey(ref refKey, 0);
                Tuple<string, int, int, byte[]> refKeyTuple = new Tuple<string, int, int, byte[]>(typeof(T).ToString(),
                                                                                                    ContextData.Context.Item1,
                                                                                                    ContextData.Context.Item2,
                                                                                                    refKey);
                var modifiedKeys = referenceKeysEvaluator(refKeys.Id, refKeyTuple);
                refKeys.Id = modifiedKeys;
                _idManager.SetTraceData(INVENTOR_TRACE_ID, refKeys as ISerializable);
            }
            else
            {
                inventorObject.GetReferenceKey(ref refKey, 0);
                Tuple<string, int, int, byte[]> refKeyTuple = new Tuple<string, int, int, byte[]>(typeof(T).ToString(),
                                                                                                  ContextData.Context.Item1,
                                                                                                  ContextData.Context.Item2,
                                                                                                  refKey);
                _id.Id.Add(refKeyTuple);
                _idManager.SetTraceData(INVENTOR_TRACE_ID, _id as ISerializable);
            }
        }

        public bool TryBindReferenceKey<T>(byte[] key, out T e)
        {
            try
            {
                object outType = null;
                byte[] keyContextArray = new byte[] { };

                //TODO: This will not work with BRep objects.  Inventor doesn't care about the KeyContext for anything else.
                //KeyContext is a long.  May be good to have a different set of methods for BRep objects to avoid storing this 
                //additional information when it isn't needed.
                //
                T invObject = (T)_contextManager.BindingContextManager.BindKeyToObject(ref key, 0, out outType);
                e = invObject;
                return invObject != null;
            }

            catch
            {
                e = default(T);
                return false;
            }
        }
    }
}
