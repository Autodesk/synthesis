// biome-ignore-all lint/correctness/noUnusedVariables: We need the entire mirabuf spec in this file, but might not need to use all of it right now
// biome-ignore-all lint/style/noNamespace: We need the entire mirabuf spec in this file, but don't have a choice about how it was contructed
import * as $protobuf from "protobufjs"
import Long = require("long")
/** Namespace mirabuf. */
export namespace mirabuf {
    /** Properties of an Assembly. */
    interface IAssembly {
        /** Basic information (name, Author, etc) */
        info?: mirabuf.IInfo | null

        /** All of the data in the assembly */
        data?: mirabuf.IAssemblyData | null

        /** Can it be effected by the simulation dynamically */
        dynamic?: boolean | null

        /** Overall physical data of the assembly */
        physicalData?: mirabuf.IPhysicalProperties | null

        /** The Design hierarchy represented by Part Refs - The first object is a root container for all top level items */
        designHierarchy?: mirabuf.IGraphContainer | null

        /** The Joint hierarchy for compound shapes */
        jointHierarchy?: mirabuf.IGraphContainer | null

        /** The Transform in space currently */
        transform?: mirabuf.ITransform | null

        /** Optional thumbnail saved from Fusion 360 or scraped from previous configuration */
        thumbnail?: mirabuf.IThumbnail | null
    }

    /**
     * Assembly
     * Base Design to be interacted with
     * THIS IS THE CURRENT FILE EXPORTED
     */
    class Assembly implements IAssembly {
        /**
         * Constructs a new Assembly.
         * @param [properties] Properties to set
         */
        constructor(properties?: mirabuf.IAssembly)

        /** Basic information (name, Author, etc) */
        public info?: mirabuf.IInfo | null

        /** All of the data in the assembly */
        public data?: mirabuf.IAssemblyData | null

        /** Can it be effected by the simulation dynamically */
        public dynamic: boolean

        /** Overall physical data of the assembly */
        public physicalData?: mirabuf.IPhysicalProperties | null

        /** The Design hierarchy represented by Part Refs - The first object is a root container for all top level items */
        public designHierarchy?: mirabuf.IGraphContainer | null

        /** The Joint hierarchy for compound shapes */
        public jointHierarchy?: mirabuf.IGraphContainer | null

        /** The Transform in space currently */
        public transform?: mirabuf.ITransform | null

        /** Optional thumbnail saved from Fusion 360 or scraped from previous configuration */
        public thumbnail?: mirabuf.IThumbnail | null

        /**
         * Creates a new Assembly instance using the specified properties.
         * @param [properties] Properties to set
         * @returns Assembly instance
         */
        public static create(properties?: mirabuf.IAssembly): mirabuf.Assembly

        /**
         * Encodes the specified Assembly message. Does not implicitly {@link mirabuf.Assembly.verify|verify} messages.
         * @param message Assembly message or plain object to encode
         * @param [writer] Writer to encode to
         * @returns Writer
         */
        public static encode(message: mirabuf.IAssembly, writer?: $protobuf.Writer): $protobuf.Writer

        /**
         * Encodes the specified Assembly message, length delimited. Does not implicitly {@link mirabuf.Assembly.verify|verify} messages.
         * @param message Assembly message or plain object to encode
         * @param [writer] Writer to encode to
         * @returns Writer
         */
        public static encodeDelimited(message: mirabuf.IAssembly, writer?: $protobuf.Writer): $protobuf.Writer

        /**
         * Decodes an Assembly message from the specified reader or buffer.
         * @param reader Reader or buffer to decode from
         * @param [length] Message length if known beforehand
         * @returns Assembly
         * @throws {Error} If the payload is not a reader or valid buffer
         * @throws {$protobuf.util.ProtocolError} If required fields are missing
         */
        public static decode(reader: $protobuf.Reader | Uint8Array, length?: number): mirabuf.Assembly

        /**
         * Decodes an Assembly message from the specified reader or buffer, length delimited.
         * @param reader Reader or buffer to decode from
         * @returns Assembly
         * @throws {Error} If the payload is not a reader or valid buffer
         * @throws {$protobuf.util.ProtocolError} If required fields are missing
         */
        public static decodeDelimited(reader: $protobuf.Reader | Uint8Array): mirabuf.Assembly

        /**
         * Verifies an Assembly message.
         * @param message Plain object to verify
         * @returns `null` if valid, otherwise the reason why it is not
         */
        public static verify(message: { [k: string]: unknown }): string | null

        /**
         * Creates an Assembly message from a plain object. Also converts values to their respective internal types.
         * @param object Plain object
         * @returns Assembly
         */
        public static fromObject(object: { [k: string]: unknown }): mirabuf.Assembly

        /**
         * Creates a plain object from an Assembly message. Also converts values to other types if specified.
         * @param message Assembly
         * @param [options] Conversion options
         * @returns Plain object
         */
        public static toObject(
            message: mirabuf.Assembly,
            options?: $protobuf.IConversionOptions
        ): { [k: string]: unknown }

        /**
         * Converts this Assembly to JSON.
         * @returns JSON object
         */
        public toJSON(): { [k: string]: unknown }

        /**
         * Gets the default type url for Assembly
         * @param [typeUrlPrefix] your custom typeUrlPrefix(default "type.googleapis.com")
         * @returns The default type url
         */
        public static getTypeUrl(typeUrlPrefix?: string): string
    }

    /** Properties of an AssemblyData. */
    interface IAssemblyData {
        /** Meshes and Design Objects */
        parts?: mirabuf.IParts | null

        /** Joint Definition Set */
        joints?: mirabuf.joint.IJoints | null

        /** Appearance and Physical Material Set */
        materials?: mirabuf.material.IMaterials | null

        /** AssemblyData signals */
        signals?: mirabuf.signal.ISignals | null
    }

    /** Data used to construct the assembly */
    class AssemblyData implements IAssemblyData {
        /**
         * Constructs a new AssemblyData.
         * @param [properties] Properties to set
         */
        constructor(properties?: mirabuf.IAssemblyData)

        /** Meshes and Design Objects */
        public parts?: mirabuf.IParts | null

        /** Joint Definition Set */
        public joints?: mirabuf.joint.IJoints | null

        /** Appearance and Physical Material Set */
        public materials?: mirabuf.material.IMaterials | null

        /** AssemblyData signals. */
        public signals?: mirabuf.signal.ISignals | null

        /**
         * Creates a new AssemblyData instance using the specified properties.
         * @param [properties] Properties to set
         * @returns AssemblyData instance
         */
        public static create(properties?: mirabuf.IAssemblyData): mirabuf.AssemblyData

        /**
         * Encodes the specified AssemblyData message. Does not implicitly {@link mirabuf.AssemblyData.verify|verify} messages.
         * @param message AssemblyData message or plain object to encode
         * @param [writer] Writer to encode to
         * @returns Writer
         */
        public static encode(message: mirabuf.IAssemblyData, writer?: $protobuf.Writer): $protobuf.Writer

        /**
         * Encodes the specified AssemblyData message, length delimited. Does not implicitly {@link mirabuf.AssemblyData.verify|verify} messages.
         * @param message AssemblyData message or plain object to encode
         * @param [writer] Writer to encode to
         * @returns Writer
         */
        public static encodeDelimited(message: mirabuf.IAssemblyData, writer?: $protobuf.Writer): $protobuf.Writer

        /**
         * Decodes an AssemblyData message from the specified reader or buffer.
         * @param reader Reader or buffer to decode from
         * @param [length] Message length if known beforehand
         * @returns AssemblyData
         * @throws {Error} If the payload is not a reader or valid buffer
         * @throws {$protobuf.util.ProtocolError} If required fields are missing
         */
        public static decode(reader: $protobuf.Reader | Uint8Array, length?: number): mirabuf.AssemblyData

        /**
         * Decodes an AssemblyData message from the specified reader or buffer, length delimited.
         * @param reader Reader or buffer to decode from
         * @returns AssemblyData
         * @throws {Error} If the payload is not a reader or valid buffer
         * @throws {$protobuf.util.ProtocolError} If required fields are missing
         */
        public static decodeDelimited(reader: $protobuf.Reader | Uint8Array): mirabuf.AssemblyData

        /**
         * Verifies an AssemblyData message.
         * @param message Plain object to verify
         * @returns `null` if valid, otherwise the reason why it is not
         */
        public static verify(message: { [k: string]: unknown }): string | null

        /**
         * Creates an AssemblyData message from a plain object. Also converts values to their respective internal types.
         * @param object Plain object
         * @returns AssemblyData
         */
        public static fromObject(object: { [k: string]: unknown }): mirabuf.AssemblyData

        /**
         * Creates a plain object from an AssemblyData message. Also converts values to other types if specified.
         * @param message AssemblyData
         * @param [options] Conversion options
         * @returns Plain object
         */
        public static toObject(
            message: mirabuf.AssemblyData,
            options?: $protobuf.IConversionOptions
        ): { [k: string]: unknown }

        /**
         * Converts this AssemblyData to JSON.
         * @returns JSON object
         */
        public toJSON(): { [k: string]: unknown }

        /**
         * Gets the default type url for AssemblyData
         * @param [typeUrlPrefix] your custom typeUrlPrefix(default "type.googleapis.com")
         * @returns The default type url
         */
        public static getTypeUrl(typeUrlPrefix?: string): string
    }

    /** Properties of a Parts. */
    interface IParts {
        /** Part name, version, GUID */
        info?: mirabuf.IInfo | null

        /** Map of the Exported Part Definitions */
        partDefinitions?: { [k: string]: mirabuf.IPartDefinition } | null

        /** Map of the Exported Parts that make up the object */
        partInstances?: { [k: string]: mirabuf.IPartInstance } | null

        /** other associated data that can be used */
        userData?: mirabuf.IUserData | null
    }

    /** Represents a Parts. */
    class Parts implements IParts {
        /**
         * Constructs a new Parts.
         * @param [properties] Properties to set
         */
        constructor(properties?: mirabuf.IParts)

        /** Part name, version, GUID */
        public info?: mirabuf.IInfo | null

        /** Map of the Exported Part Definitions */
        public partDefinitions: { [k: string]: mirabuf.IPartDefinition }

        /** Map of the Exported Parts that make up the object */
        public partInstances: { [k: string]: mirabuf.IPartInstance }

        /** other associated data that can be used */
        public userData?: mirabuf.IUserData | null

        /**
         * Creates a new Parts instance using the specified properties.
         * @param [properties] Properties to set
         * @returns Parts instance
         */
        public static create(properties?: mirabuf.IParts): mirabuf.Parts

        /**
         * Encodes the specified Parts message. Does not implicitly {@link mirabuf.Parts.verify|verify} messages.
         * @param message Parts message or plain object to encode
         * @param [writer] Writer to encode to
         * @returns Writer
         */
        public static encode(message: mirabuf.IParts, writer?: $protobuf.Writer): $protobuf.Writer

        /**
         * Encodes the specified Parts message, length delimited. Does not implicitly {@link mirabuf.Parts.verify|verify} messages.
         * @param message Parts message or plain object to encode
         * @param [writer] Writer to encode to
         * @returns Writer
         */
        public static encodeDelimited(message: mirabuf.IParts, writer?: $protobuf.Writer): $protobuf.Writer

        /**
         * Decodes a Parts message from the specified reader or buffer.
         * @param reader Reader or buffer to decode from
         * @param [length] Message length if known beforehand
         * @returns Parts
         * @throws {Error} If the payload is not a reader or valid buffer
         * @throws {$protobuf.util.ProtocolError} If required fields are missing
         */
        public static decode(reader: $protobuf.Reader | Uint8Array, length?: number): mirabuf.Parts

        /**
         * Decodes a Parts message from the specified reader or buffer, length delimited.
         * @param reader Reader or buffer to decode from
         * @returns Parts
         * @throws {Error} If the payload is not a reader or valid buffer
         * @throws {$protobuf.util.ProtocolError} If required fields are missing
         */
        public static decodeDelimited(reader: $protobuf.Reader | Uint8Array): mirabuf.Parts

        /**
         * Verifies a Parts message.
         * @param message Plain object to verify
         * @returns `null` if valid, otherwise the reason why it is not
         */
        public static verify(message: { [k: string]: unknown }): string | null

        /**
         * Creates a Parts message from a plain object. Also converts values to their respective internal types.
         * @param object Plain object
         * @returns Parts
         */
        public static fromObject(object: { [k: string]: unknown }): mirabuf.Parts

        /**
         * Creates a plain object from a Parts message. Also converts values to other types if specified.
         * @param message Parts
         * @param [options] Conversion options
         * @returns Plain object
         */
        public static toObject(message: mirabuf.Parts, options?: $protobuf.IConversionOptions): { [k: string]: unknown }

        /**
         * Converts this Parts to JSON.
         * @returns JSON object
         */
        public toJSON(): { [k: string]: unknown }

        /**
         * Gets the default type url for Parts
         * @param [typeUrlPrefix] your custom typeUrlPrefix(default "type.googleapis.com")
         * @returns The default type url
         */
        public static getTypeUrl(typeUrlPrefix?: string): string
    }

    /** Properties of a PartDefinition. */
    interface IPartDefinition {
        /** Information about version - id - name */
        info?: mirabuf.IInfo | null

        /** Physical data associated with Part */
        physicalData?: mirabuf.IPhysicalProperties | null

        /** Base Transform applied - Most Likely Identity Matrix */
        baseTransform?: mirabuf.ITransform | null

        /** Mesh Bodies to populate part */
        bodies?: mirabuf.IBody[] | null

        /** Optional value to state whether an object is a dynamic object in a static assembly - all children are also considered overriden */
        dynamic?: boolean | null

        /** Optional value for overriding the friction value 0-1 */
        frictionOverride?: number | null

        /** Optional value for overriding an indiviaul object's mass */
        massOverride?: number | null
    }

    /**
     * Part Definition
     * Unique Definition of a part that can be replicated.
     * Useful for keeping the object counter down in the scene.
     */
    class PartDefinition implements IPartDefinition {
        /**
         * Constructs a new PartDefinition.
         * @param [properties] Properties to set
         */
        constructor(properties?: mirabuf.IPartDefinition)

        /** Information about version - id - name */
        public info?: mirabuf.IInfo | null

        /** Physical data associated with Part */
        public physicalData?: mirabuf.IPhysicalProperties | null

        /** Base Transform applied - Most Likely Identity Matrix */
        public baseTransform?: mirabuf.ITransform | null

        /** Mesh Bodies to populate part */
        public bodies: mirabuf.IBody[]

        /** Optional value to state whether an object is a dynamic object in a static assembly - all children are also considered overriden */
        public dynamic: boolean

        /** Optional value for overriding the friction value 0-1 */
        public frictionOverride: number

        /** Optional value for overriding an indiviaul object's mass */
        public massOverride: number

        /**
         * Creates a new PartDefinition instance using the specified properties.
         * @param [properties] Properties to set
         * @returns PartDefinition instance
         */
        public static create(properties?: mirabuf.IPartDefinition): mirabuf.PartDefinition

        /**
         * Encodes the specified PartDefinition message. Does not implicitly {@link mirabuf.PartDefinition.verify|verify} messages.
         * @param message PartDefinition message or plain object to encode
         * @param [writer] Writer to encode to
         * @returns Writer
         */
        public static encode(message: mirabuf.IPartDefinition, writer?: $protobuf.Writer): $protobuf.Writer

        /**
         * Encodes the specified PartDefinition message, length delimited. Does not implicitly {@link mirabuf.PartDefinition.verify|verify} messages.
         * @param message PartDefinition message or plain object to encode
         * @param [writer] Writer to encode to
         * @returns Writer
         */
        public static encodeDelimited(message: mirabuf.IPartDefinition, writer?: $protobuf.Writer): $protobuf.Writer

        /**
         * Decodes a PartDefinition message from the specified reader or buffer.
         * @param reader Reader or buffer to decode from
         * @param [length] Message length if known beforehand
         * @returns PartDefinition
         * @throws {Error} If the payload is not a reader or valid buffer
         * @throws {$protobuf.util.ProtocolError} If required fields are missing
         */
        public static decode(reader: $protobuf.Reader | Uint8Array, length?: number): mirabuf.PartDefinition

        /**
         * Decodes a PartDefinition message from the specified reader or buffer, length delimited.
         * @param reader Reader or buffer to decode from
         * @returns PartDefinition
         * @throws {Error} If the payload is not a reader or valid buffer
         * @throws {$protobuf.util.ProtocolError} If required fields are missing
         */
        public static decodeDelimited(reader: $protobuf.Reader | Uint8Array): mirabuf.PartDefinition

        /**
         * Verifies a PartDefinition message.
         * @param message Plain object to verify
         * @returns `null` if valid, otherwise the reason why it is not
         */
        public static verify(message: { [k: string]: unknown }): string | null

        /**
         * Creates a PartDefinition message from a plain object. Also converts values to their respective internal types.
         * @param object Plain object
         * @returns PartDefinition
         */
        public static fromObject(object: { [k: string]: unknown }): mirabuf.PartDefinition

        /**
         * Creates a plain object from a PartDefinition message. Also converts values to other types if specified.
         * @param message PartDefinition
         * @param [options] Conversion options
         * @returns Plain object
         */
        public static toObject(
            message: mirabuf.PartDefinition,
            options?: $protobuf.IConversionOptions
        ): { [k: string]: unknown }

        /**
         * Converts this PartDefinition to JSON.
         * @returns JSON object
         */
        public toJSON(): { [k: string]: unknown }

        /**
         * Gets the default type url for PartDefinition
         * @param [typeUrlPrefix] your custom typeUrlPrefix(default "type.googleapis.com")
         * @returns The default type url
         */
        public static getTypeUrl(typeUrlPrefix?: string): string
    }

    /** Properties of a PartInstance. */
    interface IPartInstance {
        /** PartInstance info */
        info?: mirabuf.IInfo | null

        /** Reference to the Part Definition defined in Assembly Data */
        partDefinitionReference?: string | null

        /** Overriding the object transform (moves the part from the def) - in design hierarchy context */
        transform?: mirabuf.ITransform | null

        /** Position transform from a global scope */
        globalTransform?: mirabuf.ITransform | null

        /** Joints that interact with this element */
        joints?: string[] | null

        /** PartInstance appearance */
        appearance?: string | null

        /** Physical Material Reference to link to `Materials->PhysicalMaterial->Info->id` */
        physicalMaterial?: string | null

        /** Flag that if enabled indicates we should skip generating a collider, defaults to FALSE or undefined */
        skipCollider?: boolean | null
    }

    /** Represents a PartInstance. */
    class PartInstance implements IPartInstance {
        /**
         * Constructs a new PartInstance.
         * @param [properties] Properties to set
         */
        constructor(properties?: mirabuf.IPartInstance)

        /** PartInstance info. */
        public info?: mirabuf.IInfo | null

        /** Reference to the Part Definition defined in Assembly Data */
        public partDefinitionReference: string

        /** Overriding the object transform (moves the part from the def) - in design hierarchy context */
        public transform?: mirabuf.ITransform | null

        /** Position transform from a global scope */
        public globalTransform?: mirabuf.ITransform | null

        /** Joints that interact with this element */
        public joints: string[]

        /** PartInstance appearance. */
        public appearance: string

        /** Physical Material Reference to link to `Materials->PhysicalMaterial->Info->id` */
        public physicalMaterial: string

        /** Flag that if enabled indicates we should skip generating a collider, defaults to FALSE or undefined */
        public skipCollider: boolean

        /**
         * Creates a new PartInstance instance using the specified properties.
         * @param [properties] Properties to set
         * @returns PartInstance instance
         */
        public static create(properties?: mirabuf.IPartInstance): mirabuf.PartInstance

        /**
         * Encodes the specified PartInstance message. Does not implicitly {@link mirabuf.PartInstance.verify|verify} messages.
         * @param message PartInstance message or plain object to encode
         * @param [writer] Writer to encode to
         * @returns Writer
         */
        public static encode(message: mirabuf.IPartInstance, writer?: $protobuf.Writer): $protobuf.Writer

        /**
         * Encodes the specified PartInstance message, length delimited. Does not implicitly {@link mirabuf.PartInstance.verify|verify} messages.
         * @param message PartInstance message or plain object to encode
         * @param [writer] Writer to encode to
         * @returns Writer
         */
        public static encodeDelimited(message: mirabuf.IPartInstance, writer?: $protobuf.Writer): $protobuf.Writer

        /**
         * Decodes a PartInstance message from the specified reader or buffer.
         * @param reader Reader or buffer to decode from
         * @param [length] Message length if known beforehand
         * @returns PartInstance
         * @throws {Error} If the payload is not a reader or valid buffer
         * @throws {$protobuf.util.ProtocolError} If required fields are missing
         */
        public static decode(reader: $protobuf.Reader | Uint8Array, length?: number): mirabuf.PartInstance

        /**
         * Decodes a PartInstance message from the specified reader or buffer, length delimited.
         * @param reader Reader or buffer to decode from
         * @returns PartInstance
         * @throws {Error} If the payload is not a reader or valid buffer
         * @throws {$protobuf.util.ProtocolError} If required fields are missing
         */
        public static decodeDelimited(reader: $protobuf.Reader | Uint8Array): mirabuf.PartInstance

        /**
         * Verifies a PartInstance message.
         * @param message Plain object to verify
         * @returns `null` if valid, otherwise the reason why it is not
         */
        public static verify(message: { [k: string]: unknown }): string | null

        /**
         * Creates a PartInstance message from a plain object. Also converts values to their respective internal types.
         * @param object Plain object
         * @returns PartInstance
         */
        public static fromObject(object: { [k: string]: unknown }): mirabuf.PartInstance

        /**
         * Creates a plain object from a PartInstance message. Also converts values to other types if specified.
         * @param message PartInstance
         * @param [options] Conversion options
         * @returns Plain object
         */
        public static toObject(
            message: mirabuf.PartInstance,
            options?: $protobuf.IConversionOptions
        ): { [k: string]: unknown }

        /**
         * Converts this PartInstance to JSON.
         * @returns JSON object
         */
        public toJSON(): { [k: string]: unknown }

        /**
         * Gets the default type url for PartInstance
         * @param [typeUrlPrefix] your custom typeUrlPrefix(default "type.googleapis.com")
         * @returns The default type url
         */
        public static getTypeUrl(typeUrlPrefix?: string): string
    }

    /** Properties of a Body. */
    interface IBody {
        /** Body info */
        info?: mirabuf.IInfo | null

        /** Reference to Part Definition */
        part?: string | null

        /** Triangle Mesh for rendering */
        triangleMesh?: mirabuf.ITriangleMesh | null

        /** Override Visual Appearance for the body */
        appearanceOverride?: string | null
    }

    /** Represents a Body. */
    class Body implements IBody {
        /**
         * Constructs a new Body.
         * @param [properties] Properties to set
         */
        constructor(properties?: mirabuf.IBody)

        /** Body info. */
        public info?: mirabuf.IInfo | null

        /** Reference to Part Definition */
        public part: string

        /** Triangle Mesh for rendering */
        public triangleMesh?: mirabuf.ITriangleMesh | null

        /** Override Visual Appearance for the body */
        public appearanceOverride: string

        /**
         * Creates a new Body instance using the specified properties.
         * @param [properties] Properties to set
         * @returns Body instance
         */
        public static create(properties?: mirabuf.IBody): mirabuf.Body

        /**
         * Encodes the specified Body message. Does not implicitly {@link mirabuf.Body.verify|verify} messages.
         * @param message Body message or plain object to encode
         * @param [writer] Writer to encode to
         * @returns Writer
         */
        public static encode(message: mirabuf.IBody, writer?: $protobuf.Writer): $protobuf.Writer

        /**
         * Encodes the specified Body message, length delimited. Does not implicitly {@link mirabuf.Body.verify|verify} messages.
         * @param message Body message or plain object to encode
         * @param [writer] Writer to encode to
         * @returns Writer
         */
        public static encodeDelimited(message: mirabuf.IBody, writer?: $protobuf.Writer): $protobuf.Writer

        /**
         * Decodes a Body message from the specified reader or buffer.
         * @param reader Reader or buffer to decode from
         * @param [length] Message length if known beforehand
         * @returns Body
         * @throws {Error} If the payload is not a reader or valid buffer
         * @throws {$protobuf.util.ProtocolError} If required fields are missing
         */
        public static decode(reader: $protobuf.Reader | Uint8Array, length?: number): mirabuf.Body

        /**
         * Decodes a Body message from the specified reader or buffer, length delimited.
         * @param reader Reader or buffer to decode from
         * @returns Body
         * @throws {Error} If the payload is not a reader or valid buffer
         * @throws {$protobuf.util.ProtocolError} If required fields are missing
         */
        public static decodeDelimited(reader: $protobuf.Reader | Uint8Array): mirabuf.Body

        /**
         * Verifies a Body message.
         * @param message Plain object to verify
         * @returns `null` if valid, otherwise the reason why it is not
         */
        public static verify(message: { [k: string]: unknown }): string | null

        /**
         * Creates a Body message from a plain object. Also converts values to their respective internal types.
         * @param object Plain object
         * @returns Body
         */
        public static fromObject(object: { [k: string]: unknown }): mirabuf.Body

        /**
         * Creates a plain object from a Body message. Also converts values to other types if specified.
         * @param message Body
         * @param [options] Conversion options
         * @returns Plain object
         */
        public static toObject(message: mirabuf.Body, options?: $protobuf.IConversionOptions): { [k: string]: unknown }

        /**
         * Converts this Body to JSON.
         * @returns JSON object
         */
        public toJSON(): { [k: string]: unknown }

        /**
         * Gets the default type url for Body
         * @param [typeUrlPrefix] your custom typeUrlPrefix(default "type.googleapis.com")
         * @returns The default type url
         */
        public static getTypeUrl(typeUrlPrefix?: string): string
    }

    /** Properties of a TriangleMesh. */
    interface ITriangleMesh {
        /** TriangleMesh info */
        info?: mirabuf.IInfo | null

        /** Is this object a Plane ? (Does it have volume) */
        hasVolume?: boolean | null

        /** Rendered Appearance properties referenced from Assembly Data */
        materialReference?: string | null

        /** Stored as true types, inidicies, verts, uv */
        mesh?: mirabuf.IMesh | null

        /** Stored as binary data in bytes */
        bmesh?: mirabuf.IBinaryMesh | null
    }

    /** Traingle Mesh for Storing Display Mesh data */
    class TriangleMesh implements ITriangleMesh {
        /**
         * Constructs a new TriangleMesh.
         * @param [properties] Properties to set
         */
        constructor(properties?: mirabuf.ITriangleMesh)

        /** TriangleMesh info. */
        public info?: mirabuf.IInfo | null

        /** Is this object a Plane ? (Does it have volume) */
        public hasVolume: boolean

        /** Rendered Appearance properties referenced from Assembly Data */
        public materialReference: string

        /** Stored as true types, inidicies, verts, uv */
        public mesh?: mirabuf.IMesh | null

        /** Stored as binary data in bytes */
        public bmesh?: mirabuf.IBinaryMesh | null

        /** What kind of Mesh Data exists in this Triangle Mesh */
        public meshType?: "mesh" | "bmesh"

        /**
         * Creates a new TriangleMesh instance using the specified properties.
         * @param [properties] Properties to set
         * @returns TriangleMesh instance
         */
        public static create(properties?: mirabuf.ITriangleMesh): mirabuf.TriangleMesh

        /**
         * Encodes the specified TriangleMesh message. Does not implicitly {@link mirabuf.TriangleMesh.verify|verify} messages.
         * @param message TriangleMesh message or plain object to encode
         * @param [writer] Writer to encode to
         * @returns Writer
         */
        public static encode(message: mirabuf.ITriangleMesh, writer?: $protobuf.Writer): $protobuf.Writer

        /**
         * Encodes the specified TriangleMesh message, length delimited. Does not implicitly {@link mirabuf.TriangleMesh.verify|verify} messages.
         * @param message TriangleMesh message or plain object to encode
         * @param [writer] Writer to encode to
         * @returns Writer
         */
        public static encodeDelimited(message: mirabuf.ITriangleMesh, writer?: $protobuf.Writer): $protobuf.Writer

        /**
         * Decodes a TriangleMesh message from the specified reader or buffer.
         * @param reader Reader or buffer to decode from
         * @param [length] Message length if known beforehand
         * @returns TriangleMesh
         * @throws {Error} If the payload is not a reader or valid buffer
         * @throws {$protobuf.util.ProtocolError} If required fields are missing
         */
        public static decode(reader: $protobuf.Reader | Uint8Array, length?: number): mirabuf.TriangleMesh

        /**
         * Decodes a TriangleMesh message from the specified reader or buffer, length delimited.
         * @param reader Reader or buffer to decode from
         * @returns TriangleMesh
         * @throws {Error} If the payload is not a reader or valid buffer
         * @throws {$protobuf.util.ProtocolError} If required fields are missing
         */
        public static decodeDelimited(reader: $protobuf.Reader | Uint8Array): mirabuf.TriangleMesh

        /**
         * Verifies a TriangleMesh message.
         * @param message Plain object to verify
         * @returns `null` if valid, otherwise the reason why it is not
         */
        public static verify(message: { [k: string]: unknown }): string | null

        /**
         * Creates a TriangleMesh message from a plain object. Also converts values to their respective internal types.
         * @param object Plain object
         * @returns TriangleMesh
         */
        public static fromObject(object: { [k: string]: unknown }): mirabuf.TriangleMesh

        /**
         * Creates a plain object from a TriangleMesh message. Also converts values to other types if specified.
         * @param message TriangleMesh
         * @param [options] Conversion options
         * @returns Plain object
         */
        public static toObject(
            message: mirabuf.TriangleMesh,
            options?: $protobuf.IConversionOptions
        ): { [k: string]: unknown }

        /**
         * Converts this TriangleMesh to JSON.
         * @returns JSON object
         */
        public toJSON(): { [k: string]: unknown }

        /**
         * Gets the default type url for TriangleMesh
         * @param [typeUrlPrefix] your custom typeUrlPrefix(default "type.googleapis.com")
         * @returns The default type url
         */
        public static getTypeUrl(typeUrlPrefix?: string): string
    }

    /** Properties of a Mesh. */
    interface IMesh {
        /** Tri Mesh Verts vec3 */
        verts?: number[] | null

        /** Tri Mesh Normals vec3 */
        normals?: number[] | null

        /** Tri Mesh uv Mapping vec2 */
        uv?: number[] | null

        /** Tri Mesh indicies (Vert Map) */
        indices?: number[] | null
    }

    /** Mesh Data stored as generic Data Structure */
    class Mesh implements IMesh {
        /**
         * Constructs a new Mesh.
         * @param [properties] Properties to set
         */
        constructor(properties?: mirabuf.IMesh)

        /** Tri Mesh Verts vec3 */
        public verts: number[]

        /** Tri Mesh Normals vec3 */
        public normals: number[]

        /** Tri Mesh uv Mapping vec2 */
        public uv: number[]

        /** Tri Mesh indicies (Vert Map) */
        public indices: number[]

        /**
         * Creates a new Mesh instance using the specified properties.
         * @param [properties] Properties to set
         * @returns Mesh instance
         */
        public static create(properties?: mirabuf.IMesh): mirabuf.Mesh

        /**
         * Encodes the specified Mesh message. Does not implicitly {@link mirabuf.Mesh.verify|verify} messages.
         * @param message Mesh message or plain object to encode
         * @param [writer] Writer to encode to
         * @returns Writer
         */
        public static encode(message: mirabuf.IMesh, writer?: $protobuf.Writer): $protobuf.Writer

        /**
         * Encodes the specified Mesh message, length delimited. Does not implicitly {@link mirabuf.Mesh.verify|verify} messages.
         * @param message Mesh message or plain object to encode
         * @param [writer] Writer to encode to
         * @returns Writer
         */
        public static encodeDelimited(message: mirabuf.IMesh, writer?: $protobuf.Writer): $protobuf.Writer

        /**
         * Decodes a Mesh message from the specified reader or buffer.
         * @param reader Reader or buffer to decode from
         * @param [length] Message length if known beforehand
         * @returns Mesh
         * @throws {Error} If the payload is not a reader or valid buffer
         * @throws {$protobuf.util.ProtocolError} If required fields are missing
         */
        public static decode(reader: $protobuf.Reader | Uint8Array, length?: number): mirabuf.Mesh

        /**
         * Decodes a Mesh message from the specified reader or buffer, length delimited.
         * @param reader Reader or buffer to decode from
         * @returns Mesh
         * @throws {Error} If the payload is not a reader or valid buffer
         * @throws {$protobuf.util.ProtocolError} If required fields are missing
         */
        public static decodeDelimited(reader: $protobuf.Reader | Uint8Array): mirabuf.Mesh

        /**
         * Verifies a Mesh message.
         * @param message Plain object to verify
         * @returns `null` if valid, otherwise the reason why it is not
         */
        public static verify(message: { [k: string]: unknown }): string | null

        /**
         * Creates a Mesh message from a plain object. Also converts values to their respective internal types.
         * @param object Plain object
         * @returns Mesh
         */
        public static fromObject(object: { [k: string]: unknown }): mirabuf.Mesh

        /**
         * Creates a plain object from a Mesh message. Also converts values to other types if specified.
         * @param message Mesh
         * @param [options] Conversion options
         * @returns Plain object
         */
        public static toObject(message: mirabuf.Mesh, options?: $protobuf.IConversionOptions): { [k: string]: unknown }

        /**
         * Converts this Mesh to JSON.
         * @returns JSON object
         */
        public toJSON(): { [k: string]: unknown }

        /**
         * Gets the default type url for Mesh
         * @param [typeUrlPrefix] your custom typeUrlPrefix(default "type.googleapis.com")
         * @returns The default type url
         */
        public static getTypeUrl(typeUrlPrefix?: string): string
    }

    /** Properties of a BinaryMesh. */
    interface IBinaryMesh {
        /** BEWARE of ENDIANESS */
        data?: Uint8Array | null
    }

    /** Mesh used for more effective file transfers */
    class BinaryMesh implements IBinaryMesh {
        /**
         * Constructs a new BinaryMesh.
         * @param [properties] Properties to set
         */
        constructor(properties?: mirabuf.IBinaryMesh)

        /** BEWARE of ENDIANESS */
        public data: Uint8Array

        /**
         * Creates a new BinaryMesh instance using the specified properties.
         * @param [properties] Properties to set
         * @returns BinaryMesh instance
         */
        public static create(properties?: mirabuf.IBinaryMesh): mirabuf.BinaryMesh

        /**
         * Encodes the specified BinaryMesh message. Does not implicitly {@link mirabuf.BinaryMesh.verify|verify} messages.
         * @param message BinaryMesh message or plain object to encode
         * @param [writer] Writer to encode to
         * @returns Writer
         */
        public static encode(message: mirabuf.IBinaryMesh, writer?: $protobuf.Writer): $protobuf.Writer

        /**
         * Encodes the specified BinaryMesh message, length delimited. Does not implicitly {@link mirabuf.BinaryMesh.verify|verify} messages.
         * @param message BinaryMesh message or plain object to encode
         * @param [writer] Writer to encode to
         * @returns Writer
         */
        public static encodeDelimited(message: mirabuf.IBinaryMesh, writer?: $protobuf.Writer): $protobuf.Writer

        /**
         * Decodes a BinaryMesh message from the specified reader or buffer.
         * @param reader Reader or buffer to decode from
         * @param [length] Message length if known beforehand
         * @returns BinaryMesh
         * @throws {Error} If the payload is not a reader or valid buffer
         * @throws {$protobuf.util.ProtocolError} If required fields are missing
         */
        public static decode(reader: $protobuf.Reader | Uint8Array, length?: number): mirabuf.BinaryMesh

        /**
         * Decodes a BinaryMesh message from the specified reader or buffer, length delimited.
         * @param reader Reader or buffer to decode from
         * @returns BinaryMesh
         * @throws {Error} If the payload is not a reader or valid buffer
         * @throws {$protobuf.util.ProtocolError} If required fields are missing
         */
        public static decodeDelimited(reader: $protobuf.Reader | Uint8Array): mirabuf.BinaryMesh

        /**
         * Verifies a BinaryMesh message.
         * @param message Plain object to verify
         * @returns `null` if valid, otherwise the reason why it is not
         */
        public static verify(message: { [k: string]: unknown }): string | null

        /**
         * Creates a BinaryMesh message from a plain object. Also converts values to their respective internal types.
         * @param object Plain object
         * @returns BinaryMesh
         */
        public static fromObject(object: { [k: string]: unknown }): mirabuf.BinaryMesh

        /**
         * Creates a plain object from a BinaryMesh message. Also converts values to other types if specified.
         * @param message BinaryMesh
         * @param [options] Conversion options
         * @returns Plain object
         */
        public static toObject(
            message: mirabuf.BinaryMesh,
            options?: $protobuf.IConversionOptions
        ): { [k: string]: unknown }

        /**
         * Converts this BinaryMesh to JSON.
         * @returns JSON object
         */
        public toJSON(): { [k: string]: unknown }

        /**
         * Gets the default type url for BinaryMesh
         * @param [typeUrlPrefix] your custom typeUrlPrefix(default "type.googleapis.com")
         * @returns The default type url
         */
        public static getTypeUrl(typeUrlPrefix?: string): string
    }

    /** Properties of a Node. */
    interface INode {
        /** the reference ID for whatever kind of graph this is */
        value?: string | null

        /** the children for the given leaf */
        children?: mirabuf.INode[] | null

        /** other associated data that can be used */
        userData?: mirabuf.IUserData | null
    }

    /** Represents a Node. */
    class Node implements INode {
        /**
         * Constructs a new Node.
         * @param [properties] Properties to set
         */
        constructor(properties?: mirabuf.INode)

        /** the reference ID for whatever kind of graph this is */
        public value: string

        /** the children for the given leaf */
        public children: mirabuf.INode[]

        /** other associated data that can be used */
        public userData?: mirabuf.IUserData | null

        /**
         * Creates a new Node instance using the specified properties.
         * @param [properties] Properties to set
         * @returns Node instance
         */
        public static create(properties?: mirabuf.INode): mirabuf.Node

        /**
         * Encodes the specified Node message. Does not implicitly {@link mirabuf.Node.verify|verify} messages.
         * @param message Node message or plain object to encode
         * @param [writer] Writer to encode to
         * @returns Writer
         */
        public static encode(message: mirabuf.INode, writer?: $protobuf.Writer): $protobuf.Writer

        /**
         * Encodes the specified Node message, length delimited. Does not implicitly {@link mirabuf.Node.verify|verify} messages.
         * @param message Node message or plain object to encode
         * @param [writer] Writer to encode to
         * @returns Writer
         */
        public static encodeDelimited(message: mirabuf.INode, writer?: $protobuf.Writer): $protobuf.Writer

        /**
         * Decodes a Node message from the specified reader or buffer.
         * @param reader Reader or buffer to decode from
         * @param [length] Message length if known beforehand
         * @returns Node
         * @throws {Error} If the payload is not a reader or valid buffer
         * @throws {$protobuf.util.ProtocolError} If required fields are missing
         */
        public static decode(reader: $protobuf.Reader | Uint8Array, length?: number): mirabuf.Node

        /**
         * Decodes a Node message from the specified reader or buffer, length delimited.
         * @param reader Reader or buffer to decode from
         * @returns Node
         * @throws {Error} If the payload is not a reader or valid buffer
         * @throws {$protobuf.util.ProtocolError} If required fields are missing
         */
        public static decodeDelimited(reader: $protobuf.Reader | Uint8Array): mirabuf.Node

        /**
         * Verifies a Node message.
         * @param message Plain object to verify
         * @returns `null` if valid, otherwise the reason why it is not
         */
        public static verify(message: { [k: string]: unknown }): string | null

        /**
         * Creates a Node message from a plain object. Also converts values to their respective internal types.
         * @param object Plain object
         * @returns Node
         */
        public static fromObject(object: { [k: string]: unknown }): mirabuf.Node

        /**
         * Creates a plain object from a Node message. Also converts values to other types if specified.
         * @param message Node
         * @param [options] Conversion options
         * @returns Plain object
         */
        public static toObject(message: mirabuf.Node, options?: $protobuf.IConversionOptions): { [k: string]: unknown }

        /**
         * Converts this Node to JSON.
         * @returns JSON object
         */
        public toJSON(): { [k: string]: unknown }

        /**
         * Gets the default type url for Node
         * @param [typeUrlPrefix] your custom typeUrlPrefix(default "type.googleapis.com")
         * @returns The default type url
         */
        public static getTypeUrl(typeUrlPrefix?: string): string
    }

    /** Properties of a GraphContainer. */
    interface IGraphContainer {
        /** GraphContainer nodes */
        nodes?: mirabuf.INode[] | null
    }

    /** Represents a GraphContainer. */
    class GraphContainer implements IGraphContainer {
        /**
         * Constructs a new GraphContainer.
         * @param [properties] Properties to set
         */
        constructor(properties?: mirabuf.IGraphContainer)

        /** GraphContainer nodes. */
        public nodes: mirabuf.INode[]

        /**
         * Creates a new GraphContainer instance using the specified properties.
         * @param [properties] Properties to set
         * @returns GraphContainer instance
         */
        public static create(properties?: mirabuf.IGraphContainer): mirabuf.GraphContainer

        /**
         * Encodes the specified GraphContainer message. Does not implicitly {@link mirabuf.GraphContainer.verify|verify} messages.
         * @param message GraphContainer message or plain object to encode
         * @param [writer] Writer to encode to
         * @returns Writer
         */
        public static encode(message: mirabuf.IGraphContainer, writer?: $protobuf.Writer): $protobuf.Writer

        /**
         * Encodes the specified GraphContainer message, length delimited. Does not implicitly {@link mirabuf.GraphContainer.verify|verify} messages.
         * @param message GraphContainer message or plain object to encode
         * @param [writer] Writer to encode to
         * @returns Writer
         */
        public static encodeDelimited(message: mirabuf.IGraphContainer, writer?: $protobuf.Writer): $protobuf.Writer

        /**
         * Decodes a GraphContainer message from the specified reader or buffer.
         * @param reader Reader or buffer to decode from
         * @param [length] Message length if known beforehand
         * @returns GraphContainer
         * @throws {Error} If the payload is not a reader or valid buffer
         * @throws {$protobuf.util.ProtocolError} If required fields are missing
         */
        public static decode(reader: $protobuf.Reader | Uint8Array, length?: number): mirabuf.GraphContainer

        /**
         * Decodes a GraphContainer message from the specified reader or buffer, length delimited.
         * @param reader Reader or buffer to decode from
         * @returns GraphContainer
         * @throws {Error} If the payload is not a reader or valid buffer
         * @throws {$protobuf.util.ProtocolError} If required fields are missing
         */
        public static decodeDelimited(reader: $protobuf.Reader | Uint8Array): mirabuf.GraphContainer

        /**
         * Verifies a GraphContainer message.
         * @param message Plain object to verify
         * @returns `null` if valid, otherwise the reason why it is not
         */
        public static verify(message: { [k: string]: unknown }): string | null

        /**
         * Creates a GraphContainer message from a plain object. Also converts values to their respective internal types.
         * @param object Plain object
         * @returns GraphContainer
         */
        public static fromObject(object: { [k: string]: unknown }): mirabuf.GraphContainer

        /**
         * Creates a plain object from a GraphContainer message. Also converts values to other types if specified.
         * @param message GraphContainer
         * @param [options] Conversion options
         * @returns Plain object
         */
        public static toObject(
            message: mirabuf.GraphContainer,
            options?: $protobuf.IConversionOptions
        ): { [k: string]: unknown }

        /**
         * Converts this GraphContainer to JSON.
         * @returns JSON object
         */
        public toJSON(): { [k: string]: unknown }

        /**
         * Gets the default type url for GraphContainer
         * @param [typeUrlPrefix] your custom typeUrlPrefix(default "type.googleapis.com")
         * @returns The default type url
         */
        public static getTypeUrl(typeUrlPrefix?: string): string
    }

    /** Properties of a UserData. */
    interface IUserData {
        /** e.g. data["wheel"] = "yes" */
        data?: { [k: string]: string } | null
    }

    /**
     * UserData
     *
     * Arbitrary data to append to a given message in map form
     */
    class UserData implements IUserData {
        /**
         * Constructs a new UserData.
         * @param [properties] Properties to set
         */
        constructor(properties?: mirabuf.IUserData)

        /** e.g. data["wheel"] = "yes" */
        public data: { [k: string]: string }

        /**
         * Creates a new UserData instance using the specified properties.
         * @param [properties] Properties to set
         * @returns UserData instance
         */
        public static create(properties?: mirabuf.IUserData): mirabuf.UserData

        /**
         * Encodes the specified UserData message. Does not implicitly {@link mirabuf.UserData.verify|verify} messages.
         * @param message UserData message or plain object to encode
         * @param [writer] Writer to encode to
         * @returns Writer
         */
        public static encode(message: mirabuf.IUserData, writer?: $protobuf.Writer): $protobuf.Writer

        /**
         * Encodes the specified UserData message, length delimited. Does not implicitly {@link mirabuf.UserData.verify|verify} messages.
         * @param message UserData message or plain object to encode
         * @param [writer] Writer to encode to
         * @returns Writer
         */
        public static encodeDelimited(message: mirabuf.IUserData, writer?: $protobuf.Writer): $protobuf.Writer

        /**
         * Decodes a UserData message from the specified reader or buffer.
         * @param reader Reader or buffer to decode from
         * @param [length] Message length if known beforehand
         * @returns UserData
         * @throws {Error} If the payload is not a reader or valid buffer
         * @throws {$protobuf.util.ProtocolError} If required fields are missing
         */
        public static decode(reader: $protobuf.Reader | Uint8Array, length?: number): mirabuf.UserData

        /**
         * Decodes a UserData message from the specified reader or buffer, length delimited.
         * @param reader Reader or buffer to decode from
         * @returns UserData
         * @throws {Error} If the payload is not a reader or valid buffer
         * @throws {$protobuf.util.ProtocolError} If required fields are missing
         */
        public static decodeDelimited(reader: $protobuf.Reader | Uint8Array): mirabuf.UserData

        /**
         * Verifies a UserData message.
         * @param message Plain object to verify
         * @returns `null` if valid, otherwise the reason why it is not
         */
        public static verify(message: { [k: string]: unknown }): string | null

        /**
         * Creates a UserData message from a plain object. Also converts values to their respective internal types.
         * @param object Plain object
         * @returns UserData
         */
        public static fromObject(object: { [k: string]: unknown }): mirabuf.UserData

        /**
         * Creates a plain object from a UserData message. Also converts values to other types if specified.
         * @param message UserData
         * @param [options] Conversion options
         * @returns Plain object
         */
        public static toObject(
            message: mirabuf.UserData,
            options?: $protobuf.IConversionOptions
        ): { [k: string]: unknown }

        /**
         * Converts this UserData to JSON.
         * @returns JSON object
         */
        public toJSON(): { [k: string]: unknown }

        /**
         * Gets the default type url for UserData
         * @param [typeUrlPrefix] your custom typeUrlPrefix(default "type.googleapis.com")
         * @returns The default type url
         */
        public static getTypeUrl(typeUrlPrefix?: string): string
    }

    /** Properties of a Vector3. */
    interface IVector3 {
        /** Vector3 x */
        x?: number | null

        /** Vector3 y */
        y?: number | null

        /** Vector3 z */
        z?: number | null
    }

    /** Represents a Vector3. */
    class Vector3 implements IVector3 {
        /**
         * Constructs a new Vector3.
         * @param [properties] Properties to set
         */
        constructor(properties?: mirabuf.IVector3)

        /** Vector3 x. */
        public x: number

        /** Vector3 y. */
        public y: number

        /** Vector3 z. */
        public z: number

        /**
         * Creates a new Vector3 instance using the specified properties.
         * @param [properties] Properties to set
         * @returns Vector3 instance
         */
        public static create(properties?: mirabuf.IVector3): mirabuf.Vector3

        /**
         * Encodes the specified Vector3 message. Does not implicitly {@link mirabuf.Vector3.verify|verify} messages.
         * @param message Vector3 message or plain object to encode
         * @param [writer] Writer to encode to
         * @returns Writer
         */
        public static encode(message: mirabuf.IVector3, writer?: $protobuf.Writer): $protobuf.Writer

        /**
         * Encodes the specified Vector3 message, length delimited. Does not implicitly {@link mirabuf.Vector3.verify|verify} messages.
         * @param message Vector3 message or plain object to encode
         * @param [writer] Writer to encode to
         * @returns Writer
         */
        public static encodeDelimited(message: mirabuf.IVector3, writer?: $protobuf.Writer): $protobuf.Writer

        /**
         * Decodes a Vector3 message from the specified reader or buffer.
         * @param reader Reader or buffer to decode from
         * @param [length] Message length if known beforehand
         * @returns Vector3
         * @throws {Error} If the payload is not a reader or valid buffer
         * @throws {$protobuf.util.ProtocolError} If required fields are missing
         */
        public static decode(reader: $protobuf.Reader | Uint8Array, length?: number): mirabuf.Vector3

        /**
         * Decodes a Vector3 message from the specified reader or buffer, length delimited.
         * @param reader Reader or buffer to decode from
         * @returns Vector3
         * @throws {Error} If the payload is not a reader or valid buffer
         * @throws {$protobuf.util.ProtocolError} If required fields are missing
         */
        public static decodeDelimited(reader: $protobuf.Reader | Uint8Array): mirabuf.Vector3

        /**
         * Verifies a Vector3 message.
         * @param message Plain object to verify
         * @returns `null` if valid, otherwise the reason why it is not
         */
        public static verify(message: { [k: string]: unknown }): string | null

        /**
         * Creates a Vector3 message from a plain object. Also converts values to their respective internal types.
         * @param object Plain object
         * @returns Vector3
         */
        public static fromObject(object: { [k: string]: unknown }): mirabuf.Vector3

        /**
         * Creates a plain object from a Vector3 message. Also converts values to other types if specified.
         * @param message Vector3
         * @param [options] Conversion options
         * @returns Plain object
         */
        public static toObject(
            message: mirabuf.Vector3,
            options?: $protobuf.IConversionOptions
        ): { [k: string]: unknown }

        /**
         * Converts this Vector3 to JSON.
         * @returns JSON object
         */
        public toJSON(): { [k: string]: unknown }

        /**
         * Gets the default type url for Vector3
         * @param [typeUrlPrefix] your custom typeUrlPrefix(default "type.googleapis.com")
         * @returns The default type url
         */
        public static getTypeUrl(typeUrlPrefix?: string): string
    }

    /** Properties of a PhysicalProperties. */
    interface IPhysicalProperties {
        /** kg per cubic cm kg/(cm^3) */
        density?: number | null

        /** kg */
        mass?: number | null

        /** cm^3 */
        volume?: number | null

        /** cm^2 */
        area?: number | null

        /** non-negative? Vec3 */
        com?: mirabuf.IVector3 | null
    }

    /** Represents a PhysicalProperties. */
    class PhysicalProperties implements IPhysicalProperties {
        /**
         * Constructs a new PhysicalProperties.
         * @param [properties] Properties to set
         */
        constructor(properties?: mirabuf.IPhysicalProperties)

        /** kg per cubic cm kg/(cm^3) */
        public density: number

        /** kg */
        public mass: number

        /** cm^3 */
        public volume: number

        /** cm^2 */
        public area: number

        /** non-negative? Vec3 */
        public com?: mirabuf.IVector3 | null

        /**
         * Creates a new PhysicalProperties instance using the specified properties.
         * @param [properties] Properties to set
         * @returns PhysicalProperties instance
         */
        public static create(properties?: mirabuf.IPhysicalProperties): mirabuf.PhysicalProperties

        /**
         * Encodes the specified PhysicalProperties message. Does not implicitly {@link mirabuf.PhysicalProperties.verify|verify} messages.
         * @param message PhysicalProperties message or plain object to encode
         * @param [writer] Writer to encode to
         * @returns Writer
         */
        public static encode(message: mirabuf.IPhysicalProperties, writer?: $protobuf.Writer): $protobuf.Writer

        /**
         * Encodes the specified PhysicalProperties message, length delimited. Does not implicitly {@link mirabuf.PhysicalProperties.verify|verify} messages.
         * @param message PhysicalProperties message or plain object to encode
         * @param [writer] Writer to encode to
         * @returns Writer
         */
        public static encodeDelimited(message: mirabuf.IPhysicalProperties, writer?: $protobuf.Writer): $protobuf.Writer

        /**
         * Decodes a PhysicalProperties message from the specified reader or buffer.
         * @param reader Reader or buffer to decode from
         * @param [length] Message length if known beforehand
         * @returns PhysicalProperties
         * @throws {Error} If the payload is not a reader or valid buffer
         * @throws {$protobuf.util.ProtocolError} If required fields are missing
         */
        public static decode(reader: $protobuf.Reader | Uint8Array, length?: number): mirabuf.PhysicalProperties

        /**
         * Decodes a PhysicalProperties message from the specified reader or buffer, length delimited.
         * @param reader Reader or buffer to decode from
         * @returns PhysicalProperties
         * @throws {Error} If the payload is not a reader or valid buffer
         * @throws {$protobuf.util.ProtocolError} If required fields are missing
         */
        public static decodeDelimited(reader: $protobuf.Reader | Uint8Array): mirabuf.PhysicalProperties

        /**
         * Verifies a PhysicalProperties message.
         * @param message Plain object to verify
         * @returns `null` if valid, otherwise the reason why it is not
         */
        public static verify(message: { [k: string]: unknown }): string | null

        /**
         * Creates a PhysicalProperties message from a plain object. Also converts values to their respective internal types.
         * @param object Plain object
         * @returns PhysicalProperties
         */
        public static fromObject(object: { [k: string]: unknown }): mirabuf.PhysicalProperties

        /**
         * Creates a plain object from a PhysicalProperties message. Also converts values to other types if specified.
         * @param message PhysicalProperties
         * @param [options] Conversion options
         * @returns Plain object
         */
        public static toObject(
            message: mirabuf.PhysicalProperties,
            options?: $protobuf.IConversionOptions
        ): { [k: string]: unknown }

        /**
         * Converts this PhysicalProperties to JSON.
         * @returns JSON object
         */
        public toJSON(): { [k: string]: unknown }

        /**
         * Gets the default type url for PhysicalProperties
         * @param [typeUrlPrefix] your custom typeUrlPrefix(default "type.googleapis.com")
         * @returns The default type url
         */
        public static getTypeUrl(typeUrlPrefix?: string): string
    }

    /** Properties of a Transform. */
    interface ITransform {
        /** Transform spatialMatrix */
        spatialMatrix?: number[] | null
    }

    /**
     * Transform
     *
     * Data needed to apply scale, position, and rotational changes
     */
    class Transform implements ITransform {
        /**
         * Constructs a new Transform.
         * @param [properties] Properties to set
         */
        constructor(properties?: mirabuf.ITransform)

        /** Transform spatialMatrix. */
        public spatialMatrix: number[]

        /**
         * Creates a new Transform instance using the specified properties.
         * @param [properties] Properties to set
         * @returns Transform instance
         */
        public static create(properties?: mirabuf.ITransform): mirabuf.Transform

        /**
         * Encodes the specified Transform message. Does not implicitly {@link mirabuf.Transform.verify|verify} messages.
         * @param message Transform message or plain object to encode
         * @param [writer] Writer to encode to
         * @returns Writer
         */
        public static encode(message: mirabuf.ITransform, writer?: $protobuf.Writer): $protobuf.Writer

        /**
         * Encodes the specified Transform message, length delimited. Does not implicitly {@link mirabuf.Transform.verify|verify} messages.
         * @param message Transform message or plain object to encode
         * @param [writer] Writer to encode to
         * @returns Writer
         */
        public static encodeDelimited(message: mirabuf.ITransform, writer?: $protobuf.Writer): $protobuf.Writer

        /**
         * Decodes a Transform message from the specified reader or buffer.
         * @param reader Reader or buffer to decode from
         * @param [length] Message length if known beforehand
         * @returns Transform
         * @throws {Error} If the payload is not a reader or valid buffer
         * @throws {$protobuf.util.ProtocolError} If required fields are missing
         */
        public static decode(reader: $protobuf.Reader | Uint8Array, length?: number): mirabuf.Transform

        /**
         * Decodes a Transform message from the specified reader or buffer, length delimited.
         * @param reader Reader or buffer to decode from
         * @returns Transform
         * @throws {Error} If the payload is not a reader or valid buffer
         * @throws {$protobuf.util.ProtocolError} If required fields are missing
         */
        public static decodeDelimited(reader: $protobuf.Reader | Uint8Array): mirabuf.Transform

        /**
         * Verifies a Transform message.
         * @param message Plain object to verify
         * @returns `null` if valid, otherwise the reason why it is not
         */
        public static verify(message: { [k: string]: unknown }): string | null

        /**
         * Creates a Transform message from a plain object. Also converts values to their respective internal types.
         * @param object Plain object
         * @returns Transform
         */
        public static fromObject(object: { [k: string]: unknown }): mirabuf.Transform

        /**
         * Creates a plain object from a Transform message. Also converts values to other types if specified.
         * @param message Transform
         * @param [options] Conversion options
         * @returns Plain object
         */
        public static toObject(
            message: mirabuf.Transform,
            options?: $protobuf.IConversionOptions
        ): { [k: string]: unknown }

        /**
         * Converts this Transform to JSON.
         * @returns JSON object
         */
        public toJSON(): { [k: string]: unknown }

        /**
         * Gets the default type url for Transform
         * @param [typeUrlPrefix] your custom typeUrlPrefix(default "type.googleapis.com")
         * @returns The default type url
         */
        public static getTypeUrl(typeUrlPrefix?: string): string
    }

    /** Properties of a Color. */
    interface IColor {
        /** Color R */
        R?: number | null

        /** Color G */
        G?: number | null

        /** Color B */
        B?: number | null

        /** Color A */
        A?: number | null
    }

    /** Represents a Color. */
    class Color implements IColor {
        /**
         * Constructs a new Color.
         * @param [properties] Properties to set
         */
        constructor(properties?: mirabuf.IColor)

        /** Color R. */
        public R: number

        /** Color G. */
        public G: number

        /** Color B. */
        public B: number

        /** Color A. */
        public A: number

        /**
         * Creates a new Color instance using the specified properties.
         * @param [properties] Properties to set
         * @returns Color instance
         */
        public static create(properties?: mirabuf.IColor): mirabuf.Color

        /**
         * Encodes the specified Color message. Does not implicitly {@link mirabuf.Color.verify|verify} messages.
         * @param message Color message or plain object to encode
         * @param [writer] Writer to encode to
         * @returns Writer
         */
        public static encode(message: mirabuf.IColor, writer?: $protobuf.Writer): $protobuf.Writer

        /**
         * Encodes the specified Color message, length delimited. Does not implicitly {@link mirabuf.Color.verify|verify} messages.
         * @param message Color message or plain object to encode
         * @param [writer] Writer to encode to
         * @returns Writer
         */
        public static encodeDelimited(message: mirabuf.IColor, writer?: $protobuf.Writer): $protobuf.Writer

        /**
         * Decodes a Color message from the specified reader or buffer.
         * @param reader Reader or buffer to decode from
         * @param [length] Message length if known beforehand
         * @returns Color
         * @throws {Error} If the payload is not a reader or valid buffer
         * @throws {$protobuf.util.ProtocolError} If required fields are missing
         */
        public static decode(reader: $protobuf.Reader | Uint8Array, length?: number): mirabuf.Color

        /**
         * Decodes a Color message from the specified reader or buffer, length delimited.
         * @param reader Reader or buffer to decode from
         * @returns Color
         * @throws {Error} If the payload is not a reader or valid buffer
         * @throws {$protobuf.util.ProtocolError} If required fields are missing
         */
        public static decodeDelimited(reader: $protobuf.Reader | Uint8Array): mirabuf.Color

        /**
         * Verifies a Color message.
         * @param message Plain object to verify
         * @returns `null` if valid, otherwise the reason why it is not
         */
        public static verify(message: { [k: string]: unknown }): string | null

        /**
         * Creates a Color message from a plain object. Also converts values to their respective internal types.
         * @param object Plain object
         * @returns Color
         */
        public static fromObject(object: { [k: string]: unknown }): mirabuf.Color

        /**
         * Creates a plain object from a Color message. Also converts values to other types if specified.
         * @param message Color
         * @param [options] Conversion options
         * @returns Plain object
         */
        public static toObject(message: mirabuf.Color, options?: $protobuf.IConversionOptions): { [k: string]: unknown }

        /**
         * Converts this Color to JSON.
         * @returns JSON object
         */
        public toJSON(): { [k: string]: unknown }

        /**
         * Gets the default type url for Color
         * @param [typeUrlPrefix] your custom typeUrlPrefix(default "type.googleapis.com")
         * @returns The default type url
         */
        public static getTypeUrl(typeUrlPrefix?: string): string
    }

    /** Axis enum. */
    enum Axis {
        X = 0,
        Y = 1,
        Z = 2,
    }

    /** Properties of an Info. */
    interface IInfo {
        /** Info GUID */
        GUID?: string | null

        /** Info name */
        name?: string | null

        /** Info version */
        version?: number | null
    }

    /**
     * Defines basic fields for almost all objects
     * The location where you can access the GUID for a reference
     */
    class Info implements IInfo {
        /**
         * Constructs a new Info.
         * @param [properties] Properties to set
         */
        constructor(properties?: mirabuf.IInfo)

        /** Info GUID. */
        public GUID: string

        /** Info name. */
        public name: string

        /** Info version. */
        public version: number

        /**
         * Creates a new Info instance using the specified properties.
         * @param [properties] Properties to set
         * @returns Info instance
         */
        public static create(properties?: mirabuf.IInfo): mirabuf.Info

        /**
         * Encodes the specified Info message. Does not implicitly {@link mirabuf.Info.verify|verify} messages.
         * @param message Info message or plain object to encode
         * @param [writer] Writer to encode to
         * @returns Writer
         */
        public static encode(message: mirabuf.IInfo, writer?: $protobuf.Writer): $protobuf.Writer

        /**
         * Encodes the specified Info message, length delimited. Does not implicitly {@link mirabuf.Info.verify|verify} messages.
         * @param message Info message or plain object to encode
         * @param [writer] Writer to encode to
         * @returns Writer
         */
        public static encodeDelimited(message: mirabuf.IInfo, writer?: $protobuf.Writer): $protobuf.Writer

        /**
         * Decodes an Info message from the specified reader or buffer.
         * @param reader Reader or buffer to decode from
         * @param [length] Message length if known beforehand
         * @returns Info
         * @throws {Error} If the payload is not a reader or valid buffer
         * @throws {$protobuf.util.ProtocolError} If required fields are missing
         */
        public static decode(reader: $protobuf.Reader | Uint8Array, length?: number): mirabuf.Info

        /**
         * Decodes an Info message from the specified reader or buffer, length delimited.
         * @param reader Reader or buffer to decode from
         * @returns Info
         * @throws {Error} If the payload is not a reader or valid buffer
         * @throws {$protobuf.util.ProtocolError} If required fields are missing
         */
        public static decodeDelimited(reader: $protobuf.Reader | Uint8Array): mirabuf.Info

        /**
         * Verifies an Info message.
         * @param message Plain object to verify
         * @returns `null` if valid, otherwise the reason why it is not
         */
        public static verify(message: { [k: string]: unknown }): string | null

        /**
         * Creates an Info message from a plain object. Also converts values to their respective internal types.
         * @param object Plain object
         * @returns Info
         */
        public static fromObject(object: { [k: string]: unknown }): mirabuf.Info

        /**
         * Creates a plain object from an Info message. Also converts values to other types if specified.
         * @param message Info
         * @param [options] Conversion options
         * @returns Plain object
         */
        public static toObject(message: mirabuf.Info, options?: $protobuf.IConversionOptions): { [k: string]: unknown }

        /**
         * Converts this Info to JSON.
         * @returns JSON object
         */
        public toJSON(): { [k: string]: unknown }

        /**
         * Gets the default type url for Info
         * @param [typeUrlPrefix] your custom typeUrlPrefix(default "type.googleapis.com")
         * @returns The default type url
         */
        public static getTypeUrl(typeUrlPrefix?: string): string
    }

    /** Properties of a Thumbnail. */
    interface IThumbnail {
        /** Image Width */
        width?: number | null

        /** Image Height */
        height?: number | null

        /** Image Extension - ex. (.png, .bitmap, .jpeg) */
        extension?: string | null

        /** Transparency - true from fusion when correctly configured */
        transparent?: boolean | null

        /** Data as read from the file in bytes[] form */
        data?: Uint8Array | null
    }

    /**
     * A basic Thumbnail to be encoded in the file
     * Most of the Time Fusion can encode the file with transparency as PNG not bitmap
     */
    class Thumbnail implements IThumbnail {
        /**
         * Constructs a new Thumbnail.
         * @param [properties] Properties to set
         */
        constructor(properties?: mirabuf.IThumbnail)

        /** Image Width */
        public width: number

        /** Image Height */
        public height: number

        /** Image Extension - ex. (.png, .bitmap, .jpeg) */
        public extension: string

        /** Transparency - true from fusion when correctly configured */
        public transparent: boolean

        /** Data as read from the file in bytes[] form */
        public data: Uint8Array

        /**
         * Creates a new Thumbnail instance using the specified properties.
         * @param [properties] Properties to set
         * @returns Thumbnail instance
         */
        public static create(properties?: mirabuf.IThumbnail): mirabuf.Thumbnail

        /**
         * Encodes the specified Thumbnail message. Does not implicitly {@link mirabuf.Thumbnail.verify|verify} messages.
         * @param message Thumbnail message or plain object to encode
         * @param [writer] Writer to encode to
         * @returns Writer
         */
        public static encode(message: mirabuf.IThumbnail, writer?: $protobuf.Writer): $protobuf.Writer

        /**
         * Encodes the specified Thumbnail message, length delimited. Does not implicitly {@link mirabuf.Thumbnail.verify|verify} messages.
         * @param message Thumbnail message or plain object to encode
         * @param [writer] Writer to encode to
         * @returns Writer
         */
        public static encodeDelimited(message: mirabuf.IThumbnail, writer?: $protobuf.Writer): $protobuf.Writer

        /**
         * Decodes a Thumbnail message from the specified reader or buffer.
         * @param reader Reader or buffer to decode from
         * @param [length] Message length if known beforehand
         * @returns Thumbnail
         * @throws {Error} If the payload is not a reader or valid buffer
         * @throws {$protobuf.util.ProtocolError} If required fields are missing
         */
        public static decode(reader: $protobuf.Reader | Uint8Array, length?: number): mirabuf.Thumbnail

        /**
         * Decodes a Thumbnail message from the specified reader or buffer, length delimited.
         * @param reader Reader or buffer to decode from
         * @returns Thumbnail
         * @throws {Error} If the payload is not a reader or valid buffer
         * @throws {$protobuf.util.ProtocolError} If required fields are missing
         */
        public static decodeDelimited(reader: $protobuf.Reader | Uint8Array): mirabuf.Thumbnail

        /**
         * Verifies a Thumbnail message.
         * @param message Plain object to verify
         * @returns `null` if valid, otherwise the reason why it is not
         */
        public static verify(message: { [k: string]: unknown }): string | null

        /**
         * Creates a Thumbnail message from a plain object. Also converts values to their respective internal types.
         * @param object Plain object
         * @returns Thumbnail
         */
        public static fromObject(object: { [k: string]: unknown }): mirabuf.Thumbnail

        /**
         * Creates a plain object from a Thumbnail message. Also converts values to other types if specified.
         * @param message Thumbnail
         * @param [options] Conversion options
         * @returns Plain object
         */
        public static toObject(
            message: mirabuf.Thumbnail,
            options?: $protobuf.IConversionOptions
        ): { [k: string]: unknown }

        /**
         * Converts this Thumbnail to JSON.
         * @returns JSON object
         */
        public toJSON(): { [k: string]: unknown }

        /**
         * Gets the default type url for Thumbnail
         * @param [typeUrlPrefix] your custom typeUrlPrefix(default "type.googleapis.com")
         * @returns The default type url
         */
        public static getTypeUrl(typeUrlPrefix?: string): string
    }

    /** Namespace joint. */
    namespace joint {
        /** Properties of a Joints. */
        interface IJoints {
            /** name, version, uid */
            info?: mirabuf.IInfo | null

            /** Unique Joint Implementations */
            jointDefinitions?: { [k: string]: mirabuf.joint.IJoint } | null

            /** Instances of the Joint Implementations */
            jointInstances?: {
                [k: string]: mirabuf.joint.IJointInstance
            } | null

            /** Rigidgroups ? */
            rigidGroups?: mirabuf.joint.IRigidGroup[] | null

            /** Collection of all Motors exported */
            motorDefinitions?: { [k: string]: mirabuf.motor.IMotor } | null
        }

        /**
         * Joints
         * A way to define the motion between various group connections
         */
        class Joints implements IJoints {
            /**
             * Constructs a new Joints.
             * @param [properties] Properties to set
             */
            constructor(properties?: mirabuf.joint.IJoints)

            /** name, version, uid */
            public info?: mirabuf.IInfo | null

            /** Unique Joint Implementations */
            public jointDefinitions: { [k: string]: mirabuf.joint.IJoint }

            /** Instances of the Joint Implementations */
            public jointInstances: { [k: string]: mirabuf.joint.IJointInstance }

            /** Rigidgroups ? */
            public rigidGroups: mirabuf.joint.IRigidGroup[]

            /** Collection of all Motors exported */
            public motorDefinitions: { [k: string]: mirabuf.motor.IMotor }

            /**
             * Creates a new Joints instance using the specified properties.
             * @param [properties] Properties to set
             * @returns Joints instance
             */
            public static create(properties?: mirabuf.joint.IJoints): mirabuf.joint.Joints

            /**
             * Encodes the specified Joints message. Does not implicitly {@link mirabuf.joint.Joints.verify|verify} messages.
             * @param message Joints message or plain object to encode
             * @param [writer] Writer to encode to
             * @returns Writer
             */
            public static encode(message: mirabuf.joint.IJoints, writer?: $protobuf.Writer): $protobuf.Writer

            /**
             * Encodes the specified Joints message, length delimited. Does not implicitly {@link mirabuf.joint.Joints.verify|verify} messages.
             * @param message Joints message or plain object to encode
             * @param [writer] Writer to encode to
             * @returns Writer
             */
            public static encodeDelimited(message: mirabuf.joint.IJoints, writer?: $protobuf.Writer): $protobuf.Writer

            /**
             * Decodes a Joints message from the specified reader or buffer.
             * @param reader Reader or buffer to decode from
             * @param [length] Message length if known beforehand
             * @returns Joints
             * @throws {Error} If the payload is not a reader or valid buffer
             * @throws {$protobuf.util.ProtocolError} If required fields are missing
             */
            public static decode(reader: $protobuf.Reader | Uint8Array, length?: number): mirabuf.joint.Joints

            /**
             * Decodes a Joints message from the specified reader or buffer, length delimited.
             * @param reader Reader or buffer to decode from
             * @returns Joints
             * @throws {Error} If the payload is not a reader or valid buffer
             * @throws {$protobuf.util.ProtocolError} If required fields are missing
             */
            public static decodeDelimited(reader: $protobuf.Reader | Uint8Array): mirabuf.joint.Joints

            /**
             * Verifies a Joints message.
             * @param message Plain object to verify
             * @returns `null` if valid, otherwise the reason why it is not
             */
            public static verify(message: { [k: string]: unknown }): string | null

            /**
             * Creates a Joints message from a plain object. Also converts values to their respective internal types.
             * @param object Plain object
             * @returns Joints
             */
            public static fromObject(object: { [k: string]: unknown }): mirabuf.joint.Joints

            /**
             * Creates a plain object from a Joints message. Also converts values to other types if specified.
             * @param message Joints
             * @param [options] Conversion options
             * @returns Plain object
             */
            public static toObject(
                message: mirabuf.joint.Joints,
                options?: $protobuf.IConversionOptions
            ): { [k: string]: unknown }

            /**
             * Converts this Joints to JSON.
             * @returns JSON object
             */
            public toJSON(): { [k: string]: unknown }

            /**
             * Gets the default type url for Joints
             * @param [typeUrlPrefix] your custom typeUrlPrefix(default "type.googleapis.com")
             * @returns The default type url
             */
            public static getTypeUrl(typeUrlPrefix?: string): string
        }

        /** JointMotion enum. */
        enum JointMotion {
            RIGID = 0,
            REVOLUTE = 1,
            SLIDER = 2,
            CYLINDRICAL = 3,
            PINSLOT = 4,
            PLANAR = 5,
            BALL = 6,
            CUSTOM = 7,
        }

        /** Properties of a JointInstance. */
        interface IJointInstance {
            /** JointInstance info */
            info?: mirabuf.IInfo | null

            /** JointInstance isEndEffector */
            isEndEffector?: boolean | null

            /** JointInstance parentPart */
            parentPart?: string | null

            /** JointInstance childPart */
            childPart?: string | null

            /** JointInstance jointReference */
            jointReference?: string | null

            /** JointInstance offset */
            offset?: mirabuf.IVector3 | null

            /** JointInstance parts */
            parts?: mirabuf.IGraphContainer | null

            /** JointInstance signalReference */
            signalReference?: string | null

            /** JointInstance motionLink */
            motionLink?: mirabuf.joint.IMotionLink[] | null
        }

        /**
         * Instance of a Joint that has a defined motion and limits.
         * Instancing helps with identifiy closed loop systems.
         */
        class JointInstance implements IJointInstance {
            /**
             * Constructs a new JointInstance.
             * @param [properties] Properties to set
             */
            constructor(properties?: mirabuf.joint.IJointInstance)

            /** JointInstance info. */
            public info?: mirabuf.IInfo | null

            /** JointInstance isEndEffector. */
            public isEndEffector: boolean

            /** JointInstance parentPart. */
            public parentPart: string

            /** JointInstance childPart. */
            public childPart: string

            /** JointInstance jointReference. */
            public jointReference: string

            /** JointInstance offset. */
            public offset?: mirabuf.IVector3 | null

            /** JointInstance parts. */
            public parts?: mirabuf.IGraphContainer | null

            /** JointInstance signalReference. */
            public signalReference: string

            /** JointInstance motionLink. */
            public motionLink: mirabuf.joint.IMotionLink[]

            /**
             * Creates a new JointInstance instance using the specified properties.
             * @param [properties] Properties to set
             * @returns JointInstance instance
             */
            public static create(properties?: mirabuf.joint.IJointInstance): mirabuf.joint.JointInstance

            /**
             * Encodes the specified JointInstance message. Does not implicitly {@link mirabuf.joint.JointInstance.verify|verify} messages.
             * @param message JointInstance message or plain object to encode
             * @param [writer] Writer to encode to
             * @returns Writer
             */
            public static encode(message: mirabuf.joint.IJointInstance, writer?: $protobuf.Writer): $protobuf.Writer

            /**
             * Encodes the specified JointInstance message, length delimited. Does not implicitly {@link mirabuf.joint.JointInstance.verify|verify} messages.
             * @param message JointInstance message or plain object to encode
             * @param [writer] Writer to encode to
             * @returns Writer
             */
            public static encodeDelimited(
                message: mirabuf.joint.IJointInstance,
                writer?: $protobuf.Writer
            ): $protobuf.Writer

            /**
             * Decodes a JointInstance message from the specified reader or buffer.
             * @param reader Reader or buffer to decode from
             * @param [length] Message length if known beforehand
             * @returns JointInstance
             * @throws {Error} If the payload is not a reader or valid buffer
             * @throws {$protobuf.util.ProtocolError} If required fields are missing
             */
            public static decode(reader: $protobuf.Reader | Uint8Array, length?: number): mirabuf.joint.JointInstance

            /**
             * Decodes a JointInstance message from the specified reader or buffer, length delimited.
             * @param reader Reader or buffer to decode from
             * @returns JointInstance
             * @throws {Error} If the payload is not a reader or valid buffer
             * @throws {$protobuf.util.ProtocolError} If required fields are missing
             */
            public static decodeDelimited(reader: $protobuf.Reader | Uint8Array): mirabuf.joint.JointInstance

            /**
             * Verifies a JointInstance message.
             * @param message Plain object to verify
             * @returns `null` if valid, otherwise the reason why it is not
             */
            public static verify(message: { [k: string]: unknown }): string | null

            /**
             * Creates a JointInstance message from a plain object. Also converts values to their respective internal types.
             * @param object Plain object
             * @returns JointInstance
             */
            public static fromObject(object: { [k: string]: unknown }): mirabuf.joint.JointInstance

            /**
             * Creates a plain object from a JointInstance message. Also converts values to other types if specified.
             * @param message JointInstance
             * @param [options] Conversion options
             * @returns Plain object
             */
            public static toObject(
                message: mirabuf.joint.JointInstance,
                options?: $protobuf.IConversionOptions
            ): { [k: string]: unknown }

            /**
             * Converts this JointInstance to JSON.
             * @returns JSON object
             */
            public toJSON(): { [k: string]: unknown }

            /**
             * Gets the default type url for JointInstance
             * @param [typeUrlPrefix] your custom typeUrlPrefix(default "type.googleapis.com")
             * @returns The default type url
             */
            public static getTypeUrl(typeUrlPrefix?: string): string
        }

        /** Properties of a MotionLink. */
        interface IMotionLink {
            /** MotionLink jointInstance */
            jointInstance?: string | null

            /** MotionLink ratio */
            ratio?: number | null

            /** MotionLink reversed */
            reversed?: boolean | null
        }

        /**
         * Motion Link Feature
         * Enables the restriction on a joint to a certain range of motion as it is relative to another joint
         * This is useful for moving parts restricted by belts and gears
         */
        class MotionLink implements IMotionLink {
            /**
             * Constructs a new MotionLink.
             * @param [properties] Properties to set
             */
            constructor(properties?: mirabuf.joint.IMotionLink)

            /** MotionLink jointInstance. */
            public jointInstance: string

            /** MotionLink ratio. */
            public ratio: number

            /** MotionLink reversed. */
            public reversed: boolean

            /**
             * Creates a new MotionLink instance using the specified properties.
             * @param [properties] Properties to set
             * @returns MotionLink instance
             */
            public static create(properties?: mirabuf.joint.IMotionLink): mirabuf.joint.MotionLink

            /**
             * Encodes the specified MotionLink message. Does not implicitly {@link mirabuf.joint.MotionLink.verify|verify} messages.
             * @param message MotionLink message or plain object to encode
             * @param [writer] Writer to encode to
             * @returns Writer
             */
            public static encode(message: mirabuf.joint.IMotionLink, writer?: $protobuf.Writer): $protobuf.Writer

            /**
             * Encodes the specified MotionLink message, length delimited. Does not implicitly {@link mirabuf.joint.MotionLink.verify|verify} messages.
             * @param message MotionLink message or plain object to encode
             * @param [writer] Writer to encode to
             * @returns Writer
             */
            public static encodeDelimited(
                message: mirabuf.joint.IMotionLink,
                writer?: $protobuf.Writer
            ): $protobuf.Writer

            /**
             * Decodes a MotionLink message from the specified reader or buffer.
             * @param reader Reader or buffer to decode from
             * @param [length] Message length if known beforehand
             * @returns MotionLink
             * @throws {Error} If the payload is not a reader or valid buffer
             * @throws {$protobuf.util.ProtocolError} If required fields are missing
             */
            public static decode(reader: $protobuf.Reader | Uint8Array, length?: number): mirabuf.joint.MotionLink

            /**
             * Decodes a MotionLink message from the specified reader or buffer, length delimited.
             * @param reader Reader or buffer to decode from
             * @returns MotionLink
             * @throws {Error} If the payload is not a reader or valid buffer
             * @throws {$protobuf.util.ProtocolError} If required fields are missing
             */
            public static decodeDelimited(reader: $protobuf.Reader | Uint8Array): mirabuf.joint.MotionLink

            /**
             * Verifies a MotionLink message.
             * @param message Plain object to verify
             * @returns `null` if valid, otherwise the reason why it is not
             */
            public static verify(message: { [k: string]: unknown }): string | null

            /**
             * Creates a MotionLink message from a plain object. Also converts values to their respective internal types.
             * @param object Plain object
             * @returns MotionLink
             */
            public static fromObject(object: { [k: string]: unknown }): mirabuf.joint.MotionLink

            /**
             * Creates a plain object from a MotionLink message. Also converts values to other types if specified.
             * @param message MotionLink
             * @param [options] Conversion options
             * @returns Plain object
             */
            public static toObject(
                message: mirabuf.joint.MotionLink,
                options?: $protobuf.IConversionOptions
            ): { [k: string]: unknown }

            /**
             * Converts this MotionLink to JSON.
             * @returns JSON object
             */
            public toJSON(): { [k: string]: unknown }

            /**
             * Gets the default type url for MotionLink
             * @param [typeUrlPrefix] your custom typeUrlPrefix(default "type.googleapis.com")
             * @returns The default type url
             */
            public static getTypeUrl(typeUrlPrefix?: string): string
        }

        /** Properties of a Joint. */
        interface IJoint {
            /** Joint name, ID, version, etc */
            info?: mirabuf.IInfo | null

            /** Joint origin */
            origin?: mirabuf.IVector3 | null

            /** Joint jointMotionType */
            jointMotionType?: mirabuf.joint.JointMotion | null

            /** Joint breakMagnitude */
            breakMagnitude?: number | null

            /** ONEOF rotational joint */
            rotational?: mirabuf.joint.IRotationalJoint | null

            /** ONEOF prismatic joint */
            prismatic?: mirabuf.joint.IPrismaticJoint | null

            /** ONEOF custom joint */
            custom?: mirabuf.joint.ICustomJoint | null

            /** Additional information someone can query or store relative to your joint. */
            userData?: mirabuf.IUserData | null

            /** Motor definition reference to lookup in joints collection */
            motorReference?: string | null
        }

        /**
         * A unqiue implementation of a joint motion
         * Contains information about motion but not assembly relation
         * NOTE: A spring motion is a joint with no driver
         */
        class Joint implements IJoint {
            /**
             * Constructs a new Joint.
             * @param [properties] Properties to set
             */
            constructor(properties?: mirabuf.joint.IJoint)

            /** Joint name, ID, version, etc */
            public info?: mirabuf.IInfo | null

            /** Joint origin. */
            public origin?: mirabuf.IVector3 | null

            /** Joint jointMotionType. */
            public jointMotionType: mirabuf.joint.JointMotion

            /** Joint breakMagnitude. */
            public breakMagnitude: number

            /** ONEOF rotational joint */
            public rotational?: mirabuf.joint.IRotationalJoint | null

            /** ONEOF prismatic joint */
            public prismatic?: mirabuf.joint.IPrismaticJoint | null

            /** ONEOF custom joint */
            public custom?: mirabuf.joint.ICustomJoint | null

            /** Additional information someone can query or store relative to your joint. */
            public userData?: mirabuf.IUserData | null

            /** Motor definition reference to lookup in joints collection */
            public motorReference: string

            /** Joint JointMotion. */
            public JointMotion?: "rotational" | "prismatic" | "custom"

            /**
             * Creates a new Joint instance using the specified properties.
             * @param [properties] Properties to set
             * @returns Joint instance
             */
            public static create(properties?: mirabuf.joint.IJoint): mirabuf.joint.Joint

            /**
             * Encodes the specified Joint message. Does not implicitly {@link mirabuf.joint.Joint.verify|verify} messages.
             * @param message Joint message or plain object to encode
             * @param [writer] Writer to encode to
             * @returns Writer
             */
            public static encode(message: mirabuf.joint.IJoint, writer?: $protobuf.Writer): $protobuf.Writer

            /**
             * Encodes the specified Joint message, length delimited. Does not implicitly {@link mirabuf.joint.Joint.verify|verify} messages.
             * @param message Joint message or plain object to encode
             * @param [writer] Writer to encode to
             * @returns Writer
             */
            public static encodeDelimited(message: mirabuf.joint.IJoint, writer?: $protobuf.Writer): $protobuf.Writer

            /**
             * Decodes a Joint message from the specified reader or buffer.
             * @param reader Reader or buffer to decode from
             * @param [length] Message length if known beforehand
             * @returns Joint
             * @throws {Error} If the payload is not a reader or valid buffer
             * @throws {$protobuf.util.ProtocolError} If required fields are missing
             */
            public static decode(reader: $protobuf.Reader | Uint8Array, length?: number): mirabuf.joint.Joint

            /**
             * Decodes a Joint message from the specified reader or buffer, length delimited.
             * @param reader Reader or buffer to decode from
             * @returns Joint
             * @throws {Error} If the payload is not a reader or valid buffer
             * @throws {$protobuf.util.ProtocolError} If required fields are missing
             */
            public static decodeDelimited(reader: $protobuf.Reader | Uint8Array): mirabuf.joint.Joint

            /**
             * Verifies a Joint message.
             * @param message Plain object to verify
             * @returns `null` if valid, otherwise the reason why it is not
             */
            public static verify(message: { [k: string]: unknown }): string | null

            /**
             * Creates a Joint message from a plain object. Also converts values to their respective internal types.
             * @param object Plain object
             * @returns Joint
             */
            public static fromObject(object: { [k: string]: unknown }): mirabuf.joint.Joint

            /**
             * Creates a plain object from a Joint message. Also converts values to other types if specified.
             * @param message Joint
             * @param [options] Conversion options
             * @returns Plain object
             */
            public static toObject(
                message: mirabuf.joint.Joint,
                options?: $protobuf.IConversionOptions
            ): { [k: string]: unknown }

            /**
             * Converts this Joint to JSON.
             * @returns JSON object
             */
            public toJSON(): { [k: string]: unknown }

            /**
             * Gets the default type url for Joint
             * @param [typeUrlPrefix] your custom typeUrlPrefix(default "type.googleapis.com")
             * @returns The default type url
             */
            public static getTypeUrl(typeUrlPrefix?: string): string
        }

        /** Properties of a Dynamics. */
        interface IDynamics {
            /** Damping effect on a given joint motion */
            damping?: number | null

            /** Friction effect on a given joint motion */
            friction?: number | null
        }

        /** Dynamics specify the mechanical effects on the motion. */
        class Dynamics implements IDynamics {
            /**
             * Constructs a new Dynamics.
             * @param [properties] Properties to set
             */
            constructor(properties?: mirabuf.joint.IDynamics)

            /** Damping effect on a given joint motion */
            public damping: number

            /** Friction effect on a given joint motion */
            public friction: number

            /**
             * Creates a new Dynamics instance using the specified properties.
             * @param [properties] Properties to set
             * @returns Dynamics instance
             */
            public static create(properties?: mirabuf.joint.IDynamics): mirabuf.joint.Dynamics

            /**
             * Encodes the specified Dynamics message. Does not implicitly {@link mirabuf.joint.Dynamics.verify|verify} messages.
             * @param message Dynamics message or plain object to encode
             * @param [writer] Writer to encode to
             * @returns Writer
             */
            public static encode(message: mirabuf.joint.IDynamics, writer?: $protobuf.Writer): $protobuf.Writer

            /**
             * Encodes the specified Dynamics message, length delimited. Does not implicitly {@link mirabuf.joint.Dynamics.verify|verify} messages.
             * @param message Dynamics message or plain object to encode
             * @param [writer] Writer to encode to
             * @returns Writer
             */
            public static encodeDelimited(message: mirabuf.joint.IDynamics, writer?: $protobuf.Writer): $protobuf.Writer

            /**
             * Decodes a Dynamics message from the specified reader or buffer.
             * @param reader Reader or buffer to decode from
             * @param [length] Message length if known beforehand
             * @returns Dynamics
             * @throws {Error} If the payload is not a reader or valid buffer
             * @throws {$protobuf.util.ProtocolError} If required fields are missing
             */
            public static decode(reader: $protobuf.Reader | Uint8Array, length?: number): mirabuf.joint.Dynamics

            /**
             * Decodes a Dynamics message from the specified reader or buffer, length delimited.
             * @param reader Reader or buffer to decode from
             * @returns Dynamics
             * @throws {Error} If the payload is not a reader or valid buffer
             * @throws {$protobuf.util.ProtocolError} If required fields are missing
             */
            public static decodeDelimited(reader: $protobuf.Reader | Uint8Array): mirabuf.joint.Dynamics

            /**
             * Verifies a Dynamics message.
             * @param message Plain object to verify
             * @returns `null` if valid, otherwise the reason why it is not
             */
            public static verify(message: { [k: string]: unknown }): string | null

            /**
             * Creates a Dynamics message from a plain object. Also converts values to their respective internal types.
             * @param object Plain object
             * @returns Dynamics
             */
            public static fromObject(object: { [k: string]: unknown }): mirabuf.joint.Dynamics

            /**
             * Creates a plain object from a Dynamics message. Also converts values to other types if specified.
             * @param message Dynamics
             * @param [options] Conversion options
             * @returns Plain object
             */
            public static toObject(
                message: mirabuf.joint.Dynamics,
                options?: $protobuf.IConversionOptions
            ): { [k: string]: unknown }

            /**
             * Converts this Dynamics to JSON.
             * @returns JSON object
             */
            public toJSON(): { [k: string]: unknown }

            /**
             * Gets the default type url for Dynamics
             * @param [typeUrlPrefix] your custom typeUrlPrefix(default "type.googleapis.com")
             * @returns The default type url
             */
            public static getTypeUrl(typeUrlPrefix?: string): string
        }

        /** Properties of a Limits. */
        interface ILimits {
            /** Lower Limit corresponds to default displacement */
            lower?: number | null

            /** Upper Limit is the joint extent */
            upper?: number | null

            /** Velocity Max in m/s^2 (angular for rotational) */
            velocity?: number | null

            /** Effort is the absolute force a joint can apply for a given instant - ROS has a great article on it http://wiki.ros.org/pr2_controller_manager/safety_limits */
            effort?: number | null
        }

        /**
         * Limits specify the mechanical range of a given joint.
         *
         * TODO: Add units
         */
        class Limits implements ILimits {
            /**
             * Constructs a new Limits.
             * @param [properties] Properties to set
             */
            constructor(properties?: mirabuf.joint.ILimits)

            /** Lower Limit corresponds to default displacement */
            public lower: number

            /** Upper Limit is the joint extent */
            public upper: number

            /** Velocity Max in m/s^2 (angular for rotational) */
            public velocity: number

            /** Effort is the absolute force a joint can apply for a given instant - ROS has a great article on it http://wiki.ros.org/pr2_controller_manager/safety_limits */
            public effort: number

            /**
             * Creates a new Limits instance using the specified properties.
             * @param [properties] Properties to set
             * @returns Limits instance
             */
            public static create(properties?: mirabuf.joint.ILimits): mirabuf.joint.Limits

            /**
             * Encodes the specified Limits message. Does not implicitly {@link mirabuf.joint.Limits.verify|verify} messages.
             * @param message Limits message or plain object to encode
             * @param [writer] Writer to encode to
             * @returns Writer
             */
            public static encode(message: mirabuf.joint.ILimits, writer?: $protobuf.Writer): $protobuf.Writer

            /**
             * Encodes the specified Limits message, length delimited. Does not implicitly {@link mirabuf.joint.Limits.verify|verify} messages.
             * @param message Limits message or plain object to encode
             * @param [writer] Writer to encode to
             * @returns Writer
             */
            public static encodeDelimited(message: mirabuf.joint.ILimits, writer?: $protobuf.Writer): $protobuf.Writer

            /**
             * Decodes a Limits message from the specified reader or buffer.
             * @param reader Reader or buffer to decode from
             * @param [length] Message length if known beforehand
             * @returns Limits
             * @throws {Error} If the payload is not a reader or valid buffer
             * @throws {$protobuf.util.ProtocolError} If required fields are missing
             */
            public static decode(reader: $protobuf.Reader | Uint8Array, length?: number): mirabuf.joint.Limits

            /**
             * Decodes a Limits message from the specified reader or buffer, length delimited.
             * @param reader Reader or buffer to decode from
             * @returns Limits
             * @throws {Error} If the payload is not a reader or valid buffer
             * @throws {$protobuf.util.ProtocolError} If required fields are missing
             */
            public static decodeDelimited(reader: $protobuf.Reader | Uint8Array): mirabuf.joint.Limits

            /**
             * Verifies a Limits message.
             * @param message Plain object to verify
             * @returns `null` if valid, otherwise the reason why it is not
             */
            public static verify(message: { [k: string]: unknown }): string | null

            /**
             * Creates a Limits message from a plain object. Also converts values to their respective internal types.
             * @param object Plain object
             * @returns Limits
             */
            public static fromObject(object: { [k: string]: unknown }): mirabuf.joint.Limits

            /**
             * Creates a plain object from a Limits message. Also converts values to other types if specified.
             * @param message Limits
             * @param [options] Conversion options
             * @returns Plain object
             */
            public static toObject(
                message: mirabuf.joint.Limits,
                options?: $protobuf.IConversionOptions
            ): { [k: string]: unknown }

            /**
             * Converts this Limits to JSON.
             * @returns JSON object
             */
            public toJSON(): { [k: string]: unknown }

            /**
             * Gets the default type url for Limits
             * @param [typeUrlPrefix] your custom typeUrlPrefix(default "type.googleapis.com")
             * @returns The default type url
             */
            public static getTypeUrl(typeUrlPrefix?: string): string
        }

        /** Properties of a Safety. */
        interface ISafety {
            /** Lower software limit */
            lowerLimit?: number | null

            /** Upper Software limit */
            upperLimit?: number | null

            /** Relation between position and velocity limit */
            kPosition?: number | null

            /** Relation between effort and velocity limit */
            kVelocity?: number | null
        }

        /**
         * Safety switch configuration for a given joint.
         * Can usefully indicate a bounds issue.
         * Inspired by the URDF implementation.
         *
         * This should really just be created by the controller.
         * http://wiki.ros.org/pr2_controller_manager/safety_limits
         */
        class Safety implements ISafety {
            /**
             * Constructs a new Safety.
             * @param [properties] Properties to set
             */
            constructor(properties?: mirabuf.joint.ISafety)

            /** Lower software limit */
            public lowerLimit: number

            /** Upper Software limit */
            public upperLimit: number

            /** Relation between position and velocity limit */
            public kPosition: number

            /** Relation between effort and velocity limit */
            public kVelocity: number

            /**
             * Creates a new Safety instance using the specified properties.
             * @param [properties] Properties to set
             * @returns Safety instance
             */
            public static create(properties?: mirabuf.joint.ISafety): mirabuf.joint.Safety

            /**
             * Encodes the specified Safety message. Does not implicitly {@link mirabuf.joint.Safety.verify|verify} messages.
             * @param message Safety message or plain object to encode
             * @param [writer] Writer to encode to
             * @returns Writer
             */
            public static encode(message: mirabuf.joint.ISafety, writer?: $protobuf.Writer): $protobuf.Writer

            /**
             * Encodes the specified Safety message, length delimited. Does not implicitly {@link mirabuf.joint.Safety.verify|verify} messages.
             * @param message Safety message or plain object to encode
             * @param [writer] Writer to encode to
             * @returns Writer
             */
            public static encodeDelimited(message: mirabuf.joint.ISafety, writer?: $protobuf.Writer): $protobuf.Writer

            /**
             * Decodes a Safety message from the specified reader or buffer.
             * @param reader Reader or buffer to decode from
             * @param [length] Message length if known beforehand
             * @returns Safety
             * @throws {Error} If the payload is not a reader or valid buffer
             * @throws {$protobuf.util.ProtocolError} If required fields are missing
             */
            public static decode(reader: $protobuf.Reader | Uint8Array, length?: number): mirabuf.joint.Safety

            /**
             * Decodes a Safety message from the specified reader or buffer, length delimited.
             * @param reader Reader or buffer to decode from
             * @returns Safety
             * @throws {Error} If the payload is not a reader or valid buffer
             * @throws {$protobuf.util.ProtocolError} If required fields are missing
             */
            public static decodeDelimited(reader: $protobuf.Reader | Uint8Array): mirabuf.joint.Safety

            /**
             * Verifies a Safety message.
             * @param message Plain object to verify
             * @returns `null` if valid, otherwise the reason why it is not
             */
            public static verify(message: { [k: string]: unknown }): string | null

            /**
             * Creates a Safety message from a plain object. Also converts values to their respective internal types.
             * @param object Plain object
             * @returns Safety
             */
            public static fromObject(object: { [k: string]: unknown }): mirabuf.joint.Safety

            /**
             * Creates a plain object from a Safety message. Also converts values to other types if specified.
             * @param message Safety
             * @param [options] Conversion options
             * @returns Plain object
             */
            public static toObject(
                message: mirabuf.joint.Safety,
                options?: $protobuf.IConversionOptions
            ): { [k: string]: unknown }

            /**
             * Converts this Safety to JSON.
             * @returns JSON object
             */
            public toJSON(): { [k: string]: unknown }

            /**
             * Gets the default type url for Safety
             * @param [typeUrlPrefix] your custom typeUrlPrefix(default "type.googleapis.com")
             * @returns The default type url
             */
            public static getTypeUrl(typeUrlPrefix?: string): string
        }

        /** Properties of a DOF. */
        interface IDOF {
            /** In case you want to name this degree of freedom */
            name?: string | null

            /** Axis the degree of freedom is pivoting by */
            axis?: mirabuf.IVector3 | null

            /** Direction the axis vector is offset from - this has an incorrect naming scheme */
            pivotDirection?: mirabuf.Axis | null

            /** Dynamic properties of this joint pivot */
            dynamics?: mirabuf.joint.IDynamics | null

            /** Limits of this freedom */
            limits?: mirabuf.joint.ILimits | null

            /** Current value of the DOF */
            value?: number | null
        }

        /** DOF - representing the construction of a joint motion */
        class DOF implements IDOF {
            /**
             * Constructs a new DOF.
             * @param [properties] Properties to set
             */
            constructor(properties?: mirabuf.joint.IDOF)

            /** In case you want to name this degree of freedom */
            public name: string

            /** Axis the degree of freedom is pivoting by */
            public axis?: mirabuf.IVector3 | null

            /** Direction the axis vector is offset from - this has an incorrect naming scheme */
            public pivotDirection: mirabuf.Axis

            /** Dynamic properties of this joint pivot */
            public dynamics?: mirabuf.joint.IDynamics | null

            /** Limits of this freedom */
            public limits?: mirabuf.joint.ILimits | null

            /** Current value of the DOF */
            public value: number

            /**
             * Creates a new DOF instance using the specified properties.
             * @param [properties] Properties to set
             * @returns DOF instance
             */
            public static create(properties?: mirabuf.joint.IDOF): mirabuf.joint.DOF

            /**
             * Encodes the specified DOF message. Does not implicitly {@link mirabuf.joint.DOF.verify|verify} messages.
             * @param message DOF message or plain object to encode
             * @param [writer] Writer to encode to
             * @returns Writer
             */
            public static encode(message: mirabuf.joint.IDOF, writer?: $protobuf.Writer): $protobuf.Writer

            /**
             * Encodes the specified DOF message, length delimited. Does not implicitly {@link mirabuf.joint.DOF.verify|verify} messages.
             * @param message DOF message or plain object to encode
             * @param [writer] Writer to encode to
             * @returns Writer
             */
            public static encodeDelimited(message: mirabuf.joint.IDOF, writer?: $protobuf.Writer): $protobuf.Writer

            /**
             * Decodes a DOF message from the specified reader or buffer.
             * @param reader Reader or buffer to decode from
             * @param [length] Message length if known beforehand
             * @returns DOF
             * @throws {Error} If the payload is not a reader or valid buffer
             * @throws {$protobuf.util.ProtocolError} If required fields are missing
             */
            public static decode(reader: $protobuf.Reader | Uint8Array, length?: number): mirabuf.joint.DOF

            /**
             * Decodes a DOF message from the specified reader or buffer, length delimited.
             * @param reader Reader or buffer to decode from
             * @returns DOF
             * @throws {Error} If the payload is not a reader or valid buffer
             * @throws {$protobuf.util.ProtocolError} If required fields are missing
             */
            public static decodeDelimited(reader: $protobuf.Reader | Uint8Array): mirabuf.joint.DOF

            /**
             * Verifies a DOF message.
             * @param message Plain object to verify
             * @returns `null` if valid, otherwise the reason why it is not
             */
            public static verify(message: { [k: string]: unknown }): string | null

            /**
             * Creates a DOF message from a plain object. Also converts values to their respective internal types.
             * @param object Plain object
             * @returns DOF
             */
            public static fromObject(object: { [k: string]: unknown }): mirabuf.joint.DOF

            /**
             * Creates a plain object from a DOF message. Also converts values to other types if specified.
             * @param message DOF
             * @param [options] Conversion options
             * @returns Plain object
             */
            public static toObject(
                message: mirabuf.joint.DOF,
                options?: $protobuf.IConversionOptions
            ): { [k: string]: unknown }

            /**
             * Converts this DOF to JSON.
             * @returns JSON object
             */
            public toJSON(): { [k: string]: unknown }

            /**
             * Gets the default type url for DOF
             * @param [typeUrlPrefix] your custom typeUrlPrefix(default "type.googleapis.com")
             * @returns The default type url
             */
            public static getTypeUrl(typeUrlPrefix?: string): string
        }

        /** Properties of a CustomJoint. */
        interface ICustomJoint {
            /** A list of degrees of freedom that the joint can contain */
            dofs?: mirabuf.joint.IDOF[] | null
        }

        /**
         * CustomJoint is a joint with N degrees of freedom specified.
         * There should be input validation to handle max freedom case.
         */
        class CustomJoint implements ICustomJoint {
            /**
             * Constructs a new CustomJoint.
             * @param [properties] Properties to set
             */
            constructor(properties?: mirabuf.joint.ICustomJoint)

            /** A list of degrees of freedom that the joint can contain */
            public dofs: mirabuf.joint.IDOF[]

            /**
             * Creates a new CustomJoint instance using the specified properties.
             * @param [properties] Properties to set
             * @returns CustomJoint instance
             */
            public static create(properties?: mirabuf.joint.ICustomJoint): mirabuf.joint.CustomJoint

            /**
             * Encodes the specified CustomJoint message. Does not implicitly {@link mirabuf.joint.CustomJoint.verify|verify} messages.
             * @param message CustomJoint message or plain object to encode
             * @param [writer] Writer to encode to
             * @returns Writer
             */
            public static encode(message: mirabuf.joint.ICustomJoint, writer?: $protobuf.Writer): $protobuf.Writer

            /**
             * Encodes the specified CustomJoint message, length delimited. Does not implicitly {@link mirabuf.joint.CustomJoint.verify|verify} messages.
             * @param message CustomJoint message or plain object to encode
             * @param [writer] Writer to encode to
             * @returns Writer
             */
            public static encodeDelimited(
                message: mirabuf.joint.ICustomJoint,
                writer?: $protobuf.Writer
            ): $protobuf.Writer

            /**
             * Decodes a CustomJoint message from the specified reader or buffer.
             * @param reader Reader or buffer to decode from
             * @param [length] Message length if known beforehand
             * @returns CustomJoint
             * @throws {Error} If the payload is not a reader or valid buffer
             * @throws {$protobuf.util.ProtocolError} If required fields are missing
             */
            public static decode(reader: $protobuf.Reader | Uint8Array, length?: number): mirabuf.joint.CustomJoint

            /**
             * Decodes a CustomJoint message from the specified reader or buffer, length delimited.
             * @param reader Reader or buffer to decode from
             * @returns CustomJoint
             * @throws {Error} If the payload is not a reader or valid buffer
             * @throws {$protobuf.util.ProtocolError} If required fields are missing
             */
            public static decodeDelimited(reader: $protobuf.Reader | Uint8Array): mirabuf.joint.CustomJoint

            /**
             * Verifies a CustomJoint message.
             * @param message Plain object to verify
             * @returns `null` if valid, otherwise the reason why it is not
             */
            public static verify(message: { [k: string]: unknown }): string | null

            /**
             * Creates a CustomJoint message from a plain object. Also converts values to their respective internal types.
             * @param object Plain object
             * @returns CustomJoint
             */
            public static fromObject(object: { [k: string]: unknown }): mirabuf.joint.CustomJoint

            /**
             * Creates a plain object from a CustomJoint message. Also converts values to other types if specified.
             * @param message CustomJoint
             * @param [options] Conversion options
             * @returns Plain object
             */
            public static toObject(
                message: mirabuf.joint.CustomJoint,
                options?: $protobuf.IConversionOptions
            ): { [k: string]: unknown }

            /**
             * Converts this CustomJoint to JSON.
             * @returns JSON object
             */
            public toJSON(): { [k: string]: unknown }

            /**
             * Gets the default type url for CustomJoint
             * @param [typeUrlPrefix] your custom typeUrlPrefix(default "type.googleapis.com")
             * @returns The default type url
             */
            public static getTypeUrl(typeUrlPrefix?: string): string
        }

        /** Properties of a RotationalJoint. */
        interface IRotationalJoint {
            /** RotationalJoint rotationalFreedom */
            rotationalFreedom?: mirabuf.joint.IDOF | null
        }

        /**
         * RotationalJoint describes a joint with rotational translation.
         * This is the exact same as prismatic for now.
         */
        class RotationalJoint implements IRotationalJoint {
            /**
             * Constructs a new RotationalJoint.
             * @param [properties] Properties to set
             */
            constructor(properties?: mirabuf.joint.IRotationalJoint)

            /** RotationalJoint rotationalFreedom. */
            public rotationalFreedom?: mirabuf.joint.IDOF | null

            /**
             * Creates a new RotationalJoint instance using the specified properties.
             * @param [properties] Properties to set
             * @returns RotationalJoint instance
             */
            public static create(properties?: mirabuf.joint.IRotationalJoint): mirabuf.joint.RotationalJoint

            /**
             * Encodes the specified RotationalJoint message. Does not implicitly {@link mirabuf.joint.RotationalJoint.verify|verify} messages.
             * @param message RotationalJoint message or plain object to encode
             * @param [writer] Writer to encode to
             * @returns Writer
             */
            public static encode(message: mirabuf.joint.IRotationalJoint, writer?: $protobuf.Writer): $protobuf.Writer

            /**
             * Encodes the specified RotationalJoint message, length delimited. Does not implicitly {@link mirabuf.joint.RotationalJoint.verify|verify} messages.
             * @param message RotationalJoint message or plain object to encode
             * @param [writer] Writer to encode to
             * @returns Writer
             */
            public static encodeDelimited(
                message: mirabuf.joint.IRotationalJoint,
                writer?: $protobuf.Writer
            ): $protobuf.Writer

            /**
             * Decodes a RotationalJoint message from the specified reader or buffer.
             * @param reader Reader or buffer to decode from
             * @param [length] Message length if known beforehand
             * @returns RotationalJoint
             * @throws {Error} If the payload is not a reader or valid buffer
             * @throws {$protobuf.util.ProtocolError} If required fields are missing
             */
            public static decode(reader: $protobuf.Reader | Uint8Array, length?: number): mirabuf.joint.RotationalJoint

            /**
             * Decodes a RotationalJoint message from the specified reader or buffer, length delimited.
             * @param reader Reader or buffer to decode from
             * @returns RotationalJoint
             * @throws {Error} If the payload is not a reader or valid buffer
             * @throws {$protobuf.util.ProtocolError} If required fields are missing
             */
            public static decodeDelimited(reader: $protobuf.Reader | Uint8Array): mirabuf.joint.RotationalJoint

            /**
             * Verifies a RotationalJoint message.
             * @param message Plain object to verify
             * @returns `null` if valid, otherwise the reason why it is not
             */
            public static verify(message: { [k: string]: unknown }): string | null

            /**
             * Creates a RotationalJoint message from a plain object. Also converts values to their respective internal types.
             * @param object Plain object
             * @returns RotationalJoint
             */
            public static fromObject(object: { [k: string]: unknown }): mirabuf.joint.RotationalJoint

            /**
             * Creates a plain object from a RotationalJoint message. Also converts values to other types if specified.
             * @param message RotationalJoint
             * @param [options] Conversion options
             * @returns Plain object
             */
            public static toObject(
                message: mirabuf.joint.RotationalJoint,
                options?: $protobuf.IConversionOptions
            ): { [k: string]: unknown }

            /**
             * Converts this RotationalJoint to JSON.
             * @returns JSON object
             */
            public toJSON(): { [k: string]: unknown }

            /**
             * Gets the default type url for RotationalJoint
             * @param [typeUrlPrefix] your custom typeUrlPrefix(default "type.googleapis.com")
             * @returns The default type url
             */
            public static getTypeUrl(typeUrlPrefix?: string): string
        }

        /** Properties of a BallJoint. */
        interface IBallJoint {
            /** BallJoint yaw */
            yaw?: mirabuf.joint.IDOF | null

            /** BallJoint pitch */
            pitch?: mirabuf.joint.IDOF | null

            /** BallJoint rotation */
            rotation?: mirabuf.joint.IDOF | null
        }

        /** Represents a BallJoint. */
        class BallJoint implements IBallJoint {
            /**
             * Constructs a new BallJoint.
             * @param [properties] Properties to set
             */
            constructor(properties?: mirabuf.joint.IBallJoint)

            /** BallJoint yaw. */
            public yaw?: mirabuf.joint.IDOF | null

            /** BallJoint pitch. */
            public pitch?: mirabuf.joint.IDOF | null

            /** BallJoint rotation. */
            public rotation?: mirabuf.joint.IDOF | null

            /**
             * Creates a new BallJoint instance using the specified properties.
             * @param [properties] Properties to set
             * @returns BallJoint instance
             */
            public static create(properties?: mirabuf.joint.IBallJoint): mirabuf.joint.BallJoint

            /**
             * Encodes the specified BallJoint message. Does not implicitly {@link mirabuf.joint.BallJoint.verify|verify} messages.
             * @param message BallJoint message or plain object to encode
             * @param [writer] Writer to encode to
             * @returns Writer
             */
            public static encode(message: mirabuf.joint.IBallJoint, writer?: $protobuf.Writer): $protobuf.Writer

            /**
             * Encodes the specified BallJoint message, length delimited. Does not implicitly {@link mirabuf.joint.BallJoint.verify|verify} messages.
             * @param message BallJoint message or plain object to encode
             * @param [writer] Writer to encode to
             * @returns Writer
             */
            public static encodeDelimited(
                message: mirabuf.joint.IBallJoint,
                writer?: $protobuf.Writer
            ): $protobuf.Writer

            /**
             * Decodes a BallJoint message from the specified reader or buffer.
             * @param reader Reader or buffer to decode from
             * @param [length] Message length if known beforehand
             * @returns BallJoint
             * @throws {Error} If the payload is not a reader or valid buffer
             * @throws {$protobuf.util.ProtocolError} If required fields are missing
             */
            public static decode(reader: $protobuf.Reader | Uint8Array, length?: number): mirabuf.joint.BallJoint

            /**
             * Decodes a BallJoint message from the specified reader or buffer, length delimited.
             * @param reader Reader or buffer to decode from
             * @returns BallJoint
             * @throws {Error} If the payload is not a reader or valid buffer
             * @throws {$protobuf.util.ProtocolError} If required fields are missing
             */
            public static decodeDelimited(reader: $protobuf.Reader | Uint8Array): mirabuf.joint.BallJoint

            /**
             * Verifies a BallJoint message.
             * @param message Plain object to verify
             * @returns `null` if valid, otherwise the reason why it is not
             */
            public static verify(message: { [k: string]: unknown }): string | null

            /**
             * Creates a BallJoint message from a plain object. Also converts values to their respective internal types.
             * @param object Plain object
             * @returns BallJoint
             */
            public static fromObject(object: { [k: string]: unknown }): mirabuf.joint.BallJoint

            /**
             * Creates a plain object from a BallJoint message. Also converts values to other types if specified.
             * @param message BallJoint
             * @param [options] Conversion options
             * @returns Plain object
             */
            public static toObject(
                message: mirabuf.joint.BallJoint,
                options?: $protobuf.IConversionOptions
            ): { [k: string]: unknown }

            /**
             * Converts this BallJoint to JSON.
             * @returns JSON object
             */
            public toJSON(): { [k: string]: unknown }

            /**
             * Gets the default type url for BallJoint
             * @param [typeUrlPrefix] your custom typeUrlPrefix(default "type.googleapis.com")
             * @returns The default type url
             */
            public static getTypeUrl(typeUrlPrefix?: string): string
        }

        /** Properties of a PrismaticJoint. */
        interface IPrismaticJoint {
            /** PrismaticJoint prismaticFreedom */
            prismaticFreedom?: mirabuf.joint.IDOF | null
        }

        /** Prismatic Joint describes a motion that translates the position in a single axis */
        class PrismaticJoint implements IPrismaticJoint {
            /**
             * Constructs a new PrismaticJoint.
             * @param [properties] Properties to set
             */
            constructor(properties?: mirabuf.joint.IPrismaticJoint)

            /** PrismaticJoint prismaticFreedom. */
            public prismaticFreedom?: mirabuf.joint.IDOF | null

            /**
             * Creates a new PrismaticJoint instance using the specified properties.
             * @param [properties] Properties to set
             * @returns PrismaticJoint instance
             */
            public static create(properties?: mirabuf.joint.IPrismaticJoint): mirabuf.joint.PrismaticJoint

            /**
             * Encodes the specified PrismaticJoint message. Does not implicitly {@link mirabuf.joint.PrismaticJoint.verify|verify} messages.
             * @param message PrismaticJoint message or plain object to encode
             * @param [writer] Writer to encode to
             * @returns Writer
             */
            public static encode(message: mirabuf.joint.IPrismaticJoint, writer?: $protobuf.Writer): $protobuf.Writer

            /**
             * Encodes the specified PrismaticJoint message, length delimited. Does not implicitly {@link mirabuf.joint.PrismaticJoint.verify|verify} messages.
             * @param message PrismaticJoint message or plain object to encode
             * @param [writer] Writer to encode to
             * @returns Writer
             */
            public static encodeDelimited(
                message: mirabuf.joint.IPrismaticJoint,
                writer?: $protobuf.Writer
            ): $protobuf.Writer

            /**
             * Decodes a PrismaticJoint message from the specified reader or buffer.
             * @param reader Reader or buffer to decode from
             * @param [length] Message length if known beforehand
             * @returns PrismaticJoint
             * @throws {Error} If the payload is not a reader or valid buffer
             * @throws {$protobuf.util.ProtocolError} If required fields are missing
             */
            public static decode(reader: $protobuf.Reader | Uint8Array, length?: number): mirabuf.joint.PrismaticJoint

            /**
             * Decodes a PrismaticJoint message from the specified reader or buffer, length delimited.
             * @param reader Reader or buffer to decode from
             * @returns PrismaticJoint
             * @throws {Error} If the payload is not a reader or valid buffer
             * @throws {$protobuf.util.ProtocolError} If required fields are missing
             */
            public static decodeDelimited(reader: $protobuf.Reader | Uint8Array): mirabuf.joint.PrismaticJoint

            /**
             * Verifies a PrismaticJoint message.
             * @param message Plain object to verify
             * @returns `null` if valid, otherwise the reason why it is not
             */
            public static verify(message: { [k: string]: unknown }): string | null

            /**
             * Creates a PrismaticJoint message from a plain object. Also converts values to their respective internal types.
             * @param object Plain object
             * @returns PrismaticJoint
             */
            public static fromObject(object: { [k: string]: unknown }): mirabuf.joint.PrismaticJoint

            /**
             * Creates a plain object from a PrismaticJoint message. Also converts values to other types if specified.
             * @param message PrismaticJoint
             * @param [options] Conversion options
             * @returns Plain object
             */
            public static toObject(
                message: mirabuf.joint.PrismaticJoint,
                options?: $protobuf.IConversionOptions
            ): { [k: string]: unknown }

            /**
             * Converts this PrismaticJoint to JSON.
             * @returns JSON object
             */
            public toJSON(): { [k: string]: unknown }

            /**
             * Gets the default type url for PrismaticJoint
             * @param [typeUrlPrefix] your custom typeUrlPrefix(default "type.googleapis.com")
             * @returns The default type url
             */
            public static getTypeUrl(typeUrlPrefix?: string): string
        }

        /** Properties of a RigidGroup. */
        interface IRigidGroup {
            /** RigidGroup name */
            name?: string | null

            /** RigidGroup occurrences */
            occurrences?: string[] | null
        }

        /** Represents a RigidGroup. */
        class RigidGroup implements IRigidGroup {
            /**
             * Constructs a new RigidGroup.
             * @param [properties] Properties to set
             */
            constructor(properties?: mirabuf.joint.IRigidGroup)

            /** RigidGroup name. */
            public name: string

            /** RigidGroup occurrences. */
            public occurrences: string[]

            /**
             * Creates a new RigidGroup instance using the specified properties.
             * @param [properties] Properties to set
             * @returns RigidGroup instance
             */
            public static create(properties?: mirabuf.joint.IRigidGroup): mirabuf.joint.RigidGroup

            /**
             * Encodes the specified RigidGroup message. Does not implicitly {@link mirabuf.joint.RigidGroup.verify|verify} messages.
             * @param message RigidGroup message or plain object to encode
             * @param [writer] Writer to encode to
             * @returns Writer
             */
            public static encode(message: mirabuf.joint.IRigidGroup, writer?: $protobuf.Writer): $protobuf.Writer

            /**
             * Encodes the specified RigidGroup message, length delimited. Does not implicitly {@link mirabuf.joint.RigidGroup.verify|verify} messages.
             * @param message RigidGroup message or plain object to encode
             * @param [writer] Writer to encode to
             * @returns Writer
             */
            public static encodeDelimited(
                message: mirabuf.joint.IRigidGroup,
                writer?: $protobuf.Writer
            ): $protobuf.Writer

            /**
             * Decodes a RigidGroup message from the specified reader or buffer.
             * @param reader Reader or buffer to decode from
             * @param [length] Message length if known beforehand
             * @returns RigidGroup
             * @throws {Error} If the payload is not a reader or valid buffer
             * @throws {$protobuf.util.ProtocolError} If required fields are missing
             */
            public static decode(reader: $protobuf.Reader | Uint8Array, length?: number): mirabuf.joint.RigidGroup

            /**
             * Decodes a RigidGroup message from the specified reader or buffer, length delimited.
             * @param reader Reader or buffer to decode from
             * @returns RigidGroup
             * @throws {Error} If the payload is not a reader or valid buffer
             * @throws {$protobuf.util.ProtocolError} If required fields are missing
             */
            public static decodeDelimited(reader: $protobuf.Reader | Uint8Array): mirabuf.joint.RigidGroup

            /**
             * Verifies a RigidGroup message.
             * @param message Plain object to verify
             * @returns `null` if valid, otherwise the reason why it is not
             */
            public static verify(message: { [k: string]: unknown }): string | null

            /**
             * Creates a RigidGroup message from a plain object. Also converts values to their respective internal types.
             * @param object Plain object
             * @returns RigidGroup
             */
            public static fromObject(object: { [k: string]: unknown }): mirabuf.joint.RigidGroup

            /**
             * Creates a plain object from a RigidGroup message. Also converts values to other types if specified.
             * @param message RigidGroup
             * @param [options] Conversion options
             * @returns Plain object
             */
            public static toObject(
                message: mirabuf.joint.RigidGroup,
                options?: $protobuf.IConversionOptions
            ): { [k: string]: unknown }

            /**
             * Converts this RigidGroup to JSON.
             * @returns JSON object
             */
            public toJSON(): { [k: string]: unknown }

            /**
             * Gets the default type url for RigidGroup
             * @param [typeUrlPrefix] your custom typeUrlPrefix(default "type.googleapis.com")
             * @returns The default type url
             */
            public static getTypeUrl(typeUrlPrefix?: string): string
        }
    }

    /** Namespace motor. */
    namespace motor {
        /**
         * Duty Cycles for electric motors
         * Affects the dynamic output of the motor
         * https://www.news.benevelli-group.com/index.php/en/88-what-motor-duty-cycle.html
         * These each have associated data we are not going to use right now
         */
        enum DutyCycles {
            CONTINUOUS_RUNNING = 0,
            SHORT_TIME = 1,
            INTERMITTENT_PERIODIC = 2,
            CONTINUOUS_PERIODIC = 3,
        }

        /** Properties of a Motor. */
        interface IMotor {
            /** Motor info */
            info?: mirabuf.IInfo | null

            /** Motor dcMotor */
            dcMotor?: mirabuf.motor.IDCMotor | null

            /** Motor simpleMotor */
            simpleMotor?: mirabuf.motor.ISimpleMotor | null
        }

        /**
         * A Motor should determine the relationship between an input and joint motion
         * Could represent something like a DC Motor relationship
         */
        class Motor implements IMotor {
            /**
             * Constructs a new Motor.
             * @param [properties] Properties to set
             */
            constructor(properties?: mirabuf.motor.IMotor)

            /** Motor info. */
            public info?: mirabuf.IInfo | null

            /** Motor dcMotor. */
            public dcMotor?: mirabuf.motor.IDCMotor | null

            /** Motor simpleMotor. */
            public simpleMotor?: mirabuf.motor.ISimpleMotor | null

            /** Motor motorType. */
            public motorType?: "dcMotor" | "simpleMotor"

            /**
             * Creates a new Motor instance using the specified properties.
             * @param [properties] Properties to set
             * @returns Motor instance
             */
            public static create(properties?: mirabuf.motor.IMotor): mirabuf.motor.Motor

            /**
             * Encodes the specified Motor message. Does not implicitly {@link mirabuf.motor.Motor.verify|verify} messages.
             * @param message Motor message or plain object to encode
             * @param [writer] Writer to encode to
             * @returns Writer
             */
            public static encode(message: mirabuf.motor.IMotor, writer?: $protobuf.Writer): $protobuf.Writer

            /**
             * Encodes the specified Motor message, length delimited. Does not implicitly {@link mirabuf.motor.Motor.verify|verify} messages.
             * @param message Motor message or plain object to encode
             * @param [writer] Writer to encode to
             * @returns Writer
             */
            public static encodeDelimited(message: mirabuf.motor.IMotor, writer?: $protobuf.Writer): $protobuf.Writer

            /**
             * Decodes a Motor message from the specified reader or buffer.
             * @param reader Reader or buffer to decode from
             * @param [length] Message length if known beforehand
             * @returns Motor
             * @throws {Error} If the payload is not a reader or valid buffer
             * @throws {$protobuf.util.ProtocolError} If required fields are missing
             */
            public static decode(reader: $protobuf.Reader | Uint8Array, length?: number): mirabuf.motor.Motor

            /**
             * Decodes a Motor message from the specified reader or buffer, length delimited.
             * @param reader Reader or buffer to decode from
             * @returns Motor
             * @throws {Error} If the payload is not a reader or valid buffer
             * @throws {$protobuf.util.ProtocolError} If required fields are missing
             */
            public static decodeDelimited(reader: $protobuf.Reader | Uint8Array): mirabuf.motor.Motor

            /**
             * Verifies a Motor message.
             * @param message Plain object to verify
             * @returns `null` if valid, otherwise the reason why it is not
             */
            public static verify(message: { [k: string]: unknown }): string | null

            /**
             * Creates a Motor message from a plain object. Also converts values to their respective internal types.
             * @param object Plain object
             * @returns Motor
             */
            public static fromObject(object: { [k: string]: unknown }): mirabuf.motor.Motor

            /**
             * Creates a plain object from a Motor message. Also converts values to other types if specified.
             * @param message Motor
             * @param [options] Conversion options
             * @returns Plain object
             */
            public static toObject(
                message: mirabuf.motor.Motor,
                options?: $protobuf.IConversionOptions
            ): { [k: string]: unknown }

            /**
             * Converts this Motor to JSON.
             * @returns JSON object
             */
            public toJSON(): { [k: string]: unknown }

            /**
             * Gets the default type url for Motor
             * @param [typeUrlPrefix] your custom typeUrlPrefix(default "type.googleapis.com")
             * @returns The default type url
             */
            public static getTypeUrl(typeUrlPrefix?: string): string
        }

        /** Properties of a SimpleMotor. */
        interface ISimpleMotor {
            /** Torque at 0 rpm with a inverse linear relationship to max_velocity */
            stallTorque?: number | null

            /** The target velocity in RPM, will use stall_torque relationship to reach each step */
            maxVelocity?: number | null

            /** (Optional) 0 - 1, the relationship of stall_torque used to perserve the position of this motor */
            brakingConstant?: number | null
        }

        /**
         * SimpleMotor Configuration
         * Very easy motor used to simulate joints without specifying a real motor
         * Can set braking_constant - stall_torque - and max_velocity
         * Assumes you are solving using a velocity constraint for a joint and not a acceleration constraint
         */
        class SimpleMotor implements ISimpleMotor {
            /**
             * Constructs a new SimpleMotor.
             * @param [properties] Properties to set
             */
            constructor(properties?: mirabuf.motor.ISimpleMotor)

            /** Torque at 0 rpm with a inverse linear relationship to max_velocity */
            public stallTorque: number

            /** The target velocity in RPM, will use stall_torque relationship to reach each step */
            public maxVelocity: number

            /** (Optional) 0 - 1, the relationship of stall_torque used to perserve the position of this motor */
            public brakingConstant: number

            /**
             * Creates a new SimpleMotor instance using the specified properties.
             * @param [properties] Properties to set
             * @returns SimpleMotor instance
             */
            public static create(properties?: mirabuf.motor.ISimpleMotor): mirabuf.motor.SimpleMotor

            /**
             * Encodes the specified SimpleMotor message. Does not implicitly {@link mirabuf.motor.SimpleMotor.verify|verify} messages.
             * @param message SimpleMotor message or plain object to encode
             * @param [writer] Writer to encode to
             * @returns Writer
             */
            public static encode(message: mirabuf.motor.ISimpleMotor, writer?: $protobuf.Writer): $protobuf.Writer

            /**
             * Encodes the specified SimpleMotor message, length delimited. Does not implicitly {@link mirabuf.motor.SimpleMotor.verify|verify} messages.
             * @param message SimpleMotor message or plain object to encode
             * @param [writer] Writer to encode to
             * @returns Writer
             */
            public static encodeDelimited(
                message: mirabuf.motor.ISimpleMotor,
                writer?: $protobuf.Writer
            ): $protobuf.Writer

            /**
             * Decodes a SimpleMotor message from the specified reader or buffer.
             * @param reader Reader or buffer to decode from
             * @param [length] Message length if known beforehand
             * @returns SimpleMotor
             * @throws {Error} If the payload is not a reader or valid buffer
             * @throws {$protobuf.util.ProtocolError} If required fields are missing
             */
            public static decode(reader: $protobuf.Reader | Uint8Array, length?: number): mirabuf.motor.SimpleMotor

            /**
             * Decodes a SimpleMotor message from the specified reader or buffer, length delimited.
             * @param reader Reader or buffer to decode from
             * @returns SimpleMotor
             * @throws {Error} If the payload is not a reader or valid buffer
             * @throws {$protobuf.util.ProtocolError} If required fields are missing
             */
            public static decodeDelimited(reader: $protobuf.Reader | Uint8Array): mirabuf.motor.SimpleMotor

            /**
             * Verifies a SimpleMotor message.
             * @param message Plain object to verify
             * @returns `null` if valid, otherwise the reason why it is not
             */
            public static verify(message: { [k: string]: unknown }): string | null

            /**
             * Creates a SimpleMotor message from a plain object. Also converts values to their respective internal types.
             * @param object Plain object
             * @returns SimpleMotor
             */
            public static fromObject(object: { [k: string]: unknown }): mirabuf.motor.SimpleMotor

            /**
             * Creates a plain object from a SimpleMotor message. Also converts values to other types if specified.
             * @param message SimpleMotor
             * @param [options] Conversion options
             * @returns Plain object
             */
            public static toObject(
                message: mirabuf.motor.SimpleMotor,
                options?: $protobuf.IConversionOptions
            ): { [k: string]: unknown }

            /**
             * Converts this SimpleMotor to JSON.
             * @returns JSON object
             */
            public toJSON(): { [k: string]: unknown }

            /**
             * Gets the default type url for SimpleMotor
             * @param [typeUrlPrefix] your custom typeUrlPrefix(default "type.googleapis.com")
             * @returns The default type url
             */
            public static getTypeUrl(typeUrlPrefix?: string): string
        }

        /** Properties of a DCMotor. */
        interface IDCMotor {
            /** Reference for purchase page or spec sheet */
            referenceUrl?: string | null

            /** m-Nm/Amp */
            torqueConstant?: number | null

            /** mV/rad/sec */
            emfConstant?: number | null

            /** Resistance of Motor - Optional if other values are known */
            resistance?: number | null

            /** measure in percentage of 100 - generally around 60 - measured under optimal load */
            maximumEffeciency?: number | null

            /** measured in Watts */
            maximumPower?: number | null

            /** Stated Duty Cycle of motor */
            dutyCycle?: mirabuf.motor.DutyCycles | null

            /** Optional data that can give a better relationship to the simulation */
            advanced?: mirabuf.motor.DCMotor.IAdvanced | null
        }

        /**
         * DCMotor Configuration
         * Parameters to simulate a DC Electric Motor
         * Still needs some more but overall they are most of the parameters we can use
         */
        class DCMotor implements IDCMotor {
            /**
             * Constructs a new DCMotor.
             * @param [properties] Properties to set
             */
            constructor(properties?: mirabuf.motor.IDCMotor)

            /** Reference for purchase page or spec sheet */
            public referenceUrl: string

            /** m-Nm/Amp */
            public torqueConstant: number

            /** mV/rad/sec */
            public emfConstant: number

            /** Resistance of Motor - Optional if other values are known */
            public resistance: number

            /** measure in percentage of 100 - generally around 60 - measured under optimal load */
            public maximumEffeciency: number

            /** measured in Watts */
            public maximumPower: number

            /** Stated Duty Cycle of motor */
            public dutyCycle: mirabuf.motor.DutyCycles

            /** Optional data that can give a better relationship to the simulation */
            public advanced?: mirabuf.motor.DCMotor.IAdvanced | null

            /**
             * Creates a new DCMotor instance using the specified properties.
             * @param [properties] Properties to set
             * @returns DCMotor instance
             */
            public static create(properties?: mirabuf.motor.IDCMotor): mirabuf.motor.DCMotor

            /**
             * Encodes the specified DCMotor message. Does not implicitly {@link mirabuf.motor.DCMotor.verify|verify} messages.
             * @param message DCMotor message or plain object to encode
             * @param [writer] Writer to encode to
             * @returns Writer
             */
            public static encode(message: mirabuf.motor.IDCMotor, writer?: $protobuf.Writer): $protobuf.Writer

            /**
             * Encodes the specified DCMotor message, length delimited. Does not implicitly {@link mirabuf.motor.DCMotor.verify|verify} messages.
             * @param message DCMotor message or plain object to encode
             * @param [writer] Writer to encode to
             * @returns Writer
             */
            public static encodeDelimited(message: mirabuf.motor.IDCMotor, writer?: $protobuf.Writer): $protobuf.Writer

            /**
             * Decodes a DCMotor message from the specified reader or buffer.
             * @param reader Reader or buffer to decode from
             * @param [length] Message length if known beforehand
             * @returns DCMotor
             * @throws {Error} If the payload is not a reader or valid buffer
             * @throws {$protobuf.util.ProtocolError} If required fields are missing
             */
            public static decode(reader: $protobuf.Reader | Uint8Array, length?: number): mirabuf.motor.DCMotor

            /**
             * Decodes a DCMotor message from the specified reader or buffer, length delimited.
             * @param reader Reader or buffer to decode from
             * @returns DCMotor
             * @throws {Error} If the payload is not a reader or valid buffer
             * @throws {$protobuf.util.ProtocolError} If required fields are missing
             */
            public static decodeDelimited(reader: $protobuf.Reader | Uint8Array): mirabuf.motor.DCMotor

            /**
             * Verifies a DCMotor message.
             * @param message Plain object to verify
             * @returns `null` if valid, otherwise the reason why it is not
             */
            public static verify(message: { [k: string]: unknown }): string | null

            /**
             * Creates a DCMotor message from a plain object. Also converts values to their respective internal types.
             * @param object Plain object
             * @returns DCMotor
             */
            public static fromObject(object: { [k: string]: unknown }): mirabuf.motor.DCMotor

            /**
             * Creates a plain object from a DCMotor message. Also converts values to other types if specified.
             * @param message DCMotor
             * @param [options] Conversion options
             * @returns Plain object
             */
            public static toObject(
                message: mirabuf.motor.DCMotor,
                options?: $protobuf.IConversionOptions
            ): { [k: string]: unknown }

            /**
             * Converts this DCMotor to JSON.
             * @returns JSON object
             */
            public toJSON(): { [k: string]: unknown }

            /**
             * Gets the default type url for DCMotor
             * @param [typeUrlPrefix] your custom typeUrlPrefix(default "type.googleapis.com")
             * @returns The default type url
             */
            public static getTypeUrl(typeUrlPrefix?: string): string
        }

        namespace DCMotor {
            /** Properties of an Advanced. */
            interface IAdvanced {
                /** measured in AMPs */
                freeCurrent?: number | null

                /** measured in RPM */
                freeSpeed?: number | null

                /** measure in AMPs */
                stallCurrent?: number | null

                /** measured in Nm */
                stallTorque?: number | null

                /** measured in Volts DC */
                inputVoltage?: number | null

                /** between (K * (N / 4)) and (K * ((N-2) / 4)) where N is number of poles - leave at 0 if unknown */
                resistanceVariation?: number | null
            }

            /** Information usually found on datasheet */
            class Advanced implements IAdvanced {
                /**
                 * Constructs a new Advanced.
                 * @param [properties] Properties to set
                 */
                constructor(properties?: mirabuf.motor.DCMotor.IAdvanced)

                /** measured in AMPs */
                public freeCurrent: number

                /** measured in RPM */
                public freeSpeed: number

                /** measure in AMPs */
                public stallCurrent: number

                /** measured in Nm */
                public stallTorque: number

                /** measured in Volts DC */
                public inputVoltage: number

                /** between (K * (N / 4)) and (K * ((N-2) / 4)) where N is number of poles - leave at 0 if unknown */
                public resistanceVariation: number

                /**
                 * Creates a new Advanced instance using the specified properties.
                 * @param [properties] Properties to set
                 * @returns Advanced instance
                 */
                public static create(properties?: mirabuf.motor.DCMotor.IAdvanced): mirabuf.motor.DCMotor.Advanced

                /**
                 * Encodes the specified Advanced message. Does not implicitly {@link mirabuf.motor.DCMotor.Advanced.verify|verify} messages.
                 * @param message Advanced message or plain object to encode
                 * @param [writer] Writer to encode to
                 * @returns Writer
                 */
                public static encode(
                    message: mirabuf.motor.DCMotor.IAdvanced,
                    writer?: $protobuf.Writer
                ): $protobuf.Writer

                /**
                 * Encodes the specified Advanced message, length delimited. Does not implicitly {@link mirabuf.motor.DCMotor.Advanced.verify|verify} messages.
                 * @param message Advanced message or plain object to encode
                 * @param [writer] Writer to encode to
                 * @returns Writer
                 */
                public static encodeDelimited(
                    message: mirabuf.motor.DCMotor.IAdvanced,
                    writer?: $protobuf.Writer
                ): $protobuf.Writer

                /**
                 * Decodes an Advanced message from the specified reader or buffer.
                 * @param reader Reader or buffer to decode from
                 * @param [length] Message length if known beforehand
                 * @returns Advanced
                 * @throws {Error} If the payload is not a reader or valid buffer
                 * @throws {$protobuf.util.ProtocolError} If required fields are missing
                 */
                public static decode(
                    reader: $protobuf.Reader | Uint8Array,
                    length?: number
                ): mirabuf.motor.DCMotor.Advanced

                /**
                 * Decodes an Advanced message from the specified reader or buffer, length delimited.
                 * @param reader Reader or buffer to decode from
                 * @returns Advanced
                 * @throws {Error} If the payload is not a reader or valid buffer
                 * @throws {$protobuf.util.ProtocolError} If required fields are missing
                 */
                public static decodeDelimited(reader: $protobuf.Reader | Uint8Array): mirabuf.motor.DCMotor.Advanced

                /**
                 * Verifies an Advanced message.
                 * @param message Plain object to verify
                 * @returns `null` if valid, otherwise the reason why it is not
                 */
                public static verify(message: { [k: string]: unknown }): string | null

                /**
                 * Creates an Advanced message from a plain object. Also converts values to their respective internal types.
                 * @param object Plain object
                 * @returns Advanced
                 */
                public static fromObject(object: { [k: string]: unknown }): mirabuf.motor.DCMotor.Advanced

                /**
                 * Creates a plain object from an Advanced message. Also converts values to other types if specified.
                 * @param message Advanced
                 * @param [options] Conversion options
                 * @returns Plain object
                 */
                public static toObject(
                    message: mirabuf.motor.DCMotor.Advanced,
                    options?: $protobuf.IConversionOptions
                ): { [k: string]: unknown }

                /**
                 * Converts this Advanced to JSON.
                 * @returns JSON object
                 */
                public toJSON(): { [k: string]: unknown }

                /**
                 * Gets the default type url for Advanced
                 * @param [typeUrlPrefix] your custom typeUrlPrefix(default "type.googleapis.com")
                 * @returns The default type url
                 */
                public static getTypeUrl(typeUrlPrefix?: string): string
            }
        }
    }

    /** Namespace material. */
    namespace material {
        /** Properties of a Materials. */
        interface IMaterials {
            /** Identifiable information (id, name, version) */
            info?: mirabuf.IInfo | null

            /** Map of Physical Materials */
            physicalMaterials?: {
                [k: string]: mirabuf.material.IPhysicalMaterial
            } | null

            /** Map of Appearances that are purely visual */
            appearances?: { [k: string]: mirabuf.material.IAppearance } | null
        }

        /**
         * Represents a File or Set of Materials with Appearances and Physical Data
         *
         * Can be Stored in AssemblyData
         */
        class Materials implements IMaterials {
            /**
             * Constructs a new Materials.
             * @param [properties] Properties to set
             */
            constructor(properties?: mirabuf.material.IMaterials)

            /** Identifiable information (id, name, version) */
            public info?: mirabuf.IInfo | null

            /** Map of Physical Materials */
            public physicalMaterials: {
                [k: string]: mirabuf.material.IPhysicalMaterial
            }

            /** Map of Appearances that are purely visual */
            public appearances: { [k: string]: mirabuf.material.IAppearance }

            /**
             * Creates a new Materials instance using the specified properties.
             * @param [properties] Properties to set
             * @returns Materials instance
             */
            public static create(properties?: mirabuf.material.IMaterials): mirabuf.material.Materials

            /**
             * Encodes the specified Materials message. Does not implicitly {@link mirabuf.material.Materials.verify|verify} messages.
             * @param message Materials message or plain object to encode
             * @param [writer] Writer to encode to
             * @returns Writer
             */
            public static encode(message: mirabuf.material.IMaterials, writer?: $protobuf.Writer): $protobuf.Writer

            /**
             * Encodes the specified Materials message, length delimited. Does not implicitly {@link mirabuf.material.Materials.verify|verify} messages.
             * @param message Materials message or plain object to encode
             * @param [writer] Writer to encode to
             * @returns Writer
             */
            public static encodeDelimited(
                message: mirabuf.material.IMaterials,
                writer?: $protobuf.Writer
            ): $protobuf.Writer

            /**
             * Decodes a Materials message from the specified reader or buffer.
             * @param reader Reader or buffer to decode from
             * @param [length] Message length if known beforehand
             * @returns Materials
             * @throws {Error} If the payload is not a reader or valid buffer
             * @throws {$protobuf.util.ProtocolError} If required fields are missing
             */
            public static decode(reader: $protobuf.Reader | Uint8Array, length?: number): mirabuf.material.Materials

            /**
             * Decodes a Materials message from the specified reader or buffer, length delimited.
             * @param reader Reader or buffer to decode from
             * @returns Materials
             * @throws {Error} If the payload is not a reader or valid buffer
             * @throws {$protobuf.util.ProtocolError} If required fields are missing
             */
            public static decodeDelimited(reader: $protobuf.Reader | Uint8Array): mirabuf.material.Materials

            /**
             * Verifies a Materials message.
             * @param message Plain object to verify
             * @returns `null` if valid, otherwise the reason why it is not
             */
            public static verify(message: { [k: string]: unknown }): string | null

            /**
             * Creates a Materials message from a plain object. Also converts values to their respective internal types.
             * @param object Plain object
             * @returns Materials
             */
            public static fromObject(object: { [k: string]: unknown }): mirabuf.material.Materials

            /**
             * Creates a plain object from a Materials message. Also converts values to other types if specified.
             * @param message Materials
             * @param [options] Conversion options
             * @returns Plain object
             */
            public static toObject(
                message: mirabuf.material.Materials,
                options?: $protobuf.IConversionOptions
            ): { [k: string]: unknown }

            /**
             * Converts this Materials to JSON.
             * @returns JSON object
             */
            public toJSON(): { [k: string]: unknown }

            /**
             * Gets the default type url for Materials
             * @param [typeUrlPrefix] your custom typeUrlPrefix(default "type.googleapis.com")
             * @returns The default type url
             */
            public static getTypeUrl(typeUrlPrefix?: string): string
        }

        /** Properties of an Appearance. */
        interface IAppearance {
            /** Identfiable information (id, name, version) */
            info?: mirabuf.IInfo | null

            /** albedo map RGBA 0-255 */
            albedo?: mirabuf.IColor | null

            /** roughness value 0-1 */
            roughness?: number | null

            /** metallic value 0-1 */
            metallic?: number | null

            /** specular value 0-1 */
            specular?: number | null
        }

        /**
         * Contains information on how a object looks
         * Limited to just color for now
         */
        class Appearance implements IAppearance {
            /**
             * Constructs a new Appearance.
             * @param [properties] Properties to set
             */
            constructor(properties?: mirabuf.material.IAppearance)

            /** Identfiable information (id, name, version) */
            public info?: mirabuf.IInfo | null

            /** albedo map RGBA 0-255 */
            public albedo?: mirabuf.IColor | null

            /** roughness value 0-1 */
            public roughness: number

            /** metallic value 0-1 */
            public metallic: number

            /** specular value 0-1 */
            public specular: number

            /**
             * Creates a new Appearance instance using the specified properties.
             * @param [properties] Properties to set
             * @returns Appearance instance
             */
            public static create(properties?: mirabuf.material.IAppearance): mirabuf.material.Appearance

            /**
             * Encodes the specified Appearance message. Does not implicitly {@link mirabuf.material.Appearance.verify|verify} messages.
             * @param message Appearance message or plain object to encode
             * @param [writer] Writer to encode to
             * @returns Writer
             */
            public static encode(message: mirabuf.material.IAppearance, writer?: $protobuf.Writer): $protobuf.Writer

            /**
             * Encodes the specified Appearance message, length delimited. Does not implicitly {@link mirabuf.material.Appearance.verify|verify} messages.
             * @param message Appearance message or plain object to encode
             * @param [writer] Writer to encode to
             * @returns Writer
             */
            public static encodeDelimited(
                message: mirabuf.material.IAppearance,
                writer?: $protobuf.Writer
            ): $protobuf.Writer

            /**
             * Decodes an Appearance message from the specified reader or buffer.
             * @param reader Reader or buffer to decode from
             * @param [length] Message length if known beforehand
             * @returns Appearance
             * @throws {Error} If the payload is not a reader or valid buffer
             * @throws {$protobuf.util.ProtocolError} If required fields are missing
             */
            public static decode(reader: $protobuf.Reader | Uint8Array, length?: number): mirabuf.material.Appearance

            /**
             * Decodes an Appearance message from the specified reader or buffer, length delimited.
             * @param reader Reader or buffer to decode from
             * @returns Appearance
             * @throws {Error} If the payload is not a reader or valid buffer
             * @throws {$protobuf.util.ProtocolError} If required fields are missing
             */
            public static decodeDelimited(reader: $protobuf.Reader | Uint8Array): mirabuf.material.Appearance

            /**
             * Verifies an Appearance message.
             * @param message Plain object to verify
             * @returns `null` if valid, otherwise the reason why it is not
             */
            public static verify(message: { [k: string]: unknown }): string | null

            /**
             * Creates an Appearance message from a plain object. Also converts values to their respective internal types.
             * @param object Plain object
             * @returns Appearance
             */
            public static fromObject(object: { [k: string]: unknown }): mirabuf.material.Appearance

            /**
             * Creates a plain object from an Appearance message. Also converts values to other types if specified.
             * @param message Appearance
             * @param [options] Conversion options
             * @returns Plain object
             */
            public static toObject(
                message: mirabuf.material.Appearance,
                options?: $protobuf.IConversionOptions
            ): { [k: string]: unknown }

            /**
             * Converts this Appearance to JSON.
             * @returns JSON object
             */
            public toJSON(): { [k: string]: unknown }

            /**
             * Gets the default type url for Appearance
             * @param [typeUrlPrefix] your custom typeUrlPrefix(default "type.googleapis.com")
             * @returns The default type url
             */
            public static getTypeUrl(typeUrlPrefix?: string): string
        }

        /** Properties of a PhysicalMaterial. */
        interface IPhysicalMaterial {
            /** Identifiable information (id, name, version, etc) */
            info?: mirabuf.IInfo | null

            /** short description of physical material */
            description?: string | null

            /** Thermal Physical properties of the model OPTIONAL */
            thermal?: mirabuf.material.PhysicalMaterial.IThermal | null

            /** Mechanical properties of the model OPTIONAL */
            mechanical?: mirabuf.material.PhysicalMaterial.IMechanical | null

            /** Physical Strength properties of the model OPTIONAL */
            strength?: mirabuf.material.PhysicalMaterial.IStrength | null

            /** Frictional force for dampening - Interpolate (0-1) */
            dynamicFriction?: number | null

            /** Frictional force override at stop - Interpolate (0-1) */
            staticFriction?: number | null

            /** Restitution of the object - Interpolate (0-1) */
            restitution?: number | null

            /** should this object deform when encountering large forces - TODO: This needs a proper message and equation field */
            deformable?: boolean | null

            /** generic type to assign some default params */
            matType?: mirabuf.material.PhysicalMaterial.MaterialType | null
        }

        /** Data to represent unknown given Physical Material */
        class PhysicalMaterial implements IPhysicalMaterial {
            /**
             * Constructs a new PhysicalMaterial.
             * @param [properties] Properties to set
             */
            constructor(properties?: mirabuf.material.IPhysicalMaterial)

            /** Identifiable information (id, name, version, etc) */
            public info?: mirabuf.IInfo | null

            /** short description of physical material */
            public description: string

            /** Thermal Physical properties of the model OPTIONAL */
            public thermal?: mirabuf.material.PhysicalMaterial.IThermal | null

            /** Mechanical properties of the model OPTIONAL */
            public mechanical?: mirabuf.material.PhysicalMaterial.IMechanical | null

            /** Physical Strength properties of the model OPTIONAL */
            public strength?: mirabuf.material.PhysicalMaterial.IStrength | null

            /** Frictional force for dampening - Interpolate (0-1) */
            public dynamicFriction: number

            /** Frictional force override at stop - Interpolate (0-1) */
            public staticFriction: number

            /** Restitution of the object - Interpolate (0-1) */
            public restitution: number

            /** should this object deform when encountering large forces - TODO: This needs a proper message and equation field */
            public deformable: boolean

            /** generic type to assign some default params */
            public matType: mirabuf.material.PhysicalMaterial.MaterialType

            /**
             * Creates a new PhysicalMaterial instance using the specified properties.
             * @param [properties] Properties to set
             * @returns PhysicalMaterial instance
             */
            public static create(properties?: mirabuf.material.IPhysicalMaterial): mirabuf.material.PhysicalMaterial

            /**
             * Encodes the specified PhysicalMaterial message. Does not implicitly {@link mirabuf.material.PhysicalMaterial.verify|verify} messages.
             * @param message PhysicalMaterial message or plain object to encode
             * @param [writer] Writer to encode to
             * @returns Writer
             */
            public static encode(
                message: mirabuf.material.IPhysicalMaterial,
                writer?: $protobuf.Writer
            ): $protobuf.Writer

            /**
             * Encodes the specified PhysicalMaterial message, length delimited. Does not implicitly {@link mirabuf.material.PhysicalMaterial.verify|verify} messages.
             * @param message PhysicalMaterial message or plain object to encode
             * @param [writer] Writer to encode to
             * @returns Writer
             */
            public static encodeDelimited(
                message: mirabuf.material.IPhysicalMaterial,
                writer?: $protobuf.Writer
            ): $protobuf.Writer

            /**
             * Decodes a PhysicalMaterial message from the specified reader or buffer.
             * @param reader Reader or buffer to decode from
             * @param [length] Message length if known beforehand
             * @returns PhysicalMaterial
             * @throws {Error} If the payload is not a reader or valid buffer
             * @throws {$protobuf.util.ProtocolError} If required fields are missing
             */
            public static decode(
                reader: $protobuf.Reader | Uint8Array,
                length?: number
            ): mirabuf.material.PhysicalMaterial

            /**
             * Decodes a PhysicalMaterial message from the specified reader or buffer, length delimited.
             * @param reader Reader or buffer to decode from
             * @returns PhysicalMaterial
             * @throws {Error} If the payload is not a reader or valid buffer
             * @throws {$protobuf.util.ProtocolError} If required fields are missing
             */
            public static decodeDelimited(reader: $protobuf.Reader | Uint8Array): mirabuf.material.PhysicalMaterial

            /**
             * Verifies a PhysicalMaterial message.
             * @param message Plain object to verify
             * @returns `null` if valid, otherwise the reason why it is not
             */
            public static verify(message: { [k: string]: unknown }): string | null

            /**
             * Creates a PhysicalMaterial message from a plain object. Also converts values to their respective internal types.
             * @param object Plain object
             * @returns PhysicalMaterial
             */
            public static fromObject(object: { [k: string]: unknown }): mirabuf.material.PhysicalMaterial

            /**
             * Creates a plain object from a PhysicalMaterial message. Also converts values to other types if specified.
             * @param message PhysicalMaterial
             * @param [options] Conversion options
             * @returns Plain object
             */
            public static toObject(
                message: mirabuf.material.PhysicalMaterial,
                options?: $protobuf.IConversionOptions
            ): { [k: string]: unknown }

            /**
             * Converts this PhysicalMaterial to JSON.
             * @returns JSON object
             */
            public toJSON(): { [k: string]: unknown }

            /**
             * Gets the default type url for PhysicalMaterial
             * @param [typeUrlPrefix] your custom typeUrlPrefix(default "type.googleapis.com")
             * @returns The default type url
             */
            public static getTypeUrl(typeUrlPrefix?: string): string
        }

        namespace PhysicalMaterial {
            /** MaterialType enum. */
            enum MaterialType {
                METAL = 0,
                PLASTIC = 1,
            }

            /** Properties of a Thermal. */
            interface IThermal {
                /** W/(m*K) */
                thermalConductivity?: number | null

                /** J/(g*C) */
                specificHeat?: number | null

                /** um/(m*C) */
                thermalExpansionCoefficient?: number | null
            }

            /** Thermal Properties Set Definition for Simulation. */
            class Thermal implements IThermal {
                /**
                 * Constructs a new Thermal.
                 * @param [properties] Properties to set
                 */
                constructor(properties?: mirabuf.material.PhysicalMaterial.IThermal)

                /** W/(m*K) */
                public thermalConductivity: number

                /** J/(g*C) */
                public specificHeat: number

                /** um/(m*C) */
                public thermalExpansionCoefficient: number

                /**
                 * Creates a new Thermal instance using the specified properties.
                 * @param [properties] Properties to set
                 * @returns Thermal instance
                 */
                public static create(
                    properties?: mirabuf.material.PhysicalMaterial.IThermal
                ): mirabuf.material.PhysicalMaterial.Thermal

                /**
                 * Encodes the specified Thermal message. Does not implicitly {@link mirabuf.material.PhysicalMaterial.Thermal.verify|verify} messages.
                 * @param message Thermal message or plain object to encode
                 * @param [writer] Writer to encode to
                 * @returns Writer
                 */
                public static encode(
                    message: mirabuf.material.PhysicalMaterial.IThermal,
                    writer?: $protobuf.Writer
                ): $protobuf.Writer

                /**
                 * Encodes the specified Thermal message, length delimited. Does not implicitly {@link mirabuf.material.PhysicalMaterial.Thermal.verify|verify} messages.
                 * @param message Thermal message or plain object to encode
                 * @param [writer] Writer to encode to
                 * @returns Writer
                 */
                public static encodeDelimited(
                    message: mirabuf.material.PhysicalMaterial.IThermal,
                    writer?: $protobuf.Writer
                ): $protobuf.Writer

                /**
                 * Decodes a Thermal message from the specified reader or buffer.
                 * @param reader Reader or buffer to decode from
                 * @param [length] Message length if known beforehand
                 * @returns Thermal
                 * @throws {Error} If the payload is not a reader or valid buffer
                 * @throws {$protobuf.util.ProtocolError} If required fields are missing
                 */
                public static decode(
                    reader: $protobuf.Reader | Uint8Array,
                    length?: number
                ): mirabuf.material.PhysicalMaterial.Thermal

                /**
                 * Decodes a Thermal message from the specified reader or buffer, length delimited.
                 * @param reader Reader or buffer to decode from
                 * @returns Thermal
                 * @throws {Error} If the payload is not a reader or valid buffer
                 * @throws {$protobuf.util.ProtocolError} If required fields are missing
                 */
                public static decodeDelimited(
                    reader: $protobuf.Reader | Uint8Array
                ): mirabuf.material.PhysicalMaterial.Thermal

                /**
                 * Verifies a Thermal message.
                 * @param message Plain object to verify
                 * @returns `null` if valid, otherwise the reason why it is not
                 */
                public static verify(message: { [k: string]: unknown }): string | null

                /**
                 * Creates a Thermal message from a plain object. Also converts values to their respective internal types.
                 * @param object Plain object
                 * @returns Thermal
                 */
                public static fromObject(object: { [k: string]: unknown }): mirabuf.material.PhysicalMaterial.Thermal

                /**
                 * Creates a plain object from a Thermal message. Also converts values to other types if specified.
                 * @param message Thermal
                 * @param [options] Conversion options
                 * @returns Plain object
                 */
                public static toObject(
                    message: mirabuf.material.PhysicalMaterial.Thermal,
                    options?: $protobuf.IConversionOptions
                ): { [k: string]: unknown }

                /**
                 * Converts this Thermal to JSON.
                 * @returns JSON object
                 */
                public toJSON(): { [k: string]: unknown }

                /**
                 * Gets the default type url for Thermal
                 * @param [typeUrlPrefix] your custom typeUrlPrefix(default "type.googleapis.com")
                 * @returns The default type url
                 */
                public static getTypeUrl(typeUrlPrefix?: string): string
            }

            /** Properties of a Mechanical. */
            interface IMechanical {
                /** GPa */
                youngMod?: number | null

                /** ? */
                poissonRatio?: number | null

                /** MPa */
                shearMod?: number | null

                /** g/cm^3 */
                density?: number | null

                /** ? */
                dampingCoefficient?: number | null
            }

            /** Mechanical Properties Set Definition for Simulation. */
            class Mechanical implements IMechanical {
                /**
                 * Constructs a new Mechanical.
                 * @param [properties] Properties to set
                 */
                constructor(properties?: mirabuf.material.PhysicalMaterial.IMechanical)

                /** GPa */
                public youngMod: number

                /** ? */
                public poissonRatio: number

                /** MPa */
                public shearMod: number

                /** g/cm^3 */
                public density: number

                /** ? */
                public dampingCoefficient: number

                /**
                 * Creates a new Mechanical instance using the specified properties.
                 * @param [properties] Properties to set
                 * @returns Mechanical instance
                 */
                public static create(
                    properties?: mirabuf.material.PhysicalMaterial.IMechanical
                ): mirabuf.material.PhysicalMaterial.Mechanical

                /**
                 * Encodes the specified Mechanical message. Does not implicitly {@link mirabuf.material.PhysicalMaterial.Mechanical.verify|verify} messages.
                 * @param message Mechanical message or plain object to encode
                 * @param [writer] Writer to encode to
                 * @returns Writer
                 */
                public static encode(
                    message: mirabuf.material.PhysicalMaterial.IMechanical,
                    writer?: $protobuf.Writer
                ): $protobuf.Writer

                /**
                 * Encodes the specified Mechanical message, length delimited. Does not implicitly {@link mirabuf.material.PhysicalMaterial.Mechanical.verify|verify} messages.
                 * @param message Mechanical message or plain object to encode
                 * @param [writer] Writer to encode to
                 * @returns Writer
                 */
                public static encodeDelimited(
                    message: mirabuf.material.PhysicalMaterial.IMechanical,
                    writer?: $protobuf.Writer
                ): $protobuf.Writer

                /**
                 * Decodes a Mechanical message from the specified reader or buffer.
                 * @param reader Reader or buffer to decode from
                 * @param [length] Message length if known beforehand
                 * @returns Mechanical
                 * @throws {Error} If the payload is not a reader or valid buffer
                 * @throws {$protobuf.util.ProtocolError} If required fields are missing
                 */
                public static decode(
                    reader: $protobuf.Reader | Uint8Array,
                    length?: number
                ): mirabuf.material.PhysicalMaterial.Mechanical

                /**
                 * Decodes a Mechanical message from the specified reader or buffer, length delimited.
                 * @param reader Reader or buffer to decode from
                 * @returns Mechanical
                 * @throws {Error} If the payload is not a reader or valid buffer
                 * @throws {$protobuf.util.ProtocolError} If required fields are missing
                 */
                public static decodeDelimited(
                    reader: $protobuf.Reader | Uint8Array
                ): mirabuf.material.PhysicalMaterial.Mechanical

                /**
                 * Verifies a Mechanical message.
                 * @param message Plain object to verify
                 * @returns `null` if valid, otherwise the reason why it is not
                 */
                public static verify(message: { [k: string]: unknown }): string | null

                /**
                 * Creates a Mechanical message from a plain object. Also converts values to their respective internal types.
                 * @param object Plain object
                 * @returns Mechanical
                 */
                public static fromObject(object: { [k: string]: unknown }): mirabuf.material.PhysicalMaterial.Mechanical

                /**
                 * Creates a plain object from a Mechanical message. Also converts values to other types if specified.
                 * @param message Mechanical
                 * @param [options] Conversion options
                 * @returns Plain object
                 */
                public static toObject(
                    message: mirabuf.material.PhysicalMaterial.Mechanical,
                    options?: $protobuf.IConversionOptions
                ): { [k: string]: unknown }

                /**
                 * Converts this Mechanical to JSON.
                 * @returns JSON object
                 */
                public toJSON(): { [k: string]: unknown }

                /**
                 * Gets the default type url for Mechanical
                 * @param [typeUrlPrefix] your custom typeUrlPrefix(default "type.googleapis.com")
                 * @returns The default type url
                 */
                public static getTypeUrl(typeUrlPrefix?: string): string
            }

            /** Properties of a Strength. */
            interface IStrength {
                /** MPa */
                yieldStrength?: number | null

                /** MPa */
                tensileStrength?: number | null

                /** yes / no */
                thermalTreatment?: boolean | null
            }

            /** Strength Properties Set Definition for Simulation. */
            class Strength implements IStrength {
                /**
                 * Constructs a new Strength.
                 * @param [properties] Properties to set
                 */
                constructor(properties?: mirabuf.material.PhysicalMaterial.IStrength)

                /** MPa */
                public yieldStrength: number

                /** MPa */
                public tensileStrength: number

                /** yes / no */
                public thermalTreatment: boolean

                /**
                 * Creates a new Strength instance using the specified properties.
                 * @param [properties] Properties to set
                 * @returns Strength instance
                 */
                public static create(
                    properties?: mirabuf.material.PhysicalMaterial.IStrength
                ): mirabuf.material.PhysicalMaterial.Strength

                /**
                 * Encodes the specified Strength message. Does not implicitly {@link mirabuf.material.PhysicalMaterial.Strength.verify|verify} messages.
                 * @param message Strength message or plain object to encode
                 * @param [writer] Writer to encode to
                 * @returns Writer
                 */
                public static encode(
                    message: mirabuf.material.PhysicalMaterial.IStrength,
                    writer?: $protobuf.Writer
                ): $protobuf.Writer

                /**
                 * Encodes the specified Strength message, length delimited. Does not implicitly {@link mirabuf.material.PhysicalMaterial.Strength.verify|verify} messages.
                 * @param message Strength message or plain object to encode
                 * @param [writer] Writer to encode to
                 * @returns Writer
                 */
                public static encodeDelimited(
                    message: mirabuf.material.PhysicalMaterial.IStrength,
                    writer?: $protobuf.Writer
                ): $protobuf.Writer

                /**
                 * Decodes a Strength message from the specified reader or buffer.
                 * @param reader Reader or buffer to decode from
                 * @param [length] Message length if known beforehand
                 * @returns Strength
                 * @throws {Error} If the payload is not a reader or valid buffer
                 * @throws {$protobuf.util.ProtocolError} If required fields are missing
                 */
                public static decode(
                    reader: $protobuf.Reader | Uint8Array,
                    length?: number
                ): mirabuf.material.PhysicalMaterial.Strength

                /**
                 * Decodes a Strength message from the specified reader or buffer, length delimited.
                 * @param reader Reader or buffer to decode from
                 * @returns Strength
                 * @throws {Error} If the payload is not a reader or valid buffer
                 * @throws {$protobuf.util.ProtocolError} If required fields are missing
                 */
                public static decodeDelimited(
                    reader: $protobuf.Reader | Uint8Array
                ): mirabuf.material.PhysicalMaterial.Strength

                /**
                 * Verifies a Strength message.
                 * @param message Plain object to verify
                 * @returns `null` if valid, otherwise the reason why it is not
                 */
                public static verify(message: { [k: string]: unknown }): string | null

                /**
                 * Creates a Strength message from a plain object. Also converts values to their respective internal types.
                 * @param object Plain object
                 * @returns Strength
                 */
                public static fromObject(object: { [k: string]: unknown }): mirabuf.material.PhysicalMaterial.Strength

                /**
                 * Creates a plain object from a Strength message. Also converts values to other types if specified.
                 * @param message Strength
                 * @param [options] Conversion options
                 * @returns Plain object
                 */
                public static toObject(
                    message: mirabuf.material.PhysicalMaterial.Strength,
                    options?: $protobuf.IConversionOptions
                ): { [k: string]: unknown }

                /**
                 * Converts this Strength to JSON.
                 * @returns JSON object
                 */
                public toJSON(): { [k: string]: unknown }

                /**
                 * Gets the default type url for Strength
                 * @param [typeUrlPrefix] your custom typeUrlPrefix(default "type.googleapis.com")
                 * @returns The default type url
                 */
                public static getTypeUrl(typeUrlPrefix?: string): string
            }
        }
    }

    /** Namespace signal. */
    namespace signal {
        /** Properties of a Signals. */
        interface ISignals {
            /** Has identifiable data (id, name, version) */
            info?: mirabuf.IInfo | null

            /** Contains a full collection of symbols */
            signalMap?: { [k: string]: mirabuf.signal.ISignal } | null
        }

        /** Signals is a container for all of the potential signals. */
        class Signals implements ISignals {
            /**
             * Constructs a new Signals.
             * @param [properties] Properties to set
             */
            constructor(properties?: mirabuf.signal.ISignals)

            /** Has identifiable data (id, name, version) */
            public info?: mirabuf.IInfo | null

            /** Contains a full collection of symbols */
            public signalMap: { [k: string]: mirabuf.signal.ISignal }

            /**
             * Creates a new Signals instance using the specified properties.
             * @param [properties] Properties to set
             * @returns Signals instance
             */
            public static create(properties?: mirabuf.signal.ISignals): mirabuf.signal.Signals

            /**
             * Encodes the specified Signals message. Does not implicitly {@link mirabuf.signal.Signals.verify|verify} messages.
             * @param message Signals message or plain object to encode
             * @param [writer] Writer to encode to
             * @returns Writer
             */
            public static encode(message: mirabuf.signal.ISignals, writer?: $protobuf.Writer): $protobuf.Writer

            /**
             * Encodes the specified Signals message, length delimited. Does not implicitly {@link mirabuf.signal.Signals.verify|verify} messages.
             * @param message Signals message or plain object to encode
             * @param [writer] Writer to encode to
             * @returns Writer
             */
            public static encodeDelimited(message: mirabuf.signal.ISignals, writer?: $protobuf.Writer): $protobuf.Writer

            /**
             * Decodes a Signals message from the specified reader or buffer.
             * @param reader Reader or buffer to decode from
             * @param [length] Message length if known beforehand
             * @returns Signals
             * @throws {Error} If the payload is not a reader or valid buffer
             * @throws {$protobuf.util.ProtocolError} If required fields are missing
             */
            public static decode(reader: $protobuf.Reader | Uint8Array, length?: number): mirabuf.signal.Signals

            /**
             * Decodes a Signals message from the specified reader or buffer, length delimited.
             * @param reader Reader or buffer to decode from
             * @returns Signals
             * @throws {Error} If the payload is not a reader or valid buffer
             * @throws {$protobuf.util.ProtocolError} If required fields are missing
             */
            public static decodeDelimited(reader: $protobuf.Reader | Uint8Array): mirabuf.signal.Signals

            /**
             * Verifies a Signals message.
             * @param message Plain object to verify
             * @returns `null` if valid, otherwise the reason why it is not
             */
            public static verify(message: { [k: string]: unknown }): string | null

            /**
             * Creates a Signals message from a plain object. Also converts values to their respective internal types.
             * @param object Plain object
             * @returns Signals
             */
            public static fromObject(object: { [k: string]: unknown }): mirabuf.signal.Signals

            /**
             * Creates a plain object from a Signals message. Also converts values to other types if specified.
             * @param message Signals
             * @param [options] Conversion options
             * @returns Plain object
             */
            public static toObject(
                message: mirabuf.signal.Signals,
                options?: $protobuf.IConversionOptions
            ): { [k: string]: unknown }

            /**
             * Converts this Signals to JSON.
             * @returns JSON object
             */
            public toJSON(): { [k: string]: unknown }

            /**
             * Gets the default type url for Signals
             * @param [typeUrlPrefix] your custom typeUrlPrefix(default "type.googleapis.com")
             * @returns The default type url
             */
            public static getTypeUrl(typeUrlPrefix?: string): string
        }

        /** IOType is a way to specify Input or Output. */
        enum IOType {
            INPUT = 0,
            OUTPUT = 1,
        }

        /**
         * DeviceType needs to be a type of device that has a supported connection
         * As well as a signal frmae but that can come later
         */
        enum DeviceType {
            PWM = 0,
            Digital = 1,
            Analog = 2,
            I2C = 3,
            CANBUS = 4,
            CUSTOM = 5,
        }

        /** Properties of a Signal. */
        interface ISignal {
            /** Has identifiable data (id, name, version) */
            info?: mirabuf.IInfo | null

            /** Is this a Input or Output */
            io?: mirabuf.signal.IOType | null

            /** The name of a custom input type that is not listed as a device type */
            customType?: string | null

            /** ID for a given signal that exists... PWM 2, CANBUS 4 */
            signalId?: number | null

            /** Enum for device type that should always be set */
            deviceType?: mirabuf.signal.DeviceType | null
        }

        /**
         * Signal is a way to define a controlling signal.
         *
         * TODO: Add Origin
         * TODO: Decide how this is linked to a exported object
         */
        class Signal implements ISignal {
            /**
             * Constructs a new Signal.
             * @param [properties] Properties to set
             */
            constructor(properties?: mirabuf.signal.ISignal)

            /** Has identifiable data (id, name, version) */
            public info?: mirabuf.IInfo | null

            /** Is this a Input or Output */
            public io: mirabuf.signal.IOType

            /** The name of a custom input type that is not listed as a device type */
            public customType: string

            /** ID for a given signal that exists... PWM 2, CANBUS 4 */
            public signalId: number

            /** Enum for device type that should always be set */
            public deviceType: mirabuf.signal.DeviceType

            /**
             * Creates a new Signal instance using the specified properties.
             * @param [properties] Properties to set
             * @returns Signal instance
             */
            public static create(properties?: mirabuf.signal.ISignal): mirabuf.signal.Signal

            /**
             * Encodes the specified Signal message. Does not implicitly {@link mirabuf.signal.Signal.verify|verify} messages.
             * @param message Signal message or plain object to encode
             * @param [writer] Writer to encode to
             * @returns Writer
             */
            public static encode(message: mirabuf.signal.ISignal, writer?: $protobuf.Writer): $protobuf.Writer

            /**
             * Encodes the specified Signal message, length delimited. Does not implicitly {@link mirabuf.signal.Signal.verify|verify} messages.
             * @param message Signal message or plain object to encode
             * @param [writer] Writer to encode to
             * @returns Writer
             */
            public static encodeDelimited(message: mirabuf.signal.ISignal, writer?: $protobuf.Writer): $protobuf.Writer

            /**
             * Decodes a Signal message from the specified reader or buffer.
             * @param reader Reader or buffer to decode from
             * @param [length] Message length if known beforehand
             * @returns Signal
             * @throws {Error} If the payload is not a reader or valid buffer
             * @throws {$protobuf.util.ProtocolError} If required fields are missing
             */
            public static decode(reader: $protobuf.Reader | Uint8Array, length?: number): mirabuf.signal.Signal

            /**
             * Decodes a Signal message from the specified reader or buffer, length delimited.
             * @param reader Reader or buffer to decode from
             * @returns Signal
             * @throws {Error} If the payload is not a reader or valid buffer
             * @throws {$protobuf.util.ProtocolError} If required fields are missing
             */
            public static decodeDelimited(reader: $protobuf.Reader | Uint8Array): mirabuf.signal.Signal

            /**
             * Verifies a Signal message.
             * @param message Plain object to verify
             * @returns `null` if valid, otherwise the reason why it is not
             */
            public static verify(message: { [k: string]: unknown }): string | null

            /**
             * Creates a Signal message from a plain object. Also converts values to their respective internal types.
             * @param object Plain object
             * @returns Signal
             */
            public static fromObject(object: { [k: string]: unknown }): mirabuf.signal.Signal

            /**
             * Creates a plain object from a Signal message. Also converts values to other types if specified.
             * @param message Signal
             * @param [options] Conversion options
             * @returns Plain object
             */
            public static toObject(
                message: mirabuf.signal.Signal,
                options?: $protobuf.IConversionOptions
            ): { [k: string]: unknown }

            /**
             * Converts this Signal to JSON.
             * @returns JSON object
             */
            public toJSON(): { [k: string]: unknown }

            /**
             * Gets the default type url for Signal
             * @param [typeUrlPrefix] your custom typeUrlPrefix(default "type.googleapis.com")
             * @returns The default type url
             */
            public static getTypeUrl(typeUrlPrefix?: string): string
        }
    }
}
