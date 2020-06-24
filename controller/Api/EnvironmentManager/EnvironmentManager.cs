using System;
using System.Collections.Generic;

//Entity is a 32 bit generational index
// 1st 16 bits represent index
// 2nd 16 bits represent generation
using Entity = System.UInt32; 

namespace SynthesisAPI.EnvironmentManager
{
    /// <summary>
    /// ECS System
    /// </summary>
    public class EnvironmentManager
    {
        AnyMap components; //dynamic mapping of components to Entity by index
        List<Entity> entities; //Entities that are in environment 
        Stack<Entity> removed; //deallocated Entities that still exist in entities null Entities

        const Entity NULL_ENTITY = 0; 
        const ushort BASE_GEN = 1; //no entity should have a generation of 0

        public EnvironmentManager()
        {
            entities = new List<Entity>();
            removed = new Stack<Entity>();
            components = new AnyMap();
        }

        public Entity AddEntity()
        {
            Entity newEntity;
            if (removed.Count > 0) //allocates empty/deallocated spaces in entities
            {
                Entity oldEntity = removed.Pop();
                ushort oldEntityIndex = EntityManager.GetIndex(oldEntity);
                ushort oldEntityGen = EntityManager.GetGen(oldEntity);
                newEntity = EntityManager.SetGen(oldEntity, (ushort)(oldEntityGen+1)) ;
                entities[oldEntityIndex] = newEntity;
            }
            else //adds entity by increasing entity list size
            {
                newEntity = EntityManager.CreateEntity((ushort)entities.Count, BASE_GEN);
                entities.Add(newEntity);
            }
            return newEntity;
        }

        public bool RemoveEntity(Entity entity)
        {
            if (!EntityExists(entity))
                return false;
            removed.Push(entity); //deallocate entity
            entities[EntityManager.GetIndex(entity)] = NULL_ENTITY; //invalidate components
            return true;
        }

        public T GetComponent<T>(Entity entity)
        {
            if (EntityExists(entity))
                return components.Get<T>(entity);
            return default;
        }

        public void SetComponent<T>(Entity entity, T component)
        {
            if (EntityExists(entity)) components.Set<T>(entity, component);
        }

        public void RemoveComponent<T>(Entity entity)
        {
            if(EntityExists(entity)) components.Remove<T>(entity);
        }

        public bool EntityExists(Entity entity)
        {
            ushort entityIndex = EntityManager.GetIndex(entity);
            return entityIndex < entities.Count && entities[entityIndex] == entity;
        }

        /// <summary>
        /// Dynamic mapping of any reference type to its corresponding GenIndexArray
        /// </summary>
        class AnyMap
        {
            public Dictionary<Type, object> componentDict;
            public AnyMap()
            {
                componentDict = new Dictionary<Type, object>();
            }
            public void Set<T>(Entity entity, T val)
            {
                object arr;
                if (componentDict.TryGetValue(typeof(T), out arr))
                    ((GenIndexArray<T>)arr).Set(entity, val);
                else
                {
                    componentDict.Add(typeof(T), new GenIndexArray<T>());
                    Set<T>(entity, val);
                }
            }
            public void Remove<T>(Entity entity)
            {
                object arr;
                if (componentDict.TryGetValue(typeof(T), out arr))
                    ((GenIndexArray<T>)arr).Set(entity, default);
            }
            public T Get<T>(Entity entity)
            {
                object arr;
                if (componentDict.TryGetValue(typeof(T), out arr))
                    return ((GenIndexArray<T>)arr).Get(entity);
                else
                    return default;
            }
            /// <summary>
            /// Maps Entity to its component by its index : component index in entries is the same as Entity index
            /// </summary>
            /// <typeparam name="T">Component Type i.e. Mesh</typeparam>
            class GenIndexArray<T>
            {
                List<Entry> entries;

                public GenIndexArray()
                {
                    entries = new List<Entry>();
                }

                struct Entry
                {
                    public Entry(T val, ulong gen)
                    {
                        Val = val;
                        Gen = gen;
                    }
                    public T Val { get; }
                    public ulong Gen { get; }
                }

                public void Set(Entity entity, T val)
                {
                    ushort entityIndex = EntityManager.GetIndex(entity);
                    ushort entityGen = EntityManager.GetGen(entity);
                    if (entityIndex < entries.Count)
                        entries[entityIndex] = new Entry(val, entityGen);
                    else
                    {
                        //increase list size by populating "null" Entry so we can add value at index which is outside of bounds
                        for (int i = entries.Count; i < entityIndex; i++)
                            entries.Add(new Entry(default, 0)); //no Entity has gen of 0 so these entries are essentially null
                        entries.Add(new Entry(val, entityGen));
                    }   
                }

                public T Get(Entity entity)
                {
                    ushort entityIndex = EntityManager.GetIndex(entity);
                    ushort entityGen = EntityManager.GetGen(entity);
                    if (entityIndex >= entries.Count) return default; //prevents IndexOutOfBoundsException
                    Entry entry = entries[entityIndex];
                    //only get component if generations match - avoids having reallocated Entities point to the components of deallocated Entities
                    if (entry.Gen == entityGen) return entry.Val; 
                    return default;
                }
            }
        }

        /// <summary>
        /// Encapsulation of static methods to read and write to Entities
        /// </summary>
        class EntityManager
        {
            //first 16 bits
            public static ushort GetIndex(Entity entity)
            {
                return (ushort)(entity >> 16);
            }
            //last 16 bits
            public static ushort GetGen(Entity entity)
            {
                return (ushort)(entity & 65535);
            }
            public static Entity CreateEntity(ushort index, ushort gen)
            {
                return ((uint)index << 16) + gen;
            }
            public static Entity SetIndex(Entity entity, ushort index)
            {
                return ((uint)index << 16) + (entity & 65535);
            }
            public static Entity SetGen(Entity entity,ushort gen)
            {
                return (entity & 4294901760) + gen;
            }
        }
    }
}