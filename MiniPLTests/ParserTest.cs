using System;
using System.Text;

namespace MiniPLTests
{
    [TestClass]
    public class ParserTest
    {
        private readonly string prefix = "C:\\Users\\whitegrudov\\source\\repos\\MiniPL\\TestPrograms\\ParserTest\\";
        private readonly string validPrefix = "C:\\Users\\whitegrudov\\source\\repos\\MiniPL\\TestPrograms\\ValidPrograms\\";

        [DataRow("declaration_statements.mpl")]
        [DataRow("assignment_statements.mpl")]
        [DataRow("read_statements.mpl")]
        [DataRow("print_statements.mpl")]
        [DataRow("for_statements.mpl")]
        [DataRow("if_statements.mpl")]
        [TestMethod]
        // Check if parser builds valid ASTs
        public void Parse_ValidStatements_BuildAST(string path)
        {
            Parser parser = new(prefix + path, false);

            parser.Parse();

            Assert.IsNotNull(parser.Ast.Root.Stmts);
        }
        [DataRow("illegal_statement_start.mpl", ErrorMessage.SE_ILLEGAL_TOKEN)]
        [DataRow("missing_semicolon.mpl", ErrorMessage.SE_MISSING_SEMICOLON)]
        [DataRow("unexpected_tokens.mpl", ErrorMessage.SE_UNEXPECTED_TOKEN)]
        [DataRow("various_errors.mpl", SyntaxError.type)]
        [TestMethod]
        // Check if parser throws an error when encountering an illegal token
        public void Parse_IllegalToken_ThrowSyntaxError(string path, string error)
        {
            Parser parser = new(prefix + path, false);

            try
            {
                parser.Parse();
            }
            catch (ErrorList errorList)
            {
                foreach (var e in errorList.Errors)
                {
                    Assert.IsInstanceOfType(e, typeof(SyntaxError));
                    StringAssert.Contains(e.Message, error);
                }
            }
        }
        [DataRow("nested_operations.mpl")]
        [TestMethod]
        // Check if parser can build nested expressions correctly
        public void Parse_NestedOperations_ValidExpr(string path)
        {
            // x := ((3 * (2 + x)) - 5) * (y - (2 * x));
            Parser parser = new(prefix + path, false);

            Position dummyPos = new(0, 0);
            AssignNode expected = new AssignNode(
                    new IdentNode(new Token(TokenType.IDENTIFIER, "x", dummyPos)),
                    new LRExprNode(
                        new OpndNode(
                            new LRExprNode(
                                new OpndNode(
                                    new LRExprNode(
                                        new OpndNode(new IntNode(new Token(TokenType.INT_LITERAL, "3", dummyPos))),
                                        new OpNode(new Token(TokenType.MUL, "*", dummyPos)),
                                        new OpndNode(
                                            new LRExprNode(
                                                new OpndNode(new IntNode(new Token(TokenType.INT_LITERAL, "2", dummyPos))),
                                                new OpNode(new Token(TokenType.PLUS, "+", dummyPos)),
                                                new OpndNode(new IdentNode(new Token(TokenType.IDENTIFIER, "x", dummyPos))),
                                                dummyPos
                                                )
                                            ),
                                        dummyPos
                                        )),
                                new OpNode(new Token(TokenType.MINUS, "-", dummyPos)),
                                new OpndNode(new IntNode(new Token(TokenType.INT_LITERAL, "5", dummyPos))),
                                dummyPos
                                )
                            ),
                        new OpNode(new Token(TokenType.MUL, "*", dummyPos)),
                        new OpndNode(
                            new LRExprNode(
                                new OpndNode(new IdentNode(new Token(TokenType.IDENTIFIER, "y", dummyPos))),
                                new OpNode(new Token(TokenType.MINUS, "-", dummyPos)),
                                new OpndNode(
                                    new LRExprNode(
                                        new OpndNode(new IntNode(new Token(TokenType.INT_LITERAL, "2", dummyPos))),
                                        new OpNode(new Token(TokenType.MUL, "*", dummyPos)),
                                        new OpndNode(new IdentNode(new Token(TokenType.IDENTIFIER, "x", dummyPos))),
                                        dummyPos
                                        )),
                                dummyPos
                                )
                            ),
                        dummyPos
                        )
                );
            parser.Parse();
            Assert.IsNotNull(parser.Ast.Root.Stmts);
            CompareTrees(parser.Ast.Root.Stmts.StmtNodes[0], expected);
        }
        [DataRow("valid_ast.mpl")]
        [TestMethod]
        // Check if parser builds vaild AST for a given program
        public void Parse_TestProgram_ValidAST(string path)
        {
            /*
             * var i : int := 1;
             * i := 2;
             * read i;
             * for i in 1..10 do
             *   if i > 10 do
             *     print i;
             *   end if;
             * end for;*/
            Parser parser = new(prefix + path, false);

            Position dummyPos = new(0, 0);
            StmtsNode expected = new();
            expected.AddChild(new DeclNode(
                new IdentNode(new Token(TokenType.IDENTIFIER, "i", dummyPos)),
                new TypeNode(new Token(TokenType.INT, "int", dummyPos)),
                new LExprNode(new OpndNode(new IntNode(new Token(TokenType.INT_LITERAL, "1", dummyPos))), dummyPos)
                ));
            expected.AddChild(new AssignNode(
                new IdentNode(new Token(TokenType.IDENTIFIER, "i", dummyPos)),
                new LExprNode(new OpndNode(new IntNode(new Token(TokenType.INT_LITERAL, "2", dummyPos))), dummyPos)
                ));
            expected.AddChild(new ReadNode(new IdentNode(new Token(TokenType.IDENTIFIER, "i", dummyPos))));

            StmtsNode forStmts = new();
            StmtsNode ifStmts = new();
            ifStmts.AddChild(new PrintNode(new LExprNode(
                new OpndNode(new IdentNode(new Token(TokenType.IDENTIFIER, "i", dummyPos))), dummyPos
                )));
            forStmts.AddChild(new IfNode(
                new LRExprNode(
                    new OpndNode(new IdentNode(new Token(TokenType.IDENTIFIER, "i", dummyPos))),
                    new OpNode(new Token(TokenType.GT, ">", dummyPos)),
                    new OpndNode(new IntNode(new Token(TokenType.INT_LITERAL, "10", dummyPos))),
                    dummyPos
                    ),
                ifStmts
                ));
            expected.AddChild(new ForNode(
                new IdentNode(new Token(TokenType.IDENTIFIER, "i", dummyPos)),
                new LExprNode(new OpndNode(new IntNode(new Token(TokenType.INT_LITERAL, "1", dummyPos))), dummyPos),
                new LExprNode(new OpndNode(new IntNode(new Token(TokenType.INT_LITERAL, "10", dummyPos))), dummyPos),
                forStmts
                ));

            parser.Parse();
            Assert.IsNotNull(parser.Ast.Root.Stmts);
            CompareTrees(parser.Ast.Root.Stmts, expected);
        }
        // Util method for comparing ASTs
        private void CompareTrees(INode expected, INode generated)
        {
            Assert.AreEqual(expected.GetType(), generated.GetType());

            var expectedChildren = expected.GetAllChildren();
            var generatedChildren = generated.GetAllChildren();
            Assert.AreEqual(expectedChildren.Count, generatedChildren.Count);

            if (expectedChildren.Count == 0)
            {
                var expectedToken = ((TokenNode)expected).Token;
                var generatedToken = ((TokenNode)generated).Token;

                Assert.AreEqual(expectedToken.Type, generatedToken.Type);
                Assert.AreEqual(expectedToken.Value, generatedToken.Value);

                return;
            }

            var bothNodes = expectedChildren.Zip(generatedChildren, (e, g) => new { Expected = e, Generated = g });
            foreach (var eg in bothNodes)
            {
                CompareTrees(eg.Expected, eg.Generated);
            }
        }
        [DataRow("1.mpl")]
        [DataRow("2.mpl")]
        [DataRow("3.mpl")]
        [DataRow("4.mpl")]
        [DataRow("5.mpl")]
        [TestMethod]
        // Check if parser builds valid ASTs with valid programs
        public void Parse_ValidPrograms_BuildAST(string path)
        {
            Parser parser = new(validPrefix + path, false);

            parser.Parse();

            Assert.IsNotNull(parser.Ast.Root.Stmts);
        }
    }
}
