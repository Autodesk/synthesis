import clr
clr.AddReference('System')
clr.AddReference('System.Core')
clr.AddReference('System.Reflection')
inventorAss = clr.AddReference('Autodesk.Inventor.Interop')
import System
from System import *
from System.Reflection import *

import Inventor
clr.ImportExtensions(System.Linq)


class ClassGenerator:
    def __init__(self, using_statements, type_from_assembly, target_types, destination_namespace, wrapper_abbreviation, destination_folder):
        self.using_statements = using_statements
        self.type_from_assembly = type_from_assembly
        self.target_types = target_types
        self.assembly = Assembly.GetAssembly(self.type_from_assembly)
        self.assembly_namespace = self.assembly.GetType(self.target_types[0]).Namespace #janky!
        self.destination_namespace = destination_namespace
        self.wrapper_abbreviation = wrapper_abbreviation
        self.destination_folder = destination_folder
        self.wrapper_classes = self.target_types.Select(lambda c: ClassToWrap(self.assembly, c, self.wrapper_abbreviation)).ToList()
        
        self.generate_classes()

    def generate_classes(self):
        for wrapper in self.wrapper_classes:
            if wrapper.target_type != None:
                file_path = self.destination_folder + wrapper.file_name

                with open(file_path, 'w') as class_file:
                    if self.assembly.GetType(self.target_types[0]).IsEnum:
                        self.write_enums(class_file, wrapper)
                    else:
                        self.write_classes(class_file, wrapper)
                
    def format_argument_name(self, argument_name):
        formatted_name  = lambda a: a[:1].lower() + a[1:] if a else ''
        return formatted_name(argument_name)

    def get_arguments_string(self, method_arguments):
        return '(' + ', '.join((self.get_type_aliases(method_argument[0], ref_or_out = method_argument[2]) + 
                                ' ' + self.format_argument_name(method_argument[1])) 
                               for method_argument in method_arguments) + ')\n'

    def get_method_string(self, method_arguments):
        return '(' + ', '.join((self.get_type_aliases(method_argument[0], None, True, ref_or_out = method_argument[2]) + ' ' + self.format_argument_name(method_argument[1])) for method_argument in method_arguments) + ');\n'

    def get_read_only_property_text(self, access_modifier, method_info):
        if access_modifier == 'internal':
            property_text = (self.tab(2) + 
                             access_modifier + ' ' + 
                             self.get_type_aliases(method_info.return_type.Name, access_modifier) + ' ' + 
                             'Internal' + method_info.c_sharp_name + ' ' + '{ get; }\n')
        else:
            property_text = (self.tab(2) + 
                             access_modifier + ' ' + 
                             self.get_type_aliases(method_info.return_type.Name) + ' ' + 
                             method_info.c_sharp_name + ' ' + '{ get; }\n')
        return property_text   

    def get_read_write_property_text(self, access_modifier, method_info):
        if access_modifier == 'internal':
            property_text = (self.tab(2) + 
                             access_modifier + ' ' + 
                             self.get_type_aliases(method_info.return_type.Name, access_modifier) + ' ' + 
                             'Internal' + method_info.c_sharp_name + ' ' + '{ get; set; }\n')
        else:
            property_text = (self.tab(2) + 
                             access_modifier + ' ' + 
                             self.get_type_aliases(method_info.return_type.Name) + ' ' + 
                             method_info.c_sharp_name + ' ' + '{ get; set; }\n')
        return property_text

    def get_type_aliases(self, possible_system_type, access_modifier = None, method_body = False, ref_or_out = ''):
        built_in_alias_table = [('Boolean', 'bool'),
                                ('Byte', 'byte'),
                                ('Char', 'char'),
                                ('Decimal', 'decimal'),
                                ('Double', 'double'),
                                ('Int16', 'short'),
                                ('Int32', 'int'),
                                ('Int64', 'long'),
                                #('Object', 'object'), #need to fix this ie object[] vs ObjectVisibility
                                ('SByte', 'sbyte'), 
                                ('Single', 'float'),
                                ('String', 'string'),
                                ('UInt16', 'ushort'),
                                ('UInt32', 'uint'),
                                ('UInt64', 'ulong'),
                                ('Void', 'void')]

        #it may be an out system type parameter
        if method_body == False:
            if possible_system_type[-1] == '&':
                if built_in_alias_table.Any(lambda t: possible_system_type.Contains(t[0])):
                    return possible_system_type.Replace(built_in_alias_table.First(lambda t: possible_system_type.Contains(t[0]))[0], 
                                                        ref_or_out + built_in_alias_table.First(lambda t: possible_system_type.Contains(t[0]))[1])[:-1]
                else:
                    return ref_or_out + possible_system_type[:-1]
            #otherwise this is an out parameter from some other namespace without alias
            elif built_in_alias_table.Any(lambda t: possible_system_type.Contains(t[0])):
                return possible_system_type.Replace(built_in_alias_table.First(lambda t: possible_system_type.Contains(t[0]))[0], 
                                                    built_in_alias_table.First(lambda t: possible_system_type.Contains(t[0]))[1])
            else:
                return possible_system_type

        #it is in a method body, we just want to add 'ref' or 'out' if needed,
        #otherwise just return empty string
        elif method_body == True:
            if possible_system_type[-1] == '&':
                return ref_or_out
            else:
                return ''
        
    def get_wrapper_name(self, type_name):
        return self.wrapper_abbreviation + type_name

    def tab(self, quantity):
        return ' '*4*quantity

    def write_class_declaration(self, class_file, wrapper):
        class_file.write(self.tab(1) + '[RegisterForTrace]\n')
        class_file.write(self.tab(1) + 'public class ' + wrapper.name + '\n')
        class_file.write(self.tab(1) + '{\n')

    def write_classes(self, class_file, wrapper):
        self.write_using_directives(class_file)        
        self.write_namespace(class_file)     
        self.write_class_declaration(class_file, wrapper) #what about inheritance        
        self.write_internal_properties(class_file, wrapper)        
        self.write_private_constructors(class_file, wrapper)        
        self.write_private_methods(class_file, wrapper)        
        self.write_public_properties(class_file, wrapper)        
        self.write_public_static_constructors(class_file, wrapper)       
        self.write_public_methods(class_file, wrapper)        
        self.write_end_of_class(class_file)

    def write_end_of_class(self, class_file):
        class_file.write(self.tab(1) + '}\n') 
        class_file.write('}\n')

    def write_enum_declaration(self, class_file, wrapper):
        class_file.write(self.tab(1) + '[RegisterForTrace]\n')
        class_file.write(self.tab(1) + 'public enum ' + wrapper.name + '\n')
        class_file.write(self.tab(1) + '{\n')

    def write_enum_constants(self, class_file, enum_names, wrapper):
        enum_names = list(enum_names)
        enum_names.sort()
        class_file.write(self.tab(2) + '#region Enums\n')
        for i in range(len(enum_names)-1):
            class_file.write(self.tab(2) + enum_names[i] + ' = ' + self.assembly.GetTypes()[0].ToString().split('.')[0] + '.' + wrapper.target_name + '.' + enum_names[i] + ',\n')
        class_file.write(self.tab(2) + enum_names[-1] + ' = ' + self.assembly.GetTypes()[0].ToString().split('.')[0] + '.' + wrapper.target_name + '.' + enum_names[-1] + '\n')       
        class_file.write(self.tab(2) + '#endregion\n')
        return enum_names

    def write_enums(self, class_file, wrapper):
        enum_type = self.assembly.GetType(self.target_types[0])
        enum_names = self.assembly.GetType(self.target_types[0]).GetEnumNames()
        enum_type = self.assembly.GetType(self.target_types[0]).GetEnumUnderlyingType()
        
        self.write_using_directives(class_file)       
        self.write_namespace(class_file)       
        self.write_enum_declaration(class_file, wrapper)        
        enum_names = self.write_enum_constants(class_file, enum_names, wrapper)        
        self.write_end_of_class(class_file)

    def write_internal_properties(self, class_file, wrapper):
        class_file.write(self.tab(2) + '#region Internal properties\n')
        #create internal property to hold the instance being wrapped
        class_file.write(self.tab(2) + 'internal ' + self.assembly.GetTypes()[0].ToString().split('.')[0] + '.' + 
                         wrapper.target_name + ' ' + 'Internal' + wrapper.target_name + ' ' + 
                         '{ get; set; }\n')
        class_file.write('\n')
        #create the read only properties
        access_modifier = 'internal'
        for read_only_property in wrapper.members.read_only_properties:
            #if the return type is in the namespace we are trying to wrap, we will need that return
            #type to be a wrapper version of the type the api returns.  this means we need to construct a 
            #new instance of that wrapper class, and provide the real api instance as the constructor for it.
            #we can assume what the other wrapper will be called because everything is being smurf named the same way.
            return_namespace = read_only_property.return_type.Namespace
            if return_namespace == self.assembly_namespace:
                class_file.write(self.tab(2) + 
                                 access_modifier + ' ' + self.wrapper_abbreviation +
                                 self.get_type_aliases(read_only_property.return_type.Name, access_modifier) + ' ' + 
                                 'Internal' + read_only_property.c_sharp_name + '\n')
                class_file.write(self.tab(2) + '{\n')
                prop_name = self.get_type_aliases(read_only_property.return_type.Name, access_modifier)
                if len(prop_name) > 3:
                    if prop_name[-4:] == 'Enum':
                        class_file.write(self.tab(3) + 
                                         'get { return ' + 
                                         wrapper.target_name + 
                                         'Instance.' + read_only_property.c_sharp_name + 
                                         '.As<' + self.wrapper_abbreviation +
                                         self.get_type_aliases(read_only_property.return_type.Name, access_modifier) + '>(); }\n')
                        class_file.write(self.tab(2) + '}\n')
                        #class_file.write('\n')
                    else:
                        class_file.write(self.tab(3) + 'get { return ' + 
                                         self.wrapper_abbreviation + 
                                         self.get_type_aliases(read_only_property.return_type.Name, access_modifier) + '.By' + self.wrapper_abbreviation + 
                                         self.get_type_aliases(read_only_property.return_type.Name, access_modifier) +'(' + 
                                         wrapper.target_name + 'Instance' + '.' + 
                                         read_only_property.c_sharp_name + '); }\n')
                        class_file.write(self.tab(2) + '}\n')
                        #class_file.write('\n')
                else:
                    class_file.write(self.tab(3) + 'get { return ' + 
                                     self.wrapper_abbreviation + 
                                     self.get_type_aliases(read_only_property.return_type.Name, access_modifier) + '.By' + self.wrapper_abbreviation + 
                                     self.get_type_aliases(read_only_property.return_type.Name, access_modifier) +'(' + 
                                     wrapper.target_name + 'Instance' + '.' + 
                                     read_only_property.c_sharp_name + '); }\n')
                    class_file.write(self.tab(2) + '}\n')
                    #class_file.write('\n')

            else:
                class_file.write(self.tab(2) + 
                                 access_modifier + ' ' + 
                                 self.get_type_aliases(read_only_property.return_type.Name, access_modifier) + ' ' + 
                                 'Internal' + read_only_property.c_sharp_name + '\n')
                class_file.write(self.tab(2) + '{\n')
                class_file.write(self.tab(3) + 'get { return ' + wrapper.target_name + 'Instance' + '.' + read_only_property.c_sharp_name + '; }\n')
                class_file.write(self.tab(2) + '}\n')
        
            class_file.write('\n')
        #fix this shit!
        #create the read write properties
        for i in range(len(wrapper.members.read_write_properties)-1):
            class_file.write(self.get_read_write_property_text('internal', 
                                                               wrapper.members.read_write_properties[i]))

            class_file.write('\n')

        if len(wrapper.members.read_write_properties) > 0:
            class_file.write(self.get_read_write_property_text('internal', wrapper.members.read_write_properties[-1]))
        class_file.write(self.tab(2) + '#endregion\n')
        class_file.write('\n')

    def write_method_declaration(self, class_file, method, method_access_modifier):
        namespace = method.return_type.Namespace
        if method_access_modifier == 'private':
            class_file.write(self.tab(2) + method_access_modifier + ' ' + 
                                self.get_type_aliases(method.return_type.Name) + ' Internal' + 
                                method.c_sharp_name + 
                                self.get_arguments_string(method.arguments))
        else:
            if namespace == self.assembly_namespace:
                class_file.write(self.tab(2) + method_access_modifier + ' ' + 
                                 self.wrapper_abbreviation +
                                 self.get_type_aliases(method.return_type.Name) + ' ' + 
                                 method.c_sharp_name + 
                                 self.get_arguments_string(method.arguments))
            else:
                class_file.write(self.tab(2) + method_access_modifier + ' ' + 
                                 self.get_type_aliases(method.return_type.Name) + ' ' + 
                                 method.c_sharp_name + 
                                 self.get_arguments_string(method.arguments))

    def write_private_constructors(self, class_file, wrapper):
        class_file.write(self.tab(2) + '#region Private constructors\n')     
        class_file.write(self.tab(2) + 'private ' +
                         wrapper.name + '(' + 
                         wrapper.name + ' ' + 
                         self.format_argument_name(wrapper.name) + ')\n')
        class_file.write(self.tab(2) + '{\n')
        class_file.write(self.tab(3) + 'Internal' + 
                         wrapper.target_name + ' = ' + 
                         self.format_argument_name(wrapper.name) + '.Internal' + wrapper.target_name + ';\n')
        class_file.write(self.tab(2) + '}\n')      
        class_file.write('\n')

        class_file.write(self.tab(2) + 'private ' +
                         wrapper.name + '(' + 
                         self.assembly.GetTypes()[0].ToString().split('.')[0] + '.' + 
                         wrapper.target_name + ' ' + 
                         self.format_argument_name(wrapper.name) + ')\n')
        class_file.write(self.tab(2) + '{\n')
        class_file.write(self.tab(3) + 'Internal' + 
                         wrapper.target_name + ' = ' + 
                         self.format_argument_name(wrapper.name) + ';\n')
        class_file.write(self.tab(2) + '}\n')      
        class_file.write(self.tab(2) + '#endregion\n')
        class_file.write('\n')

    def write_private_methods(self, class_file, wrapper):
        class_file.write(self.tab(2) + '#region Private methods\n')
        method_access_modifier = 'private'
        for method in wrapper.members.methods:
            if method.c_sharp_name[0] != '_':
                self.write_method_declaration(class_file, method, method_access_modifier)
                class_file.write(self.tab(2) + '{\n')

                #if return type is void, just call the internal method
                if self.get_type_aliases(method.return_type.Name) == 'void':
                    class_file.write(self.tab(3)  + wrapper.target_name + 'Instance' + '.' + method.c_sharp_name + self.get_method_string(method.arguments))
                else:
                    class_file.write(self.tab(3) + 'return ' + wrapper.target_name + 'Instance' + '.' + method.c_sharp_name + self.get_method_string(method.arguments))
                class_file.write(self.tab(2) + '}\n')
                class_file.write('\n')
        class_file.write(self.tab(2) + '#endregion\n')
        class_file.write('\n')

    def write_public_methods(self, class_file, wrapper):
        class_file.write(self.tab(2) + '#region Public methods\n')
        method_access_modifier = 'public'
        for method in wrapper.members.methods:
            if method.c_sharp_name[0] != '_':
                self.write_method_declaration(class_file, method, method_access_modifier)
                class_file.write(self.tab(2) + '{\n')
                #if return type is void, just call the internal method
                if self.get_type_aliases(method.return_type.Name) == 'void':
                    class_file.write(self.tab(3) + 'Internal' + method.c_sharp_name + self.get_method_string(method.arguments))
                #if there is a return type, return the result of calling the internal method
                else:
                    class_file.write(self.tab(3) + 'return Internal' + method.c_sharp_name + self.get_method_string(method.arguments))
                class_file.write(self.tab(2) + '}\n')
                class_file.write('\n')
        class_file.write(self.tab(2) + '#endregion\n')

    def write_public_properties(self, class_file, wrapper):
        class_file.write(self.tab(2) + '#region Public properties\n')
        
        #this first public property provides the link between the public wrapper versions of methods and properties 
        #and the internal api specific methods and properties of the class we are wrapping.
        class_file.write(self.tab(2) + 'public ' + self.assembly.GetTypes()[0].ToString().split('.')[0] + '.' + 
                 wrapper.target_name + ' ' + wrapper.target_name + 'Instance\n')
        class_file.write(self.tab(2) + '{\n')
        class_file.write(self.tab(3) + 'get { return ' + 'Internal' + wrapper.target_name + '; }\n')
        class_file.write(self.tab(3) + 'set { ' + 'Internal' + wrapper.target_name + ' = value; }\n')
        class_file.write(self.tab(2) + '}\n')
        class_file.write('\n')             
        
        #create the read only properties
        access_modifier = 'public'
        
        for read_only_property in wrapper.members.read_only_properties:
            return_namespace = read_only_property.return_type.Namespace
            if return_namespace == self.assembly_namespace:
                class_file.write(self.tab(2) + 
                                    access_modifier + ' ' + self.wrapper_abbreviation + 
                                    self.get_type_aliases(read_only_property.return_type.Name, access_modifier) + ' ' + 
                                    read_only_property.c_sharp_name + '\n')
                class_file.write(self.tab(2) + '{\n')
                class_file.write(self.tab(3) + 'get { return ' + 'Internal' + read_only_property.c_sharp_name + '; }\n')
                class_file.write(self.tab(2) + '}\n')
                class_file.write('\n')
            else:
                print return_namespace
                class_file.write(self.tab(2) + 
                                 access_modifier + ' ' +
                                 self.get_type_aliases(read_only_property.return_type.Name, access_modifier) + ' ' + 
                                 read_only_property.c_sharp_name + '\n')
                class_file.write(self.tab(2) + '{\n')
                class_file.write(self.tab(3) + 'get { return ' + 'Internal' + read_only_property.c_sharp_name + '; }\n')
                class_file.write(self.tab(2) + '}\n')
                class_file.write('\n')

        #create the read write properties
        for read_write_property in wrapper.members.read_write_properties:
            return_namespace = read_write_property.return_type.Namespace
            if return_namespace == self.assembly_namespace:
                class_file.write(self.tab(2) + 
                                    access_modifier + ' ' + self.wrapper_abbreviation +
                                    self.get_type_aliases(read_write_property.return_type.Name, access_modifier) + ' ' + 
                                    read_write_property.c_sharp_name + '\n')
                class_file.write(self.tab(2) + '{\n')
                class_file.write(self.tab(3) + 'get { return ' + 'Internal' + read_write_property.c_sharp_name + '; }\n')
                class_file.write(self.tab(3) + 'set { ' + 'Internal' + read_write_property.c_sharp_name + ' = value; }\n')
                class_file.write(self.tab(2) + '}\n')
                class_file.write('\n')

            else:
                class_file.write(self.tab(2) + 
                                    access_modifier + ' ' +
                                    self.get_type_aliases(read_write_property.return_type.Name, access_modifier) + ' ' + 
                                    read_write_property.c_sharp_name + '\n')
                class_file.write(self.tab(2) + '{\n')
                class_file.write(self.tab(3) + 'get { return ' + 'Internal' + read_write_property.c_sharp_name + '; }\n')
                class_file.write(self.tab(3) + 'set { ' + 'Internal' + read_write_property.c_sharp_name + ' = value; }\n')
                class_file.write(self.tab(2) + '}\n')
                class_file.write('\n')


        class_file.write(self.tab(2) + '#endregion\n')
        class_file.write('\n')
          
    def write_public_static_constructors(self, class_file, wrapper):
        class_file.write(self.tab(2) + '#region Public static constructors\n')
        #probably don't need this at all
        class_file.write(self.tab(2) + 'public static ' + wrapper.name + ' By' + wrapper.name + '(' + wrapper.name + ' ' + self.format_argument_name(wrapper.name) + ')\n')
        class_file.write(self.tab(2) + '{\n')
        class_file.write(self.tab(3) + 'return new ' + wrapper.name + '(' + self.format_argument_name(wrapper.name) + ');\n')
        class_file.write(self.tab(2) + '}\n')

        class_file.write('\n');
        
        class_file.write(self.tab(2) + 'public static ' + wrapper.name + ' By' + wrapper.name + '(' + self.assembly.GetTypes()[0].ToString().split('.')[0] + '.' + 
                 wrapper.target_name + ' ' + self.format_argument_name(wrapper.name) + ')\n')
        class_file.write(self.tab(2) + '{\n')
        class_file.write(self.tab(3) + 'return new ' + wrapper.name + '(' + self.format_argument_name(wrapper.name) + ');\n')
        class_file.write(self.tab(2) + '}\n')
        class_file.write(self.tab(2) + '#endregion\n')
        class_file.write('\n')

    def write_namespace(self, class_file):
        class_file.write('namespace ' + self.destination_namespace + '\n')
        class_file.write('{\n')

    def write_using_directives(self, class_file):
        class_file.writelines(self.using_statements.Select(lambda u: 'using ' + u + ';\n'))
        class_file.write('\n')


class ClassToWrap:
    def __init__(self, assembly, target_type_name, wrapper_abbreviation):
        self.assembly = assembly
        self.target_type = self.assembly.GetType(target_type_name)
        print target_type_name
        #find any index properties so we can remove them from 'members'.
        #index properties are returned from GetMethods, but starting from a MethodInfo object for
        #the index property, you won't be able to use reflection to determine its parameters, which is dumb.
        if self.target_type != None:
            self.index_properties = self.target_type.GetProperties().Where(lambda p: p.GetIndexParameters().Any())
        
            self.members = WrappedClassMembers(self.target_type.GetMethods().ToList()
                                               .Where(lambda y: (y.IsPublic) & 
                                                      (self.index_properties.All(lambda t: y.Name != ('get_' + t.Name))))
                                               .OrderBy(lambda p: p.Name))

            self.target_name = self.target_type.Name
            self.name = wrapper_abbreviation + self.target_type.Name
            self.file_name = self.name + '.cs'
               
class WrappedClassMembers:
    def __init__(self, member_info):
        self.all_members = [Method(m) for m in member_info]
        self.read_write_properties = [Method(m) for m in member_info
                                      .Where(lambda m: m.Name[:4] == 'get_')
                                      .Where(lambda p: member_info.Any(lambda k: (k.Name[4:] == p.Name[4:]) & (k.Name[:4] == 'set_')))
                                      .Where(lambda a: a.GetParameters().Count == 0).ToList()]

        self.read_only_properties = []

        get_members = self.all_members.Where(lambda p: p.name[:4] == 'get_')
        set_members = self.all_members.Where(lambda p: p.name[:4] == 'set_')
        for get_member in get_members:
            if set_members.Any(lambda k: k.name[4:] == get_member.name[4:]):
                pass
            else:
                self.read_only_properties.append(get_member)
               
        self.methods = [Method(m) for m in member_info
                        .Where(lambda p: self.read_write_properties.All(lambda q: q.name != p.Name))
                        .Where(lambda p: self.read_only_properties.All(lambda q: q.name != p.Name))
                        .Where(lambda p: p.Name[:4] != 'set_')
                        .Where(lambda p: p.Name[:4] != 'get_')]

        print "All members:  " + str(self.all_members.Count)
        print "Read-only:   " + str(self.read_only_properties.Count)
        print "Read-write:   " + str(self.read_write_properties.Count)
        print "Methods:    " + str(self.methods.Count)
   
class Method:
    def __init__(self, method_info):
        self.method_info = method_info
        self.name = self.method_info.Name
        if (self.name[:4] == 'get_') | (self.name[:4] == 'set_'):
            self.c_sharp_name = self.name[4:]
        else:
            self.c_sharp_name = self.name
        self.return_type = self.method_info.ReturnType
        self.arguments = self.method_info.GetParameters().Select(lambda p: [p.ParameterType.Name, p.Name, self.get_is_byref_or_out(p)])

    def get_is_byref_or_out(self, parameter):
        if parameter.IsOut:
            if parameter.IsIn:      
                return 'ref '
            else:
                return 'out '
        else:
            return ''