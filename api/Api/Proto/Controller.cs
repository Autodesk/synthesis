// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: protocols/v1/controller.proto
// </auto-generated>
#pragma warning disable 1591, 0612, 3021
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
/// <summary>Holder for reflection information generated from protocols/v1/controller.proto</summary>
public static partial class ControllerReflection {

  #region Descriptor
  /// <summary>File descriptor for protocols/v1/controller.proto</summary>
  public static pbr::FileDescriptor Descriptor {
    get { return descriptor; }
  }
  private static pbr::FileDescriptor descriptor;

  static ControllerReflection() {
    byte[] descriptorData = global::System.Convert.FromBase64String(
        string.Concat(
          "Ch1wcm90b2NvbHMvdjEvY29udHJvbGxlci5wcm90bxocZ29vZ2xlL3Byb3Rv",
          "YnVmL3N0cnVjdC5wcm90byJZCgxVcGRhdGVTaWduYWwSEwoCaW8YASABKA4y",
          "By5JT1R5cGUSDQoFY2xhc3MYAiABKAkSJQoFdmFsdWUYAyABKAsyFi5nb29n",
          "bGUucHJvdG9idWYuVmFsdWUikAEKDVVwZGF0ZVNpZ25hbHMSDAoEbmFtZRgB",
          "IAEoCRIwCglzaWduYWxNYXAYAiADKAsyHS5VcGRhdGVTaWduYWxzLlNpZ25h",
          "bE1hcEVudHJ5Gj8KDlNpZ25hbE1hcEVudHJ5EgsKA2tleRgBIAEoCRIcCgV2",
          "YWx1ZRgCIAEoCzINLlVwZGF0ZVNpZ25hbDoCOAEqHwoGSU9UeXBlEgkKBUlO",
          "UFVUEAASCgoGT1VUUFVUEAFiBnByb3RvMw=="));
    descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
        new pbr::FileDescriptor[] { global::Google.Protobuf.WellKnownTypes.StructReflection.Descriptor, },
        new pbr::GeneratedClrTypeInfo(new[] {typeof(global::IOType), }, null, new pbr::GeneratedClrTypeInfo[] {
          new pbr::GeneratedClrTypeInfo(typeof(global::UpdateSignal), global::UpdateSignal.Parser, new[]{ "Io", "Class", "Value" }, null, null, null, null),
          new pbr::GeneratedClrTypeInfo(typeof(global::UpdateSignals), global::UpdateSignals.Parser, new[]{ "Name", "SignalMap" }, null, null, null, new pbr::GeneratedClrTypeInfo[] { null, })
        }));
  }
  #endregion

}
#region Enums
/// <summary>
///*
/// IOType is a way to specify Input or Output.
/// 
/// </summary>
public enum IOType {
  /// <summary>
  //// Input Signal
  /// </summary>
  [pbr::OriginalName("INPUT")] Input = 0,
  /// <summary>
  //// Output Signal
  /// </summary>
  [pbr::OriginalName("OUTPUT")] Output = 1,
}

#endregion

#region Messages
public sealed partial class UpdateSignal : pb::IMessage<UpdateSignal>
#if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    , pb::IBufferMessage
#endif
{
  private static readonly pb::MessageParser<UpdateSignal> _parser = new pb::MessageParser<UpdateSignal>(() => new UpdateSignal());
  private pb::UnknownFieldSet _unknownFields;
  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
  public static pb::MessageParser<UpdateSignal> Parser { get { return _parser; } }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
  public static pbr::MessageDescriptor Descriptor {
    get { return global::ControllerReflection.Descriptor.MessageTypes[0]; }
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
  pbr::MessageDescriptor pb::IMessage.Descriptor {
    get { return Descriptor; }
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
  public UpdateSignal() {
    OnConstruction();
  }

  partial void OnConstruction();

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
  public UpdateSignal(UpdateSignal other) : this() {
    io_ = other.io_;
    class_ = other.class_;
    value_ = other.value_ != null ? other.value_.Clone() : null;
    _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
  public UpdateSignal Clone() {
    return new UpdateSignal(this);
  }

  /// <summary>Field number for the "io" field.</summary>
  public const int IoFieldNumber = 1;
  private global::IOType io_ = global::IOType.Input;
  /// <summary>
  //// Is this a Input or Output
  /// </summary>
  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
  public global::IOType Io {
    get { return io_; }
    set {
      io_ = value;
    }
  }

  /// <summary>Field number for the "class" field.</summary>
  public const int ClassFieldNumber = 2;
  private string class_ = "";
  /// <summary>
  //// Is this a PWM, Digital, Analog, I2C, etc.
  /// </summary>
  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
  public string Class {
    get { return class_; }
    set {
      class_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
    }
  }

  /// <summary>Field number for the "value" field.</summary>
  public const int ValueFieldNumber = 3;
  private global::Google.Protobuf.WellKnownTypes.Value value_;
  /// <summary>
  //// 
  /// </summary>
  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
  public global::Google.Protobuf.WellKnownTypes.Value Value {
    get { return value_; }
    set {
      value_ = value;
    }
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
  public override bool Equals(object other) {
    return Equals(other as UpdateSignal);
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
  public bool Equals(UpdateSignal other) {
    if (ReferenceEquals(other, null)) {
      return false;
    }
    if (ReferenceEquals(other, this)) {
      return true;
    }
    if (Io != other.Io) return false;
    if (Class != other.Class) return false;
    if (!object.Equals(Value, other.Value)) return false;
    return Equals(_unknownFields, other._unknownFields);
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
  public override int GetHashCode() {
    int hash = 1;
    if (Io != global::IOType.Input) hash ^= Io.GetHashCode();
    if (Class.Length != 0) hash ^= Class.GetHashCode();
    if (value_ != null) hash ^= Value.GetHashCode();
    if (_unknownFields != null) {
      hash ^= _unknownFields.GetHashCode();
    }
    return hash;
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
  public override string ToString() {
    return pb::JsonFormatter.ToDiagnosticString(this);
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
  public void WriteTo(pb::CodedOutputStream output) {
  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    output.WriteRawMessage(this);
  #else
    if (Io != global::IOType.Input) {
      output.WriteRawTag(8);
      output.WriteEnum((int) Io);
    }
    if (Class.Length != 0) {
      output.WriteRawTag(18);
      output.WriteString(Class);
    }
    if (value_ != null) {
      output.WriteRawTag(26);
      output.WriteMessage(Value);
    }
    if (_unknownFields != null) {
      _unknownFields.WriteTo(output);
    }
  #endif
  }

  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
  void pb::IBufferMessage.InternalWriteTo(ref pb::WriteContext output) {
    if (Io != global::IOType.Input) {
      output.WriteRawTag(8);
      output.WriteEnum((int) Io);
    }
    if (Class.Length != 0) {
      output.WriteRawTag(18);
      output.WriteString(Class);
    }
    if (value_ != null) {
      output.WriteRawTag(26);
      output.WriteMessage(Value);
    }
    if (_unknownFields != null) {
      _unknownFields.WriteTo(ref output);
    }
  }
  #endif

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
  public int CalculateSize() {
    int size = 0;
    if (Io != global::IOType.Input) {
      size += 1 + pb::CodedOutputStream.ComputeEnumSize((int) Io);
    }
    if (Class.Length != 0) {
      size += 1 + pb::CodedOutputStream.ComputeStringSize(Class);
    }
    if (value_ != null) {
      size += 1 + pb::CodedOutputStream.ComputeMessageSize(Value);
    }
    if (_unknownFields != null) {
      size += _unknownFields.CalculateSize();
    }
    return size;
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
  public void MergeFrom(UpdateSignal other) {
    if (other == null) {
      return;
    }
    if (other.Io != global::IOType.Input) {
      Io = other.Io;
    }
    if (other.Class.Length != 0) {
      Class = other.Class;
    }
    if (other.value_ != null) {
      if (value_ == null) {
        Value = new global::Google.Protobuf.WellKnownTypes.Value();
      }
      Value.MergeFrom(other.Value);
    }
    _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
  public void MergeFrom(pb::CodedInputStream input) {
  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    input.ReadRawMessage(this);
  #else
    uint tag;
    while ((tag = input.ReadTag()) != 0) {
      switch(tag) {
        default:
          _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
          break;
        case 8: {
          Io = (global::IOType) input.ReadEnum();
          break;
        }
        case 18: {
          Class = input.ReadString();
          break;
        }
        case 26: {
          if (value_ == null) {
            Value = new global::Google.Protobuf.WellKnownTypes.Value();
          }
          input.ReadMessage(Value);
          break;
        }
      }
    }
  #endif
  }

  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
  void pb::IBufferMessage.InternalMergeFrom(ref pb::ParseContext input) {
    uint tag;
    while ((tag = input.ReadTag()) != 0) {
      switch(tag) {
        default:
          _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
          break;
        case 8: {
          Io = (global::IOType) input.ReadEnum();
          break;
        }
        case 18: {
          Class = input.ReadString();
          break;
        }
        case 26: {
          if (value_ == null) {
            Value = new global::Google.Protobuf.WellKnownTypes.Value();
          }
          input.ReadMessage(Value);
          break;
        }
      }
    }
  }
  #endif

}

public sealed partial class UpdateSignals : pb::IMessage<UpdateSignals>
#if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    , pb::IBufferMessage
#endif
{
  private static readonly pb::MessageParser<UpdateSignals> _parser = new pb::MessageParser<UpdateSignals>(() => new UpdateSignals());
  private pb::UnknownFieldSet _unknownFields;
  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
  public static pb::MessageParser<UpdateSignals> Parser { get { return _parser; } }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
  public static pbr::MessageDescriptor Descriptor {
    get { return global::ControllerReflection.Descriptor.MessageTypes[1]; }
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
  pbr::MessageDescriptor pb::IMessage.Descriptor {
    get { return Descriptor; }
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
  public UpdateSignals() {
    OnConstruction();
  }

  partial void OnConstruction();

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
  public UpdateSignals(UpdateSignals other) : this() {
    name_ = other.name_;
    signalMap_ = other.signalMap_.Clone();
    _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
  public UpdateSignals Clone() {
    return new UpdateSignals(this);
  }

  /// <summary>Field number for the "name" field.</summary>
  public const int NameFieldNumber = 1;
  private string name_ = "";
  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
  public string Name {
    get { return name_; }
    set {
      name_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
    }
  }

  /// <summary>Field number for the "signalMap" field.</summary>
  public const int SignalMapFieldNumber = 2;
  private static readonly pbc::MapField<string, global::UpdateSignal>.Codec _map_signalMap_codec
      = new pbc::MapField<string, global::UpdateSignal>.Codec(pb::FieldCodec.ForString(10, ""), pb::FieldCodec.ForMessage(18, global::UpdateSignal.Parser), 18);
  private readonly pbc::MapField<string, global::UpdateSignal> signalMap_ = new pbc::MapField<string, global::UpdateSignal>();
  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
  public pbc::MapField<string, global::UpdateSignal> SignalMap {
    get { return signalMap_; }
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
  public override bool Equals(object other) {
    return Equals(other as UpdateSignals);
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
  public bool Equals(UpdateSignals other) {
    if (ReferenceEquals(other, null)) {
      return false;
    }
    if (ReferenceEquals(other, this)) {
      return true;
    }
    if (Name != other.Name) return false;
    if (!SignalMap.Equals(other.SignalMap)) return false;
    return Equals(_unknownFields, other._unknownFields);
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
  public override int GetHashCode() {
    int hash = 1;
    if (Name.Length != 0) hash ^= Name.GetHashCode();
    hash ^= SignalMap.GetHashCode();
    if (_unknownFields != null) {
      hash ^= _unknownFields.GetHashCode();
    }
    return hash;
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
  public override string ToString() {
    return pb::JsonFormatter.ToDiagnosticString(this);
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
  public void WriteTo(pb::CodedOutputStream output) {
  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    output.WriteRawMessage(this);
  #else
    if (Name.Length != 0) {
      output.WriteRawTag(10);
      output.WriteString(Name);
    }
    signalMap_.WriteTo(output, _map_signalMap_codec);
    if (_unknownFields != null) {
      _unknownFields.WriteTo(output);
    }
  #endif
  }

  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
  void pb::IBufferMessage.InternalWriteTo(ref pb::WriteContext output) {
    if (Name.Length != 0) {
      output.WriteRawTag(10);
      output.WriteString(Name);
    }
    signalMap_.WriteTo(ref output, _map_signalMap_codec);
    if (_unknownFields != null) {
      _unknownFields.WriteTo(ref output);
    }
  }
  #endif

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
  public int CalculateSize() {
    int size = 0;
    if (Name.Length != 0) {
      size += 1 + pb::CodedOutputStream.ComputeStringSize(Name);
    }
    size += signalMap_.CalculateSize(_map_signalMap_codec);
    if (_unknownFields != null) {
      size += _unknownFields.CalculateSize();
    }
    return size;
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
  public void MergeFrom(UpdateSignals other) {
    if (other == null) {
      return;
    }
    if (other.Name.Length != 0) {
      Name = other.Name;
    }
    signalMap_.Add(other.signalMap_);
    _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
  }

  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
  public void MergeFrom(pb::CodedInputStream input) {
  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    input.ReadRawMessage(this);
  #else
    uint tag;
    while ((tag = input.ReadTag()) != 0) {
      switch(tag) {
        default:
          _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
          break;
        case 10: {
          Name = input.ReadString();
          break;
        }
        case 18: {
          signalMap_.AddEntriesFrom(input, _map_signalMap_codec);
          break;
        }
      }
    }
  #endif
  }

  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
  [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
  [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
  void pb::IBufferMessage.InternalMergeFrom(ref pb::ParseContext input) {
    uint tag;
    while ((tag = input.ReadTag()) != 0) {
      switch(tag) {
        default:
          _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
          break;
        case 10: {
          Name = input.ReadString();
          break;
        }
        case 18: {
          signalMap_.AddEntriesFrom(ref input, _map_signalMap_codec);
          break;
        }
      }
    }
  }
  #endif

}

#endregion


#endregion Designer generated code
