// Used for hiding/showing elements in the following function
function setVisible(element, visible)
{
    element.style.visibility = visible ? '' : 'hidden';
}

// Gets an a single child element that has the class specified
function getElByClass(fieldset, className)
{
    return fieldset.getElementsByClassName(className)[0]
}
