from enum import Enum
from typing import Generic, TypeVar

from .Logging import getLogger

logger = getLogger()


# NOTE
# Severity refers to to the error's affect on the parser as a whole, rather than on the function itself
# If an error is non-fatal to the function that generated it, it should be declared but not return, which prints it to the screen
class ErrorSeverity(Enum):
    Fatal = 50  # Critical Error
    Error = 40  # Non-critical Error
    Warning = 30  # Warning


T = TypeVar("T")


class Result(Generic[T]):
    """
    Result class for error handling, similar to the Result enum in Rust.

    The `Err` and `Ok` variants are child types, rather than enum variants though. Another difference is that the error variant is necessarily packaged with a message and a severity, rather than being arbitrary.

    Since python3 has no match statements, use the `is_ok()` or `is_err()` function to check the variant, then `unwrap()` or `unwrap_err()` to get the value or error message and severity.

    ## Example
    ```py
    foo_result = foo()
    if foo_result.is_err() and foo_result.unwrap_err()[1] == ErrorSeverity.Fatal:
        return foo_result
    ```

    Please see the `Ok` and `Err` child class documentation for instructions on instantiating errors and ok-values respectively
    """

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
    """
    The non-error variant of the Result class. Contains the value of the happy path of the function. Return when the function has executed successfully.
    """

    value: T

    def __init__(self, value: T):
        self.value = value

    def __repr__(self) -> str:
        return f"Ok({self.value})"


class Err(Result[T]):
    """
    The error variant of the Result class.

    It contains an error message and severity, which is either Fatal, Error, or Warning, each corresponding to a logger severity level, Critical Error (50) and Warning (30) respectively.

    When an `Err` is instantiated, it is automatically logged in the current synthesis logfile.

    ## Examples
    If an error is fatal to the entire program (or the parent function), it should be returned and marked as Fatal:
    ```python
    return Err("Foo not found", ErrorSeverity.Fatal)
    ```

    If an error is fatal to the current function, it should be returned and marked as Error, as the parent could recover it:
    ```python
    return Err("Bar not found", ErrorSeverity.Error)
    ```

    If an error is not fatal to the current function, but ought to be logged, it should be marked as Warning and instantiated but not returned, as to not break control flow:
    ```python
    _: Err[T] = Err("Baz not found", ErrorSeverity.Warning)
    ```
    Note that the lattermost example will raise a warning if not explicitely typed
    """

    message: str
    severity: ErrorSeverity

    def __init__(self, message: str, severity: ErrorSeverity):
        self.message = message
        self.severity = severity

        self.write_error()

    def __repr__(self) -> str:
        return f"Err({self.message})"

    def write_error(self) -> None:
        logger.log(self.severity.value, self.message)
