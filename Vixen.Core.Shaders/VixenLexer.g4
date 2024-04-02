lexer grammar VixenLexer;
import UnicodeClasses;

channels { COMMENTS_CHANNEL, DIRECTIVE }
options { superClass = VixenLexerBase; }

//BYTE_ORDER_MARK: '\u00EF\u00BB\u00BF';

SINGLE_LINE_DOC_COMMENT:     '///' InputCharacter* -> channel(COMMENTS_CHANNEL);
EMPTY_DELIMITED_DOC_COMMENT: '/***/'               -> channel(COMMENTS_CHANNEL);
DELIMITED_DOC_COMMENT:       '/**' ~'/' .*? '*/'   -> channel(COMMENTS_CHANNEL);
SINGLE_LINE_COMMENT:         '//' InputCharacter*  -> channel(COMMENTS_CHANNEL);
DELIMITED_COMMENT:           '/*' .*? '*/'         -> channel(COMMENTS_CHANNEL);
WHITESPACES:                 Whitespace+           -> channel(HIDDEN);
SHARP:                       '#'                   -> mode(DIRECTIVE_MODE), skip;
//NL:                          NewLine -> channel(HIDDEN);
NL:                          NewLine;


// ===== Keywords
// definition types
FUNC:           'func';
PROTOCOL:       'protocol';
CLASS:          'class';
STRUCT:         'struct';
ENUM:           'enum';
SHADER:         'shader';

// Modifiers & Variables
PRIVATE:        'private';
STATIC:         'static';
CONST:          'const';
VAR:            'var';
VAL:            'val';

// Selection Statmenets
IF:             'if';
ELSE:           'else';
SWITCH:         'switch';

// Iteration Statements
WHILE:          'while';
REPEAT:         'repeat';
IN:             'in';
FOR:            'for';

// Jump Statements
RETURN:         'return';
BREAK:          'break';
CONTINUE:       'continue';

// Import
IMPORT:         'import';
PACKAGE:        'package';

// Other
INIT:           'init';
AS:             'as';
FALSE:          'false';
TRUE:           'true';
OUT:            'out';
REF:            'ref';
WHERE:          'where';

SELF:           'self';
BASE:           'base';
DEFAULT:        'default';
SIZEOF:         'sizeof';
NAMEOF:         'nameof';
USING:          'using';
OVERRIDE:       'override';
ABSTRACT:       'abstract';
PARTIAL:        'partial';


// Value Types
NULL_:          'null';
OBJECT:         'object';
BOOL:           'bool';
CHAR:           'char';
STRING:         'string';
BYTE:           'byte';
SHORT:          'short';
USHORT:         'ushort';
INT:            'int';
UINT:           'uint';
LONG:           'long';
ULONG:          'unlong';
DOUBLE:         'double';
FLOAT:          'float';


// Identifiers
IDENTIFIER: IdentifierOrKeyword;


// Literals
LITERAL_ACCESS:      [0-9] ('_'* [0-9])* IntegerTypeSuffix? DOT IdentifierOrKeyword;
INTEGER_LITERAL:     [0-9] ('_'* [0-9])* IntegerTypeSuffix?;
HEX_INTEGER_LITERAL: '0' [xX] ('_'* HexDigit)+ IntegerTypeSuffix?;
BIN_INTEGER_LITERAL: '0' [bB] ('_'* [01])+ IntegerTypeSuffix?;
REAL_LITERAL:        ([0-9] ('_'* [0-9])*)? '.' [0-9] ('_'* [0-9])* ExponentPart? [FfDdMm]? | [0-9] ('_'* [0-9])* ([FfDdMm] | ExponentPart [FfDdMm]?);

CHARACTER_LITERAL:   '\'' (~['\\\r\n\u0085\u2028\u2029] | CommonCharacter) '\'';
REGULAR_STRING:      '"'  (~["\\\r\n\u0085\u2028\u2029] | CommonCharacter)* '"';

// Operators and Punctuations
OPEN_BRACE:               '{' { this.OnOpenBrace(); };
CLOSE_BRACE:              '}' { this.OnCloseBrace(); };
OPEN_BRACKET:             '[';
CLOSE_BRACKET:            ']';
OPEN_PARENS:              '(';
CLOSE_PARENS:             ')';
DOT:                      '.';
COMMA:                    ',';
COLON:                    ':' { this.OnColon(); };
SEMICOLON:                ';';
PLUS:                     '+';
MINUS:                    '-';
STAR:                     '*';
DIV:                      '/';
PERCENT:                  '%';
AMP:                      '&';
BITWISE_OR:               '|';
CARET:                    '^';
BANG:                     '!';
AT:                       '@';
TILDE:                    '~';
ASSIGNMENT:               '=';
LT:                       '<';
GT:                       '>';
INTERR:                   '?';
DOUBLE_COLON:             '::';
OP_COALESCING:            '??';
OP_INC:                   '++';
OP_DEC:                   '--';
OP_AND:                   '&&';
OP_OR:                    '||';
OP_ARROW:                 '->';
OP_EQ:                    '==';
OP_NE:                    '!=';
OP_LE:                    '<=';
OP_GE:                    '>=';
OP_ADD_ASSIGNMENT:        '+=';
OP_SUB_ASSIGNMENT:        '-=';
OP_MULT_ASSIGNMENT:       '*=';
OP_DIV_ASSIGNMENT:        '/=';
OP_MOD_ASSIGNMENT:        '%=';
OP_AND_ASSIGNMENT:        '&=';
OP_OR_ASSIGNMENT:         '|=';
OP_XOR_ASSIGNMENT:        '^=';
OP_LEFT_SHIFT:            '<<';
OP_LEFT_SHIFT_ASSIGNMENT: '<<=';
OP_COALESCING_ASSIGNMENT: '??=';
OP_RANGE:                 '..';
OP_LAMBDA:                '=>';

OP_AS:                    'as';
OP_AS_SAFE:               'as?';
OP_IN:                    'in';
OP_NOT_IN:                '!in';
OP_IS:                    'is';
OP_NOT_IS:                '!is';


// TODO: string interpolation stuff





// Preprocessor directives
mode DIRECTIVE_MODE;

DIRECTIVE_WHITESPACES:         Whitespace+                      -> channel(HIDDEN);
DIGITS:                        [0-9]+                           -> channel(DIRECTIVE);
DIRECTIVE_TRUE:                'true'                           -> channel(DIRECTIVE), type(TRUE);
DIRECTIVE_FALSE:               'false'                          -> channel(DIRECTIVE), type(FALSE);
DEFINE:                        'define'                         -> channel(DIRECTIVE);
UNDEF:                         'undef'                          -> channel(DIRECTIVE);
//DIRECTIVE_IF:                  'if'                             -> channel(DIRECTIVE), type(IF);
ELIF:                          'elif'                           -> channel(DIRECTIVE);
//DIRECTIVE_ELSE:                'else'                           -> channel(DIRECTIVE), type(ELSE);
ENDIF:                         'endif'                          -> channel(DIRECTIVE);
LINE:                          'line'                           -> channel(DIRECTIVE);
ERROR:                         'error' Whitespace+              -> channel(DIRECTIVE), mode(DIRECTIVE_TEXT);
WARNING:                       'warning' Whitespace+            -> channel(DIRECTIVE), mode(DIRECTIVE_TEXT);
PRAGMA:                        'pragma' Whitespace+             -> channel(DIRECTIVE), mode(DIRECTIVE_TEXT);
//DIRECTIVE_DEFAULT:             'default'                        -> channel(DIRECTIVE), type(DEFAULT);
DIRECTIVE_HIDDEN:              'hidden'                         -> channel(DIRECTIVE);
DIRECTIVE_OPEN_PARENS:         '('                              -> channel(DIRECTIVE), type(OPEN_PARENS);
DIRECTIVE_CLOSE_PARENS:        ')'                              -> channel(DIRECTIVE), type(CLOSE_PARENS);
DIRECTIVE_BANG:                '!'                              -> channel(DIRECTIVE), type(BANG);
DIRECTIVE_OP_EQ:               '=='                             -> channel(DIRECTIVE), type(OP_EQ);
DIRECTIVE_OP_NE:               '!='                             -> channel(DIRECTIVE), type(OP_NE);
DIRECTIVE_OP_AND:              '&&'                             -> channel(DIRECTIVE), type(OP_AND);
DIRECTIVE_OP_OR:               '||'                             -> channel(DIRECTIVE), type(OP_OR);
DIRECTIVE_STRING:              '"' ~('"' | [\r\n\u0085\u2028\u2029])* '"' -> channel(DIRECTIVE), type(STRING);
CONDITIONAL_SYMBOL:            IdentifierOrKeyword              -> channel(DIRECTIVE);
DIRECTIVE_SINGLE_LINE_COMMENT: '//' ~[\r\n\u0085\u2028\u2029]*  -> channel(COMMENTS_CHANNEL), type(SINGLE_LINE_COMMENT);
DIRECTIVE_NEW_LINE:            NewLine                          -> channel(DIRECTIVE), mode(DEFAULT_MODE);

mode DIRECTIVE_TEXT;

TEXT:                          ~[\r\n\u0085\u2028\u2029]+       -> channel(DIRECTIVE);
TEXT_NEW_LINE:                 NewLine                          -> channel(DIRECTIVE), type(DIRECTIVE_NEW_LINE), mode(DEFAULT_MODE);


// Fragments
// Everything except new line character. Used for comments
fragment InputCharacter:     ~[\r\n\u0085\u2028\u2029];
fragment IntegerTypeSuffix:  [lL]? [uU] | [uU]? [lL];
fragment ExponentPart:       [eE] ('+' | '-')? [0-9] ('_'* [0-9])*;
	
fragment CommonCharacter
	: SimpleEscapeSequence
	| HexEscapeSequence
	| UnicodeEscapeSequence
	;

fragment SimpleEscapeSequence
	: '\\\''
	| '\\"'
	| '\\\\'
	| '\\0'
	| '\\a'
	| '\\b'
	| '\\f'
	| '\\n'
	| '\\r'
	| '\\t'
	| '\\v'
	;

fragment HexEscapeSequence
	: '\\x' HexDigit
	| '\\x' HexDigit HexDigit
	| '\\x' HexDigit HexDigit HexDigit
	| '\\x' HexDigit HexDigit HexDigit HexDigit
	;

fragment NewLine
	: '\r\n' | '\r' | '\n'
	| '\u0085' // <Next Line CHARACTER (U+0085)>'
	| '\u2028' // '<Line Separator CHARACTER (U+2028)>'
	| '\u2029' // '<Paragraph Separator CHARACTER (U+2029)>'
	;

fragment Whitespace
	: UNICODE_CLASS_ZS // '<Any Character With Unicode Class Zs>'
	| '\u0009' // '<Horizontal Tab Character (U+0009)>'
	| '\u000B' // '<Vertical Tab Character (U+000B)>'
	| '\u000C' // '<Form Feed Character (U+000C)>'
	;	
	
fragment IdentifierOrKeyword
	: IdentifierStartCharacter IdentifierPartCharacter*
	;

fragment IdentifierStartCharacter
	: LetterCharacter
	| '_'
	;

fragment IdentifierPartCharacter
	: LetterCharacter
	| DecimalDigitCharacter
	| ConnectingCharacter
	| CombiningCharacter
	| FormattingCharacter
	;

// '<A Unicode Character Of Classes Lu, Ll, Lt, Lm, Lo, Or Nl>'
// WARNING: ignores UnicodeEscapeSequence
fragment LetterCharacter
	: UNICODE_CLASS_LU
	| UNICODE_CLASS_LL
	| UNICODE_CLASS_LT
	| UNICODE_CLASS_LM
	| UNICODE_CLASS_LO
	| UNICODE_CLASS_NL
	| UnicodeEscapeSequence
	;	
	
// '<A Unicode Character Of The Class Nd>'
// WARNING: ignores UnicodeEscapeSequence
fragment DecimalDigitCharacter
	: UNICODE_CLASS_ND
	| UnicodeEscapeSequence
	;

//'<A Unicode Character Of The Class Pc>'
// WARNING: ignores UnicodeEscapeSequence
fragment ConnectingCharacter
	: UNICODE_CLASS_PC
	| UnicodeEscapeSequence
	;

// '<A Unicode Character Of Classes Mn Or Mc>'
// WARNING: ignores UnicodeEscapeSequence
fragment CombiningCharacter
	: UNICODE_CLASS_MN
	| UNICODE_CLASS_MC
	| UnicodeEscapeSequence
	;

// '<A Unicode Character Of The Class Cf>'
// WARNING: ignores UnicodeEscapeSequence
fragment FormattingCharacter
	: UNICODE_CLASS_CF
	| UnicodeEscapeSequence
	;

//B.1.5 Unicode Character Escape Sequences
fragment UnicodeEscapeSequence
	: '\\u' HexDigit HexDigit HexDigit HexDigit
	| '\\U' HexDigit HexDigit HexDigit HexDigit HexDigit HexDigit HexDigit HexDigit
	;

fragment HexDigit : [0-9] | [A-F] | [a-f];	
