// Used for completely hiding elements in the page
function toggleElement(id) {
    if (document.getElementById(id).style.display === "none") {
        document.getElementById(id).style.display = "block";
    } else {
        document.getElementById(id).style.display = "none";
    }
}

// Used for hiding/showing elements in the following function
function setVisible(element, visible)
{
    element.style.display = visible ? 'inherit' : 'none';
}

// Gets an a single child element that has the class specified
function getElByClass(fieldset, className)
{
    return fieldset.getElementsByClassName(className)[0]
}