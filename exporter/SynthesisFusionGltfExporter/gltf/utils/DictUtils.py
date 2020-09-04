def dictDeleteEmptyKeys(dictionary, ignoreKeys):
    """
    Delete keys with the value ``None`` in a dictionary, recursively.

    This alters the input so you may wish to ``copy`` the dict first.

    Courtesy Chris Morgan and modified from:
    https://stackoverflow.com/questions/4255400/exclude-empty-null-values-from-json-serialization
    """
    for key, value in list(dictionary.items()):
        if value is None or (hasattr(value, '__iter__') and len(value) == 0):
            del dictionary[key]
        elif isinstance(value, dict):
            if key not in ignoreKeys:
                dictDeleteEmptyKeys(value, ignoreKeys)
        elif isinstance(value, list):
            for item in value:
                if isinstance(item, dict):
                    dictDeleteEmptyKeys(item, ignoreKeys)
    return dictionary  # For convenience