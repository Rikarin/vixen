using Antlr4.Runtime.Tree;
using Rin.Core.Shaders.Ast;

namespace Rin.Core.Shaders; 

public partial class BuildAstVisitor : RinParserBaseVisitor<Node> {
    public override Node VisitCompilation_unit(RinParser.Compilation_unitContext context) {
        var package = Visit(context.package_declaration()) as PackageStatement;
        // TODO: imports

        var declarations = new DeclarationList();
        foreach (var declaration in context.type_declaration()) {
            declarations.Add(Visit(declaration));
        }

        return new Module(package!, declarations);
    }

    public override Node VisitPackage_declaration(RinParser.Package_declarationContext context) {
        var identifier = Visit(context.qualified_identifier()) as Identifier;
        return new PackageStatement(identifier!);
    }

    public override Node VisitQualified_identifier(RinParser.Qualified_identifierContext context) {
        var str = context.identifier().Select(x => x.GetText());
        return new Identifier(string.Join('.', str));
    }

    public override Node VisitType_declaration(RinParser.Type_declarationContext context) {
        return Visit(context.GetChild(0));
    }

    public override Node VisitShader_definition(RinParser.Shader_definitionContext context) {
        var identifier = Visit(context.identifier()) as Identifier;
        // TODO: types, base classes
        var declarations = Visit(context.class_body()) as DeclarationList;

        return new Shader(identifier, declarations);
    }

    public override Node VisitIdentifier(RinParser.IdentifierContext context) {
        return new Identifier(context.GetText());
    }


    public override Node VisitClass_body(RinParser.Class_bodyContext context) {
        return Visit(context.class_member_declarations());
    }

    public override Node VisitClass_member_declarations(RinParser.Class_member_declarationsContext context) {
        var declarations = new DeclarationList();
        foreach (var declaration in context.class_member_declaration()) {
            declarations.Add(Visit(declaration));
        }

        return declarations;
    }

    public override Node VisitClass_member_declaration(RinParser.Class_member_declarationContext context) {
        var declaration = Visit(context.common_member_declaration());
        
        // TODO: set attributes and modifiers to declaration
        if (context.attributes() is { } attributes) {
            var x = Visit(attributes);
        }
        
        if (context.all_member_modifiers() is { } modifiers) {
            var x = Visit(modifiers);
        }

        return declaration;
    }

    public override Node VisitCommon_member_declaration(RinParser.Common_member_declarationContext context) {
        return Visit(context.GetChild(0));
    }

    public override Node VisitConstructor_declaration(RinParser.Constructor_declarationContext context) {
        var parameterList = VisitParameterList(context.formal_parameter_list());
        var body = Visit(context.block()) as Statement;
        // TODO: type parameters, return type
        // TODO: finish

        return new ConstructorDeclaration(parameterList, body);
    }

    public override Node VisitMethod_declaration(RinParser.Method_declarationContext context) {
        var name = Visit(context.method_member_name()) as Identifier;
        var parameterList = VisitParameterList(context.formal_parameter_list());
        var body = VisitMethodBody(context.block(), context.expression());
        // TODO: type parameters, return type
        // TODO: finish

        return new MethodDeclaration(name, parameterList ?? new ParameterList(), new ValType(), body);
    }

    ParameterList? VisitParameterList(IParseTree? formalParameterList) {
        if (formalParameterList != null) {
            return Visit(formalParameterList) as ParameterList;
        }

        return null;
    }

    Statement VisitMethodBody(IParseTree? block, IParseTree expression) {
        if (block != null) {
            return Visit(block) as Statement;
        }

        return new ExpressionStatement(Visit(expression) as Expression);
    }
    

    // void global::System.Collections.Generic.ICollection<int>.GetEnumerator() { }
    public override Node VisitMethod_member_name(RinParser.Method_member_nameContext context) {
        // TODO: finish this
        return new Identifier(context.identifier(0).GetText());
    }


    public override Node VisitFormal_parameter_list(RinParser.Formal_parameter_listContext context) {
        // TODO
        return Visit(context.fixed_parameters());
    }

    public override Node VisitFixed_parameters(RinParser.Fixed_parametersContext context) {
        var parameterList = new ParameterList();
        
        foreach (var param in context.fixed_parameter()) {
            parameterList.Add(Visit(param) as Parameter);
        }

        return parameterList;
    }

    public override Node VisitFixed_parameter(RinParser.Fixed_parameterContext context) {
        var param = Visit(context.arg_declaration()) as Parameter;
        if (context.attributes() is { } attributes) {
            // TODO: attributes
        }

        return param;
    }

    public override Node VisitArg_declaration(RinParser.Arg_declarationContext context) {
        var name = Visit(context.identifier()) as Identifier;
        // TODO type
        Expression? initialValue = null;
        
        if (context.expression() is { } expression) {
            initialValue = Visit(expression) as Expression;
        }

        return new Parameter(null!, name, initialValue);
    }

    public override Node VisitConstant_declaration(RinParser.Constant_declarationContext context) {
        return new Identifier("TODO: constant decl");
    }

    public override Node VisitField_declaration(RinParser.Field_declarationContext context) {
        return new Identifier("TODO: Field Decl");
    }

    public override Node VisitLiteralExpression(RinParser.LiteralExpressionContext context) {
        return new LiteralExpression(Visit(context.literal()) as Literal);
    }

    public override Node VisitLiteral(RinParser.LiteralContext context) {
        if (context.NULL_() is not null) {
            return new Literal(null);
        }

        if (context.boolean_literal() is { } boolean) {
            return new Literal(boolean.TRUE()); // TODO: verify this
        }

        if (context.string_literal() is { } stringLiteral) {
            return new Literal(stringLiteral.GetText());
        }
        
        return new Literal("TODO: Not implemented");
    }

    public override Node VisitBlock(RinParser.BlockContext context) {
        if (context.statement_list() is { } statementList) {
            return Visit(statementList);
        }

        return new EmptyStatement();
    }
}
