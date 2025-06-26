from enum import Enum
from typing import Generic, TypeVar

# TODO Figure out if we need an error severity system
# Warnings are kind of useless if they break control flow anyways, so why not just replace warnings with writing to a log file and have errors break control flow
class ErrorSeverity(Enum):
    Fatal = 1
    Warning = 2

T = TypeVar('T')

class Result(Generic[T]):
    def is_ok(self) -> bool:
        return isinstance(self, Ok)

    def is_err(self) -> bool:
        return isinstance(self, Err)

    def unwrap(self) -> T:
        if self.is_ok():
            return self.value # type: ignore
        raise Exception(f"Called unwrap on Err: {self.error}") # type: ignore

    def unwrap_err(self) -> Error:
        if self.is_err():
            return self.error # type: ignore
        raise Exception("Called unwrap_err on Ok: {self.value}")

class Ok(Result[T]):
    value: T
    def __init__(self, value: T):
        self.value = value

    def __repr__(self):
        return f"Ok({self.value})"

class Err(Result[T]):
    message: str
    severity: ErrorSeverity
    def __init__(self, message: str, severity: ErrorSeverity):
        self.message = message
        self.severity = severity

    def __repr__(self):
        return f"Err({self.error})"
