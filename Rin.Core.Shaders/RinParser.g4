parser grammar RinParser;

options { tokenVocab=RinLexer; superClass=RinParserBase; }


compilation_unit
    : package_declaration import_directives? type_declaration+ NL* EOF
    ;


// ===== Basic Concepts
// TODO: finish this
namespace_or_type_name
    : (identifier type_argument_list? | qualified_alias_member) (DOT identifier type_argument_list?)*
    ;


// ===== Types
type_
    : base_type (INTERR | rank_specifier)*
    ;
    
base_type
    : simple_type
    | class_type
    | tuple_type
    ;
    
tuple_type
    : OPEN_PARENS tuple_element (COMMA tuple_element)+ CLOSE_PARENS
    ;
    
tuple_element
    : type_ (COLON identifier)?
    ;
    
simple_type
    : numeric_type
    | BOOL
    ;
   
numeric_type
    : integral_type
    | floating_point_type
    ;
   
integral_type
    : BYTE
    | SHORT
    | USHORT
    | INT
    | UINT
    | LONG
    | ULONG
    | CHAR
    ;
    
floating_point_type
    : FLOAT
    | DOUBLE
    ;
    
class_type
    // TODO: object / dynamic?
    : namespace_or_type_name
    | STRING
    ;
    
// <int, string, Test>
type_argument_list
    : LT type_ (COMMA type_)* GT
    ;
    
   
// ===== Expressions
// Used for method invocation
argument_list
    : argument (COMMA argument)*
    ;
    
argument
    : (identifier COLON)? // TODO: ref out in
      expression
    ;
    
expression
	: assignment
	| non_assignment_expression
// TODO	| REF non_assignment_expression
	;

non_assignment_expression
// TODO
//	: lambda_expression
//	| query_expression
	: conditional_expression
	;

assignment
	: unary_expression assignment_operator expression
	| unary_expression '??=' unary_expression // throwable_expression
	;

assignment_operator
	: '=' | '+=' | '-=' | '*=' | '/=' | '%=' | '&=' | '|=' | '^=' | '<<='// TODO | right_shift_assignment
	;

conditional_expression
//	: null_coalescing_expression ('?' throwable_expression ':' throwable_expression)?
	: null_coalescing_expression (INTERR expression COLON expression)?
	;

null_coalescing_expression
//	: conditional_or_expression ('??' (null_coalescing_expression | throw_expression))?
	: conditional_or_expression (OP_COALESCING (null_coalescing_expression | expression))?
	;

conditional_or_expression
	: conditional_and_expression (OP_OR conditional_and_expression)*
	;

conditional_and_expression
	: inclusive_or_expression (OP_AND inclusive_or_expression)*
	;

inclusive_or_expression
	: exclusive_or_expression (BITWISE_OR exclusive_or_expression)*
	;

exclusive_or_expression
	: and_expression (CARET and_expression)*
	;

and_expression
	: equality_expression (AMP equality_expression)*
	;

equality_expression
	: relational_expression ((OP_EQ | OP_NE)  relational_expression)*
	;

relational_expression
    // TODO: finish is and implement !is and !as ??
	: shift_expression (('<' | '>' | '<=' | '>=') shift_expression /*| IS isType*/ | AS type_)*
	;

shift_expression
    // TODO: finish right shift
	: additive_expression (('<<' /*| right_shift*/)  additive_expression)*
	;

additive_expression
	: multiplicative_expression (('+' | '-')  multiplicative_expression)*
	;

multiplicative_expression
	: switch_expression (('*' | '/' | '%')  switch_expression)*
	;

switch_expression
    : range_expression (SWITCH '{' switch_expression_arms? '}')?
    ;

switch_expression_arms
    : switch_expression_arm (COMMA switch_expression_arm)*
    ;

switch_expression_arm
    // TODO: finish
    : expression // case_guard? right_arrow throwable_expression
    ;

range_expression
    : unary_expression
    | unary_expression? OP_RANGE unary_expression?
    ;

unary_expression
	: cast_expression
	| primary_expression
	| '+' unary_expression
	| '-' unary_expression
	| BANG unary_expression
	| '~' unary_expression
	| '++' unary_expression
	| '--' unary_expression
//	| AWAIT unary_expression
	| '&' unary_expression
	| '*' unary_expression
	| '^' unary_expression
	;

cast_expression
    : OPEN_PARENS type_ CLOSE_PARENS unary_expression
    ;

primary_expression
    // TODO: finish
	: pe=primary_expression_start '!'? bracket_expression* '!'?
//	  ((member_access | method_invocation | '++' | '--' | '->' identifier) '!'? bracket_expression* '!'?)*
	  ((member_access | method_invocation | '++' | '--') '!'? bracket_expression* '!'?)* NL* // TODO: verify this NL*
//      (member_access | method_invocation | OP_INC | OP_DEC)* NL*
	;

primary_expression_start
//    TODO: finish
	: literal                                   #literalExpression
	| identifier type_argument_list?            #simpleNameExpression
	| OPEN_PARENS expression CLOSE_PARENS       #parenthesisExpressions
    | predefined_type                           #memberAccessExpression
    | qualified_alias_member                    #memberAccessExpression
    | LITERAL_ACCESS                            #literalAccessExpression
	;
   
member_access
    : INTERR? DOT identifier type_argument_list?
    ;
    
bracket_expression
    : INTERR? OPEN_BRACKET indexer_argument (COMMA indexer_argument)* CLOSE_BRACKET
    ;
    
indexer_argument
    : (identifier COLON)? expression
    ;
    
predefined_type
    : BOOL | BYTE | CHAR | SHORT | USHORT | INT | UINT | LONG | ULONG | DOUBLE | FLOAT | STRING
    ;
   
// TODO: expression_list, object initializers...
   
// TODO: isType...

// TODO: lambda
   
   
// ===== Statements
statement
    // labeled/goto not supported
    : declaration_statement
    | embedded_statement
    ; 
    
declaration_statement
    : local_constant_declaration NL+
    | local_variable_declaration NL+
    | local_function_declaration NL*
    ;   
   
local_function_declaration
    : local_function_header local_function_body
    ;
    
local_function_header
    // TODO: function modifiers (static...)
    // TODO: consider allowing usage of void as return type
    : FUNC identifier type_parameter_list?
      OPEN_PARENS formal_parameter_list? CLOSE_PARENS (COLON type_)? // TODO: constrains
    ;
    
local_function_body
    : block
    | OP_LAMBDA expression NL+
    ;
   
embedded_statement
    : block
    | simple_embedded_statement
    ;
    
simple_embedded_statement
    : expression                                                    #expressionStatement
    
    // selection statements
    | IF OPEN_PARENS expression CLOSE_PARENS block (ELSE block)?    #ifStatement
    // TODO: switch
    
    // iteration statements
    | WHILE OPEN_PARENS expression CLOSE_PARENS block               #whileStatement
    | REPEAT block WHILE OPEN_PARENS expression CLOSE_PARENS        #repeatStatement
    // TODO: identifier or tuple?
    | FOR OPEN_PARENS identifier IN expression CLOSE_PARENS block   #forStatement
    
    | BREAK NL+                                                     #breakStatement
    | CONTINUE NL+                                                  #continueStatement
    | RETURN expression? NL+                                        #returnStatement
    
    // using?
    // yield?
    ;
   
block
    : OPEN_BRACE NL* statement_list? NL* CLOSE_BRACE NL*
    ;
   
local_variable_declaration
    // TODO: using ref...
    : /*REF USING*/ (VAL | VAR) identifier (COLON type_)? (ASSIGNMENT /*REF*/ local_variable_initializer)?
    ;
    
local_variable_initializer
    : expression
    | array_initializer
    ;   
   
local_constant_declaration
    : CONST VAL constant_declarator
    ;
   
   
// TODO: if body, switch, ...
   
   
statement_list
    : statement+
    ;
   
// TODO: for...
   
// TODO: resource_acquisition - used inside using() statement
   
   
// ===== Imports and Package
package_declaration
    : PACKAGE qualified_identifier NL+
    ;
    
qualified_identifier
    : identifier (DOT identifier)*
    ;
    
import_directives
    : import_directive+
    ;
    
import_directive
    : IMPORT namespace_or_type_name NL+     #importPackageDirective
    ;
   
type_declaration
    // TODO: finish declaration of all stuff (attributes, modifiers, interface, struct, class, enum, ...)
    : shader_definition
    ;
   
qualified_alias_member
   	: identifier '::' identifier type_argument_list?
   	;
   
   
// ===== Classes
type_parameter_list
    : LT type_parameter (COMMA type_parameter)* GT
    ;

type_parameter
    // TODO: how to handle attributes on types in generics?
    : attributes? identifier
    ;
    
// Inheritance
class_base
    // TODO: not sure is class_type is correct way how to handle inheritance in classes
    : COLON class_type (COMMA namespace_or_type_name)*
    ;
    
// TODO: interface type list
// TODO: constrains
    
    
class_body
    : OPEN_BRACE NL* class_member_declarations? NL* CLOSE_BRACE
    ;
   
class_member_declarations
    : (class_member_declaration NL*)+
    ;
    
class_member_declaration
// TODO: destructor
    : attributes? all_member_modifiers? common_member_declaration
    ;
    
all_member_modifiers
    : all_member_modifier+
    ;
    
all_member_modifier
    // TODO
    : PRIVATE
    | STATIC
    ;
    
// Intersection of struct and class member declaration
common_member_declaration
    // typed_member_declaration is ommited as types needs to be specified per individual declaration
    // TODO: finish this (include typed_member_declaration)
    : constant_declaration
    | constructor_declaration
    
    | method_declaration
    | field_declaration
    ;
    
constant_declarator
    : identifier (COLON type_)? ASSIGNMENT expression
    ;
    
variable_declarator
    : identifier (COLON type_)? (ASSIGNMENT NL* variable_initializer)?
    ;
    
variable_initializer
    : expression
    | array_initializer
    ;
    
// TODO: return type? VOID can be omitted so probably not needed
// TODO: member_name can be omitted too and namespace_or_type_name can be used instead
// TODO: body and method_body is omitted in favor of block
    
formal_parameter_list
    //TODO params and the kotlin's delegate calling convention
    : fixed_parameters
    ;
    
fixed_parameters
    : fixed_parameter (COMMA fixed_parameter)*
    ;
    
fixed_parameter
    // TODO: parameter_modifier, arglist?
    : attributes? arg_declaration
    ;
    
// TODO: accessor declaration
// TODO: overloadable operators
// TODO: constructor initializer


    
// ===== Structs 
// TODO

    
// ===== Arrays
//array_type
//	: base_type (('*' | '?')* rank_specifier)+
//	;
    
// array declarations [] or [,]
rank_specifier
    : OPEN_BRACKET COMMA* CLOSE_BRACKET
    ;
    
array_initializer
    : OPEN_BRACKET NL* (variable_initializer (COMMA NL* variable_initializer)*)? NL* CLOSE_BRACKET
    ;
    
    
// ===== Interfaces
// TODO
    

// ===== Enums
// TODO


// ===== Attributes
// TODO: global attributes?

attributes
    : attribute+
    ;
    
attribute
    : AT namespace_or_type_name (OPEN_PARENS attribute_argument (COMMA attribute_argument)*  CLOSE_PARENS)? NL+
    ;
    
attribute_argument
    : (identifier COLON)? expression
    ;
    
    
// ===== Grammar extensions
literal
    : boolean_literal
    | string_literal
    | INTEGER_LITERAL
    | HEX_INTEGER_LITERAL
    | BIN_INTEGER_LITERAL
    | REAL_LITERAL
    | CHARACTER_LITERAL
    | NULL_
    ;

boolean_literal
    : TRUE
    | FALSE
    ;

// TODO
string_literal
// : interpolated_regular_string
// | interpolated_verbatium_string
    : REGULAR_STRING
//    | VERBATIUM_STRING
    ; 
    
    
// ===== Extra rules for modularization
shader_definition
    : SHADER identifier type_parameter_list? class_base? // TODO constrains
      class_body
    ;
    
// TODO: struct definition, interface, enum, delegate, event, class

field_declaration
    : (VAR | VAL) variable_declarator NL+
    ;
    
// TODO: propery

constant_declaration
    : CONST VAL constant_declarator NL+
    ;
    
// TODO: indexer, destructor
    
constructor_declaration
    : INIT OPEN_PARENS formal_parameter_list? CLOSE_PARENS block
    ;
    
method_declaration
    : FUNC method_member_name type_parameter_list? OPEN_PARENS formal_parameter_list? CLOSE_PARENS
      (COLON type_)?
        // TODO constrains
      (block | OP_LAMBDA expression)
    ;
    
method_member_name
    : (identifier | identifier '::' identifier) (type_argument_list? DOT identifier)*
    ;
    
// TODO: operator declaration
    
arg_declaration
    : identifier COLON type_ (ASSIGNMENT expression)?
    ;
    
method_invocation
    : OPEN_PARENS argument_list? CLOSE_PARENS NL+
    ;
    
// TODO: object creation expression
    
    
identifier
    : IDENTIFIER
    ;