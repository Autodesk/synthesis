import * as $protobuf from "protobufjs/minimal"

// Common aliases
const $Reader = $protobuf.Reader,
    $Writer = $protobuf.Writer,
    $util = $protobuf.util

// Exported root namespace
const $root = $protobuf.roots["default"] || ($protobuf.roots["default"] = {})

export const mirabuf = ($root.mirabuf = (() => {
    /**
     * Namespace mirabuf.
     * @exports mirabuf
     * @namespace
     */
    const mirabuf = {}

    mirabuf.Assembly = (function () {
        /**
         * Properties of an Assembly.
         * @memberof mirabuf
         * @interface IAssembly
         * @property {mirabuf.IInfo|null} [info] Basic information (name, Author, etc)
         * @property {mirabuf.IAssemblyData|null} [data] All of the data in the assembly
         * @property {boolean|null} [dynamic] Can it be effected by the simulation dynamically
         * @property {mirabuf.IPhysicalProperties|null} [physicalData] Overall physical data of the assembly
         * @property {mirabuf.IGraphContainer|null} [designHierarchy] The Design hierarchy represented by Part Refs - The first object is a root container for all top level items
         * @property {mirabuf.IGraphContainer|null} [jointHierarchy] The Joint hierarchy for compound shapes
         * @property {mirabuf.ITransform|null} [transform] The Transform in space currently
         * @property {mirabuf.IThumbnail|null} [thumbnail] Optional thumbnail saved from Fusion 360 or scraped from previous configuration
         */

        /**
         * Constructs a new Assembly.
         * @memberof mirabuf
         * @classdesc Assembly
         * Base Design to be interacted with
         * THIS IS THE CURRENT FILE EXPORTED
         * @implements IAssembly
         * @constructor
         * @param {mirabuf.IAssembly=} [properties] Properties to set
         */
        function Assembly(properties) {
            if (properties)
                for (let keys = Object.keys(properties), i = 0; i < keys.length; ++i)
                    if (properties[keys[i]] != null) this[keys[i]] = properties[keys[i]]
        }

        /**
         * Basic information (name, Author, etc)
         * @member {mirabuf.IInfo|null|undefined} info
         * @memberof mirabuf.Assembly
         * @instance
         */
        Assembly.prototype.info = null

        /**
         * All of the data in the assembly
         * @member {mirabuf.IAssemblyData|null|undefined} data
         * @memberof mirabuf.Assembly
         * @instance
         */
        Assembly.prototype.data = null

        /**
         * Can it be effected by the simulation dynamically
         * @member {boolean} dynamic
         * @memberof mirabuf.Assembly
         * @instance
         */
        Assembly.prototype.dynamic = false

        /**
         * Overall physical data of the assembly
         * @member {mirabuf.IPhysicalProperties|null|undefined} physicalData
         * @memberof mirabuf.Assembly
         * @instance
         */
        Assembly.prototype.physicalData = null

        /**
         * The Design hierarchy represented by Part Refs - The first object is a root container for all top level items
         * @member {mirabuf.IGraphContainer|null|undefined} designHierarchy
         * @memberof mirabuf.Assembly
         * @instance
         */
        Assembly.prototype.designHierarchy = null

        /**
         * The Joint hierarchy for compound shapes
         * @member {mirabuf.IGraphContainer|null|undefined} jointHierarchy
         * @memberof mirabuf.Assembly
         * @instance
         */
        Assembly.prototype.jointHierarchy = null

        /**
         * The Transform in space currently
         * @member {mirabuf.ITransform|null|undefined} transform
         * @memberof mirabuf.Assembly
         * @instance
         */
        Assembly.prototype.transform = null

        /**
         * Optional thumbnail saved from Fusion 360 or scraped from previous configuration
         * @member {mirabuf.IThumbnail|null|undefined} thumbnail
         * @memberof mirabuf.Assembly
         * @instance
         */
        Assembly.prototype.thumbnail = null

        /**
         * Creates a new Assembly instance using the specified properties.
         * @function create
         * @memberof mirabuf.Assembly
         * @static
         * @param {mirabuf.IAssembly=} [properties] Properties to set
         * @returns {mirabuf.Assembly} Assembly instance
         */
        Assembly.create = function create(properties) {
            return new Assembly(properties)
        }

        /**
         * Encodes the specified Assembly message. Does not implicitly {@link mirabuf.Assembly.verify|verify} messages.
         * @function encode
         * @memberof mirabuf.Assembly
         * @static
         * @param {mirabuf.IAssembly} message Assembly message or plain object to encode
         * @param {$protobuf.Writer} [writer] Writer to encode to
         * @returns {$protobuf.Writer} Writer
         */
        Assembly.encode = function encode(message, writer) {
            if (!writer) writer = $Writer.create()
            if (message.info != null && Object.hasOwn(message, "info"))
                $root.mirabuf.Info.encode(message.info, writer.uint32(/* id 1, wireType 2 =*/ 10).fork()).ldelim()
            if (message.data != null && Object.hasOwn(message, "data"))
                $root.mirabuf.AssemblyData.encode(
                    message.data,
                    writer.uint32(/* id 2, wireType 2 =*/ 18).fork()
                ).ldelim()
            if (message.dynamic != null && Object.hasOwn(message, "dynamic"))
                writer.uint32(/* id 3, wireType 0 =*/ 24).bool(message.dynamic)
            if (message.physicalData != null && Object.hasOwn(message, "physicalData"))
                $root.mirabuf.PhysicalProperties.encode(
                    message.physicalData,
                    writer.uint32(/* id 4, wireType 2 =*/ 34).fork()
                ).ldelim()
            if (message.designHierarchy != null && Object.hasOwn(message, "designHierarchy"))
                $root.mirabuf.GraphContainer.encode(
                    message.designHierarchy,
                    writer.uint32(/* id 5, wireType 2 =*/ 42).fork()
                ).ldelim()
            if (message.jointHierarchy != null && Object.hasOwn(message, "jointHierarchy"))
                $root.mirabuf.GraphContainer.encode(
                    message.jointHierarchy,
                    writer.uint32(/* id 6, wireType 2 =*/ 50).fork()
                ).ldelim()
            if (message.transform != null && Object.hasOwn(message, "transform"))
                $root.mirabuf.Transform.encode(
                    message.transform,
                    writer.uint32(/* id 7, wireType 2 =*/ 58).fork()
                ).ldelim()
            if (message.thumbnail != null && Object.hasOwn(message, "thumbnail"))
                $root.mirabuf.Thumbnail.encode(
                    message.thumbnail,
                    writer.uint32(/* id 8, wireType 2 =*/ 66).fork()
                ).ldelim()
            return writer
        }

        /**
         * Encodes the specified Assembly message, length delimited. Does not implicitly {@link mirabuf.Assembly.verify|verify} messages.
         * @function encodeDelimited
         * @memberof mirabuf.Assembly
         * @static
         * @param {mirabuf.IAssembly} message Assembly message or plain object to encode
         * @param {$protobuf.Writer} [writer] Writer to encode to
         * @returns {$protobuf.Writer} Writer
         */
        Assembly.encodeDelimited = function encodeDelimited(message, writer) {
            return this.encode(message, writer).ldelim()
        }

        /**
         * Decodes an Assembly message from the specified reader or buffer.
         * @function decode
         * @memberof mirabuf.Assembly
         * @static
         * @param {$protobuf.Reader|Uint8Array} reader Reader or buffer to decode from
         * @param {number} [length] Message length if known beforehand
         * @returns {mirabuf.Assembly} Assembly
         * @throws {Error} If the payload is not a reader or valid buffer
         * @throws {$protobuf.util.ProtocolError} If required fields are missing
         */
        Assembly.decode = function decode(reader, length) {
            if (!(reader instanceof $Reader)) reader = $Reader.create(reader)
            let end = length === undefined ? reader.len : reader.pos + length,
                message = new $root.mirabuf.Assembly()
            while (reader.pos < end) {
                let tag = reader.uint32()
                switch (tag >>> 3) {
                    case 1: {
                        message.info = $root.mirabuf.Info.decode(reader, reader.uint32())
                        break
                    }
                    case 2: {
                        message.data = $root.mirabuf.AssemblyData.decode(reader, reader.uint32())
                        break
                    }
                    case 3: {
                        message.dynamic = reader.bool()
                        break
                    }
                    case 4: {
                        message.physicalData = $root.mirabuf.PhysicalProperties.decode(reader, reader.uint32())
                        break
                    }
                    case 5: {
                        message.designHierarchy = $root.mirabuf.GraphContainer.decode(reader, reader.uint32())
                        break
                    }
                    case 6: {
                        message.jointHierarchy = $root.mirabuf.GraphContainer.decode(reader, reader.uint32())
                        break
                    }
                    case 7: {
                        message.transform = $root.mirabuf.Transform.decode(reader, reader.uint32())
                        break
                    }
                    case 8: {
                        message.thumbnail = $root.mirabuf.Thumbnail.decode(reader, reader.uint32())
                        break
                    }
                    default:
                        reader.skipType(tag & 7)
                        break
                }
            }
            return message
        }

        /**
         * Decodes an Assembly message from the specified reader or buffer, length delimited.
         * @function decodeDelimited
         * @memberof mirabuf.Assembly
         * @static
         * @param {$protobuf.Reader|Uint8Array} reader Reader or buffer to decode from
         * @returns {mirabuf.Assembly} Assembly
         * @throws {Error} If the payload is not a reader or valid buffer
         * @throws {$protobuf.util.ProtocolError} If required fields are missing
         */
        Assembly.decodeDelimited = function decodeDelimited(reader) {
            if (!(reader instanceof $Reader)) reader = new $Reader(reader)
            return this.decode(reader, reader.uint32())
        }

        /**
         * Verifies an Assembly message.
         * @function verify
         * @memberof mirabuf.Assembly
         * @static
         * @param {Object.<string,*>} message Plain object to verify
         * @returns {string|null} `null` if valid, otherwise the reason why it is not
         */
        Assembly.verify = function verify(message) {
            if (typeof message !== "object" || message === null) return "object expected"
            if (message.info != null && Object.hasOwn(message, "info")) {
                let error = $root.mirabuf.Info.verify(message.info)
                if (error) return "info." + error
            }
            if (message.data != null && Object.hasOwn(message, "data")) {
                let error = $root.mirabuf.AssemblyData.verify(message.data)
                if (error) return "data." + error
            }
            if (message.dynamic != null && Object.hasOwn(message, "dynamic"))
                if (typeof message.dynamic !== "boolean") return "dynamic: boolean expected"
            if (message.physicalData != null && Object.hasOwn(message, "physicalData")) {
                let error = $root.mirabuf.PhysicalProperties.verify(message.physicalData)
                if (error) return "physicalData." + error
            }
            if (message.designHierarchy != null && Object.hasOwn(message, "designHierarchy")) {
                let error = $root.mirabuf.GraphContainer.verify(message.designHierarchy)
                if (error) return "designHierarchy." + error
            }
            if (message.jointHierarchy != null && Object.hasOwn(message, "jointHierarchy")) {
                let error = $root.mirabuf.GraphContainer.verify(message.jointHierarchy)
                if (error) return "jointHierarchy." + error
            }
            if (message.transform != null && Object.hasOwn(message, "transform")) {
                let error = $root.mirabuf.Transform.verify(message.transform)
                if (error) return "transform." + error
            }
            if (message.thumbnail != null && Object.hasOwn(message, "thumbnail")) {
                let error = $root.mirabuf.Thumbnail.verify(message.thumbnail)
                if (error) return "thumbnail." + error
            }
            return null
        }

        /**
         * Creates an Assembly message from a plain object. Also converts values to their respective internal types.
         * @function fromObject
         * @memberof mirabuf.Assembly
         * @static
         * @param {Object.<string,*>} object Plain object
         * @returns {mirabuf.Assembly} Assembly
         */
        Assembly.fromObject = function fromObject(object) {
            if (object instanceof $root.mirabuf.Assembly) return object
            let message = new $root.mirabuf.Assembly()
            if (object.info != null) {
                if (typeof object.info !== "object") throw TypeError(".mirabuf.Assembly.info: object expected")
                message.info = $root.mirabuf.Info.fromObject(object.info)
            }
            if (object.data != null) {
                if (typeof object.data !== "object") throw TypeError(".mirabuf.Assembly.data: object expected")
                message.data = $root.mirabuf.AssemblyData.fromObject(object.data)
            }
            if (object.dynamic != null) message.dynamic = Boolean(object.dynamic)
            if (object.physicalData != null) {
                if (typeof object.physicalData !== "object")
                    throw TypeError(".mirabuf.Assembly.physicalData: object expected")
                message.physicalData = $root.mirabuf.PhysicalProperties.fromObject(object.physicalData)
            }
            if (object.designHierarchy != null) {
                if (typeof object.designHierarchy !== "object")
                    throw TypeError(".mirabuf.Assembly.designHierarchy: object expected")
                message.designHierarchy = $root.mirabuf.GraphContainer.fromObject(object.designHierarchy)
            }
            if (object.jointHierarchy != null) {
                if (typeof object.jointHierarchy !== "object")
                    throw TypeError(".mirabuf.Assembly.jointHierarchy: object expected")
                message.jointHierarchy = $root.mirabuf.GraphContainer.fromObject(object.jointHierarchy)
            }
            if (object.transform != null) {
                if (typeof object.transform !== "object")
                    throw TypeError(".mirabuf.Assembly.transform: object expected")
                message.transform = $root.mirabuf.Transform.fromObject(object.transform)
            }
            if (object.thumbnail != null) {
                if (typeof object.thumbnail !== "object")
                    throw TypeError(".mirabuf.Assembly.thumbnail: object expected")
                message.thumbnail = $root.mirabuf.Thumbnail.fromObject(object.thumbnail)
            }
            return message
        }

        /**
         * Creates a plain object from an Assembly message. Also converts values to other types if specified.
         * @function toObject
         * @memberof mirabuf.Assembly
         * @static
         * @param {mirabuf.Assembly} message Assembly
         * @param {$protobuf.IConversionOptions} [options] Conversion options
         * @returns {Object.<string,*>} Plain object
         */
        Assembly.toObject = function toObject(message, options) {
            if (!options) options = {}
            let object = {}
            if (options.defaults) {
                object.info = null
                object.data = null
                object.dynamic = false
                object.physicalData = null
                object.designHierarchy = null
                object.jointHierarchy = null
                object.transform = null
                object.thumbnail = null
            }
            if (message.info != null && Object.hasOwn(message, "info"))
                object.info = $root.mirabuf.Info.toObject(message.info, options)
            if (message.data != null && Object.hasOwn(message, "data"))
                object.data = $root.mirabuf.AssemblyData.toObject(message.data, options)
            if (message.dynamic != null && Object.hasOwn(message, "dynamic")) object.dynamic = message.dynamic
            if (message.physicalData != null && Object.hasOwn(message, "physicalData"))
                object.physicalData = $root.mirabuf.PhysicalProperties.toObject(message.physicalData, options)
            if (message.designHierarchy != null && Object.hasOwn(message, "designHierarchy"))
                object.designHierarchy = $root.mirabuf.GraphContainer.toObject(message.designHierarchy, options)
            if (message.jointHierarchy != null && Object.hasOwn(message, "jointHierarchy"))
                object.jointHierarchy = $root.mirabuf.GraphContainer.toObject(message.jointHierarchy, options)
            if (message.transform != null && Object.hasOwn(message, "transform"))
                object.transform = $root.mirabuf.Transform.toObject(message.transform, options)
            if (message.thumbnail != null && Object.hasOwn(message, "thumbnail"))
                object.thumbnail = $root.mirabuf.Thumbnail.toObject(message.thumbnail, options)
            return object
        }

        /**
         * Converts this Assembly to JSON.
         * @function toJSON
         * @memberof mirabuf.Assembly
         * @instance
         * @returns {Object.<string,*>} JSON object
         */
        Assembly.prototype.toJSON = function toJSON() {
            return this.constructor.toObject(this, $protobuf.util.toJSONOptions)
        }

        /**
         * Gets the default type url for Assembly
         * @function getTypeUrl
         * @memberof mirabuf.Assembly
         * @static
         * @param {string} [typeUrlPrefix] your custom typeUrlPrefix(default "type.googleapis.com")
         * @returns {string} The default type url
         */
        Assembly.getTypeUrl = function getTypeUrl(typeUrlPrefix) {
            if (typeUrlPrefix === undefined) {
                typeUrlPrefix = "type.googleapis.com"
            }
            return typeUrlPrefix + "/mirabuf.Assembly"
        }

        return Assembly
    })()

    mirabuf.AssemblyData = (function () {
        /**
         * Properties of an AssemblyData.
         * @memberof mirabuf
         * @interface IAssemblyData
         * @property {mirabuf.IParts|null} [parts] Meshes and Design Objects
         * @property {mirabuf.joint.IJoints|null} [joints] Joint Definition Set
         * @property {mirabuf.material.IMaterials|null} [materials] Appearance and Physical Material Set
         * @property {mirabuf.signal.ISignals|null} [signals] AssemblyData signals
         */

        /**
         * Constructs a new AssemblyData.
         * @memberof mirabuf
         * @classdesc Data used to construct the assembly
         * @implements IAssemblyData
         * @constructor
         * @param {mirabuf.IAssemblyData=} [properties] Properties to set
         */
        function AssemblyData(properties) {
            if (properties)
                for (let keys = Object.keys(properties), i = 0; i < keys.length; ++i)
                    if (properties[keys[i]] != null) this[keys[i]] = properties[keys[i]]
        }

        /**
         * Meshes and Design Objects
         * @member {mirabuf.IParts|null|undefined} parts
         * @memberof mirabuf.AssemblyData
         * @instance
         */
        AssemblyData.prototype.parts = null

        /**
         * Joint Definition Set
         * @member {mirabuf.joint.IJoints|null|undefined} joints
         * @memberof mirabuf.AssemblyData
         * @instance
         */
        AssemblyData.prototype.joints = null

        /**
         * Appearance and Physical Material Set
         * @member {mirabuf.material.IMaterials|null|undefined} materials
         * @memberof mirabuf.AssemblyData
         * @instance
         */
        AssemblyData.prototype.materials = null

        /**
         * AssemblyData signals.
         * @member {mirabuf.signal.ISignals|null|undefined} signals
         * @memberof mirabuf.AssemblyData
         * @instance
         */
        AssemblyData.prototype.signals = null

        /**
         * Creates a new AssemblyData instance using the specified properties.
         * @function create
         * @memberof mirabuf.AssemblyData
         * @static
         * @param {mirabuf.IAssemblyData=} [properties] Properties to set
         * @returns {mirabuf.AssemblyData} AssemblyData instance
         */
        AssemblyData.create = function create(properties) {
            return new AssemblyData(properties)
        }

        /**
         * Encodes the specified AssemblyData message. Does not implicitly {@link mirabuf.AssemblyData.verify|verify} messages.
         * @function encode
         * @memberof mirabuf.AssemblyData
         * @static
         * @param {mirabuf.IAssemblyData} message AssemblyData message or plain object to encode
         * @param {$protobuf.Writer} [writer] Writer to encode to
         * @returns {$protobuf.Writer} Writer
         */
        AssemblyData.encode = function encode(message, writer) {
            if (!writer) writer = $Writer.create()
            if (message.parts != null && Object.hasOwn(message, "parts"))
                $root.mirabuf.Parts.encode(message.parts, writer.uint32(/* id 1, wireType 2 =*/ 10).fork()).ldelim()
            if (message.joints != null && Object.hasOwn(message, "joints"))
                $root.mirabuf.joint.Joints.encode(
                    message.joints,
                    writer.uint32(/* id 2, wireType 2 =*/ 18).fork()
                ).ldelim()
            if (message.materials != null && Object.hasOwn(message, "materials"))
                $root.mirabuf.material.Materials.encode(
                    message.materials,
                    writer.uint32(/* id 3, wireType 2 =*/ 26).fork()
                ).ldelim()
            if (message.signals != null && Object.hasOwn(message, "signals"))
                $root.mirabuf.signal.Signals.encode(
                    message.signals,
                    writer.uint32(/* id 4, wireType 2 =*/ 34).fork()
                ).ldelim()
            return writer
        }

        /**
         * Encodes the specified AssemblyData message, length delimited. Does not implicitly {@link mirabuf.AssemblyData.verify|verify} messages.
         * @function encodeDelimited
         * @memberof mirabuf.AssemblyData
         * @static
         * @param {mirabuf.IAssemblyData} message AssemblyData message or plain object to encode
         * @param {$protobuf.Writer} [writer] Writer to encode to
         * @returns {$protobuf.Writer} Writer
         */
        AssemblyData.encodeDelimited = function encodeDelimited(message, writer) {
            return this.encode(message, writer).ldelim()
        }

        /**
         * Decodes an AssemblyData message from the specified reader or buffer.
         * @function decode
         * @memberof mirabuf.AssemblyData
         * @static
         * @param {$protobuf.Reader|Uint8Array} reader Reader or buffer to decode from
         * @param {number} [length] Message length if known beforehand
         * @returns {mirabuf.AssemblyData} AssemblyData
         * @throws {Error} If the payload is not a reader or valid buffer
         * @throws {$protobuf.util.ProtocolError} If required fields are missing
         */
        AssemblyData.decode = function decode(reader, length) {
            if (!(reader instanceof $Reader)) reader = $Reader.create(reader)
            let end = length === undefined ? reader.len : reader.pos + length,
                message = new $root.mirabuf.AssemblyData()
            while (reader.pos < end) {
                let tag = reader.uint32()
                switch (tag >>> 3) {
                    case 1: {
                        message.parts = $root.mirabuf.Parts.decode(reader, reader.uint32())
                        break
                    }
                    case 2: {
                        message.joints = $root.mirabuf.joint.Joints.decode(reader, reader.uint32())
                        break
                    }
                    case 3: {
                        message.materials = $root.mirabuf.material.Materials.decode(reader, reader.uint32())
                        break
                    }
                    case 4: {
                        message.signals = $root.mirabuf.signal.Signals.decode(reader, reader.uint32())
                        break
                    }
                    default:
                        reader.skipType(tag & 7)
                        break
                }
            }
            return message
        }

        /**
         * Decodes an AssemblyData message from the specified reader or buffer, length delimited.
         * @function decodeDelimited
         * @memberof mirabuf.AssemblyData
         * @static
         * @param {$protobuf.Reader|Uint8Array} reader Reader or buffer to decode from
         * @returns {mirabuf.AssemblyData} AssemblyData
         * @throws {Error} If the payload is not a reader or valid buffer
         * @throws {$protobuf.util.ProtocolError} If required fields are missing
         */
        AssemblyData.decodeDelimited = function decodeDelimited(reader) {
            if (!(reader instanceof $Reader)) reader = new $Reader(reader)
            return this.decode(reader, reader.uint32())
        }

        /**
         * Verifies an AssemblyData message.
         * @function verify
         * @memberof mirabuf.AssemblyData
         * @static
         * @param {Object.<string,*>} message Plain object to verify
         * @returns {string|null} `null` if valid, otherwise the reason why it is not
         */
        AssemblyData.verify = function verify(message) {
            if (typeof message !== "object" || message === null) return "object expected"
            if (message.parts != null && Object.hasOwn(message, "parts")) {
                let error = $root.mirabuf.Parts.verify(message.parts)
                if (error) return "parts." + error
            }
            if (message.joints != null && Object.hasOwn(message, "joints")) {
                let error = $root.mirabuf.joint.Joints.verify(message.joints)
                if (error) return "joints." + error
            }
            if (message.materials != null && Object.hasOwn(message, "materials")) {
                let error = $root.mirabuf.material.Materials.verify(message.materials)
                if (error) return "materials." + error
            }
            if (message.signals != null && Object.hasOwn(message, "signals")) {
                let error = $root.mirabuf.signal.Signals.verify(message.signals)
                if (error) return "signals." + error
            }
            return null
        }

        /**
         * Creates an AssemblyData message from a plain object. Also converts values to their respective internal types.
         * @function fromObject
         * @memberof mirabuf.AssemblyData
         * @static
         * @param {Object.<string,*>} object Plain object
         * @returns {mirabuf.AssemblyData} AssemblyData
         */
        AssemblyData.fromObject = function fromObject(object) {
            if (object instanceof $root.mirabuf.AssemblyData) return object
            let message = new $root.mirabuf.AssemblyData()
            if (object.parts != null) {
                if (typeof object.parts !== "object") throw TypeError(".mirabuf.AssemblyData.parts: object expected")
                message.parts = $root.mirabuf.Parts.fromObject(object.parts)
            }
            if (object.joints != null) {
                if (typeof object.joints !== "object") throw TypeError(".mirabuf.AssemblyData.joints: object expected")
                message.joints = $root.mirabuf.joint.Joints.fromObject(object.joints)
            }
            if (object.materials != null) {
                if (typeof object.materials !== "object")
                    throw TypeError(".mirabuf.AssemblyData.materials: object expected")
                message.materials = $root.mirabuf.material.Materials.fromObject(object.materials)
            }
            if (object.signals != null) {
                if (typeof object.signals !== "object")
                    throw TypeError(".mirabuf.AssemblyData.signals: object expected")
                message.signals = $root.mirabuf.signal.Signals.fromObject(object.signals)
            }
            return message
        }

        /**
         * Creates a plain object from an AssemblyData message. Also converts values to other types if specified.
         * @function toObject
         * @memberof mirabuf.AssemblyData
         * @static
         * @param {mirabuf.AssemblyData} message AssemblyData
         * @param {$protobuf.IConversionOptions} [options] Conversion options
         * @returns {Object.<string,*>} Plain object
         */
        AssemblyData.toObject = function toObject(message, options) {
            if (!options) options = {}
            let object = {}
            if (options.defaults) {
                object.parts = null
                object.joints = null
                object.materials = null
                object.signals = null
            }
            if (message.parts != null && Object.hasOwn(message, "parts"))
                object.parts = $root.mirabuf.Parts.toObject(message.parts, options)
            if (message.joints != null && Object.hasOwn(message, "joints"))
                object.joints = $root.mirabuf.joint.Joints.toObject(message.joints, options)
            if (message.materials != null && Object.hasOwn(message, "materials"))
                object.materials = $root.mirabuf.material.Materials.toObject(message.materials, options)
            if (message.signals != null && Object.hasOwn(message, "signals"))
                object.signals = $root.mirabuf.signal.Signals.toObject(message.signals, options)
            return object
        }

        /**
         * Converts this AssemblyData to JSON.
         * @function toJSON
         * @memberof mirabuf.AssemblyData
         * @instance
         * @returns {Object.<string,*>} JSON object
         */
        AssemblyData.prototype.toJSON = function toJSON() {
            return this.constructor.toObject(this, $protobuf.util.toJSONOptions)
        }

        /**
         * Gets the default type url for AssemblyData
         * @function getTypeUrl
         * @memberof mirabuf.AssemblyData
         * @static
         * @param {string} [typeUrlPrefix] your custom typeUrlPrefix(default "type.googleapis.com")
         * @returns {string} The default type url
         */
        AssemblyData.getTypeUrl = function getTypeUrl(typeUrlPrefix) {
            if (typeUrlPrefix === undefined) {
                typeUrlPrefix = "type.googleapis.com"
            }
            return typeUrlPrefix + "/mirabuf.AssemblyData"
        }

        return AssemblyData
    })()

    mirabuf.Parts = (function () {
        /**
         * Properties of a Parts.
         * @memberof mirabuf
         * @interface IParts
         * @property {mirabuf.IInfo|null} [info] Part name, version, GUID
         * @property {Object.<string,mirabuf.IPartDefinition>|null} [partDefinitions] Map of the Exported Part Definitions
         * @property {Object.<string,mirabuf.IPartInstance>|null} [partInstances] Map of the Exported Parts that make up the object
         * @property {mirabuf.IUserData|null} [userData] other associated data that can be used
         */

        /**
         * Constructs a new Parts.
         * @memberof mirabuf
         * @classdesc Represents a Parts.
         * @implements IParts
         * @constructor
         * @param {mirabuf.IParts=} [properties] Properties to set
         */
        function Parts(properties) {
            this.partDefinitions = {}
            this.partInstances = {}
            if (properties)
                for (let keys = Object.keys(properties), i = 0; i < keys.length; ++i)
                    if (properties[keys[i]] != null) this[keys[i]] = properties[keys[i]]
        }

        /**
         * Part name, version, GUID
         * @member {mirabuf.IInfo|null|undefined} info
         * @memberof mirabuf.Parts
         * @instance
         */
        Parts.prototype.info = null

        /**
         * Map of the Exported Part Definitions
         * @member {Object.<string,mirabuf.IPartDefinition>} partDefinitions
         * @memberof mirabuf.Parts
         * @instance
         */
        Parts.prototype.partDefinitions = $util.emptyObject

        /**
         * Map of the Exported Parts that make up the object
         * @member {Object.<string,mirabuf.IPartInstance>} partInstances
         * @memberof mirabuf.Parts
         * @instance
         */
        Parts.prototype.partInstances = $util.emptyObject

        /**
         * other associated data that can be used
         * @member {mirabuf.IUserData|null|undefined} userData
         * @memberof mirabuf.Parts
         * @instance
         */
        Parts.prototype.userData = null

        /**
         * Creates a new Parts instance using the specified properties.
         * @function create
         * @memberof mirabuf.Parts
         * @static
         * @param {mirabuf.IParts=} [properties] Properties to set
         * @returns {mirabuf.Parts} Parts instance
         */
        Parts.create = function create(properties) {
            return new Parts(properties)
        }

        /**
         * Encodes the specified Parts message. Does not implicitly {@link mirabuf.Parts.verify|verify} messages.
         * @function encode
         * @memberof mirabuf.Parts
         * @static
         * @param {mirabuf.IParts} message Parts message or plain object to encode
         * @param {$protobuf.Writer} [writer] Writer to encode to
         * @returns {$protobuf.Writer} Writer
         */
        Parts.encode = function encode(message, writer) {
            if (!writer) writer = $Writer.create()
            if (message.info != null && Object.hasOwn(message, "info"))
                $root.mirabuf.Info.encode(message.info, writer.uint32(/* id 1, wireType 2 =*/ 10).fork()).ldelim()
            if (message.partDefinitions != null && Object.hasOwn(message, "partDefinitions"))
                for (let keys = Object.keys(message.partDefinitions), i = 0; i < keys.length; ++i) {
                    writer.uint32(/* id 2, wireType 2 =*/ 18).fork().uint32(/* id 1, wireType 2 =*/ 10).string(keys[i])
                    $root.mirabuf.PartDefinition.encode(
                        message.partDefinitions[keys[i]],
                        writer.uint32(/* id 2, wireType 2 =*/ 18).fork()
                    )
                        .ldelim()
                        .ldelim()
                }
            if (message.partInstances != null && Object.hasOwn(message, "partInstances"))
                for (let keys = Object.keys(message.partInstances), i = 0; i < keys.length; ++i) {
                    writer.uint32(/* id 3, wireType 2 =*/ 26).fork().uint32(/* id 1, wireType 2 =*/ 10).string(keys[i])
                    $root.mirabuf.PartInstance.encode(
                        message.partInstances[keys[i]],
                        writer.uint32(/* id 2, wireType 2 =*/ 18).fork()
                    )
                        .ldelim()
                        .ldelim()
                }
            if (message.userData != null && Object.hasOwn(message, "userData"))
                $root.mirabuf.UserData.encode(
                    message.userData,
                    writer.uint32(/* id 4, wireType 2 =*/ 34).fork()
                ).ldelim()
            return writer
        }

        /**
         * Encodes the specified Parts message, length delimited. Does not implicitly {@link mirabuf.Parts.verify|verify} messages.
         * @function encodeDelimited
         * @memberof mirabuf.Parts
         * @static
         * @param {mirabuf.IParts} message Parts message or plain object to encode
         * @param {$protobuf.Writer} [writer] Writer to encode to
         * @returns {$protobuf.Writer} Writer
         */
        Parts.encodeDelimited = function encodeDelimited(message, writer) {
            return this.encode(message, writer).ldelim()
        }

        /**
         * Decodes a Parts message from the specified reader or buffer.
         * @function decode
         * @memberof mirabuf.Parts
         * @static
         * @param {$protobuf.Reader|Uint8Array} reader Reader or buffer to decode from
         * @param {number} [length] Message length if known beforehand
         * @returns {mirabuf.Parts} Parts
         * @throws {Error} If the payload is not a reader or valid buffer
         * @throws {$protobuf.util.ProtocolError} If required fields are missing
         */
        Parts.decode = function decode(reader, length) {
            if (!(reader instanceof $Reader)) reader = $Reader.create(reader)
            let end = length === undefined ? reader.len : reader.pos + length,
                message = new $root.mirabuf.Parts(),
                key,
                value
            while (reader.pos < end) {
                let tag = reader.uint32()
                switch (tag >>> 3) {
                    case 1: {
                        message.info = $root.mirabuf.Info.decode(reader, reader.uint32())
                        break
                    }
                    case 2: {
                        if (message.partDefinitions === $util.emptyObject) message.partDefinitions = {}
                        let end2 = reader.uint32() + reader.pos
                        key = ""
                        value = null
                        while (reader.pos < end2) {
                            let tag2 = reader.uint32()
                            switch (tag2 >>> 3) {
                                case 1:
                                    key = reader.string()
                                    break
                                case 2:
                                    value = $root.mirabuf.PartDefinition.decode(reader, reader.uint32())
                                    break
                                default:
                                    reader.skipType(tag2 & 7)
                                    break
                            }
                        }
                        message.partDefinitions[key] = value
                        break
                    }
                    case 3: {
                        if (message.partInstances === $util.emptyObject) message.partInstances = {}
                        let end2 = reader.uint32() + reader.pos
                        key = ""
                        value = null
                        while (reader.pos < end2) {
                            let tag2 = reader.uint32()
                            switch (tag2 >>> 3) {
                                case 1:
                                    key = reader.string()
                                    break
                                case 2:
                                    value = $root.mirabuf.PartInstance.decode(reader, reader.uint32())
                                    break
                                default:
                                    reader.skipType(tag2 & 7)
                                    break
                            }
                        }
                        message.partInstances[key] = value
                        break
                    }
                    case 4: {
                        message.userData = $root.mirabuf.UserData.decode(reader, reader.uint32())
                        break
                    }
                    default:
                        reader.skipType(tag & 7)
                        break
                }
            }
            return message
        }

        /**
         * Decodes a Parts message from the specified reader or buffer, length delimited.
         * @function decodeDelimited
         * @memberof mirabuf.Parts
         * @static
         * @param {$protobuf.Reader|Uint8Array} reader Reader or buffer to decode from
         * @returns {mirabuf.Parts} Parts
         * @throws {Error} If the payload is not a reader or valid buffer
         * @throws {$protobuf.util.ProtocolError} If required fields are missing
         */
        Parts.decodeDelimited = function decodeDelimited(reader) {
            if (!(reader instanceof $Reader)) reader = new $Reader(reader)
            return this.decode(reader, reader.uint32())
        }

        /**
         * Verifies a Parts message.
         * @function verify
         * @memberof mirabuf.Parts
         * @static
         * @param {Object.<string,*>} message Plain object to verify
         * @returns {string|null} `null` if valid, otherwise the reason why it is not
         */
        Parts.verify = function verify(message) {
            if (typeof message !== "object" || message === null) return "object expected"
            if (message.info != null && Object.hasOwn(message, "info")) {
                let error = $root.mirabuf.Info.verify(message.info)
                if (error) return "info." + error
            }
            if (message.partDefinitions != null && Object.hasOwn(message, "partDefinitions")) {
                if (!$util.isObject(message.partDefinitions)) return "partDefinitions: object expected"
                let key = Object.keys(message.partDefinitions)
                for (let i = 0; i < key.length; ++i) {
                    let error = $root.mirabuf.PartDefinition.verify(message.partDefinitions[key[i]])
                    if (error) return "partDefinitions." + error
                }
            }
            if (message.partInstances != null && Object.hasOwn(message, "partInstances")) {
                if (!$util.isObject(message.partInstances)) return "partInstances: object expected"
                let key = Object.keys(message.partInstances)
                for (let i = 0; i < key.length; ++i) {
                    let error = $root.mirabuf.PartInstance.verify(message.partInstances[key[i]])
                    if (error) return "partInstances." + error
                }
            }
            if (message.userData != null && Object.hasOwn(message, "userData")) {
                let error = $root.mirabuf.UserData.verify(message.userData)
                if (error) return "userData." + error
            }
            return null
        }

        /**
         * Creates a Parts message from a plain object. Also converts values to their respective internal types.
         * @function fromObject
         * @memberof mirabuf.Parts
         * @static
         * @param {Object.<string,*>} object Plain object
         * @returns {mirabuf.Parts} Parts
         */
        Parts.fromObject = function fromObject(object) {
            if (object instanceof $root.mirabuf.Parts) return object
            let message = new $root.mirabuf.Parts()
            if (object.info != null) {
                if (typeof object.info !== "object") throw TypeError(".mirabuf.Parts.info: object expected")
                message.info = $root.mirabuf.Info.fromObject(object.info)
            }
            if (object.partDefinitions) {
                if (typeof object.partDefinitions !== "object")
                    throw TypeError(".mirabuf.Parts.partDefinitions: object expected")
                message.partDefinitions = {}
                for (let keys = Object.keys(object.partDefinitions), i = 0; i < keys.length; ++i) {
                    if (typeof object.partDefinitions[keys[i]] !== "object")
                        throw TypeError(".mirabuf.Parts.partDefinitions: object expected")
                    message.partDefinitions[keys[i]] = $root.mirabuf.PartDefinition.fromObject(
                        object.partDefinitions[keys[i]]
                    )
                }
            }
            if (object.partInstances) {
                if (typeof object.partInstances !== "object")
                    throw TypeError(".mirabuf.Parts.partInstances: object expected")
                message.partInstances = {}
                for (let keys = Object.keys(object.partInstances), i = 0; i < keys.length; ++i) {
                    if (typeof object.partInstances[keys[i]] !== "object")
                        throw TypeError(".mirabuf.Parts.partInstances: object expected")
                    message.partInstances[keys[i]] = $root.mirabuf.PartInstance.fromObject(
                        object.partInstances[keys[i]]
                    )
                }
            }
            if (object.userData != null) {
                if (typeof object.userData !== "object") throw TypeError(".mirabuf.Parts.userData: object expected")
                message.userData = $root.mirabuf.UserData.fromObject(object.userData)
            }
            return message
        }

        /**
         * Creates a plain object from a Parts message. Also converts values to other types if specified.
         * @function toObject
         * @memberof mirabuf.Parts
         * @static
         * @param {mirabuf.Parts} message Parts
         * @param {$protobuf.IConversionOptions} [options] Conversion options
         * @returns {Object.<string,*>} Plain object
         */
        Parts.toObject = function toObject(message, options) {
            if (!options) options = {}
            let object = {}
            if (options.objects || options.defaults) {
                object.partDefinitions = {}
                object.partInstances = {}
            }
            if (options.defaults) {
                object.info = null
                object.userData = null
            }
            if (message.info != null && Object.hasOwn(message, "info"))
                object.info = $root.mirabuf.Info.toObject(message.info, options)
            let keys2
            if (message.partDefinitions && (keys2 = Object.keys(message.partDefinitions)).length) {
                object.partDefinitions = {}
                for (let j = 0; j < keys2.length; ++j)
                    object.partDefinitions[keys2[j]] = $root.mirabuf.PartDefinition.toObject(
                        message.partDefinitions[keys2[j]],
                        options
                    )
            }
            if (message.partInstances && (keys2 = Object.keys(message.partInstances)).length) {
                object.partInstances = {}
                for (let j = 0; j < keys2.length; ++j)
                    object.partInstances[keys2[j]] = $root.mirabuf.PartInstance.toObject(
                        message.partInstances[keys2[j]],
                        options
                    )
            }
            if (message.userData != null && Object.hasOwn(message, "userData"))
                object.userData = $root.mirabuf.UserData.toObject(message.userData, options)
            return object
        }

        /**
         * Converts this Parts to JSON.
         * @function toJSON
         * @memberof mirabuf.Parts
         * @instance
         * @returns {Object.<string,*>} JSON object
         */
        Parts.prototype.toJSON = function toJSON() {
            return this.constructor.toObject(this, $protobuf.util.toJSONOptions)
        }

        /**
         * Gets the default type url for Parts
         * @function getTypeUrl
         * @memberof mirabuf.Parts
         * @static
         * @param {string} [typeUrlPrefix] your custom typeUrlPrefix(default "type.googleapis.com")
         * @returns {string} The default type url
         */
        Parts.getTypeUrl = function getTypeUrl(typeUrlPrefix) {
            if (typeUrlPrefix === undefined) {
                typeUrlPrefix = "type.googleapis.com"
            }
            return typeUrlPrefix + "/mirabuf.Parts"
        }

        return Parts
    })()

    mirabuf.PartDefinition = (function () {
        /**
         * Properties of a PartDefinition.
         * @memberof mirabuf
         * @interface IPartDefinition
         * @property {mirabuf.IInfo|null} [info] Information about version - id - name
         * @property {mirabuf.IPhysicalProperties|null} [physicalData] Physical data associated with Part
         * @property {mirabuf.ITransform|null} [baseTransform] Base Transform applied - Most Likely Identity Matrix
         * @property {Array.<mirabuf.IBody>|null} [bodies] Mesh Bodies to populate part
         * @property {boolean|null} [dynamic] Optional value to state whether an object is a dynamic object in a static assembly - all children are also considered overriden
         * @property {number|null} [frictionOverride] Optional value for overriding the friction value 0-1
         * @property {number|null} [massOverride] Optional value for overriding an indiviaul object's mass
         */

        /**
         * Constructs a new PartDefinition.
         * @memberof mirabuf
         * @classdesc Part Definition
         * Unique Definition of a part that can be replicated.
         * Useful for keeping the object counter down in the scene.
         * @implements IPartDefinition
         * @constructor
         * @param {mirabuf.IPartDefinition=} [properties] Properties to set
         */
        function PartDefinition(properties) {
            this.bodies = []
            if (properties)
                for (let keys = Object.keys(properties), i = 0; i < keys.length; ++i)
                    if (properties[keys[i]] != null) this[keys[i]] = properties[keys[i]]
        }

        /**
         * Information about version - id - name
         * @member {mirabuf.IInfo|null|undefined} info
         * @memberof mirabuf.PartDefinition
         * @instance
         */
        PartDefinition.prototype.info = null

        /**
         * Physical data associated with Part
         * @member {mirabuf.IPhysicalProperties|null|undefined} physicalData
         * @memberof mirabuf.PartDefinition
         * @instance
         */
        PartDefinition.prototype.physicalData = null

        /**
         * Base Transform applied - Most Likely Identity Matrix
         * @member {mirabuf.ITransform|null|undefined} baseTransform
         * @memberof mirabuf.PartDefinition
         * @instance
         */
        PartDefinition.prototype.baseTransform = null

        /**
         * Mesh Bodies to populate part
         * @member {Array.<mirabuf.IBody>} bodies
         * @memberof mirabuf.PartDefinition
         * @instance
         */
        PartDefinition.prototype.bodies = $util.emptyArray

        /**
         * Optional value to state whether an object is a dynamic object in a static assembly - all children are also considered overriden
         * @member {boolean} dynamic
         * @memberof mirabuf.PartDefinition
         * @instance
         */
        PartDefinition.prototype.dynamic = false

        /**
         * Optional value for overriding the friction value 0-1
         * @member {number} frictionOverride
         * @memberof mirabuf.PartDefinition
         * @instance
         */
        PartDefinition.prototype.frictionOverride = 0

        /**
         * Optional value for overriding an indiviaul object's mass
         * @member {number} massOverride
         * @memberof mirabuf.PartDefinition
         * @instance
         */
        PartDefinition.prototype.massOverride = 0

        /**
         * Creates a new PartDefinition instance using the specified properties.
         * @function create
         * @memberof mirabuf.PartDefinition
         * @static
         * @param {mirabuf.IPartDefinition=} [properties] Properties to set
         * @returns {mirabuf.PartDefinition} PartDefinition instance
         */
        PartDefinition.create = function create(properties) {
            return new PartDefinition(properties)
        }

        /**
         * Encodes the specified PartDefinition message. Does not implicitly {@link mirabuf.PartDefinition.verify|verify} messages.
         * @function encode
         * @memberof mirabuf.PartDefinition
         * @static
         * @param {mirabuf.IPartDefinition} message PartDefinition message or plain object to encode
         * @param {$protobuf.Writer} [writer] Writer to encode to
         * @returns {$protobuf.Writer} Writer
         */
        PartDefinition.encode = function encode(message, writer) {
            if (!writer) writer = $Writer.create()
            if (message.info != null && Object.hasOwn(message, "info"))
                $root.mirabuf.Info.encode(message.info, writer.uint32(/* id 1, wireType 2 =*/ 10).fork()).ldelim()
            if (message.physicalData != null && Object.hasOwn(message, "physicalData"))
                $root.mirabuf.PhysicalProperties.encode(
                    message.physicalData,
                    writer.uint32(/* id 2, wireType 2 =*/ 18).fork()
                ).ldelim()
            if (message.baseTransform != null && Object.hasOwn(message, "baseTransform"))
                $root.mirabuf.Transform.encode(
                    message.baseTransform,
                    writer.uint32(/* id 3, wireType 2 =*/ 26).fork()
                ).ldelim()
            if (message.bodies != null && message.bodies.length)
                for (let i = 0; i < message.bodies.length; ++i)
                    $root.mirabuf.Body.encode(
                        message.bodies[i],
                        writer.uint32(/* id 4, wireType 2 =*/ 34).fork()
                    ).ldelim()
            if (message.dynamic != null && Object.hasOwn(message, "dynamic"))
                writer.uint32(/* id 5, wireType 0 =*/ 40).bool(message.dynamic)
            if (message.frictionOverride != null && Object.hasOwn(message, "frictionOverride"))
                writer.uint32(/* id 6, wireType 5 =*/ 53).float(message.frictionOverride)
            if (message.massOverride != null && Object.hasOwn(message, "massOverride"))
                writer.uint32(/* id 7, wireType 5 =*/ 61).float(message.massOverride)
            return writer
        }

        /**
         * Encodes the specified PartDefinition message, length delimited. Does not implicitly {@link mirabuf.PartDefinition.verify|verify} messages.
         * @function encodeDelimited
         * @memberof mirabuf.PartDefinition
         * @static
         * @param {mirabuf.IPartDefinition} message PartDefinition message or plain object to encode
         * @param {$protobuf.Writer} [writer] Writer to encode to
         * @returns {$protobuf.Writer} Writer
         */
        PartDefinition.encodeDelimited = function encodeDelimited(message, writer) {
            return this.encode(message, writer).ldelim()
        }

        /**
         * Decodes a PartDefinition message from the specified reader or buffer.
         * @function decode
         * @memberof mirabuf.PartDefinition
         * @static
         * @param {$protobuf.Reader|Uint8Array} reader Reader or buffer to decode from
         * @param {number} [length] Message length if known beforehand
         * @returns {mirabuf.PartDefinition} PartDefinition
         * @throws {Error} If the payload is not a reader or valid buffer
         * @throws {$protobuf.util.ProtocolError} If required fields are missing
         */
        PartDefinition.decode = function decode(reader, length) {
            if (!(reader instanceof $Reader)) reader = $Reader.create(reader)
            let end = length === undefined ? reader.len : reader.pos + length,
                message = new $root.mirabuf.PartDefinition()
            while (reader.pos < end) {
                let tag = reader.uint32()
                switch (tag >>> 3) {
                    case 1: {
                        message.info = $root.mirabuf.Info.decode(reader, reader.uint32())
                        break
                    }
                    case 2: {
                        message.physicalData = $root.mirabuf.PhysicalProperties.decode(reader, reader.uint32())
                        break
                    }
                    case 3: {
                        message.baseTransform = $root.mirabuf.Transform.decode(reader, reader.uint32())
                        break
                    }
                    case 4: {
                        if (!(message.bodies && message.bodies.length)) message.bodies = []
                        message.bodies.push($root.mirabuf.Body.decode(reader, reader.uint32()))
                        break
                    }
                    case 5: {
                        message.dynamic = reader.bool()
                        break
                    }
                    case 6: {
                        message.frictionOverride = reader.float()
                        break
                    }
                    case 7: {
                        message.massOverride = reader.float()
                        break
                    }
                    default:
                        reader.skipType(tag & 7)
                        break
                }
            }
            return message
        }

        /**
         * Decodes a PartDefinition message from the specified reader or buffer, length delimited.
         * @function decodeDelimited
         * @memberof mirabuf.PartDefinition
         * @static
         * @param {$protobuf.Reader|Uint8Array} reader Reader or buffer to decode from
         * @returns {mirabuf.PartDefinition} PartDefinition
         * @throws {Error} If the payload is not a reader or valid buffer
         * @throws {$protobuf.util.ProtocolError} If required fields are missing
         */
        PartDefinition.decodeDelimited = function decodeDelimited(reader) {
            if (!(reader instanceof $Reader)) reader = new $Reader(reader)
            return this.decode(reader, reader.uint32())
        }

        /**
         * Verifies a PartDefinition message.
         * @function verify
         * @memberof mirabuf.PartDefinition
         * @static
         * @param {Object.<string,*>} message Plain object to verify
         * @returns {string|null} `null` if valid, otherwise the reason why it is not
         */
        PartDefinition.verify = function verify(message) {
            if (typeof message !== "object" || message === null) return "object expected"
            if (message.info != null && Object.hasOwn(message, "info")) {
                let error = $root.mirabuf.Info.verify(message.info)
                if (error) return "info." + error
            }
            if (message.physicalData != null && Object.hasOwn(message, "physicalData")) {
                let error = $root.mirabuf.PhysicalProperties.verify(message.physicalData)
                if (error) return "physicalData." + error
            }
            if (message.baseTransform != null && Object.hasOwn(message, "baseTransform")) {
                let error = $root.mirabuf.Transform.verify(message.baseTransform)
                if (error) return "baseTransform." + error
            }
            if (message.bodies != null && Object.hasOwn(message, "bodies")) {
                if (!Array.isArray(message.bodies)) return "bodies: array expected"
                for (let i = 0; i < message.bodies.length; ++i) {
                    let error = $root.mirabuf.Body.verify(message.bodies[i])
                    if (error) return "bodies." + error
                }
            }
            if (message.dynamic != null && Object.hasOwn(message, "dynamic"))
                if (typeof message.dynamic !== "boolean") return "dynamic: boolean expected"
            if (message.frictionOverride != null && Object.hasOwn(message, "frictionOverride"))
                if (typeof message.frictionOverride !== "number") return "frictionOverride: number expected"
            if (message.massOverride != null && Object.hasOwn(message, "massOverride"))
                if (typeof message.massOverride !== "number") return "massOverride: number expected"
            return null
        }

        /**
         * Creates a PartDefinition message from a plain object. Also converts values to their respective internal types.
         * @function fromObject
         * @memberof mirabuf.PartDefinition
         * @static
         * @param {Object.<string,*>} object Plain object
         * @returns {mirabuf.PartDefinition} PartDefinition
         */
        PartDefinition.fromObject = function fromObject(object) {
            if (object instanceof $root.mirabuf.PartDefinition) return object
            let message = new $root.mirabuf.PartDefinition()
            if (object.info != null) {
                if (typeof object.info !== "object") throw TypeError(".mirabuf.PartDefinition.info: object expected")
                message.info = $root.mirabuf.Info.fromObject(object.info)
            }
            if (object.physicalData != null) {
                if (typeof object.physicalData !== "object")
                    throw TypeError(".mirabuf.PartDefinition.physicalData: object expected")
                message.physicalData = $root.mirabuf.PhysicalProperties.fromObject(object.physicalData)
            }
            if (object.baseTransform != null) {
                if (typeof object.baseTransform !== "object")
                    throw TypeError(".mirabuf.PartDefinition.baseTransform: object expected")
                message.baseTransform = $root.mirabuf.Transform.fromObject(object.baseTransform)
            }
            if (object.bodies) {
                if (!Array.isArray(object.bodies)) throw TypeError(".mirabuf.PartDefinition.bodies: array expected")
                message.bodies = []
                for (let i = 0; i < object.bodies.length; ++i) {
                    if (typeof object.bodies[i] !== "object")
                        throw TypeError(".mirabuf.PartDefinition.bodies: object expected")
                    message.bodies[i] = $root.mirabuf.Body.fromObject(object.bodies[i])
                }
            }
            if (object.dynamic != null) message.dynamic = Boolean(object.dynamic)
            if (object.frictionOverride != null) message.frictionOverride = Number(object.frictionOverride)
            if (object.massOverride != null) message.massOverride = Number(object.massOverride)
            return message
        }

        /**
         * Creates a plain object from a PartDefinition message. Also converts values to other types if specified.
         * @function toObject
         * @memberof mirabuf.PartDefinition
         * @static
         * @param {mirabuf.PartDefinition} message PartDefinition
         * @param {$protobuf.IConversionOptions} [options] Conversion options
         * @returns {Object.<string,*>} Plain object
         */
        PartDefinition.toObject = function toObject(message, options) {
            if (!options) options = {}
            let object = {}
            if (options.arrays || options.defaults) object.bodies = []
            if (options.defaults) {
                object.info = null
                object.physicalData = null
                object.baseTransform = null
                object.dynamic = false
                object.frictionOverride = 0
                object.massOverride = 0
            }
            if (message.info != null && Object.hasOwn(message, "info"))
                object.info = $root.mirabuf.Info.toObject(message.info, options)
            if (message.physicalData != null && Object.hasOwn(message, "physicalData"))
                object.physicalData = $root.mirabuf.PhysicalProperties.toObject(message.physicalData, options)
            if (message.baseTransform != null && Object.hasOwn(message, "baseTransform"))
                object.baseTransform = $root.mirabuf.Transform.toObject(message.baseTransform, options)
            if (message.bodies && message.bodies.length) {
                object.bodies = []
                for (let j = 0; j < message.bodies.length; ++j)
                    object.bodies[j] = $root.mirabuf.Body.toObject(message.bodies[j], options)
            }
            if (message.dynamic != null && Object.hasOwn(message, "dynamic")) object.dynamic = message.dynamic
            if (message.frictionOverride != null && Object.hasOwn(message, "frictionOverride"))
                object.frictionOverride =
                    options.json && !isFinite(message.frictionOverride)
                        ? String(message.frictionOverride)
                        : message.frictionOverride
            if (message.massOverride != null && Object.hasOwn(message, "massOverride"))
                object.massOverride =
                    options.json && !isFinite(message.massOverride)
                        ? String(message.massOverride)
                        : message.massOverride
            return object
        }

        /**
         * Converts this PartDefinition to JSON.
         * @function toJSON
         * @memberof mirabuf.PartDefinition
         * @instance
         * @returns {Object.<string,*>} JSON object
         */
        PartDefinition.prototype.toJSON = function toJSON() {
            return this.constructor.toObject(this, $protobuf.util.toJSONOptions)
        }

        /**
         * Gets the default type url for PartDefinition
         * @function getTypeUrl
         * @memberof mirabuf.PartDefinition
         * @static
         * @param {string} [typeUrlPrefix] your custom typeUrlPrefix(default "type.googleapis.com")
         * @returns {string} The default type url
         */
        PartDefinition.getTypeUrl = function getTypeUrl(typeUrlPrefix) {
            if (typeUrlPrefix === undefined) {
                typeUrlPrefix = "type.googleapis.com"
            }
            return typeUrlPrefix + "/mirabuf.PartDefinition"
        }

        return PartDefinition
    })()

    mirabuf.PartInstance = (function () {
        /**
         * Properties of a PartInstance.
         * @memberof mirabuf
         * @interface IPartInstance
         * @property {mirabuf.IInfo|null} [info] PartInstance info
         * @property {string|null} [partDefinitionReference] Reference to the Part Definition defined in Assembly Data
         * @property {mirabuf.ITransform|null} [transform] Overriding the object transform (moves the part from the def) - in design hierarchy context
         * @property {mirabuf.ITransform|null} [globalTransform] Position transform from a global scope
         * @property {Array.<string>|null} [joints] Joints that interact with this element
         * @property {string|null} [appearance] PartInstance appearance
         * @property {string|null} [physicalMaterial] Physical Material Reference to link to `Materials->PhysicalMaterial->Info->id`
         * @property {boolean|null} [skipCollider] Flag that if enabled indicates we should skip generating a collider, defaults to FALSE or undefined
         */

        /**
         * Constructs a new PartInstance.
         * @memberof mirabuf
         * @classdesc Represents a PartInstance.
         * @implements IPartInstance
         * @constructor
         * @param {mirabuf.IPartInstance=} [properties] Properties to set
         */
        function PartInstance(properties) {
            this.joints = []
            if (properties)
                for (let keys = Object.keys(properties), i = 0; i < keys.length; ++i)
                    if (properties[keys[i]] != null) this[keys[i]] = properties[keys[i]]
        }

        /**
         * PartInstance info.
         * @member {mirabuf.IInfo|null|undefined} info
         * @memberof mirabuf.PartInstance
         * @instance
         */
        PartInstance.prototype.info = null

        /**
         * Reference to the Part Definition defined in Assembly Data
         * @member {string} partDefinitionReference
         * @memberof mirabuf.PartInstance
         * @instance
         */
        PartInstance.prototype.partDefinitionReference = ""

        /**
         * Overriding the object transform (moves the part from the def) - in design hierarchy context
         * @member {mirabuf.ITransform|null|undefined} transform
         * @memberof mirabuf.PartInstance
         * @instance
         */
        PartInstance.prototype.transform = null

        /**
         * Position transform from a global scope
         * @member {mirabuf.ITransform|null|undefined} globalTransform
         * @memberof mirabuf.PartInstance
         * @instance
         */
        PartInstance.prototype.globalTransform = null

        /**
         * Joints that interact with this element
         * @member {Array.<string>} joints
         * @memberof mirabuf.PartInstance
         * @instance
         */
        PartInstance.prototype.joints = $util.emptyArray

        /**
         * PartInstance appearance.
         * @member {string} appearance
         * @memberof mirabuf.PartInstance
         * @instance
         */
        PartInstance.prototype.appearance = ""

        /**
         * Physical Material Reference to link to `Materials->PhysicalMaterial->Info->id`
         * @member {string} physicalMaterial
         * @memberof mirabuf.PartInstance
         * @instance
         */
        PartInstance.prototype.physicalMaterial = ""

        /**
         * Flag that if enabled indicates we should skip generating a collider, defaults to FALSE or undefined
         * @member {boolean} skipCollider
         * @memberof mirabuf.PartInstance
         * @instance
         */
        PartInstance.prototype.skipCollider = false

        /**
         * Creates a new PartInstance instance using the specified properties.
         * @function create
         * @memberof mirabuf.PartInstance
         * @static
         * @param {mirabuf.IPartInstance=} [properties] Properties to set
         * @returns {mirabuf.PartInstance} PartInstance instance
         */
        PartInstance.create = function create(properties) {
            return new PartInstance(properties)
        }

        /**
         * Encodes the specified PartInstance message. Does not implicitly {@link mirabuf.PartInstance.verify|verify} messages.
         * @function encode
         * @memberof mirabuf.PartInstance
         * @static
         * @param {mirabuf.IPartInstance} message PartInstance message or plain object to encode
         * @param {$protobuf.Writer} [writer] Writer to encode to
         * @returns {$protobuf.Writer} Writer
         */
        PartInstance.encode = function encode(message, writer) {
            if (!writer) writer = $Writer.create()
            if (message.info != null && Object.hasOwn(message, "info"))
                $root.mirabuf.Info.encode(message.info, writer.uint32(/* id 1, wireType 2 =*/ 10).fork()).ldelim()
            if (message.partDefinitionReference != null && Object.hasOwn(message, "partDefinitionReference"))
                writer.uint32(/* id 2, wireType 2 =*/ 18).string(message.partDefinitionReference)
            if (message.transform != null && Object.hasOwn(message, "transform"))
                $root.mirabuf.Transform.encode(
                    message.transform,
                    writer.uint32(/* id 3, wireType 2 =*/ 26).fork()
                ).ldelim()
            if (message.globalTransform != null && Object.hasOwn(message, "globalTransform"))
                $root.mirabuf.Transform.encode(
                    message.globalTransform,
                    writer.uint32(/* id 4, wireType 2 =*/ 34).fork()
                ).ldelim()
            if (message.joints != null && message.joints.length)
                for (let i = 0; i < message.joints.length; ++i)
                    writer.uint32(/* id 5, wireType 2 =*/ 42).string(message.joints[i])
            if (message.appearance != null && Object.hasOwn(message, "appearance"))
                writer.uint32(/* id 6, wireType 2 =*/ 50).string(message.appearance)
            if (message.physicalMaterial != null && Object.hasOwn(message, "physicalMaterial"))
                writer.uint32(/* id 7, wireType 2 =*/ 58).string(message.physicalMaterial)
            if (message.skipCollider != null && Object.hasOwn(message, "skipCollider"))
                writer.uint32(/* id 8, wireType 0 =*/ 64).bool(message.skipCollider)
            return writer
        }

        /**
         * Encodes the specified PartInstance message, length delimited. Does not implicitly {@link mirabuf.PartInstance.verify|verify} messages.
         * @function encodeDelimited
         * @memberof mirabuf.PartInstance
         * @static
         * @param {mirabuf.IPartInstance} message PartInstance message or plain object to encode
         * @param {$protobuf.Writer} [writer] Writer to encode to
         * @returns {$protobuf.Writer} Writer
         */
        PartInstance.encodeDelimited = function encodeDelimited(message, writer) {
            return this.encode(message, writer).ldelim()
        }

        /**
         * Decodes a PartInstance message from the specified reader or buffer.
         * @function decode
         * @memberof mirabuf.PartInstance
         * @static
         * @param {$protobuf.Reader|Uint8Array} reader Reader or buffer to decode from
         * @param {number} [length] Message length if known beforehand
         * @returns {mirabuf.PartInstance} PartInstance
         * @throws {Error} If the payload is not a reader or valid buffer
         * @throws {$protobuf.util.ProtocolError} If required fields are missing
         */
        PartInstance.decode = function decode(reader, length) {
            if (!(reader instanceof $Reader)) reader = $Reader.create(reader)
            let end = length === undefined ? reader.len : reader.pos + length,
                message = new $root.mirabuf.PartInstance()
            while (reader.pos < end) {
                let tag = reader.uint32()
                switch (tag >>> 3) {
                    case 1: {
                        message.info = $root.mirabuf.Info.decode(reader, reader.uint32())
                        break
                    }
                    case 2: {
                        message.partDefinitionReference = reader.string()
                        break
                    }
                    case 3: {
                        message.transform = $root.mirabuf.Transform.decode(reader, reader.uint32())
                        break
                    }
                    case 4: {
                        message.globalTransform = $root.mirabuf.Transform.decode(reader, reader.uint32())
                        break
                    }
                    case 5: {
                        if (!(message.joints && message.joints.length)) message.joints = []
                        message.joints.push(reader.string())
                        break
                    }
                    case 6: {
                        message.appearance = reader.string()
                        break
                    }
                    case 7: {
                        message.physicalMaterial = reader.string()
                        break
                    }
                    case 8: {
                        message.skipCollider = reader.bool()
                        break
                    }
                    default:
                        reader.skipType(tag & 7)
                        break
                }
            }
            return message
        }

        /**
         * Decodes a PartInstance message from the specified reader or buffer, length delimited.
         * @function decodeDelimited
         * @memberof mirabuf.PartInstance
         * @static
         * @param {$protobuf.Reader|Uint8Array} reader Reader or buffer to decode from
         * @returns {mirabuf.PartInstance} PartInstance
         * @throws {Error} If the payload is not a reader or valid buffer
         * @throws {$protobuf.util.ProtocolError} If required fields are missing
         */
        PartInstance.decodeDelimited = function decodeDelimited(reader) {
            if (!(reader instanceof $Reader)) reader = new $Reader(reader)
            return this.decode(reader, reader.uint32())
        }

        /**
         * Verifies a PartInstance message.
         * @function verify
         * @memberof mirabuf.PartInstance
         * @static
         * @param {Object.<string,*>} message Plain object to verify
         * @returns {string|null} `null` if valid, otherwise the reason why it is not
         */
        PartInstance.verify = function verify(message) {
            if (typeof message !== "object" || message === null) return "object expected"
            if (message.info != null && Object.hasOwn(message, "info")) {
                let error = $root.mirabuf.Info.verify(message.info)
                if (error) return "info." + error
            }
            if (message.partDefinitionReference != null && Object.hasOwn(message, "partDefinitionReference"))
                if (!$util.isString(message.partDefinitionReference)) return "partDefinitionReference: string expected"
            if (message.transform != null && Object.hasOwn(message, "transform")) {
                let error = $root.mirabuf.Transform.verify(message.transform)
                if (error) return "transform." + error
            }
            if (message.globalTransform != null && Object.hasOwn(message, "globalTransform")) {
                let error = $root.mirabuf.Transform.verify(message.globalTransform)
                if (error) return "globalTransform." + error
            }
            if (message.joints != null && Object.hasOwn(message, "joints")) {
                if (!Array.isArray(message.joints)) return "joints: array expected"
                for (let i = 0; i < message.joints.length; ++i)
                    if (!$util.isString(message.joints[i])) return "joints: string[] expected"
            }
            if (message.appearance != null && Object.hasOwn(message, "appearance"))
                if (!$util.isString(message.appearance)) return "appearance: string expected"
            if (message.physicalMaterial != null && Object.hasOwn(message, "physicalMaterial"))
                if (!$util.isString(message.physicalMaterial)) return "physicalMaterial: string expected"
            if (message.skipCollider != null && Object.hasOwn(message, "skipCollider"))
                if (typeof message.skipCollider !== "boolean") return "skipCollider: boolean expected"
            return null
        }

        /**
         * Creates a PartInstance message from a plain object. Also converts values to their respective internal types.
         * @function fromObject
         * @memberof mirabuf.PartInstance
         * @static
         * @param {Object.<string,*>} object Plain object
         * @returns {mirabuf.PartInstance} PartInstance
         */
        PartInstance.fromObject = function fromObject(object) {
            if (object instanceof $root.mirabuf.PartInstance) return object
            let message = new $root.mirabuf.PartInstance()
            if (object.info != null) {
                if (typeof object.info !== "object") throw TypeError(".mirabuf.PartInstance.info: object expected")
                message.info = $root.mirabuf.Info.fromObject(object.info)
            }
            if (object.partDefinitionReference != null)
                message.partDefinitionReference = String(object.partDefinitionReference)
            if (object.transform != null) {
                if (typeof object.transform !== "object")
                    throw TypeError(".mirabuf.PartInstance.transform: object expected")
                message.transform = $root.mirabuf.Transform.fromObject(object.transform)
            }
            if (object.globalTransform != null) {
                if (typeof object.globalTransform !== "object")
                    throw TypeError(".mirabuf.PartInstance.globalTransform: object expected")
                message.globalTransform = $root.mirabuf.Transform.fromObject(object.globalTransform)
            }
            if (object.joints) {
                if (!Array.isArray(object.joints)) throw TypeError(".mirabuf.PartInstance.joints: array expected")
                message.joints = []
                for (let i = 0; i < object.joints.length; ++i) message.joints[i] = String(object.joints[i])
            }
            if (object.appearance != null) message.appearance = String(object.appearance)
            if (object.physicalMaterial != null) message.physicalMaterial = String(object.physicalMaterial)
            if (object.skipCollider != null) message.skipCollider = Boolean(object.skipCollider)
            return message
        }

        /**
         * Creates a plain object from a PartInstance message. Also converts values to other types if specified.
         * @function toObject
         * @memberof mirabuf.PartInstance
         * @static
         * @param {mirabuf.PartInstance} message PartInstance
         * @param {$protobuf.IConversionOptions} [options] Conversion options
         * @returns {Object.<string,*>} Plain object
         */
        PartInstance.toObject = function toObject(message, options) {
            if (!options) options = {}
            let object = {}
            if (options.arrays || options.defaults) object.joints = []
            if (options.defaults) {
                object.info = null
                object.partDefinitionReference = ""
                object.transform = null
                object.globalTransform = null
                object.appearance = ""
                object.physicalMaterial = ""
                object.skipCollider = false
            }
            if (message.info != null && Object.hasOwn(message, "info"))
                object.info = $root.mirabuf.Info.toObject(message.info, options)
            if (message.partDefinitionReference != null && Object.hasOwn(message, "partDefinitionReference"))
                object.partDefinitionReference = message.partDefinitionReference
            if (message.transform != null && Object.hasOwn(message, "transform"))
                object.transform = $root.mirabuf.Transform.toObject(message.transform, options)
            if (message.globalTransform != null && Object.hasOwn(message, "globalTransform"))
                object.globalTransform = $root.mirabuf.Transform.toObject(message.globalTransform, options)
            if (message.joints && message.joints.length) {
                object.joints = []
                for (let j = 0; j < message.joints.length; ++j) object.joints[j] = message.joints[j]
            }
            if (message.appearance != null && Object.hasOwn(message, "appearance"))
                object.appearance = message.appearance
            if (message.physicalMaterial != null && Object.hasOwn(message, "physicalMaterial"))
                object.physicalMaterial = message.physicalMaterial
            if (message.skipCollider != null && Object.hasOwn(message, "skipCollider"))
                object.skipCollider = message.skipCollider
            return object
        }

        /**
         * Converts this PartInstance to JSON.
         * @function toJSON
         * @memberof mirabuf.PartInstance
         * @instance
         * @returns {Object.<string,*>} JSON object
         */
        PartInstance.prototype.toJSON = function toJSON() {
            return this.constructor.toObject(this, $protobuf.util.toJSONOptions)
        }

        /**
         * Gets the default type url for PartInstance
         * @function getTypeUrl
         * @memberof mirabuf.PartInstance
         * @static
         * @param {string} [typeUrlPrefix] your custom typeUrlPrefix(default "type.googleapis.com")
         * @returns {string} The default type url
         */
        PartInstance.getTypeUrl = function getTypeUrl(typeUrlPrefix) {
            if (typeUrlPrefix === undefined) {
                typeUrlPrefix = "type.googleapis.com"
            }
            return typeUrlPrefix + "/mirabuf.PartInstance"
        }

        return PartInstance
    })()

    mirabuf.Body = (function () {
        /**
         * Properties of a Body.
         * @memberof mirabuf
         * @interface IBody
         * @property {mirabuf.IInfo|null} [info] Body info
         * @property {string|null} [part] Reference to Part Definition
         * @property {mirabuf.ITriangleMesh|null} [triangleMesh] Triangle Mesh for rendering
         * @property {string|null} [appearanceOverride] Override Visual Appearance for the body
         */

        /**
         * Constructs a new Body.
         * @memberof mirabuf
         * @classdesc Represents a Body.
         * @implements IBody
         * @constructor
         * @param {mirabuf.IBody=} [properties] Properties to set
         */
        function Body(properties) {
            if (properties)
                for (let keys = Object.keys(properties), i = 0; i < keys.length; ++i)
                    if (properties[keys[i]] != null) this[keys[i]] = properties[keys[i]]
        }

        /**
         * Body info.
         * @member {mirabuf.IInfo|null|undefined} info
         * @memberof mirabuf.Body
         * @instance
         */
        Body.prototype.info = null

        /**
         * Reference to Part Definition
         * @member {string} part
         * @memberof mirabuf.Body
         * @instance
         */
        Body.prototype.part = ""

        /**
         * Triangle Mesh for rendering
         * @member {mirabuf.ITriangleMesh|null|undefined} triangleMesh
         * @memberof mirabuf.Body
         * @instance
         */
        Body.prototype.triangleMesh = null

        /**
         * Override Visual Appearance for the body
         * @member {string} appearanceOverride
         * @memberof mirabuf.Body
         * @instance
         */
        Body.prototype.appearanceOverride = ""

        /**
         * Creates a new Body instance using the specified properties.
         * @function create
         * @memberof mirabuf.Body
         * @static
         * @param {mirabuf.IBody=} [properties] Properties to set
         * @returns {mirabuf.Body} Body instance
         */
        Body.create = function create(properties) {
            return new Body(properties)
        }

        /**
         * Encodes the specified Body message. Does not implicitly {@link mirabuf.Body.verify|verify} messages.
         * @function encode
         * @memberof mirabuf.Body
         * @static
         * @param {mirabuf.IBody} message Body message or plain object to encode
         * @param {$protobuf.Writer} [writer] Writer to encode to
         * @returns {$protobuf.Writer} Writer
         */
        Body.encode = function encode(message, writer) {
            if (!writer) writer = $Writer.create()
            if (message.info != null && Object.hasOwn(message, "info"))
                $root.mirabuf.Info.encode(message.info, writer.uint32(/* id 1, wireType 2 =*/ 10).fork()).ldelim()
            if (message.part != null && Object.hasOwn(message, "part"))
                writer.uint32(/* id 2, wireType 2 =*/ 18).string(message.part)
            if (message.triangleMesh != null && Object.hasOwn(message, "triangleMesh"))
                $root.mirabuf.TriangleMesh.encode(
                    message.triangleMesh,
                    writer.uint32(/* id 3, wireType 2 =*/ 26).fork()
                ).ldelim()
            if (message.appearanceOverride != null && Object.hasOwn(message, "appearanceOverride"))
                writer.uint32(/* id 4, wireType 2 =*/ 34).string(message.appearanceOverride)
            return writer
        }

        /**
         * Encodes the specified Body message, length delimited. Does not implicitly {@link mirabuf.Body.verify|verify} messages.
         * @function encodeDelimited
         * @memberof mirabuf.Body
         * @static
         * @param {mirabuf.IBody} message Body message or plain object to encode
         * @param {$protobuf.Writer} [writer] Writer to encode to
         * @returns {$protobuf.Writer} Writer
         */
        Body.encodeDelimited = function encodeDelimited(message, writer) {
            return this.encode(message, writer).ldelim()
        }

        /**
         * Decodes a Body message from the specified reader or buffer.
         * @function decode
         * @memberof mirabuf.Body
         * @static
         * @param {$protobuf.Reader|Uint8Array} reader Reader or buffer to decode from
         * @param {number} [length] Message length if known beforehand
         * @returns {mirabuf.Body} Body
         * @throws {Error} If the payload is not a reader or valid buffer
         * @throws {$protobuf.util.ProtocolError} If required fields are missing
         */
        Body.decode = function decode(reader, length) {
            if (!(reader instanceof $Reader)) reader = $Reader.create(reader)
            let end = length === undefined ? reader.len : reader.pos + length,
                message = new $root.mirabuf.Body()
            while (reader.pos < end) {
                let tag = reader.uint32()
                switch (tag >>> 3) {
                    case 1: {
                        message.info = $root.mirabuf.Info.decode(reader, reader.uint32())
                        break
                    }
                    case 2: {
                        message.part = reader.string()
                        break
                    }
                    case 3: {
                        message.triangleMesh = $root.mirabuf.TriangleMesh.decode(reader, reader.uint32())
                        break
                    }
                    case 4: {
                        message.appearanceOverride = reader.string()
                        break
                    }
                    default:
                        reader.skipType(tag & 7)
                        break
                }
            }
            return message
        }

        /**
         * Decodes a Body message from the specified reader or buffer, length delimited.
         * @function decodeDelimited
         * @memberof mirabuf.Body
         * @static
         * @param {$protobuf.Reader|Uint8Array} reader Reader or buffer to decode from
         * @returns {mirabuf.Body} Body
         * @throws {Error} If the payload is not a reader or valid buffer
         * @throws {$protobuf.util.ProtocolError} If required fields are missing
         */
        Body.decodeDelimited = function decodeDelimited(reader) {
            if (!(reader instanceof $Reader)) reader = new $Reader(reader)
            return this.decode(reader, reader.uint32())
        }

        /**
         * Verifies a Body message.
         * @function verify
         * @memberof mirabuf.Body
         * @static
         * @param {Object.<string,*>} message Plain object to verify
         * @returns {string|null} `null` if valid, otherwise the reason why it is not
         */
        Body.verify = function verify(message) {
            if (typeof message !== "object" || message === null) return "object expected"
            if (message.info != null && Object.hasOwn(message, "info")) {
                let error = $root.mirabuf.Info.verify(message.info)
                if (error) return "info." + error
            }
            if (message.part != null && Object.hasOwn(message, "part"))
                if (!$util.isString(message.part)) return "part: string expected"
            if (message.triangleMesh != null && Object.hasOwn(message, "triangleMesh")) {
                let error = $root.mirabuf.TriangleMesh.verify(message.triangleMesh)
                if (error) return "triangleMesh." + error
            }
            if (message.appearanceOverride != null && Object.hasOwn(message, "appearanceOverride"))
                if (!$util.isString(message.appearanceOverride)) return "appearanceOverride: string expected"
            return null
        }

        /**
         * Creates a Body message from a plain object. Also converts values to their respective internal types.
         * @function fromObject
         * @memberof mirabuf.Body
         * @static
         * @param {Object.<string,*>} object Plain object
         * @returns {mirabuf.Body} Body
         */
        Body.fromObject = function fromObject(object) {
            if (object instanceof $root.mirabuf.Body) return object
            let message = new $root.mirabuf.Body()
            if (object.info != null) {
                if (typeof object.info !== "object") throw TypeError(".mirabuf.Body.info: object expected")
                message.info = $root.mirabuf.Info.fromObject(object.info)
            }
            if (object.part != null) message.part = String(object.part)
            if (object.triangleMesh != null) {
                if (typeof object.triangleMesh !== "object")
                    throw TypeError(".mirabuf.Body.triangleMesh: object expected")
                message.triangleMesh = $root.mirabuf.TriangleMesh.fromObject(object.triangleMesh)
            }
            if (object.appearanceOverride != null) message.appearanceOverride = String(object.appearanceOverride)
            return message
        }

        /**
         * Creates a plain object from a Body message. Also converts values to other types if specified.
         * @function toObject
         * @memberof mirabuf.Body
         * @static
         * @param {mirabuf.Body} message Body
         * @param {$protobuf.IConversionOptions} [options] Conversion options
         * @returns {Object.<string,*>} Plain object
         */
        Body.toObject = function toObject(message, options) {
            if (!options) options = {}
            let object = {}
            if (options.defaults) {
                object.info = null
                object.part = ""
                object.triangleMesh = null
                object.appearanceOverride = ""
            }
            if (message.info != null && Object.hasOwn(message, "info"))
                object.info = $root.mirabuf.Info.toObject(message.info, options)
            if (message.part != null && Object.hasOwn(message, "part")) object.part = message.part
            if (message.triangleMesh != null && Object.hasOwn(message, "triangleMesh"))
                object.triangleMesh = $root.mirabuf.TriangleMesh.toObject(message.triangleMesh, options)
            if (message.appearanceOverride != null && Object.hasOwn(message, "appearanceOverride"))
                object.appearanceOverride = message.appearanceOverride
            return object
        }

        /**
         * Converts this Body to JSON.
         * @function toJSON
         * @memberof mirabuf.Body
         * @instance
         * @returns {Object.<string,*>} JSON object
         */
        Body.prototype.toJSON = function toJSON() {
            return this.constructor.toObject(this, $protobuf.util.toJSONOptions)
        }

        /**
         * Gets the default type url for Body
         * @function getTypeUrl
         * @memberof mirabuf.Body
         * @static
         * @param {string} [typeUrlPrefix] your custom typeUrlPrefix(default "type.googleapis.com")
         * @returns {string} The default type url
         */
        Body.getTypeUrl = function getTypeUrl(typeUrlPrefix) {
            if (typeUrlPrefix === undefined) {
                typeUrlPrefix = "type.googleapis.com"
            }
            return typeUrlPrefix + "/mirabuf.Body"
        }

        return Body
    })()

    mirabuf.TriangleMesh = (function () {
        /**
         * Properties of a TriangleMesh.
         * @memberof mirabuf
         * @interface ITriangleMesh
         * @property {mirabuf.IInfo|null} [info] TriangleMesh info
         * @property {boolean|null} [hasVolume] Is this object a Plane ? (Does it have volume)
         * @property {string|null} [materialReference] Rendered Appearance properties referenced from Assembly Data
         * @property {mirabuf.IMesh|null} [mesh] Stored as true types, inidicies, verts, uv
         * @property {mirabuf.IBinaryMesh|null} [bmesh] Stored as binary data in bytes
         */

        /**
         * Constructs a new TriangleMesh.
         * @memberof mirabuf
         * @classdesc Traingle Mesh for Storing Display Mesh data
         * @implements ITriangleMesh
         * @constructor
         * @param {mirabuf.ITriangleMesh=} [properties] Properties to set
         */
        function TriangleMesh(properties) {
            if (properties)
                for (let keys = Object.keys(properties), i = 0; i < keys.length; ++i)
                    if (properties[keys[i]] != null) this[keys[i]] = properties[keys[i]]
        }

        /**
         * TriangleMesh info.
         * @member {mirabuf.IInfo|null|undefined} info
         * @memberof mirabuf.TriangleMesh
         * @instance
         */
        TriangleMesh.prototype.info = null

        /**
         * Is this object a Plane ? (Does it have volume)
         * @member {boolean} hasVolume
         * @memberof mirabuf.TriangleMesh
         * @instance
         */
        TriangleMesh.prototype.hasVolume = false

        /**
         * Rendered Appearance properties referenced from Assembly Data
         * @member {string} materialReference
         * @memberof mirabuf.TriangleMesh
         * @instance
         */
        TriangleMesh.prototype.materialReference = ""

        /**
         * Stored as true types, inidicies, verts, uv
         * @member {mirabuf.IMesh|null|undefined} mesh
         * @memberof mirabuf.TriangleMesh
         * @instance
         */
        TriangleMesh.prototype.mesh = null

        /**
         * Stored as binary data in bytes
         * @member {mirabuf.IBinaryMesh|null|undefined} bmesh
         * @memberof mirabuf.TriangleMesh
         * @instance
         */
        TriangleMesh.prototype.bmesh = null

        // OneOf field names bound to virtual getters and setters
        let $oneOfFields

        /**
         * What kind of Mesh Data exists in this Triangle Mesh
         * @member {"mesh"|"bmesh"|undefined} meshType
         * @memberof mirabuf.TriangleMesh
         * @instance
         */
        Object.defineProperty(TriangleMesh.prototype, "meshType", {
            get: $util.oneOfGetter(($oneOfFields = ["mesh", "bmesh"])),
            set: $util.oneOfSetter($oneOfFields),
        })

        /**
         * Creates a new TriangleMesh instance using the specified properties.
         * @function create
         * @memberof mirabuf.TriangleMesh
         * @static
         * @param {mirabuf.ITriangleMesh=} [properties] Properties to set
         * @returns {mirabuf.TriangleMesh} TriangleMesh instance
         */
        TriangleMesh.create = function create(properties) {
            return new TriangleMesh(properties)
        }

        /**
         * Encodes the specified TriangleMesh message. Does not implicitly {@link mirabuf.TriangleMesh.verify|verify} messages.
         * @function encode
         * @memberof mirabuf.TriangleMesh
         * @static
         * @param {mirabuf.ITriangleMesh} message TriangleMesh message or plain object to encode
         * @param {$protobuf.Writer} [writer] Writer to encode to
         * @returns {$protobuf.Writer} Writer
         */
        TriangleMesh.encode = function encode(message, writer) {
            if (!writer) writer = $Writer.create()
            if (message.info != null && Object.hasOwn(message, "info"))
                $root.mirabuf.Info.encode(message.info, writer.uint32(/* id 1, wireType 2 =*/ 10).fork()).ldelim()
            if (message.hasVolume != null && Object.hasOwn(message, "hasVolume"))
                writer.uint32(/* id 2, wireType 0 =*/ 16).bool(message.hasVolume)
            if (message.materialReference != null && Object.hasOwn(message, "materialReference"))
                writer.uint32(/* id 3, wireType 2 =*/ 26).string(message.materialReference)
            if (message.mesh != null && Object.hasOwn(message, "mesh"))
                $root.mirabuf.Mesh.encode(message.mesh, writer.uint32(/* id 4, wireType 2 =*/ 34).fork()).ldelim()
            if (message.bmesh != null && Object.hasOwn(message, "bmesh"))
                $root.mirabuf.BinaryMesh.encode(
                    message.bmesh,
                    writer.uint32(/* id 5, wireType 2 =*/ 42).fork()
                ).ldelim()
            return writer
        }

        /**
         * Encodes the specified TriangleMesh message, length delimited. Does not implicitly {@link mirabuf.TriangleMesh.verify|verify} messages.
         * @function encodeDelimited
         * @memberof mirabuf.TriangleMesh
         * @static
         * @param {mirabuf.ITriangleMesh} message TriangleMesh message or plain object to encode
         * @param {$protobuf.Writer} [writer] Writer to encode to
         * @returns {$protobuf.Writer} Writer
         */
        TriangleMesh.encodeDelimited = function encodeDelimited(message, writer) {
            return this.encode(message, writer).ldelim()
        }

        /**
         * Decodes a TriangleMesh message from the specified reader or buffer.
         * @function decode
         * @memberof mirabuf.TriangleMesh
         * @static
         * @param {$protobuf.Reader|Uint8Array} reader Reader or buffer to decode from
         * @param {number} [length] Message length if known beforehand
         * @returns {mirabuf.TriangleMesh} TriangleMesh
         * @throws {Error} If the payload is not a reader or valid buffer
         * @throws {$protobuf.util.ProtocolError} If required fields are missing
         */
        TriangleMesh.decode = function decode(reader, length) {
            if (!(reader instanceof $Reader)) reader = $Reader.create(reader)
            let end = length === undefined ? reader.len : reader.pos + length,
                message = new $root.mirabuf.TriangleMesh()
            while (reader.pos < end) {
                let tag = reader.uint32()
                switch (tag >>> 3) {
                    case 1: {
                        message.info = $root.mirabuf.Info.decode(reader, reader.uint32())
                        break
                    }
                    case 2: {
                        message.hasVolume = reader.bool()
                        break
                    }
                    case 3: {
                        message.materialReference = reader.string()
                        break
                    }
                    case 4: {
                        message.mesh = $root.mirabuf.Mesh.decode(reader, reader.uint32())
                        break
                    }
                    case 5: {
                        message.bmesh = $root.mirabuf.BinaryMesh.decode(reader, reader.uint32())
                        break
                    }
                    default:
                        reader.skipType(tag & 7)
                        break
                }
            }
            return message
        }

        /**
         * Decodes a TriangleMesh message from the specified reader or buffer, length delimited.
         * @function decodeDelimited
         * @memberof mirabuf.TriangleMesh
         * @static
         * @param {$protobuf.Reader|Uint8Array} reader Reader or buffer to decode from
         * @returns {mirabuf.TriangleMesh} TriangleMesh
         * @throws {Error} If the payload is not a reader or valid buffer
         * @throws {$protobuf.util.ProtocolError} If required fields are missing
         */
        TriangleMesh.decodeDelimited = function decodeDelimited(reader) {
            if (!(reader instanceof $Reader)) reader = new $Reader(reader)
            return this.decode(reader, reader.uint32())
        }

        /**
         * Verifies a TriangleMesh message.
         * @function verify
         * @memberof mirabuf.TriangleMesh
         * @static
         * @param {Object.<string,*>} message Plain object to verify
         * @returns {string|null} `null` if valid, otherwise the reason why it is not
         */
        TriangleMesh.verify = function verify(message) {
            if (typeof message !== "object" || message === null) return "object expected"
            let properties = {}
            if (message.info != null && Object.hasOwn(message, "info")) {
                let error = $root.mirabuf.Info.verify(message.info)
                if (error) return "info." + error
            }
            if (message.hasVolume != null && Object.hasOwn(message, "hasVolume"))
                if (typeof message.hasVolume !== "boolean") return "hasVolume: boolean expected"
            if (message.materialReference != null && Object.hasOwn(message, "materialReference"))
                if (!$util.isString(message.materialReference)) return "materialReference: string expected"
            if (message.mesh != null && Object.hasOwn(message, "mesh")) {
                properties.meshType = 1
                {
                    let error = $root.mirabuf.Mesh.verify(message.mesh)
                    if (error) return "mesh." + error
                }
            }
            if (message.bmesh != null && Object.hasOwn(message, "bmesh")) {
                if (properties.meshType === 1) return "meshType: multiple values"
                properties.meshType = 1
                {
                    let error = $root.mirabuf.BinaryMesh.verify(message.bmesh)
                    if (error) return "bmesh." + error
                }
            }
            return null
        }

        /**
         * Creates a TriangleMesh message from a plain object. Also converts values to their respective internal types.
         * @function fromObject
         * @memberof mirabuf.TriangleMesh
         * @static
         * @param {Object.<string,*>} object Plain object
         * @returns {mirabuf.TriangleMesh} TriangleMesh
         */
        TriangleMesh.fromObject = function fromObject(object) {
            if (object instanceof $root.mirabuf.TriangleMesh) return object
            let message = new $root.mirabuf.TriangleMesh()
            if (object.info != null) {
                if (typeof object.info !== "object") throw TypeError(".mirabuf.TriangleMesh.info: object expected")
                message.info = $root.mirabuf.Info.fromObject(object.info)
            }
            if (object.hasVolume != null) message.hasVolume = Boolean(object.hasVolume)
            if (object.materialReference != null) message.materialReference = String(object.materialReference)
            if (object.mesh != null) {
                if (typeof object.mesh !== "object") throw TypeError(".mirabuf.TriangleMesh.mesh: object expected")
                message.mesh = $root.mirabuf.Mesh.fromObject(object.mesh)
            }
            if (object.bmesh != null) {
                if (typeof object.bmesh !== "object") throw TypeError(".mirabuf.TriangleMesh.bmesh: object expected")
                message.bmesh = $root.mirabuf.BinaryMesh.fromObject(object.bmesh)
            }
            return message
        }

        /**
         * Creates a plain object from a TriangleMesh message. Also converts values to other types if specified.
         * @function toObject
         * @memberof mirabuf.TriangleMesh
         * @static
         * @param {mirabuf.TriangleMesh} message TriangleMesh
         * @param {$protobuf.IConversionOptions} [options] Conversion options
         * @returns {Object.<string,*>} Plain object
         */
        TriangleMesh.toObject = function toObject(message, options) {
            if (!options) options = {}
            let object = {}
            if (options.defaults) {
                object.info = null
                object.hasVolume = false
                object.materialReference = ""
            }
            if (message.info != null && Object.hasOwn(message, "info"))
                object.info = $root.mirabuf.Info.toObject(message.info, options)
            if (message.hasVolume != null && Object.hasOwn(message, "hasVolume")) object.hasVolume = message.hasVolume
            if (message.materialReference != null && Object.hasOwn(message, "materialReference"))
                object.materialReference = message.materialReference
            if (message.mesh != null && Object.hasOwn(message, "mesh")) {
                object.mesh = $root.mirabuf.Mesh.toObject(message.mesh, options)
                if (options.oneofs) object.meshType = "mesh"
            }
            if (message.bmesh != null && Object.hasOwn(message, "bmesh")) {
                object.bmesh = $root.mirabuf.BinaryMesh.toObject(message.bmesh, options)
                if (options.oneofs) object.meshType = "bmesh"
            }
            return object
        }

        /**
         * Converts this TriangleMesh to JSON.
         * @function toJSON
         * @memberof mirabuf.TriangleMesh
         * @instance
         * @returns {Object.<string,*>} JSON object
         */
        TriangleMesh.prototype.toJSON = function toJSON() {
            return this.constructor.toObject(this, $protobuf.util.toJSONOptions)
        }

        /**
         * Gets the default type url for TriangleMesh
         * @function getTypeUrl
         * @memberof mirabuf.TriangleMesh
         * @static
         * @param {string} [typeUrlPrefix] your custom typeUrlPrefix(default "type.googleapis.com")
         * @returns {string} The default type url
         */
        TriangleMesh.getTypeUrl = function getTypeUrl(typeUrlPrefix) {
            if (typeUrlPrefix === undefined) {
                typeUrlPrefix = "type.googleapis.com"
            }
            return typeUrlPrefix + "/mirabuf.TriangleMesh"
        }

        return TriangleMesh
    })()

    mirabuf.Mesh = (function () {
        /**
         * Properties of a Mesh.
         * @memberof mirabuf
         * @interface IMesh
         * @property {Array.<number>|null} [verts] Tri Mesh Verts vec3
         * @property {Array.<number>|null} [normals] Tri Mesh Normals vec3
         * @property {Array.<number>|null} [uv] Tri Mesh uv Mapping vec2
         * @property {Array.<number>|null} [indices] Tri Mesh indicies (Vert Map)
         */

        /**
         * Constructs a new Mesh.
         * @memberof mirabuf
         * @classdesc Mesh Data stored as generic Data Structure
         * @implements IMesh
         * @constructor
         * @param {mirabuf.IMesh=} [properties] Properties to set
         */
        function Mesh(properties) {
            this.verts = []
            this.normals = []
            this.uv = []
            this.indices = []
            if (properties)
                for (let keys = Object.keys(properties), i = 0; i < keys.length; ++i)
                    if (properties[keys[i]] != null) this[keys[i]] = properties[keys[i]]
        }

        /**
         * Tri Mesh Verts vec3
         * @member {Array.<number>} verts
         * @memberof mirabuf.Mesh
         * @instance
         */
        Mesh.prototype.verts = $util.emptyArray

        /**
         * Tri Mesh Normals vec3
         * @member {Array.<number>} normals
         * @memberof mirabuf.Mesh
         * @instance
         */
        Mesh.prototype.normals = $util.emptyArray

        /**
         * Tri Mesh uv Mapping vec2
         * @member {Array.<number>} uv
         * @memberof mirabuf.Mesh
         * @instance
         */
        Mesh.prototype.uv = $util.emptyArray

        /**
         * Tri Mesh indicies (Vert Map)
         * @member {Array.<number>} indices
         * @memberof mirabuf.Mesh
         * @instance
         */
        Mesh.prototype.indices = $util.emptyArray

        /**
         * Creates a new Mesh instance using the specified properties.
         * @function create
         * @memberof mirabuf.Mesh
         * @static
         * @param {mirabuf.IMesh=} [properties] Properties to set
         * @returns {mirabuf.Mesh} Mesh instance
         */
        Mesh.create = function create(properties) {
            return new Mesh(properties)
        }

        /**
         * Encodes the specified Mesh message. Does not implicitly {@link mirabuf.Mesh.verify|verify} messages.
         * @function encode
         * @memberof mirabuf.Mesh
         * @static
         * @param {mirabuf.IMesh} message Mesh message or plain object to encode
         * @param {$protobuf.Writer} [writer] Writer to encode to
         * @returns {$protobuf.Writer} Writer
         */
        Mesh.encode = function encode(message, writer) {
            if (!writer) writer = $Writer.create()
            if (message.verts != null && message.verts.length) {
                writer.uint32(/* id 1, wireType 2 =*/ 10).fork()
                for (let i = 0; i < message.verts.length; ++i) writer.float(message.verts[i])
                writer.ldelim()
            }
            if (message.normals != null && message.normals.length) {
                writer.uint32(/* id 2, wireType 2 =*/ 18).fork()
                for (let i = 0; i < message.normals.length; ++i) writer.float(message.normals[i])
                writer.ldelim()
            }
            if (message.uv != null && message.uv.length) {
                writer.uint32(/* id 3, wireType 2 =*/ 26).fork()
                for (let i = 0; i < message.uv.length; ++i) writer.float(message.uv[i])
                writer.ldelim()
            }
            if (message.indices != null && message.indices.length) {
                writer.uint32(/* id 4, wireType 2 =*/ 34).fork()
                for (let i = 0; i < message.indices.length; ++i) writer.int32(message.indices[i])
                writer.ldelim()
            }
            return writer
        }

        /**
         * Encodes the specified Mesh message, length delimited. Does not implicitly {@link mirabuf.Mesh.verify|verify} messages.
         * @function encodeDelimited
         * @memberof mirabuf.Mesh
         * @static
         * @param {mirabuf.IMesh} message Mesh message or plain object to encode
         * @param {$protobuf.Writer} [writer] Writer to encode to
         * @returns {$protobuf.Writer} Writer
         */
        Mesh.encodeDelimited = function encodeDelimited(message, writer) {
            return this.encode(message, writer).ldelim()
        }

        /**
         * Decodes a Mesh message from the specified reader or buffer.
         * @function decode
         * @memberof mirabuf.Mesh
         * @static
         * @param {$protobuf.Reader|Uint8Array} reader Reader or buffer to decode from
         * @param {number} [length] Message length if known beforehand
         * @returns {mirabuf.Mesh} Mesh
         * @throws {Error} If the payload is not a reader or valid buffer
         * @throws {$protobuf.util.ProtocolError} If required fields are missing
         */
        Mesh.decode = function decode(reader, length) {
            if (!(reader instanceof $Reader)) reader = $Reader.create(reader)
            let end = length === undefined ? reader.len : reader.pos + length,
                message = new $root.mirabuf.Mesh()
            while (reader.pos < end) {
                let tag = reader.uint32()
                switch (tag >>> 3) {
                    case 1: {
                        if (!(message.verts && message.verts.length)) message.verts = []
                        if ((tag & 7) === 2) {
                            let end2 = reader.uint32() + reader.pos
                            while (reader.pos < end2) message.verts.push(reader.float())
                        } else message.verts.push(reader.float())
                        break
                    }
                    case 2: {
                        if (!(message.normals && message.normals.length)) message.normals = []
                        if ((tag & 7) === 2) {
                            let end2 = reader.uint32() + reader.pos
                            while (reader.pos < end2) message.normals.push(reader.float())
                        } else message.normals.push(reader.float())
                        break
                    }
                    case 3: {
                        if (!(message.uv && message.uv.length)) message.uv = []
                        if ((tag & 7) === 2) {
                            let end2 = reader.uint32() + reader.pos
                            while (reader.pos < end2) message.uv.push(reader.float())
                        } else message.uv.push(reader.float())
                        break
                    }
                    case 4: {
                        if (!(message.indices && message.indices.length)) message.indices = []
                        if ((tag & 7) === 2) {
                            let end2 = reader.uint32() + reader.pos
                            while (reader.pos < end2) message.indices.push(reader.int32())
                        } else message.indices.push(reader.int32())
                        break
                    }
                    default:
                        reader.skipType(tag & 7)
                        break
                }
            }
            return message
        }

        /**
         * Decodes a Mesh message from the specified reader or buffer, length delimited.
         * @function decodeDelimited
         * @memberof mirabuf.Mesh
         * @static
         * @param {$protobuf.Reader|Uint8Array} reader Reader or buffer to decode from
         * @returns {mirabuf.Mesh} Mesh
         * @throws {Error} If the payload is not a reader or valid buffer
         * @throws {$protobuf.util.ProtocolError} If required fields are missing
         */
        Mesh.decodeDelimited = function decodeDelimited(reader) {
            if (!(reader instanceof $Reader)) reader = new $Reader(reader)
            return this.decode(reader, reader.uint32())
        }

        /**
         * Verifies a Mesh message.
         * @function verify
         * @memberof mirabuf.Mesh
         * @static
         * @param {Object.<string,*>} message Plain object to verify
         * @returns {string|null} `null` if valid, otherwise the reason why it is not
         */
        Mesh.verify = function verify(message) {
            if (typeof message !== "object" || message === null) return "object expected"
            if (message.verts != null && Object.hasOwn(message, "verts")) {
                if (!Array.isArray(message.verts)) return "verts: array expected"
                for (let i = 0; i < message.verts.length; ++i)
                    if (typeof message.verts[i] !== "number") return "verts: number[] expected"
            }
            if (message.normals != null && Object.hasOwn(message, "normals")) {
                if (!Array.isArray(message.normals)) return "normals: array expected"
                for (let i = 0; i < message.normals.length; ++i)
                    if (typeof message.normals[i] !== "number") return "normals: number[] expected"
            }
            if (message.uv != null && Object.hasOwn(message, "uv")) {
                if (!Array.isArray(message.uv)) return "uv: array expected"
                for (let i = 0; i < message.uv.length; ++i)
                    if (typeof message.uv[i] !== "number") return "uv: number[] expected"
            }
            if (message.indices != null && Object.hasOwn(message, "indices")) {
                if (!Array.isArray(message.indices)) return "indices: array expected"
                for (let i = 0; i < message.indices.length; ++i)
                    if (!$util.isInteger(message.indices[i])) return "indices: integer[] expected"
            }
            return null
        }

        /**
         * Creates a Mesh message from a plain object. Also converts values to their respective internal types.
         * @function fromObject
         * @memberof mirabuf.Mesh
         * @static
         * @param {Object.<string,*>} object Plain object
         * @returns {mirabuf.Mesh} Mesh
         */
        Mesh.fromObject = function fromObject(object) {
            if (object instanceof $root.mirabuf.Mesh) return object
            let message = new $root.mirabuf.Mesh()
            if (object.verts) {
                if (!Array.isArray(object.verts)) throw TypeError(".mirabuf.Mesh.verts: array expected")
                message.verts = []
                for (let i = 0; i < object.verts.length; ++i) message.verts[i] = Number(object.verts[i])
            }
            if (object.normals) {
                if (!Array.isArray(object.normals)) throw TypeError(".mirabuf.Mesh.normals: array expected")
                message.normals = []
                for (let i = 0; i < object.normals.length; ++i) message.normals[i] = Number(object.normals[i])
            }
            if (object.uv) {
                if (!Array.isArray(object.uv)) throw TypeError(".mirabuf.Mesh.uv: array expected")
                message.uv = []
                for (let i = 0; i < object.uv.length; ++i) message.uv[i] = Number(object.uv[i])
            }
            if (object.indices) {
                if (!Array.isArray(object.indices)) throw TypeError(".mirabuf.Mesh.indices: array expected")
                message.indices = []
                for (let i = 0; i < object.indices.length; ++i) message.indices[i] = object.indices[i] | 0
            }
            return message
        }

        /**
         * Creates a plain object from a Mesh message. Also converts values to other types if specified.
         * @function toObject
         * @memberof mirabuf.Mesh
         * @static
         * @param {mirabuf.Mesh} message Mesh
         * @param {$protobuf.IConversionOptions} [options] Conversion options
         * @returns {Object.<string,*>} Plain object
         */
        Mesh.toObject = function toObject(message, options) {
            if (!options) options = {}
            let object = {}
            if (options.arrays || options.defaults) {
                object.verts = []
                object.normals = []
                object.uv = []
                object.indices = []
            }
            if (message.verts && message.verts.length) {
                object.verts = []
                for (let j = 0; j < message.verts.length; ++j)
                    object.verts[j] =
                        options.json && !isFinite(message.verts[j]) ? String(message.verts[j]) : message.verts[j]
            }
            if (message.normals && message.normals.length) {
                object.normals = []
                for (let j = 0; j < message.normals.length; ++j)
                    object.normals[j] =
                        options.json && !isFinite(message.normals[j]) ? String(message.normals[j]) : message.normals[j]
            }
            if (message.uv && message.uv.length) {
                object.uv = []
                for (let j = 0; j < message.uv.length; ++j)
                    object.uv[j] = options.json && !isFinite(message.uv[j]) ? String(message.uv[j]) : message.uv[j]
            }
            if (message.indices && message.indices.length) {
                object.indices = []
                for (let j = 0; j < message.indices.length; ++j) object.indices[j] = message.indices[j]
            }
            return object
        }

        /**
         * Converts this Mesh to JSON.
         * @function toJSON
         * @memberof mirabuf.Mesh
         * @instance
         * @returns {Object.<string,*>} JSON object
         */
        Mesh.prototype.toJSON = function toJSON() {
            return this.constructor.toObject(this, $protobuf.util.toJSONOptions)
        }

        /**
         * Gets the default type url for Mesh
         * @function getTypeUrl
         * @memberof mirabuf.Mesh
         * @static
         * @param {string} [typeUrlPrefix] your custom typeUrlPrefix(default "type.googleapis.com")
         * @returns {string} The default type url
         */
        Mesh.getTypeUrl = function getTypeUrl(typeUrlPrefix) {
            if (typeUrlPrefix === undefined) {
                typeUrlPrefix = "type.googleapis.com"
            }
            return typeUrlPrefix + "/mirabuf.Mesh"
        }

        return Mesh
    })()

    mirabuf.BinaryMesh = (function () {
        /**
         * Properties of a BinaryMesh.
         * @memberof mirabuf
         * @interface IBinaryMesh
         * @property {Uint8Array|null} [data] BEWARE of ENDIANESS
         */

        /**
         * Constructs a new BinaryMesh.
         * @memberof mirabuf
         * @classdesc Mesh used for more effective file transfers
         * @implements IBinaryMesh
         * @constructor
         * @param {mirabuf.IBinaryMesh=} [properties] Properties to set
         */
        function BinaryMesh(properties) {
            if (properties)
                for (let keys = Object.keys(properties), i = 0; i < keys.length; ++i)
                    if (properties[keys[i]] != null) this[keys[i]] = properties[keys[i]]
        }

        /**
         * BEWARE of ENDIANESS
         * @member {Uint8Array} data
         * @memberof mirabuf.BinaryMesh
         * @instance
         */
        BinaryMesh.prototype.data = $util.newBuffer([])

        /**
         * Creates a new BinaryMesh instance using the specified properties.
         * @function create
         * @memberof mirabuf.BinaryMesh
         * @static
         * @param {mirabuf.IBinaryMesh=} [properties] Properties to set
         * @returns {mirabuf.BinaryMesh} BinaryMesh instance
         */
        BinaryMesh.create = function create(properties) {
            return new BinaryMesh(properties)
        }

        /**
         * Encodes the specified BinaryMesh message. Does not implicitly {@link mirabuf.BinaryMesh.verify|verify} messages.
         * @function encode
         * @memberof mirabuf.BinaryMesh
         * @static
         * @param {mirabuf.IBinaryMesh} message BinaryMesh message or plain object to encode
         * @param {$protobuf.Writer} [writer] Writer to encode to
         * @returns {$protobuf.Writer} Writer
         */
        BinaryMesh.encode = function encode(message, writer) {
            if (!writer) writer = $Writer.create()
            if (message.data != null && Object.hasOwn(message, "data"))
                writer.uint32(/* id 1, wireType 2 =*/ 10).bytes(message.data)
            return writer
        }

        /**
         * Encodes the specified BinaryMesh message, length delimited. Does not implicitly {@link mirabuf.BinaryMesh.verify|verify} messages.
         * @function encodeDelimited
         * @memberof mirabuf.BinaryMesh
         * @static
         * @param {mirabuf.IBinaryMesh} message BinaryMesh message or plain object to encode
         * @param {$protobuf.Writer} [writer] Writer to encode to
         * @returns {$protobuf.Writer} Writer
         */
        BinaryMesh.encodeDelimited = function encodeDelimited(message, writer) {
            return this.encode(message, writer).ldelim()
        }

        /**
         * Decodes a BinaryMesh message from the specified reader or buffer.
         * @function decode
         * @memberof mirabuf.BinaryMesh
         * @static
         * @param {$protobuf.Reader|Uint8Array} reader Reader or buffer to decode from
         * @param {number} [length] Message length if known beforehand
         * @returns {mirabuf.BinaryMesh} BinaryMesh
         * @throws {Error} If the payload is not a reader or valid buffer
         * @throws {$protobuf.util.ProtocolError} If required fields are missing
         */
        BinaryMesh.decode = function decode(reader, length) {
            if (!(reader instanceof $Reader)) reader = $Reader.create(reader)
            let end = length === undefined ? reader.len : reader.pos + length,
                message = new $root.mirabuf.BinaryMesh()
            while (reader.pos < end) {
                let tag = reader.uint32()
                switch (tag >>> 3) {
                    case 1: {
                        message.data = reader.bytes()
                        break
                    }
                    default:
                        reader.skipType(tag & 7)
                        break
                }
            }
            return message
        }

        /**
         * Decodes a BinaryMesh message from the specified reader or buffer, length delimited.
         * @function decodeDelimited
         * @memberof mirabuf.BinaryMesh
         * @static
         * @param {$protobuf.Reader|Uint8Array} reader Reader or buffer to decode from
         * @returns {mirabuf.BinaryMesh} BinaryMesh
         * @throws {Error} If the payload is not a reader or valid buffer
         * @throws {$protobuf.util.ProtocolError} If required fields are missing
         */
        BinaryMesh.decodeDelimited = function decodeDelimited(reader) {
            if (!(reader instanceof $Reader)) reader = new $Reader(reader)
            return this.decode(reader, reader.uint32())
        }

        /**
         * Verifies a BinaryMesh message.
         * @function verify
         * @memberof mirabuf.BinaryMesh
         * @static
         * @param {Object.<string,*>} message Plain object to verify
         * @returns {string|null} `null` if valid, otherwise the reason why it is not
         */
        BinaryMesh.verify = function verify(message) {
            if (typeof message !== "object" || message === null) return "object expected"
            if (message.data != null && Object.hasOwn(message, "data"))
                if (!((message.data && typeof message.data.length === "number") || $util.isString(message.data)))
                    return "data: buffer expected"
            return null
        }

        /**
         * Creates a BinaryMesh message from a plain object. Also converts values to their respective internal types.
         * @function fromObject
         * @memberof mirabuf.BinaryMesh
         * @static
         * @param {Object.<string,*>} object Plain object
         * @returns {mirabuf.BinaryMesh} BinaryMesh
         */
        BinaryMesh.fromObject = function fromObject(object) {
            if (object instanceof $root.mirabuf.BinaryMesh) return object
            let message = new $root.mirabuf.BinaryMesh()
            if (object.data != null)
                if (typeof object.data === "string")
                    $util.base64.decode(
                        object.data,
                        (message.data = $util.newBuffer($util.base64.length(object.data))),
                        0
                    )
                else if (object.data.length >= 0) message.data = object.data
            return message
        }

        /**
         * Creates a plain object from a BinaryMesh message. Also converts values to other types if specified.
         * @function toObject
         * @memberof mirabuf.BinaryMesh
         * @static
         * @param {mirabuf.BinaryMesh} message BinaryMesh
         * @param {$protobuf.IConversionOptions} [options] Conversion options
         * @returns {Object.<string,*>} Plain object
         */
        BinaryMesh.toObject = function toObject(message, options) {
            if (!options) options = {}
            let object = {}
            if (options.defaults)
                if (options.bytes === String) object.data = ""
                else {
                    object.data = []
                    if (options.bytes !== Array) object.data = $util.newBuffer(object.data)
                }
            if (message.data != null && Object.hasOwn(message, "data"))
                object.data =
                    options.bytes === String
                        ? $util.base64.encode(message.data, 0, message.data.length)
                        : options.bytes === Array
                          ? Array.prototype.slice.call(message.data)
                          : message.data
            return object
        }

        /**
         * Converts this BinaryMesh to JSON.
         * @function toJSON
         * @memberof mirabuf.BinaryMesh
         * @instance
         * @returns {Object.<string,*>} JSON object
         */
        BinaryMesh.prototype.toJSON = function toJSON() {
            return this.constructor.toObject(this, $protobuf.util.toJSONOptions)
        }

        /**
         * Gets the default type url for BinaryMesh
         * @function getTypeUrl
         * @memberof mirabuf.BinaryMesh
         * @static
         * @param {string} [typeUrlPrefix] your custom typeUrlPrefix(default "type.googleapis.com")
         * @returns {string} The default type url
         */
        BinaryMesh.getTypeUrl = function getTypeUrl(typeUrlPrefix) {
            if (typeUrlPrefix === undefined) {
                typeUrlPrefix = "type.googleapis.com"
            }
            return typeUrlPrefix + "/mirabuf.BinaryMesh"
        }

        return BinaryMesh
    })()

    mirabuf.Node = (function () {
        /**
         * Properties of a Node.
         * @memberof mirabuf
         * @interface INode
         * @property {string|null} [value] the reference ID for whatever kind of graph this is
         * @property {Array.<mirabuf.INode>|null} [children] the children for the given leaf
         * @property {mirabuf.IUserData|null} [userData] other associated data that can be used
         */

        /**
         * Constructs a new Node.
         * @memberof mirabuf
         * @classdesc Represents a Node.
         * @implements INode
         * @constructor
         * @param {mirabuf.INode=} [properties] Properties to set
         */
        function Node(properties) {
            this.children = []
            if (properties)
                for (let keys = Object.keys(properties), i = 0; i < keys.length; ++i)
                    if (properties[keys[i]] != null) this[keys[i]] = properties[keys[i]]
        }

        /**
         * the reference ID for whatever kind of graph this is
         * @member {string} value
         * @memberof mirabuf.Node
         * @instance
         */
        Node.prototype.value = ""

        /**
         * the children for the given leaf
         * @member {Array.<mirabuf.INode>} children
         * @memberof mirabuf.Node
         * @instance
         */
        Node.prototype.children = $util.emptyArray

        /**
         * other associated data that can be used
         * @member {mirabuf.IUserData|null|undefined} userData
         * @memberof mirabuf.Node
         * @instance
         */
        Node.prototype.userData = null

        /**
         * Creates a new Node instance using the specified properties.
         * @function create
         * @memberof mirabuf.Node
         * @static
         * @param {mirabuf.INode=} [properties] Properties to set
         * @returns {mirabuf.Node} Node instance
         */
        Node.create = function create(properties) {
            return new Node(properties)
        }

        /**
         * Encodes the specified Node message. Does not implicitly {@link mirabuf.Node.verify|verify} messages.
         * @function encode
         * @memberof mirabuf.Node
         * @static
         * @param {mirabuf.INode} message Node message or plain object to encode
         * @param {$protobuf.Writer} [writer] Writer to encode to
         * @returns {$protobuf.Writer} Writer
         */
        Node.encode = function encode(message, writer) {
            if (!writer) writer = $Writer.create()
            if (message.value != null && Object.hasOwn(message, "value"))
                writer.uint32(/* id 1, wireType 2 =*/ 10).string(message.value)
            if (message.children != null && message.children.length)
                for (let i = 0; i < message.children.length; ++i)
                    $root.mirabuf.Node.encode(
                        message.children[i],
                        writer.uint32(/* id 2, wireType 2 =*/ 18).fork()
                    ).ldelim()
            if (message.userData != null && Object.hasOwn(message, "userData"))
                $root.mirabuf.UserData.encode(
                    message.userData,
                    writer.uint32(/* id 3, wireType 2 =*/ 26).fork()
                ).ldelim()
            return writer
        }

        /**
         * Encodes the specified Node message, length delimited. Does not implicitly {@link mirabuf.Node.verify|verify} messages.
         * @function encodeDelimited
         * @memberof mirabuf.Node
         * @static
         * @param {mirabuf.INode} message Node message or plain object to encode
         * @param {$protobuf.Writer} [writer] Writer to encode to
         * @returns {$protobuf.Writer} Writer
         */
        Node.encodeDelimited = function encodeDelimited(message, writer) {
            return this.encode(message, writer).ldelim()
        }

        /**
         * Decodes a Node message from the specified reader or buffer.
         * @function decode
         * @memberof mirabuf.Node
         * @static
         * @param {$protobuf.Reader|Uint8Array} reader Reader or buffer to decode from
         * @param {number} [length] Message length if known beforehand
         * @returns {mirabuf.Node} Node
         * @throws {Error} If the payload is not a reader or valid buffer
         * @throws {$protobuf.util.ProtocolError} If required fields are missing
         */
        Node.decode = function decode(reader, length) {
            if (!(reader instanceof $Reader)) reader = $Reader.create(reader)
            let end = length === undefined ? reader.len : reader.pos + length,
                message = new $root.mirabuf.Node()
            while (reader.pos < end) {
                let tag = reader.uint32()
                switch (tag >>> 3) {
                    case 1: {
                        message.value = reader.string()
                        break
                    }
                    case 2: {
                        if (!(message.children && message.children.length)) message.children = []
                        message.children.push($root.mirabuf.Node.decode(reader, reader.uint32()))
                        break
                    }
                    case 3: {
                        message.userData = $root.mirabuf.UserData.decode(reader, reader.uint32())
                        break
                    }
                    default:
                        reader.skipType(tag & 7)
                        break
                }
            }
            return message
        }

        /**
         * Decodes a Node message from the specified reader or buffer, length delimited.
         * @function decodeDelimited
         * @memberof mirabuf.Node
         * @static
         * @param {$protobuf.Reader|Uint8Array} reader Reader or buffer to decode from
         * @returns {mirabuf.Node} Node
         * @throws {Error} If the payload is not a reader or valid buffer
         * @throws {$protobuf.util.ProtocolError} If required fields are missing
         */
        Node.decodeDelimited = function decodeDelimited(reader) {
            if (!(reader instanceof $Reader)) reader = new $Reader(reader)
            return this.decode(reader, reader.uint32())
        }

        /**
         * Verifies a Node message.
         * @function verify
         * @memberof mirabuf.Node
         * @static
         * @param {Object.<string,*>} message Plain object to verify
         * @returns {string|null} `null` if valid, otherwise the reason why it is not
         */
        Node.verify = function verify(message) {
            if (typeof message !== "object" || message === null) return "object expected"
            if (message.value != null && Object.hasOwn(message, "value"))
                if (!$util.isString(message.value)) return "value: string expected"
            if (message.children != null && Object.hasOwn(message, "children")) {
                if (!Array.isArray(message.children)) return "children: array expected"
                for (let i = 0; i < message.children.length; ++i) {
                    let error = $root.mirabuf.Node.verify(message.children[i])
                    if (error) return "children." + error
                }
            }
            if (message.userData != null && Object.hasOwn(message, "userData")) {
                let error = $root.mirabuf.UserData.verify(message.userData)
                if (error) return "userData." + error
            }
            return null
        }

        /**
         * Creates a Node message from a plain object. Also converts values to their respective internal types.
         * @function fromObject
         * @memberof mirabuf.Node
         * @static
         * @param {Object.<string,*>} object Plain object
         * @returns {mirabuf.Node} Node
         */
        Node.fromObject = function fromObject(object) {
            if (object instanceof $root.mirabuf.Node) return object
            let message = new $root.mirabuf.Node()
            if (object.value != null) message.value = String(object.value)
            if (object.children) {
                if (!Array.isArray(object.children)) throw TypeError(".mirabuf.Node.children: array expected")
                message.children = []
                for (let i = 0; i < object.children.length; ++i) {
                    if (typeof object.children[i] !== "object")
                        throw TypeError(".mirabuf.Node.children: object expected")
                    message.children[i] = $root.mirabuf.Node.fromObject(object.children[i])
                }
            }
            if (object.userData != null) {
                if (typeof object.userData !== "object") throw TypeError(".mirabuf.Node.userData: object expected")
                message.userData = $root.mirabuf.UserData.fromObject(object.userData)
            }
            return message
        }

        /**
         * Creates a plain object from a Node message. Also converts values to other types if specified.
         * @function toObject
         * @memberof mirabuf.Node
         * @static
         * @param {mirabuf.Node} message Node
         * @param {$protobuf.IConversionOptions} [options] Conversion options
         * @returns {Object.<string,*>} Plain object
         */
        Node.toObject = function toObject(message, options) {
            if (!options) options = {}
            let object = {}
            if (options.arrays || options.defaults) object.children = []
            if (options.defaults) {
                object.value = ""
                object.userData = null
            }
            if (message.value != null && Object.hasOwn(message, "value")) object.value = message.value
            if (message.children && message.children.length) {
                object.children = []
                for (let j = 0; j < message.children.length; ++j)
                    object.children[j] = $root.mirabuf.Node.toObject(message.children[j], options)
            }
            if (message.userData != null && Object.hasOwn(message, "userData"))
                object.userData = $root.mirabuf.UserData.toObject(message.userData, options)
            return object
        }

        /**
         * Converts this Node to JSON.
         * @function toJSON
         * @memberof mirabuf.Node
         * @instance
         * @returns {Object.<string,*>} JSON object
         */
        Node.prototype.toJSON = function toJSON() {
            return this.constructor.toObject(this, $protobuf.util.toJSONOptions)
        }

        /**
         * Gets the default type url for Node
         * @function getTypeUrl
         * @memberof mirabuf.Node
         * @static
         * @param {string} [typeUrlPrefix] your custom typeUrlPrefix(default "type.googleapis.com")
         * @returns {string} The default type url
         */
        Node.getTypeUrl = function getTypeUrl(typeUrlPrefix) {
            if (typeUrlPrefix === undefined) {
                typeUrlPrefix = "type.googleapis.com"
            }
            return typeUrlPrefix + "/mirabuf.Node"
        }

        return Node
    })()

    mirabuf.GraphContainer = (function () {
        /**
         * Properties of a GraphContainer.
         * @memberof mirabuf
         * @interface IGraphContainer
         * @property {Array.<mirabuf.INode>|null} [nodes] GraphContainer nodes
         */

        /**
         * Constructs a new GraphContainer.
         * @memberof mirabuf
         * @classdesc Represents a GraphContainer.
         * @implements IGraphContainer
         * @constructor
         * @param {mirabuf.IGraphContainer=} [properties] Properties to set
         */
        function GraphContainer(properties) {
            this.nodes = []
            if (properties)
                for (let keys = Object.keys(properties), i = 0; i < keys.length; ++i)
                    if (properties[keys[i]] != null) this[keys[i]] = properties[keys[i]]
        }

        /**
         * GraphContainer nodes.
         * @member {Array.<mirabuf.INode>} nodes
         * @memberof mirabuf.GraphContainer
         * @instance
         */
        GraphContainer.prototype.nodes = $util.emptyArray

        /**
         * Creates a new GraphContainer instance using the specified properties.
         * @function create
         * @memberof mirabuf.GraphContainer
         * @static
         * @param {mirabuf.IGraphContainer=} [properties] Properties to set
         * @returns {mirabuf.GraphContainer} GraphContainer instance
         */
        GraphContainer.create = function create(properties) {
            return new GraphContainer(properties)
        }

        /**
         * Encodes the specified GraphContainer message. Does not implicitly {@link mirabuf.GraphContainer.verify|verify} messages.
         * @function encode
         * @memberof mirabuf.GraphContainer
         * @static
         * @param {mirabuf.IGraphContainer} message GraphContainer message or plain object to encode
         * @param {$protobuf.Writer} [writer] Writer to encode to
         * @returns {$protobuf.Writer} Writer
         */
        GraphContainer.encode = function encode(message, writer) {
            if (!writer) writer = $Writer.create()
            if (message.nodes != null && message.nodes.length)
                for (let i = 0; i < message.nodes.length; ++i)
                    $root.mirabuf.Node.encode(
                        message.nodes[i],
                        writer.uint32(/* id 1, wireType 2 =*/ 10).fork()
                    ).ldelim()
            return writer
        }

        /**
         * Encodes the specified GraphContainer message, length delimited. Does not implicitly {@link mirabuf.GraphContainer.verify|verify} messages.
         * @function encodeDelimited
         * @memberof mirabuf.GraphContainer
         * @static
         * @param {mirabuf.IGraphContainer} message GraphContainer message or plain object to encode
         * @param {$protobuf.Writer} [writer] Writer to encode to
         * @returns {$protobuf.Writer} Writer
         */
        GraphContainer.encodeDelimited = function encodeDelimited(message, writer) {
            return this.encode(message, writer).ldelim()
        }

        /**
         * Decodes a GraphContainer message from the specified reader or buffer.
         * @function decode
         * @memberof mirabuf.GraphContainer
         * @static
         * @param {$protobuf.Reader|Uint8Array} reader Reader or buffer to decode from
         * @param {number} [length] Message length if known beforehand
         * @returns {mirabuf.GraphContainer} GraphContainer
         * @throws {Error} If the payload is not a reader or valid buffer
         * @throws {$protobuf.util.ProtocolError} If required fields are missing
         */
        GraphContainer.decode = function decode(reader, length) {
            if (!(reader instanceof $Reader)) reader = $Reader.create(reader)
            let end = length === undefined ? reader.len : reader.pos + length,
                message = new $root.mirabuf.GraphContainer()
            while (reader.pos < end) {
                let tag = reader.uint32()
                switch (tag >>> 3) {
                    case 1: {
                        if (!(message.nodes && message.nodes.length)) message.nodes = []
                        message.nodes.push($root.mirabuf.Node.decode(reader, reader.uint32()))
                        break
                    }
                    default:
                        reader.skipType(tag & 7)
                        break
                }
            }
            return message
        }

        /**
         * Decodes a GraphContainer message from the specified reader or buffer, length delimited.
         * @function decodeDelimited
         * @memberof mirabuf.GraphContainer
         * @static
         * @param {$protobuf.Reader|Uint8Array} reader Reader or buffer to decode from
         * @returns {mirabuf.GraphContainer} GraphContainer
         * @throws {Error} If the payload is not a reader or valid buffer
         * @throws {$protobuf.util.ProtocolError} If required fields are missing
         */
        GraphContainer.decodeDelimited = function decodeDelimited(reader) {
            if (!(reader instanceof $Reader)) reader = new $Reader(reader)
            return this.decode(reader, reader.uint32())
        }

        /**
         * Verifies a GraphContainer message.
         * @function verify
         * @memberof mirabuf.GraphContainer
         * @static
         * @param {Object.<string,*>} message Plain object to verify
         * @returns {string|null} `null` if valid, otherwise the reason why it is not
         */
        GraphContainer.verify = function verify(message) {
            if (typeof message !== "object" || message === null) return "object expected"
            if (message.nodes != null && Object.hasOwn(message, "nodes")) {
                if (!Array.isArray(message.nodes)) return "nodes: array expected"
                for (let i = 0; i < message.nodes.length; ++i) {
                    let error = $root.mirabuf.Node.verify(message.nodes[i])
                    if (error) return "nodes." + error
                }
            }
            return null
        }

        /**
         * Creates a GraphContainer message from a plain object. Also converts values to their respective internal types.
         * @function fromObject
         * @memberof mirabuf.GraphContainer
         * @static
         * @param {Object.<string,*>} object Plain object
         * @returns {mirabuf.GraphContainer} GraphContainer
         */
        GraphContainer.fromObject = function fromObject(object) {
            if (object instanceof $root.mirabuf.GraphContainer) return object
            let message = new $root.mirabuf.GraphContainer()
            if (object.nodes) {
                if (!Array.isArray(object.nodes)) throw TypeError(".mirabuf.GraphContainer.nodes: array expected")
                message.nodes = []
                for (let i = 0; i < object.nodes.length; ++i) {
                    if (typeof object.nodes[i] !== "object")
                        throw TypeError(".mirabuf.GraphContainer.nodes: object expected")
                    message.nodes[i] = $root.mirabuf.Node.fromObject(object.nodes[i])
                }
            }
            return message
        }

        /**
         * Creates a plain object from a GraphContainer message. Also converts values to other types if specified.
         * @function toObject
         * @memberof mirabuf.GraphContainer
         * @static
         * @param {mirabuf.GraphContainer} message GraphContainer
         * @param {$protobuf.IConversionOptions} [options] Conversion options
         * @returns {Object.<string,*>} Plain object
         */
        GraphContainer.toObject = function toObject(message, options) {
            if (!options) options = {}
            let object = {}
            if (options.arrays || options.defaults) object.nodes = []
            if (message.nodes && message.nodes.length) {
                object.nodes = []
                for (let j = 0; j < message.nodes.length; ++j)
                    object.nodes[j] = $root.mirabuf.Node.toObject(message.nodes[j], options)
            }
            return object
        }

        /**
         * Converts this GraphContainer to JSON.
         * @function toJSON
         * @memberof mirabuf.GraphContainer
         * @instance
         * @returns {Object.<string,*>} JSON object
         */
        GraphContainer.prototype.toJSON = function toJSON() {
            return this.constructor.toObject(this, $protobuf.util.toJSONOptions)
        }

        /**
         * Gets the default type url for GraphContainer
         * @function getTypeUrl
         * @memberof mirabuf.GraphContainer
         * @static
         * @param {string} [typeUrlPrefix] your custom typeUrlPrefix(default "type.googleapis.com")
         * @returns {string} The default type url
         */
        GraphContainer.getTypeUrl = function getTypeUrl(typeUrlPrefix) {
            if (typeUrlPrefix === undefined) {
                typeUrlPrefix = "type.googleapis.com"
            }
            return typeUrlPrefix + "/mirabuf.GraphContainer"
        }

        return GraphContainer
    })()

    mirabuf.UserData = (function () {
        /**
         * Properties of a UserData.
         * @memberof mirabuf
         * @interface IUserData
         * @property {Object.<string,string>|null} [data] e.g. data["wheel"] = "yes"
         */

        /**
         * Constructs a new UserData.
         * @memberof mirabuf
         * @classdesc UserData
         *
         * Arbitrary data to append to a given message in map form
         * @implements IUserData
         * @constructor
         * @param {mirabuf.IUserData=} [properties] Properties to set
         */
        function UserData(properties) {
            this.data = {}
            if (properties)
                for (let keys = Object.keys(properties), i = 0; i < keys.length; ++i)
                    if (properties[keys[i]] != null) this[keys[i]] = properties[keys[i]]
        }

        /**
         * e.g. data["wheel"] = "yes"
         * @member {Object.<string,string>} data
         * @memberof mirabuf.UserData
         * @instance
         */
        UserData.prototype.data = $util.emptyObject

        /**
         * Creates a new UserData instance using the specified properties.
         * @function create
         * @memberof mirabuf.UserData
         * @static
         * @param {mirabuf.IUserData=} [properties] Properties to set
         * @returns {mirabuf.UserData} UserData instance
         */
        UserData.create = function create(properties) {
            return new UserData(properties)
        }

        /**
         * Encodes the specified UserData message. Does not implicitly {@link mirabuf.UserData.verify|verify} messages.
         * @function encode
         * @memberof mirabuf.UserData
         * @static
         * @param {mirabuf.IUserData} message UserData message or plain object to encode
         * @param {$protobuf.Writer} [writer] Writer to encode to
         * @returns {$protobuf.Writer} Writer
         */
        UserData.encode = function encode(message, writer) {
            if (!writer) writer = $Writer.create()
            if (message.data != null && Object.hasOwn(message, "data"))
                for (let keys = Object.keys(message.data), i = 0; i < keys.length; ++i)
                    writer
                        .uint32(/* id 1, wireType 2 =*/ 10)
                        .fork()
                        .uint32(/* id 1, wireType 2 =*/ 10)
                        .string(keys[i])
                        .uint32(/* id 2, wireType 2 =*/ 18)
                        .string(message.data[keys[i]])
                        .ldelim()
            return writer
        }

        /**
         * Encodes the specified UserData message, length delimited. Does not implicitly {@link mirabuf.UserData.verify|verify} messages.
         * @function encodeDelimited
         * @memberof mirabuf.UserData
         * @static
         * @param {mirabuf.IUserData} message UserData message or plain object to encode
         * @param {$protobuf.Writer} [writer] Writer to encode to
         * @returns {$protobuf.Writer} Writer
         */
        UserData.encodeDelimited = function encodeDelimited(message, writer) {
            return this.encode(message, writer).ldelim()
        }

        /**
         * Decodes a UserData message from the specified reader or buffer.
         * @function decode
         * @memberof mirabuf.UserData
         * @static
         * @param {$protobuf.Reader|Uint8Array} reader Reader or buffer to decode from
         * @param {number} [length] Message length if known beforehand
         * @returns {mirabuf.UserData} UserData
         * @throws {Error} If the payload is not a reader or valid buffer
         * @throws {$protobuf.util.ProtocolError} If required fields are missing
         */
        UserData.decode = function decode(reader, length) {
            if (!(reader instanceof $Reader)) reader = $Reader.create(reader)
            let end = length === undefined ? reader.len : reader.pos + length,
                message = new $root.mirabuf.UserData(),
                key,
                value
            while (reader.pos < end) {
                let tag = reader.uint32()
                switch (tag >>> 3) {
                    case 1: {
                        if (message.data === $util.emptyObject) message.data = {}
                        let end2 = reader.uint32() + reader.pos
                        key = ""
                        value = ""
                        while (reader.pos < end2) {
                            let tag2 = reader.uint32()
                            switch (tag2 >>> 3) {
                                case 1:
                                    key = reader.string()
                                    break
                                case 2:
                                    value = reader.string()
                                    break
                                default:
                                    reader.skipType(tag2 & 7)
                                    break
                            }
                        }
                        message.data[key] = value
                        break
                    }
                    default:
                        reader.skipType(tag & 7)
                        break
                }
            }
            return message
        }

        /**
         * Decodes a UserData message from the specified reader or buffer, length delimited.
         * @function decodeDelimited
         * @memberof mirabuf.UserData
         * @static
         * @param {$protobuf.Reader|Uint8Array} reader Reader or buffer to decode from
         * @returns {mirabuf.UserData} UserData
         * @throws {Error} If the payload is not a reader or valid buffer
         * @throws {$protobuf.util.ProtocolError} If required fields are missing
         */
        UserData.decodeDelimited = function decodeDelimited(reader) {
            if (!(reader instanceof $Reader)) reader = new $Reader(reader)
            return this.decode(reader, reader.uint32())
        }

        /**
         * Verifies a UserData message.
         * @function verify
         * @memberof mirabuf.UserData
         * @static
         * @param {Object.<string,*>} message Plain object to verify
         * @returns {string|null} `null` if valid, otherwise the reason why it is not
         */
        UserData.verify = function verify(message) {
            if (typeof message !== "object" || message === null) return "object expected"
            if (message.data != null && Object.hasOwn(message, "data")) {
                if (!$util.isObject(message.data)) return "data: object expected"
                let key = Object.keys(message.data)
                for (let i = 0; i < key.length; ++i)
                    if (!$util.isString(message.data[key[i]])) return "data: string{k:string} expected"
            }
            return null
        }

        /**
         * Creates a UserData message from a plain object. Also converts values to their respective internal types.
         * @function fromObject
         * @memberof mirabuf.UserData
         * @static
         * @param {Object.<string,*>} object Plain object
         * @returns {mirabuf.UserData} UserData
         */
        UserData.fromObject = function fromObject(object) {
            if (object instanceof $root.mirabuf.UserData) return object
            let message = new $root.mirabuf.UserData()
            if (object.data) {
                if (typeof object.data !== "object") throw TypeError(".mirabuf.UserData.data: object expected")
                message.data = {}
                for (let keys = Object.keys(object.data), i = 0; i < keys.length; ++i)
                    message.data[keys[i]] = String(object.data[keys[i]])
            }
            return message
        }

        /**
         * Creates a plain object from a UserData message. Also converts values to other types if specified.
         * @function toObject
         * @memberof mirabuf.UserData
         * @static
         * @param {mirabuf.UserData} message UserData
         * @param {$protobuf.IConversionOptions} [options] Conversion options
         * @returns {Object.<string,*>} Plain object
         */
        UserData.toObject = function toObject(message, options) {
            if (!options) options = {}
            let object = {}
            if (options.objects || options.defaults) object.data = {}
            let keys2
            if (message.data && (keys2 = Object.keys(message.data)).length) {
                object.data = {}
                for (let j = 0; j < keys2.length; ++j) object.data[keys2[j]] = message.data[keys2[j]]
            }
            return object
        }

        /**
         * Converts this UserData to JSON.
         * @function toJSON
         * @memberof mirabuf.UserData
         * @instance
         * @returns {Object.<string,*>} JSON object
         */
        UserData.prototype.toJSON = function toJSON() {
            return this.constructor.toObject(this, $protobuf.util.toJSONOptions)
        }

        /**
         * Gets the default type url for UserData
         * @function getTypeUrl
         * @memberof mirabuf.UserData
         * @static
         * @param {string} [typeUrlPrefix] your custom typeUrlPrefix(default "type.googleapis.com")
         * @returns {string} The default type url
         */
        UserData.getTypeUrl = function getTypeUrl(typeUrlPrefix) {
            if (typeUrlPrefix === undefined) {
                typeUrlPrefix = "type.googleapis.com"
            }
            return typeUrlPrefix + "/mirabuf.UserData"
        }

        return UserData
    })()

    mirabuf.Vector3 = (function () {
        /**
         * Properties of a Vector3.
         * @memberof mirabuf
         * @interface IVector3
         * @property {number|null} [x] Vector3 x
         * @property {number|null} [y] Vector3 y
         * @property {number|null} [z] Vector3 z
         */

        /**
         * Constructs a new Vector3.
         * @memberof mirabuf
         * @classdesc Represents a Vector3.
         * @implements IVector3
         * @constructor
         * @param {mirabuf.IVector3=} [properties] Properties to set
         */
        function Vector3(properties) {
            if (properties)
                for (let keys = Object.keys(properties), i = 0; i < keys.length; ++i)
                    if (properties[keys[i]] != null) this[keys[i]] = properties[keys[i]]
        }

        /**
         * Vector3 x.
         * @member {number} x
         * @memberof mirabuf.Vector3
         * @instance
         */
        Vector3.prototype.x = 0

        /**
         * Vector3 y.
         * @member {number} y
         * @memberof mirabuf.Vector3
         * @instance
         */
        Vector3.prototype.y = 0

        /**
         * Vector3 z.
         * @member {number} z
         * @memberof mirabuf.Vector3
         * @instance
         */
        Vector3.prototype.z = 0

        /**
         * Creates a new Vector3 instance using the specified properties.
         * @function create
         * @memberof mirabuf.Vector3
         * @static
         * @param {mirabuf.IVector3=} [properties] Properties to set
         * @returns {mirabuf.Vector3} Vector3 instance
         */
        Vector3.create = function create(properties) {
            return new Vector3(properties)
        }

        /**
         * Encodes the specified Vector3 message. Does not implicitly {@link mirabuf.Vector3.verify|verify} messages.
         * @function encode
         * @memberof mirabuf.Vector3
         * @static
         * @param {mirabuf.IVector3} message Vector3 message or plain object to encode
         * @param {$protobuf.Writer} [writer] Writer to encode to
         * @returns {$protobuf.Writer} Writer
         */
        Vector3.encode = function encode(message, writer) {
            if (!writer) writer = $Writer.create()
            if (message.x != null && Object.hasOwn(message, "x"))
                writer.uint32(/* id 1, wireType 5 =*/ 13).float(message.x)
            if (message.y != null && Object.hasOwn(message, "y"))
                writer.uint32(/* id 2, wireType 5 =*/ 21).float(message.y)
            if (message.z != null && Object.hasOwn(message, "z"))
                writer.uint32(/* id 3, wireType 5 =*/ 29).float(message.z)
            return writer
        }

        /**
         * Encodes the specified Vector3 message, length delimited. Does not implicitly {@link mirabuf.Vector3.verify|verify} messages.
         * @function encodeDelimited
         * @memberof mirabuf.Vector3
         * @static
         * @param {mirabuf.IVector3} message Vector3 message or plain object to encode
         * @param {$protobuf.Writer} [writer] Writer to encode to
         * @returns {$protobuf.Writer} Writer
         */
        Vector3.encodeDelimited = function encodeDelimited(message, writer) {
            return this.encode(message, writer).ldelim()
        }

        /**
         * Decodes a Vector3 message from the specified reader or buffer.
         * @function decode
         * @memberof mirabuf.Vector3
         * @static
         * @param {$protobuf.Reader|Uint8Array} reader Reader or buffer to decode from
         * @param {number} [length] Message length if known beforehand
         * @returns {mirabuf.Vector3} Vector3
         * @throws {Error} If the payload is not a reader or valid buffer
         * @throws {$protobuf.util.ProtocolError} If required fields are missing
         */
        Vector3.decode = function decode(reader, length) {
            if (!(reader instanceof $Reader)) reader = $Reader.create(reader)
            let end = length === undefined ? reader.len : reader.pos + length,
                message = new $root.mirabuf.Vector3()
            while (reader.pos < end) {
                let tag = reader.uint32()
                switch (tag >>> 3) {
                    case 1: {
                        message.x = reader.float()
                        break
                    }
                    case 2: {
                        message.y = reader.float()
                        break
                    }
                    case 3: {
                        message.z = reader.float()
                        break
                    }
                    default:
                        reader.skipType(tag & 7)
                        break
                }
            }
            return message
        }

        /**
         * Decodes a Vector3 message from the specified reader or buffer, length delimited.
         * @function decodeDelimited
         * @memberof mirabuf.Vector3
         * @static
         * @param {$protobuf.Reader|Uint8Array} reader Reader or buffer to decode from
         * @returns {mirabuf.Vector3} Vector3
         * @throws {Error} If the payload is not a reader or valid buffer
         * @throws {$protobuf.util.ProtocolError} If required fields are missing
         */
        Vector3.decodeDelimited = function decodeDelimited(reader) {
            if (!(reader instanceof $Reader)) reader = new $Reader(reader)
            return this.decode(reader, reader.uint32())
        }

        /**
         * Verifies a Vector3 message.
         * @function verify
         * @memberof mirabuf.Vector3
         * @static
         * @param {Object.<string,*>} message Plain object to verify
         * @returns {string|null} `null` if valid, otherwise the reason why it is not
         */
        Vector3.verify = function verify(message) {
            if (typeof message !== "object" || message === null) return "object expected"
            if (message.x != null && Object.hasOwn(message, "x"))
                if (typeof message.x !== "number") return "x: number expected"
            if (message.y != null && Object.hasOwn(message, "y"))
                if (typeof message.y !== "number") return "y: number expected"
            if (message.z != null && Object.hasOwn(message, "z"))
                if (typeof message.z !== "number") return "z: number expected"
            return null
        }

        /**
         * Creates a Vector3 message from a plain object. Also converts values to their respective internal types.
         * @function fromObject
         * @memberof mirabuf.Vector3
         * @static
         * @param {Object.<string,*>} object Plain object
         * @returns {mirabuf.Vector3} Vector3
         */
        Vector3.fromObject = function fromObject(object) {
            if (object instanceof $root.mirabuf.Vector3) return object
            let message = new $root.mirabuf.Vector3()
            if (object.x != null) message.x = Number(object.x)
            if (object.y != null) message.y = Number(object.y)
            if (object.z != null) message.z = Number(object.z)
            return message
        }

        /**
         * Creates a plain object from a Vector3 message. Also converts values to other types if specified.
         * @function toObject
         * @memberof mirabuf.Vector3
         * @static
         * @param {mirabuf.Vector3} message Vector3
         * @param {$protobuf.IConversionOptions} [options] Conversion options
         * @returns {Object.<string,*>} Plain object
         */
        Vector3.toObject = function toObject(message, options) {
            if (!options) options = {}
            let object = {}
            if (options.defaults) {
                object.x = 0
                object.y = 0
                object.z = 0
            }
            if (message.x != null && Object.hasOwn(message, "x"))
                object.x = options.json && !isFinite(message.x) ? String(message.x) : message.x
            if (message.y != null && Object.hasOwn(message, "y"))
                object.y = options.json && !isFinite(message.y) ? String(message.y) : message.y
            if (message.z != null && Object.hasOwn(message, "z"))
                object.z = options.json && !isFinite(message.z) ? String(message.z) : message.z
            return object
        }

        /**
         * Converts this Vector3 to JSON.
         * @function toJSON
         * @memberof mirabuf.Vector3
         * @instance
         * @returns {Object.<string,*>} JSON object
         */
        Vector3.prototype.toJSON = function toJSON() {
            return this.constructor.toObject(this, $protobuf.util.toJSONOptions)
        }

        /**
         * Gets the default type url for Vector3
         * @function getTypeUrl
         * @memberof mirabuf.Vector3
         * @static
         * @param {string} [typeUrlPrefix] your custom typeUrlPrefix(default "type.googleapis.com")
         * @returns {string} The default type url
         */
        Vector3.getTypeUrl = function getTypeUrl(typeUrlPrefix) {
            if (typeUrlPrefix === undefined) {
                typeUrlPrefix = "type.googleapis.com"
            }
            return typeUrlPrefix + "/mirabuf.Vector3"
        }

        return Vector3
    })()

    mirabuf.PhysicalProperties = (function () {
        /**
         * Properties of a PhysicalProperties.
         * @memberof mirabuf
         * @interface IPhysicalProperties
         * @property {number|null} [density] kg per cubic cm kg/(cm^3)
         * @property {number|null} [mass] kg
         * @property {number|null} [volume] cm^3
         * @property {number|null} [area] cm^2
         * @property {mirabuf.IVector3|null} [com] non-negative? Vec3
         */

        /**
         * Constructs a new PhysicalProperties.
         * @memberof mirabuf
         * @classdesc Represents a PhysicalProperties.
         * @implements IPhysicalProperties
         * @constructor
         * @param {mirabuf.IPhysicalProperties=} [properties] Properties to set
         */
        function PhysicalProperties(properties) {
            if (properties)
                for (let keys = Object.keys(properties), i = 0; i < keys.length; ++i)
                    if (properties[keys[i]] != null) this[keys[i]] = properties[keys[i]]
        }

        /**
         * kg per cubic cm kg/(cm^3)
         * @member {number} density
         * @memberof mirabuf.PhysicalProperties
         * @instance
         */
        PhysicalProperties.prototype.density = 0

        /**
         * kg
         * @member {number} mass
         * @memberof mirabuf.PhysicalProperties
         * @instance
         */
        PhysicalProperties.prototype.mass = 0

        /**
         * cm^3
         * @member {number} volume
         * @memberof mirabuf.PhysicalProperties
         * @instance
         */
        PhysicalProperties.prototype.volume = 0

        /**
         * cm^2
         * @member {number} area
         * @memberof mirabuf.PhysicalProperties
         * @instance
         */
        PhysicalProperties.prototype.area = 0

        /**
         * non-negative? Vec3
         * @member {mirabuf.IVector3|null|undefined} com
         * @memberof mirabuf.PhysicalProperties
         * @instance
         */
        PhysicalProperties.prototype.com = null

        /**
         * Creates a new PhysicalProperties instance using the specified properties.
         * @function create
         * @memberof mirabuf.PhysicalProperties
         * @static
         * @param {mirabuf.IPhysicalProperties=} [properties] Properties to set
         * @returns {mirabuf.PhysicalProperties} PhysicalProperties instance
         */
        PhysicalProperties.create = function create(properties) {
            return new PhysicalProperties(properties)
        }

        /**
         * Encodes the specified PhysicalProperties message. Does not implicitly {@link mirabuf.PhysicalProperties.verify|verify} messages.
         * @function encode
         * @memberof mirabuf.PhysicalProperties
         * @static
         * @param {mirabuf.IPhysicalProperties} message PhysicalProperties message or plain object to encode
         * @param {$protobuf.Writer} [writer] Writer to encode to
         * @returns {$protobuf.Writer} Writer
         */
        PhysicalProperties.encode = function encode(message, writer) {
            if (!writer) writer = $Writer.create()
            if (message.density != null && Object.hasOwn(message, "density"))
                writer.uint32(/* id 1, wireType 1 =*/ 9).double(message.density)
            if (message.mass != null && Object.hasOwn(message, "mass"))
                writer.uint32(/* id 2, wireType 1 =*/ 17).double(message.mass)
            if (message.volume != null && Object.hasOwn(message, "volume"))
                writer.uint32(/* id 3, wireType 1 =*/ 25).double(message.volume)
            if (message.area != null && Object.hasOwn(message, "area"))
                writer.uint32(/* id 4, wireType 1 =*/ 33).double(message.area)
            if (message.com != null && Object.hasOwn(message, "com"))
                $root.mirabuf.Vector3.encode(message.com, writer.uint32(/* id 5, wireType 2 =*/ 42).fork()).ldelim()
            return writer
        }

        /**
         * Encodes the specified PhysicalProperties message, length delimited. Does not implicitly {@link mirabuf.PhysicalProperties.verify|verify} messages.
         * @function encodeDelimited
         * @memberof mirabuf.PhysicalProperties
         * @static
         * @param {mirabuf.IPhysicalProperties} message PhysicalProperties message or plain object to encode
         * @param {$protobuf.Writer} [writer] Writer to encode to
         * @returns {$protobuf.Writer} Writer
         */
        PhysicalProperties.encodeDelimited = function encodeDelimited(message, writer) {
            return this.encode(message, writer).ldelim()
        }

        /**
         * Decodes a PhysicalProperties message from the specified reader or buffer.
         * @function decode
         * @memberof mirabuf.PhysicalProperties
         * @static
         * @param {$protobuf.Reader|Uint8Array} reader Reader or buffer to decode from
         * @param {number} [length] Message length if known beforehand
         * @returns {mirabuf.PhysicalProperties} PhysicalProperties
         * @throws {Error} If the payload is not a reader or valid buffer
         * @throws {$protobuf.util.ProtocolError} If required fields are missing
         */
        PhysicalProperties.decode = function decode(reader, length) {
            if (!(reader instanceof $Reader)) reader = $Reader.create(reader)
            let end = length === undefined ? reader.len : reader.pos + length,
                message = new $root.mirabuf.PhysicalProperties()
            while (reader.pos < end) {
                let tag = reader.uint32()
                switch (tag >>> 3) {
                    case 1: {
                        message.density = reader.double()
                        break
                    }
                    case 2: {
                        message.mass = reader.double()
                        break
                    }
                    case 3: {
                        message.volume = reader.double()
                        break
                    }
                    case 4: {
                        message.area = reader.double()
                        break
                    }
                    case 5: {
                        message.com = $root.mirabuf.Vector3.decode(reader, reader.uint32())
                        break
                    }
                    default:
                        reader.skipType(tag & 7)
                        break
                }
            }
            return message
        }

        /**
         * Decodes a PhysicalProperties message from the specified reader or buffer, length delimited.
         * @function decodeDelimited
         * @memberof mirabuf.PhysicalProperties
         * @static
         * @param {$protobuf.Reader|Uint8Array} reader Reader or buffer to decode from
         * @returns {mirabuf.PhysicalProperties} PhysicalProperties
         * @throws {Error} If the payload is not a reader or valid buffer
         * @throws {$protobuf.util.ProtocolError} If required fields are missing
         */
        PhysicalProperties.decodeDelimited = function decodeDelimited(reader) {
            if (!(reader instanceof $Reader)) reader = new $Reader(reader)
            return this.decode(reader, reader.uint32())
        }

        /**
         * Verifies a PhysicalProperties message.
         * @function verify
         * @memberof mirabuf.PhysicalProperties
         * @static
         * @param {Object.<string,*>} message Plain object to verify
         * @returns {string|null} `null` if valid, otherwise the reason why it is not
         */
        PhysicalProperties.verify = function verify(message) {
            if (typeof message !== "object" || message === null) return "object expected"
            if (message.density != null && Object.hasOwn(message, "density"))
                if (typeof message.density !== "number") return "density: number expected"
            if (message.mass != null && Object.hasOwn(message, "mass"))
                if (typeof message.mass !== "number") return "mass: number expected"
            if (message.volume != null && Object.hasOwn(message, "volume"))
                if (typeof message.volume !== "number") return "volume: number expected"
            if (message.area != null && Object.hasOwn(message, "area"))
                if (typeof message.area !== "number") return "area: number expected"
            if (message.com != null && Object.hasOwn(message, "com")) {
                let error = $root.mirabuf.Vector3.verify(message.com)
                if (error) return "com." + error
            }
            return null
        }

        /**
         * Creates a PhysicalProperties message from a plain object. Also converts values to their respective internal types.
         * @function fromObject
         * @memberof mirabuf.PhysicalProperties
         * @static
         * @param {Object.<string,*>} object Plain object
         * @returns {mirabuf.PhysicalProperties} PhysicalProperties
         */
        PhysicalProperties.fromObject = function fromObject(object) {
            if (object instanceof $root.mirabuf.PhysicalProperties) return object
            let message = new $root.mirabuf.PhysicalProperties()
            if (object.density != null) message.density = Number(object.density)
            if (object.mass != null) message.mass = Number(object.mass)
            if (object.volume != null) message.volume = Number(object.volume)
            if (object.area != null) message.area = Number(object.area)
            if (object.com != null) {
                if (typeof object.com !== "object") throw TypeError(".mirabuf.PhysicalProperties.com: object expected")
                message.com = $root.mirabuf.Vector3.fromObject(object.com)
            }
            return message
        }

        /**
         * Creates a plain object from a PhysicalProperties message. Also converts values to other types if specified.
         * @function toObject
         * @memberof mirabuf.PhysicalProperties
         * @static
         * @param {mirabuf.PhysicalProperties} message PhysicalProperties
         * @param {$protobuf.IConversionOptions} [options] Conversion options
         * @returns {Object.<string,*>} Plain object
         */
        PhysicalProperties.toObject = function toObject(message, options) {
            if (!options) options = {}
            let object = {}
            if (options.defaults) {
                object.density = 0
                object.mass = 0
                object.volume = 0
                object.area = 0
                object.com = null
            }
            if (message.density != null && Object.hasOwn(message, "density"))
                object.density = options.json && !isFinite(message.density) ? String(message.density) : message.density
            if (message.mass != null && Object.hasOwn(message, "mass"))
                object.mass = options.json && !isFinite(message.mass) ? String(message.mass) : message.mass
            if (message.volume != null && Object.hasOwn(message, "volume"))
                object.volume = options.json && !isFinite(message.volume) ? String(message.volume) : message.volume
            if (message.area != null && Object.hasOwn(message, "area"))
                object.area = options.json && !isFinite(message.area) ? String(message.area) : message.area
            if (message.com != null && Object.hasOwn(message, "com"))
                object.com = $root.mirabuf.Vector3.toObject(message.com, options)
            return object
        }

        /**
         * Converts this PhysicalProperties to JSON.
         * @function toJSON
         * @memberof mirabuf.PhysicalProperties
         * @instance
         * @returns {Object.<string,*>} JSON object
         */
        PhysicalProperties.prototype.toJSON = function toJSON() {
            return this.constructor.toObject(this, $protobuf.util.toJSONOptions)
        }

        /**
         * Gets the default type url for PhysicalProperties
         * @function getTypeUrl
         * @memberof mirabuf.PhysicalProperties
         * @static
         * @param {string} [typeUrlPrefix] your custom typeUrlPrefix(default "type.googleapis.com")
         * @returns {string} The default type url
         */
        PhysicalProperties.getTypeUrl = function getTypeUrl(typeUrlPrefix) {
            if (typeUrlPrefix === undefined) {
                typeUrlPrefix = "type.googleapis.com"
            }
            return typeUrlPrefix + "/mirabuf.PhysicalProperties"
        }

        return PhysicalProperties
    })()

    mirabuf.Transform = (function () {
        /**
         * Properties of a Transform.
         * @memberof mirabuf
         * @interface ITransform
         * @property {Array.<number>|null} [spatialMatrix] Transform spatialMatrix
         */

        /**
         * Constructs a new Transform.
         * @memberof mirabuf
         * @classdesc Transform
         *
         * Data needed to apply scale, position, and rotational changes
         * @implements ITransform
         * @constructor
         * @param {mirabuf.ITransform=} [properties] Properties to set
         */
        function Transform(properties) {
            this.spatialMatrix = []
            if (properties)
                for (let keys = Object.keys(properties), i = 0; i < keys.length; ++i)
                    if (properties[keys[i]] != null) this[keys[i]] = properties[keys[i]]
        }

        /**
         * Transform spatialMatrix.
         * @member {Array.<number>} spatialMatrix
         * @memberof mirabuf.Transform
         * @instance
         */
        Transform.prototype.spatialMatrix = $util.emptyArray

        /**
         * Creates a new Transform instance using the specified properties.
         * @function create
         * @memberof mirabuf.Transform
         * @static
         * @param {mirabuf.ITransform=} [properties] Properties to set
         * @returns {mirabuf.Transform} Transform instance
         */
        Transform.create = function create(properties) {
            return new Transform(properties)
        }

        /**
         * Encodes the specified Transform message. Does not implicitly {@link mirabuf.Transform.verify|verify} messages.
         * @function encode
         * @memberof mirabuf.Transform
         * @static
         * @param {mirabuf.ITransform} message Transform message or plain object to encode
         * @param {$protobuf.Writer} [writer] Writer to encode to
         * @returns {$protobuf.Writer} Writer
         */
        Transform.encode = function encode(message, writer) {
            if (!writer) writer = $Writer.create()
            if (message.spatialMatrix != null && message.spatialMatrix.length) {
                writer.uint32(/* id 1, wireType 2 =*/ 10).fork()
                for (let i = 0; i < message.spatialMatrix.length; ++i) writer.float(message.spatialMatrix[i])
                writer.ldelim()
            }
            return writer
        }

        /**
         * Encodes the specified Transform message, length delimited. Does not implicitly {@link mirabuf.Transform.verify|verify} messages.
         * @function encodeDelimited
         * @memberof mirabuf.Transform
         * @static
         * @param {mirabuf.ITransform} message Transform message or plain object to encode
         * @param {$protobuf.Writer} [writer] Writer to encode to
         * @returns {$protobuf.Writer} Writer
         */
        Transform.encodeDelimited = function encodeDelimited(message, writer) {
            return this.encode(message, writer).ldelim()
        }

        /**
         * Decodes a Transform message from the specified reader or buffer.
         * @function decode
         * @memberof mirabuf.Transform
         * @static
         * @param {$protobuf.Reader|Uint8Array} reader Reader or buffer to decode from
         * @param {number} [length] Message length if known beforehand
         * @returns {mirabuf.Transform} Transform
         * @throws {Error} If the payload is not a reader or valid buffer
         * @throws {$protobuf.util.ProtocolError} If required fields are missing
         */
        Transform.decode = function decode(reader, length) {
            if (!(reader instanceof $Reader)) reader = $Reader.create(reader)
            let end = length === undefined ? reader.len : reader.pos + length,
                message = new $root.mirabuf.Transform()
            while (reader.pos < end) {
                let tag = reader.uint32()
                switch (tag >>> 3) {
                    case 1: {
                        if (!(message.spatialMatrix && message.spatialMatrix.length)) message.spatialMatrix = []
                        if ((tag & 7) === 2) {
                            let end2 = reader.uint32() + reader.pos
                            while (reader.pos < end2) message.spatialMatrix.push(reader.float())
                        } else message.spatialMatrix.push(reader.float())
                        break
                    }
                    default:
                        reader.skipType(tag & 7)
                        break
                }
            }
            return message
        }

        /**
         * Decodes a Transform message from the specified reader or buffer, length delimited.
         * @function decodeDelimited
         * @memberof mirabuf.Transform
         * @static
         * @param {$protobuf.Reader|Uint8Array} reader Reader or buffer to decode from
         * @returns {mirabuf.Transform} Transform
         * @throws {Error} If the payload is not a reader or valid buffer
         * @throws {$protobuf.util.ProtocolError} If required fields are missing
         */
        Transform.decodeDelimited = function decodeDelimited(reader) {
            if (!(reader instanceof $Reader)) reader = new $Reader(reader)
            return this.decode(reader, reader.uint32())
        }

        /**
         * Verifies a Transform message.
         * @function verify
         * @memberof mirabuf.Transform
         * @static
         * @param {Object.<string,*>} message Plain object to verify
         * @returns {string|null} `null` if valid, otherwise the reason why it is not
         */
        Transform.verify = function verify(message) {
            if (typeof message !== "object" || message === null) return "object expected"
            if (message.spatialMatrix != null && Object.hasOwn(message, "spatialMatrix")) {
                if (!Array.isArray(message.spatialMatrix)) return "spatialMatrix: array expected"
                for (let i = 0; i < message.spatialMatrix.length; ++i)
                    if (typeof message.spatialMatrix[i] !== "number") return "spatialMatrix: number[] expected"
            }
            return null
        }

        /**
         * Creates a Transform message from a plain object. Also converts values to their respective internal types.
         * @function fromObject
         * @memberof mirabuf.Transform
         * @static
         * @param {Object.<string,*>} object Plain object
         * @returns {mirabuf.Transform} Transform
         */
        Transform.fromObject = function fromObject(object) {
            if (object instanceof $root.mirabuf.Transform) return object
            let message = new $root.mirabuf.Transform()
            if (object.spatialMatrix) {
                if (!Array.isArray(object.spatialMatrix))
                    throw TypeError(".mirabuf.Transform.spatialMatrix: array expected")
                message.spatialMatrix = []
                for (let i = 0; i < object.spatialMatrix.length; ++i)
                    message.spatialMatrix[i] = Number(object.spatialMatrix[i])
            }
            return message
        }

        /**
         * Creates a plain object from a Transform message. Also converts values to other types if specified.
         * @function toObject
         * @memberof mirabuf.Transform
         * @static
         * @param {mirabuf.Transform} message Transform
         * @param {$protobuf.IConversionOptions} [options] Conversion options
         * @returns {Object.<string,*>} Plain object
         */
        Transform.toObject = function toObject(message, options) {
            if (!options) options = {}
            let object = {}
            if (options.arrays || options.defaults) object.spatialMatrix = []
            if (message.spatialMatrix && message.spatialMatrix.length) {
                object.spatialMatrix = []
                for (let j = 0; j < message.spatialMatrix.length; ++j)
                    object.spatialMatrix[j] =
                        options.json && !isFinite(message.spatialMatrix[j])
                            ? String(message.spatialMatrix[j])
                            : message.spatialMatrix[j]
            }
            return object
        }

        /**
         * Converts this Transform to JSON.
         * @function toJSON
         * @memberof mirabuf.Transform
         * @instance
         * @returns {Object.<string,*>} JSON object
         */
        Transform.prototype.toJSON = function toJSON() {
            return this.constructor.toObject(this, $protobuf.util.toJSONOptions)
        }

        /**
         * Gets the default type url for Transform
         * @function getTypeUrl
         * @memberof mirabuf.Transform
         * @static
         * @param {string} [typeUrlPrefix] your custom typeUrlPrefix(default "type.googleapis.com")
         * @returns {string} The default type url
         */
        Transform.getTypeUrl = function getTypeUrl(typeUrlPrefix) {
            if (typeUrlPrefix === undefined) {
                typeUrlPrefix = "type.googleapis.com"
            }
            return typeUrlPrefix + "/mirabuf.Transform"
        }

        return Transform
    })()

    mirabuf.Color = (function () {
        /**
         * Properties of a Color.
         * @memberof mirabuf
         * @interface IColor
         * @property {number|null} [R] Color R
         * @property {number|null} [G] Color G
         * @property {number|null} [B] Color B
         * @property {number|null} [A] Color A
         */

        /**
         * Constructs a new Color.
         * @memberof mirabuf
         * @classdesc Represents a Color.
         * @implements IColor
         * @constructor
         * @param {mirabuf.IColor=} [properties] Properties to set
         */
        function Color(properties) {
            if (properties)
                for (let keys = Object.keys(properties), i = 0; i < keys.length; ++i)
                    if (properties[keys[i]] != null) this[keys[i]] = properties[keys[i]]
        }

        /**
         * Color R.
         * @member {number} R
         * @memberof mirabuf.Color
         * @instance
         */
        Color.prototype.R = 0

        /**
         * Color G.
         * @member {number} G
         * @memberof mirabuf.Color
         * @instance
         */
        Color.prototype.G = 0

        /**
         * Color B.
         * @member {number} B
         * @memberof mirabuf.Color
         * @instance
         */
        Color.prototype.B = 0

        /**
         * Color A.
         * @member {number} A
         * @memberof mirabuf.Color
         * @instance
         */
        Color.prototype.A = 0

        /**
         * Creates a new Color instance using the specified properties.
         * @function create
         * @memberof mirabuf.Color
         * @static
         * @param {mirabuf.IColor=} [properties] Properties to set
         * @returns {mirabuf.Color} Color instance
         */
        Color.create = function create(properties) {
            return new Color(properties)
        }

        /**
         * Encodes the specified Color message. Does not implicitly {@link mirabuf.Color.verify|verify} messages.
         * @function encode
         * @memberof mirabuf.Color
         * @static
         * @param {mirabuf.IColor} message Color message or plain object to encode
         * @param {$protobuf.Writer} [writer] Writer to encode to
         * @returns {$protobuf.Writer} Writer
         */
        Color.encode = function encode(message, writer) {
            if (!writer) writer = $Writer.create()
            if (message.R != null && Object.hasOwn(message, "R"))
                writer.uint32(/* id 1, wireType 0 =*/ 8).int32(message.R)
            if (message.G != null && Object.hasOwn(message, "G"))
                writer.uint32(/* id 2, wireType 0 =*/ 16).int32(message.G)
            if (message.B != null && Object.hasOwn(message, "B"))
                writer.uint32(/* id 3, wireType 0 =*/ 24).int32(message.B)
            if (message.A != null && Object.hasOwn(message, "A"))
                writer.uint32(/* id 4, wireType 0 =*/ 32).int32(message.A)
            return writer
        }

        /**
         * Encodes the specified Color message, length delimited. Does not implicitly {@link mirabuf.Color.verify|verify} messages.
         * @function encodeDelimited
         * @memberof mirabuf.Color
         * @static
         * @param {mirabuf.IColor} message Color message or plain object to encode
         * @param {$protobuf.Writer} [writer] Writer to encode to
         * @returns {$protobuf.Writer} Writer
         */
        Color.encodeDelimited = function encodeDelimited(message, writer) {
            return this.encode(message, writer).ldelim()
        }

        /**
         * Decodes a Color message from the specified reader or buffer.
         * @function decode
         * @memberof mirabuf.Color
         * @static
         * @param {$protobuf.Reader|Uint8Array} reader Reader or buffer to decode from
         * @param {number} [length] Message length if known beforehand
         * @returns {mirabuf.Color} Color
         * @throws {Error} If the payload is not a reader or valid buffer
         * @throws {$protobuf.util.ProtocolError} If required fields are missing
         */
        Color.decode = function decode(reader, length) {
            if (!(reader instanceof $Reader)) reader = $Reader.create(reader)
            let end = length === undefined ? reader.len : reader.pos + length,
                message = new $root.mirabuf.Color()
            while (reader.pos < end) {
                let tag = reader.uint32()
                switch (tag >>> 3) {
                    case 1: {
                        message.R = reader.int32()
                        break
                    }
                    case 2: {
                        message.G = reader.int32()
                        break
                    }
                    case 3: {
                        message.B = reader.int32()
                        break
                    }
                    case 4: {
                        message.A = reader.int32()
                        break
                    }
                    default:
                        reader.skipType(tag & 7)
                        break
                }
            }
            return message
        }

        /**
         * Decodes a Color message from the specified reader or buffer, length delimited.
         * @function decodeDelimited
         * @memberof mirabuf.Color
         * @static
         * @param {$protobuf.Reader|Uint8Array} reader Reader or buffer to decode from
         * @returns {mirabuf.Color} Color
         * @throws {Error} If the payload is not a reader or valid buffer
         * @throws {$protobuf.util.ProtocolError} If required fields are missing
         */
        Color.decodeDelimited = function decodeDelimited(reader) {
            if (!(reader instanceof $Reader)) reader = new $Reader(reader)
            return this.decode(reader, reader.uint32())
        }

        /**
         * Verifies a Color message.
         * @function verify
         * @memberof mirabuf.Color
         * @static
         * @param {Object.<string,*>} message Plain object to verify
         * @returns {string|null} `null` if valid, otherwise the reason why it is not
         */
        Color.verify = function verify(message) {
            if (typeof message !== "object" || message === null) return "object expected"
            if (message.R != null && Object.hasOwn(message, "R"))
                if (!$util.isInteger(message.R)) return "R: integer expected"
            if (message.G != null && Object.hasOwn(message, "G"))
                if (!$util.isInteger(message.G)) return "G: integer expected"
            if (message.B != null && Object.hasOwn(message, "B"))
                if (!$util.isInteger(message.B)) return "B: integer expected"
            if (message.A != null && Object.hasOwn(message, "A"))
                if (!$util.isInteger(message.A)) return "A: integer expected"
            return null
        }

        /**
         * Creates a Color message from a plain object. Also converts values to their respective internal types.
         * @function fromObject
         * @memberof mirabuf.Color
         * @static
         * @param {Object.<string,*>} object Plain object
         * @returns {mirabuf.Color} Color
         */
        Color.fromObject = function fromObject(object) {
            if (object instanceof $root.mirabuf.Color) return object
            let message = new $root.mirabuf.Color()
            if (object.R != null) message.R = object.R | 0
            if (object.G != null) message.G = object.G | 0
            if (object.B != null) message.B = object.B | 0
            if (object.A != null) message.A = object.A | 0
            return message
        }

        /**
         * Creates a plain object from a Color message. Also converts values to other types if specified.
         * @function toObject
         * @memberof mirabuf.Color
         * @static
         * @param {mirabuf.Color} message Color
         * @param {$protobuf.IConversionOptions} [options] Conversion options
         * @returns {Object.<string,*>} Plain object
         */
        Color.toObject = function toObject(message, options) {
            if (!options) options = {}
            let object = {}
            if (options.defaults) {
                object.R = 0
                object.G = 0
                object.B = 0
                object.A = 0
            }
            if (message.R != null && Object.hasOwn(message, "R")) object.R = message.R
            if (message.G != null && Object.hasOwn(message, "G")) object.G = message.G
            if (message.B != null && Object.hasOwn(message, "B")) object.B = message.B
            if (message.A != null && Object.hasOwn(message, "A")) object.A = message.A
            return object
        }

        /**
         * Converts this Color to JSON.
         * @function toJSON
         * @memberof mirabuf.Color
         * @instance
         * @returns {Object.<string,*>} JSON object
         */
        Color.prototype.toJSON = function toJSON() {
            return this.constructor.toObject(this, $protobuf.util.toJSONOptions)
        }

        /**
         * Gets the default type url for Color
         * @function getTypeUrl
         * @memberof mirabuf.Color
         * @static
         * @param {string} [typeUrlPrefix] your custom typeUrlPrefix(default "type.googleapis.com")
         * @returns {string} The default type url
         */
        Color.getTypeUrl = function getTypeUrl(typeUrlPrefix) {
            if (typeUrlPrefix === undefined) {
                typeUrlPrefix = "type.googleapis.com"
            }
            return typeUrlPrefix + "/mirabuf.Color"
        }

        return Color
    })()

    /**
     * Axis enum.
     * @name mirabuf.Axis
     * @enum {number}
     * @property {number} X=0 X value
     * @property {number} Y=1 Y value
     * @property {number} Z=2 Z value
     */
    mirabuf.Axis = (function () {
        const valuesById = {},
            values = Object.create(valuesById)
        values[(valuesById[0] = "X")] = 0
        values[(valuesById[1] = "Y")] = 1
        values[(valuesById[2] = "Z")] = 2
        return values
    })()

    mirabuf.Info = (function () {
        /**
         * Properties of an Info.
         * @memberof mirabuf
         * @interface IInfo
         * @property {string|null} [GUID] Info GUID
         * @property {string|null} [name] Info name
         * @property {number|null} [version] Info version
         */

        /**
         * Constructs a new Info.
         * @memberof mirabuf
         * @classdesc Defines basic fields for almost all objects
         * The location where you can access the GUID for a reference
         * @implements IInfo
         * @constructor
         * @param {mirabuf.IInfo=} [properties] Properties to set
         */
        function Info(properties) {
            if (properties)
                for (let keys = Object.keys(properties), i = 0; i < keys.length; ++i)
                    if (properties[keys[i]] != null) this[keys[i]] = properties[keys[i]]
        }

        /**
         * Info GUID.
         * @member {string} GUID
         * @memberof mirabuf.Info
         * @instance
         */
        Info.prototype.GUID = ""

        /**
         * Info name.
         * @member {string} name
         * @memberof mirabuf.Info
         * @instance
         */
        Info.prototype.name = ""

        /**
         * Info version.
         * @member {number} version
         * @memberof mirabuf.Info
         * @instance
         */
        Info.prototype.version = 0

        /**
         * Creates a new Info instance using the specified properties.
         * @function create
         * @memberof mirabuf.Info
         * @static
         * @param {mirabuf.IInfo=} [properties] Properties to set
         * @returns {mirabuf.Info} Info instance
         */
        Info.create = function create(properties) {
            return new Info(properties)
        }

        /**
         * Encodes the specified Info message. Does not implicitly {@link mirabuf.Info.verify|verify} messages.
         * @function encode
         * @memberof mirabuf.Info
         * @static
         * @param {mirabuf.IInfo} message Info message or plain object to encode
         * @param {$protobuf.Writer} [writer] Writer to encode to
         * @returns {$protobuf.Writer} Writer
         */
        Info.encode = function encode(message, writer) {
            if (!writer) writer = $Writer.create()
            if (message.GUID != null && Object.hasOwn(message, "GUID"))
                writer.uint32(/* id 1, wireType 2 =*/ 10).string(message.GUID)
            if (message.name != null && Object.hasOwn(message, "name"))
                writer.uint32(/* id 2, wireType 2 =*/ 18).string(message.name)
            if (message.version != null && Object.hasOwn(message, "version"))
                writer.uint32(/* id 3, wireType 0 =*/ 24).uint32(message.version)
            return writer
        }

        /**
         * Encodes the specified Info message, length delimited. Does not implicitly {@link mirabuf.Info.verify|verify} messages.
         * @function encodeDelimited
         * @memberof mirabuf.Info
         * @static
         * @param {mirabuf.IInfo} message Info message or plain object to encode
         * @param {$protobuf.Writer} [writer] Writer to encode to
         * @returns {$protobuf.Writer} Writer
         */
        Info.encodeDelimited = function encodeDelimited(message, writer) {
            return this.encode(message, writer).ldelim()
        }

        /**
         * Decodes an Info message from the specified reader or buffer.
         * @function decode
         * @memberof mirabuf.Info
         * @static
         * @param {$protobuf.Reader|Uint8Array} reader Reader or buffer to decode from
         * @param {number} [length] Message length if known beforehand
         * @returns {mirabuf.Info} Info
         * @throws {Error} If the payload is not a reader or valid buffer
         * @throws {$protobuf.util.ProtocolError} If required fields are missing
         */
        Info.decode = function decode(reader, length) {
            if (!(reader instanceof $Reader)) reader = $Reader.create(reader)
            let end = length === undefined ? reader.len : reader.pos + length,
                message = new $root.mirabuf.Info()
            while (reader.pos < end) {
                let tag = reader.uint32()
                switch (tag >>> 3) {
                    case 1: {
                        message.GUID = reader.string()
                        break
                    }
                    case 2: {
                        message.name = reader.string()
                        break
                    }
                    case 3: {
                        message.version = reader.uint32()
                        break
                    }
                    default:
                        reader.skipType(tag & 7)
                        break
                }
            }
            return message
        }

        /**
         * Decodes an Info message from the specified reader or buffer, length delimited.
         * @function decodeDelimited
         * @memberof mirabuf.Info
         * @static
         * @param {$protobuf.Reader|Uint8Array} reader Reader or buffer to decode from
         * @returns {mirabuf.Info} Info
         * @throws {Error} If the payload is not a reader or valid buffer
         * @throws {$protobuf.util.ProtocolError} If required fields are missing
         */
        Info.decodeDelimited = function decodeDelimited(reader) {
            if (!(reader instanceof $Reader)) reader = new $Reader(reader)
            return this.decode(reader, reader.uint32())
        }

        /**
         * Verifies an Info message.
         * @function verify
         * @memberof mirabuf.Info
         * @static
         * @param {Object.<string,*>} message Plain object to verify
         * @returns {string|null} `null` if valid, otherwise the reason why it is not
         */
        Info.verify = function verify(message) {
            if (typeof message !== "object" || message === null) return "object expected"
            if (message.GUID != null && Object.hasOwn(message, "GUID"))
                if (!$util.isString(message.GUID)) return "GUID: string expected"
            if (message.name != null && Object.hasOwn(message, "name"))
                if (!$util.isString(message.name)) return "name: string expected"
            if (message.version != null && Object.hasOwn(message, "version"))
                if (!$util.isInteger(message.version)) return "version: integer expected"
            return null
        }

        /**
         * Creates an Info message from a plain object. Also converts values to their respective internal types.
         * @function fromObject
         * @memberof mirabuf.Info
         * @static
         * @param {Object.<string,*>} object Plain object
         * @returns {mirabuf.Info} Info
         */
        Info.fromObject = function fromObject(object) {
            if (object instanceof $root.mirabuf.Info) return object
            let message = new $root.mirabuf.Info()
            if (object.GUID != null) message.GUID = String(object.GUID)
            if (object.name != null) message.name = String(object.name)
            if (object.version != null) message.version = object.version >>> 0
            return message
        }

        /**
         * Creates a plain object from an Info message. Also converts values to other types if specified.
         * @function toObject
         * @memberof mirabuf.Info
         * @static
         * @param {mirabuf.Info} message Info
         * @param {$protobuf.IConversionOptions} [options] Conversion options
         * @returns {Object.<string,*>} Plain object
         */
        Info.toObject = function toObject(message, options) {
            if (!options) options = {}
            let object = {}
            if (options.defaults) {
                object.GUID = ""
                object.name = ""
                object.version = 0
            }
            if (message.GUID != null && Object.hasOwn(message, "GUID")) object.GUID = message.GUID
            if (message.name != null && Object.hasOwn(message, "name")) object.name = message.name
            if (message.version != null && Object.hasOwn(message, "version")) object.version = message.version
            return object
        }

        /**
         * Converts this Info to JSON.
         * @function toJSON
         * @memberof mirabuf.Info
         * @instance
         * @returns {Object.<string,*>} JSON object
         */
        Info.prototype.toJSON = function toJSON() {
            return this.constructor.toObject(this, $protobuf.util.toJSONOptions)
        }

        /**
         * Gets the default type url for Info
         * @function getTypeUrl
         * @memberof mirabuf.Info
         * @static
         * @param {string} [typeUrlPrefix] your custom typeUrlPrefix(default "type.googleapis.com")
         * @returns {string} The default type url
         */
        Info.getTypeUrl = function getTypeUrl(typeUrlPrefix) {
            if (typeUrlPrefix === undefined) {
                typeUrlPrefix = "type.googleapis.com"
            }
            return typeUrlPrefix + "/mirabuf.Info"
        }

        return Info
    })()

    mirabuf.Thumbnail = (function () {
        /**
         * Properties of a Thumbnail.
         * @memberof mirabuf
         * @interface IThumbnail
         * @property {number|null} [width] Image Width
         * @property {number|null} [height] Image Height
         * @property {string|null} [extension] Image Extension - ex. (.png, .bitmap, .jpeg)
         * @property {boolean|null} [transparent] Transparency - true from fusion when correctly configured
         * @property {Uint8Array|null} [data] Data as read from the file in bytes[] form
         */

        /**
         * Constructs a new Thumbnail.
         * @memberof mirabuf
         * @classdesc A basic Thumbnail to be encoded in the file
         * Most of the Time Fusion can encode the file with transparency as PNG not bitmap
         * @implements IThumbnail
         * @constructor
         * @param {mirabuf.IThumbnail=} [properties] Properties to set
         */
        function Thumbnail(properties) {
            if (properties)
                for (let keys = Object.keys(properties), i = 0; i < keys.length; ++i)
                    if (properties[keys[i]] != null) this[keys[i]] = properties[keys[i]]
        }

        /**
         * Image Width
         * @member {number} width
         * @memberof mirabuf.Thumbnail
         * @instance
         */
        Thumbnail.prototype.width = 0

        /**
         * Image Height
         * @member {number} height
         * @memberof mirabuf.Thumbnail
         * @instance
         */
        Thumbnail.prototype.height = 0

        /**
         * Image Extension - ex. (.png, .bitmap, .jpeg)
         * @member {string} extension
         * @memberof mirabuf.Thumbnail
         * @instance
         */
        Thumbnail.prototype.extension = ""

        /**
         * Transparency - true from fusion when correctly configured
         * @member {boolean} transparent
         * @memberof mirabuf.Thumbnail
         * @instance
         */
        Thumbnail.prototype.transparent = false

        /**
         * Data as read from the file in bytes[] form
         * @member {Uint8Array} data
         * @memberof mirabuf.Thumbnail
         * @instance
         */
        Thumbnail.prototype.data = $util.newBuffer([])

        /**
         * Creates a new Thumbnail instance using the specified properties.
         * @function create
         * @memberof mirabuf.Thumbnail
         * @static
         * @param {mirabuf.IThumbnail=} [properties] Properties to set
         * @returns {mirabuf.Thumbnail} Thumbnail instance
         */
        Thumbnail.create = function create(properties) {
            return new Thumbnail(properties)
        }

        /**
         * Encodes the specified Thumbnail message. Does not implicitly {@link mirabuf.Thumbnail.verify|verify} messages.
         * @function encode
         * @memberof mirabuf.Thumbnail
         * @static
         * @param {mirabuf.IThumbnail} message Thumbnail message or plain object to encode
         * @param {$protobuf.Writer} [writer] Writer to encode to
         * @returns {$protobuf.Writer} Writer
         */
        Thumbnail.encode = function encode(message, writer) {
            if (!writer) writer = $Writer.create()
            if (message.width != null && Object.hasOwn(message, "width"))
                writer.uint32(/* id 1, wireType 0 =*/ 8).int32(message.width)
            if (message.height != null && Object.hasOwn(message, "height"))
                writer.uint32(/* id 2, wireType 0 =*/ 16).int32(message.height)
            if (message.extension != null && Object.hasOwn(message, "extension"))
                writer.uint32(/* id 3, wireType 2 =*/ 26).string(message.extension)
            if (message.transparent != null && Object.hasOwn(message, "transparent"))
                writer.uint32(/* id 4, wireType 0 =*/ 32).bool(message.transparent)
            if (message.data != null && Object.hasOwn(message, "data"))
                writer.uint32(/* id 5, wireType 2 =*/ 42).bytes(message.data)
            return writer
        }

        /**
         * Encodes the specified Thumbnail message, length delimited. Does not implicitly {@link mirabuf.Thumbnail.verify|verify} messages.
         * @function encodeDelimited
         * @memberof mirabuf.Thumbnail
         * @static
         * @param {mirabuf.IThumbnail} message Thumbnail message or plain object to encode
         * @param {$protobuf.Writer} [writer] Writer to encode to
         * @returns {$protobuf.Writer} Writer
         */
        Thumbnail.encodeDelimited = function encodeDelimited(message, writer) {
            return this.encode(message, writer).ldelim()
        }

        /**
         * Decodes a Thumbnail message from the specified reader or buffer.
         * @function decode
         * @memberof mirabuf.Thumbnail
         * @static
         * @param {$protobuf.Reader|Uint8Array} reader Reader or buffer to decode from
         * @param {number} [length] Message length if known beforehand
         * @returns {mirabuf.Thumbnail} Thumbnail
         * @throws {Error} If the payload is not a reader or valid buffer
         * @throws {$protobuf.util.ProtocolError} If required fields are missing
         */
        Thumbnail.decode = function decode(reader, length) {
            if (!(reader instanceof $Reader)) reader = $Reader.create(reader)
            let end = length === undefined ? reader.len : reader.pos + length,
                message = new $root.mirabuf.Thumbnail()
            while (reader.pos < end) {
                let tag = reader.uint32()
                switch (tag >>> 3) {
                    case 1: {
                        message.width = reader.int32()
                        break
                    }
                    case 2: {
                        message.height = reader.int32()
                        break
                    }
                    case 3: {
                        message.extension = reader.string()
                        break
                    }
                    case 4: {
                        message.transparent = reader.bool()
                        break
                    }
                    case 5: {
                        message.data = reader.bytes()
                        break
                    }
                    default:
                        reader.skipType(tag & 7)
                        break
                }
            }
            return message
        }

        /**
         * Decodes a Thumbnail message from the specified reader or buffer, length delimited.
         * @function decodeDelimited
         * @memberof mirabuf.Thumbnail
         * @static
         * @param {$protobuf.Reader|Uint8Array} reader Reader or buffer to decode from
         * @returns {mirabuf.Thumbnail} Thumbnail
         * @throws {Error} If the payload is not a reader or valid buffer
         * @throws {$protobuf.util.ProtocolError} If required fields are missing
         */
        Thumbnail.decodeDelimited = function decodeDelimited(reader) {
            if (!(reader instanceof $Reader)) reader = new $Reader(reader)
            return this.decode(reader, reader.uint32())
        }

        /**
         * Verifies a Thumbnail message.
         * @function verify
         * @memberof mirabuf.Thumbnail
         * @static
         * @param {Object.<string,*>} message Plain object to verify
         * @returns {string|null} `null` if valid, otherwise the reason why it is not
         */
        Thumbnail.verify = function verify(message) {
            if (typeof message !== "object" || message === null) return "object expected"
            if (message.width != null && Object.hasOwn(message, "width"))
                if (!$util.isInteger(message.width)) return "width: integer expected"
            if (message.height != null && Object.hasOwn(message, "height"))
                if (!$util.isInteger(message.height)) return "height: integer expected"
            if (message.extension != null && Object.hasOwn(message, "extension"))
                if (!$util.isString(message.extension)) return "extension: string expected"
            if (message.transparent != null && Object.hasOwn(message, "transparent"))
                if (typeof message.transparent !== "boolean") return "transparent: boolean expected"
            if (message.data != null && Object.hasOwn(message, "data"))
                if (!((message.data && typeof message.data.length === "number") || $util.isString(message.data)))
                    return "data: buffer expected"
            return null
        }

        /**
         * Creates a Thumbnail message from a plain object. Also converts values to their respective internal types.
         * @function fromObject
         * @memberof mirabuf.Thumbnail
         * @static
         * @param {Object.<string,*>} object Plain object
         * @returns {mirabuf.Thumbnail} Thumbnail
         */
        Thumbnail.fromObject = function fromObject(object) {
            if (object instanceof $root.mirabuf.Thumbnail) return object
            let message = new $root.mirabuf.Thumbnail()
            if (object.width != null) message.width = object.width | 0
            if (object.height != null) message.height = object.height | 0
            if (object.extension != null) message.extension = String(object.extension)
            if (object.transparent != null) message.transparent = Boolean(object.transparent)
            if (object.data != null)
                if (typeof object.data === "string")
                    $util.base64.decode(
                        object.data,
                        (message.data = $util.newBuffer($util.base64.length(object.data))),
                        0
                    )
                else if (object.data.length >= 0) message.data = object.data
            return message
        }

        /**
         * Creates a plain object from a Thumbnail message. Also converts values to other types if specified.
         * @function toObject
         * @memberof mirabuf.Thumbnail
         * @static
         * @param {mirabuf.Thumbnail} message Thumbnail
         * @param {$protobuf.IConversionOptions} [options] Conversion options
         * @returns {Object.<string,*>} Plain object
         */
        Thumbnail.toObject = function toObject(message, options) {
            if (!options) options = {}
            let object = {}
            if (options.defaults) {
                object.width = 0
                object.height = 0
                object.extension = ""
                object.transparent = false
                if (options.bytes === String) object.data = ""
                else {
                    object.data = []
                    if (options.bytes !== Array) object.data = $util.newBuffer(object.data)
                }
            }
            if (message.width != null && Object.hasOwn(message, "width")) object.width = message.width
            if (message.height != null && Object.hasOwn(message, "height")) object.height = message.height
            if (message.extension != null && Object.hasOwn(message, "extension")) object.extension = message.extension
            if (message.transparent != null && Object.hasOwn(message, "transparent"))
                object.transparent = message.transparent
            if (message.data != null && Object.hasOwn(message, "data"))
                object.data =
                    options.bytes === String
                        ? $util.base64.encode(message.data, 0, message.data.length)
                        : options.bytes === Array
                          ? Array.prototype.slice.call(message.data)
                          : message.data
            return object
        }

        /**
         * Converts this Thumbnail to JSON.
         * @function toJSON
         * @memberof mirabuf.Thumbnail
         * @instance
         * @returns {Object.<string,*>} JSON object
         */
        Thumbnail.prototype.toJSON = function toJSON() {
            return this.constructor.toObject(this, $protobuf.util.toJSONOptions)
        }

        /**
         * Gets the default type url for Thumbnail
         * @function getTypeUrl
         * @memberof mirabuf.Thumbnail
         * @static
         * @param {string} [typeUrlPrefix] your custom typeUrlPrefix(default "type.googleapis.com")
         * @returns {string} The default type url
         */
        Thumbnail.getTypeUrl = function getTypeUrl(typeUrlPrefix) {
            if (typeUrlPrefix === undefined) {
                typeUrlPrefix = "type.googleapis.com"
            }
            return typeUrlPrefix + "/mirabuf.Thumbnail"
        }

        return Thumbnail
    })()

    mirabuf.joint = (function () {
        /**
         * Namespace joint.
         * @memberof mirabuf
         * @namespace
         */
        const joint = {}

        joint.Joints = (function () {
            /**
             * Properties of a Joints.
             * @memberof mirabuf.joint
             * @interface IJoints
             * @property {mirabuf.IInfo|null} [info] name, version, uid
             * @property {Object.<string,mirabuf.joint.IJoint>|null} [jointDefinitions] Unique Joint Implementations
             * @property {Object.<string,mirabuf.joint.IJointInstance>|null} [jointInstances] Instances of the Joint Implementations
             * @property {Array.<mirabuf.joint.IRigidGroup>|null} [rigidGroups] Rigidgroups ?
             * @property {Object.<string,mirabuf.motor.IMotor>|null} [motorDefinitions] Collection of all Motors exported
             */

            /**
             * Constructs a new Joints.
             * @memberof mirabuf.joint
             * @classdesc Joints
             * A way to define the motion between various group connections
             * @implements IJoints
             * @constructor
             * @param {mirabuf.joint.IJoints=} [properties] Properties to set
             */
            function Joints(properties) {
                this.jointDefinitions = {}
                this.jointInstances = {}
                this.rigidGroups = []
                this.motorDefinitions = {}
                if (properties)
                    for (let keys = Object.keys(properties), i = 0; i < keys.length; ++i)
                        if (properties[keys[i]] != null) this[keys[i]] = properties[keys[i]]
            }

            /**
             * name, version, uid
             * @member {mirabuf.IInfo|null|undefined} info
             * @memberof mirabuf.joint.Joints
             * @instance
             */
            Joints.prototype.info = null

            /**
             * Unique Joint Implementations
             * @member {Object.<string,mirabuf.joint.IJoint>} jointDefinitions
             * @memberof mirabuf.joint.Joints
             * @instance
             */
            Joints.prototype.jointDefinitions = $util.emptyObject

            /**
             * Instances of the Joint Implementations
             * @member {Object.<string,mirabuf.joint.IJointInstance>} jointInstances
             * @memberof mirabuf.joint.Joints
             * @instance
             */
            Joints.prototype.jointInstances = $util.emptyObject

            /**
             * Rigidgroups ?
             * @member {Array.<mirabuf.joint.IRigidGroup>} rigidGroups
             * @memberof mirabuf.joint.Joints
             * @instance
             */
            Joints.prototype.rigidGroups = $util.emptyArray

            /**
             * Collection of all Motors exported
             * @member {Object.<string,mirabuf.motor.IMotor>} motorDefinitions
             * @memberof mirabuf.joint.Joints
             * @instance
             */
            Joints.prototype.motorDefinitions = $util.emptyObject

            /**
             * Creates a new Joints instance using the specified properties.
             * @function create
             * @memberof mirabuf.joint.Joints
             * @static
             * @param {mirabuf.joint.IJoints=} [properties] Properties to set
             * @returns {mirabuf.joint.Joints} Joints instance
             */
            Joints.create = function create(properties) {
                return new Joints(properties)
            }

            /**
             * Encodes the specified Joints message. Does not implicitly {@link mirabuf.joint.Joints.verify|verify} messages.
             * @function encode
             * @memberof mirabuf.joint.Joints
             * @static
             * @param {mirabuf.joint.IJoints} message Joints message or plain object to encode
             * @param {$protobuf.Writer} [writer] Writer to encode to
             * @returns {$protobuf.Writer} Writer
             */
            Joints.encode = function encode(message, writer) {
                if (!writer) writer = $Writer.create()
                if (message.info != null && Object.hasOwn(message, "info"))
                    $root.mirabuf.Info.encode(message.info, writer.uint32(/* id 1, wireType 2 =*/ 10).fork()).ldelim()
                if (message.jointDefinitions != null && Object.hasOwn(message, "jointDefinitions"))
                    for (let keys = Object.keys(message.jointDefinitions), i = 0; i < keys.length; ++i) {
                        writer
                            .uint32(/* id 2, wireType 2 =*/ 18)
                            .fork()
                            .uint32(/* id 1, wireType 2 =*/ 10)
                            .string(keys[i])
                        $root.mirabuf.joint.Joint.encode(
                            message.jointDefinitions[keys[i]],
                            writer.uint32(/* id 2, wireType 2 =*/ 18).fork()
                        )
                            .ldelim()
                            .ldelim()
                    }
                if (message.jointInstances != null && Object.hasOwn(message, "jointInstances"))
                    for (let keys = Object.keys(message.jointInstances), i = 0; i < keys.length; ++i) {
                        writer
                            .uint32(/* id 3, wireType 2 =*/ 26)
                            .fork()
                            .uint32(/* id 1, wireType 2 =*/ 10)
                            .string(keys[i])
                        $root.mirabuf.joint.JointInstance.encode(
                            message.jointInstances[keys[i]],
                            writer.uint32(/* id 2, wireType 2 =*/ 18).fork()
                        )
                            .ldelim()
                            .ldelim()
                    }
                if (message.rigidGroups != null && message.rigidGroups.length)
                    for (let i = 0; i < message.rigidGroups.length; ++i)
                        $root.mirabuf.joint.RigidGroup.encode(
                            message.rigidGroups[i],
                            writer.uint32(/* id 4, wireType 2 =*/ 34).fork()
                        ).ldelim()
                if (message.motorDefinitions != null && Object.hasOwn(message, "motorDefinitions"))
                    for (let keys = Object.keys(message.motorDefinitions), i = 0; i < keys.length; ++i) {
                        writer
                            .uint32(/* id 5, wireType 2 =*/ 42)
                            .fork()
                            .uint32(/* id 1, wireType 2 =*/ 10)
                            .string(keys[i])
                        $root.mirabuf.motor.Motor.encode(
                            message.motorDefinitions[keys[i]],
                            writer.uint32(/* id 2, wireType 2 =*/ 18).fork()
                        )
                            .ldelim()
                            .ldelim()
                    }
                return writer
            }

            /**
             * Encodes the specified Joints message, length delimited. Does not implicitly {@link mirabuf.joint.Joints.verify|verify} messages.
             * @function encodeDelimited
             * @memberof mirabuf.joint.Joints
             * @static
             * @param {mirabuf.joint.IJoints} message Joints message or plain object to encode
             * @param {$protobuf.Writer} [writer] Writer to encode to
             * @returns {$protobuf.Writer} Writer
             */
            Joints.encodeDelimited = function encodeDelimited(message, writer) {
                return this.encode(message, writer).ldelim()
            }

            /**
             * Decodes a Joints message from the specified reader or buffer.
             * @function decode
             * @memberof mirabuf.joint.Joints
             * @static
             * @param {$protobuf.Reader|Uint8Array} reader Reader or buffer to decode from
             * @param {number} [length] Message length if known beforehand
             * @returns {mirabuf.joint.Joints} Joints
             * @throws {Error} If the payload is not a reader or valid buffer
             * @throws {$protobuf.util.ProtocolError} If required fields are missing
             */
            Joints.decode = function decode(reader, length) {
                if (!(reader instanceof $Reader)) reader = $Reader.create(reader)
                let end = length === undefined ? reader.len : reader.pos + length,
                    message = new $root.mirabuf.joint.Joints(),
                    key,
                    value
                while (reader.pos < end) {
                    let tag = reader.uint32()
                    switch (tag >>> 3) {
                        case 1: {
                            message.info = $root.mirabuf.Info.decode(reader, reader.uint32())
                            break
                        }
                        case 2: {
                            if (message.jointDefinitions === $util.emptyObject) message.jointDefinitions = {}
                            let end2 = reader.uint32() + reader.pos
                            key = ""
                            value = null
                            while (reader.pos < end2) {
                                let tag2 = reader.uint32()
                                switch (tag2 >>> 3) {
                                    case 1:
                                        key = reader.string()
                                        break
                                    case 2:
                                        value = $root.mirabuf.joint.Joint.decode(reader, reader.uint32())
                                        break
                                    default:
                                        reader.skipType(tag2 & 7)
                                        break
                                }
                            }
                            message.jointDefinitions[key] = value
                            break
                        }
                        case 3: {
                            if (message.jointInstances === $util.emptyObject) message.jointInstances = {}
                            let end2 = reader.uint32() + reader.pos
                            key = ""
                            value = null
                            while (reader.pos < end2) {
                                let tag2 = reader.uint32()
                                switch (tag2 >>> 3) {
                                    case 1:
                                        key = reader.string()
                                        break
                                    case 2:
                                        value = $root.mirabuf.joint.JointInstance.decode(reader, reader.uint32())
                                        break
                                    default:
                                        reader.skipType(tag2 & 7)
                                        break
                                }
                            }
                            message.jointInstances[key] = value
                            break
                        }
                        case 4: {
                            if (!(message.rigidGroups && message.rigidGroups.length)) message.rigidGroups = []
                            message.rigidGroups.push($root.mirabuf.joint.RigidGroup.decode(reader, reader.uint32()))
                            break
                        }
                        case 5: {
                            if (message.motorDefinitions === $util.emptyObject) message.motorDefinitions = {}
                            let end2 = reader.uint32() + reader.pos
                            key = ""
                            value = null
                            while (reader.pos < end2) {
                                let tag2 = reader.uint32()
                                switch (tag2 >>> 3) {
                                    case 1:
                                        key = reader.string()
                                        break
                                    case 2:
                                        value = $root.mirabuf.motor.Motor.decode(reader, reader.uint32())
                                        break
                                    default:
                                        reader.skipType(tag2 & 7)
                                        break
                                }
                            }
                            message.motorDefinitions[key] = value
                            break
                        }
                        default:
                            reader.skipType(tag & 7)
                            break
                    }
                }
                return message
            }

            /**
             * Decodes a Joints message from the specified reader or buffer, length delimited.
             * @function decodeDelimited
             * @memberof mirabuf.joint.Joints
             * @static
             * @param {$protobuf.Reader|Uint8Array} reader Reader or buffer to decode from
             * @returns {mirabuf.joint.Joints} Joints
             * @throws {Error} If the payload is not a reader or valid buffer
             * @throws {$protobuf.util.ProtocolError} If required fields are missing
             */
            Joints.decodeDelimited = function decodeDelimited(reader) {
                if (!(reader instanceof $Reader)) reader = new $Reader(reader)
                return this.decode(reader, reader.uint32())
            }

            /**
             * Verifies a Joints message.
             * @function verify
             * @memberof mirabuf.joint.Joints
             * @static
             * @param {Object.<string,*>} message Plain object to verify
             * @returns {string|null} `null` if valid, otherwise the reason why it is not
             */
            Joints.verify = function verify(message) {
                if (typeof message !== "object" || message === null) return "object expected"
                if (message.info != null && Object.hasOwn(message, "info")) {
                    let error = $root.mirabuf.Info.verify(message.info)
                    if (error) return "info." + error
                }
                if (message.jointDefinitions != null && Object.hasOwn(message, "jointDefinitions")) {
                    if (!$util.isObject(message.jointDefinitions)) return "jointDefinitions: object expected"
                    let key = Object.keys(message.jointDefinitions)
                    for (let i = 0; i < key.length; ++i) {
                        let error = $root.mirabuf.joint.Joint.verify(message.jointDefinitions[key[i]])
                        if (error) return "jointDefinitions." + error
                    }
                }
                if (message.jointInstances != null && Object.hasOwn(message, "jointInstances")) {
                    if (!$util.isObject(message.jointInstances)) return "jointInstances: object expected"
                    let key = Object.keys(message.jointInstances)
                    for (let i = 0; i < key.length; ++i) {
                        let error = $root.mirabuf.joint.JointInstance.verify(message.jointInstances[key[i]])
                        if (error) return "jointInstances." + error
                    }
                }
                if (message.rigidGroups != null && Object.hasOwn(message, "rigidGroups")) {
                    if (!Array.isArray(message.rigidGroups)) return "rigidGroups: array expected"
                    for (let i = 0; i < message.rigidGroups.length; ++i) {
                        let error = $root.mirabuf.joint.RigidGroup.verify(message.rigidGroups[i])
                        if (error) return "rigidGroups." + error
                    }
                }
                if (message.motorDefinitions != null && Object.hasOwn(message, "motorDefinitions")) {
                    if (!$util.isObject(message.motorDefinitions)) return "motorDefinitions: object expected"
                    let key = Object.keys(message.motorDefinitions)
                    for (let i = 0; i < key.length; ++i) {
                        let error = $root.mirabuf.motor.Motor.verify(message.motorDefinitions[key[i]])
                        if (error) return "motorDefinitions." + error
                    }
                }
                return null
            }

            /**
             * Creates a Joints message from a plain object. Also converts values to their respective internal types.
             * @function fromObject
             * @memberof mirabuf.joint.Joints
             * @static
             * @param {Object.<string,*>} object Plain object
             * @returns {mirabuf.joint.Joints} Joints
             */
            Joints.fromObject = function fromObject(object) {
                if (object instanceof $root.mirabuf.joint.Joints) return object
                let message = new $root.mirabuf.joint.Joints()
                if (object.info != null) {
                    if (typeof object.info !== "object") throw TypeError(".mirabuf.joint.Joints.info: object expected")
                    message.info = $root.mirabuf.Info.fromObject(object.info)
                }
                if (object.jointDefinitions) {
                    if (typeof object.jointDefinitions !== "object")
                        throw TypeError(".mirabuf.joint.Joints.jointDefinitions: object expected")
                    message.jointDefinitions = {}
                    for (let keys = Object.keys(object.jointDefinitions), i = 0; i < keys.length; ++i) {
                        if (typeof object.jointDefinitions[keys[i]] !== "object")
                            throw TypeError(".mirabuf.joint.Joints.jointDefinitions: object expected")
                        message.jointDefinitions[keys[i]] = $root.mirabuf.joint.Joint.fromObject(
                            object.jointDefinitions[keys[i]]
                        )
                    }
                }
                if (object.jointInstances) {
                    if (typeof object.jointInstances !== "object")
                        throw TypeError(".mirabuf.joint.Joints.jointInstances: object expected")
                    message.jointInstances = {}
                    for (let keys = Object.keys(object.jointInstances), i = 0; i < keys.length; ++i) {
                        if (typeof object.jointInstances[keys[i]] !== "object")
                            throw TypeError(".mirabuf.joint.Joints.jointInstances: object expected")
                        message.jointInstances[keys[i]] = $root.mirabuf.joint.JointInstance.fromObject(
                            object.jointInstances[keys[i]]
                        )
                    }
                }
                if (object.rigidGroups) {
                    if (!Array.isArray(object.rigidGroups))
                        throw TypeError(".mirabuf.joint.Joints.rigidGroups: array expected")
                    message.rigidGroups = []
                    for (let i = 0; i < object.rigidGroups.length; ++i) {
                        if (typeof object.rigidGroups[i] !== "object")
                            throw TypeError(".mirabuf.joint.Joints.rigidGroups: object expected")
                        message.rigidGroups[i] = $root.mirabuf.joint.RigidGroup.fromObject(object.rigidGroups[i])
                    }
                }
                if (object.motorDefinitions) {
                    if (typeof object.motorDefinitions !== "object")
                        throw TypeError(".mirabuf.joint.Joints.motorDefinitions: object expected")
                    message.motorDefinitions = {}
                    for (let keys = Object.keys(object.motorDefinitions), i = 0; i < keys.length; ++i) {
                        if (typeof object.motorDefinitions[keys[i]] !== "object")
                            throw TypeError(".mirabuf.joint.Joints.motorDefinitions: object expected")
                        message.motorDefinitions[keys[i]] = $root.mirabuf.motor.Motor.fromObject(
                            object.motorDefinitions[keys[i]]
                        )
                    }
                }
                return message
            }

            /**
             * Creates a plain object from a Joints message. Also converts values to other types if specified.
             * @function toObject
             * @memberof mirabuf.joint.Joints
             * @static
             * @param {mirabuf.joint.Joints} message Joints
             * @param {$protobuf.IConversionOptions} [options] Conversion options
             * @returns {Object.<string,*>} Plain object
             */
            Joints.toObject = function toObject(message, options) {
                if (!options) options = {}
                let object = {}
                if (options.arrays || options.defaults) object.rigidGroups = []
                if (options.objects || options.defaults) {
                    object.jointDefinitions = {}
                    object.jointInstances = {}
                    object.motorDefinitions = {}
                }
                if (options.defaults) object.info = null
                if (message.info != null && Object.hasOwn(message, "info"))
                    object.info = $root.mirabuf.Info.toObject(message.info, options)
                let keys2
                if (message.jointDefinitions && (keys2 = Object.keys(message.jointDefinitions)).length) {
                    object.jointDefinitions = {}
                    for (let j = 0; j < keys2.length; ++j)
                        object.jointDefinitions[keys2[j]] = $root.mirabuf.joint.Joint.toObject(
                            message.jointDefinitions[keys2[j]],
                            options
                        )
                }
                if (message.jointInstances && (keys2 = Object.keys(message.jointInstances)).length) {
                    object.jointInstances = {}
                    for (let j = 0; j < keys2.length; ++j)
                        object.jointInstances[keys2[j]] = $root.mirabuf.joint.JointInstance.toObject(
                            message.jointInstances[keys2[j]],
                            options
                        )
                }
                if (message.rigidGroups && message.rigidGroups.length) {
                    object.rigidGroups = []
                    for (let j = 0; j < message.rigidGroups.length; ++j)
                        object.rigidGroups[j] = $root.mirabuf.joint.RigidGroup.toObject(message.rigidGroups[j], options)
                }
                if (message.motorDefinitions && (keys2 = Object.keys(message.motorDefinitions)).length) {
                    object.motorDefinitions = {}
                    for (let j = 0; j < keys2.length; ++j)
                        object.motorDefinitions[keys2[j]] = $root.mirabuf.motor.Motor.toObject(
                            message.motorDefinitions[keys2[j]],
                            options
                        )
                }
                return object
            }

            /**
             * Converts this Joints to JSON.
             * @function toJSON
             * @memberof mirabuf.joint.Joints
             * @instance
             * @returns {Object.<string,*>} JSON object
             */
            Joints.prototype.toJSON = function toJSON() {
                return this.constructor.toObject(this, $protobuf.util.toJSONOptions)
            }

            /**
             * Gets the default type url for Joints
             * @function getTypeUrl
             * @memberof mirabuf.joint.Joints
             * @static
             * @param {string} [typeUrlPrefix] your custom typeUrlPrefix(default "type.googleapis.com")
             * @returns {string} The default type url
             */
            Joints.getTypeUrl = function getTypeUrl(typeUrlPrefix) {
                if (typeUrlPrefix === undefined) {
                    typeUrlPrefix = "type.googleapis.com"
                }
                return typeUrlPrefix + "/mirabuf.joint.Joints"
            }

            return Joints
        })()

        /**
         * JointMotion enum.
         * @name mirabuf.joint.JointMotion
         * @enum {number}
         * @property {number} RIGID=0 RIGID value
         * @property {number} REVOLUTE=1 REVOLUTE value
         * @property {number} SLIDER=2 SLIDER value
         * @property {number} CYLINDRICAL=3 CYLINDRICAL value
         * @property {number} PINSLOT=4 PINSLOT value
         * @property {number} PLANAR=5 PLANAR value
         * @property {number} BALL=6 BALL value
         * @property {number} CUSTOM=7 CUSTOM value
         */
        joint.JointMotion = (function () {
            const valuesById = {},
                values = Object.create(valuesById)
            values[(valuesById[0] = "RIGID")] = 0
            values[(valuesById[1] = "REVOLUTE")] = 1
            values[(valuesById[2] = "SLIDER")] = 2
            values[(valuesById[3] = "CYLINDRICAL")] = 3
            values[(valuesById[4] = "PINSLOT")] = 4
            values[(valuesById[5] = "PLANAR")] = 5
            values[(valuesById[6] = "BALL")] = 6
            values[(valuesById[7] = "CUSTOM")] = 7
            return values
        })()

        joint.JointInstance = (function () {
            /**
             * Properties of a JointInstance.
             * @memberof mirabuf.joint
             * @interface IJointInstance
             * @property {mirabuf.IInfo|null} [info] JointInstance info
             * @property {boolean|null} [isEndEffector] JointInstance isEndEffector
             * @property {string|null} [parentPart] JointInstance parentPart
             * @property {string|null} [childPart] JointInstance childPart
             * @property {string|null} [jointReference] JointInstance jointReference
             * @property {mirabuf.IVector3|null} [offset] JointInstance offset
             * @property {mirabuf.IGraphContainer|null} [parts] JointInstance parts
             * @property {string|null} [signalReference] JointInstance signalReference
             * @property {Array.<mirabuf.joint.IMotionLink>|null} [motionLink] JointInstance motionLink
             */

            /**
             * Constructs a new JointInstance.
             * @memberof mirabuf.joint
             * @classdesc Instance of a Joint that has a defined motion and limits.
             * Instancing helps with identifiy closed loop systems.
             * @implements IJointInstance
             * @constructor
             * @param {mirabuf.joint.IJointInstance=} [properties] Properties to set
             */
            function JointInstance(properties) {
                this.motionLink = []
                if (properties)
                    for (let keys = Object.keys(properties), i = 0; i < keys.length; ++i)
                        if (properties[keys[i]] != null) this[keys[i]] = properties[keys[i]]
            }

            /**
             * JointInstance info.
             * @member {mirabuf.IInfo|null|undefined} info
             * @memberof mirabuf.joint.JointInstance
             * @instance
             */
            JointInstance.prototype.info = null

            /**
             * JointInstance isEndEffector.
             * @member {boolean} isEndEffector
             * @memberof mirabuf.joint.JointInstance
             * @instance
             */
            JointInstance.prototype.isEndEffector = false

            /**
             * JointInstance parentPart.
             * @member {string} parentPart
             * @memberof mirabuf.joint.JointInstance
             * @instance
             */
            JointInstance.prototype.parentPart = ""

            /**
             * JointInstance childPart.
             * @member {string} childPart
             * @memberof mirabuf.joint.JointInstance
             * @instance
             */
            JointInstance.prototype.childPart = ""

            /**
             * JointInstance jointReference.
             * @member {string} jointReference
             * @memberof mirabuf.joint.JointInstance
             * @instance
             */
            JointInstance.prototype.jointReference = ""

            /**
             * JointInstance offset.
             * @member {mirabuf.IVector3|null|undefined} offset
             * @memberof mirabuf.joint.JointInstance
             * @instance
             */
            JointInstance.prototype.offset = null

            /**
             * JointInstance parts.
             * @member {mirabuf.IGraphContainer|null|undefined} parts
             * @memberof mirabuf.joint.JointInstance
             * @instance
             */
            JointInstance.prototype.parts = null

            /**
             * JointInstance signalReference.
             * @member {string} signalReference
             * @memberof mirabuf.joint.JointInstance
             * @instance
             */
            JointInstance.prototype.signalReference = ""

            /**
             * JointInstance motionLink.
             * @member {Array.<mirabuf.joint.IMotionLink>} motionLink
             * @memberof mirabuf.joint.JointInstance
             * @instance
             */
            JointInstance.prototype.motionLink = $util.emptyArray

            /**
             * Creates a new JointInstance instance using the specified properties.
             * @function create
             * @memberof mirabuf.joint.JointInstance
             * @static
             * @param {mirabuf.joint.IJointInstance=} [properties] Properties to set
             * @returns {mirabuf.joint.JointInstance} JointInstance instance
             */
            JointInstance.create = function create(properties) {
                return new JointInstance(properties)
            }

            /**
             * Encodes the specified JointInstance message. Does not implicitly {@link mirabuf.joint.JointInstance.verify|verify} messages.
             * @function encode
             * @memberof mirabuf.joint.JointInstance
             * @static
             * @param {mirabuf.joint.IJointInstance} message JointInstance message or plain object to encode
             * @param {$protobuf.Writer} [writer] Writer to encode to
             * @returns {$protobuf.Writer} Writer
             */
            JointInstance.encode = function encode(message, writer) {
                if (!writer) writer = $Writer.create()
                if (message.info != null && Object.hasOwn(message, "info"))
                    $root.mirabuf.Info.encode(message.info, writer.uint32(/* id 1, wireType 2 =*/ 10).fork()).ldelim()
                if (message.isEndEffector != null && Object.hasOwn(message, "isEndEffector"))
                    writer.uint32(/* id 2, wireType 0 =*/ 16).bool(message.isEndEffector)
                if (message.parentPart != null && Object.hasOwn(message, "parentPart"))
                    writer.uint32(/* id 3, wireType 2 =*/ 26).string(message.parentPart)
                if (message.childPart != null && Object.hasOwn(message, "childPart"))
                    writer.uint32(/* id 4, wireType 2 =*/ 34).string(message.childPart)
                if (message.jointReference != null && Object.hasOwn(message, "jointReference"))
                    writer.uint32(/* id 5, wireType 2 =*/ 42).string(message.jointReference)
                if (message.offset != null && Object.hasOwn(message, "offset"))
                    $root.mirabuf.Vector3.encode(
                        message.offset,
                        writer.uint32(/* id 6, wireType 2 =*/ 50).fork()
                    ).ldelim()
                if (message.parts != null && Object.hasOwn(message, "parts"))
                    $root.mirabuf.GraphContainer.encode(
                        message.parts,
                        writer.uint32(/* id 7, wireType 2 =*/ 58).fork()
                    ).ldelim()
                if (message.signalReference != null && Object.hasOwn(message, "signalReference"))
                    writer.uint32(/* id 8, wireType 2 =*/ 66).string(message.signalReference)
                if (message.motionLink != null && message.motionLink.length)
                    for (let i = 0; i < message.motionLink.length; ++i)
                        $root.mirabuf.joint.MotionLink.encode(
                            message.motionLink[i],
                            writer.uint32(/* id 9, wireType 2 =*/ 74).fork()
                        ).ldelim()
                return writer
            }

            /**
             * Encodes the specified JointInstance message, length delimited. Does not implicitly {@link mirabuf.joint.JointInstance.verify|verify} messages.
             * @function encodeDelimited
             * @memberof mirabuf.joint.JointInstance
             * @static
             * @param {mirabuf.joint.IJointInstance} message JointInstance message or plain object to encode
             * @param {$protobuf.Writer} [writer] Writer to encode to
             * @returns {$protobuf.Writer} Writer
             */
            JointInstance.encodeDelimited = function encodeDelimited(message, writer) {
                return this.encode(message, writer).ldelim()
            }

            /**
             * Decodes a JointInstance message from the specified reader or buffer.
             * @function decode
             * @memberof mirabuf.joint.JointInstance
             * @static
             * @param {$protobuf.Reader|Uint8Array} reader Reader or buffer to decode from
             * @param {number} [length] Message length if known beforehand
             * @returns {mirabuf.joint.JointInstance} JointInstance
             * @throws {Error} If the payload is not a reader or valid buffer
             * @throws {$protobuf.util.ProtocolError} If required fields are missing
             */
            JointInstance.decode = function decode(reader, length) {
                if (!(reader instanceof $Reader)) reader = $Reader.create(reader)
                let end = length === undefined ? reader.len : reader.pos + length,
                    message = new $root.mirabuf.joint.JointInstance()
                while (reader.pos < end) {
                    let tag = reader.uint32()
                    switch (tag >>> 3) {
                        case 1: {
                            message.info = $root.mirabuf.Info.decode(reader, reader.uint32())
                            break
                        }
                        case 2: {
                            message.isEndEffector = reader.bool()
                            break
                        }
                        case 3: {
                            message.parentPart = reader.string()
                            break
                        }
                        case 4: {
                            message.childPart = reader.string()
                            break
                        }
                        case 5: {
                            message.jointReference = reader.string()
                            break
                        }
                        case 6: {
                            message.offset = $root.mirabuf.Vector3.decode(reader, reader.uint32())
                            break
                        }
                        case 7: {
                            message.parts = $root.mirabuf.GraphContainer.decode(reader, reader.uint32())
                            break
                        }
                        case 8: {
                            message.signalReference = reader.string()
                            break
                        }
                        case 9: {
                            if (!(message.motionLink && message.motionLink.length)) message.motionLink = []
                            message.motionLink.push($root.mirabuf.joint.MotionLink.decode(reader, reader.uint32()))
                            break
                        }
                        default:
                            reader.skipType(tag & 7)
                            break
                    }
                }
                return message
            }

            /**
             * Decodes a JointInstance message from the specified reader or buffer, length delimited.
             * @function decodeDelimited
             * @memberof mirabuf.joint.JointInstance
             * @static
             * @param {$protobuf.Reader|Uint8Array} reader Reader or buffer to decode from
             * @returns {mirabuf.joint.JointInstance} JointInstance
             * @throws {Error} If the payload is not a reader or valid buffer
             * @throws {$protobuf.util.ProtocolError} If required fields are missing
             */
            JointInstance.decodeDelimited = function decodeDelimited(reader) {
                if (!(reader instanceof $Reader)) reader = new $Reader(reader)
                return this.decode(reader, reader.uint32())
            }

            /**
             * Verifies a JointInstance message.
             * @function verify
             * @memberof mirabuf.joint.JointInstance
             * @static
             * @param {Object.<string,*>} message Plain object to verify
             * @returns {string|null} `null` if valid, otherwise the reason why it is not
             */
            JointInstance.verify = function verify(message) {
                if (typeof message !== "object" || message === null) return "object expected"
                if (message.info != null && Object.hasOwn(message, "info")) {
                    let error = $root.mirabuf.Info.verify(message.info)
                    if (error) return "info." + error
                }
                if (message.isEndEffector != null && Object.hasOwn(message, "isEndEffector"))
                    if (typeof message.isEndEffector !== "boolean") return "isEndEffector: boolean expected"
                if (message.parentPart != null && Object.hasOwn(message, "parentPart"))
                    if (!$util.isString(message.parentPart)) return "parentPart: string expected"
                if (message.childPart != null && Object.hasOwn(message, "childPart"))
                    if (!$util.isString(message.childPart)) return "childPart: string expected"
                if (message.jointReference != null && Object.hasOwn(message, "jointReference"))
                    if (!$util.isString(message.jointReference)) return "jointReference: string expected"
                if (message.offset != null && Object.hasOwn(message, "offset")) {
                    let error = $root.mirabuf.Vector3.verify(message.offset)
                    if (error) return "offset." + error
                }
                if (message.parts != null && Object.hasOwn(message, "parts")) {
                    let error = $root.mirabuf.GraphContainer.verify(message.parts)
                    if (error) return "parts." + error
                }
                if (message.signalReference != null && Object.hasOwn(message, "signalReference"))
                    if (!$util.isString(message.signalReference)) return "signalReference: string expected"
                if (message.motionLink != null && Object.hasOwn(message, "motionLink")) {
                    if (!Array.isArray(message.motionLink)) return "motionLink: array expected"
                    for (let i = 0; i < message.motionLink.length; ++i) {
                        let error = $root.mirabuf.joint.MotionLink.verify(message.motionLink[i])
                        if (error) return "motionLink." + error
                    }
                }
                return null
            }

            /**
             * Creates a JointInstance message from a plain object. Also converts values to their respective internal types.
             * @function fromObject
             * @memberof mirabuf.joint.JointInstance
             * @static
             * @param {Object.<string,*>} object Plain object
             * @returns {mirabuf.joint.JointInstance} JointInstance
             */
            JointInstance.fromObject = function fromObject(object) {
                if (object instanceof $root.mirabuf.joint.JointInstance) return object
                let message = new $root.mirabuf.joint.JointInstance()
                if (object.info != null) {
                    if (typeof object.info !== "object")
                        throw TypeError(".mirabuf.joint.JointInstance.info: object expected")
                    message.info = $root.mirabuf.Info.fromObject(object.info)
                }
                if (object.isEndEffector != null) message.isEndEffector = Boolean(object.isEndEffector)
                if (object.parentPart != null) message.parentPart = String(object.parentPart)
                if (object.childPart != null) message.childPart = String(object.childPart)
                if (object.jointReference != null) message.jointReference = String(object.jointReference)
                if (object.offset != null) {
                    if (typeof object.offset !== "object")
                        throw TypeError(".mirabuf.joint.JointInstance.offset: object expected")
                    message.offset = $root.mirabuf.Vector3.fromObject(object.offset)
                }
                if (object.parts != null) {
                    if (typeof object.parts !== "object")
                        throw TypeError(".mirabuf.joint.JointInstance.parts: object expected")
                    message.parts = $root.mirabuf.GraphContainer.fromObject(object.parts)
                }
                if (object.signalReference != null) message.signalReference = String(object.signalReference)
                if (object.motionLink) {
                    if (!Array.isArray(object.motionLink))
                        throw TypeError(".mirabuf.joint.JointInstance.motionLink: array expected")
                    message.motionLink = []
                    for (let i = 0; i < object.motionLink.length; ++i) {
                        if (typeof object.motionLink[i] !== "object")
                            throw TypeError(".mirabuf.joint.JointInstance.motionLink: object expected")
                        message.motionLink[i] = $root.mirabuf.joint.MotionLink.fromObject(object.motionLink[i])
                    }
                }
                return message
            }

            /**
             * Creates a plain object from a JointInstance message. Also converts values to other types if specified.
             * @function toObject
             * @memberof mirabuf.joint.JointInstance
             * @static
             * @param {mirabuf.joint.JointInstance} message JointInstance
             * @param {$protobuf.IConversionOptions} [options] Conversion options
             * @returns {Object.<string,*>} Plain object
             */
            JointInstance.toObject = function toObject(message, options) {
                if (!options) options = {}
                let object = {}
                if (options.arrays || options.defaults) object.motionLink = []
                if (options.defaults) {
                    object.info = null
                    object.isEndEffector = false
                    object.parentPart = ""
                    object.childPart = ""
                    object.jointReference = ""
                    object.offset = null
                    object.parts = null
                    object.signalReference = ""
                }
                if (message.info != null && Object.hasOwn(message, "info"))
                    object.info = $root.mirabuf.Info.toObject(message.info, options)
                if (message.isEndEffector != null && Object.hasOwn(message, "isEndEffector"))
                    object.isEndEffector = message.isEndEffector
                if (message.parentPart != null && Object.hasOwn(message, "parentPart"))
                    object.parentPart = message.parentPart
                if (message.childPart != null && Object.hasOwn(message, "childPart"))
                    object.childPart = message.childPart
                if (message.jointReference != null && Object.hasOwn(message, "jointReference"))
                    object.jointReference = message.jointReference
                if (message.offset != null && Object.hasOwn(message, "offset"))
                    object.offset = $root.mirabuf.Vector3.toObject(message.offset, options)
                if (message.parts != null && Object.hasOwn(message, "parts"))
                    object.parts = $root.mirabuf.GraphContainer.toObject(message.parts, options)
                if (message.signalReference != null && Object.hasOwn(message, "signalReference"))
                    object.signalReference = message.signalReference
                if (message.motionLink && message.motionLink.length) {
                    object.motionLink = []
                    for (let j = 0; j < message.motionLink.length; ++j)
                        object.motionLink[j] = $root.mirabuf.joint.MotionLink.toObject(message.motionLink[j], options)
                }
                return object
            }

            /**
             * Converts this JointInstance to JSON.
             * @function toJSON
             * @memberof mirabuf.joint.JointInstance
             * @instance
             * @returns {Object.<string,*>} JSON object
             */
            JointInstance.prototype.toJSON = function toJSON() {
                return this.constructor.toObject(this, $protobuf.util.toJSONOptions)
            }

            /**
             * Gets the default type url for JointInstance
             * @function getTypeUrl
             * @memberof mirabuf.joint.JointInstance
             * @static
             * @param {string} [typeUrlPrefix] your custom typeUrlPrefix(default "type.googleapis.com")
             * @returns {string} The default type url
             */
            JointInstance.getTypeUrl = function getTypeUrl(typeUrlPrefix) {
                if (typeUrlPrefix === undefined) {
                    typeUrlPrefix = "type.googleapis.com"
                }
                return typeUrlPrefix + "/mirabuf.joint.JointInstance"
            }

            return JointInstance
        })()

        joint.MotionLink = (function () {
            /**
             * Properties of a MotionLink.
             * @memberof mirabuf.joint
             * @interface IMotionLink
             * @property {string|null} [jointInstance] MotionLink jointInstance
             * @property {number|null} [ratio] MotionLink ratio
             * @property {boolean|null} [reversed] MotionLink reversed
             */

            /**
             * Constructs a new MotionLink.
             * @memberof mirabuf.joint
             * @classdesc Motion Link Feature
             * Enables the restriction on a joint to a certain range of motion as it is relative to another joint
             * This is useful for moving parts restricted by belts and gears
             * @implements IMotionLink
             * @constructor
             * @param {mirabuf.joint.IMotionLink=} [properties] Properties to set
             */
            function MotionLink(properties) {
                if (properties)
                    for (let keys = Object.keys(properties), i = 0; i < keys.length; ++i)
                        if (properties[keys[i]] != null) this[keys[i]] = properties[keys[i]]
            }

            /**
             * MotionLink jointInstance.
             * @member {string} jointInstance
             * @memberof mirabuf.joint.MotionLink
             * @instance
             */
            MotionLink.prototype.jointInstance = ""

            /**
             * MotionLink ratio.
             * @member {number} ratio
             * @memberof mirabuf.joint.MotionLink
             * @instance
             */
            MotionLink.prototype.ratio = 0

            /**
             * MotionLink reversed.
             * @member {boolean} reversed
             * @memberof mirabuf.joint.MotionLink
             * @instance
             */
            MotionLink.prototype.reversed = false

            /**
             * Creates a new MotionLink instance using the specified properties.
             * @function create
             * @memberof mirabuf.joint.MotionLink
             * @static
             * @param {mirabuf.joint.IMotionLink=} [properties] Properties to set
             * @returns {mirabuf.joint.MotionLink} MotionLink instance
             */
            MotionLink.create = function create(properties) {
                return new MotionLink(properties)
            }

            /**
             * Encodes the specified MotionLink message. Does not implicitly {@link mirabuf.joint.MotionLink.verify|verify} messages.
             * @function encode
             * @memberof mirabuf.joint.MotionLink
             * @static
             * @param {mirabuf.joint.IMotionLink} message MotionLink message or plain object to encode
             * @param {$protobuf.Writer} [writer] Writer to encode to
             * @returns {$protobuf.Writer} Writer
             */
            MotionLink.encode = function encode(message, writer) {
                if (!writer) writer = $Writer.create()
                if (message.jointInstance != null && Object.hasOwn(message, "jointInstance"))
                    writer.uint32(/* id 1, wireType 2 =*/ 10).string(message.jointInstance)
                if (message.ratio != null && Object.hasOwn(message, "ratio"))
                    writer.uint32(/* id 2, wireType 5 =*/ 21).float(message.ratio)
                if (message.reversed != null && Object.hasOwn(message, "reversed"))
                    writer.uint32(/* id 3, wireType 0 =*/ 24).bool(message.reversed)
                return writer
            }

            /**
             * Encodes the specified MotionLink message, length delimited. Does not implicitly {@link mirabuf.joint.MotionLink.verify|verify} messages.
             * @function encodeDelimited
             * @memberof mirabuf.joint.MotionLink
             * @static
             * @param {mirabuf.joint.IMotionLink} message MotionLink message or plain object to encode
             * @param {$protobuf.Writer} [writer] Writer to encode to
             * @returns {$protobuf.Writer} Writer
             */
            MotionLink.encodeDelimited = function encodeDelimited(message, writer) {
                return this.encode(message, writer).ldelim()
            }

            /**
             * Decodes a MotionLink message from the specified reader or buffer.
             * @function decode
             * @memberof mirabuf.joint.MotionLink
             * @static
             * @param {$protobuf.Reader|Uint8Array} reader Reader or buffer to decode from
             * @param {number} [length] Message length if known beforehand
             * @returns {mirabuf.joint.MotionLink} MotionLink
             * @throws {Error} If the payload is not a reader or valid buffer
             * @throws {$protobuf.util.ProtocolError} If required fields are missing
             */
            MotionLink.decode = function decode(reader, length) {
                if (!(reader instanceof $Reader)) reader = $Reader.create(reader)
                let end = length === undefined ? reader.len : reader.pos + length,
                    message = new $root.mirabuf.joint.MotionLink()
                while (reader.pos < end) {
                    let tag = reader.uint32()
                    switch (tag >>> 3) {
                        case 1: {
                            message.jointInstance = reader.string()
                            break
                        }
                        case 2: {
                            message.ratio = reader.float()
                            break
                        }
                        case 3: {
                            message.reversed = reader.bool()
                            break
                        }
                        default:
                            reader.skipType(tag & 7)
                            break
                    }
                }
                return message
            }

            /**
             * Decodes a MotionLink message from the specified reader or buffer, length delimited.
             * @function decodeDelimited
             * @memberof mirabuf.joint.MotionLink
             * @static
             * @param {$protobuf.Reader|Uint8Array} reader Reader or buffer to decode from
             * @returns {mirabuf.joint.MotionLink} MotionLink
             * @throws {Error} If the payload is not a reader or valid buffer
             * @throws {$protobuf.util.ProtocolError} If required fields are missing
             */
            MotionLink.decodeDelimited = function decodeDelimited(reader) {
                if (!(reader instanceof $Reader)) reader = new $Reader(reader)
                return this.decode(reader, reader.uint32())
            }

            /**
             * Verifies a MotionLink message.
             * @function verify
             * @memberof mirabuf.joint.MotionLink
             * @static
             * @param {Object.<string,*>} message Plain object to verify
             * @returns {string|null} `null` if valid, otherwise the reason why it is not
             */
            MotionLink.verify = function verify(message) {
                if (typeof message !== "object" || message === null) return "object expected"
                if (message.jointInstance != null && Object.hasOwn(message, "jointInstance"))
                    if (!$util.isString(message.jointInstance)) return "jointInstance: string expected"
                if (message.ratio != null && Object.hasOwn(message, "ratio"))
                    if (typeof message.ratio !== "number") return "ratio: number expected"
                if (message.reversed != null && Object.hasOwn(message, "reversed"))
                    if (typeof message.reversed !== "boolean") return "reversed: boolean expected"
                return null
            }

            /**
             * Creates a MotionLink message from a plain object. Also converts values to their respective internal types.
             * @function fromObject
             * @memberof mirabuf.joint.MotionLink
             * @static
             * @param {Object.<string,*>} object Plain object
             * @returns {mirabuf.joint.MotionLink} MotionLink
             */
            MotionLink.fromObject = function fromObject(object) {
                if (object instanceof $root.mirabuf.joint.MotionLink) return object
                let message = new $root.mirabuf.joint.MotionLink()
                if (object.jointInstance != null) message.jointInstance = String(object.jointInstance)
                if (object.ratio != null) message.ratio = Number(object.ratio)
                if (object.reversed != null) message.reversed = Boolean(object.reversed)
                return message
            }

            /**
             * Creates a plain object from a MotionLink message. Also converts values to other types if specified.
             * @function toObject
             * @memberof mirabuf.joint.MotionLink
             * @static
             * @param {mirabuf.joint.MotionLink} message MotionLink
             * @param {$protobuf.IConversionOptions} [options] Conversion options
             * @returns {Object.<string,*>} Plain object
             */
            MotionLink.toObject = function toObject(message, options) {
                if (!options) options = {}
                let object = {}
                if (options.defaults) {
                    object.jointInstance = ""
                    object.ratio = 0
                    object.reversed = false
                }
                if (message.jointInstance != null && Object.hasOwn(message, "jointInstance"))
                    object.jointInstance = message.jointInstance
                if (message.ratio != null && Object.hasOwn(message, "ratio"))
                    object.ratio = options.json && !isFinite(message.ratio) ? String(message.ratio) : message.ratio
                if (message.reversed != null && Object.hasOwn(message, "reversed")) object.reversed = message.reversed
                return object
            }

            /**
             * Converts this MotionLink to JSON.
             * @function toJSON
             * @memberof mirabuf.joint.MotionLink
             * @instance
             * @returns {Object.<string,*>} JSON object
             */
            MotionLink.prototype.toJSON = function toJSON() {
                return this.constructor.toObject(this, $protobuf.util.toJSONOptions)
            }

            /**
             * Gets the default type url for MotionLink
             * @function getTypeUrl
             * @memberof mirabuf.joint.MotionLink
             * @static
             * @param {string} [typeUrlPrefix] your custom typeUrlPrefix(default "type.googleapis.com")
             * @returns {string} The default type url
             */
            MotionLink.getTypeUrl = function getTypeUrl(typeUrlPrefix) {
                if (typeUrlPrefix === undefined) {
                    typeUrlPrefix = "type.googleapis.com"
                }
                return typeUrlPrefix + "/mirabuf.joint.MotionLink"
            }

            return MotionLink
        })()

        joint.Joint = (function () {
            /**
             * Properties of a Joint.
             * @memberof mirabuf.joint
             * @interface IJoint
             * @property {mirabuf.IInfo|null} [info] Joint name, ID, version, etc
             * @property {mirabuf.IVector3|null} [origin] Joint origin
             * @property {mirabuf.joint.JointMotion|null} [jointMotionType] Joint jointMotionType
             * @property {number|null} [breakMagnitude] Joint breakMagnitude
             * @property {mirabuf.joint.IRotationalJoint|null} [rotational] ONEOF rotational joint
             * @property {mirabuf.joint.IPrismaticJoint|null} [prismatic] ONEOF prismatic joint
             * @property {mirabuf.joint.ICustomJoint|null} [custom] ONEOF custom joint
             * @property {mirabuf.IUserData|null} [userData] Additional information someone can query or store relative to your joint.
             * @property {string|null} [motorReference] Motor definition reference to lookup in joints collection
             */

            /**
             * Constructs a new Joint.
             * @memberof mirabuf.joint
             * @classdesc A unqiue implementation of a joint motion
             * Contains information about motion but not assembly relation
             * NOTE: A spring motion is a joint with no driver
             * @implements IJoint
             * @constructor
             * @param {mirabuf.joint.IJoint=} [properties] Properties to set
             */
            function Joint(properties) {
                if (properties)
                    for (let keys = Object.keys(properties), i = 0; i < keys.length; ++i)
                        if (properties[keys[i]] != null) this[keys[i]] = properties[keys[i]]
            }

            /**
             * Joint name, ID, version, etc
             * @member {mirabuf.IInfo|null|undefined} info
             * @memberof mirabuf.joint.Joint
             * @instance
             */
            Joint.prototype.info = null

            /**
             * Joint origin.
             * @member {mirabuf.IVector3|null|undefined} origin
             * @memberof mirabuf.joint.Joint
             * @instance
             */
            Joint.prototype.origin = null

            /**
             * Joint jointMotionType.
             * @member {mirabuf.joint.JointMotion} jointMotionType
             * @memberof mirabuf.joint.Joint
             * @instance
             */
            Joint.prototype.jointMotionType = 0

            /**
             * Joint breakMagnitude.
             * @member {number} breakMagnitude
             * @memberof mirabuf.joint.Joint
             * @instance
             */
            Joint.prototype.breakMagnitude = 0

            /**
             * ONEOF rotational joint
             * @member {mirabuf.joint.IRotationalJoint|null|undefined} rotational
             * @memberof mirabuf.joint.Joint
             * @instance
             */
            Joint.prototype.rotational = null

            /**
             * ONEOF prismatic joint
             * @member {mirabuf.joint.IPrismaticJoint|null|undefined} prismatic
             * @memberof mirabuf.joint.Joint
             * @instance
             */
            Joint.prototype.prismatic = null

            /**
             * ONEOF custom joint
             * @member {mirabuf.joint.ICustomJoint|null|undefined} custom
             * @memberof mirabuf.joint.Joint
             * @instance
             */
            Joint.prototype.custom = null

            /**
             * Additional information someone can query or store relative to your joint.
             * @member {mirabuf.IUserData|null|undefined} userData
             * @memberof mirabuf.joint.Joint
             * @instance
             */
            Joint.prototype.userData = null

            /**
             * Motor definition reference to lookup in joints collection
             * @member {string} motorReference
             * @memberof mirabuf.joint.Joint
             * @instance
             */
            Joint.prototype.motorReference = ""

            // OneOf field names bound to virtual getters and setters
            let $oneOfFields

            /**
             * Joint JointMotion.
             * @member {"rotational"|"prismatic"|"custom"|undefined} JointMotion
             * @memberof mirabuf.joint.Joint
             * @instance
             */
            Object.defineProperty(Joint.prototype, "JointMotion", {
                get: $util.oneOfGetter(($oneOfFields = ["rotational", "prismatic", "custom"])),
                set: $util.oneOfSetter($oneOfFields),
            })

            /**
             * Creates a new Joint instance using the specified properties.
             * @function create
             * @memberof mirabuf.joint.Joint
             * @static
             * @param {mirabuf.joint.IJoint=} [properties] Properties to set
             * @returns {mirabuf.joint.Joint} Joint instance
             */
            Joint.create = function create(properties) {
                return new Joint(properties)
            }

            /**
             * Encodes the specified Joint message. Does not implicitly {@link mirabuf.joint.Joint.verify|verify} messages.
             * @function encode
             * @memberof mirabuf.joint.Joint
             * @static
             * @param {mirabuf.joint.IJoint} message Joint message or plain object to encode
             * @param {$protobuf.Writer} [writer] Writer to encode to
             * @returns {$protobuf.Writer} Writer
             */
            Joint.encode = function encode(message, writer) {
                if (!writer) writer = $Writer.create()
                if (message.info != null && Object.hasOwn(message, "info"))
                    $root.mirabuf.Info.encode(message.info, writer.uint32(/* id 1, wireType 2 =*/ 10).fork()).ldelim()
                if (message.origin != null && Object.hasOwn(message, "origin"))
                    $root.mirabuf.Vector3.encode(
                        message.origin,
                        writer.uint32(/* id 2, wireType 2 =*/ 18).fork()
                    ).ldelim()
                if (message.jointMotionType != null && Object.hasOwn(message, "jointMotionType"))
                    writer.uint32(/* id 3, wireType 0 =*/ 24).int32(message.jointMotionType)
                if (message.breakMagnitude != null && Object.hasOwn(message, "breakMagnitude"))
                    writer.uint32(/* id 4, wireType 5 =*/ 37).float(message.breakMagnitude)
                if (message.rotational != null && Object.hasOwn(message, "rotational"))
                    $root.mirabuf.joint.RotationalJoint.encode(
                        message.rotational,
                        writer.uint32(/* id 5, wireType 2 =*/ 42).fork()
                    ).ldelim()
                if (message.prismatic != null && Object.hasOwn(message, "prismatic"))
                    $root.mirabuf.joint.PrismaticJoint.encode(
                        message.prismatic,
                        writer.uint32(/* id 6, wireType 2 =*/ 50).fork()
                    ).ldelim()
                if (message.custom != null && Object.hasOwn(message, "custom"))
                    $root.mirabuf.joint.CustomJoint.encode(
                        message.custom,
                        writer.uint32(/* id 7, wireType 2 =*/ 58).fork()
                    ).ldelim()
                if (message.userData != null && Object.hasOwn(message, "userData"))
                    $root.mirabuf.UserData.encode(
                        message.userData,
                        writer.uint32(/* id 8, wireType 2 =*/ 66).fork()
                    ).ldelim()
                if (message.motorReference != null && Object.hasOwn(message, "motorReference"))
                    writer.uint32(/* id 9, wireType 2 =*/ 74).string(message.motorReference)
                return writer
            }

            /**
             * Encodes the specified Joint message, length delimited. Does not implicitly {@link mirabuf.joint.Joint.verify|verify} messages.
             * @function encodeDelimited
             * @memberof mirabuf.joint.Joint
             * @static
             * @param {mirabuf.joint.IJoint} message Joint message or plain object to encode
             * @param {$protobuf.Writer} [writer] Writer to encode to
             * @returns {$protobuf.Writer} Writer
             */
            Joint.encodeDelimited = function encodeDelimited(message, writer) {
                return this.encode(message, writer).ldelim()
            }

            /**
             * Decodes a Joint message from the specified reader or buffer.
             * @function decode
             * @memberof mirabuf.joint.Joint
             * @static
             * @param {$protobuf.Reader|Uint8Array} reader Reader or buffer to decode from
             * @param {number} [length] Message length if known beforehand
             * @returns {mirabuf.joint.Joint} Joint
             * @throws {Error} If the payload is not a reader or valid buffer
             * @throws {$protobuf.util.ProtocolError} If required fields are missing
             */
            Joint.decode = function decode(reader, length) {
                if (!(reader instanceof $Reader)) reader = $Reader.create(reader)
                let end = length === undefined ? reader.len : reader.pos + length,
                    message = new $root.mirabuf.joint.Joint()
                while (reader.pos < end) {
                    let tag = reader.uint32()
                    switch (tag >>> 3) {
                        case 1: {
                            message.info = $root.mirabuf.Info.decode(reader, reader.uint32())
                            break
                        }
                        case 2: {
                            message.origin = $root.mirabuf.Vector3.decode(reader, reader.uint32())
                            break
                        }
                        case 3: {
                            message.jointMotionType = reader.int32()
                            break
                        }
                        case 4: {
                            message.breakMagnitude = reader.float()
                            break
                        }
                        case 5: {
                            message.rotational = $root.mirabuf.joint.RotationalJoint.decode(reader, reader.uint32())
                            break
                        }
                        case 6: {
                            message.prismatic = $root.mirabuf.joint.PrismaticJoint.decode(reader, reader.uint32())
                            break
                        }
                        case 7: {
                            message.custom = $root.mirabuf.joint.CustomJoint.decode(reader, reader.uint32())
                            break
                        }
                        case 8: {
                            message.userData = $root.mirabuf.UserData.decode(reader, reader.uint32())
                            break
                        }
                        case 9: {
                            message.motorReference = reader.string()
                            break
                        }
                        default:
                            reader.skipType(tag & 7)
                            break
                    }
                }
                return message
            }

            /**
             * Decodes a Joint message from the specified reader or buffer, length delimited.
             * @function decodeDelimited
             * @memberof mirabuf.joint.Joint
             * @static
             * @param {$protobuf.Reader|Uint8Array} reader Reader or buffer to decode from
             * @returns {mirabuf.joint.Joint} Joint
             * @throws {Error} If the payload is not a reader or valid buffer
             * @throws {$protobuf.util.ProtocolError} If required fields are missing
             */
            Joint.decodeDelimited = function decodeDelimited(reader) {
                if (!(reader instanceof $Reader)) reader = new $Reader(reader)
                return this.decode(reader, reader.uint32())
            }

            /**
             * Verifies a Joint message.
             * @function verify
             * @memberof mirabuf.joint.Joint
             * @static
             * @param {Object.<string,*>} message Plain object to verify
             * @returns {string|null} `null` if valid, otherwise the reason why it is not
             */
            Joint.verify = function verify(message) {
                if (typeof message !== "object" || message === null) return "object expected"
                let properties = {}
                if (message.info != null && Object.hasOwn(message, "info")) {
                    let error = $root.mirabuf.Info.verify(message.info)
                    if (error) return "info." + error
                }
                if (message.origin != null && Object.hasOwn(message, "origin")) {
                    let error = $root.mirabuf.Vector3.verify(message.origin)
                    if (error) return "origin." + error
                }
                if (message.jointMotionType != null && Object.hasOwn(message, "jointMotionType"))
                    switch (message.jointMotionType) {
                        default:
                            return "jointMotionType: enum value expected"
                        case 0:
                        case 1:
                        case 2:
                        case 3:
                        case 4:
                        case 5:
                        case 6:
                        case 7:
                            break
                    }
                if (message.breakMagnitude != null && Object.hasOwn(message, "breakMagnitude"))
                    if (typeof message.breakMagnitude !== "number") return "breakMagnitude: number expected"
                if (message.rotational != null && Object.hasOwn(message, "rotational")) {
                    properties.JointMotion = 1
                    {
                        let error = $root.mirabuf.joint.RotationalJoint.verify(message.rotational)
                        if (error) return "rotational." + error
                    }
                }
                if (message.prismatic != null && Object.hasOwn(message, "prismatic")) {
                    if (properties.JointMotion === 1) return "JointMotion: multiple values"
                    properties.JointMotion = 1
                    {
                        let error = $root.mirabuf.joint.PrismaticJoint.verify(message.prismatic)
                        if (error) return "prismatic." + error
                    }
                }
                if (message.custom != null && Object.hasOwn(message, "custom")) {
                    if (properties.JointMotion === 1) return "JointMotion: multiple values"
                    properties.JointMotion = 1
                    {
                        let error = $root.mirabuf.joint.CustomJoint.verify(message.custom)
                        if (error) return "custom." + error
                    }
                }
                if (message.userData != null && Object.hasOwn(message, "userData")) {
                    let error = $root.mirabuf.UserData.verify(message.userData)
                    if (error) return "userData." + error
                }
                if (message.motorReference != null && Object.hasOwn(message, "motorReference"))
                    if (!$util.isString(message.motorReference)) return "motorReference: string expected"
                return null
            }

            /**
             * Creates a Joint message from a plain object. Also converts values to their respective internal types.
             * @function fromObject
             * @memberof mirabuf.joint.Joint
             * @static
             * @param {Object.<string,*>} object Plain object
             * @returns {mirabuf.joint.Joint} Joint
             */
            Joint.fromObject = function fromObject(object) {
                if (object instanceof $root.mirabuf.joint.Joint) return object
                let message = new $root.mirabuf.joint.Joint()
                if (object.info != null) {
                    if (typeof object.info !== "object") throw TypeError(".mirabuf.joint.Joint.info: object expected")
                    message.info = $root.mirabuf.Info.fromObject(object.info)
                }
                if (object.origin != null) {
                    if (typeof object.origin !== "object")
                        throw TypeError(".mirabuf.joint.Joint.origin: object expected")
                    message.origin = $root.mirabuf.Vector3.fromObject(object.origin)
                }
                switch (object.jointMotionType) {
                    default:
                        if (typeof object.jointMotionType === "number") {
                            message.jointMotionType = object.jointMotionType
                            break
                        }
                        break
                    case "RIGID":
                    case 0:
                        message.jointMotionType = 0
                        break
                    case "REVOLUTE":
                    case 1:
                        message.jointMotionType = 1
                        break
                    case "SLIDER":
                    case 2:
                        message.jointMotionType = 2
                        break
                    case "CYLINDRICAL":
                    case 3:
                        message.jointMotionType = 3
                        break
                    case "PINSLOT":
                    case 4:
                        message.jointMotionType = 4
                        break
                    case "PLANAR":
                    case 5:
                        message.jointMotionType = 5
                        break
                    case "BALL":
                    case 6:
                        message.jointMotionType = 6
                        break
                    case "CUSTOM":
                    case 7:
                        message.jointMotionType = 7
                        break
                }
                if (object.breakMagnitude != null) message.breakMagnitude = Number(object.breakMagnitude)
                if (object.rotational != null) {
                    if (typeof object.rotational !== "object")
                        throw TypeError(".mirabuf.joint.Joint.rotational: object expected")
                    message.rotational = $root.mirabuf.joint.RotationalJoint.fromObject(object.rotational)
                }
                if (object.prismatic != null) {
                    if (typeof object.prismatic !== "object")
                        throw TypeError(".mirabuf.joint.Joint.prismatic: object expected")
                    message.prismatic = $root.mirabuf.joint.PrismaticJoint.fromObject(object.prismatic)
                }
                if (object.custom != null) {
                    if (typeof object.custom !== "object")
                        throw TypeError(".mirabuf.joint.Joint.custom: object expected")
                    message.custom = $root.mirabuf.joint.CustomJoint.fromObject(object.custom)
                }
                if (object.userData != null) {
                    if (typeof object.userData !== "object")
                        throw TypeError(".mirabuf.joint.Joint.userData: object expected")
                    message.userData = $root.mirabuf.UserData.fromObject(object.userData)
                }
                if (object.motorReference != null) message.motorReference = String(object.motorReference)
                return message
            }

            /**
             * Creates a plain object from a Joint message. Also converts values to other types if specified.
             * @function toObject
             * @memberof mirabuf.joint.Joint
             * @static
             * @param {mirabuf.joint.Joint} message Joint
             * @param {$protobuf.IConversionOptions} [options] Conversion options
             * @returns {Object.<string,*>} Plain object
             */
            Joint.toObject = function toObject(message, options) {
                if (!options) options = {}
                let object = {}
                if (options.defaults) {
                    object.info = null
                    object.origin = null
                    object.jointMotionType = options.enums === String ? "RIGID" : 0
                    object.breakMagnitude = 0
                    object.userData = null
                    object.motorReference = ""
                }
                if (message.info != null && Object.hasOwn(message, "info"))
                    object.info = $root.mirabuf.Info.toObject(message.info, options)
                if (message.origin != null && Object.hasOwn(message, "origin"))
                    object.origin = $root.mirabuf.Vector3.toObject(message.origin, options)
                if (message.jointMotionType != null && Object.hasOwn(message, "jointMotionType"))
                    object.jointMotionType =
                        options.enums === String
                            ? $root.mirabuf.joint.JointMotion[message.jointMotionType] === undefined
                                ? message.jointMotionType
                                : $root.mirabuf.joint.JointMotion[message.jointMotionType]
                            : message.jointMotionType
                if (message.breakMagnitude != null && Object.hasOwn(message, "breakMagnitude"))
                    object.breakMagnitude =
                        options.json && !isFinite(message.breakMagnitude)
                            ? String(message.breakMagnitude)
                            : message.breakMagnitude
                if (message.rotational != null && Object.hasOwn(message, "rotational")) {
                    object.rotational = $root.mirabuf.joint.RotationalJoint.toObject(message.rotational, options)
                    if (options.oneofs) object.JointMotion = "rotational"
                }
                if (message.prismatic != null && Object.hasOwn(message, "prismatic")) {
                    object.prismatic = $root.mirabuf.joint.PrismaticJoint.toObject(message.prismatic, options)
                    if (options.oneofs) object.JointMotion = "prismatic"
                }
                if (message.custom != null && Object.hasOwn(message, "custom")) {
                    object.custom = $root.mirabuf.joint.CustomJoint.toObject(message.custom, options)
                    if (options.oneofs) object.JointMotion = "custom"
                }
                if (message.userData != null && Object.hasOwn(message, "userData"))
                    object.userData = $root.mirabuf.UserData.toObject(message.userData, options)
                if (message.motorReference != null && Object.hasOwn(message, "motorReference"))
                    object.motorReference = message.motorReference
                return object
            }

            /**
             * Converts this Joint to JSON.
             * @function toJSON
             * @memberof mirabuf.joint.Joint
             * @instance
             * @returns {Object.<string,*>} JSON object
             */
            Joint.prototype.toJSON = function toJSON() {
                return this.constructor.toObject(this, $protobuf.util.toJSONOptions)
            }

            /**
             * Gets the default type url for Joint
             * @function getTypeUrl
             * @memberof mirabuf.joint.Joint
             * @static
             * @param {string} [typeUrlPrefix] your custom typeUrlPrefix(default "type.googleapis.com")
             * @returns {string} The default type url
             */
            Joint.getTypeUrl = function getTypeUrl(typeUrlPrefix) {
                if (typeUrlPrefix === undefined) {
                    typeUrlPrefix = "type.googleapis.com"
                }
                return typeUrlPrefix + "/mirabuf.joint.Joint"
            }

            return Joint
        })()

        joint.Dynamics = (function () {
            /**
             * Properties of a Dynamics.
             * @memberof mirabuf.joint
             * @interface IDynamics
             * @property {number|null} [damping] Damping effect on a given joint motion
             * @property {number|null} [friction] Friction effect on a given joint motion
             */

            /**
             * Constructs a new Dynamics.
             * @memberof mirabuf.joint
             * @classdesc Dynamics specify the mechanical effects on the motion.
             * @implements IDynamics
             * @constructor
             * @param {mirabuf.joint.IDynamics=} [properties] Properties to set
             */
            function Dynamics(properties) {
                if (properties)
                    for (let keys = Object.keys(properties), i = 0; i < keys.length; ++i)
                        if (properties[keys[i]] != null) this[keys[i]] = properties[keys[i]]
            }

            /**
             * Damping effect on a given joint motion
             * @member {number} damping
             * @memberof mirabuf.joint.Dynamics
             * @instance
             */
            Dynamics.prototype.damping = 0

            /**
             * Friction effect on a given joint motion
             * @member {number} friction
             * @memberof mirabuf.joint.Dynamics
             * @instance
             */
            Dynamics.prototype.friction = 0

            /**
             * Creates a new Dynamics instance using the specified properties.
             * @function create
             * @memberof mirabuf.joint.Dynamics
             * @static
             * @param {mirabuf.joint.IDynamics=} [properties] Properties to set
             * @returns {mirabuf.joint.Dynamics} Dynamics instance
             */
            Dynamics.create = function create(properties) {
                return new Dynamics(properties)
            }

            /**
             * Encodes the specified Dynamics message. Does not implicitly {@link mirabuf.joint.Dynamics.verify|verify} messages.
             * @function encode
             * @memberof mirabuf.joint.Dynamics
             * @static
             * @param {mirabuf.joint.IDynamics} message Dynamics message or plain object to encode
             * @param {$protobuf.Writer} [writer] Writer to encode to
             * @returns {$protobuf.Writer} Writer
             */
            Dynamics.encode = function encode(message, writer) {
                if (!writer) writer = $Writer.create()
                if (message.damping != null && Object.hasOwn(message, "damping"))
                    writer.uint32(/* id 1, wireType 5 =*/ 13).float(message.damping)
                if (message.friction != null && Object.hasOwn(message, "friction"))
                    writer.uint32(/* id 2, wireType 5 =*/ 21).float(message.friction)
                return writer
            }

            /**
             * Encodes the specified Dynamics message, length delimited. Does not implicitly {@link mirabuf.joint.Dynamics.verify|verify} messages.
             * @function encodeDelimited
             * @memberof mirabuf.joint.Dynamics
             * @static
             * @param {mirabuf.joint.IDynamics} message Dynamics message or plain object to encode
             * @param {$protobuf.Writer} [writer] Writer to encode to
             * @returns {$protobuf.Writer} Writer
             */
            Dynamics.encodeDelimited = function encodeDelimited(message, writer) {
                return this.encode(message, writer).ldelim()
            }

            /**
             * Decodes a Dynamics message from the specified reader or buffer.
             * @function decode
             * @memberof mirabuf.joint.Dynamics
             * @static
             * @param {$protobuf.Reader|Uint8Array} reader Reader or buffer to decode from
             * @param {number} [length] Message length if known beforehand
             * @returns {mirabuf.joint.Dynamics} Dynamics
             * @throws {Error} If the payload is not a reader or valid buffer
             * @throws {$protobuf.util.ProtocolError} If required fields are missing
             */
            Dynamics.decode = function decode(reader, length) {
                if (!(reader instanceof $Reader)) reader = $Reader.create(reader)
                let end = length === undefined ? reader.len : reader.pos + length,
                    message = new $root.mirabuf.joint.Dynamics()
                while (reader.pos < end) {
                    let tag = reader.uint32()
                    switch (tag >>> 3) {
                        case 1: {
                            message.damping = reader.float()
                            break
                        }
                        case 2: {
                            message.friction = reader.float()
                            break
                        }
                        default:
                            reader.skipType(tag & 7)
                            break
                    }
                }
                return message
            }

            /**
             * Decodes a Dynamics message from the specified reader or buffer, length delimited.
             * @function decodeDelimited
             * @memberof mirabuf.joint.Dynamics
             * @static
             * @param {$protobuf.Reader|Uint8Array} reader Reader or buffer to decode from
             * @returns {mirabuf.joint.Dynamics} Dynamics
             * @throws {Error} If the payload is not a reader or valid buffer
             * @throws {$protobuf.util.ProtocolError} If required fields are missing
             */
            Dynamics.decodeDelimited = function decodeDelimited(reader) {
                if (!(reader instanceof $Reader)) reader = new $Reader(reader)
                return this.decode(reader, reader.uint32())
            }

            /**
             * Verifies a Dynamics message.
             * @function verify
             * @memberof mirabuf.joint.Dynamics
             * @static
             * @param {Object.<string,*>} message Plain object to verify
             * @returns {string|null} `null` if valid, otherwise the reason why it is not
             */
            Dynamics.verify = function verify(message) {
                if (typeof message !== "object" || message === null) return "object expected"
                if (message.damping != null && Object.hasOwn(message, "damping"))
                    if (typeof message.damping !== "number") return "damping: number expected"
                if (message.friction != null && Object.hasOwn(message, "friction"))
                    if (typeof message.friction !== "number") return "friction: number expected"
                return null
            }

            /**
             * Creates a Dynamics message from a plain object. Also converts values to their respective internal types.
             * @function fromObject
             * @memberof mirabuf.joint.Dynamics
             * @static
             * @param {Object.<string,*>} object Plain object
             * @returns {mirabuf.joint.Dynamics} Dynamics
             */
            Dynamics.fromObject = function fromObject(object) {
                if (object instanceof $root.mirabuf.joint.Dynamics) return object
                let message = new $root.mirabuf.joint.Dynamics()
                if (object.damping != null) message.damping = Number(object.damping)
                if (object.friction != null) message.friction = Number(object.friction)
                return message
            }

            /**
             * Creates a plain object from a Dynamics message. Also converts values to other types if specified.
             * @function toObject
             * @memberof mirabuf.joint.Dynamics
             * @static
             * @param {mirabuf.joint.Dynamics} message Dynamics
             * @param {$protobuf.IConversionOptions} [options] Conversion options
             * @returns {Object.<string,*>} Plain object
             */
            Dynamics.toObject = function toObject(message, options) {
                if (!options) options = {}
                let object = {}
                if (options.defaults) {
                    object.damping = 0
                    object.friction = 0
                }
                if (message.damping != null && Object.hasOwn(message, "damping"))
                    object.damping =
                        options.json && !isFinite(message.damping) ? String(message.damping) : message.damping
                if (message.friction != null && Object.hasOwn(message, "friction"))
                    object.friction =
                        options.json && !isFinite(message.friction) ? String(message.friction) : message.friction
                return object
            }

            /**
             * Converts this Dynamics to JSON.
             * @function toJSON
             * @memberof mirabuf.joint.Dynamics
             * @instance
             * @returns {Object.<string,*>} JSON object
             */
            Dynamics.prototype.toJSON = function toJSON() {
                return this.constructor.toObject(this, $protobuf.util.toJSONOptions)
            }

            /**
             * Gets the default type url for Dynamics
             * @function getTypeUrl
             * @memberof mirabuf.joint.Dynamics
             * @static
             * @param {string} [typeUrlPrefix] your custom typeUrlPrefix(default "type.googleapis.com")
             * @returns {string} The default type url
             */
            Dynamics.getTypeUrl = function getTypeUrl(typeUrlPrefix) {
                if (typeUrlPrefix === undefined) {
                    typeUrlPrefix = "type.googleapis.com"
                }
                return typeUrlPrefix + "/mirabuf.joint.Dynamics"
            }

            return Dynamics
        })()

        joint.Limits = (function () {
            /**
             * Properties of a Limits.
             * @memberof mirabuf.joint
             * @interface ILimits
             * @property {number|null} [lower] Lower Limit corresponds to default displacement
             * @property {number|null} [upper] Upper Limit is the joint extent
             * @property {number|null} [velocity] Velocity Max in m/s^2 (angular for rotational)
             * @property {number|null} [effort] Effort is the absolute force a joint can apply for a given instant - ROS has a great article on it http://wiki.ros.org/pr2_controller_manager/safety_limits
             */

            /**
             * Constructs a new Limits.
             * @memberof mirabuf.joint
             * @classdesc Limits specify the mechanical range of a given joint.
             *
             * TODO: Add units
             * @implements ILimits
             * @constructor
             * @param {mirabuf.joint.ILimits=} [properties] Properties to set
             */
            function Limits(properties) {
                if (properties)
                    for (let keys = Object.keys(properties), i = 0; i < keys.length; ++i)
                        if (properties[keys[i]] != null) this[keys[i]] = properties[keys[i]]
            }

            /**
             * Lower Limit corresponds to default displacement
             * @member {number} lower
             * @memberof mirabuf.joint.Limits
             * @instance
             */
            Limits.prototype.lower = 0

            /**
             * Upper Limit is the joint extent
             * @member {number} upper
             * @memberof mirabuf.joint.Limits
             * @instance
             */
            Limits.prototype.upper = 0

            /**
             * Velocity Max in m/s^2 (angular for rotational)
             * @member {number} velocity
             * @memberof mirabuf.joint.Limits
             * @instance
             */
            Limits.prototype.velocity = 0

            /**
             * Effort is the absolute force a joint can apply for a given instant - ROS has a great article on it http://wiki.ros.org/pr2_controller_manager/safety_limits
             * @member {number} effort
             * @memberof mirabuf.joint.Limits
             * @instance
             */
            Limits.prototype.effort = 0

            /**
             * Creates a new Limits instance using the specified properties.
             * @function create
             * @memberof mirabuf.joint.Limits
             * @static
             * @param {mirabuf.joint.ILimits=} [properties] Properties to set
             * @returns {mirabuf.joint.Limits} Limits instance
             */
            Limits.create = function create(properties) {
                return new Limits(properties)
            }

            /**
             * Encodes the specified Limits message. Does not implicitly {@link mirabuf.joint.Limits.verify|verify} messages.
             * @function encode
             * @memberof mirabuf.joint.Limits
             * @static
             * @param {mirabuf.joint.ILimits} message Limits message or plain object to encode
             * @param {$protobuf.Writer} [writer] Writer to encode to
             * @returns {$protobuf.Writer} Writer
             */
            Limits.encode = function encode(message, writer) {
                if (!writer) writer = $Writer.create()
                if (message.lower != null && Object.hasOwn(message, "lower"))
                    writer.uint32(/* id 1, wireType 5 =*/ 13).float(message.lower)
                if (message.upper != null && Object.hasOwn(message, "upper"))
                    writer.uint32(/* id 2, wireType 5 =*/ 21).float(message.upper)
                if (message.velocity != null && Object.hasOwn(message, "velocity"))
                    writer.uint32(/* id 3, wireType 5 =*/ 29).float(message.velocity)
                if (message.effort != null && Object.hasOwn(message, "effort"))
                    writer.uint32(/* id 4, wireType 5 =*/ 37).float(message.effort)
                return writer
            }

            /**
             * Encodes the specified Limits message, length delimited. Does not implicitly {@link mirabuf.joint.Limits.verify|verify} messages.
             * @function encodeDelimited
             * @memberof mirabuf.joint.Limits
             * @static
             * @param {mirabuf.joint.ILimits} message Limits message or plain object to encode
             * @param {$protobuf.Writer} [writer] Writer to encode to
             * @returns {$protobuf.Writer} Writer
             */
            Limits.encodeDelimited = function encodeDelimited(message, writer) {
                return this.encode(message, writer).ldelim()
            }

            /**
             * Decodes a Limits message from the specified reader or buffer.
             * @function decode
             * @memberof mirabuf.joint.Limits
             * @static
             * @param {$protobuf.Reader|Uint8Array} reader Reader or buffer to decode from
             * @param {number} [length] Message length if known beforehand
             * @returns {mirabuf.joint.Limits} Limits
             * @throws {Error} If the payload is not a reader or valid buffer
             * @throws {$protobuf.util.ProtocolError} If required fields are missing
             */
            Limits.decode = function decode(reader, length) {
                if (!(reader instanceof $Reader)) reader = $Reader.create(reader)
                let end = length === undefined ? reader.len : reader.pos + length,
                    message = new $root.mirabuf.joint.Limits()
                while (reader.pos < end) {
                    let tag = reader.uint32()
                    switch (tag >>> 3) {
                        case 1: {
                            message.lower = reader.float()
                            break
                        }
                        case 2: {
                            message.upper = reader.float()
                            break
                        }
                        case 3: {
                            message.velocity = reader.float()
                            break
                        }
                        case 4: {
                            message.effort = reader.float()
                            break
                        }
                        default:
                            reader.skipType(tag & 7)
                            break
                    }
                }
                return message
            }

            /**
             * Decodes a Limits message from the specified reader or buffer, length delimited.
             * @function decodeDelimited
             * @memberof mirabuf.joint.Limits
             * @static
             * @param {$protobuf.Reader|Uint8Array} reader Reader or buffer to decode from
             * @returns {mirabuf.joint.Limits} Limits
             * @throws {Error} If the payload is not a reader or valid buffer
             * @throws {$protobuf.util.ProtocolError} If required fields are missing
             */
            Limits.decodeDelimited = function decodeDelimited(reader) {
                if (!(reader instanceof $Reader)) reader = new $Reader(reader)
                return this.decode(reader, reader.uint32())
            }

            /**
             * Verifies a Limits message.
             * @function verify
             * @memberof mirabuf.joint.Limits
             * @static
             * @param {Object.<string,*>} message Plain object to verify
             * @returns {string|null} `null` if valid, otherwise the reason why it is not
             */
            Limits.verify = function verify(message) {
                if (typeof message !== "object" || message === null) return "object expected"
                if (message.lower != null && Object.hasOwn(message, "lower"))
                    if (typeof message.lower !== "number") return "lower: number expected"
                if (message.upper != null && Object.hasOwn(message, "upper"))
                    if (typeof message.upper !== "number") return "upper: number expected"
                if (message.velocity != null && Object.hasOwn(message, "velocity"))
                    if (typeof message.velocity !== "number") return "velocity: number expected"
                if (message.effort != null && Object.hasOwn(message, "effort"))
                    if (typeof message.effort !== "number") return "effort: number expected"
                return null
            }

            /**
             * Creates a Limits message from a plain object. Also converts values to their respective internal types.
             * @function fromObject
             * @memberof mirabuf.joint.Limits
             * @static
             * @param {Object.<string,*>} object Plain object
             * @returns {mirabuf.joint.Limits} Limits
             */
            Limits.fromObject = function fromObject(object) {
                if (object instanceof $root.mirabuf.joint.Limits) return object
                let message = new $root.mirabuf.joint.Limits()
                if (object.lower != null) message.lower = Number(object.lower)
                if (object.upper != null) message.upper = Number(object.upper)
                if (object.velocity != null) message.velocity = Number(object.velocity)
                if (object.effort != null) message.effort = Number(object.effort)
                return message
            }

            /**
             * Creates a plain object from a Limits message. Also converts values to other types if specified.
             * @function toObject
             * @memberof mirabuf.joint.Limits
             * @static
             * @param {mirabuf.joint.Limits} message Limits
             * @param {$protobuf.IConversionOptions} [options] Conversion options
             * @returns {Object.<string,*>} Plain object
             */
            Limits.toObject = function toObject(message, options) {
                if (!options) options = {}
                let object = {}
                if (options.defaults) {
                    object.lower = 0
                    object.upper = 0
                    object.velocity = 0
                    object.effort = 0
                }
                if (message.lower != null && Object.hasOwn(message, "lower"))
                    object.lower = options.json && !isFinite(message.lower) ? String(message.lower) : message.lower
                if (message.upper != null && Object.hasOwn(message, "upper"))
                    object.upper = options.json && !isFinite(message.upper) ? String(message.upper) : message.upper
                if (message.velocity != null && Object.hasOwn(message, "velocity"))
                    object.velocity =
                        options.json && !isFinite(message.velocity) ? String(message.velocity) : message.velocity
                if (message.effort != null && Object.hasOwn(message, "effort"))
                    object.effort = options.json && !isFinite(message.effort) ? String(message.effort) : message.effort
                return object
            }

            /**
             * Converts this Limits to JSON.
             * @function toJSON
             * @memberof mirabuf.joint.Limits
             * @instance
             * @returns {Object.<string,*>} JSON object
             */
            Limits.prototype.toJSON = function toJSON() {
                return this.constructor.toObject(this, $protobuf.util.toJSONOptions)
            }

            /**
             * Gets the default type url for Limits
             * @function getTypeUrl
             * @memberof mirabuf.joint.Limits
             * @static
             * @param {string} [typeUrlPrefix] your custom typeUrlPrefix(default "type.googleapis.com")
             * @returns {string} The default type url
             */
            Limits.getTypeUrl = function getTypeUrl(typeUrlPrefix) {
                if (typeUrlPrefix === undefined) {
                    typeUrlPrefix = "type.googleapis.com"
                }
                return typeUrlPrefix + "/mirabuf.joint.Limits"
            }

            return Limits
        })()

        joint.Safety = (function () {
            /**
             * Properties of a Safety.
             * @memberof mirabuf.joint
             * @interface ISafety
             * @property {number|null} [lowerLimit] Lower software limit
             * @property {number|null} [upperLimit] Upper Software limit
             * @property {number|null} [kPosition] Relation between position and velocity limit
             * @property {number|null} [kVelocity] Relation between effort and velocity limit
             */

            /**
             * Constructs a new Safety.
             * @memberof mirabuf.joint
             * @classdesc Safety switch configuration for a given joint.
             * Can usefully indicate a bounds issue.
             * Inspired by the URDF implementation.
             *
             * This should really just be created by the controller.
             * http://wiki.ros.org/pr2_controller_manager/safety_limits
             * @implements ISafety
             * @constructor
             * @param {mirabuf.joint.ISafety=} [properties] Properties to set
             */
            function Safety(properties) {
                if (properties)
                    for (let keys = Object.keys(properties), i = 0; i < keys.length; ++i)
                        if (properties[keys[i]] != null) this[keys[i]] = properties[keys[i]]
            }

            /**
             * Lower software limit
             * @member {number} lowerLimit
             * @memberof mirabuf.joint.Safety
             * @instance
             */
            Safety.prototype.lowerLimit = 0

            /**
             * Upper Software limit
             * @member {number} upperLimit
             * @memberof mirabuf.joint.Safety
             * @instance
             */
            Safety.prototype.upperLimit = 0

            /**
             * Relation between position and velocity limit
             * @member {number} kPosition
             * @memberof mirabuf.joint.Safety
             * @instance
             */
            Safety.prototype.kPosition = 0

            /**
             * Relation between effort and velocity limit
             * @member {number} kVelocity
             * @memberof mirabuf.joint.Safety
             * @instance
             */
            Safety.prototype.kVelocity = 0

            /**
             * Creates a new Safety instance using the specified properties.
             * @function create
             * @memberof mirabuf.joint.Safety
             * @static
             * @param {mirabuf.joint.ISafety=} [properties] Properties to set
             * @returns {mirabuf.joint.Safety} Safety instance
             */
            Safety.create = function create(properties) {
                return new Safety(properties)
            }

            /**
             * Encodes the specified Safety message. Does not implicitly {@link mirabuf.joint.Safety.verify|verify} messages.
             * @function encode
             * @memberof mirabuf.joint.Safety
             * @static
             * @param {mirabuf.joint.ISafety} message Safety message or plain object to encode
             * @param {$protobuf.Writer} [writer] Writer to encode to
             * @returns {$protobuf.Writer} Writer
             */
            Safety.encode = function encode(message, writer) {
                if (!writer) writer = $Writer.create()
                if (message.lowerLimit != null && Object.hasOwn(message, "lowerLimit"))
                    writer.uint32(/* id 1, wireType 5 =*/ 13).float(message.lowerLimit)
                if (message.upperLimit != null && Object.hasOwn(message, "upperLimit"))
                    writer.uint32(/* id 2, wireType 5 =*/ 21).float(message.upperLimit)
                if (message.kPosition != null && Object.hasOwn(message, "kPosition"))
                    writer.uint32(/* id 3, wireType 5 =*/ 29).float(message.kPosition)
                if (message.kVelocity != null && Object.hasOwn(message, "kVelocity"))
                    writer.uint32(/* id 4, wireType 5 =*/ 37).float(message.kVelocity)
                return writer
            }

            /**
             * Encodes the specified Safety message, length delimited. Does not implicitly {@link mirabuf.joint.Safety.verify|verify} messages.
             * @function encodeDelimited
             * @memberof mirabuf.joint.Safety
             * @static
             * @param {mirabuf.joint.ISafety} message Safety message or plain object to encode
             * @param {$protobuf.Writer} [writer] Writer to encode to
             * @returns {$protobuf.Writer} Writer
             */
            Safety.encodeDelimited = function encodeDelimited(message, writer) {
                return this.encode(message, writer).ldelim()
            }

            /**
             * Decodes a Safety message from the specified reader or buffer.
             * @function decode
             * @memberof mirabuf.joint.Safety
             * @static
             * @param {$protobuf.Reader|Uint8Array} reader Reader or buffer to decode from
             * @param {number} [length] Message length if known beforehand
             * @returns {mirabuf.joint.Safety} Safety
             * @throws {Error} If the payload is not a reader or valid buffer
             * @throws {$protobuf.util.ProtocolError} If required fields are missing
             */
            Safety.decode = function decode(reader, length) {
                if (!(reader instanceof $Reader)) reader = $Reader.create(reader)
                let end = length === undefined ? reader.len : reader.pos + length,
                    message = new $root.mirabuf.joint.Safety()
                while (reader.pos < end) {
                    let tag = reader.uint32()
                    switch (tag >>> 3) {
                        case 1: {
                            message.lowerLimit = reader.float()
                            break
                        }
                        case 2: {
                            message.upperLimit = reader.float()
                            break
                        }
                        case 3: {
                            message.kPosition = reader.float()
                            break
                        }
                        case 4: {
                            message.kVelocity = reader.float()
                            break
                        }
                        default:
                            reader.skipType(tag & 7)
                            break
                    }
                }
                return message
            }

            /**
             * Decodes a Safety message from the specified reader or buffer, length delimited.
             * @function decodeDelimited
             * @memberof mirabuf.joint.Safety
             * @static
             * @param {$protobuf.Reader|Uint8Array} reader Reader or buffer to decode from
             * @returns {mirabuf.joint.Safety} Safety
             * @throws {Error} If the payload is not a reader or valid buffer
             * @throws {$protobuf.util.ProtocolError} If required fields are missing
             */
            Safety.decodeDelimited = function decodeDelimited(reader) {
                if (!(reader instanceof $Reader)) reader = new $Reader(reader)
                return this.decode(reader, reader.uint32())
            }

            /**
             * Verifies a Safety message.
             * @function verify
             * @memberof mirabuf.joint.Safety
             * @static
             * @param {Object.<string,*>} message Plain object to verify
             * @returns {string|null} `null` if valid, otherwise the reason why it is not
             */
            Safety.verify = function verify(message) {
                if (typeof message !== "object" || message === null) return "object expected"
                if (message.lowerLimit != null && Object.hasOwn(message, "lowerLimit"))
                    if (typeof message.lowerLimit !== "number") return "lowerLimit: number expected"
                if (message.upperLimit != null && Object.hasOwn(message, "upperLimit"))
                    if (typeof message.upperLimit !== "number") return "upperLimit: number expected"
                if (message.kPosition != null && Object.hasOwn(message, "kPosition"))
                    if (typeof message.kPosition !== "number") return "kPosition: number expected"
                if (message.kVelocity != null && Object.hasOwn(message, "kVelocity"))
                    if (typeof message.kVelocity !== "number") return "kVelocity: number expected"
                return null
            }

            /**
             * Creates a Safety message from a plain object. Also converts values to their respective internal types.
             * @function fromObject
             * @memberof mirabuf.joint.Safety
             * @static
             * @param {Object.<string,*>} object Plain object
             * @returns {mirabuf.joint.Safety} Safety
             */
            Safety.fromObject = function fromObject(object) {
                if (object instanceof $root.mirabuf.joint.Safety) return object
                let message = new $root.mirabuf.joint.Safety()
                if (object.lowerLimit != null) message.lowerLimit = Number(object.lowerLimit)
                if (object.upperLimit != null) message.upperLimit = Number(object.upperLimit)
                if (object.kPosition != null) message.kPosition = Number(object.kPosition)
                if (object.kVelocity != null) message.kVelocity = Number(object.kVelocity)
                return message
            }

            /**
             * Creates a plain object from a Safety message. Also converts values to other types if specified.
             * @function toObject
             * @memberof mirabuf.joint.Safety
             * @static
             * @param {mirabuf.joint.Safety} message Safety
             * @param {$protobuf.IConversionOptions} [options] Conversion options
             * @returns {Object.<string,*>} Plain object
             */
            Safety.toObject = function toObject(message, options) {
                if (!options) options = {}
                let object = {}
                if (options.defaults) {
                    object.lowerLimit = 0
                    object.upperLimit = 0
                    object.kPosition = 0
                    object.kVelocity = 0
                }
                if (message.lowerLimit != null && Object.hasOwn(message, "lowerLimit"))
                    object.lowerLimit =
                        options.json && !isFinite(message.lowerLimit) ? String(message.lowerLimit) : message.lowerLimit
                if (message.upperLimit != null && Object.hasOwn(message, "upperLimit"))
                    object.upperLimit =
                        options.json && !isFinite(message.upperLimit) ? String(message.upperLimit) : message.upperLimit
                if (message.kPosition != null && Object.hasOwn(message, "kPosition"))
                    object.kPosition =
                        options.json && !isFinite(message.kPosition) ? String(message.kPosition) : message.kPosition
                if (message.kVelocity != null && Object.hasOwn(message, "kVelocity"))
                    object.kVelocity =
                        options.json && !isFinite(message.kVelocity) ? String(message.kVelocity) : message.kVelocity
                return object
            }

            /**
             * Converts this Safety to JSON.
             * @function toJSON
             * @memberof mirabuf.joint.Safety
             * @instance
             * @returns {Object.<string,*>} JSON object
             */
            Safety.prototype.toJSON = function toJSON() {
                return this.constructor.toObject(this, $protobuf.util.toJSONOptions)
            }

            /**
             * Gets the default type url for Safety
             * @function getTypeUrl
             * @memberof mirabuf.joint.Safety
             * @static
             * @param {string} [typeUrlPrefix] your custom typeUrlPrefix(default "type.googleapis.com")
             * @returns {string} The default type url
             */
            Safety.getTypeUrl = function getTypeUrl(typeUrlPrefix) {
                if (typeUrlPrefix === undefined) {
                    typeUrlPrefix = "type.googleapis.com"
                }
                return typeUrlPrefix + "/mirabuf.joint.Safety"
            }

            return Safety
        })()

        joint.DOF = (function () {
            /**
             * Properties of a DOF.
             * @memberof mirabuf.joint
             * @interface IDOF
             * @property {string|null} [name] In case you want to name this degree of freedom
             * @property {mirabuf.IVector3|null} [axis] Axis the degree of freedom is pivoting by
             * @property {mirabuf.Axis|null} [pivotDirection] Direction the axis vector is offset from - this has an incorrect naming scheme
             * @property {mirabuf.joint.IDynamics|null} [dynamics] Dynamic properties of this joint pivot
             * @property {mirabuf.joint.ILimits|null} [limits] Limits of this freedom
             * @property {number|null} [value] Current value of the DOF
             */

            /**
             * Constructs a new DOF.
             * @memberof mirabuf.joint
             * @classdesc DOF - representing the construction of a joint motion
             * @implements IDOF
             * @constructor
             * @param {mirabuf.joint.IDOF=} [properties] Properties to set
             */
            function DOF(properties) {
                if (properties)
                    for (let keys = Object.keys(properties), i = 0; i < keys.length; ++i)
                        if (properties[keys[i]] != null) this[keys[i]] = properties[keys[i]]
            }

            /**
             * In case you want to name this degree of freedom
             * @member {string} name
             * @memberof mirabuf.joint.DOF
             * @instance
             */
            DOF.prototype.name = ""

            /**
             * Axis the degree of freedom is pivoting by
             * @member {mirabuf.IVector3|null|undefined} axis
             * @memberof mirabuf.joint.DOF
             * @instance
             */
            DOF.prototype.axis = null

            /**
             * Direction the axis vector is offset from - this has an incorrect naming scheme
             * @member {mirabuf.Axis} pivotDirection
             * @memberof mirabuf.joint.DOF
             * @instance
             */
            DOF.prototype.pivotDirection = 0

            /**
             * Dynamic properties of this joint pivot
             * @member {mirabuf.joint.IDynamics|null|undefined} dynamics
             * @memberof mirabuf.joint.DOF
             * @instance
             */
            DOF.prototype.dynamics = null

            /**
             * Limits of this freedom
             * @member {mirabuf.joint.ILimits|null|undefined} limits
             * @memberof mirabuf.joint.DOF
             * @instance
             */
            DOF.prototype.limits = null

            /**
             * Current value of the DOF
             * @member {number} value
             * @memberof mirabuf.joint.DOF
             * @instance
             */
            DOF.prototype.value = 0

            /**
             * Creates a new DOF instance using the specified properties.
             * @function create
             * @memberof mirabuf.joint.DOF
             * @static
             * @param {mirabuf.joint.IDOF=} [properties] Properties to set
             * @returns {mirabuf.joint.DOF} DOF instance
             */
            DOF.create = function create(properties) {
                return new DOF(properties)
            }

            /**
             * Encodes the specified DOF message. Does not implicitly {@link mirabuf.joint.DOF.verify|verify} messages.
             * @function encode
             * @memberof mirabuf.joint.DOF
             * @static
             * @param {mirabuf.joint.IDOF} message DOF message or plain object to encode
             * @param {$protobuf.Writer} [writer] Writer to encode to
             * @returns {$protobuf.Writer} Writer
             */
            DOF.encode = function encode(message, writer) {
                if (!writer) writer = $Writer.create()
                if (message.name != null && Object.hasOwn(message, "name"))
                    writer.uint32(/* id 1, wireType 2 =*/ 10).string(message.name)
                if (message.axis != null && Object.hasOwn(message, "axis"))
                    $root.mirabuf.Vector3.encode(
                        message.axis,
                        writer.uint32(/* id 2, wireType 2 =*/ 18).fork()
                    ).ldelim()
                if (message.pivotDirection != null && Object.hasOwn(message, "pivotDirection"))
                    writer.uint32(/* id 3, wireType 0 =*/ 24).int32(message.pivotDirection)
                if (message.dynamics != null && Object.hasOwn(message, "dynamics"))
                    $root.mirabuf.joint.Dynamics.encode(
                        message.dynamics,
                        writer.uint32(/* id 4, wireType 2 =*/ 34).fork()
                    ).ldelim()
                if (message.limits != null && Object.hasOwn(message, "limits"))
                    $root.mirabuf.joint.Limits.encode(
                        message.limits,
                        writer.uint32(/* id 5, wireType 2 =*/ 42).fork()
                    ).ldelim()
                if (message.value != null && Object.hasOwn(message, "value"))
                    writer.uint32(/* id 6, wireType 5 =*/ 53).float(message.value)
                return writer
            }

            /**
             * Encodes the specified DOF message, length delimited. Does not implicitly {@link mirabuf.joint.DOF.verify|verify} messages.
             * @function encodeDelimited
             * @memberof mirabuf.joint.DOF
             * @static
             * @param {mirabuf.joint.IDOF} message DOF message or plain object to encode
             * @param {$protobuf.Writer} [writer] Writer to encode to
             * @returns {$protobuf.Writer} Writer
             */
            DOF.encodeDelimited = function encodeDelimited(message, writer) {
                return this.encode(message, writer).ldelim()
            }

            /**
             * Decodes a DOF message from the specified reader or buffer.
             * @function decode
             * @memberof mirabuf.joint.DOF
             * @static
             * @param {$protobuf.Reader|Uint8Array} reader Reader or buffer to decode from
             * @param {number} [length] Message length if known beforehand
             * @returns {mirabuf.joint.DOF} DOF
             * @throws {Error} If the payload is not a reader or valid buffer
             * @throws {$protobuf.util.ProtocolError} If required fields are missing
             */
            DOF.decode = function decode(reader, length) {
                if (!(reader instanceof $Reader)) reader = $Reader.create(reader)
                let end = length === undefined ? reader.len : reader.pos + length,
                    message = new $root.mirabuf.joint.DOF()
                while (reader.pos < end) {
                    let tag = reader.uint32()
                    switch (tag >>> 3) {
                        case 1: {
                            message.name = reader.string()
                            break
                        }
                        case 2: {
                            message.axis = $root.mirabuf.Vector3.decode(reader, reader.uint32())
                            break
                        }
                        case 3: {
                            message.pivotDirection = reader.int32()
                            break
                        }
                        case 4: {
                            message.dynamics = $root.mirabuf.joint.Dynamics.decode(reader, reader.uint32())
                            break
                        }
                        case 5: {
                            message.limits = $root.mirabuf.joint.Limits.decode(reader, reader.uint32())
                            break
                        }
                        case 6: {
                            message.value = reader.float()
                            break
                        }
                        default:
                            reader.skipType(tag & 7)
                            break
                    }
                }
                return message
            }

            /**
             * Decodes a DOF message from the specified reader or buffer, length delimited.
             * @function decodeDelimited
             * @memberof mirabuf.joint.DOF
             * @static
             * @param {$protobuf.Reader|Uint8Array} reader Reader or buffer to decode from
             * @returns {mirabuf.joint.DOF} DOF
             * @throws {Error} If the payload is not a reader or valid buffer
             * @throws {$protobuf.util.ProtocolError} If required fields are missing
             */
            DOF.decodeDelimited = function decodeDelimited(reader) {
                if (!(reader instanceof $Reader)) reader = new $Reader(reader)
                return this.decode(reader, reader.uint32())
            }

            /**
             * Verifies a DOF message.
             * @function verify
             * @memberof mirabuf.joint.DOF
             * @static
             * @param {Object.<string,*>} message Plain object to verify
             * @returns {string|null} `null` if valid, otherwise the reason why it is not
             */
            DOF.verify = function verify(message) {
                if (typeof message !== "object" || message === null) return "object expected"
                if (message.name != null && Object.hasOwn(message, "name"))
                    if (!$util.isString(message.name)) return "name: string expected"
                if (message.axis != null && Object.hasOwn(message, "axis")) {
                    let error = $root.mirabuf.Vector3.verify(message.axis)
                    if (error) return "axis." + error
                }
                if (message.pivotDirection != null && Object.hasOwn(message, "pivotDirection"))
                    switch (message.pivotDirection) {
                        default:
                            return "pivotDirection: enum value expected"
                        case 0:
                        case 1:
                        case 2:
                            break
                    }
                if (message.dynamics != null && Object.hasOwn(message, "dynamics")) {
                    let error = $root.mirabuf.joint.Dynamics.verify(message.dynamics)
                    if (error) return "dynamics." + error
                }
                if (message.limits != null && Object.hasOwn(message, "limits")) {
                    let error = $root.mirabuf.joint.Limits.verify(message.limits)
                    if (error) return "limits." + error
                }
                if (message.value != null && Object.hasOwn(message, "value"))
                    if (typeof message.value !== "number") return "value: number expected"
                return null
            }

            /**
             * Creates a DOF message from a plain object. Also converts values to their respective internal types.
             * @function fromObject
             * @memberof mirabuf.joint.DOF
             * @static
             * @param {Object.<string,*>} object Plain object
             * @returns {mirabuf.joint.DOF} DOF
             */
            DOF.fromObject = function fromObject(object) {
                if (object instanceof $root.mirabuf.joint.DOF) return object
                let message = new $root.mirabuf.joint.DOF()
                if (object.name != null) message.name = String(object.name)
                if (object.axis != null) {
                    if (typeof object.axis !== "object") throw TypeError(".mirabuf.joint.DOF.axis: object expected")
                    message.axis = $root.mirabuf.Vector3.fromObject(object.axis)
                }
                switch (object.pivotDirection) {
                    default:
                        if (typeof object.pivotDirection === "number") {
                            message.pivotDirection = object.pivotDirection
                            break
                        }
                        break
                    case "X":
                    case 0:
                        message.pivotDirection = 0
                        break
                    case "Y":
                    case 1:
                        message.pivotDirection = 1
                        break
                    case "Z":
                    case 2:
                        message.pivotDirection = 2
                        break
                }
                if (object.dynamics != null) {
                    if (typeof object.dynamics !== "object")
                        throw TypeError(".mirabuf.joint.DOF.dynamics: object expected")
                    message.dynamics = $root.mirabuf.joint.Dynamics.fromObject(object.dynamics)
                }
                if (object.limits != null) {
                    if (typeof object.limits !== "object") throw TypeError(".mirabuf.joint.DOF.limits: object expected")
                    message.limits = $root.mirabuf.joint.Limits.fromObject(object.limits)
                }
                if (object.value != null) message.value = Number(object.value)
                return message
            }

            /**
             * Creates a plain object from a DOF message. Also converts values to other types if specified.
             * @function toObject
             * @memberof mirabuf.joint.DOF
             * @static
             * @param {mirabuf.joint.DOF} message DOF
             * @param {$protobuf.IConversionOptions} [options] Conversion options
             * @returns {Object.<string,*>} Plain object
             */
            DOF.toObject = function toObject(message, options) {
                if (!options) options = {}
                let object = {}
                if (options.defaults) {
                    object.name = ""
                    object.axis = null
                    object.pivotDirection = options.enums === String ? "X" : 0
                    object.dynamics = null
                    object.limits = null
                    object.value = 0
                }
                if (message.name != null && Object.hasOwn(message, "name")) object.name = message.name
                if (message.axis != null && Object.hasOwn(message, "axis"))
                    object.axis = $root.mirabuf.Vector3.toObject(message.axis, options)
                if (message.pivotDirection != null && Object.hasOwn(message, "pivotDirection"))
                    object.pivotDirection =
                        options.enums === String
                            ? $root.mirabuf.Axis[message.pivotDirection] === undefined
                                ? message.pivotDirection
                                : $root.mirabuf.Axis[message.pivotDirection]
                            : message.pivotDirection
                if (message.dynamics != null && Object.hasOwn(message, "dynamics"))
                    object.dynamics = $root.mirabuf.joint.Dynamics.toObject(message.dynamics, options)
                if (message.limits != null && Object.hasOwn(message, "limits"))
                    object.limits = $root.mirabuf.joint.Limits.toObject(message.limits, options)
                if (message.value != null && Object.hasOwn(message, "value"))
                    object.value = options.json && !isFinite(message.value) ? String(message.value) : message.value
                return object
            }

            /**
             * Converts this DOF to JSON.
             * @function toJSON
             * @memberof mirabuf.joint.DOF
             * @instance
             * @returns {Object.<string,*>} JSON object
             */
            DOF.prototype.toJSON = function toJSON() {
                return this.constructor.toObject(this, $protobuf.util.toJSONOptions)
            }

            /**
             * Gets the default type url for DOF
             * @function getTypeUrl
             * @memberof mirabuf.joint.DOF
             * @static
             * @param {string} [typeUrlPrefix] your custom typeUrlPrefix(default "type.googleapis.com")
             * @returns {string} The default type url
             */
            DOF.getTypeUrl = function getTypeUrl(typeUrlPrefix) {
                if (typeUrlPrefix === undefined) {
                    typeUrlPrefix = "type.googleapis.com"
                }
                return typeUrlPrefix + "/mirabuf.joint.DOF"
            }

            return DOF
        })()

        joint.CustomJoint = (function () {
            /**
             * Properties of a CustomJoint.
             * @memberof mirabuf.joint
             * @interface ICustomJoint
             * @property {Array.<mirabuf.joint.IDOF>|null} [dofs] A list of degrees of freedom that the joint can contain
             */

            /**
             * Constructs a new CustomJoint.
             * @memberof mirabuf.joint
             * @classdesc CustomJoint is a joint with N degrees of freedom specified.
             * There should be input validation to handle max freedom case.
             * @implements ICustomJoint
             * @constructor
             * @param {mirabuf.joint.ICustomJoint=} [properties] Properties to set
             */
            function CustomJoint(properties) {
                this.dofs = []
                if (properties)
                    for (let keys = Object.keys(properties), i = 0; i < keys.length; ++i)
                        if (properties[keys[i]] != null) this[keys[i]] = properties[keys[i]]
            }

            /**
             * A list of degrees of freedom that the joint can contain
             * @member {Array.<mirabuf.joint.IDOF>} dofs
             * @memberof mirabuf.joint.CustomJoint
             * @instance
             */
            CustomJoint.prototype.dofs = $util.emptyArray

            /**
             * Creates a new CustomJoint instance using the specified properties.
             * @function create
             * @memberof mirabuf.joint.CustomJoint
             * @static
             * @param {mirabuf.joint.ICustomJoint=} [properties] Properties to set
             * @returns {mirabuf.joint.CustomJoint} CustomJoint instance
             */
            CustomJoint.create = function create(properties) {
                return new CustomJoint(properties)
            }

            /**
             * Encodes the specified CustomJoint message. Does not implicitly {@link mirabuf.joint.CustomJoint.verify|verify} messages.
             * @function encode
             * @memberof mirabuf.joint.CustomJoint
             * @static
             * @param {mirabuf.joint.ICustomJoint} message CustomJoint message or plain object to encode
             * @param {$protobuf.Writer} [writer] Writer to encode to
             * @returns {$protobuf.Writer} Writer
             */
            CustomJoint.encode = function encode(message, writer) {
                if (!writer) writer = $Writer.create()
                if (message.dofs != null && message.dofs.length)
                    for (let i = 0; i < message.dofs.length; ++i)
                        $root.mirabuf.joint.DOF.encode(
                            message.dofs[i],
                            writer.uint32(/* id 1, wireType 2 =*/ 10).fork()
                        ).ldelim()
                return writer
            }

            /**
             * Encodes the specified CustomJoint message, length delimited. Does not implicitly {@link mirabuf.joint.CustomJoint.verify|verify} messages.
             * @function encodeDelimited
             * @memberof mirabuf.joint.CustomJoint
             * @static
             * @param {mirabuf.joint.ICustomJoint} message CustomJoint message or plain object to encode
             * @param {$protobuf.Writer} [writer] Writer to encode to
             * @returns {$protobuf.Writer} Writer
             */
            CustomJoint.encodeDelimited = function encodeDelimited(message, writer) {
                return this.encode(message, writer).ldelim()
            }

            /**
             * Decodes a CustomJoint message from the specified reader or buffer.
             * @function decode
             * @memberof mirabuf.joint.CustomJoint
             * @static
             * @param {$protobuf.Reader|Uint8Array} reader Reader or buffer to decode from
             * @param {number} [length] Message length if known beforehand
             * @returns {mirabuf.joint.CustomJoint} CustomJoint
             * @throws {Error} If the payload is not a reader or valid buffer
             * @throws {$protobuf.util.ProtocolError} If required fields are missing
             */
            CustomJoint.decode = function decode(reader, length) {
                if (!(reader instanceof $Reader)) reader = $Reader.create(reader)
                let end = length === undefined ? reader.len : reader.pos + length,
                    message = new $root.mirabuf.joint.CustomJoint()
                while (reader.pos < end) {
                    let tag = reader.uint32()
                    switch (tag >>> 3) {
                        case 1: {
                            if (!(message.dofs && message.dofs.length)) message.dofs = []
                            message.dofs.push($root.mirabuf.joint.DOF.decode(reader, reader.uint32()))
                            break
                        }
                        default:
                            reader.skipType(tag & 7)
                            break
                    }
                }
                return message
            }

            /**
             * Decodes a CustomJoint message from the specified reader or buffer, length delimited.
             * @function decodeDelimited
             * @memberof mirabuf.joint.CustomJoint
             * @static
             * @param {$protobuf.Reader|Uint8Array} reader Reader or buffer to decode from
             * @returns {mirabuf.joint.CustomJoint} CustomJoint
             * @throws {Error} If the payload is not a reader or valid buffer
             * @throws {$protobuf.util.ProtocolError} If required fields are missing
             */
            CustomJoint.decodeDelimited = function decodeDelimited(reader) {
                if (!(reader instanceof $Reader)) reader = new $Reader(reader)
                return this.decode(reader, reader.uint32())
            }

            /**
             * Verifies a CustomJoint message.
             * @function verify
             * @memberof mirabuf.joint.CustomJoint
             * @static
             * @param {Object.<string,*>} message Plain object to verify
             * @returns {string|null} `null` if valid, otherwise the reason why it is not
             */
            CustomJoint.verify = function verify(message) {
                if (typeof message !== "object" || message === null) return "object expected"
                if (message.dofs != null && Object.hasOwn(message, "dofs")) {
                    if (!Array.isArray(message.dofs)) return "dofs: array expected"
                    for (let i = 0; i < message.dofs.length; ++i) {
                        let error = $root.mirabuf.joint.DOF.verify(message.dofs[i])
                        if (error) return "dofs." + error
                    }
                }
                return null
            }

            /**
             * Creates a CustomJoint message from a plain object. Also converts values to their respective internal types.
             * @function fromObject
             * @memberof mirabuf.joint.CustomJoint
             * @static
             * @param {Object.<string,*>} object Plain object
             * @returns {mirabuf.joint.CustomJoint} CustomJoint
             */
            CustomJoint.fromObject = function fromObject(object) {
                if (object instanceof $root.mirabuf.joint.CustomJoint) return object
                let message = new $root.mirabuf.joint.CustomJoint()
                if (object.dofs) {
                    if (!Array.isArray(object.dofs)) throw TypeError(".mirabuf.joint.CustomJoint.dofs: array expected")
                    message.dofs = []
                    for (let i = 0; i < object.dofs.length; ++i) {
                        if (typeof object.dofs[i] !== "object")
                            throw TypeError(".mirabuf.joint.CustomJoint.dofs: object expected")
                        message.dofs[i] = $root.mirabuf.joint.DOF.fromObject(object.dofs[i])
                    }
                }
                return message
            }

            /**
             * Creates a plain object from a CustomJoint message. Also converts values to other types if specified.
             * @function toObject
             * @memberof mirabuf.joint.CustomJoint
             * @static
             * @param {mirabuf.joint.CustomJoint} message CustomJoint
             * @param {$protobuf.IConversionOptions} [options] Conversion options
             * @returns {Object.<string,*>} Plain object
             */
            CustomJoint.toObject = function toObject(message, options) {
                if (!options) options = {}
                let object = {}
                if (options.arrays || options.defaults) object.dofs = []
                if (message.dofs && message.dofs.length) {
                    object.dofs = []
                    for (let j = 0; j < message.dofs.length; ++j)
                        object.dofs[j] = $root.mirabuf.joint.DOF.toObject(message.dofs[j], options)
                }
                return object
            }

            /**
             * Converts this CustomJoint to JSON.
             * @function toJSON
             * @memberof mirabuf.joint.CustomJoint
             * @instance
             * @returns {Object.<string,*>} JSON object
             */
            CustomJoint.prototype.toJSON = function toJSON() {
                return this.constructor.toObject(this, $protobuf.util.toJSONOptions)
            }

            /**
             * Gets the default type url for CustomJoint
             * @function getTypeUrl
             * @memberof mirabuf.joint.CustomJoint
             * @static
             * @param {string} [typeUrlPrefix] your custom typeUrlPrefix(default "type.googleapis.com")
             * @returns {string} The default type url
             */
            CustomJoint.getTypeUrl = function getTypeUrl(typeUrlPrefix) {
                if (typeUrlPrefix === undefined) {
                    typeUrlPrefix = "type.googleapis.com"
                }
                return typeUrlPrefix + "/mirabuf.joint.CustomJoint"
            }

            return CustomJoint
        })()

        joint.RotationalJoint = (function () {
            /**
             * Properties of a RotationalJoint.
             * @memberof mirabuf.joint
             * @interface IRotationalJoint
             * @property {mirabuf.joint.IDOF|null} [rotationalFreedom] RotationalJoint rotationalFreedom
             */

            /**
             * Constructs a new RotationalJoint.
             * @memberof mirabuf.joint
             * @classdesc RotationalJoint describes a joint with rotational translation.
             * This is the exact same as prismatic for now.
             * @implements IRotationalJoint
             * @constructor
             * @param {mirabuf.joint.IRotationalJoint=} [properties] Properties to set
             */
            function RotationalJoint(properties) {
                if (properties)
                    for (let keys = Object.keys(properties), i = 0; i < keys.length; ++i)
                        if (properties[keys[i]] != null) this[keys[i]] = properties[keys[i]]
            }

            /**
             * RotationalJoint rotationalFreedom.
             * @member {mirabuf.joint.IDOF|null|undefined} rotationalFreedom
             * @memberof mirabuf.joint.RotationalJoint
             * @instance
             */
            RotationalJoint.prototype.rotationalFreedom = null

            /**
             * Creates a new RotationalJoint instance using the specified properties.
             * @function create
             * @memberof mirabuf.joint.RotationalJoint
             * @static
             * @param {mirabuf.joint.IRotationalJoint=} [properties] Properties to set
             * @returns {mirabuf.joint.RotationalJoint} RotationalJoint instance
             */
            RotationalJoint.create = function create(properties) {
                return new RotationalJoint(properties)
            }

            /**
             * Encodes the specified RotationalJoint message. Does not implicitly {@link mirabuf.joint.RotationalJoint.verify|verify} messages.
             * @function encode
             * @memberof mirabuf.joint.RotationalJoint
             * @static
             * @param {mirabuf.joint.IRotationalJoint} message RotationalJoint message or plain object to encode
             * @param {$protobuf.Writer} [writer] Writer to encode to
             * @returns {$protobuf.Writer} Writer
             */
            RotationalJoint.encode = function encode(message, writer) {
                if (!writer) writer = $Writer.create()
                if (message.rotationalFreedom != null && Object.hasOwn(message, "rotationalFreedom"))
                    $root.mirabuf.joint.DOF.encode(
                        message.rotationalFreedom,
                        writer.uint32(/* id 1, wireType 2 =*/ 10).fork()
                    ).ldelim()
                return writer
            }

            /**
             * Encodes the specified RotationalJoint message, length delimited. Does not implicitly {@link mirabuf.joint.RotationalJoint.verify|verify} messages.
             * @function encodeDelimited
             * @memberof mirabuf.joint.RotationalJoint
             * @static
             * @param {mirabuf.joint.IRotationalJoint} message RotationalJoint message or plain object to encode
             * @param {$protobuf.Writer} [writer] Writer to encode to
             * @returns {$protobuf.Writer} Writer
             */
            RotationalJoint.encodeDelimited = function encodeDelimited(message, writer) {
                return this.encode(message, writer).ldelim()
            }

            /**
             * Decodes a RotationalJoint message from the specified reader or buffer.
             * @function decode
             * @memberof mirabuf.joint.RotationalJoint
             * @static
             * @param {$protobuf.Reader|Uint8Array} reader Reader or buffer to decode from
             * @param {number} [length] Message length if known beforehand
             * @returns {mirabuf.joint.RotationalJoint} RotationalJoint
             * @throws {Error} If the payload is not a reader or valid buffer
             * @throws {$protobuf.util.ProtocolError} If required fields are missing
             */
            RotationalJoint.decode = function decode(reader, length) {
                if (!(reader instanceof $Reader)) reader = $Reader.create(reader)
                let end = length === undefined ? reader.len : reader.pos + length,
                    message = new $root.mirabuf.joint.RotationalJoint()
                while (reader.pos < end) {
                    let tag = reader.uint32()
                    switch (tag >>> 3) {
                        case 1: {
                            message.rotationalFreedom = $root.mirabuf.joint.DOF.decode(reader, reader.uint32())
                            break
                        }
                        default:
                            reader.skipType(tag & 7)
                            break
                    }
                }
                return message
            }

            /**
             * Decodes a RotationalJoint message from the specified reader or buffer, length delimited.
             * @function decodeDelimited
             * @memberof mirabuf.joint.RotationalJoint
             * @static
             * @param {$protobuf.Reader|Uint8Array} reader Reader or buffer to decode from
             * @returns {mirabuf.joint.RotationalJoint} RotationalJoint
             * @throws {Error} If the payload is not a reader or valid buffer
             * @throws {$protobuf.util.ProtocolError} If required fields are missing
             */
            RotationalJoint.decodeDelimited = function decodeDelimited(reader) {
                if (!(reader instanceof $Reader)) reader = new $Reader(reader)
                return this.decode(reader, reader.uint32())
            }

            /**
             * Verifies a RotationalJoint message.
             * @function verify
             * @memberof mirabuf.joint.RotationalJoint
             * @static
             * @param {Object.<string,*>} message Plain object to verify
             * @returns {string|null} `null` if valid, otherwise the reason why it is not
             */
            RotationalJoint.verify = function verify(message) {
                if (typeof message !== "object" || message === null) return "object expected"
                if (message.rotationalFreedom != null && Object.hasOwn(message, "rotationalFreedom")) {
                    let error = $root.mirabuf.joint.DOF.verify(message.rotationalFreedom)
                    if (error) return "rotationalFreedom." + error
                }
                return null
            }

            /**
             * Creates a RotationalJoint message from a plain object. Also converts values to their respective internal types.
             * @function fromObject
             * @memberof mirabuf.joint.RotationalJoint
             * @static
             * @param {Object.<string,*>} object Plain object
             * @returns {mirabuf.joint.RotationalJoint} RotationalJoint
             */
            RotationalJoint.fromObject = function fromObject(object) {
                if (object instanceof $root.mirabuf.joint.RotationalJoint) return object
                let message = new $root.mirabuf.joint.RotationalJoint()
                if (object.rotationalFreedom != null) {
                    if (typeof object.rotationalFreedom !== "object")
                        throw TypeError(".mirabuf.joint.RotationalJoint.rotationalFreedom: object expected")
                    message.rotationalFreedom = $root.mirabuf.joint.DOF.fromObject(object.rotationalFreedom)
                }
                return message
            }

            /**
             * Creates a plain object from a RotationalJoint message. Also converts values to other types if specified.
             * @function toObject
             * @memberof mirabuf.joint.RotationalJoint
             * @static
             * @param {mirabuf.joint.RotationalJoint} message RotationalJoint
             * @param {$protobuf.IConversionOptions} [options] Conversion options
             * @returns {Object.<string,*>} Plain object
             */
            RotationalJoint.toObject = function toObject(message, options) {
                if (!options) options = {}
                let object = {}
                if (options.defaults) object.rotationalFreedom = null
                if (message.rotationalFreedom != null && Object.hasOwn(message, "rotationalFreedom"))
                    object.rotationalFreedom = $root.mirabuf.joint.DOF.toObject(message.rotationalFreedom, options)
                return object
            }

            /**
             * Converts this RotationalJoint to JSON.
             * @function toJSON
             * @memberof mirabuf.joint.RotationalJoint
             * @instance
             * @returns {Object.<string,*>} JSON object
             */
            RotationalJoint.prototype.toJSON = function toJSON() {
                return this.constructor.toObject(this, $protobuf.util.toJSONOptions)
            }

            /**
             * Gets the default type url for RotationalJoint
             * @function getTypeUrl
             * @memberof mirabuf.joint.RotationalJoint
             * @static
             * @param {string} [typeUrlPrefix] your custom typeUrlPrefix(default "type.googleapis.com")
             * @returns {string} The default type url
             */
            RotationalJoint.getTypeUrl = function getTypeUrl(typeUrlPrefix) {
                if (typeUrlPrefix === undefined) {
                    typeUrlPrefix = "type.googleapis.com"
                }
                return typeUrlPrefix + "/mirabuf.joint.RotationalJoint"
            }

            return RotationalJoint
        })()

        joint.BallJoint = (function () {
            /**
             * Properties of a BallJoint.
             * @memberof mirabuf.joint
             * @interface IBallJoint
             * @property {mirabuf.joint.IDOF|null} [yaw] BallJoint yaw
             * @property {mirabuf.joint.IDOF|null} [pitch] BallJoint pitch
             * @property {mirabuf.joint.IDOF|null} [rotation] BallJoint rotation
             */

            /**
             * Constructs a new BallJoint.
             * @memberof mirabuf.joint
             * @classdesc Represents a BallJoint.
             * @implements IBallJoint
             * @constructor
             * @param {mirabuf.joint.IBallJoint=} [properties] Properties to set
             */
            function BallJoint(properties) {
                if (properties)
                    for (let keys = Object.keys(properties), i = 0; i < keys.length; ++i)
                        if (properties[keys[i]] != null) this[keys[i]] = properties[keys[i]]
            }

            /**
             * BallJoint yaw.
             * @member {mirabuf.joint.IDOF|null|undefined} yaw
             * @memberof mirabuf.joint.BallJoint
             * @instance
             */
            BallJoint.prototype.yaw = null

            /**
             * BallJoint pitch.
             * @member {mirabuf.joint.IDOF|null|undefined} pitch
             * @memberof mirabuf.joint.BallJoint
             * @instance
             */
            BallJoint.prototype.pitch = null

            /**
             * BallJoint rotation.
             * @member {mirabuf.joint.IDOF|null|undefined} rotation
             * @memberof mirabuf.joint.BallJoint
             * @instance
             */
            BallJoint.prototype.rotation = null

            /**
             * Creates a new BallJoint instance using the specified properties.
             * @function create
             * @memberof mirabuf.joint.BallJoint
             * @static
             * @param {mirabuf.joint.IBallJoint=} [properties] Properties to set
             * @returns {mirabuf.joint.BallJoint} BallJoint instance
             */
            BallJoint.create = function create(properties) {
                return new BallJoint(properties)
            }

            /**
             * Encodes the specified BallJoint message. Does not implicitly {@link mirabuf.joint.BallJoint.verify|verify} messages.
             * @function encode
             * @memberof mirabuf.joint.BallJoint
             * @static
             * @param {mirabuf.joint.IBallJoint} message BallJoint message or plain object to encode
             * @param {$protobuf.Writer} [writer] Writer to encode to
             * @returns {$protobuf.Writer} Writer
             */
            BallJoint.encode = function encode(message, writer) {
                if (!writer) writer = $Writer.create()
                if (message.yaw != null && Object.hasOwn(message, "yaw"))
                    $root.mirabuf.joint.DOF.encode(
                        message.yaw,
                        writer.uint32(/* id 1, wireType 2 =*/ 10).fork()
                    ).ldelim()
                if (message.pitch != null && Object.hasOwn(message, "pitch"))
                    $root.mirabuf.joint.DOF.encode(
                        message.pitch,
                        writer.uint32(/* id 2, wireType 2 =*/ 18).fork()
                    ).ldelim()
                if (message.rotation != null && Object.hasOwn(message, "rotation"))
                    $root.mirabuf.joint.DOF.encode(
                        message.rotation,
                        writer.uint32(/* id 3, wireType 2 =*/ 26).fork()
                    ).ldelim()
                return writer
            }

            /**
             * Encodes the specified BallJoint message, length delimited. Does not implicitly {@link mirabuf.joint.BallJoint.verify|verify} messages.
             * @function encodeDelimited
             * @memberof mirabuf.joint.BallJoint
             * @static
             * @param {mirabuf.joint.IBallJoint} message BallJoint message or plain object to encode
             * @param {$protobuf.Writer} [writer] Writer to encode to
             * @returns {$protobuf.Writer} Writer
             */
            BallJoint.encodeDelimited = function encodeDelimited(message, writer) {
                return this.encode(message, writer).ldelim()
            }

            /**
             * Decodes a BallJoint message from the specified reader or buffer.
             * @function decode
             * @memberof mirabuf.joint.BallJoint
             * @static
             * @param {$protobuf.Reader|Uint8Array} reader Reader or buffer to decode from
             * @param {number} [length] Message length if known beforehand
             * @returns {mirabuf.joint.BallJoint} BallJoint
             * @throws {Error} If the payload is not a reader or valid buffer
             * @throws {$protobuf.util.ProtocolError} If required fields are missing
             */
            BallJoint.decode = function decode(reader, length) {
                if (!(reader instanceof $Reader)) reader = $Reader.create(reader)
                let end = length === undefined ? reader.len : reader.pos + length,
                    message = new $root.mirabuf.joint.BallJoint()
                while (reader.pos < end) {
                    let tag = reader.uint32()
                    switch (tag >>> 3) {
                        case 1: {
                            message.yaw = $root.mirabuf.joint.DOF.decode(reader, reader.uint32())
                            break
                        }
                        case 2: {
                            message.pitch = $root.mirabuf.joint.DOF.decode(reader, reader.uint32())
                            break
                        }
                        case 3: {
                            message.rotation = $root.mirabuf.joint.DOF.decode(reader, reader.uint32())
                            break
                        }
                        default:
                            reader.skipType(tag & 7)
                            break
                    }
                }
                return message
            }

            /**
             * Decodes a BallJoint message from the specified reader or buffer, length delimited.
             * @function decodeDelimited
             * @memberof mirabuf.joint.BallJoint
             * @static
             * @param {$protobuf.Reader|Uint8Array} reader Reader or buffer to decode from
             * @returns {mirabuf.joint.BallJoint} BallJoint
             * @throws {Error} If the payload is not a reader or valid buffer
             * @throws {$protobuf.util.ProtocolError} If required fields are missing
             */
            BallJoint.decodeDelimited = function decodeDelimited(reader) {
                if (!(reader instanceof $Reader)) reader = new $Reader(reader)
                return this.decode(reader, reader.uint32())
            }

            /**
             * Verifies a BallJoint message.
             * @function verify
             * @memberof mirabuf.joint.BallJoint
             * @static
             * @param {Object.<string,*>} message Plain object to verify
             * @returns {string|null} `null` if valid, otherwise the reason why it is not
             */
            BallJoint.verify = function verify(message) {
                if (typeof message !== "object" || message === null) return "object expected"
                if (message.yaw != null && Object.hasOwn(message, "yaw")) {
                    let error = $root.mirabuf.joint.DOF.verify(message.yaw)
                    if (error) return "yaw." + error
                }
                if (message.pitch != null && Object.hasOwn(message, "pitch")) {
                    let error = $root.mirabuf.joint.DOF.verify(message.pitch)
                    if (error) return "pitch." + error
                }
                if (message.rotation != null && Object.hasOwn(message, "rotation")) {
                    let error = $root.mirabuf.joint.DOF.verify(message.rotation)
                    if (error) return "rotation." + error
                }
                return null
            }

            /**
             * Creates a BallJoint message from a plain object. Also converts values to their respective internal types.
             * @function fromObject
             * @memberof mirabuf.joint.BallJoint
             * @static
             * @param {Object.<string,*>} object Plain object
             * @returns {mirabuf.joint.BallJoint} BallJoint
             */
            BallJoint.fromObject = function fromObject(object) {
                if (object instanceof $root.mirabuf.joint.BallJoint) return object
                let message = new $root.mirabuf.joint.BallJoint()
                if (object.yaw != null) {
                    if (typeof object.yaw !== "object") throw TypeError(".mirabuf.joint.BallJoint.yaw: object expected")
                    message.yaw = $root.mirabuf.joint.DOF.fromObject(object.yaw)
                }
                if (object.pitch != null) {
                    if (typeof object.pitch !== "object")
                        throw TypeError(".mirabuf.joint.BallJoint.pitch: object expected")
                    message.pitch = $root.mirabuf.joint.DOF.fromObject(object.pitch)
                }
                if (object.rotation != null) {
                    if (typeof object.rotation !== "object")
                        throw TypeError(".mirabuf.joint.BallJoint.rotation: object expected")
                    message.rotation = $root.mirabuf.joint.DOF.fromObject(object.rotation)
                }
                return message
            }

            /**
             * Creates a plain object from a BallJoint message. Also converts values to other types if specified.
             * @function toObject
             * @memberof mirabuf.joint.BallJoint
             * @static
             * @param {mirabuf.joint.BallJoint} message BallJoint
             * @param {$protobuf.IConversionOptions} [options] Conversion options
             * @returns {Object.<string,*>} Plain object
             */
            BallJoint.toObject = function toObject(message, options) {
                if (!options) options = {}
                let object = {}
                if (options.defaults) {
                    object.yaw = null
                    object.pitch = null
                    object.rotation = null
                }
                if (message.yaw != null && Object.hasOwn(message, "yaw"))
                    object.yaw = $root.mirabuf.joint.DOF.toObject(message.yaw, options)
                if (message.pitch != null && Object.hasOwn(message, "pitch"))
                    object.pitch = $root.mirabuf.joint.DOF.toObject(message.pitch, options)
                if (message.rotation != null && Object.hasOwn(message, "rotation"))
                    object.rotation = $root.mirabuf.joint.DOF.toObject(message.rotation, options)
                return object
            }

            /**
             * Converts this BallJoint to JSON.
             * @function toJSON
             * @memberof mirabuf.joint.BallJoint
             * @instance
             * @returns {Object.<string,*>} JSON object
             */
            BallJoint.prototype.toJSON = function toJSON() {
                return this.constructor.toObject(this, $protobuf.util.toJSONOptions)
            }

            /**
             * Gets the default type url for BallJoint
             * @function getTypeUrl
             * @memberof mirabuf.joint.BallJoint
             * @static
             * @param {string} [typeUrlPrefix] your custom typeUrlPrefix(default "type.googleapis.com")
             * @returns {string} The default type url
             */
            BallJoint.getTypeUrl = function getTypeUrl(typeUrlPrefix) {
                if (typeUrlPrefix === undefined) {
                    typeUrlPrefix = "type.googleapis.com"
                }
                return typeUrlPrefix + "/mirabuf.joint.BallJoint"
            }

            return BallJoint
        })()

        joint.PrismaticJoint = (function () {
            /**
             * Properties of a PrismaticJoint.
             * @memberof mirabuf.joint
             * @interface IPrismaticJoint
             * @property {mirabuf.joint.IDOF|null} [prismaticFreedom] PrismaticJoint prismaticFreedom
             */

            /**
             * Constructs a new PrismaticJoint.
             * @memberof mirabuf.joint
             * @classdesc Prismatic Joint describes a motion that translates the position in a single axis
             * @implements IPrismaticJoint
             * @constructor
             * @param {mirabuf.joint.IPrismaticJoint=} [properties] Properties to set
             */
            function PrismaticJoint(properties) {
                if (properties)
                    for (let keys = Object.keys(properties), i = 0; i < keys.length; ++i)
                        if (properties[keys[i]] != null) this[keys[i]] = properties[keys[i]]
            }

            /**
             * PrismaticJoint prismaticFreedom.
             * @member {mirabuf.joint.IDOF|null|undefined} prismaticFreedom
             * @memberof mirabuf.joint.PrismaticJoint
             * @instance
             */
            PrismaticJoint.prototype.prismaticFreedom = null

            /**
             * Creates a new PrismaticJoint instance using the specified properties.
             * @function create
             * @memberof mirabuf.joint.PrismaticJoint
             * @static
             * @param {mirabuf.joint.IPrismaticJoint=} [properties] Properties to set
             * @returns {mirabuf.joint.PrismaticJoint} PrismaticJoint instance
             */
            PrismaticJoint.create = function create(properties) {
                return new PrismaticJoint(properties)
            }

            /**
             * Encodes the specified PrismaticJoint message. Does not implicitly {@link mirabuf.joint.PrismaticJoint.verify|verify} messages.
             * @function encode
             * @memberof mirabuf.joint.PrismaticJoint
             * @static
             * @param {mirabuf.joint.IPrismaticJoint} message PrismaticJoint message or plain object to encode
             * @param {$protobuf.Writer} [writer] Writer to encode to
             * @returns {$protobuf.Writer} Writer
             */
            PrismaticJoint.encode = function encode(message, writer) {
                if (!writer) writer = $Writer.create()
                if (message.prismaticFreedom != null && Object.hasOwn(message, "prismaticFreedom"))
                    $root.mirabuf.joint.DOF.encode(
                        message.prismaticFreedom,
                        writer.uint32(/* id 1, wireType 2 =*/ 10).fork()
                    ).ldelim()
                return writer
            }

            /**
             * Encodes the specified PrismaticJoint message, length delimited. Does not implicitly {@link mirabuf.joint.PrismaticJoint.verify|verify} messages.
             * @function encodeDelimited
             * @memberof mirabuf.joint.PrismaticJoint
             * @static
             * @param {mirabuf.joint.IPrismaticJoint} message PrismaticJoint message or plain object to encode
             * @param {$protobuf.Writer} [writer] Writer to encode to
             * @returns {$protobuf.Writer} Writer
             */
            PrismaticJoint.encodeDelimited = function encodeDelimited(message, writer) {
                return this.encode(message, writer).ldelim()
            }

            /**
             * Decodes a PrismaticJoint message from the specified reader or buffer.
             * @function decode
             * @memberof mirabuf.joint.PrismaticJoint
             * @static
             * @param {$protobuf.Reader|Uint8Array} reader Reader or buffer to decode from
             * @param {number} [length] Message length if known beforehand
             * @returns {mirabuf.joint.PrismaticJoint} PrismaticJoint
             * @throws {Error} If the payload is not a reader or valid buffer
             * @throws {$protobuf.util.ProtocolError} If required fields are missing
             */
            PrismaticJoint.decode = function decode(reader, length) {
                if (!(reader instanceof $Reader)) reader = $Reader.create(reader)
                let end = length === undefined ? reader.len : reader.pos + length,
                    message = new $root.mirabuf.joint.PrismaticJoint()
                while (reader.pos < end) {
                    let tag = reader.uint32()
                    switch (tag >>> 3) {
                        case 1: {
                            message.prismaticFreedom = $root.mirabuf.joint.DOF.decode(reader, reader.uint32())
                            break
                        }
                        default:
                            reader.skipType(tag & 7)
                            break
                    }
                }
                return message
            }

            /**
             * Decodes a PrismaticJoint message from the specified reader or buffer, length delimited.
             * @function decodeDelimited
             * @memberof mirabuf.joint.PrismaticJoint
             * @static
             * @param {$protobuf.Reader|Uint8Array} reader Reader or buffer to decode from
             * @returns {mirabuf.joint.PrismaticJoint} PrismaticJoint
             * @throws {Error} If the payload is not a reader or valid buffer
             * @throws {$protobuf.util.ProtocolError} If required fields are missing
             */
            PrismaticJoint.decodeDelimited = function decodeDelimited(reader) {
                if (!(reader instanceof $Reader)) reader = new $Reader(reader)
                return this.decode(reader, reader.uint32())
            }

            /**
             * Verifies a PrismaticJoint message.
             * @function verify
             * @memberof mirabuf.joint.PrismaticJoint
             * @static
             * @param {Object.<string,*>} message Plain object to verify
             * @returns {string|null} `null` if valid, otherwise the reason why it is not
             */
            PrismaticJoint.verify = function verify(message) {
                if (typeof message !== "object" || message === null) return "object expected"
                if (message.prismaticFreedom != null && Object.hasOwn(message, "prismaticFreedom")) {
                    let error = $root.mirabuf.joint.DOF.verify(message.prismaticFreedom)
                    if (error) return "prismaticFreedom." + error
                }
                return null
            }

            /**
             * Creates a PrismaticJoint message from a plain object. Also converts values to their respective internal types.
             * @function fromObject
             * @memberof mirabuf.joint.PrismaticJoint
             * @static
             * @param {Object.<string,*>} object Plain object
             * @returns {mirabuf.joint.PrismaticJoint} PrismaticJoint
             */
            PrismaticJoint.fromObject = function fromObject(object) {
                if (object instanceof $root.mirabuf.joint.PrismaticJoint) return object
                let message = new $root.mirabuf.joint.PrismaticJoint()
                if (object.prismaticFreedom != null) {
                    if (typeof object.prismaticFreedom !== "object")
                        throw TypeError(".mirabuf.joint.PrismaticJoint.prismaticFreedom: object expected")
                    message.prismaticFreedom = $root.mirabuf.joint.DOF.fromObject(object.prismaticFreedom)
                }
                return message
            }

            /**
             * Creates a plain object from a PrismaticJoint message. Also converts values to other types if specified.
             * @function toObject
             * @memberof mirabuf.joint.PrismaticJoint
             * @static
             * @param {mirabuf.joint.PrismaticJoint} message PrismaticJoint
             * @param {$protobuf.IConversionOptions} [options] Conversion options
             * @returns {Object.<string,*>} Plain object
             */
            PrismaticJoint.toObject = function toObject(message, options) {
                if (!options) options = {}
                let object = {}
                if (options.defaults) object.prismaticFreedom = null
                if (message.prismaticFreedom != null && Object.hasOwn(message, "prismaticFreedom"))
                    object.prismaticFreedom = $root.mirabuf.joint.DOF.toObject(message.prismaticFreedom, options)
                return object
            }

            /**
             * Converts this PrismaticJoint to JSON.
             * @function toJSON
             * @memberof mirabuf.joint.PrismaticJoint
             * @instance
             * @returns {Object.<string,*>} JSON object
             */
            PrismaticJoint.prototype.toJSON = function toJSON() {
                return this.constructor.toObject(this, $protobuf.util.toJSONOptions)
            }

            /**
             * Gets the default type url for PrismaticJoint
             * @function getTypeUrl
             * @memberof mirabuf.joint.PrismaticJoint
             * @static
             * @param {string} [typeUrlPrefix] your custom typeUrlPrefix(default "type.googleapis.com")
             * @returns {string} The default type url
             */
            PrismaticJoint.getTypeUrl = function getTypeUrl(typeUrlPrefix) {
                if (typeUrlPrefix === undefined) {
                    typeUrlPrefix = "type.googleapis.com"
                }
                return typeUrlPrefix + "/mirabuf.joint.PrismaticJoint"
            }

            return PrismaticJoint
        })()

        joint.RigidGroup = (function () {
            /**
             * Properties of a RigidGroup.
             * @memberof mirabuf.joint
             * @interface IRigidGroup
             * @property {string|null} [name] RigidGroup name
             * @property {Array.<string>|null} [occurrences] RigidGroup occurrences
             */

            /**
             * Constructs a new RigidGroup.
             * @memberof mirabuf.joint
             * @classdesc Represents a RigidGroup.
             * @implements IRigidGroup
             * @constructor
             * @param {mirabuf.joint.IRigidGroup=} [properties] Properties to set
             */
            function RigidGroup(properties) {
                this.occurrences = []
                if (properties)
                    for (let keys = Object.keys(properties), i = 0; i < keys.length; ++i)
                        if (properties[keys[i]] != null) this[keys[i]] = properties[keys[i]]
            }

            /**
             * RigidGroup name.
             * @member {string} name
             * @memberof mirabuf.joint.RigidGroup
             * @instance
             */
            RigidGroup.prototype.name = ""

            /**
             * RigidGroup occurrences.
             * @member {Array.<string>} occurrences
             * @memberof mirabuf.joint.RigidGroup
             * @instance
             */
            RigidGroup.prototype.occurrences = $util.emptyArray

            /**
             * Creates a new RigidGroup instance using the specified properties.
             * @function create
             * @memberof mirabuf.joint.RigidGroup
             * @static
             * @param {mirabuf.joint.IRigidGroup=} [properties] Properties to set
             * @returns {mirabuf.joint.RigidGroup} RigidGroup instance
             */
            RigidGroup.create = function create(properties) {
                return new RigidGroup(properties)
            }

            /**
             * Encodes the specified RigidGroup message. Does not implicitly {@link mirabuf.joint.RigidGroup.verify|verify} messages.
             * @function encode
             * @memberof mirabuf.joint.RigidGroup
             * @static
             * @param {mirabuf.joint.IRigidGroup} message RigidGroup message or plain object to encode
             * @param {$protobuf.Writer} [writer] Writer to encode to
             * @returns {$protobuf.Writer} Writer
             */
            RigidGroup.encode = function encode(message, writer) {
                if (!writer) writer = $Writer.create()
                if (message.name != null && Object.hasOwn(message, "name"))
                    writer.uint32(/* id 1, wireType 2 =*/ 10).string(message.name)
                if (message.occurrences != null && message.occurrences.length)
                    for (let i = 0; i < message.occurrences.length; ++i)
                        writer.uint32(/* id 2, wireType 2 =*/ 18).string(message.occurrences[i])
                return writer
            }

            /**
             * Encodes the specified RigidGroup message, length delimited. Does not implicitly {@link mirabuf.joint.RigidGroup.verify|verify} messages.
             * @function encodeDelimited
             * @memberof mirabuf.joint.RigidGroup
             * @static
             * @param {mirabuf.joint.IRigidGroup} message RigidGroup message or plain object to encode
             * @param {$protobuf.Writer} [writer] Writer to encode to
             * @returns {$protobuf.Writer} Writer
             */
            RigidGroup.encodeDelimited = function encodeDelimited(message, writer) {
                return this.encode(message, writer).ldelim()
            }

            /**
             * Decodes a RigidGroup message from the specified reader or buffer.
             * @function decode
             * @memberof mirabuf.joint.RigidGroup
             * @static
             * @param {$protobuf.Reader|Uint8Array} reader Reader or buffer to decode from
             * @param {number} [length] Message length if known beforehand
             * @returns {mirabuf.joint.RigidGroup} RigidGroup
             * @throws {Error} If the payload is not a reader or valid buffer
             * @throws {$protobuf.util.ProtocolError} If required fields are missing
             */
            RigidGroup.decode = function decode(reader, length) {
                if (!(reader instanceof $Reader)) reader = $Reader.create(reader)
                let end = length === undefined ? reader.len : reader.pos + length,
                    message = new $root.mirabuf.joint.RigidGroup()
                while (reader.pos < end) {
                    let tag = reader.uint32()
                    switch (tag >>> 3) {
                        case 1: {
                            message.name = reader.string()
                            break
                        }
                        case 2: {
                            if (!(message.occurrences && message.occurrences.length)) message.occurrences = []
                            message.occurrences.push(reader.string())
                            break
                        }
                        default:
                            reader.skipType(tag & 7)
                            break
                    }
                }
                return message
            }

            /**
             * Decodes a RigidGroup message from the specified reader or buffer, length delimited.
             * @function decodeDelimited
             * @memberof mirabuf.joint.RigidGroup
             * @static
             * @param {$protobuf.Reader|Uint8Array} reader Reader or buffer to decode from
             * @returns {mirabuf.joint.RigidGroup} RigidGroup
             * @throws {Error} If the payload is not a reader or valid buffer
             * @throws {$protobuf.util.ProtocolError} If required fields are missing
             */
            RigidGroup.decodeDelimited = function decodeDelimited(reader) {
                if (!(reader instanceof $Reader)) reader = new $Reader(reader)
                return this.decode(reader, reader.uint32())
            }

            /**
             * Verifies a RigidGroup message.
             * @function verify
             * @memberof mirabuf.joint.RigidGroup
             * @static
             * @param {Object.<string,*>} message Plain object to verify
             * @returns {string|null} `null` if valid, otherwise the reason why it is not
             */
            RigidGroup.verify = function verify(message) {
                if (typeof message !== "object" || message === null) return "object expected"
                if (message.name != null && Object.hasOwn(message, "name"))
                    if (!$util.isString(message.name)) return "name: string expected"
                if (message.occurrences != null && Object.hasOwn(message, "occurrences")) {
                    if (!Array.isArray(message.occurrences)) return "occurrences: array expected"
                    for (let i = 0; i < message.occurrences.length; ++i)
                        if (!$util.isString(message.occurrences[i])) return "occurrences: string[] expected"
                }
                return null
            }

            /**
             * Creates a RigidGroup message from a plain object. Also converts values to their respective internal types.
             * @function fromObject
             * @memberof mirabuf.joint.RigidGroup
             * @static
             * @param {Object.<string,*>} object Plain object
             * @returns {mirabuf.joint.RigidGroup} RigidGroup
             */
            RigidGroup.fromObject = function fromObject(object) {
                if (object instanceof $root.mirabuf.joint.RigidGroup) return object
                let message = new $root.mirabuf.joint.RigidGroup()
                if (object.name != null) message.name = String(object.name)
                if (object.occurrences) {
                    if (!Array.isArray(object.occurrences))
                        throw TypeError(".mirabuf.joint.RigidGroup.occurrences: array expected")
                    message.occurrences = []
                    for (let i = 0; i < object.occurrences.length; ++i)
                        message.occurrences[i] = String(object.occurrences[i])
                }
                return message
            }

            /**
             * Creates a plain object from a RigidGroup message. Also converts values to other types if specified.
             * @function toObject
             * @memberof mirabuf.joint.RigidGroup
             * @static
             * @param {mirabuf.joint.RigidGroup} message RigidGroup
             * @param {$protobuf.IConversionOptions} [options] Conversion options
             * @returns {Object.<string,*>} Plain object
             */
            RigidGroup.toObject = function toObject(message, options) {
                if (!options) options = {}
                let object = {}
                if (options.arrays || options.defaults) object.occurrences = []
                if (options.defaults) object.name = ""
                if (message.name != null && Object.hasOwn(message, "name")) object.name = message.name
                if (message.occurrences && message.occurrences.length) {
                    object.occurrences = []
                    for (let j = 0; j < message.occurrences.length; ++j) object.occurrences[j] = message.occurrences[j]
                }
                return object
            }

            /**
             * Converts this RigidGroup to JSON.
             * @function toJSON
             * @memberof mirabuf.joint.RigidGroup
             * @instance
             * @returns {Object.<string,*>} JSON object
             */
            RigidGroup.prototype.toJSON = function toJSON() {
                return this.constructor.toObject(this, $protobuf.util.toJSONOptions)
            }

            /**
             * Gets the default type url for RigidGroup
             * @function getTypeUrl
             * @memberof mirabuf.joint.RigidGroup
             * @static
             * @param {string} [typeUrlPrefix] your custom typeUrlPrefix(default "type.googleapis.com")
             * @returns {string} The default type url
             */
            RigidGroup.getTypeUrl = function getTypeUrl(typeUrlPrefix) {
                if (typeUrlPrefix === undefined) {
                    typeUrlPrefix = "type.googleapis.com"
                }
                return typeUrlPrefix + "/mirabuf.joint.RigidGroup"
            }

            return RigidGroup
        })()

        return joint
    })()

    mirabuf.motor = (function () {
        /**
         * Namespace motor.
         * @memberof mirabuf
         * @namespace
         */
        const motor = {}

        /**
         * Duty Cycles for electric motors
         * Affects the dynamic output of the motor
         * https://www.news.benevelli-group.com/index.php/en/88-what-motor-duty-cycle.html
         * These each have associated data we are not going to use right now
         * @name mirabuf.motor.DutyCycles
         * @enum {number}
         * @property {number} CONTINUOUS_RUNNING=0 S1
         * @property {number} SHORT_TIME=1 S2
         * @property {number} INTERMITTENT_PERIODIC=2 S3
         * @property {number} CONTINUOUS_PERIODIC=3 S6 Continuous Operation with Periodic Duty
         */
        motor.DutyCycles = (function () {
            const valuesById = {},
                values = Object.create(valuesById)
            values[(valuesById[0] = "CONTINUOUS_RUNNING")] = 0
            values[(valuesById[1] = "SHORT_TIME")] = 1
            values[(valuesById[2] = "INTERMITTENT_PERIODIC")] = 2
            values[(valuesById[3] = "CONTINUOUS_PERIODIC")] = 3
            return values
        })()

        motor.Motor = (function () {
            /**
             * Properties of a Motor.
             * @memberof mirabuf.motor
             * @interface IMotor
             * @property {mirabuf.IInfo|null} [info] Motor info
             * @property {mirabuf.motor.IDCMotor|null} [dcMotor] Motor dcMotor
             * @property {mirabuf.motor.ISimpleMotor|null} [simpleMotor] Motor simpleMotor
             */

            /**
             * Constructs a new Motor.
             * @memberof mirabuf.motor
             * @classdesc A Motor should determine the relationship between an input and joint motion
             * Could represent something like a DC Motor relationship
             * @implements IMotor
             * @constructor
             * @param {mirabuf.motor.IMotor=} [properties] Properties to set
             */
            function Motor(properties) {
                if (properties)
                    for (let keys = Object.keys(properties), i = 0; i < keys.length; ++i)
                        if (properties[keys[i]] != null) this[keys[i]] = properties[keys[i]]
            }

            /**
             * Motor info.
             * @member {mirabuf.IInfo|null|undefined} info
             * @memberof mirabuf.motor.Motor
             * @instance
             */
            Motor.prototype.info = null

            /**
             * Motor dcMotor.
             * @member {mirabuf.motor.IDCMotor|null|undefined} dcMotor
             * @memberof mirabuf.motor.Motor
             * @instance
             */
            Motor.prototype.dcMotor = null

            /**
             * Motor simpleMotor.
             * @member {mirabuf.motor.ISimpleMotor|null|undefined} simpleMotor
             * @memberof mirabuf.motor.Motor
             * @instance
             */
            Motor.prototype.simpleMotor = null

            // OneOf field names bound to virtual getters and setters
            let $oneOfFields

            /**
             * Motor motorType.
             * @member {"dcMotor"|"simpleMotor"|undefined} motorType
             * @memberof mirabuf.motor.Motor
             * @instance
             */
            Object.defineProperty(Motor.prototype, "motorType", {
                get: $util.oneOfGetter(($oneOfFields = ["dcMotor", "simpleMotor"])),
                set: $util.oneOfSetter($oneOfFields),
            })

            /**
             * Creates a new Motor instance using the specified properties.
             * @function create
             * @memberof mirabuf.motor.Motor
             * @static
             * @param {mirabuf.motor.IMotor=} [properties] Properties to set
             * @returns {mirabuf.motor.Motor} Motor instance
             */
            Motor.create = function create(properties) {
                return new Motor(properties)
            }

            /**
             * Encodes the specified Motor message. Does not implicitly {@link mirabuf.motor.Motor.verify|verify} messages.
             * @function encode
             * @memberof mirabuf.motor.Motor
             * @static
             * @param {mirabuf.motor.IMotor} message Motor message or plain object to encode
             * @param {$protobuf.Writer} [writer] Writer to encode to
             * @returns {$protobuf.Writer} Writer
             */
            Motor.encode = function encode(message, writer) {
                if (!writer) writer = $Writer.create()
                if (message.info != null && Object.hasOwn(message, "info"))
                    $root.mirabuf.Info.encode(message.info, writer.uint32(/* id 1, wireType 2 =*/ 10).fork()).ldelim()
                if (message.dcMotor != null && Object.hasOwn(message, "dcMotor"))
                    $root.mirabuf.motor.DCMotor.encode(
                        message.dcMotor,
                        writer.uint32(/* id 2, wireType 2 =*/ 18).fork()
                    ).ldelim()
                if (message.simpleMotor != null && Object.hasOwn(message, "simpleMotor"))
                    $root.mirabuf.motor.SimpleMotor.encode(
                        message.simpleMotor,
                        writer.uint32(/* id 3, wireType 2 =*/ 26).fork()
                    ).ldelim()
                return writer
            }

            /**
             * Encodes the specified Motor message, length delimited. Does not implicitly {@link mirabuf.motor.Motor.verify|verify} messages.
             * @function encodeDelimited
             * @memberof mirabuf.motor.Motor
             * @static
             * @param {mirabuf.motor.IMotor} message Motor message or plain object to encode
             * @param {$protobuf.Writer} [writer] Writer to encode to
             * @returns {$protobuf.Writer} Writer
             */
            Motor.encodeDelimited = function encodeDelimited(message, writer) {
                return this.encode(message, writer).ldelim()
            }

            /**
             * Decodes a Motor message from the specified reader or buffer.
             * @function decode
             * @memberof mirabuf.motor.Motor
             * @static
             * @param {$protobuf.Reader|Uint8Array} reader Reader or buffer to decode from
             * @param {number} [length] Message length if known beforehand
             * @returns {mirabuf.motor.Motor} Motor
             * @throws {Error} If the payload is not a reader or valid buffer
             * @throws {$protobuf.util.ProtocolError} If required fields are missing
             */
            Motor.decode = function decode(reader, length) {
                if (!(reader instanceof $Reader)) reader = $Reader.create(reader)
                let end = length === undefined ? reader.len : reader.pos + length,
                    message = new $root.mirabuf.motor.Motor()
                while (reader.pos < end) {
                    let tag = reader.uint32()
                    switch (tag >>> 3) {
                        case 1: {
                            message.info = $root.mirabuf.Info.decode(reader, reader.uint32())
                            break
                        }
                        case 2: {
                            message.dcMotor = $root.mirabuf.motor.DCMotor.decode(reader, reader.uint32())
                            break
                        }
                        case 3: {
                            message.simpleMotor = $root.mirabuf.motor.SimpleMotor.decode(reader, reader.uint32())
                            break
                        }
                        default:
                            reader.skipType(tag & 7)
                            break
                    }
                }
                return message
            }

            /**
             * Decodes a Motor message from the specified reader or buffer, length delimited.
             * @function decodeDelimited
             * @memberof mirabuf.motor.Motor
             * @static
             * @param {$protobuf.Reader|Uint8Array} reader Reader or buffer to decode from
             * @returns {mirabuf.motor.Motor} Motor
             * @throws {Error} If the payload is not a reader or valid buffer
             * @throws {$protobuf.util.ProtocolError} If required fields are missing
             */
            Motor.decodeDelimited = function decodeDelimited(reader) {
                if (!(reader instanceof $Reader)) reader = new $Reader(reader)
                return this.decode(reader, reader.uint32())
            }

            /**
             * Verifies a Motor message.
             * @function verify
             * @memberof mirabuf.motor.Motor
             * @static
             * @param {Object.<string,*>} message Plain object to verify
             * @returns {string|null} `null` if valid, otherwise the reason why it is not
             */
            Motor.verify = function verify(message) {
                if (typeof message !== "object" || message === null) return "object expected"
                let properties = {}
                if (message.info != null && Object.hasOwn(message, "info")) {
                    let error = $root.mirabuf.Info.verify(message.info)
                    if (error) return "info." + error
                }
                if (message.dcMotor != null && Object.hasOwn(message, "dcMotor")) {
                    properties.motorType = 1
                    {
                        let error = $root.mirabuf.motor.DCMotor.verify(message.dcMotor)
                        if (error) return "dcMotor." + error
                    }
                }
                if (message.simpleMotor != null && Object.hasOwn(message, "simpleMotor")) {
                    if (properties.motorType === 1) return "motorType: multiple values"
                    properties.motorType = 1
                    {
                        let error = $root.mirabuf.motor.SimpleMotor.verify(message.simpleMotor)
                        if (error) return "simpleMotor." + error
                    }
                }
                return null
            }

            /**
             * Creates a Motor message from a plain object. Also converts values to their respective internal types.
             * @function fromObject
             * @memberof mirabuf.motor.Motor
             * @static
             * @param {Object.<string,*>} object Plain object
             * @returns {mirabuf.motor.Motor} Motor
             */
            Motor.fromObject = function fromObject(object) {
                if (object instanceof $root.mirabuf.motor.Motor) return object
                let message = new $root.mirabuf.motor.Motor()
                if (object.info != null) {
                    if (typeof object.info !== "object") throw TypeError(".mirabuf.motor.Motor.info: object expected")
                    message.info = $root.mirabuf.Info.fromObject(object.info)
                }
                if (object.dcMotor != null) {
                    if (typeof object.dcMotor !== "object")
                        throw TypeError(".mirabuf.motor.Motor.dcMotor: object expected")
                    message.dcMotor = $root.mirabuf.motor.DCMotor.fromObject(object.dcMotor)
                }
                if (object.simpleMotor != null) {
                    if (typeof object.simpleMotor !== "object")
                        throw TypeError(".mirabuf.motor.Motor.simpleMotor: object expected")
                    message.simpleMotor = $root.mirabuf.motor.SimpleMotor.fromObject(object.simpleMotor)
                }
                return message
            }

            /**
             * Creates a plain object from a Motor message. Also converts values to other types if specified.
             * @function toObject
             * @memberof mirabuf.motor.Motor
             * @static
             * @param {mirabuf.motor.Motor} message Motor
             * @param {$protobuf.IConversionOptions} [options] Conversion options
             * @returns {Object.<string,*>} Plain object
             */
            Motor.toObject = function toObject(message, options) {
                if (!options) options = {}
                let object = {}
                if (options.defaults) object.info = null
                if (message.info != null && Object.hasOwn(message, "info"))
                    object.info = $root.mirabuf.Info.toObject(message.info, options)
                if (message.dcMotor != null && Object.hasOwn(message, "dcMotor")) {
                    object.dcMotor = $root.mirabuf.motor.DCMotor.toObject(message.dcMotor, options)
                    if (options.oneofs) object.motorType = "dcMotor"
                }
                if (message.simpleMotor != null && Object.hasOwn(message, "simpleMotor")) {
                    object.simpleMotor = $root.mirabuf.motor.SimpleMotor.toObject(message.simpleMotor, options)
                    if (options.oneofs) object.motorType = "simpleMotor"
                }
                return object
            }

            /**
             * Converts this Motor to JSON.
             * @function toJSON
             * @memberof mirabuf.motor.Motor
             * @instance
             * @returns {Object.<string,*>} JSON object
             */
            Motor.prototype.toJSON = function toJSON() {
                return this.constructor.toObject(this, $protobuf.util.toJSONOptions)
            }

            /**
             * Gets the default type url for Motor
             * @function getTypeUrl
             * @memberof mirabuf.motor.Motor
             * @static
             * @param {string} [typeUrlPrefix] your custom typeUrlPrefix(default "type.googleapis.com")
             * @returns {string} The default type url
             */
            Motor.getTypeUrl = function getTypeUrl(typeUrlPrefix) {
                if (typeUrlPrefix === undefined) {
                    typeUrlPrefix = "type.googleapis.com"
                }
                return typeUrlPrefix + "/mirabuf.motor.Motor"
            }

            return Motor
        })()

        motor.SimpleMotor = (function () {
            /**
             * Properties of a SimpleMotor.
             * @memberof mirabuf.motor
             * @interface ISimpleMotor
             * @property {number|null} [stallTorque] Torque at 0 rpm with a inverse linear relationship to max_velocity
             * @property {number|null} [maxVelocity] The target velocity in RPM, will use stall_torque relationship to reach each step
             * @property {number|null} [brakingConstant] (Optional) 0 - 1, the relationship of stall_torque used to perserve the position of this motor
             */

            /**
             * Constructs a new SimpleMotor.
             * @memberof mirabuf.motor
             * @classdesc SimpleMotor Configuration
             * Very easy motor used to simulate joints without specifying a real motor
             * Can set braking_constant - stall_torque - and max_velocity
             * Assumes you are solving using a velocity constraint for a joint and not a acceleration constraint
             * @implements ISimpleMotor
             * @constructor
             * @param {mirabuf.motor.ISimpleMotor=} [properties] Properties to set
             */
            function SimpleMotor(properties) {
                if (properties)
                    for (let keys = Object.keys(properties), i = 0; i < keys.length; ++i)
                        if (properties[keys[i]] != null) this[keys[i]] = properties[keys[i]]
            }

            /**
             * Torque at 0 rpm with a inverse linear relationship to max_velocity
             * @member {number} stallTorque
             * @memberof mirabuf.motor.SimpleMotor
             * @instance
             */
            SimpleMotor.prototype.stallTorque = 0

            /**
             * The target velocity in RPM, will use stall_torque relationship to reach each step
             * @member {number} maxVelocity
             * @memberof mirabuf.motor.SimpleMotor
             * @instance
             */
            SimpleMotor.prototype.maxVelocity = 0

            /**
             * (Optional) 0 - 1, the relationship of stall_torque used to perserve the position of this motor
             * @member {number} brakingConstant
             * @memberof mirabuf.motor.SimpleMotor
             * @instance
             */
            SimpleMotor.prototype.brakingConstant = 0

            /**
             * Creates a new SimpleMotor instance using the specified properties.
             * @function create
             * @memberof mirabuf.motor.SimpleMotor
             * @static
             * @param {mirabuf.motor.ISimpleMotor=} [properties] Properties to set
             * @returns {mirabuf.motor.SimpleMotor} SimpleMotor instance
             */
            SimpleMotor.create = function create(properties) {
                return new SimpleMotor(properties)
            }

            /**
             * Encodes the specified SimpleMotor message. Does not implicitly {@link mirabuf.motor.SimpleMotor.verify|verify} messages.
             * @function encode
             * @memberof mirabuf.motor.SimpleMotor
             * @static
             * @param {mirabuf.motor.ISimpleMotor} message SimpleMotor message or plain object to encode
             * @param {$protobuf.Writer} [writer] Writer to encode to
             * @returns {$protobuf.Writer} Writer
             */
            SimpleMotor.encode = function encode(message, writer) {
                if (!writer) writer = $Writer.create()
                if (message.stallTorque != null && Object.hasOwn(message, "stallTorque"))
                    writer.uint32(/* id 1, wireType 5 =*/ 13).float(message.stallTorque)
                if (message.maxVelocity != null && Object.hasOwn(message, "maxVelocity"))
                    writer.uint32(/* id 2, wireType 5 =*/ 21).float(message.maxVelocity)
                if (message.brakingConstant != null && Object.hasOwn(message, "brakingConstant"))
                    writer.uint32(/* id 3, wireType 5 =*/ 29).float(message.brakingConstant)
                return writer
            }

            /**
             * Encodes the specified SimpleMotor message, length delimited. Does not implicitly {@link mirabuf.motor.SimpleMotor.verify|verify} messages.
             * @function encodeDelimited
             * @memberof mirabuf.motor.SimpleMotor
             * @static
             * @param {mirabuf.motor.ISimpleMotor} message SimpleMotor message or plain object to encode
             * @param {$protobuf.Writer} [writer] Writer to encode to
             * @returns {$protobuf.Writer} Writer
             */
            SimpleMotor.encodeDelimited = function encodeDelimited(message, writer) {
                return this.encode(message, writer).ldelim()
            }

            /**
             * Decodes a SimpleMotor message from the specified reader or buffer.
             * @function decode
             * @memberof mirabuf.motor.SimpleMotor
             * @static
             * @param {$protobuf.Reader|Uint8Array} reader Reader or buffer to decode from
             * @param {number} [length] Message length if known beforehand
             * @returns {mirabuf.motor.SimpleMotor} SimpleMotor
             * @throws {Error} If the payload is not a reader or valid buffer
             * @throws {$protobuf.util.ProtocolError} If required fields are missing
             */
            SimpleMotor.decode = function decode(reader, length) {
                if (!(reader instanceof $Reader)) reader = $Reader.create(reader)
                let end = length === undefined ? reader.len : reader.pos + length,
                    message = new $root.mirabuf.motor.SimpleMotor()
                while (reader.pos < end) {
                    let tag = reader.uint32()
                    switch (tag >>> 3) {
                        case 1: {
                            message.stallTorque = reader.float()
                            break
                        }
                        case 2: {
                            message.maxVelocity = reader.float()
                            break
                        }
                        case 3: {
                            message.brakingConstant = reader.float()
                            break
                        }
                        default:
                            reader.skipType(tag & 7)
                            break
                    }
                }
                return message
            }

            /**
             * Decodes a SimpleMotor message from the specified reader or buffer, length delimited.
             * @function decodeDelimited
             * @memberof mirabuf.motor.SimpleMotor
             * @static
             * @param {$protobuf.Reader|Uint8Array} reader Reader or buffer to decode from
             * @returns {mirabuf.motor.SimpleMotor} SimpleMotor
             * @throws {Error} If the payload is not a reader or valid buffer
             * @throws {$protobuf.util.ProtocolError} If required fields are missing
             */
            SimpleMotor.decodeDelimited = function decodeDelimited(reader) {
                if (!(reader instanceof $Reader)) reader = new $Reader(reader)
                return this.decode(reader, reader.uint32())
            }

            /**
             * Verifies a SimpleMotor message.
             * @function verify
             * @memberof mirabuf.motor.SimpleMotor
             * @static
             * @param {Object.<string,*>} message Plain object to verify
             * @returns {string|null} `null` if valid, otherwise the reason why it is not
             */
            SimpleMotor.verify = function verify(message) {
                if (typeof message !== "object" || message === null) return "object expected"
                if (message.stallTorque != null && Object.hasOwn(message, "stallTorque"))
                    if (typeof message.stallTorque !== "number") return "stallTorque: number expected"
                if (message.maxVelocity != null && Object.hasOwn(message, "maxVelocity"))
                    if (typeof message.maxVelocity !== "number") return "maxVelocity: number expected"
                if (message.brakingConstant != null && Object.hasOwn(message, "brakingConstant"))
                    if (typeof message.brakingConstant !== "number") return "brakingConstant: number expected"
                return null
            }

            /**
             * Creates a SimpleMotor message from a plain object. Also converts values to their respective internal types.
             * @function fromObject
             * @memberof mirabuf.motor.SimpleMotor
             * @static
             * @param {Object.<string,*>} object Plain object
             * @returns {mirabuf.motor.SimpleMotor} SimpleMotor
             */
            SimpleMotor.fromObject = function fromObject(object) {
                if (object instanceof $root.mirabuf.motor.SimpleMotor) return object
                let message = new $root.mirabuf.motor.SimpleMotor()
                if (object.stallTorque != null) message.stallTorque = Number(object.stallTorque)
                if (object.maxVelocity != null) message.maxVelocity = Number(object.maxVelocity)
                if (object.brakingConstant != null) message.brakingConstant = Number(object.brakingConstant)
                return message
            }

            /**
             * Creates a plain object from a SimpleMotor message. Also converts values to other types if specified.
             * @function toObject
             * @memberof mirabuf.motor.SimpleMotor
             * @static
             * @param {mirabuf.motor.SimpleMotor} message SimpleMotor
             * @param {$protobuf.IConversionOptions} [options] Conversion options
             * @returns {Object.<string,*>} Plain object
             */
            SimpleMotor.toObject = function toObject(message, options) {
                if (!options) options = {}
                let object = {}
                if (options.defaults) {
                    object.stallTorque = 0
                    object.maxVelocity = 0
                    object.brakingConstant = 0
                }
                if (message.stallTorque != null && Object.hasOwn(message, "stallTorque"))
                    object.stallTorque =
                        options.json && !isFinite(message.stallTorque)
                            ? String(message.stallTorque)
                            : message.stallTorque
                if (message.maxVelocity != null && Object.hasOwn(message, "maxVelocity"))
                    object.maxVelocity =
                        options.json && !isFinite(message.maxVelocity)
                            ? String(message.maxVelocity)
                            : message.maxVelocity
                if (message.brakingConstant != null && Object.hasOwn(message, "brakingConstant"))
                    object.brakingConstant =
                        options.json && !isFinite(message.brakingConstant)
                            ? String(message.brakingConstant)
                            : message.brakingConstant
                return object
            }

            /**
             * Converts this SimpleMotor to JSON.
             * @function toJSON
             * @memberof mirabuf.motor.SimpleMotor
             * @instance
             * @returns {Object.<string,*>} JSON object
             */
            SimpleMotor.prototype.toJSON = function toJSON() {
                return this.constructor.toObject(this, $protobuf.util.toJSONOptions)
            }

            /**
             * Gets the default type url for SimpleMotor
             * @function getTypeUrl
             * @memberof mirabuf.motor.SimpleMotor
             * @static
             * @param {string} [typeUrlPrefix] your custom typeUrlPrefix(default "type.googleapis.com")
             * @returns {string} The default type url
             */
            SimpleMotor.getTypeUrl = function getTypeUrl(typeUrlPrefix) {
                if (typeUrlPrefix === undefined) {
                    typeUrlPrefix = "type.googleapis.com"
                }
                return typeUrlPrefix + "/mirabuf.motor.SimpleMotor"
            }

            return SimpleMotor
        })()

        motor.DCMotor = (function () {
            /**
             * Properties of a DCMotor.
             * @memberof mirabuf.motor
             * @interface IDCMotor
             * @property {string|null} [referenceUrl] Reference for purchase page or spec sheet
             * @property {number|null} [torqueConstant] m-Nm/Amp
             * @property {number|null} [emfConstant] mV/rad/sec
             * @property {number|null} [resistance] Resistance of Motor - Optional if other values are known
             * @property {number|null} [maximumEffeciency] measure in percentage of 100 - generally around 60 - measured under optimal load
             * @property {number|null} [maximumPower] measured in Watts
             * @property {mirabuf.motor.DutyCycles|null} [dutyCycle] Stated Duty Cycle of motor
             * @property {mirabuf.motor.DCMotor.IAdvanced|null} [advanced] Optional data that can give a better relationship to the simulation
             */

            /**
             * Constructs a new DCMotor.
             * @memberof mirabuf.motor
             * @classdesc DCMotor Configuration
             * Parameters to simulate a DC Electric Motor
             * Still needs some more but overall they are most of the parameters we can use
             * @implements IDCMotor
             * @constructor
             * @param {mirabuf.motor.IDCMotor=} [properties] Properties to set
             */
            function DCMotor(properties) {
                if (properties)
                    for (let keys = Object.keys(properties), i = 0; i < keys.length; ++i)
                        if (properties[keys[i]] != null) this[keys[i]] = properties[keys[i]]
            }

            /**
             * Reference for purchase page or spec sheet
             * @member {string} referenceUrl
             * @memberof mirabuf.motor.DCMotor
             * @instance
             */
            DCMotor.prototype.referenceUrl = ""

            /**
             * m-Nm/Amp
             * @member {number} torqueConstant
             * @memberof mirabuf.motor.DCMotor
             * @instance
             */
            DCMotor.prototype.torqueConstant = 0

            /**
             * mV/rad/sec
             * @member {number} emfConstant
             * @memberof mirabuf.motor.DCMotor
             * @instance
             */
            DCMotor.prototype.emfConstant = 0

            /**
             * Resistance of Motor - Optional if other values are known
             * @member {number} resistance
             * @memberof mirabuf.motor.DCMotor
             * @instance
             */
            DCMotor.prototype.resistance = 0

            /**
             * measure in percentage of 100 - generally around 60 - measured under optimal load
             * @member {number} maximumEffeciency
             * @memberof mirabuf.motor.DCMotor
             * @instance
             */
            DCMotor.prototype.maximumEffeciency = 0

            /**
             * measured in Watts
             * @member {number} maximumPower
             * @memberof mirabuf.motor.DCMotor
             * @instance
             */
            DCMotor.prototype.maximumPower = 0

            /**
             * Stated Duty Cycle of motor
             * @member {mirabuf.motor.DutyCycles} dutyCycle
             * @memberof mirabuf.motor.DCMotor
             * @instance
             */
            DCMotor.prototype.dutyCycle = 0

            /**
             * Optional data that can give a better relationship to the simulation
             * @member {mirabuf.motor.DCMotor.IAdvanced|null|undefined} advanced
             * @memberof mirabuf.motor.DCMotor
             * @instance
             */
            DCMotor.prototype.advanced = null

            /**
             * Creates a new DCMotor instance using the specified properties.
             * @function create
             * @memberof mirabuf.motor.DCMotor
             * @static
             * @param {mirabuf.motor.IDCMotor=} [properties] Properties to set
             * @returns {mirabuf.motor.DCMotor} DCMotor instance
             */
            DCMotor.create = function create(properties) {
                return new DCMotor(properties)
            }

            /**
             * Encodes the specified DCMotor message. Does not implicitly {@link mirabuf.motor.DCMotor.verify|verify} messages.
             * @function encode
             * @memberof mirabuf.motor.DCMotor
             * @static
             * @param {mirabuf.motor.IDCMotor} message DCMotor message or plain object to encode
             * @param {$protobuf.Writer} [writer] Writer to encode to
             * @returns {$protobuf.Writer} Writer
             */
            DCMotor.encode = function encode(message, writer) {
                if (!writer) writer = $Writer.create()
                if (message.referenceUrl != null && Object.hasOwn(message, "referenceUrl"))
                    writer.uint32(/* id 2, wireType 2 =*/ 18).string(message.referenceUrl)
                if (message.torqueConstant != null && Object.hasOwn(message, "torqueConstant"))
                    writer.uint32(/* id 3, wireType 5 =*/ 29).float(message.torqueConstant)
                if (message.emfConstant != null && Object.hasOwn(message, "emfConstant"))
                    writer.uint32(/* id 4, wireType 5 =*/ 37).float(message.emfConstant)
                if (message.resistance != null && Object.hasOwn(message, "resistance"))
                    writer.uint32(/* id 5, wireType 5 =*/ 45).float(message.resistance)
                if (message.maximumEffeciency != null && Object.hasOwn(message, "maximumEffeciency"))
                    writer.uint32(/* id 6, wireType 0 =*/ 48).uint32(message.maximumEffeciency)
                if (message.maximumPower != null && Object.hasOwn(message, "maximumPower"))
                    writer.uint32(/* id 7, wireType 0 =*/ 56).uint32(message.maximumPower)
                if (message.dutyCycle != null && Object.hasOwn(message, "dutyCycle"))
                    writer.uint32(/* id 8, wireType 0 =*/ 64).int32(message.dutyCycle)
                if (message.advanced != null && Object.hasOwn(message, "advanced"))
                    $root.mirabuf.motor.DCMotor.Advanced.encode(
                        message.advanced,
                        writer.uint32(/* id 16, wireType 2 =*/ 130).fork()
                    ).ldelim()
                return writer
            }

            /**
             * Encodes the specified DCMotor message, length delimited. Does not implicitly {@link mirabuf.motor.DCMotor.verify|verify} messages.
             * @function encodeDelimited
             * @memberof mirabuf.motor.DCMotor
             * @static
             * @param {mirabuf.motor.IDCMotor} message DCMotor message or plain object to encode
             * @param {$protobuf.Writer} [writer] Writer to encode to
             * @returns {$protobuf.Writer} Writer
             */
            DCMotor.encodeDelimited = function encodeDelimited(message, writer) {
                return this.encode(message, writer).ldelim()
            }

            /**
             * Decodes a DCMotor message from the specified reader or buffer.
             * @function decode
             * @memberof mirabuf.motor.DCMotor
             * @static
             * @param {$protobuf.Reader|Uint8Array} reader Reader or buffer to decode from
             * @param {number} [length] Message length if known beforehand
             * @returns {mirabuf.motor.DCMotor} DCMotor
             * @throws {Error} If the payload is not a reader or valid buffer
             * @throws {$protobuf.util.ProtocolError} If required fields are missing
             */
            DCMotor.decode = function decode(reader, length) {
                if (!(reader instanceof $Reader)) reader = $Reader.create(reader)
                let end = length === undefined ? reader.len : reader.pos + length,
                    message = new $root.mirabuf.motor.DCMotor()
                while (reader.pos < end) {
                    let tag = reader.uint32()
                    switch (tag >>> 3) {
                        case 2: {
                            message.referenceUrl = reader.string()
                            break
                        }
                        case 3: {
                            message.torqueConstant = reader.float()
                            break
                        }
                        case 4: {
                            message.emfConstant = reader.float()
                            break
                        }
                        case 5: {
                            message.resistance = reader.float()
                            break
                        }
                        case 6: {
                            message.maximumEffeciency = reader.uint32()
                            break
                        }
                        case 7: {
                            message.maximumPower = reader.uint32()
                            break
                        }
                        case 8: {
                            message.dutyCycle = reader.int32()
                            break
                        }
                        case 16: {
                            message.advanced = $root.mirabuf.motor.DCMotor.Advanced.decode(reader, reader.uint32())
                            break
                        }
                        default:
                            reader.skipType(tag & 7)
                            break
                    }
                }
                return message
            }

            /**
             * Decodes a DCMotor message from the specified reader or buffer, length delimited.
             * @function decodeDelimited
             * @memberof mirabuf.motor.DCMotor
             * @static
             * @param {$protobuf.Reader|Uint8Array} reader Reader or buffer to decode from
             * @returns {mirabuf.motor.DCMotor} DCMotor
             * @throws {Error} If the payload is not a reader or valid buffer
             * @throws {$protobuf.util.ProtocolError} If required fields are missing
             */
            DCMotor.decodeDelimited = function decodeDelimited(reader) {
                if (!(reader instanceof $Reader)) reader = new $Reader(reader)
                return this.decode(reader, reader.uint32())
            }

            /**
             * Verifies a DCMotor message.
             * @function verify
             * @memberof mirabuf.motor.DCMotor
             * @static
             * @param {Object.<string,*>} message Plain object to verify
             * @returns {string|null} `null` if valid, otherwise the reason why it is not
             */
            DCMotor.verify = function verify(message) {
                if (typeof message !== "object" || message === null) return "object expected"
                if (message.referenceUrl != null && Object.hasOwn(message, "referenceUrl"))
                    if (!$util.isString(message.referenceUrl)) return "referenceUrl: string expected"
                if (message.torqueConstant != null && Object.hasOwn(message, "torqueConstant"))
                    if (typeof message.torqueConstant !== "number") return "torqueConstant: number expected"
                if (message.emfConstant != null && Object.hasOwn(message, "emfConstant"))
                    if (typeof message.emfConstant !== "number") return "emfConstant: number expected"
                if (message.resistance != null && Object.hasOwn(message, "resistance"))
                    if (typeof message.resistance !== "number") return "resistance: number expected"
                if (message.maximumEffeciency != null && Object.hasOwn(message, "maximumEffeciency"))
                    if (!$util.isInteger(message.maximumEffeciency)) return "maximumEffeciency: integer expected"
                if (message.maximumPower != null && Object.hasOwn(message, "maximumPower"))
                    if (!$util.isInteger(message.maximumPower)) return "maximumPower: integer expected"
                if (message.dutyCycle != null && Object.hasOwn(message, "dutyCycle"))
                    switch (message.dutyCycle) {
                        default:
                            return "dutyCycle: enum value expected"
                        case 0:
                        case 1:
                        case 2:
                        case 3:
                            break
                    }
                if (message.advanced != null && Object.hasOwn(message, "advanced")) {
                    let error = $root.mirabuf.motor.DCMotor.Advanced.verify(message.advanced)
                    if (error) return "advanced." + error
                }
                return null
            }

            /**
             * Creates a DCMotor message from a plain object. Also converts values to their respective internal types.
             * @function fromObject
             * @memberof mirabuf.motor.DCMotor
             * @static
             * @param {Object.<string,*>} object Plain object
             * @returns {mirabuf.motor.DCMotor} DCMotor
             */
            DCMotor.fromObject = function fromObject(object) {
                if (object instanceof $root.mirabuf.motor.DCMotor) return object
                let message = new $root.mirabuf.motor.DCMotor()
                if (object.referenceUrl != null) message.referenceUrl = String(object.referenceUrl)
                if (object.torqueConstant != null) message.torqueConstant = Number(object.torqueConstant)
                if (object.emfConstant != null) message.emfConstant = Number(object.emfConstant)
                if (object.resistance != null) message.resistance = Number(object.resistance)
                if (object.maximumEffeciency != null) message.maximumEffeciency = object.maximumEffeciency >>> 0
                if (object.maximumPower != null) message.maximumPower = object.maximumPower >>> 0
                switch (object.dutyCycle) {
                    default:
                        if (typeof object.dutyCycle === "number") {
                            message.dutyCycle = object.dutyCycle
                            break
                        }
                        break
                    case "CONTINUOUS_RUNNING":
                    case 0:
                        message.dutyCycle = 0
                        break
                    case "SHORT_TIME":
                    case 1:
                        message.dutyCycle = 1
                        break
                    case "INTERMITTENT_PERIODIC":
                    case 2:
                        message.dutyCycle = 2
                        break
                    case "CONTINUOUS_PERIODIC":
                    case 3:
                        message.dutyCycle = 3
                        break
                }
                if (object.advanced != null) {
                    if (typeof object.advanced !== "object")
                        throw TypeError(".mirabuf.motor.DCMotor.advanced: object expected")
                    message.advanced = $root.mirabuf.motor.DCMotor.Advanced.fromObject(object.advanced)
                }
                return message
            }

            /**
             * Creates a plain object from a DCMotor message. Also converts values to other types if specified.
             * @function toObject
             * @memberof mirabuf.motor.DCMotor
             * @static
             * @param {mirabuf.motor.DCMotor} message DCMotor
             * @param {$protobuf.IConversionOptions} [options] Conversion options
             * @returns {Object.<string,*>} Plain object
             */
            DCMotor.toObject = function toObject(message, options) {
                if (!options) options = {}
                let object = {}
                if (options.defaults) {
                    object.referenceUrl = ""
                    object.torqueConstant = 0
                    object.emfConstant = 0
                    object.resistance = 0
                    object.maximumEffeciency = 0
                    object.maximumPower = 0
                    object.dutyCycle = options.enums === String ? "CONTINUOUS_RUNNING" : 0
                    object.advanced = null
                }
                if (message.referenceUrl != null && Object.hasOwn(message, "referenceUrl"))
                    object.referenceUrl = message.referenceUrl
                if (message.torqueConstant != null && Object.hasOwn(message, "torqueConstant"))
                    object.torqueConstant =
                        options.json && !isFinite(message.torqueConstant)
                            ? String(message.torqueConstant)
                            : message.torqueConstant
                if (message.emfConstant != null && Object.hasOwn(message, "emfConstant"))
                    object.emfConstant =
                        options.json && !isFinite(message.emfConstant)
                            ? String(message.emfConstant)
                            : message.emfConstant
                if (message.resistance != null && Object.hasOwn(message, "resistance"))
                    object.resistance =
                        options.json && !isFinite(message.resistance) ? String(message.resistance) : message.resistance
                if (message.maximumEffeciency != null && Object.hasOwn(message, "maximumEffeciency"))
                    object.maximumEffeciency = message.maximumEffeciency
                if (message.maximumPower != null && Object.hasOwn(message, "maximumPower"))
                    object.maximumPower = message.maximumPower
                if (message.dutyCycle != null && Object.hasOwn(message, "dutyCycle"))
                    object.dutyCycle =
                        options.enums === String
                            ? $root.mirabuf.motor.DutyCycles[message.dutyCycle] === undefined
                                ? message.dutyCycle
                                : $root.mirabuf.motor.DutyCycles[message.dutyCycle]
                            : message.dutyCycle
                if (message.advanced != null && Object.hasOwn(message, "advanced"))
                    object.advanced = $root.mirabuf.motor.DCMotor.Advanced.toObject(message.advanced, options)
                return object
            }

            /**
             * Converts this DCMotor to JSON.
             * @function toJSON
             * @memberof mirabuf.motor.DCMotor
             * @instance
             * @returns {Object.<string,*>} JSON object
             */
            DCMotor.prototype.toJSON = function toJSON() {
                return this.constructor.toObject(this, $protobuf.util.toJSONOptions)
            }

            /**
             * Gets the default type url for DCMotor
             * @function getTypeUrl
             * @memberof mirabuf.motor.DCMotor
             * @static
             * @param {string} [typeUrlPrefix] your custom typeUrlPrefix(default "type.googleapis.com")
             * @returns {string} The default type url
             */
            DCMotor.getTypeUrl = function getTypeUrl(typeUrlPrefix) {
                if (typeUrlPrefix === undefined) {
                    typeUrlPrefix = "type.googleapis.com"
                }
                return typeUrlPrefix + "/mirabuf.motor.DCMotor"
            }

            DCMotor.Advanced = (function () {
                /**
                 * Properties of an Advanced.
                 * @memberof mirabuf.motor.DCMotor
                 * @interface IAdvanced
                 * @property {number|null} [freeCurrent] measured in AMPs
                 * @property {number|null} [freeSpeed] measured in RPM
                 * @property {number|null} [stallCurrent] measure in AMPs
                 * @property {number|null} [stallTorque] measured in Nm
                 * @property {number|null} [inputVoltage] measured in Volts DC
                 * @property {number|null} [resistanceVariation] between (K * (N / 4)) and (K * ((N-2) / 4)) where N is number of poles - leave at 0 if unknown
                 */

                /**
                 * Constructs a new Advanced.
                 * @memberof mirabuf.motor.DCMotor
                 * @classdesc Information usually found on datasheet
                 * @implements IAdvanced
                 * @constructor
                 * @param {mirabuf.motor.DCMotor.IAdvanced=} [properties] Properties to set
                 */
                function Advanced(properties) {
                    if (properties)
                        for (let keys = Object.keys(properties), i = 0; i < keys.length; ++i)
                            if (properties[keys[i]] != null) this[keys[i]] = properties[keys[i]]
                }

                /**
                 * measured in AMPs
                 * @member {number} freeCurrent
                 * @memberof mirabuf.motor.DCMotor.Advanced
                 * @instance
                 */
                Advanced.prototype.freeCurrent = 0

                /**
                 * measured in RPM
                 * @member {number} freeSpeed
                 * @memberof mirabuf.motor.DCMotor.Advanced
                 * @instance
                 */
                Advanced.prototype.freeSpeed = 0

                /**
                 * measure in AMPs
                 * @member {number} stallCurrent
                 * @memberof mirabuf.motor.DCMotor.Advanced
                 * @instance
                 */
                Advanced.prototype.stallCurrent = 0

                /**
                 * measured in Nm
                 * @member {number} stallTorque
                 * @memberof mirabuf.motor.DCMotor.Advanced
                 * @instance
                 */
                Advanced.prototype.stallTorque = 0

                /**
                 * measured in Volts DC
                 * @member {number} inputVoltage
                 * @memberof mirabuf.motor.DCMotor.Advanced
                 * @instance
                 */
                Advanced.prototype.inputVoltage = 0

                /**
                 * between (K * (N / 4)) and (K * ((N-2) / 4)) where N is number of poles - leave at 0 if unknown
                 * @member {number} resistanceVariation
                 * @memberof mirabuf.motor.DCMotor.Advanced
                 * @instance
                 */
                Advanced.prototype.resistanceVariation = 0

                /**
                 * Creates a new Advanced instance using the specified properties.
                 * @function create
                 * @memberof mirabuf.motor.DCMotor.Advanced
                 * @static
                 * @param {mirabuf.motor.DCMotor.IAdvanced=} [properties] Properties to set
                 * @returns {mirabuf.motor.DCMotor.Advanced} Advanced instance
                 */
                Advanced.create = function create(properties) {
                    return new Advanced(properties)
                }

                /**
                 * Encodes the specified Advanced message. Does not implicitly {@link mirabuf.motor.DCMotor.Advanced.verify|verify} messages.
                 * @function encode
                 * @memberof mirabuf.motor.DCMotor.Advanced
                 * @static
                 * @param {mirabuf.motor.DCMotor.IAdvanced} message Advanced message or plain object to encode
                 * @param {$protobuf.Writer} [writer] Writer to encode to
                 * @returns {$protobuf.Writer} Writer
                 */
                Advanced.encode = function encode(message, writer) {
                    if (!writer) writer = $Writer.create()
                    if (message.freeCurrent != null && Object.hasOwn(message, "freeCurrent"))
                        writer.uint32(/* id 1, wireType 5 =*/ 13).float(message.freeCurrent)
                    if (message.freeSpeed != null && Object.hasOwn(message, "freeSpeed"))
                        writer.uint32(/* id 2, wireType 0 =*/ 16).uint32(message.freeSpeed)
                    if (message.stallCurrent != null && Object.hasOwn(message, "stallCurrent"))
                        writer.uint32(/* id 3, wireType 5 =*/ 29).float(message.stallCurrent)
                    if (message.stallTorque != null && Object.hasOwn(message, "stallTorque"))
                        writer.uint32(/* id 4, wireType 5 =*/ 37).float(message.stallTorque)
                    if (message.inputVoltage != null && Object.hasOwn(message, "inputVoltage"))
                        writer.uint32(/* id 5, wireType 0 =*/ 40).uint32(message.inputVoltage)
                    if (message.resistanceVariation != null && Object.hasOwn(message, "resistanceVariation"))
                        writer.uint32(/* id 7, wireType 5 =*/ 61).float(message.resistanceVariation)
                    return writer
                }

                /**
                 * Encodes the specified Advanced message, length delimited. Does not implicitly {@link mirabuf.motor.DCMotor.Advanced.verify|verify} messages.
                 * @function encodeDelimited
                 * @memberof mirabuf.motor.DCMotor.Advanced
                 * @static
                 * @param {mirabuf.motor.DCMotor.IAdvanced} message Advanced message or plain object to encode
                 * @param {$protobuf.Writer} [writer] Writer to encode to
                 * @returns {$protobuf.Writer} Writer
                 */
                Advanced.encodeDelimited = function encodeDelimited(message, writer) {
                    return this.encode(message, writer).ldelim()
                }

                /**
                 * Decodes an Advanced message from the specified reader or buffer.
                 * @function decode
                 * @memberof mirabuf.motor.DCMotor.Advanced
                 * @static
                 * @param {$protobuf.Reader|Uint8Array} reader Reader or buffer to decode from
                 * @param {number} [length] Message length if known beforehand
                 * @returns {mirabuf.motor.DCMotor.Advanced} Advanced
                 * @throws {Error} If the payload is not a reader or valid buffer
                 * @throws {$protobuf.util.ProtocolError} If required fields are missing
                 */
                Advanced.decode = function decode(reader, length) {
                    if (!(reader instanceof $Reader)) reader = $Reader.create(reader)
                    let end = length === undefined ? reader.len : reader.pos + length,
                        message = new $root.mirabuf.motor.DCMotor.Advanced()
                    while (reader.pos < end) {
                        let tag = reader.uint32()
                        switch (tag >>> 3) {
                            case 1: {
                                message.freeCurrent = reader.float()
                                break
                            }
                            case 2: {
                                message.freeSpeed = reader.uint32()
                                break
                            }
                            case 3: {
                                message.stallCurrent = reader.float()
                                break
                            }
                            case 4: {
                                message.stallTorque = reader.float()
                                break
                            }
                            case 5: {
                                message.inputVoltage = reader.uint32()
                                break
                            }
                            case 7: {
                                message.resistanceVariation = reader.float()
                                break
                            }
                            default:
                                reader.skipType(tag & 7)
                                break
                        }
                    }
                    return message
                }

                /**
                 * Decodes an Advanced message from the specified reader or buffer, length delimited.
                 * @function decodeDelimited
                 * @memberof mirabuf.motor.DCMotor.Advanced
                 * @static
                 * @param {$protobuf.Reader|Uint8Array} reader Reader or buffer to decode from
                 * @returns {mirabuf.motor.DCMotor.Advanced} Advanced
                 * @throws {Error} If the payload is not a reader or valid buffer
                 * @throws {$protobuf.util.ProtocolError} If required fields are missing
                 */
                Advanced.decodeDelimited = function decodeDelimited(reader) {
                    if (!(reader instanceof $Reader)) reader = new $Reader(reader)
                    return this.decode(reader, reader.uint32())
                }

                /**
                 * Verifies an Advanced message.
                 * @function verify
                 * @memberof mirabuf.motor.DCMotor.Advanced
                 * @static
                 * @param {Object.<string,*>} message Plain object to verify
                 * @returns {string|null} `null` if valid, otherwise the reason why it is not
                 */
                Advanced.verify = function verify(message) {
                    if (typeof message !== "object" || message === null) return "object expected"
                    if (message.freeCurrent != null && Object.hasOwn(message, "freeCurrent"))
                        if (typeof message.freeCurrent !== "number") return "freeCurrent: number expected"
                    if (message.freeSpeed != null && Object.hasOwn(message, "freeSpeed"))
                        if (!$util.isInteger(message.freeSpeed)) return "freeSpeed: integer expected"
                    if (message.stallCurrent != null && Object.hasOwn(message, "stallCurrent"))
                        if (typeof message.stallCurrent !== "number") return "stallCurrent: number expected"
                    if (message.stallTorque != null && Object.hasOwn(message, "stallTorque"))
                        if (typeof message.stallTorque !== "number") return "stallTorque: number expected"
                    if (message.inputVoltage != null && Object.hasOwn(message, "inputVoltage"))
                        if (!$util.isInteger(message.inputVoltage)) return "inputVoltage: integer expected"
                    if (message.resistanceVariation != null && Object.hasOwn(message, "resistanceVariation"))
                        if (typeof message.resistanceVariation !== "number")
                            return "resistanceVariation: number expected"
                    return null
                }

                /**
                 * Creates an Advanced message from a plain object. Also converts values to their respective internal types.
                 * @function fromObject
                 * @memberof mirabuf.motor.DCMotor.Advanced
                 * @static
                 * @param {Object.<string,*>} object Plain object
                 * @returns {mirabuf.motor.DCMotor.Advanced} Advanced
                 */
                Advanced.fromObject = function fromObject(object) {
                    if (object instanceof $root.mirabuf.motor.DCMotor.Advanced) return object
                    let message = new $root.mirabuf.motor.DCMotor.Advanced()
                    if (object.freeCurrent != null) message.freeCurrent = Number(object.freeCurrent)
                    if (object.freeSpeed != null) message.freeSpeed = object.freeSpeed >>> 0
                    if (object.stallCurrent != null) message.stallCurrent = Number(object.stallCurrent)
                    if (object.stallTorque != null) message.stallTorque = Number(object.stallTorque)
                    if (object.inputVoltage != null) message.inputVoltage = object.inputVoltage >>> 0
                    if (object.resistanceVariation != null)
                        message.resistanceVariation = Number(object.resistanceVariation)
                    return message
                }

                /**
                 * Creates a plain object from an Advanced message. Also converts values to other types if specified.
                 * @function toObject
                 * @memberof mirabuf.motor.DCMotor.Advanced
                 * @static
                 * @param {mirabuf.motor.DCMotor.Advanced} message Advanced
                 * @param {$protobuf.IConversionOptions} [options] Conversion options
                 * @returns {Object.<string,*>} Plain object
                 */
                Advanced.toObject = function toObject(message, options) {
                    if (!options) options = {}
                    let object = {}
                    if (options.defaults) {
                        object.freeCurrent = 0
                        object.freeSpeed = 0
                        object.stallCurrent = 0
                        object.stallTorque = 0
                        object.inputVoltage = 0
                        object.resistanceVariation = 0
                    }
                    if (message.freeCurrent != null && Object.hasOwn(message, "freeCurrent"))
                        object.freeCurrent =
                            options.json && !isFinite(message.freeCurrent)
                                ? String(message.freeCurrent)
                                : message.freeCurrent
                    if (message.freeSpeed != null && Object.hasOwn(message, "freeSpeed"))
                        object.freeSpeed = message.freeSpeed
                    if (message.stallCurrent != null && Object.hasOwn(message, "stallCurrent"))
                        object.stallCurrent =
                            options.json && !isFinite(message.stallCurrent)
                                ? String(message.stallCurrent)
                                : message.stallCurrent
                    if (message.stallTorque != null && Object.hasOwn(message, "stallTorque"))
                        object.stallTorque =
                            options.json && !isFinite(message.stallTorque)
                                ? String(message.stallTorque)
                                : message.stallTorque
                    if (message.inputVoltage != null && Object.hasOwn(message, "inputVoltage"))
                        object.inputVoltage = message.inputVoltage
                    if (message.resistanceVariation != null && Object.hasOwn(message, "resistanceVariation"))
                        object.resistanceVariation =
                            options.json && !isFinite(message.resistanceVariation)
                                ? String(message.resistanceVariation)
                                : message.resistanceVariation
                    return object
                }

                /**
                 * Converts this Advanced to JSON.
                 * @function toJSON
                 * @memberof mirabuf.motor.DCMotor.Advanced
                 * @instance
                 * @returns {Object.<string,*>} JSON object
                 */
                Advanced.prototype.toJSON = function toJSON() {
                    return this.constructor.toObject(this, $protobuf.util.toJSONOptions)
                }

                /**
                 * Gets the default type url for Advanced
                 * @function getTypeUrl
                 * @memberof mirabuf.motor.DCMotor.Advanced
                 * @static
                 * @param {string} [typeUrlPrefix] your custom typeUrlPrefix(default "type.googleapis.com")
                 * @returns {string} The default type url
                 */
                Advanced.getTypeUrl = function getTypeUrl(typeUrlPrefix) {
                    if (typeUrlPrefix === undefined) {
                        typeUrlPrefix = "type.googleapis.com"
                    }
                    return typeUrlPrefix + "/mirabuf.motor.DCMotor.Advanced"
                }

                return Advanced
            })()

            return DCMotor
        })()

        return motor
    })()

    mirabuf.material = (function () {
        /**
         * Namespace material.
         * @memberof mirabuf
         * @namespace
         */
        const material = {}

        material.Materials = (function () {
            /**
             * Properties of a Materials.
             * @memberof mirabuf.material
             * @interface IMaterials
             * @property {mirabuf.IInfo|null} [info] Identifiable information (id, name, version)
             * @property {Object.<string,mirabuf.material.IPhysicalMaterial>|null} [physicalMaterials] Map of Physical Materials
             * @property {Object.<string,mirabuf.material.IAppearance>|null} [appearances] Map of Appearances that are purely visual
             */

            /**
             * Constructs a new Materials.
             * @memberof mirabuf.material
             * @classdesc Represents a File or Set of Materials with Appearances and Physical Data
             *
             * Can be Stored in AssemblyData
             * @implements IMaterials
             * @constructor
             * @param {mirabuf.material.IMaterials=} [properties] Properties to set
             */
            function Materials(properties) {
                this.physicalMaterials = {}
                this.appearances = {}
                if (properties)
                    for (let keys = Object.keys(properties), i = 0; i < keys.length; ++i)
                        if (properties[keys[i]] != null) this[keys[i]] = properties[keys[i]]
            }

            /**
             * Identifiable information (id, name, version)
             * @member {mirabuf.IInfo|null|undefined} info
             * @memberof mirabuf.material.Materials
             * @instance
             */
            Materials.prototype.info = null

            /**
             * Map of Physical Materials
             * @member {Object.<string,mirabuf.material.IPhysicalMaterial>} physicalMaterials
             * @memberof mirabuf.material.Materials
             * @instance
             */
            Materials.prototype.physicalMaterials = $util.emptyObject

            /**
             * Map of Appearances that are purely visual
             * @member {Object.<string,mirabuf.material.IAppearance>} appearances
             * @memberof mirabuf.material.Materials
             * @instance
             */
            Materials.prototype.appearances = $util.emptyObject

            /**
             * Creates a new Materials instance using the specified properties.
             * @function create
             * @memberof mirabuf.material.Materials
             * @static
             * @param {mirabuf.material.IMaterials=} [properties] Properties to set
             * @returns {mirabuf.material.Materials} Materials instance
             */
            Materials.create = function create(properties) {
                return new Materials(properties)
            }

            /**
             * Encodes the specified Materials message. Does not implicitly {@link mirabuf.material.Materials.verify|verify} messages.
             * @function encode
             * @memberof mirabuf.material.Materials
             * @static
             * @param {mirabuf.material.IMaterials} message Materials message or plain object to encode
             * @param {$protobuf.Writer} [writer] Writer to encode to
             * @returns {$protobuf.Writer} Writer
             */
            Materials.encode = function encode(message, writer) {
                if (!writer) writer = $Writer.create()
                if (message.info != null && Object.hasOwn(message, "info"))
                    $root.mirabuf.Info.encode(message.info, writer.uint32(/* id 1, wireType 2 =*/ 10).fork()).ldelim()
                if (message.physicalMaterials != null && Object.hasOwn(message, "physicalMaterials"))
                    for (let keys = Object.keys(message.physicalMaterials), i = 0; i < keys.length; ++i) {
                        writer
                            .uint32(/* id 2, wireType 2 =*/ 18)
                            .fork()
                            .uint32(/* id 1, wireType 2 =*/ 10)
                            .string(keys[i])
                        $root.mirabuf.material.PhysicalMaterial.encode(
                            message.physicalMaterials[keys[i]],
                            writer.uint32(/* id 2, wireType 2 =*/ 18).fork()
                        )
                            .ldelim()
                            .ldelim()
                    }
                if (message.appearances != null && Object.hasOwn(message, "appearances"))
                    for (let keys = Object.keys(message.appearances), i = 0; i < keys.length; ++i) {
                        writer
                            .uint32(/* id 3, wireType 2 =*/ 26)
                            .fork()
                            .uint32(/* id 1, wireType 2 =*/ 10)
                            .string(keys[i])
                        $root.mirabuf.material.Appearance.encode(
                            message.appearances[keys[i]],
                            writer.uint32(/* id 2, wireType 2 =*/ 18).fork()
                        )
                            .ldelim()
                            .ldelim()
                    }
                return writer
            }

            /**
             * Encodes the specified Materials message, length delimited. Does not implicitly {@link mirabuf.material.Materials.verify|verify} messages.
             * @function encodeDelimited
             * @memberof mirabuf.material.Materials
             * @static
             * @param {mirabuf.material.IMaterials} message Materials message or plain object to encode
             * @param {$protobuf.Writer} [writer] Writer to encode to
             * @returns {$protobuf.Writer} Writer
             */
            Materials.encodeDelimited = function encodeDelimited(message, writer) {
                return this.encode(message, writer).ldelim()
            }

            /**
             * Decodes a Materials message from the specified reader or buffer.
             * @function decode
             * @memberof mirabuf.material.Materials
             * @static
             * @param {$protobuf.Reader|Uint8Array} reader Reader or buffer to decode from
             * @param {number} [length] Message length if known beforehand
             * @returns {mirabuf.material.Materials} Materials
             * @throws {Error} If the payload is not a reader or valid buffer
             * @throws {$protobuf.util.ProtocolError} If required fields are missing
             */
            Materials.decode = function decode(reader, length) {
                if (!(reader instanceof $Reader)) reader = $Reader.create(reader)
                let end = length === undefined ? reader.len : reader.pos + length,
                    message = new $root.mirabuf.material.Materials(),
                    key,
                    value
                while (reader.pos < end) {
                    let tag = reader.uint32()
                    switch (tag >>> 3) {
                        case 1: {
                            message.info = $root.mirabuf.Info.decode(reader, reader.uint32())
                            break
                        }
                        case 2: {
                            if (message.physicalMaterials === $util.emptyObject) message.physicalMaterials = {}
                            let end2 = reader.uint32() + reader.pos
                            key = ""
                            value = null
                            while (reader.pos < end2) {
                                let tag2 = reader.uint32()
                                switch (tag2 >>> 3) {
                                    case 1:
                                        key = reader.string()
                                        break
                                    case 2:
                                        value = $root.mirabuf.material.PhysicalMaterial.decode(reader, reader.uint32())
                                        break
                                    default:
                                        reader.skipType(tag2 & 7)
                                        break
                                }
                            }
                            message.physicalMaterials[key] = value
                            break
                        }
                        case 3: {
                            if (message.appearances === $util.emptyObject) message.appearances = {}
                            let end2 = reader.uint32() + reader.pos
                            key = ""
                            value = null
                            while (reader.pos < end2) {
                                let tag2 = reader.uint32()
                                switch (tag2 >>> 3) {
                                    case 1:
                                        key = reader.string()
                                        break
                                    case 2:
                                        value = $root.mirabuf.material.Appearance.decode(reader, reader.uint32())
                                        break
                                    default:
                                        reader.skipType(tag2 & 7)
                                        break
                                }
                            }
                            message.appearances[key] = value
                            break
                        }
                        default:
                            reader.skipType(tag & 7)
                            break
                    }
                }
                return message
            }

            /**
             * Decodes a Materials message from the specified reader or buffer, length delimited.
             * @function decodeDelimited
             * @memberof mirabuf.material.Materials
             * @static
             * @param {$protobuf.Reader|Uint8Array} reader Reader or buffer to decode from
             * @returns {mirabuf.material.Materials} Materials
             * @throws {Error} If the payload is not a reader or valid buffer
             * @throws {$protobuf.util.ProtocolError} If required fields are missing
             */
            Materials.decodeDelimited = function decodeDelimited(reader) {
                if (!(reader instanceof $Reader)) reader = new $Reader(reader)
                return this.decode(reader, reader.uint32())
            }

            /**
             * Verifies a Materials message.
             * @function verify
             * @memberof mirabuf.material.Materials
             * @static
             * @param {Object.<string,*>} message Plain object to verify
             * @returns {string|null} `null` if valid, otherwise the reason why it is not
             */
            Materials.verify = function verify(message) {
                if (typeof message !== "object" || message === null) return "object expected"
                if (message.info != null && Object.hasOwn(message, "info")) {
                    let error = $root.mirabuf.Info.verify(message.info)
                    if (error) return "info." + error
                }
                if (message.physicalMaterials != null && Object.hasOwn(message, "physicalMaterials")) {
                    if (!$util.isObject(message.physicalMaterials)) return "physicalMaterials: object expected"
                    let key = Object.keys(message.physicalMaterials)
                    for (let i = 0; i < key.length; ++i) {
                        let error = $root.mirabuf.material.PhysicalMaterial.verify(message.physicalMaterials[key[i]])
                        if (error) return "physicalMaterials." + error
                    }
                }
                if (message.appearances != null && Object.hasOwn(message, "appearances")) {
                    if (!$util.isObject(message.appearances)) return "appearances: object expected"
                    let key = Object.keys(message.appearances)
                    for (let i = 0; i < key.length; ++i) {
                        let error = $root.mirabuf.material.Appearance.verify(message.appearances[key[i]])
                        if (error) return "appearances." + error
                    }
                }
                return null
            }

            /**
             * Creates a Materials message from a plain object. Also converts values to their respective internal types.
             * @function fromObject
             * @memberof mirabuf.material.Materials
             * @static
             * @param {Object.<string,*>} object Plain object
             * @returns {mirabuf.material.Materials} Materials
             */
            Materials.fromObject = function fromObject(object) {
                if (object instanceof $root.mirabuf.material.Materials) return object
                let message = new $root.mirabuf.material.Materials()
                if (object.info != null) {
                    if (typeof object.info !== "object")
                        throw TypeError(".mirabuf.material.Materials.info: object expected")
                    message.info = $root.mirabuf.Info.fromObject(object.info)
                }
                if (object.physicalMaterials) {
                    if (typeof object.physicalMaterials !== "object")
                        throw TypeError(".mirabuf.material.Materials.physicalMaterials: object expected")
                    message.physicalMaterials = {}
                    for (let keys = Object.keys(object.physicalMaterials), i = 0; i < keys.length; ++i) {
                        if (typeof object.physicalMaterials[keys[i]] !== "object")
                            throw TypeError(".mirabuf.material.Materials.physicalMaterials: object expected")
                        message.physicalMaterials[keys[i]] = $root.mirabuf.material.PhysicalMaterial.fromObject(
                            object.physicalMaterials[keys[i]]
                        )
                    }
                }
                if (object.appearances) {
                    if (typeof object.appearances !== "object")
                        throw TypeError(".mirabuf.material.Materials.appearances: object expected")
                    message.appearances = {}
                    for (let keys = Object.keys(object.appearances), i = 0; i < keys.length; ++i) {
                        if (typeof object.appearances[keys[i]] !== "object")
                            throw TypeError(".mirabuf.material.Materials.appearances: object expected")
                        message.appearances[keys[i]] = $root.mirabuf.material.Appearance.fromObject(
                            object.appearances[keys[i]]
                        )
                    }
                }
                return message
            }

            /**
             * Creates a plain object from a Materials message. Also converts values to other types if specified.
             * @function toObject
             * @memberof mirabuf.material.Materials
             * @static
             * @param {mirabuf.material.Materials} message Materials
             * @param {$protobuf.IConversionOptions} [options] Conversion options
             * @returns {Object.<string,*>} Plain object
             */
            Materials.toObject = function toObject(message, options) {
                if (!options) options = {}
                let object = {}
                if (options.objects || options.defaults) {
                    object.physicalMaterials = {}
                    object.appearances = {}
                }
                if (options.defaults) object.info = null
                if (message.info != null && Object.hasOwn(message, "info"))
                    object.info = $root.mirabuf.Info.toObject(message.info, options)
                let keys2
                if (message.physicalMaterials && (keys2 = Object.keys(message.physicalMaterials)).length) {
                    object.physicalMaterials = {}
                    for (let j = 0; j < keys2.length; ++j)
                        object.physicalMaterials[keys2[j]] = $root.mirabuf.material.PhysicalMaterial.toObject(
                            message.physicalMaterials[keys2[j]],
                            options
                        )
                }
                if (message.appearances && (keys2 = Object.keys(message.appearances)).length) {
                    object.appearances = {}
                    for (let j = 0; j < keys2.length; ++j)
                        object.appearances[keys2[j]] = $root.mirabuf.material.Appearance.toObject(
                            message.appearances[keys2[j]],
                            options
                        )
                }
                return object
            }

            /**
             * Converts this Materials to JSON.
             * @function toJSON
             * @memberof mirabuf.material.Materials
             * @instance
             * @returns {Object.<string,*>} JSON object
             */
            Materials.prototype.toJSON = function toJSON() {
                return this.constructor.toObject(this, $protobuf.util.toJSONOptions)
            }

            /**
             * Gets the default type url for Materials
             * @function getTypeUrl
             * @memberof mirabuf.material.Materials
             * @static
             * @param {string} [typeUrlPrefix] your custom typeUrlPrefix(default "type.googleapis.com")
             * @returns {string} The default type url
             */
            Materials.getTypeUrl = function getTypeUrl(typeUrlPrefix) {
                if (typeUrlPrefix === undefined) {
                    typeUrlPrefix = "type.googleapis.com"
                }
                return typeUrlPrefix + "/mirabuf.material.Materials"
            }

            return Materials
        })()

        material.Appearance = (function () {
            /**
             * Properties of an Appearance.
             * @memberof mirabuf.material
             * @interface IAppearance
             * @property {mirabuf.IInfo|null} [info] Identfiable information (id, name, version)
             * @property {mirabuf.IColor|null} [albedo] albedo map RGBA 0-255
             * @property {number|null} [roughness] roughness value 0-1
             * @property {number|null} [metallic] metallic value 0-1
             * @property {number|null} [specular] specular value 0-1
             */

            /**
             * Constructs a new Appearance.
             * @memberof mirabuf.material
             * @classdesc Contains information on how a object looks
             * Limited to just color for now
             * @implements IAppearance
             * @constructor
             * @param {mirabuf.material.IAppearance=} [properties] Properties to set
             */
            function Appearance(properties) {
                if (properties)
                    for (let keys = Object.keys(properties), i = 0; i < keys.length; ++i)
                        if (properties[keys[i]] != null) this[keys[i]] = properties[keys[i]]
            }

            /**
             * Identfiable information (id, name, version)
             * @member {mirabuf.IInfo|null|undefined} info
             * @memberof mirabuf.material.Appearance
             * @instance
             */
            Appearance.prototype.info = null

            /**
             * albedo map RGBA 0-255
             * @member {mirabuf.IColor|null|undefined} albedo
             * @memberof mirabuf.material.Appearance
             * @instance
             */
            Appearance.prototype.albedo = null

            /**
             * roughness value 0-1
             * @member {number} roughness
             * @memberof mirabuf.material.Appearance
             * @instance
             */
            Appearance.prototype.roughness = 0

            /**
             * metallic value 0-1
             * @member {number} metallic
             * @memberof mirabuf.material.Appearance
             * @instance
             */
            Appearance.prototype.metallic = 0

            /**
             * specular value 0-1
             * @member {number} specular
             * @memberof mirabuf.material.Appearance
             * @instance
             */
            Appearance.prototype.specular = 0

            /**
             * Creates a new Appearance instance using the specified properties.
             * @function create
             * @memberof mirabuf.material.Appearance
             * @static
             * @param {mirabuf.material.IAppearance=} [properties] Properties to set
             * @returns {mirabuf.material.Appearance} Appearance instance
             */
            Appearance.create = function create(properties) {
                return new Appearance(properties)
            }

            /**
             * Encodes the specified Appearance message. Does not implicitly {@link mirabuf.material.Appearance.verify|verify} messages.
             * @function encode
             * @memberof mirabuf.material.Appearance
             * @static
             * @param {mirabuf.material.IAppearance} message Appearance message or plain object to encode
             * @param {$protobuf.Writer} [writer] Writer to encode to
             * @returns {$protobuf.Writer} Writer
             */
            Appearance.encode = function encode(message, writer) {
                if (!writer) writer = $Writer.create()
                if (message.info != null && Object.hasOwn(message, "info"))
                    $root.mirabuf.Info.encode(message.info, writer.uint32(/* id 1, wireType 2 =*/ 10).fork()).ldelim()
                if (message.albedo != null && Object.hasOwn(message, "albedo"))
                    $root.mirabuf.Color.encode(
                        message.albedo,
                        writer.uint32(/* id 2, wireType 2 =*/ 18).fork()
                    ).ldelim()
                if (message.roughness != null && Object.hasOwn(message, "roughness"))
                    writer.uint32(/* id 3, wireType 1 =*/ 25).double(message.roughness)
                if (message.metallic != null && Object.hasOwn(message, "metallic"))
                    writer.uint32(/* id 4, wireType 1 =*/ 33).double(message.metallic)
                if (message.specular != null && Object.hasOwn(message, "specular"))
                    writer.uint32(/* id 5, wireType 1 =*/ 41).double(message.specular)
                return writer
            }

            /**
             * Encodes the specified Appearance message, length delimited. Does not implicitly {@link mirabuf.material.Appearance.verify|verify} messages.
             * @function encodeDelimited
             * @memberof mirabuf.material.Appearance
             * @static
             * @param {mirabuf.material.IAppearance} message Appearance message or plain object to encode
             * @param {$protobuf.Writer} [writer] Writer to encode to
             * @returns {$protobuf.Writer} Writer
             */
            Appearance.encodeDelimited = function encodeDelimited(message, writer) {
                return this.encode(message, writer).ldelim()
            }

            /**
             * Decodes an Appearance message from the specified reader or buffer.
             * @function decode
             * @memberof mirabuf.material.Appearance
             * @static
             * @param {$protobuf.Reader|Uint8Array} reader Reader or buffer to decode from
             * @param {number} [length] Message length if known beforehand
             * @returns {mirabuf.material.Appearance} Appearance
             * @throws {Error} If the payload is not a reader or valid buffer
             * @throws {$protobuf.util.ProtocolError} If required fields are missing
             */
            Appearance.decode = function decode(reader, length) {
                if (!(reader instanceof $Reader)) reader = $Reader.create(reader)
                let end = length === undefined ? reader.len : reader.pos + length,
                    message = new $root.mirabuf.material.Appearance()
                while (reader.pos < end) {
                    let tag = reader.uint32()
                    switch (tag >>> 3) {
                        case 1: {
                            message.info = $root.mirabuf.Info.decode(reader, reader.uint32())
                            break
                        }
                        case 2: {
                            message.albedo = $root.mirabuf.Color.decode(reader, reader.uint32())
                            break
                        }
                        case 3: {
                            message.roughness = reader.double()
                            break
                        }
                        case 4: {
                            message.metallic = reader.double()
                            break
                        }
                        case 5: {
                            message.specular = reader.double()
                            break
                        }
                        default:
                            reader.skipType(tag & 7)
                            break
                    }
                }
                return message
            }

            /**
             * Decodes an Appearance message from the specified reader or buffer, length delimited.
             * @function decodeDelimited
             * @memberof mirabuf.material.Appearance
             * @static
             * @param {$protobuf.Reader|Uint8Array} reader Reader or buffer to decode from
             * @returns {mirabuf.material.Appearance} Appearance
             * @throws {Error} If the payload is not a reader or valid buffer
             * @throws {$protobuf.util.ProtocolError} If required fields are missing
             */
            Appearance.decodeDelimited = function decodeDelimited(reader) {
                if (!(reader instanceof $Reader)) reader = new $Reader(reader)
                return this.decode(reader, reader.uint32())
            }

            /**
             * Verifies an Appearance message.
             * @function verify
             * @memberof mirabuf.material.Appearance
             * @static
             * @param {Object.<string,*>} message Plain object to verify
             * @returns {string|null} `null` if valid, otherwise the reason why it is not
             */
            Appearance.verify = function verify(message) {
                if (typeof message !== "object" || message === null) return "object expected"
                if (message.info != null && Object.hasOwn(message, "info")) {
                    let error = $root.mirabuf.Info.verify(message.info)
                    if (error) return "info." + error
                }
                if (message.albedo != null && Object.hasOwn(message, "albedo")) {
                    let error = $root.mirabuf.Color.verify(message.albedo)
                    if (error) return "albedo." + error
                }
                if (message.roughness != null && Object.hasOwn(message, "roughness"))
                    if (typeof message.roughness !== "number") return "roughness: number expected"
                if (message.metallic != null && Object.hasOwn(message, "metallic"))
                    if (typeof message.metallic !== "number") return "metallic: number expected"
                if (message.specular != null && Object.hasOwn(message, "specular"))
                    if (typeof message.specular !== "number") return "specular: number expected"
                return null
            }

            /**
             * Creates an Appearance message from a plain object. Also converts values to their respective internal types.
             * @function fromObject
             * @memberof mirabuf.material.Appearance
             * @static
             * @param {Object.<string,*>} object Plain object
             * @returns {mirabuf.material.Appearance} Appearance
             */
            Appearance.fromObject = function fromObject(object) {
                if (object instanceof $root.mirabuf.material.Appearance) return object
                let message = new $root.mirabuf.material.Appearance()
                if (object.info != null) {
                    if (typeof object.info !== "object")
                        throw TypeError(".mirabuf.material.Appearance.info: object expected")
                    message.info = $root.mirabuf.Info.fromObject(object.info)
                }
                if (object.albedo != null) {
                    if (typeof object.albedo !== "object")
                        throw TypeError(".mirabuf.material.Appearance.albedo: object expected")
                    message.albedo = $root.mirabuf.Color.fromObject(object.albedo)
                }
                if (object.roughness != null) message.roughness = Number(object.roughness)
                if (object.metallic != null) message.metallic = Number(object.metallic)
                if (object.specular != null) message.specular = Number(object.specular)
                return message
            }

            /**
             * Creates a plain object from an Appearance message. Also converts values to other types if specified.
             * @function toObject
             * @memberof mirabuf.material.Appearance
             * @static
             * @param {mirabuf.material.Appearance} message Appearance
             * @param {$protobuf.IConversionOptions} [options] Conversion options
             * @returns {Object.<string,*>} Plain object
             */
            Appearance.toObject = function toObject(message, options) {
                if (!options) options = {}
                let object = {}
                if (options.defaults) {
                    object.info = null
                    object.albedo = null
                    object.roughness = 0
                    object.metallic = 0
                    object.specular = 0
                }
                if (message.info != null && Object.hasOwn(message, "info"))
                    object.info = $root.mirabuf.Info.toObject(message.info, options)
                if (message.albedo != null && Object.hasOwn(message, "albedo"))
                    object.albedo = $root.mirabuf.Color.toObject(message.albedo, options)
                if (message.roughness != null && Object.hasOwn(message, "roughness"))
                    object.roughness =
                        options.json && !isFinite(message.roughness) ? String(message.roughness) : message.roughness
                if (message.metallic != null && Object.hasOwn(message, "metallic"))
                    object.metallic =
                        options.json && !isFinite(message.metallic) ? String(message.metallic) : message.metallic
                if (message.specular != null && Object.hasOwn(message, "specular"))
                    object.specular =
                        options.json && !isFinite(message.specular) ? String(message.specular) : message.specular
                return object
            }

            /**
             * Converts this Appearance to JSON.
             * @function toJSON
             * @memberof mirabuf.material.Appearance
             * @instance
             * @returns {Object.<string,*>} JSON object
             */
            Appearance.prototype.toJSON = function toJSON() {
                return this.constructor.toObject(this, $protobuf.util.toJSONOptions)
            }

            /**
             * Gets the default type url for Appearance
             * @function getTypeUrl
             * @memberof mirabuf.material.Appearance
             * @static
             * @param {string} [typeUrlPrefix] your custom typeUrlPrefix(default "type.googleapis.com")
             * @returns {string} The default type url
             */
            Appearance.getTypeUrl = function getTypeUrl(typeUrlPrefix) {
                if (typeUrlPrefix === undefined) {
                    typeUrlPrefix = "type.googleapis.com"
                }
                return typeUrlPrefix + "/mirabuf.material.Appearance"
            }

            return Appearance
        })()

        material.PhysicalMaterial = (function () {
            /**
             * Properties of a PhysicalMaterial.
             * @memberof mirabuf.material
             * @interface IPhysicalMaterial
             * @property {mirabuf.IInfo|null} [info] Identifiable information (id, name, version, etc)
             * @property {string|null} [description] short description of physical material
             * @property {mirabuf.material.PhysicalMaterial.IThermal|null} [thermal] Thermal Physical properties of the model OPTIONAL
             * @property {mirabuf.material.PhysicalMaterial.IMechanical|null} [mechanical] Mechanical properties of the model OPTIONAL
             * @property {mirabuf.material.PhysicalMaterial.IStrength|null} [strength] Physical Strength properties of the model OPTIONAL
             * @property {number|null} [dynamicFriction] Frictional force for dampening - Interpolate (0-1)
             * @property {number|null} [staticFriction] Frictional force override at stop - Interpolate (0-1)
             * @property {number|null} [restitution] Restitution of the object - Interpolate (0-1)
             * @property {boolean|null} [deformable] should this object deform when encountering large forces - TODO: This needs a proper message and equation field
             * @property {mirabuf.material.PhysicalMaterial.MaterialType|null} [matType] generic type to assign some default params
             */

            /**
             * Constructs a new PhysicalMaterial.
             * @memberof mirabuf.material
             * @classdesc Data to represent any given Physical Material
             * @implements IPhysicalMaterial
             * @constructor
             * @param {mirabuf.material.IPhysicalMaterial=} [properties] Properties to set
             */
            function PhysicalMaterial(properties) {
                if (properties)
                    for (let keys = Object.keys(properties), i = 0; i < keys.length; ++i)
                        if (properties[keys[i]] != null) this[keys[i]] = properties[keys[i]]
            }

            /**
             * Identifiable information (id, name, version, etc)
             * @member {mirabuf.IInfo|null|undefined} info
             * @memberof mirabuf.material.PhysicalMaterial
             * @instance
             */
            PhysicalMaterial.prototype.info = null

            /**
             * short description of physical material
             * @member {string} description
             * @memberof mirabuf.material.PhysicalMaterial
             * @instance
             */
            PhysicalMaterial.prototype.description = ""

            /**
             * Thermal Physical properties of the model OPTIONAL
             * @member {mirabuf.material.PhysicalMaterial.IThermal|null|undefined} thermal
             * @memberof mirabuf.material.PhysicalMaterial
             * @instance
             */
            PhysicalMaterial.prototype.thermal = null

            /**
             * Mechanical properties of the model OPTIONAL
             * @member {mirabuf.material.PhysicalMaterial.IMechanical|null|undefined} mechanical
             * @memberof mirabuf.material.PhysicalMaterial
             * @instance
             */
            PhysicalMaterial.prototype.mechanical = null

            /**
             * Physical Strength properties of the model OPTIONAL
             * @member {mirabuf.material.PhysicalMaterial.IStrength|null|undefined} strength
             * @memberof mirabuf.material.PhysicalMaterial
             * @instance
             */
            PhysicalMaterial.prototype.strength = null

            /**
             * Frictional force for dampening - Interpolate (0-1)
             * @member {number} dynamicFriction
             * @memberof mirabuf.material.PhysicalMaterial
             * @instance
             */
            PhysicalMaterial.prototype.dynamicFriction = 0

            /**
             * Frictional force override at stop - Interpolate (0-1)
             * @member {number} staticFriction
             * @memberof mirabuf.material.PhysicalMaterial
             * @instance
             */
            PhysicalMaterial.prototype.staticFriction = 0

            /**
             * Restitution of the object - Interpolate (0-1)
             * @member {number} restitution
             * @memberof mirabuf.material.PhysicalMaterial
             * @instance
             */
            PhysicalMaterial.prototype.restitution = 0

            /**
             * should this object deform when encountering large forces - TODO: This needs a proper message and equation field
             * @member {boolean} deformable
             * @memberof mirabuf.material.PhysicalMaterial
             * @instance
             */
            PhysicalMaterial.prototype.deformable = false

            /**
             * generic type to assign some default params
             * @member {mirabuf.material.PhysicalMaterial.MaterialType} matType
             * @memberof mirabuf.material.PhysicalMaterial
             * @instance
             */
            PhysicalMaterial.prototype.matType = 0

            /**
             * Creates a new PhysicalMaterial instance using the specified properties.
             * @function create
             * @memberof mirabuf.material.PhysicalMaterial
             * @static
             * @param {mirabuf.material.IPhysicalMaterial=} [properties] Properties to set
             * @returns {mirabuf.material.PhysicalMaterial} PhysicalMaterial instance
             */
            PhysicalMaterial.create = function create(properties) {
                return new PhysicalMaterial(properties)
            }

            /**
             * Encodes the specified PhysicalMaterial message. Does not implicitly {@link mirabuf.material.PhysicalMaterial.verify|verify} messages.
             * @function encode
             * @memberof mirabuf.material.PhysicalMaterial
             * @static
             * @param {mirabuf.material.IPhysicalMaterial} message PhysicalMaterial message or plain object to encode
             * @param {$protobuf.Writer} [writer] Writer to encode to
             * @returns {$protobuf.Writer} Writer
             */
            PhysicalMaterial.encode = function encode(message, writer) {
                if (!writer) writer = $Writer.create()
                if (message.info != null && Object.hasOwn(message, "info"))
                    $root.mirabuf.Info.encode(message.info, writer.uint32(/* id 1, wireType 2 =*/ 10).fork()).ldelim()
                if (message.description != null && Object.hasOwn(message, "description"))
                    writer.uint32(/* id 2, wireType 2 =*/ 18).string(message.description)
                if (message.thermal != null && Object.hasOwn(message, "thermal"))
                    $root.mirabuf.material.PhysicalMaterial.Thermal.encode(
                        message.thermal,
                        writer.uint32(/* id 3, wireType 2 =*/ 26).fork()
                    ).ldelim()
                if (message.mechanical != null && Object.hasOwn(message, "mechanical"))
                    $root.mirabuf.material.PhysicalMaterial.Mechanical.encode(
                        message.mechanical,
                        writer.uint32(/* id 4, wireType 2 =*/ 34).fork()
                    ).ldelim()
                if (message.strength != null && Object.hasOwn(message, "strength"))
                    $root.mirabuf.material.PhysicalMaterial.Strength.encode(
                        message.strength,
                        writer.uint32(/* id 5, wireType 2 =*/ 42).fork()
                    ).ldelim()
                if (message.dynamicFriction != null && Object.hasOwn(message, "dynamicFriction"))
                    writer.uint32(/* id 6, wireType 5 =*/ 53).float(message.dynamicFriction)
                if (message.staticFriction != null && Object.hasOwn(message, "staticFriction"))
                    writer.uint32(/* id 7, wireType 5 =*/ 61).float(message.staticFriction)
                if (message.restitution != null && Object.hasOwn(message, "restitution"))
                    writer.uint32(/* id 8, wireType 5 =*/ 69).float(message.restitution)
                if (message.deformable != null && Object.hasOwn(message, "deformable"))
                    writer.uint32(/* id 9, wireType 0 =*/ 72).bool(message.deformable)
                if (message.matType != null && Object.hasOwn(message, "matType"))
                    writer.uint32(/* id 10, wireType 0 =*/ 80).int32(message.matType)
                return writer
            }

            /**
             * Encodes the specified PhysicalMaterial message, length delimited. Does not implicitly {@link mirabuf.material.PhysicalMaterial.verify|verify} messages.
             * @function encodeDelimited
             * @memberof mirabuf.material.PhysicalMaterial
             * @static
             * @param {mirabuf.material.IPhysicalMaterial} message PhysicalMaterial message or plain object to encode
             * @param {$protobuf.Writer} [writer] Writer to encode to
             * @returns {$protobuf.Writer} Writer
             */
            PhysicalMaterial.encodeDelimited = function encodeDelimited(message, writer) {
                return this.encode(message, writer).ldelim()
            }

            /**
             * Decodes a PhysicalMaterial message from the specified reader or buffer.
             * @function decode
             * @memberof mirabuf.material.PhysicalMaterial
             * @static
             * @param {$protobuf.Reader|Uint8Array} reader Reader or buffer to decode from
             * @param {number} [length] Message length if known beforehand
             * @returns {mirabuf.material.PhysicalMaterial} PhysicalMaterial
             * @throws {Error} If the payload is not a reader or valid buffer
             * @throws {$protobuf.util.ProtocolError} If required fields are missing
             */
            PhysicalMaterial.decode = function decode(reader, length) {
                if (!(reader instanceof $Reader)) reader = $Reader.create(reader)
                let end = length === undefined ? reader.len : reader.pos + length,
                    message = new $root.mirabuf.material.PhysicalMaterial()
                while (reader.pos < end) {
                    let tag = reader.uint32()
                    switch (tag >>> 3) {
                        case 1: {
                            message.info = $root.mirabuf.Info.decode(reader, reader.uint32())
                            break
                        }
                        case 2: {
                            message.description = reader.string()
                            break
                        }
                        case 3: {
                            message.thermal = $root.mirabuf.material.PhysicalMaterial.Thermal.decode(
                                reader,
                                reader.uint32()
                            )
                            break
                        }
                        case 4: {
                            message.mechanical = $root.mirabuf.material.PhysicalMaterial.Mechanical.decode(
                                reader,
                                reader.uint32()
                            )
                            break
                        }
                        case 5: {
                            message.strength = $root.mirabuf.material.PhysicalMaterial.Strength.decode(
                                reader,
                                reader.uint32()
                            )
                            break
                        }
                        case 6: {
                            message.dynamicFriction = reader.float()
                            break
                        }
                        case 7: {
                            message.staticFriction = reader.float()
                            break
                        }
                        case 8: {
                            message.restitution = reader.float()
                            break
                        }
                        case 9: {
                            message.deformable = reader.bool()
                            break
                        }
                        case 10: {
                            message.matType = reader.int32()
                            break
                        }
                        default:
                            reader.skipType(tag & 7)
                            break
                    }
                }
                return message
            }

            /**
             * Decodes a PhysicalMaterial message from the specified reader or buffer, length delimited.
             * @function decodeDelimited
             * @memberof mirabuf.material.PhysicalMaterial
             * @static
             * @param {$protobuf.Reader|Uint8Array} reader Reader or buffer to decode from
             * @returns {mirabuf.material.PhysicalMaterial} PhysicalMaterial
             * @throws {Error} If the payload is not a reader or valid buffer
             * @throws {$protobuf.util.ProtocolError} If required fields are missing
             */
            PhysicalMaterial.decodeDelimited = function decodeDelimited(reader) {
                if (!(reader instanceof $Reader)) reader = new $Reader(reader)
                return this.decode(reader, reader.uint32())
            }

            /**
             * Verifies a PhysicalMaterial message.
             * @function verify
             * @memberof mirabuf.material.PhysicalMaterial
             * @static
             * @param {Object.<string,*>} message Plain object to verify
             * @returns {string|null} `null` if valid, otherwise the reason why it is not
             */
            PhysicalMaterial.verify = function verify(message) {
                if (typeof message !== "object" || message === null) return "object expected"
                if (message.info != null && Object.hasOwn(message, "info")) {
                    let error = $root.mirabuf.Info.verify(message.info)
                    if (error) return "info." + error
                }
                if (message.description != null && Object.hasOwn(message, "description"))
                    if (!$util.isString(message.description)) return "description: string expected"
                if (message.thermal != null && Object.hasOwn(message, "thermal")) {
                    let error = $root.mirabuf.material.PhysicalMaterial.Thermal.verify(message.thermal)
                    if (error) return "thermal." + error
                }
                if (message.mechanical != null && Object.hasOwn(message, "mechanical")) {
                    let error = $root.mirabuf.material.PhysicalMaterial.Mechanical.verify(message.mechanical)
                    if (error) return "mechanical." + error
                }
                if (message.strength != null && Object.hasOwn(message, "strength")) {
                    let error = $root.mirabuf.material.PhysicalMaterial.Strength.verify(message.strength)
                    if (error) return "strength." + error
                }
                if (message.dynamicFriction != null && Object.hasOwn(message, "dynamicFriction"))
                    if (typeof message.dynamicFriction !== "number") return "dynamicFriction: number expected"
                if (message.staticFriction != null && Object.hasOwn(message, "staticFriction"))
                    if (typeof message.staticFriction !== "number") return "staticFriction: number expected"
                if (message.restitution != null && Object.hasOwn(message, "restitution"))
                    if (typeof message.restitution !== "number") return "restitution: number expected"
                if (message.deformable != null && Object.hasOwn(message, "deformable"))
                    if (typeof message.deformable !== "boolean") return "deformable: boolean expected"
                if (message.matType != null && Object.hasOwn(message, "matType"))
                    switch (message.matType) {
                        default:
                            return "matType: enum value expected"
                        case 0:
                        case 1:
                            break
                    }
                return null
            }

            /**
             * Creates a PhysicalMaterial message from a plain object. Also converts values to their respective internal types.
             * @function fromObject
             * @memberof mirabuf.material.PhysicalMaterial
             * @static
             * @param {Object.<string,*>} object Plain object
             * @returns {mirabuf.material.PhysicalMaterial} PhysicalMaterial
             */
            PhysicalMaterial.fromObject = function fromObject(object) {
                if (object instanceof $root.mirabuf.material.PhysicalMaterial) return object
                let message = new $root.mirabuf.material.PhysicalMaterial()
                if (object.info != null) {
                    if (typeof object.info !== "object")
                        throw TypeError(".mirabuf.material.PhysicalMaterial.info: object expected")
                    message.info = $root.mirabuf.Info.fromObject(object.info)
                }
                if (object.description != null) message.description = String(object.description)
                if (object.thermal != null) {
                    if (typeof object.thermal !== "object")
                        throw TypeError(".mirabuf.material.PhysicalMaterial.thermal: object expected")
                    message.thermal = $root.mirabuf.material.PhysicalMaterial.Thermal.fromObject(object.thermal)
                }
                if (object.mechanical != null) {
                    if (typeof object.mechanical !== "object")
                        throw TypeError(".mirabuf.material.PhysicalMaterial.mechanical: object expected")
                    message.mechanical = $root.mirabuf.material.PhysicalMaterial.Mechanical.fromObject(
                        object.mechanical
                    )
                }
                if (object.strength != null) {
                    if (typeof object.strength !== "object")
                        throw TypeError(".mirabuf.material.PhysicalMaterial.strength: object expected")
                    message.strength = $root.mirabuf.material.PhysicalMaterial.Strength.fromObject(object.strength)
                }
                if (object.dynamicFriction != null) message.dynamicFriction = Number(object.dynamicFriction)
                if (object.staticFriction != null) message.staticFriction = Number(object.staticFriction)
                if (object.restitution != null) message.restitution = Number(object.restitution)
                if (object.deformable != null) message.deformable = Boolean(object.deformable)
                switch (object.matType) {
                    default:
                        if (typeof object.matType === "number") {
                            message.matType = object.matType
                            break
                        }
                        break
                    case "METAL":
                    case 0:
                        message.matType = 0
                        break
                    case "PLASTIC":
                    case 1:
                        message.matType = 1
                        break
                }
                return message
            }

            /**
             * Creates a plain object from a PhysicalMaterial message. Also converts values to other types if specified.
             * @function toObject
             * @memberof mirabuf.material.PhysicalMaterial
             * @static
             * @param {mirabuf.material.PhysicalMaterial} message PhysicalMaterial
             * @param {$protobuf.IConversionOptions} [options] Conversion options
             * @returns {Object.<string,*>} Plain object
             */
            PhysicalMaterial.toObject = function toObject(message, options) {
                if (!options) options = {}
                let object = {}
                if (options.defaults) {
                    object.info = null
                    object.description = ""
                    object.thermal = null
                    object.mechanical = null
                    object.strength = null
                    object.dynamicFriction = 0
                    object.staticFriction = 0
                    object.restitution = 0
                    object.deformable = false
                    object.matType = options.enums === String ? "METAL" : 0
                }
                if (message.info != null && Object.hasOwn(message, "info"))
                    object.info = $root.mirabuf.Info.toObject(message.info, options)
                if (message.description != null && Object.hasOwn(message, "description"))
                    object.description = message.description
                if (message.thermal != null && Object.hasOwn(message, "thermal"))
                    object.thermal = $root.mirabuf.material.PhysicalMaterial.Thermal.toObject(message.thermal, options)
                if (message.mechanical != null && Object.hasOwn(message, "mechanical"))
                    object.mechanical = $root.mirabuf.material.PhysicalMaterial.Mechanical.toObject(
                        message.mechanical,
                        options
                    )
                if (message.strength != null && Object.hasOwn(message, "strength"))
                    object.strength = $root.mirabuf.material.PhysicalMaterial.Strength.toObject(
                        message.strength,
                        options
                    )
                if (message.dynamicFriction != null && Object.hasOwn(message, "dynamicFriction"))
                    object.dynamicFriction =
                        options.json && !isFinite(message.dynamicFriction)
                            ? String(message.dynamicFriction)
                            : message.dynamicFriction
                if (message.staticFriction != null && Object.hasOwn(message, "staticFriction"))
                    object.staticFriction =
                        options.json && !isFinite(message.staticFriction)
                            ? String(message.staticFriction)
                            : message.staticFriction
                if (message.restitution != null && Object.hasOwn(message, "restitution"))
                    object.restitution =
                        options.json && !isFinite(message.restitution)
                            ? String(message.restitution)
                            : message.restitution
                if (message.deformable != null && Object.hasOwn(message, "deformable"))
                    object.deformable = message.deformable
                if (message.matType != null && Object.hasOwn(message, "matType"))
                    object.matType =
                        options.enums === String
                            ? $root.mirabuf.material.PhysicalMaterial.MaterialType[message.matType] === undefined
                                ? message.matType
                                : $root.mirabuf.material.PhysicalMaterial.MaterialType[message.matType]
                            : message.matType
                return object
            }

            /**
             * Converts this PhysicalMaterial to JSON.
             * @function toJSON
             * @memberof mirabuf.material.PhysicalMaterial
             * @instance
             * @returns {Object.<string,*>} JSON object
             */
            PhysicalMaterial.prototype.toJSON = function toJSON() {
                return this.constructor.toObject(this, $protobuf.util.toJSONOptions)
            }

            /**
             * Gets the default type url for PhysicalMaterial
             * @function getTypeUrl
             * @memberof mirabuf.material.PhysicalMaterial
             * @static
             * @param {string} [typeUrlPrefix] your custom typeUrlPrefix(default "type.googleapis.com")
             * @returns {string} The default type url
             */
            PhysicalMaterial.getTypeUrl = function getTypeUrl(typeUrlPrefix) {
                if (typeUrlPrefix === undefined) {
                    typeUrlPrefix = "type.googleapis.com"
                }
                return typeUrlPrefix + "/mirabuf.material.PhysicalMaterial"
            }

            /**
             * MaterialType enum.
             * @name mirabuf.material.PhysicalMaterial.MaterialType
             * @enum {number}
             * @property {number} METAL=0 METAL value
             * @property {number} PLASTIC=1 PLASTIC value
             */
            PhysicalMaterial.MaterialType = (function () {
                const valuesById = {},
                    values = Object.create(valuesById)
                values[(valuesById[0] = "METAL")] = 0
                values[(valuesById[1] = "PLASTIC")] = 1
                return values
            })()

            PhysicalMaterial.Thermal = (function () {
                /**
                 * Properties of a Thermal.
                 * @memberof mirabuf.material.PhysicalMaterial
                 * @interface IThermal
                 * @property {number|null} [thermalConductivity] W/(m*K)
                 * @property {number|null} [specificHeat] J/(g*C)
                 * @property {number|null} [thermalExpansionCoefficient] um/(m*C)
                 */

                /**
                 * Constructs a new Thermal.
                 * @memberof mirabuf.material.PhysicalMaterial
                 * @classdesc Thermal Properties Set Definition for Simulation.
                 * @implements IThermal
                 * @constructor
                 * @param {mirabuf.material.PhysicalMaterial.IThermal=} [properties] Properties to set
                 */
                function Thermal(properties) {
                    if (properties)
                        for (let keys = Object.keys(properties), i = 0; i < keys.length; ++i)
                            if (properties[keys[i]] != null) this[keys[i]] = properties[keys[i]]
                }

                /**
                 * W/(m*K)
                 * @member {number} thermalConductivity
                 * @memberof mirabuf.material.PhysicalMaterial.Thermal
                 * @instance
                 */
                Thermal.prototype.thermalConductivity = 0

                /**
                 * J/(g*C)
                 * @member {number} specificHeat
                 * @memberof mirabuf.material.PhysicalMaterial.Thermal
                 * @instance
                 */
                Thermal.prototype.specificHeat = 0

                /**
                 * um/(m*C)
                 * @member {number} thermalExpansionCoefficient
                 * @memberof mirabuf.material.PhysicalMaterial.Thermal
                 * @instance
                 */
                Thermal.prototype.thermalExpansionCoefficient = 0

                /**
                 * Creates a new Thermal instance using the specified properties.
                 * @function create
                 * @memberof mirabuf.material.PhysicalMaterial.Thermal
                 * @static
                 * @param {mirabuf.material.PhysicalMaterial.IThermal=} [properties] Properties to set
                 * @returns {mirabuf.material.PhysicalMaterial.Thermal} Thermal instance
                 */
                Thermal.create = function create(properties) {
                    return new Thermal(properties)
                }

                /**
                 * Encodes the specified Thermal message. Does not implicitly {@link mirabuf.material.PhysicalMaterial.Thermal.verify|verify} messages.
                 * @function encode
                 * @memberof mirabuf.material.PhysicalMaterial.Thermal
                 * @static
                 * @param {mirabuf.material.PhysicalMaterial.IThermal} message Thermal message or plain object to encode
                 * @param {$protobuf.Writer} [writer] Writer to encode to
                 * @returns {$protobuf.Writer} Writer
                 */
                Thermal.encode = function encode(message, writer) {
                    if (!writer) writer = $Writer.create()
                    if (message.thermalConductivity != null && Object.hasOwn(message, "thermalConductivity"))
                        writer.uint32(/* id 1, wireType 5 =*/ 13).float(message.thermalConductivity)
                    if (message.specificHeat != null && Object.hasOwn(message, "specificHeat"))
                        writer.uint32(/* id 2, wireType 5 =*/ 21).float(message.specificHeat)
                    if (
                        message.thermalExpansionCoefficient != null &&
                        Object.hasOwn(message, "thermalExpansionCoefficient")
                    )
                        writer.uint32(/* id 3, wireType 5 =*/ 29).float(message.thermalExpansionCoefficient)
                    return writer
                }

                /**
                 * Encodes the specified Thermal message, length delimited. Does not implicitly {@link mirabuf.material.PhysicalMaterial.Thermal.verify|verify} messages.
                 * @function encodeDelimited
                 * @memberof mirabuf.material.PhysicalMaterial.Thermal
                 * @static
                 * @param {mirabuf.material.PhysicalMaterial.IThermal} message Thermal message or plain object to encode
                 * @param {$protobuf.Writer} [writer] Writer to encode to
                 * @returns {$protobuf.Writer} Writer
                 */
                Thermal.encodeDelimited = function encodeDelimited(message, writer) {
                    return this.encode(message, writer).ldelim()
                }

                /**
                 * Decodes a Thermal message from the specified reader or buffer.
                 * @function decode
                 * @memberof mirabuf.material.PhysicalMaterial.Thermal
                 * @static
                 * @param {$protobuf.Reader|Uint8Array} reader Reader or buffer to decode from
                 * @param {number} [length] Message length if known beforehand
                 * @returns {mirabuf.material.PhysicalMaterial.Thermal} Thermal
                 * @throws {Error} If the payload is not a reader or valid buffer
                 * @throws {$protobuf.util.ProtocolError} If required fields are missing
                 */
                Thermal.decode = function decode(reader, length) {
                    if (!(reader instanceof $Reader)) reader = $Reader.create(reader)
                    let end = length === undefined ? reader.len : reader.pos + length,
                        message = new $root.mirabuf.material.PhysicalMaterial.Thermal()
                    while (reader.pos < end) {
                        let tag = reader.uint32()
                        switch (tag >>> 3) {
                            case 1: {
                                message.thermalConductivity = reader.float()
                                break
                            }
                            case 2: {
                                message.specificHeat = reader.float()
                                break
                            }
                            case 3: {
                                message.thermalExpansionCoefficient = reader.float()
                                break
                            }
                            default:
                                reader.skipType(tag & 7)
                                break
                        }
                    }
                    return message
                }

                /**
                 * Decodes a Thermal message from the specified reader or buffer, length delimited.
                 * @function decodeDelimited
                 * @memberof mirabuf.material.PhysicalMaterial.Thermal
                 * @static
                 * @param {$protobuf.Reader|Uint8Array} reader Reader or buffer to decode from
                 * @returns {mirabuf.material.PhysicalMaterial.Thermal} Thermal
                 * @throws {Error} If the payload is not a reader or valid buffer
                 * @throws {$protobuf.util.ProtocolError} If required fields are missing
                 */
                Thermal.decodeDelimited = function decodeDelimited(reader) {
                    if (!(reader instanceof $Reader)) reader = new $Reader(reader)
                    return this.decode(reader, reader.uint32())
                }

                /**
                 * Verifies a Thermal message.
                 * @function verify
                 * @memberof mirabuf.material.PhysicalMaterial.Thermal
                 * @static
                 * @param {Object.<string,*>} message Plain object to verify
                 * @returns {string|null} `null` if valid, otherwise the reason why it is not
                 */
                Thermal.verify = function verify(message) {
                    if (typeof message !== "object" || message === null) return "object expected"
                    if (message.thermalConductivity != null && Object.hasOwn(message, "thermalConductivity"))
                        if (typeof message.thermalConductivity !== "number")
                            return "thermalConductivity: number expected"
                    if (message.specificHeat != null && Object.hasOwn(message, "specificHeat"))
                        if (typeof message.specificHeat !== "number") return "specificHeat: number expected"
                    if (
                        message.thermalExpansionCoefficient != null &&
                        Object.hasOwn(message, "thermalExpansionCoefficient")
                    )
                        if (typeof message.thermalExpansionCoefficient !== "number")
                            return "thermalExpansionCoefficient: number expected"
                    return null
                }

                /**
                 * Creates a Thermal message from a plain object. Also converts values to their respective internal types.
                 * @function fromObject
                 * @memberof mirabuf.material.PhysicalMaterial.Thermal
                 * @static
                 * @param {Object.<string,*>} object Plain object
                 * @returns {mirabuf.material.PhysicalMaterial.Thermal} Thermal
                 */
                Thermal.fromObject = function fromObject(object) {
                    if (object instanceof $root.mirabuf.material.PhysicalMaterial.Thermal) return object
                    let message = new $root.mirabuf.material.PhysicalMaterial.Thermal()
                    if (object.thermalConductivity != null)
                        message.thermalConductivity = Number(object.thermalConductivity)
                    if (object.specificHeat != null) message.specificHeat = Number(object.specificHeat)
                    if (object.thermalExpansionCoefficient != null)
                        message.thermalExpansionCoefficient = Number(object.thermalExpansionCoefficient)
                    return message
                }

                /**
                 * Creates a plain object from a Thermal message. Also converts values to other types if specified.
                 * @function toObject
                 * @memberof mirabuf.material.PhysicalMaterial.Thermal
                 * @static
                 * @param {mirabuf.material.PhysicalMaterial.Thermal} message Thermal
                 * @param {$protobuf.IConversionOptions} [options] Conversion options
                 * @returns {Object.<string,*>} Plain object
                 */
                Thermal.toObject = function toObject(message, options) {
                    if (!options) options = {}
                    let object = {}
                    if (options.defaults) {
                        object.thermalConductivity = 0
                        object.specificHeat = 0
                        object.thermalExpansionCoefficient = 0
                    }
                    if (message.thermalConductivity != null && Object.hasOwn(message, "thermalConductivity"))
                        object.thermalConductivity =
                            options.json && !isFinite(message.thermalConductivity)
                                ? String(message.thermalConductivity)
                                : message.thermalConductivity
                    if (message.specificHeat != null && Object.hasOwn(message, "specificHeat"))
                        object.specificHeat =
                            options.json && !isFinite(message.specificHeat)
                                ? String(message.specificHeat)
                                : message.specificHeat
                    if (
                        message.thermalExpansionCoefficient != null &&
                        Object.hasOwn(message, "thermalExpansionCoefficient")
                    )
                        object.thermalExpansionCoefficient =
                            options.json && !isFinite(message.thermalExpansionCoefficient)
                                ? String(message.thermalExpansionCoefficient)
                                : message.thermalExpansionCoefficient
                    return object
                }

                /**
                 * Converts this Thermal to JSON.
                 * @function toJSON
                 * @memberof mirabuf.material.PhysicalMaterial.Thermal
                 * @instance
                 * @returns {Object.<string,*>} JSON object
                 */
                Thermal.prototype.toJSON = function toJSON() {
                    return this.constructor.toObject(this, $protobuf.util.toJSONOptions)
                }

                /**
                 * Gets the default type url for Thermal
                 * @function getTypeUrl
                 * @memberof mirabuf.material.PhysicalMaterial.Thermal
                 * @static
                 * @param {string} [typeUrlPrefix] your custom typeUrlPrefix(default "type.googleapis.com")
                 * @returns {string} The default type url
                 */
                Thermal.getTypeUrl = function getTypeUrl(typeUrlPrefix) {
                    if (typeUrlPrefix === undefined) {
                        typeUrlPrefix = "type.googleapis.com"
                    }
                    return typeUrlPrefix + "/mirabuf.material.PhysicalMaterial.Thermal"
                }

                return Thermal
            })()

            PhysicalMaterial.Mechanical = (function () {
                /**
                 * Properties of a Mechanical.
                 * @memberof mirabuf.material.PhysicalMaterial
                 * @interface IMechanical
                 * @property {number|null} [youngMod] GPa
                 * @property {number|null} [poissonRatio] ?
                 * @property {number|null} [shearMod] MPa
                 * @property {number|null} [density] g/cm^3
                 * @property {number|null} [dampingCoefficient] ?
                 */

                /**
                 * Constructs a new Mechanical.
                 * @memberof mirabuf.material.PhysicalMaterial
                 * @classdesc Mechanical Properties Set Definition for Simulation.
                 * @implements IMechanical
                 * @constructor
                 * @param {mirabuf.material.PhysicalMaterial.IMechanical=} [properties] Properties to set
                 */
                function Mechanical(properties) {
                    if (properties)
                        for (let keys = Object.keys(properties), i = 0; i < keys.length; ++i)
                            if (properties[keys[i]] != null) this[keys[i]] = properties[keys[i]]
                }

                /**
                 * GPa
                 * @member {number} youngMod
                 * @memberof mirabuf.material.PhysicalMaterial.Mechanical
                 * @instance
                 */
                Mechanical.prototype.youngMod = 0

                /**
                 * ?
                 * @member {number} poissonRatio
                 * @memberof mirabuf.material.PhysicalMaterial.Mechanical
                 * @instance
                 */
                Mechanical.prototype.poissonRatio = 0

                /**
                 * MPa
                 * @member {number} shearMod
                 * @memberof mirabuf.material.PhysicalMaterial.Mechanical
                 * @instance
                 */
                Mechanical.prototype.shearMod = 0

                /**
                 * g/cm^3
                 * @member {number} density
                 * @memberof mirabuf.material.PhysicalMaterial.Mechanical
                 * @instance
                 */
                Mechanical.prototype.density = 0

                /**
                 * ?
                 * @member {number} dampingCoefficient
                 * @memberof mirabuf.material.PhysicalMaterial.Mechanical
                 * @instance
                 */
                Mechanical.prototype.dampingCoefficient = 0

                /**
                 * Creates a new Mechanical instance using the specified properties.
                 * @function create
                 * @memberof mirabuf.material.PhysicalMaterial.Mechanical
                 * @static
                 * @param {mirabuf.material.PhysicalMaterial.IMechanical=} [properties] Properties to set
                 * @returns {mirabuf.material.PhysicalMaterial.Mechanical} Mechanical instance
                 */
                Mechanical.create = function create(properties) {
                    return new Mechanical(properties)
                }

                /**
                 * Encodes the specified Mechanical message. Does not implicitly {@link mirabuf.material.PhysicalMaterial.Mechanical.verify|verify} messages.
                 * @function encode
                 * @memberof mirabuf.material.PhysicalMaterial.Mechanical
                 * @static
                 * @param {mirabuf.material.PhysicalMaterial.IMechanical} message Mechanical message or plain object to encode
                 * @param {$protobuf.Writer} [writer] Writer to encode to
                 * @returns {$protobuf.Writer} Writer
                 */
                Mechanical.encode = function encode(message, writer) {
                    if (!writer) writer = $Writer.create()
                    if (message.youngMod != null && Object.hasOwn(message, "youngMod"))
                        writer.uint32(/* id 1, wireType 5 =*/ 13).float(message.youngMod)
                    if (message.poissonRatio != null && Object.hasOwn(message, "poissonRatio"))
                        writer.uint32(/* id 2, wireType 5 =*/ 21).float(message.poissonRatio)
                    if (message.shearMod != null && Object.hasOwn(message, "shearMod"))
                        writer.uint32(/* id 3, wireType 5 =*/ 29).float(message.shearMod)
                    if (message.density != null && Object.hasOwn(message, "density"))
                        writer.uint32(/* id 4, wireType 5 =*/ 37).float(message.density)
                    if (message.dampingCoefficient != null && Object.hasOwn(message, "dampingCoefficient"))
                        writer.uint32(/* id 5, wireType 5 =*/ 45).float(message.dampingCoefficient)
                    return writer
                }

                /**
                 * Encodes the specified Mechanical message, length delimited. Does not implicitly {@link mirabuf.material.PhysicalMaterial.Mechanical.verify|verify} messages.
                 * @function encodeDelimited
                 * @memberof mirabuf.material.PhysicalMaterial.Mechanical
                 * @static
                 * @param {mirabuf.material.PhysicalMaterial.IMechanical} message Mechanical message or plain object to encode
                 * @param {$protobuf.Writer} [writer] Writer to encode to
                 * @returns {$protobuf.Writer} Writer
                 */
                Mechanical.encodeDelimited = function encodeDelimited(message, writer) {
                    return this.encode(message, writer).ldelim()
                }

                /**
                 * Decodes a Mechanical message from the specified reader or buffer.
                 * @function decode
                 * @memberof mirabuf.material.PhysicalMaterial.Mechanical
                 * @static
                 * @param {$protobuf.Reader|Uint8Array} reader Reader or buffer to decode from
                 * @param {number} [length] Message length if known beforehand
                 * @returns {mirabuf.material.PhysicalMaterial.Mechanical} Mechanical
                 * @throws {Error} If the payload is not a reader or valid buffer
                 * @throws {$protobuf.util.ProtocolError} If required fields are missing
                 */
                Mechanical.decode = function decode(reader, length) {
                    if (!(reader instanceof $Reader)) reader = $Reader.create(reader)
                    let end = length === undefined ? reader.len : reader.pos + length,
                        message = new $root.mirabuf.material.PhysicalMaterial.Mechanical()
                    while (reader.pos < end) {
                        let tag = reader.uint32()
                        switch (tag >>> 3) {
                            case 1: {
                                message.youngMod = reader.float()
                                break
                            }
                            case 2: {
                                message.poissonRatio = reader.float()
                                break
                            }
                            case 3: {
                                message.shearMod = reader.float()
                                break
                            }
                            case 4: {
                                message.density = reader.float()
                                break
                            }
                            case 5: {
                                message.dampingCoefficient = reader.float()
                                break
                            }
                            default:
                                reader.skipType(tag & 7)
                                break
                        }
                    }
                    return message
                }

                /**
                 * Decodes a Mechanical message from the specified reader or buffer, length delimited.
                 * @function decodeDelimited
                 * @memberof mirabuf.material.PhysicalMaterial.Mechanical
                 * @static
                 * @param {$protobuf.Reader|Uint8Array} reader Reader or buffer to decode from
                 * @returns {mirabuf.material.PhysicalMaterial.Mechanical} Mechanical
                 * @throws {Error} If the payload is not a reader or valid buffer
                 * @throws {$protobuf.util.ProtocolError} If required fields are missing
                 */
                Mechanical.decodeDelimited = function decodeDelimited(reader) {
                    if (!(reader instanceof $Reader)) reader = new $Reader(reader)
                    return this.decode(reader, reader.uint32())
                }

                /**
                 * Verifies a Mechanical message.
                 * @function verify
                 * @memberof mirabuf.material.PhysicalMaterial.Mechanical
                 * @static
                 * @param {Object.<string,*>} message Plain object to verify
                 * @returns {string|null} `null` if valid, otherwise the reason why it is not
                 */
                Mechanical.verify = function verify(message) {
                    if (typeof message !== "object" || message === null) return "object expected"
                    if (message.youngMod != null && Object.hasOwn(message, "youngMod"))
                        if (typeof message.youngMod !== "number") return "youngMod: number expected"
                    if (message.poissonRatio != null && Object.hasOwn(message, "poissonRatio"))
                        if (typeof message.poissonRatio !== "number") return "poissonRatio: number expected"
                    if (message.shearMod != null && Object.hasOwn(message, "shearMod"))
                        if (typeof message.shearMod !== "number") return "shearMod: number expected"
                    if (message.density != null && Object.hasOwn(message, "density"))
                        if (typeof message.density !== "number") return "density: number expected"
                    if (message.dampingCoefficient != null && Object.hasOwn(message, "dampingCoefficient"))
                        if (typeof message.dampingCoefficient !== "number") return "dampingCoefficient: number expected"
                    return null
                }

                /**
                 * Creates a Mechanical message from a plain object. Also converts values to their respective internal types.
                 * @function fromObject
                 * @memberof mirabuf.material.PhysicalMaterial.Mechanical
                 * @static
                 * @param {Object.<string,*>} object Plain object
                 * @returns {mirabuf.material.PhysicalMaterial.Mechanical} Mechanical
                 */
                Mechanical.fromObject = function fromObject(object) {
                    if (object instanceof $root.mirabuf.material.PhysicalMaterial.Mechanical) return object
                    let message = new $root.mirabuf.material.PhysicalMaterial.Mechanical()
                    if (object.youngMod != null) message.youngMod = Number(object.youngMod)
                    if (object.poissonRatio != null) message.poissonRatio = Number(object.poissonRatio)
                    if (object.shearMod != null) message.shearMod = Number(object.shearMod)
                    if (object.density != null) message.density = Number(object.density)
                    if (object.dampingCoefficient != null)
                        message.dampingCoefficient = Number(object.dampingCoefficient)
                    return message
                }

                /**
                 * Creates a plain object from a Mechanical message. Also converts values to other types if specified.
                 * @function toObject
                 * @memberof mirabuf.material.PhysicalMaterial.Mechanical
                 * @static
                 * @param {mirabuf.material.PhysicalMaterial.Mechanical} message Mechanical
                 * @param {$protobuf.IConversionOptions} [options] Conversion options
                 * @returns {Object.<string,*>} Plain object
                 */
                Mechanical.toObject = function toObject(message, options) {
                    if (!options) options = {}
                    let object = {}
                    if (options.defaults) {
                        object.youngMod = 0
                        object.poissonRatio = 0
                        object.shearMod = 0
                        object.density = 0
                        object.dampingCoefficient = 0
                    }
                    if (message.youngMod != null && Object.hasOwn(message, "youngMod"))
                        object.youngMod =
                            options.json && !isFinite(message.youngMod) ? String(message.youngMod) : message.youngMod
                    if (message.poissonRatio != null && Object.hasOwn(message, "poissonRatio"))
                        object.poissonRatio =
                            options.json && !isFinite(message.poissonRatio)
                                ? String(message.poissonRatio)
                                : message.poissonRatio
                    if (message.shearMod != null && Object.hasOwn(message, "shearMod"))
                        object.shearMod =
                            options.json && !isFinite(message.shearMod) ? String(message.shearMod) : message.shearMod
                    if (message.density != null && Object.hasOwn(message, "density"))
                        object.density =
                            options.json && !isFinite(message.density) ? String(message.density) : message.density
                    if (message.dampingCoefficient != null && Object.hasOwn(message, "dampingCoefficient"))
                        object.dampingCoefficient =
                            options.json && !isFinite(message.dampingCoefficient)
                                ? String(message.dampingCoefficient)
                                : message.dampingCoefficient
                    return object
                }

                /**
                 * Converts this Mechanical to JSON.
                 * @function toJSON
                 * @memberof mirabuf.material.PhysicalMaterial.Mechanical
                 * @instance
                 * @returns {Object.<string,*>} JSON object
                 */
                Mechanical.prototype.toJSON = function toJSON() {
                    return this.constructor.toObject(this, $protobuf.util.toJSONOptions)
                }

                /**
                 * Gets the default type url for Mechanical
                 * @function getTypeUrl
                 * @memberof mirabuf.material.PhysicalMaterial.Mechanical
                 * @static
                 * @param {string} [typeUrlPrefix] your custom typeUrlPrefix(default "type.googleapis.com")
                 * @returns {string} The default type url
                 */
                Mechanical.getTypeUrl = function getTypeUrl(typeUrlPrefix) {
                    if (typeUrlPrefix === undefined) {
                        typeUrlPrefix = "type.googleapis.com"
                    }
                    return typeUrlPrefix + "/mirabuf.material.PhysicalMaterial.Mechanical"
                }

                return Mechanical
            })()

            PhysicalMaterial.Strength = (function () {
                /**
                 * Properties of a Strength.
                 * @memberof mirabuf.material.PhysicalMaterial
                 * @interface IStrength
                 * @property {number|null} [yieldStrength] MPa
                 * @property {number|null} [tensileStrength] MPa
                 * @property {boolean|null} [thermalTreatment] yes / no
                 */

                /**
                 * Constructs a new Strength.
                 * @memberof mirabuf.material.PhysicalMaterial
                 * @classdesc Strength Properties Set Definition for Simulation.
                 * @implements IStrength
                 * @constructor
                 * @param {mirabuf.material.PhysicalMaterial.IStrength=} [properties] Properties to set
                 */
                function Strength(properties) {
                    if (properties)
                        for (let keys = Object.keys(properties), i = 0; i < keys.length; ++i)
                            if (properties[keys[i]] != null) this[keys[i]] = properties[keys[i]]
                }

                /**
                 * MPa
                 * @member {number} yieldStrength
                 * @memberof mirabuf.material.PhysicalMaterial.Strength
                 * @instance
                 */
                Strength.prototype.yieldStrength = 0

                /**
                 * MPa
                 * @member {number} tensileStrength
                 * @memberof mirabuf.material.PhysicalMaterial.Strength
                 * @instance
                 */
                Strength.prototype.tensileStrength = 0

                /**
                 * yes / no
                 * @member {boolean} thermalTreatment
                 * @memberof mirabuf.material.PhysicalMaterial.Strength
                 * @instance
                 */
                Strength.prototype.thermalTreatment = false

                /**
                 * Creates a new Strength instance using the specified properties.
                 * @function create
                 * @memberof mirabuf.material.PhysicalMaterial.Strength
                 * @static
                 * @param {mirabuf.material.PhysicalMaterial.IStrength=} [properties] Properties to set
                 * @returns {mirabuf.material.PhysicalMaterial.Strength} Strength instance
                 */
                Strength.create = function create(properties) {
                    return new Strength(properties)
                }

                /**
                 * Encodes the specified Strength message. Does not implicitly {@link mirabuf.material.PhysicalMaterial.Strength.verify|verify} messages.
                 * @function encode
                 * @memberof mirabuf.material.PhysicalMaterial.Strength
                 * @static
                 * @param {mirabuf.material.PhysicalMaterial.IStrength} message Strength message or plain object to encode
                 * @param {$protobuf.Writer} [writer] Writer to encode to
                 * @returns {$protobuf.Writer} Writer
                 */
                Strength.encode = function encode(message, writer) {
                    if (!writer) writer = $Writer.create()
                    if (message.yieldStrength != null && Object.hasOwn(message, "yieldStrength"))
                        writer.uint32(/* id 1, wireType 5 =*/ 13).float(message.yieldStrength)
                    if (message.tensileStrength != null && Object.hasOwn(message, "tensileStrength"))
                        writer.uint32(/* id 2, wireType 5 =*/ 21).float(message.tensileStrength)
                    if (message.thermalTreatment != null && Object.hasOwn(message, "thermalTreatment"))
                        writer.uint32(/* id 3, wireType 0 =*/ 24).bool(message.thermalTreatment)
                    return writer
                }

                /**
                 * Encodes the specified Strength message, length delimited. Does not implicitly {@link mirabuf.material.PhysicalMaterial.Strength.verify|verify} messages.
                 * @function encodeDelimited
                 * @memberof mirabuf.material.PhysicalMaterial.Strength
                 * @static
                 * @param {mirabuf.material.PhysicalMaterial.IStrength} message Strength message or plain object to encode
                 * @param {$protobuf.Writer} [writer] Writer to encode to
                 * @returns {$protobuf.Writer} Writer
                 */
                Strength.encodeDelimited = function encodeDelimited(message, writer) {
                    return this.encode(message, writer).ldelim()
                }

                /**
                 * Decodes a Strength message from the specified reader or buffer.
                 * @function decode
                 * @memberof mirabuf.material.PhysicalMaterial.Strength
                 * @static
                 * @param {$protobuf.Reader|Uint8Array} reader Reader or buffer to decode from
                 * @param {number} [length] Message length if known beforehand
                 * @returns {mirabuf.material.PhysicalMaterial.Strength} Strength
                 * @throws {Error} If the payload is not a reader or valid buffer
                 * @throws {$protobuf.util.ProtocolError} If required fields are missing
                 */
                Strength.decode = function decode(reader, length) {
                    if (!(reader instanceof $Reader)) reader = $Reader.create(reader)
                    let end = length === undefined ? reader.len : reader.pos + length,
                        message = new $root.mirabuf.material.PhysicalMaterial.Strength()
                    while (reader.pos < end) {
                        let tag = reader.uint32()
                        switch (tag >>> 3) {
                            case 1: {
                                message.yieldStrength = reader.float()
                                break
                            }
                            case 2: {
                                message.tensileStrength = reader.float()
                                break
                            }
                            case 3: {
                                message.thermalTreatment = reader.bool()
                                break
                            }
                            default:
                                reader.skipType(tag & 7)
                                break
                        }
                    }
                    return message
                }

                /**
                 * Decodes a Strength message from the specified reader or buffer, length delimited.
                 * @function decodeDelimited
                 * @memberof mirabuf.material.PhysicalMaterial.Strength
                 * @static
                 * @param {$protobuf.Reader|Uint8Array} reader Reader or buffer to decode from
                 * @returns {mirabuf.material.PhysicalMaterial.Strength} Strength
                 * @throws {Error} If the payload is not a reader or valid buffer
                 * @throws {$protobuf.util.ProtocolError} If required fields are missing
                 */
                Strength.decodeDelimited = function decodeDelimited(reader) {
                    if (!(reader instanceof $Reader)) reader = new $Reader(reader)
                    return this.decode(reader, reader.uint32())
                }

                /**
                 * Verifies a Strength message.
                 * @function verify
                 * @memberof mirabuf.material.PhysicalMaterial.Strength
                 * @static
                 * @param {Object.<string,*>} message Plain object to verify
                 * @returns {string|null} `null` if valid, otherwise the reason why it is not
                 */
                Strength.verify = function verify(message) {
                    if (typeof message !== "object" || message === null) return "object expected"
                    if (message.yieldStrength != null && Object.hasOwn(message, "yieldStrength"))
                        if (typeof message.yieldStrength !== "number") return "yieldStrength: number expected"
                    if (message.tensileStrength != null && Object.hasOwn(message, "tensileStrength"))
                        if (typeof message.tensileStrength !== "number") return "tensileStrength: number expected"
                    if (message.thermalTreatment != null && Object.hasOwn(message, "thermalTreatment"))
                        if (typeof message.thermalTreatment !== "boolean") return "thermalTreatment: boolean expected"
                    return null
                }

                /**
                 * Creates a Strength message from a plain object. Also converts values to their respective internal types.
                 * @function fromObject
                 * @memberof mirabuf.material.PhysicalMaterial.Strength
                 * @static
                 * @param {Object.<string,*>} object Plain object
                 * @returns {mirabuf.material.PhysicalMaterial.Strength} Strength
                 */
                Strength.fromObject = function fromObject(object) {
                    if (object instanceof $root.mirabuf.material.PhysicalMaterial.Strength) return object
                    let message = new $root.mirabuf.material.PhysicalMaterial.Strength()
                    if (object.yieldStrength != null) message.yieldStrength = Number(object.yieldStrength)
                    if (object.tensileStrength != null) message.tensileStrength = Number(object.tensileStrength)
                    if (object.thermalTreatment != null) message.thermalTreatment = Boolean(object.thermalTreatment)
                    return message
                }

                /**
                 * Creates a plain object from a Strength message. Also converts values to other types if specified.
                 * @function toObject
                 * @memberof mirabuf.material.PhysicalMaterial.Strength
                 * @static
                 * @param {mirabuf.material.PhysicalMaterial.Strength} message Strength
                 * @param {$protobuf.IConversionOptions} [options] Conversion options
                 * @returns {Object.<string,*>} Plain object
                 */
                Strength.toObject = function toObject(message, options) {
                    if (!options) options = {}
                    let object = {}
                    if (options.defaults) {
                        object.yieldStrength = 0
                        object.tensileStrength = 0
                        object.thermalTreatment = false
                    }
                    if (message.yieldStrength != null && Object.hasOwn(message, "yieldStrength"))
                        object.yieldStrength =
                            options.json && !isFinite(message.yieldStrength)
                                ? String(message.yieldStrength)
                                : message.yieldStrength
                    if (message.tensileStrength != null && Object.hasOwn(message, "tensileStrength"))
                        object.tensileStrength =
                            options.json && !isFinite(message.tensileStrength)
                                ? String(message.tensileStrength)
                                : message.tensileStrength
                    if (message.thermalTreatment != null && Object.hasOwn(message, "thermalTreatment"))
                        object.thermalTreatment = message.thermalTreatment
                    return object
                }

                /**
                 * Converts this Strength to JSON.
                 * @function toJSON
                 * @memberof mirabuf.material.PhysicalMaterial.Strength
                 * @instance
                 * @returns {Object.<string,*>} JSON object
                 */
                Strength.prototype.toJSON = function toJSON() {
                    return this.constructor.toObject(this, $protobuf.util.toJSONOptions)
                }

                /**
                 * Gets the default type url for Strength
                 * @function getTypeUrl
                 * @memberof mirabuf.material.PhysicalMaterial.Strength
                 * @static
                 * @param {string} [typeUrlPrefix] your custom typeUrlPrefix(default "type.googleapis.com")
                 * @returns {string} The default type url
                 */
                Strength.getTypeUrl = function getTypeUrl(typeUrlPrefix) {
                    if (typeUrlPrefix === undefined) {
                        typeUrlPrefix = "type.googleapis.com"
                    }
                    return typeUrlPrefix + "/mirabuf.material.PhysicalMaterial.Strength"
                }

                return Strength
            })()

            return PhysicalMaterial
        })()

        return material
    })()

    mirabuf.signal = (function () {
        /**
         * Namespace signal.
         * @memberof mirabuf
         * @namespace
         */
        const signal = {}

        signal.Signals = (function () {
            /**
             * Properties of a Signals.
             * @memberof mirabuf.signal
             * @interface ISignals
             * @property {mirabuf.IInfo|null} [info] Has identifiable data (id, name, version)
             * @property {Object.<string,mirabuf.signal.ISignal>|null} [signalMap] Contains a full collection of symbols
             */

            /**
             * Constructs a new Signals.
             * @memberof mirabuf.signal
             * @classdesc Signals is a container for all of the potential signals.
             * @implements ISignals
             * @constructor
             * @param {mirabuf.signal.ISignals=} [properties] Properties to set
             */
            function Signals(properties) {
                this.signalMap = {}
                if (properties)
                    for (let keys = Object.keys(properties), i = 0; i < keys.length; ++i)
                        if (properties[keys[i]] != null) this[keys[i]] = properties[keys[i]]
            }

            /**
             * Has identifiable data (id, name, version)
             * @member {mirabuf.IInfo|null|undefined} info
             * @memberof mirabuf.signal.Signals
             * @instance
             */
            Signals.prototype.info = null

            /**
             * Contains a full collection of symbols
             * @member {Object.<string,mirabuf.signal.ISignal>} signalMap
             * @memberof mirabuf.signal.Signals
             * @instance
             */
            Signals.prototype.signalMap = $util.emptyObject

            /**
             * Creates a new Signals instance using the specified properties.
             * @function create
             * @memberof mirabuf.signal.Signals
             * @static
             * @param {mirabuf.signal.ISignals=} [properties] Properties to set
             * @returns {mirabuf.signal.Signals} Signals instance
             */
            Signals.create = function create(properties) {
                return new Signals(properties)
            }

            /**
             * Encodes the specified Signals message. Does not implicitly {@link mirabuf.signal.Signals.verify|verify} messages.
             * @function encode
             * @memberof mirabuf.signal.Signals
             * @static
             * @param {mirabuf.signal.ISignals} message Signals message or plain object to encode
             * @param {$protobuf.Writer} [writer] Writer to encode to
             * @returns {$protobuf.Writer} Writer
             */
            Signals.encode = function encode(message, writer) {
                if (!writer) writer = $Writer.create()
                if (message.info != null && Object.hasOwn(message, "info"))
                    $root.mirabuf.Info.encode(message.info, writer.uint32(/* id 1, wireType 2 =*/ 10).fork()).ldelim()
                if (message.signalMap != null && Object.hasOwn(message, "signalMap"))
                    for (let keys = Object.keys(message.signalMap), i = 0; i < keys.length; ++i) {
                        writer
                            .uint32(/* id 2, wireType 2 =*/ 18)
                            .fork()
                            .uint32(/* id 1, wireType 2 =*/ 10)
                            .string(keys[i])
                        $root.mirabuf.signal.Signal.encode(
                            message.signalMap[keys[i]],
                            writer.uint32(/* id 2, wireType 2 =*/ 18).fork()
                        )
                            .ldelim()
                            .ldelim()
                    }
                return writer
            }

            /**
             * Encodes the specified Signals message, length delimited. Does not implicitly {@link mirabuf.signal.Signals.verify|verify} messages.
             * @function encodeDelimited
             * @memberof mirabuf.signal.Signals
             * @static
             * @param {mirabuf.signal.ISignals} message Signals message or plain object to encode
             * @param {$protobuf.Writer} [writer] Writer to encode to
             * @returns {$protobuf.Writer} Writer
             */
            Signals.encodeDelimited = function encodeDelimited(message, writer) {
                return this.encode(message, writer).ldelim()
            }

            /**
             * Decodes a Signals message from the specified reader or buffer.
             * @function decode
             * @memberof mirabuf.signal.Signals
             * @static
             * @param {$protobuf.Reader|Uint8Array} reader Reader or buffer to decode from
             * @param {number} [length] Message length if known beforehand
             * @returns {mirabuf.signal.Signals} Signals
             * @throws {Error} If the payload is not a reader or valid buffer
             * @throws {$protobuf.util.ProtocolError} If required fields are missing
             */
            Signals.decode = function decode(reader, length) {
                if (!(reader instanceof $Reader)) reader = $Reader.create(reader)
                let end = length === undefined ? reader.len : reader.pos + length,
                    message = new $root.mirabuf.signal.Signals(),
                    key,
                    value
                while (reader.pos < end) {
                    let tag = reader.uint32()
                    switch (tag >>> 3) {
                        case 1: {
                            message.info = $root.mirabuf.Info.decode(reader, reader.uint32())
                            break
                        }
                        case 2: {
                            if (message.signalMap === $util.emptyObject) message.signalMap = {}
                            let end2 = reader.uint32() + reader.pos
                            key = ""
                            value = null
                            while (reader.pos < end2) {
                                let tag2 = reader.uint32()
                                switch (tag2 >>> 3) {
                                    case 1:
                                        key = reader.string()
                                        break
                                    case 2:
                                        value = $root.mirabuf.signal.Signal.decode(reader, reader.uint32())
                                        break
                                    default:
                                        reader.skipType(tag2 & 7)
                                        break
                                }
                            }
                            message.signalMap[key] = value
                            break
                        }
                        default:
                            reader.skipType(tag & 7)
                            break
                    }
                }
                return message
            }

            /**
             * Decodes a Signals message from the specified reader or buffer, length delimited.
             * @function decodeDelimited
             * @memberof mirabuf.signal.Signals
             * @static
             * @param {$protobuf.Reader|Uint8Array} reader Reader or buffer to decode from
             * @returns {mirabuf.signal.Signals} Signals
             * @throws {Error} If the payload is not a reader or valid buffer
             * @throws {$protobuf.util.ProtocolError} If required fields are missing
             */
            Signals.decodeDelimited = function decodeDelimited(reader) {
                if (!(reader instanceof $Reader)) reader = new $Reader(reader)
                return this.decode(reader, reader.uint32())
            }

            /**
             * Verifies a Signals message.
             * @function verify
             * @memberof mirabuf.signal.Signals
             * @static
             * @param {Object.<string,*>} message Plain object to verify
             * @returns {string|null} `null` if valid, otherwise the reason why it is not
             */
            Signals.verify = function verify(message) {
                if (typeof message !== "object" || message === null) return "object expected"
                if (message.info != null && Object.hasOwn(message, "info")) {
                    let error = $root.mirabuf.Info.verify(message.info)
                    if (error) return "info." + error
                }
                if (message.signalMap != null && Object.hasOwn(message, "signalMap")) {
                    if (!$util.isObject(message.signalMap)) return "signalMap: object expected"
                    let key = Object.keys(message.signalMap)
                    for (let i = 0; i < key.length; ++i) {
                        let error = $root.mirabuf.signal.Signal.verify(message.signalMap[key[i]])
                        if (error) return "signalMap." + error
                    }
                }
                return null
            }

            /**
             * Creates a Signals message from a plain object. Also converts values to their respective internal types.
             * @function fromObject
             * @memberof mirabuf.signal.Signals
             * @static
             * @param {Object.<string,*>} object Plain object
             * @returns {mirabuf.signal.Signals} Signals
             */
            Signals.fromObject = function fromObject(object) {
                if (object instanceof $root.mirabuf.signal.Signals) return object
                let message = new $root.mirabuf.signal.Signals()
                if (object.info != null) {
                    if (typeof object.info !== "object")
                        throw TypeError(".mirabuf.signal.Signals.info: object expected")
                    message.info = $root.mirabuf.Info.fromObject(object.info)
                }
                if (object.signalMap) {
                    if (typeof object.signalMap !== "object")
                        throw TypeError(".mirabuf.signal.Signals.signalMap: object expected")
                    message.signalMap = {}
                    for (let keys = Object.keys(object.signalMap), i = 0; i < keys.length; ++i) {
                        if (typeof object.signalMap[keys[i]] !== "object")
                            throw TypeError(".mirabuf.signal.Signals.signalMap: object expected")
                        message.signalMap[keys[i]] = $root.mirabuf.signal.Signal.fromObject(object.signalMap[keys[i]])
                    }
                }
                return message
            }

            /**
             * Creates a plain object from a Signals message. Also converts values to other types if specified.
             * @function toObject
             * @memberof mirabuf.signal.Signals
             * @static
             * @param {mirabuf.signal.Signals} message Signals
             * @param {$protobuf.IConversionOptions} [options] Conversion options
             * @returns {Object.<string,*>} Plain object
             */
            Signals.toObject = function toObject(message, options) {
                if (!options) options = {}
                let object = {}
                if (options.objects || options.defaults) object.signalMap = {}
                if (options.defaults) object.info = null
                if (message.info != null && Object.hasOwn(message, "info"))
                    object.info = $root.mirabuf.Info.toObject(message.info, options)
                let keys2
                if (message.signalMap && (keys2 = Object.keys(message.signalMap)).length) {
                    object.signalMap = {}
                    for (let j = 0; j < keys2.length; ++j)
                        object.signalMap[keys2[j]] = $root.mirabuf.signal.Signal.toObject(
                            message.signalMap[keys2[j]],
                            options
                        )
                }
                return object
            }

            /**
             * Converts this Signals to JSON.
             * @function toJSON
             * @memberof mirabuf.signal.Signals
             * @instance
             * @returns {Object.<string,*>} JSON object
             */
            Signals.prototype.toJSON = function toJSON() {
                return this.constructor.toObject(this, $protobuf.util.toJSONOptions)
            }

            /**
             * Gets the default type url for Signals
             * @function getTypeUrl
             * @memberof mirabuf.signal.Signals
             * @static
             * @param {string} [typeUrlPrefix] your custom typeUrlPrefix(default "type.googleapis.com")
             * @returns {string} The default type url
             */
            Signals.getTypeUrl = function getTypeUrl(typeUrlPrefix) {
                if (typeUrlPrefix === undefined) {
                    typeUrlPrefix = "type.googleapis.com"
                }
                return typeUrlPrefix + "/mirabuf.signal.Signals"
            }

            return Signals
        })()

        /**
         * IOType is a way to specify Input or Output.
         * @name mirabuf.signal.IOType
         * @enum {number}
         * @property {number} INPUT=0 Input Signal
         * @property {number} OUTPUT=1 Output Signal
         */
        signal.IOType = (function () {
            const valuesById = {},
                values = Object.create(valuesById)
            values[(valuesById[0] = "INPUT")] = 0
            values[(valuesById[1] = "OUTPUT")] = 1
            return values
        })()

        /**
         * DeviceType needs to be a type of device that has a supported connection
         * As well as a signal frmae but that can come later
         * @name mirabuf.signal.DeviceType
         * @enum {number}
         * @property {number} PWM=0 PWM value
         * @property {number} Digital=1 Digital value
         * @property {number} Analog=2 Analog value
         * @property {number} I2C=3 I2C value
         * @property {number} CANBUS=4 CANBUS value
         * @property {number} CUSTOM=5 CUSTOM value
         */
        signal.DeviceType = (function () {
            const valuesById = {},
                values = Object.create(valuesById)
            values[(valuesById[0] = "PWM")] = 0
            values[(valuesById[1] = "Digital")] = 1
            values[(valuesById[2] = "Analog")] = 2
            values[(valuesById[3] = "I2C")] = 3
            values[(valuesById[4] = "CANBUS")] = 4
            values[(valuesById[5] = "CUSTOM")] = 5
            return values
        })()

        signal.Signal = (function () {
            /**
             * Properties of a Signal.
             * @memberof mirabuf.signal
             * @interface ISignal
             * @property {mirabuf.IInfo|null} [info] Has identifiable data (id, name, version)
             * @property {mirabuf.signal.IOType|null} [io] Is this a Input or Output
             * @property {string|null} [customType] The name of a custom input type that is not listed as a device type
             * @property {number|null} [signalId] ID for a given signal that exists... PWM 2, CANBUS 4
             * @property {mirabuf.signal.DeviceType|null} [deviceType] Enum for device type that should always be set
             */

            /**
             * Constructs a new Signal.
             * @memberof mirabuf.signal
             * @classdesc Signal is a way to define a controlling signal.
             *
             * TODO: Add Origin
             * TODO: Decide how this is linked to a exported object
             * @implements ISignal
             * @constructor
             * @param {mirabuf.signal.ISignal=} [properties] Properties to set
             */
            function Signal(properties) {
                if (properties)
                    for (let keys = Object.keys(properties), i = 0; i < keys.length; ++i)
                        if (properties[keys[i]] != null) this[keys[i]] = properties[keys[i]]
            }

            /**
             * Has identifiable data (id, name, version)
             * @member {mirabuf.IInfo|null|undefined} info
             * @memberof mirabuf.signal.Signal
             * @instance
             */
            Signal.prototype.info = null

            /**
             * Is this a Input or Output
             * @member {mirabuf.signal.IOType} io
             * @memberof mirabuf.signal.Signal
             * @instance
             */
            Signal.prototype.io = 0

            /**
             * The name of a custom input type that is not listed as a device type
             * @member {string} customType
             * @memberof mirabuf.signal.Signal
             * @instance
             */
            Signal.prototype.customType = ""

            /**
             * ID for a given signal that exists... PWM 2, CANBUS 4
             * @member {number} signalId
             * @memberof mirabuf.signal.Signal
             * @instance
             */
            Signal.prototype.signalId = 0

            /**
             * Enum for device type that should always be set
             * @member {mirabuf.signal.DeviceType} deviceType
             * @memberof mirabuf.signal.Signal
             * @instance
             */
            Signal.prototype.deviceType = 0

            /**
             * Creates a new Signal instance using the specified properties.
             * @function create
             * @memberof mirabuf.signal.Signal
             * @static
             * @param {mirabuf.signal.ISignal=} [properties] Properties to set
             * @returns {mirabuf.signal.Signal} Signal instance
             */
            Signal.create = function create(properties) {
                return new Signal(properties)
            }

            /**
             * Encodes the specified Signal message. Does not implicitly {@link mirabuf.signal.Signal.verify|verify} messages.
             * @function encode
             * @memberof mirabuf.signal.Signal
             * @static
             * @param {mirabuf.signal.ISignal} message Signal message or plain object to encode
             * @param {$protobuf.Writer} [writer] Writer to encode to
             * @returns {$protobuf.Writer} Writer
             */
            Signal.encode = function encode(message, writer) {
                if (!writer) writer = $Writer.create()
                if (message.info != null && Object.hasOwn(message, "info"))
                    $root.mirabuf.Info.encode(message.info, writer.uint32(/* id 1, wireType 2 =*/ 10).fork()).ldelim()
                if (message.io != null && Object.hasOwn(message, "io"))
                    writer.uint32(/* id 2, wireType 0 =*/ 16).int32(message.io)
                if (message.customType != null && Object.hasOwn(message, "customType"))
                    writer.uint32(/* id 3, wireType 2 =*/ 26).string(message.customType)
                if (message.signalId != null && Object.hasOwn(message, "signalId"))
                    writer.uint32(/* id 4, wireType 0 =*/ 32).uint32(message.signalId)
                if (message.deviceType != null && Object.hasOwn(message, "deviceType"))
                    writer.uint32(/* id 5, wireType 0 =*/ 40).int32(message.deviceType)
                return writer
            }

            /**
             * Encodes the specified Signal message, length delimited. Does not implicitly {@link mirabuf.signal.Signal.verify|verify} messages.
             * @function encodeDelimited
             * @memberof mirabuf.signal.Signal
             * @static
             * @param {mirabuf.signal.ISignal} message Signal message or plain object to encode
             * @param {$protobuf.Writer} [writer] Writer to encode to
             * @returns {$protobuf.Writer} Writer
             */
            Signal.encodeDelimited = function encodeDelimited(message, writer) {
                return this.encode(message, writer).ldelim()
            }

            /**
             * Decodes a Signal message from the specified reader or buffer.
             * @function decode
             * @memberof mirabuf.signal.Signal
             * @static
             * @param {$protobuf.Reader|Uint8Array} reader Reader or buffer to decode from
             * @param {number} [length] Message length if known beforehand
             * @returns {mirabuf.signal.Signal} Signal
             * @throws {Error} If the payload is not a reader or valid buffer
             * @throws {$protobuf.util.ProtocolError} If required fields are missing
             */
            Signal.decode = function decode(reader, length) {
                if (!(reader instanceof $Reader)) reader = $Reader.create(reader)
                let end = length === undefined ? reader.len : reader.pos + length,
                    message = new $root.mirabuf.signal.Signal()
                while (reader.pos < end) {
                    let tag = reader.uint32()
                    switch (tag >>> 3) {
                        case 1: {
                            message.info = $root.mirabuf.Info.decode(reader, reader.uint32())
                            break
                        }
                        case 2: {
                            message.io = reader.int32()
                            break
                        }
                        case 3: {
                            message.customType = reader.string()
                            break
                        }
                        case 4: {
                            message.signalId = reader.uint32()
                            break
                        }
                        case 5: {
                            message.deviceType = reader.int32()
                            break
                        }
                        default:
                            reader.skipType(tag & 7)
                            break
                    }
                }
                return message
            }

            /**
             * Decodes a Signal message from the specified reader or buffer, length delimited.
             * @function decodeDelimited
             * @memberof mirabuf.signal.Signal
             * @static
             * @param {$protobuf.Reader|Uint8Array} reader Reader or buffer to decode from
             * @returns {mirabuf.signal.Signal} Signal
             * @throws {Error} If the payload is not a reader or valid buffer
             * @throws {$protobuf.util.ProtocolError} If required fields are missing
             */
            Signal.decodeDelimited = function decodeDelimited(reader) {
                if (!(reader instanceof $Reader)) reader = new $Reader(reader)
                return this.decode(reader, reader.uint32())
            }

            /**
             * Verifies a Signal message.
             * @function verify
             * @memberof mirabuf.signal.Signal
             * @static
             * @param {Object.<string,*>} message Plain object to verify
             * @returns {string|null} `null` if valid, otherwise the reason why it is not
             */
            Signal.verify = function verify(message) {
                if (typeof message !== "object" || message === null) return "object expected"
                if (message.info != null && Object.hasOwn(message, "info")) {
                    let error = $root.mirabuf.Info.verify(message.info)
                    if (error) return "info." + error
                }
                if (message.io != null && Object.hasOwn(message, "io"))
                    switch (message.io) {
                        default:
                            return "io: enum value expected"
                        case 0:
                        case 1:
                            break
                    }
                if (message.customType != null && Object.hasOwn(message, "customType"))
                    if (!$util.isString(message.customType)) return "customType: string expected"
                if (message.signalId != null && Object.hasOwn(message, "signalId"))
                    if (!$util.isInteger(message.signalId)) return "signalId: integer expected"
                if (message.deviceType != null && Object.hasOwn(message, "deviceType"))
                    switch (message.deviceType) {
                        default:
                            return "deviceType: enum value expected"
                        case 0:
                        case 1:
                        case 2:
                        case 3:
                        case 4:
                        case 5:
                            break
                    }
                return null
            }

            /**
             * Creates a Signal message from a plain object. Also converts values to their respective internal types.
             * @function fromObject
             * @memberof mirabuf.signal.Signal
             * @static
             * @param {Object.<string,*>} object Plain object
             * @returns {mirabuf.signal.Signal} Signal
             */
            Signal.fromObject = function fromObject(object) {
                if (object instanceof $root.mirabuf.signal.Signal) return object
                let message = new $root.mirabuf.signal.Signal()
                if (object.info != null) {
                    if (typeof object.info !== "object") throw TypeError(".mirabuf.signal.Signal.info: object expected")
                    message.info = $root.mirabuf.Info.fromObject(object.info)
                }
                switch (object.io) {
                    default:
                        if (typeof object.io === "number") {
                            message.io = object.io
                            break
                        }
                        break
                    case "INPUT":
                    case 0:
                        message.io = 0
                        break
                    case "OUTPUT":
                    case 1:
                        message.io = 1
                        break
                }
                if (object.customType != null) message.customType = String(object.customType)
                if (object.signalId != null) message.signalId = object.signalId >>> 0
                switch (object.deviceType) {
                    default:
                        if (typeof object.deviceType === "number") {
                            message.deviceType = object.deviceType
                            break
                        }
                        break
                    case "PWM":
                    case 0:
                        message.deviceType = 0
                        break
                    case "Digital":
                    case 1:
                        message.deviceType = 1
                        break
                    case "Analog":
                    case 2:
                        message.deviceType = 2
                        break
                    case "I2C":
                    case 3:
                        message.deviceType = 3
                        break
                    case "CANBUS":
                    case 4:
                        message.deviceType = 4
                        break
                    case "CUSTOM":
                    case 5:
                        message.deviceType = 5
                        break
                }
                return message
            }

            /**
             * Creates a plain object from a Signal message. Also converts values to other types if specified.
             * @function toObject
             * @memberof mirabuf.signal.Signal
             * @static
             * @param {mirabuf.signal.Signal} message Signal
             * @param {$protobuf.IConversionOptions} [options] Conversion options
             * @returns {Object.<string,*>} Plain object
             */
            Signal.toObject = function toObject(message, options) {
                if (!options) options = {}
                let object = {}
                if (options.defaults) {
                    object.info = null
                    object.io = options.enums === String ? "INPUT" : 0
                    object.customType = ""
                    object.signalId = 0
                    object.deviceType = options.enums === String ? "PWM" : 0
                }
                if (message.info != null && Object.hasOwn(message, "info"))
                    object.info = $root.mirabuf.Info.toObject(message.info, options)
                if (message.io != null && Object.hasOwn(message, "io"))
                    object.io =
                        options.enums === String
                            ? $root.mirabuf.signal.IOType[message.io] === undefined
                                ? message.io
                                : $root.mirabuf.signal.IOType[message.io]
                            : message.io
                if (message.customType != null && Object.hasOwn(message, "customType"))
                    object.customType = message.customType
                if (message.signalId != null && Object.hasOwn(message, "signalId")) object.signalId = message.signalId
                if (message.deviceType != null && Object.hasOwn(message, "deviceType"))
                    object.deviceType =
                        options.enums === String
                            ? $root.mirabuf.signal.DeviceType[message.deviceType] === undefined
                                ? message.deviceType
                                : $root.mirabuf.signal.DeviceType[message.deviceType]
                            : message.deviceType
                return object
            }

            /**
             * Converts this Signal to JSON.
             * @function toJSON
             * @memberof mirabuf.signal.Signal
             * @instance
             * @returns {Object.<string,*>} JSON object
             */
            Signal.prototype.toJSON = function toJSON() {
                return this.constructor.toObject(this, $protobuf.util.toJSONOptions)
            }

            /**
             * Gets the default type url for Signal
             * @function getTypeUrl
             * @memberof mirabuf.signal.Signal
             * @static
             * @param {string} [typeUrlPrefix] your custom typeUrlPrefix(default "type.googleapis.com")
             * @returns {string} The default type url
             */
            Signal.getTypeUrl = function getTypeUrl(typeUrlPrefix) {
                if (typeUrlPrefix === undefined) {
                    typeUrlPrefix = "type.googleapis.com"
                }
                return typeUrlPrefix + "/mirabuf.signal.Signal"
            }

            return Signal
        })()

        return signal
    })()

    return mirabuf
})())

export { $root as default }
