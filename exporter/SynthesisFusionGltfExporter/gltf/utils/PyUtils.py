from typing import *

def appendGetIndex(list: List[any], value: any) -> int:
    list.append(value)
    return len(list) - 1