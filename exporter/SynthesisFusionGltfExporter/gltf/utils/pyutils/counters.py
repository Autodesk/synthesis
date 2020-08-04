class EventCounter(object):
  def __init__(self):
    self._active_counts = dict()
    self._active_counts_meta = dict()

  def event(self, segment_name, meta = None):
    if segment_name not in self._active_counts:
      self._active_counts[segment_name] = 0
    self._active_counts[segment_name] += 1
    if meta is not None:
      if segment_name not in self._active_counts_meta:
        self._active_counts_meta[segment_name] = []
      self._active_counts_meta[segment_name].append(meta)

  def __str__(self):
    s = ''
    for name, counts in self._active_counts.items():
      s += f'{name} {counts} time(s). {"" if name not in self._active_counts_meta else f" Meta: {str(self._active_counts_meta[name])}" }\n'
    return s