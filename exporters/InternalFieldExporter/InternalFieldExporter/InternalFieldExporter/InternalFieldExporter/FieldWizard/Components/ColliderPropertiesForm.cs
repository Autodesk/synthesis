namespace InternalFieldExporter.FieldWizard
{
    public interface ColliderPropertiesForm
    {
        /// <summary>
        /// Used to get a PropertySetCollider from information contained in the control
        /// </summary>
        /// <returns></returns>
        PropertySet.PropertySetCollider GetCollider();
    }
}
