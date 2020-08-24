# -*- coding: utf-8 -*-
#
# Copyright (c) 2016 Ross Korsky
#
# Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to
# use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions: The above copyright notice and this
# permission notice shall be included in all copies or substantial portions of the Software. THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
# MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
# ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
#
# Timers for profiling performance, etc.
#
import time
from collections import OrderedDict

__all__ = ['Stopwatch', 'RelayStopwatch', 'SegmentedStopwatch']

class Stopwatch(object):
  """Provides basic stopwatch functionality to time the duration between events.
  May be paused and resumed (with another call to run).
  """
  def __init__(self, time_fn=time.process_time):
    """
    time.process_time - The sum of the system and user CPU time of the current process.
    time.perf_counter - A clock with the highest available resolution to measure a short duration.
                        It does include time elapsed during thread sleep and is system-wide.
    """
    self._elapsed = 0
    self._started = -1
    self._time_fn = time_fn

  @property
  def elapsed(self):
    return self._elapsed

  def reset(self):
    self._elapsed = 0
    self._started = -1

  def run(self):
    self._started = self._time_fn()

  def pause(self):
    now = self._time_fn()
    if self._started > 0:
      self._elapsed += now - self._started
      self._started = -1


class RelayStopwatch(object):
  """Allows the timing of each part of a chain of events.
  Each section is unique even if the same section name is used multiple times.
  """
  def __init__(self, time_fn=time.perf_counter):
    """
    time.process_time - The sum of the system and user CPU time of the current process
    time.perf_counter - A clock with the highest available resolution to measure a short duration.
                        It does include time elapsed during sleep and is system-wide.
    """
    self.sections = []
    self.__active_section_name = ""
    self.__active_section_start = -1
    self._time_fn = time_fn

  def start_section(self, section_name):
    now = self._time_fn()
    if self.__active_section_start > 0:
      self.sections.append((self.__active_section_name, now - self.__active_section_start))
    self.__active_section_name = section_name
    self.__active_section_start = self._time_fn()

  def stop(self):
    now = self._time_fn()
    if self.__active_section_start > 0:
      self.sections.append((self.__active_section_name, now - self.__active_section_start))
      self.__active_section_start = -1

  def __str__(self):
    s = ''
    for name, elapsed in self.sections:
      s += '{}: {}\n'.format(name, elapsed)
    return s


class SegmentedStopwatch(object):
  """Provides a simple container to maintin several named Stopwatch object."""
  def __init__(self, time_fn=time.process_time):
    """
    time.process_time - The sum of the system and user CPU time of the current process
    time.perf_counter - A clock with the highest available resolution to measure a short duration.
                        It does include time elapsed during sleep and is system-wide.

    Returns:
      :
    """
    self._segments = OrderedDict()
    self._active_counts = dict()
    self._active_stopwatch = None
    self._time_fn = time_fn

  def switch_segment(self, segment_name):
    if self._active_stopwatch:
      self._active_stopwatch.pause()
    if segment_name not in self._segments:
      self._segments[segment_name] = Stopwatch(self._time_fn)
      self._active_counts[segment_name] = 0
    self._active_stopwatch = self._segments[segment_name]
    self._active_counts[segment_name] += 1
    self._active_stopwatch.run()

  def pause(self):
    """Pauses the current segment stopwatch."""
    if self._active_stopwatch:
      self._active_stopwatch.pause()

  def resume(self):
    """Resumes the current segment stopwatch."""
    if self._active_stopwatch:
      self._active_stopwatch.run()

  def stop(self):
    """Stops the current segment stopwatch, switch_segment must be called to resume."""
    if self._active_stopwatch:
      self._active_stopwatch.pause()
      self._active_stopwatch = None

  def __str__(self):
    s = ''
    total = 0
    for name, stopwatch in self._segments.items():
      s += '{}: {} // {}\n'.format(name, stopwatch.elapsed, self._active_counts[name])
      total += stopwatch.elapsed
    s += 'TOTAL ELAPSED: {}\n'.format(total)
    return s