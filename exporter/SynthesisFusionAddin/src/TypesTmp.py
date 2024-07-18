# Pure typing hints.
class LBS(float):
    """Mass Unit in Pounds."""


class KG(float):
    """Mass Unit in Kilograms."""


def toLbs(kgs: float) -> LBS:
    return LBS(kgs * 2.2062)


def toKg(pounds: float) -> KG:
    return KG(pounds / 2.2062)
