from ctypes import *
import os

_path = os.path.abspath("../../native/controller_sys/target/debug/controller_sys.dll")
_controller_sys = CDLL(_path)


def Test(value):
    error_code = c_int()
    error_message = c_char_p()
    error_data = c_char_p()

    ret: int = _controller_sys.Test(value, byref(error_code), byref(error_message), byref(error_data))

    error_message = None if error_message.value is None else error_message.value.decode("utf-8")
    error_data = None if error_data.value is None else error_data.value.decode("utf-8")

    return ret, error_code.value, error_message, error_data


def main():
    print("No errors:\n")
    val, error_code, error_message, error_data = Test(5)
    print(val)
    print(error_code)
    print(error_message)
    print(error_data)

    print("\nErrors:\n")
    val, error_code, error_message, error_data = Test(25)
    print(val)
    print(error_code)
    print(error_message)
    print(error_data)


main()
