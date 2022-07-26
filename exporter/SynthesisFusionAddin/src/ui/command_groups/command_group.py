from abc import ABC, abstractmethod


class CommandGroup(ABC):

    @abstractmethod
    def __init__(self):
        pass

    @abstractmethod
    def configure(self):
        pass
