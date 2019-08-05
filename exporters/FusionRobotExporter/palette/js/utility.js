// Used for hiding/showing elements in the following function
function setVisible(element, visible)
{
    element.style.display = visible ? '' : 'none';
}

// Gets an a single child element that has the class specified
function getElByClass(fieldset, className)
{
    return fieldset.getElementsByClassName(className)[0]
}
