from abc import ABC, abstractmethod


class CommandGroup(ABC):

    def __init__(self, parent):
        self.parent = parent

    @abstractmethod
    def configure(self):
        pass
