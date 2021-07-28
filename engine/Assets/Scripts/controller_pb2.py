# -*- coding: utf-8 -*-
# Generated by the protocol buffer compiler.  DO NOT EDIT!
# source: controller.proto
"""Generated protocol buffer code."""
from google.protobuf.internal import enum_type_wrapper
from google.protobuf import descriptor as _descriptor
from google.protobuf import message as _message
from google.protobuf import reflection as _reflection
from google.protobuf import symbol_database as _symbol_database
# @@protoc_insertion_point(imports)

_sym_db = _symbol_database.Default()


from google.protobuf import struct_pb2 as google_dot_protobuf_dot_struct__pb2


DESCRIPTOR = _descriptor.FileDescriptor(
  name='controller.proto',
  package='',
  syntax='proto3',
  serialized_options=None,
  create_key=_descriptor._internal_create_key,
  serialized_pb=b'\n\x10\x63ontroller.proto\x1a\x1cgoogle/protobuf/struct.proto\"_\n\x0cUpdateSignal\x12\x19\n\x02io\x18\x01 \x01(\x0e\x32\r.UpdateIOType\x12\r\n\x05\x63lass\x18\x02 \x01(\t\x12%\n\x05value\x18\x03 \x01(\x0b\x32\x16.google.protobuf.Value\"\x90\x01\n\rUpdateSignals\x12\x0c\n\x04name\x18\x01 \x01(\t\x12\x30\n\tsignalMap\x18\x02 \x03(\x0b\x32\x1d.UpdateSignals.SignalMapEntry\x1a?\n\x0eSignalMapEntry\x12\x0b\n\x03key\x18\x01 \x01(\t\x12\x1c\n\x05value\x18\x02 \x01(\x0b\x32\r.UpdateSignal:\x02\x38\x01*%\n\x0cUpdateIOType\x12\t\n\x05INPUT\x10\x00\x12\n\n\x06OUTPUT\x10\x01\x62\x06proto3'
  ,
  dependencies=[google_dot_protobuf_dot_struct__pb2.DESCRIPTOR,])

_UPDATEIOTYPE = _descriptor.EnumDescriptor(
  name='UpdateIOType',
  full_name='UpdateIOType',
  filename=None,
  file=DESCRIPTOR,
  create_key=_descriptor._internal_create_key,
  values=[
    _descriptor.EnumValueDescriptor(
      name='INPUT', index=0, number=0,
      serialized_options=None,
      type=None,
      create_key=_descriptor._internal_create_key),
    _descriptor.EnumValueDescriptor(
      name='OUTPUT', index=1, number=1,
      serialized_options=None,
      type=None,
      create_key=_descriptor._internal_create_key),
  ],
  containing_type=None,
  serialized_options=None,
  serialized_start=294,
  serialized_end=331,
)
_sym_db.RegisterEnumDescriptor(_UPDATEIOTYPE)

UpdateIOType = enum_type_wrapper.EnumTypeWrapper(_UPDATEIOTYPE)
INPUT = 0
OUTPUT = 1



_UPDATESIGNAL = _descriptor.Descriptor(
  name='UpdateSignal',
  full_name='UpdateSignal',
  filename=None,
  file=DESCRIPTOR,
  containing_type=None,
  create_key=_descriptor._internal_create_key,
  fields=[
    _descriptor.FieldDescriptor(
      name='io', full_name='UpdateSignal.io', index=0,
      number=1, type=14, cpp_type=8, label=1,
      has_default_value=False, default_value=0,
      message_type=None, enum_type=None, containing_type=None,
      is_extension=False, extension_scope=None,
      serialized_options=None, file=DESCRIPTOR,  create_key=_descriptor._internal_create_key),
    _descriptor.FieldDescriptor(
      name='class', full_name='UpdateSignal.class', index=1,
      number=2, type=9, cpp_type=9, label=1,
      has_default_value=False, default_value=b"".decode('utf-8'),
      message_type=None, enum_type=None, containing_type=None,
      is_extension=False, extension_scope=None,
      serialized_options=None, file=DESCRIPTOR,  create_key=_descriptor._internal_create_key),
    _descriptor.FieldDescriptor(
      name='value', full_name='UpdateSignal.value', index=2,
      number=3, type=11, cpp_type=10, label=1,
      has_default_value=False, default_value=None,
      message_type=None, enum_type=None, containing_type=None,
      is_extension=False, extension_scope=None,
      serialized_options=None, file=DESCRIPTOR,  create_key=_descriptor._internal_create_key),
  ],
  extensions=[
  ],
  nested_types=[],
  enum_types=[
  ],
  serialized_options=None,
  is_extendable=False,
  syntax='proto3',
  extension_ranges=[],
  oneofs=[
  ],
  serialized_start=50,
  serialized_end=145,
)


_UPDATESIGNALS_SIGNALMAPENTRY = _descriptor.Descriptor(
  name='SignalMapEntry',
  full_name='UpdateSignals.SignalMapEntry',
  filename=None,
  file=DESCRIPTOR,
  containing_type=None,
  create_key=_descriptor._internal_create_key,
  fields=[
    _descriptor.FieldDescriptor(
      name='key', full_name='UpdateSignals.SignalMapEntry.key', index=0,
      number=1, type=9, cpp_type=9, label=1,
      has_default_value=False, default_value=b"".decode('utf-8'),
      message_type=None, enum_type=None, containing_type=None,
      is_extension=False, extension_scope=None,
      serialized_options=None, file=DESCRIPTOR,  create_key=_descriptor._internal_create_key),
    _descriptor.FieldDescriptor(
      name='value', full_name='UpdateSignals.SignalMapEntry.value', index=1,
      number=2, type=11, cpp_type=10, label=1,
      has_default_value=False, default_value=None,
      message_type=None, enum_type=None, containing_type=None,
      is_extension=False, extension_scope=None,
      serialized_options=None, file=DESCRIPTOR,  create_key=_descriptor._internal_create_key),
  ],
  extensions=[
  ],
  nested_types=[],
  enum_types=[
  ],
  serialized_options=b'8\001',
  is_extendable=False,
  syntax='proto3',
  extension_ranges=[],
  oneofs=[
  ],
  serialized_start=229,
  serialized_end=292,
)

_UPDATESIGNALS = _descriptor.Descriptor(
  name='UpdateSignals',
  full_name='UpdateSignals',
  filename=None,
  file=DESCRIPTOR,
  containing_type=None,
  create_key=_descriptor._internal_create_key,
  fields=[
    _descriptor.FieldDescriptor(
      name='name', full_name='UpdateSignals.name', index=0,
      number=1, type=9, cpp_type=9, label=1,
      has_default_value=False, default_value=b"".decode('utf-8'),
      message_type=None, enum_type=None, containing_type=None,
      is_extension=False, extension_scope=None,
      serialized_options=None, file=DESCRIPTOR,  create_key=_descriptor._internal_create_key),
    _descriptor.FieldDescriptor(
      name='signalMap', full_name='UpdateSignals.signalMap', index=1,
      number=2, type=11, cpp_type=10, label=3,
      has_default_value=False, default_value=[],
      message_type=None, enum_type=None, containing_type=None,
      is_extension=False, extension_scope=None,
      serialized_options=None, file=DESCRIPTOR,  create_key=_descriptor._internal_create_key),
  ],
  extensions=[
  ],
  nested_types=[_UPDATESIGNALS_SIGNALMAPENTRY, ],
  enum_types=[
  ],
  serialized_options=None,
  is_extendable=False,
  syntax='proto3',
  extension_ranges=[],
  oneofs=[
  ],
  serialized_start=148,
  serialized_end=292,
)

_UPDATESIGNAL.fields_by_name['io'].enum_type = _UPDATEIOTYPE
_UPDATESIGNAL.fields_by_name['value'].message_type = google_dot_protobuf_dot_struct__pb2._VALUE
_UPDATESIGNALS_SIGNALMAPENTRY.fields_by_name['value'].message_type = _UPDATESIGNAL
_UPDATESIGNALS_SIGNALMAPENTRY.containing_type = _UPDATESIGNALS
_UPDATESIGNALS.fields_by_name['signalMap'].message_type = _UPDATESIGNALS_SIGNALMAPENTRY
DESCRIPTOR.message_types_by_name['UpdateSignal'] = _UPDATESIGNAL
DESCRIPTOR.message_types_by_name['UpdateSignals'] = _UPDATESIGNALS
DESCRIPTOR.enum_types_by_name['UpdateIOType'] = _UPDATEIOTYPE
_sym_db.RegisterFileDescriptor(DESCRIPTOR)

UpdateSignal = _reflection.GeneratedProtocolMessageType('UpdateSignal', (_message.Message,), {
  'DESCRIPTOR' : _UPDATESIGNAL,
  '__module__' : 'controller_pb2'
  # @@protoc_insertion_point(class_scope:UpdateSignal)
  })
_sym_db.RegisterMessage(UpdateSignal)

UpdateSignals = _reflection.GeneratedProtocolMessageType('UpdateSignals', (_message.Message,), {

  'SignalMapEntry' : _reflection.GeneratedProtocolMessageType('SignalMapEntry', (_message.Message,), {
    'DESCRIPTOR' : _UPDATESIGNALS_SIGNALMAPENTRY,
    '__module__' : 'controller_pb2'
    # @@protoc_insertion_point(class_scope:UpdateSignals.SignalMapEntry)
    })
  ,
  'DESCRIPTOR' : _UPDATESIGNALS,
  '__module__' : 'controller_pb2'
  # @@protoc_insertion_point(class_scope:UpdateSignals)
  })
_sym_db.RegisterMessage(UpdateSignals)
_sym_db.RegisterMessage(UpdateSignals.SignalMapEntry)


_UPDATESIGNALS_SIGNALMAPENTRY._options = None
# @@protoc_insertion_point(module_scope)
