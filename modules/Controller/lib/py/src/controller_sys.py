from ctypes import *
import os, time


class RpcError(Exception):
    def __init__(self, code, message, data):
        super().__init__(message + "\n" + data)
        self.code = code
        self.message = message
        self.data = data


class ControllerSys:
    def __init__(self):
        _path = os.path.abspath("../../native/controller_sys/target/debug/controller_sys.dll")
        self._controller_sys = CDLL(_path)

    def log_str(self, message, log_level):
        error_code = c_int()
        error_message = c_char_p()
        error_data = c_char_p()

        message = message.encode('utf-8')
        self._controller_sys.log_str(message, log_level, byref(error_code), byref(error_message), byref(error_data))

        error_code = error_code.value
        if error_code != 0:
            error_message = "" if error_message.value is None else error_message.value.decode("utf-8")
            error_data = "" if error_data.value is None else error_data.value.decode("utf-8")
            raise RpcError(error_code, error_message, error_data)

    def forward(self, channel, distance):
        error_code = c_int()
        error_message = c_char_p()
        error_data = c_char_p()

        self._controller_sys.forward(channel, c_double(distance), byref(error_code), byref(error_message), byref(error_data))

        error_code = error_code.value
        if error_code != 0:
            error_message = "" if error_message.value is None else error_message.value.decode("utf-8")
            error_data = "" if error_data.value is None else error_data.value.decode("utf-8")
            raise RpcError(error_code, error_message, error_data)

    def backward(self, channel, distance):
        error_code = c_int()
        error_message = c_char_p()
        error_data = c_char_p()

        self._controller_sys.backward(channel, c_double(distance), byref(error_code), byref(error_message), byref(error_data))

        error_code = error_code.value
        if error_code != 0:
            error_message = "" if error_message.value is None else error_message.value.decode("utf-8")
            error_data = "" if error_data.value is None else error_data.value.decode("utf-8")
            raise RpcError(error_code, error_message, error_data)

    def left(self, channel, distance):
        error_code = c_int()
        error_message = c_char_p()
        error_data = c_char_p()

        self._controller_sys.left(channel, c_double(distance), byref(error_code), byref(error_message), byref(error_data))

        error_code = error_code.value
        if error_code != 0:
            error_message = "" if error_message.value is None else error_message.value.decode("utf-8")
            error_data = "" if error_data.value is None else error_data.value.decode("utf-8")
            raise RpcError(error_code, error_message, error_data)

    def right(self, channel, distance):
        error_code = c_int()
        error_message = c_char_p()
        error_data = c_char_p()

        self._controller_sys.right(channel, c_double(distance), byref(error_code), byref(error_message), byref(error_data))

        error_code = error_code.value
        if error_code != 0:
            error_message = "" if error_message.value is None else error_message.value.decode("utf-8")
            error_data = "" if error_data.value is None else error_data.value.decode("utf-8")
            raise RpcError(error_code, error_message, error_data)

    def up(self, channel, distance):
        error_code = c_int()
        error_message = c_char_p()
        error_data = c_char_p()

        self._controller_sys.up(channel, c_double(distance), byref(error_code), byref(error_message), byref(error_data))

        error_code = error_code.value
        if error_code != 0:
            error_message = "" if error_message.value is None else error_message.value.decode("utf-8")
            error_data = "" if error_data.value is None else error_data.value.decode("utf-8")
            raise RpcError(error_code, error_message, error_data)

    def down(self, channel, distance):
        error_code = c_int()
        error_message = c_char_p()
        error_data = c_char_p()

        self._controller_sys.down(channel, c_double(distance), byref(error_code), byref(error_message), byref(error_data))

        error_code = error_code.value
        if error_code != 0:
            error_message = "" if error_message.value is None else error_message.value.decode("utf-8")
            error_data = "" if error_data.value is None else error_data.value.decode("utf-8")
            raise RpcError(error_code, error_message, error_data)

    def test(self, value):
        error_code = c_int()
        error_message = c_char_p()
        error_data = c_char_p()

        ret: int = self._controller_sys.test(value, byref(error_code), byref(error_message), byref(error_data))

        error_code = error_code.value
        if error_code != 0:
            error_message = "" if error_message.value is None else error_message.value.decode("utf-8")
            error_data = "" if error_data.value is None else error_data.value.decode("utf-8")
            raise RpcError(error_code, error_message, error_data)

        return ret


def test(controller_sys):
    print("No errors:\n")
    val = controller_sys.test(5)
    print(val)

    print("\nErrors:\n")
    try:
        val = controller_sys.test(25)
        print(val)
    except RpcError as error:
        print(error.code)
        print(error.message)
        print(error.data)


def box(controller_sys):
    while True:
        controller_sys.forward(5, 1);
        time.sleep(1)
        controller_sys.left(5, 1);
        time.sleep(1)
        controller_sys.backward(5, 1);
        time.sleep(1)
        controller_sys.right(5, 1);
        time.sleep(1)


def messages(controller_sys):
    i = 0;
    while True:
        controller_sys.log_str("This is an info toast i = " + str(i) + " plus a lot of other content", 0);
        time.sleep(1)
        controller_sys.log_str("This is a debug toast i = " + str(i), 1);
        time.sleep(1)
        controller_sys.log_str("This is a warning toast i = " + str(i), 2);
        time.sleep(1)
        controller_sys.log_str("This is an error toast i = " + str(i), 3);
        time.sleep(1)


def main():
    controller_sys = ControllerSys()

    # test(controller_sys)

    # controller_sys.log_str("Hello World!", 2);

    # box(controller_sys)

    messages(controller_sys)


main()
