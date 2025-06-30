from .Logging import getLogger
from enum import Enum
from typing import Generic, TypeVar

logger = getLogger()


# NOTE
# Severity refers to to the error's affect on the parser as a whole, rather than on the function itself
# If an error is non-fatal to the function that generated it, it should be declared but not return, which prints it to the screen
class ErrorSeverity(Enum):
    Fatal = 50  # Critical Error
    Warning = 30  # Warning


T = TypeVar("T")


class Result(Generic[T]):
    def is_ok(self) -> bool:
        return isinstance(self, Ok)

    def is_err(self) -> bool:
        return isinstance(self, Err)

    def unwrap(self) -> T:
        if self.is_ok():
            return self.value  # type: ignore
        raise Exception(f"Called unwrap on Err: {self.message}")  # type: ignore

    def unwrap_err(self) -> tuple[str, ErrorSeverity]:
        if self.is_err():
            return (self.message, self.severity)  # type: ignore
        raise Exception(f"Called unwrap_err on Ok: {self.value}")  # type: ignore


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

        self.write_error()

    def __repr__(self):
        return f"Err({self.message})"

    def write_error(self) -> None:
        # Figure out how to integrate severity with the logger
        logger.log(self.severity.value, self.message)
