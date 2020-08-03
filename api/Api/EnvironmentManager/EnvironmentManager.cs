using System;
using System.Collections.Generic;
using System.Linq;
using SynthesisAPI.EnvironmentManager.Components;
using SynthesisAPI.Modules;
using SynthesisAPI.Runtime;
using SynthesisAPI.Utilities;
using UnityEngine.PlayerLoop;

#nullable enable

namespace SynthesisAPI.EnvironmentManager
{
    /// <summary>
    /// ECS System
    /// </summary>
    public static class EnvironmentManager
    {
        private static AnyMap<Component> components = new AnyMap<Component>(); //dynamic mapping of components to Entity by index
        private static List<Entity> entities = new List<Entity>(); //Entities that are in environment 
        private static Stack<Entity> removed = new Stack<Entity>(); //deallocated Entities that still exist in entities null Entities

        private static readonly Entity NULL_ENTITY = 0;
        private const ushort BASE_GEN = 1; //no entity should have a generation of 0


        #region EntityManagement

        /// <summary>
        /// Create and allocate new entity
        /// </summary>
        /// <returns>new Entity</returns>
        public static Entity AddEntity()
        {
            Entity newEntity;
            if (removed.Count > 0) //allocates empty/deallocated spaces in entities
            {
                Entity oldEntity = removed.Pop();
                ushort oldEntityIndex = oldEntity.Index;
                ushort oldEntityGen = oldEntity.Gen;
                newEntity = oldEntity.SetGen((ushort)(oldEntityGen + 1));
                ApiProvider.AddEntityToScene(newEntity);
                entities[oldEntityIndex] = newEntity;
            }
            else //adds entity by increasing entity list size
            {
                newEntity = Entity.Create((ushort)entities.Count, BASE_GEN);
                ApiProvider.AddEntityToScene(newEntity);
                entities.Add(newEntity);
            }
            newEntity.AddComponent<Parent>();
            return newEntity;
        }

        /// <summary>
        /// Remove and deallocate entity
        /// </summary>
        /// <param name="entity">entity to be removed</param>
        /// <returns>if entity was removed - will only return false if entity does not exist/is not allocated</returns>
        public static bool RemoveEntity(this Entity entity)
        {
            if (!EntityExists(entity))
                return false;
            ApiProvider.RemoveEntityFromScene(entity);
            removed.Push(entity); //deallocate entity
            entities[entity.Index] = NULL_ENTITY; //invalidate components
            return true;
        }

        /// <summary>
        /// Check if entity exists in current context
        /// </summary>
        /// <param name="entity">entity to be checked</param>
        /// <returns>if entity equals the entity in its given index</returns>
        public static bool EntityExists(this Entity entity)
        {
            ushort entityIndex = entity.Index;
            return entityIndex < entities.Count && entities[entityIndex] == entity;
        }


        public static IEnumerable<Entity> GetEntitiesWhere(Func<Entity, bool> predicate)
        {
            return entities.Where(predicate);
        }

        #endregion

        #region ComponentManagement

        /// <summary>
        /// Get component of type, TComponent, that is associated with the given entity
        /// </summary>
        /// <typeparam name="TComponent">Component type</typeparam>
        /// <param name="entity">given entity</param>
        /// <returns>Component instance</returns>
        public static TComponent? GetComponent<TComponent>(this Entity entity) where TComponent : Component
        {
            return (TComponent?) GetComponent(entity, typeof(TComponent));
        }

        public static Component? GetComponent(this Entity entity, Type componentType)
        {
            if (IsComponent(componentType) && EntityExists(entity))
                return components.Get(entity.Index, entity.Gen, componentType);
            return null;
        }

        public static List<Component>? GetComponents(this Entity entity) => components.GetAll(entity.Index, entity.Gen);

        /// <summary>
        /// Set component of type, TComponent, to the given entity
        /// </summary>
        /// <param name="entity">given entity</param>
        /// <param name="component">instance of component to be set</param>
        public static Component? AddComponent(this Entity entity, Type componentType)
        {
            if (IsComponent(componentType) && EntityExists(entity))
            {
                Component? c = ApiProvider.AddComponentToScene(entity, componentType);
                c?.SetEntity(entity);
                components.Set(entity.Index, entity.Gen, c);
                return c;
            }
            return null;
        }

        public static TComponent? AddComponent<TComponent>(this Entity entity) where TComponent : Component
        {
            return (TComponent?) AddComponent(entity, typeof(TComponent));
        }

        private static void AddComponent<TComponent>(this Entity entity, TComponent component) where TComponent : Component
        {
            if (EntityExists(entity))
            {
                ApiProvider.AddComponentToScene(entity, component);
                component.SetEntity(entity);
                components.Set(entity.Index, entity.Gen, component);
            }
        }
        

        /// <summary>
        /// Remove component (sets component to null) of type, TComponent, from given entity
        /// </summary>
        /// <typeparam name="TComponent">Component Type</typeparam>
        /// <param name="entity">given entity</param>
        public static void RemoveComponent<TComponent>(this Entity entity) where TComponent : Component
        {
            RemoveComponent(entity, typeof(TComponent));
        }

        public static void RemoveComponent(this Entity entity, Type componentType)
        {
            if (IsComponent(componentType) && EntityExists(entity))
            {
                ApiProvider.RemoveComponentFromScene(entity, componentType);
                components.Remove(entity.Index, entity.Gen, componentType);
            }
        }

        private static bool IsComponent(Type type)
        {
            return type.IsSubclassOf(typeof(Component)) || type == typeof(Component);
        }

        public static IEnumerable<TComponent> GetComponentsWhere<TComponent>(Func<TComponent, bool> predicate) where TComponent : Component
        {
            List<TComponent> result = new List<TComponent>();
            foreach (var e in entities)
            {
                foreach (var c in components.GetAll(e.Index, e.Gen) ?? new List<Component>())
                {
                    if (c is TComponent tc && predicate(tc))
                    {
                        result.Add(tc);
                    }
                }

            }
            return result;
        }

        public static void AddBundle(this Entity entity, Bundle bundle)
        {
            if (EntityExists(entity)){
                foreach (Bundle b in bundle.ChildBundles)
                {
                    Entity e = AddEntity();
                    e.GetComponent<Parent>()!.Set(entity);
                    e.AddBundle(b);
                }
                foreach (Component c in bundle.Components)
                    entity.AddComponent(c);
            }
        }

        #endregion

        public static void Clear()
        {
            if (AppDomain.CurrentDomain.GetAssemblies()
                .Any(a => a.FullName.ToLowerInvariant().StartsWith("nunit.framework")))
            {
                components.Clear();
                entities.Clear();
                removed.Clear();
            }
            else
            {
                throw new Exception("Users are not allowed to clear the environment manager");
            }
        }
    }
}