from typing import *

def appendGetIndex(inList: List[any], value: any) -> int:
    inList.append(value)
    return len(inList) - 1