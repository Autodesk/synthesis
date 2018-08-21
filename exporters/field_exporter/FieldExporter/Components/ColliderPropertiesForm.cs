namespace FieldExporter.Components
{
    public interface ColliderPropertiesForm
    {
        /// <summary>
        /// Used for getting/settings a PropertySetCollider from information contained in the control.
        /// </summary>
        /// <returns></returns>
        PropertySet.PropertySetCollider Collider
        {
            get;
            set;
        }
    }
}
