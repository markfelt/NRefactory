﻿// 
// CompareBooleanWithTrueOrFalseAnalyzer.cs
// 
// Author:
//      Mansheng Yang <lightyang0@gmail.com>
// 
// Copyright (c) 2012 Mansheng Yang <lightyang0@gmail.com>
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CodeFixes;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.Text;
using System.Threading;
using System.Linq;

namespace ICSharpCode.NRefactory6.CSharp.Diagnostics
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	[NRefactoryCodeDiagnosticAnalyzer(AnalysisDisableKeyword = "RedundantBoolCompare")]
	public class RedundantBoolCompareAnalyzer : GatherVisitorDiagnosticAnalyzer
	{
		internal const string DiagnosticId  = "RedundantBoolCompareAnalyzer";
		const string Category               = DiagnosticAnalyzerCategories.RedundanciesInCode;

		static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor (DiagnosticId, "Comparison of a boolean value with 'true' or 'false' constant.", "Comparison with '{0}' is redundant", Category, DiagnosticSeverity.Warning, true, "Comparison of boolean with 'true' or 'false'");

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics {
			get {
				return ImmutableArray.Create(Rule);
			}
		}

		protected override CSharpSyntaxWalker CreateVisitor (SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, CancellationToken cancellationToken)
		{
			return new GatherVisitor(semanticModel, addDiagnostic, cancellationToken);
		}

		class GatherVisitor : GatherVisitorBase<RedundantBoolCompareAnalyzer>
		{
//			// note:this action should only check <bool> == true or <bool> != null - it needs excectly 
//			//      mimic the RedundantBoolCompare behavior otherwise it's no 1:1 mapping
//			static readonly Pattern pattern = new Choice {
//				PatternHelper.CommutativeOperatorWithOptionalParentheses(
//					new NamedNode ("const", new Choice { new PrimitiveExpression(true), new PrimitiveExpression(false) }),
//					BinaryOperatorType.Equality, new AnyNode("expr")),
//				PatternHelper.CommutativeOperatorWithOptionalParentheses(
//					new NamedNode ("const", new Choice { new PrimitiveExpression(true), new PrimitiveExpression(false) }),
//					BinaryOperatorType.InEquality, new AnyNode("expr")),
//			};
//
//			static readonly InsertParenthesesVisitor insertParenthesesVisitor = new InsertParenthesesVisitor ();

			public GatherVisitor(SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, CancellationToken cancellationToken)
				: base (semanticModel, addDiagnostic, cancellationToken)
			{
			}
//
//			public override void VisitBinaryOperatorExpression (BinaryOperatorExpression binaryOperatorExpression)
//			{
//				base.VisitBinaryOperatorExpression (binaryOperatorExpression);
//
//				var match = pattern.Match (binaryOperatorExpression);
//				if (!match.Success)
//					return;
//				var expr = match.Get<Expression> ("expr").First ();
//				// check if expr is of boolean type
//				var exprType = ctx.Resolve (expr).Type.GetDefinition ();
//				if (exprType == null || exprType.KnownTypeCode != KnownTypeCode.Boolean)
//					return;
//
//				var boolExpr = match.Get<PrimitiveExpression>("const").First();
//				var boolConstant = (bool)boolExpr.Value;
//
//				TextLocation start, end;
//				if (boolExpr == binaryOperatorExpression.Left) {
//					start = binaryOperatorExpression.StartLocation;
//					end = binaryOperatorExpression.OperatorToken.EndLocation;
//				} else {
//					start = binaryOperatorExpression.OperatorToken.StartLocation;
//					end = binaryOperatorExpression.EndLocation;
//				}
//
//				AddDiagnosticAnalyzer (new CodeIssue(
//					start, end, 
			//					boolConstant ? ctx.TranslateString ("Comparison with 'true' is redundant") : ctx.TranslateString ("Comparison with 'false' is redundant"),
			//					ctx.TranslateString ("Remove redundant comparison"), 
//					script => {
//						if ((binaryOperatorExpression.Operator == BinaryOperatorType.InEquality && boolConstant) ||
//							(binaryOperatorExpression.Operator == BinaryOperatorType.Equality && !boolConstant)) {
//							expr = new UnaryOperatorExpression (UnaryOperatorType.Not, expr.Clone());
//							expr.AcceptVisitor (insertParenthesesVisitor);
//						}
//						script.Replace (binaryOperatorExpression, expr);
//					}
//				) { IssueMarker = IssueMarker.GrayOut });
//			}
		}
	}

	[ExportCodeFixProvider(LanguageNames.CSharp), System.Composition.Shared]
	public class RedundantBoolCompareFixProvider : NRefactoryCodeFixProvider
	{
		protected override IEnumerable<string> InternalGetFixableDiagnosticIds()
		{
			yield return RedundantBoolCompareAnalyzer.DiagnosticId;
		}

		public override FixAllProvider GetFixAllProvider()
		{
			return WellKnownFixAllProviders.BatchFixer;
		}

		public async override Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var document = context.Document;
			var cancellationToken = context.CancellationToken;
			var span = context.Span;
			var diagnostics = context.Diagnostics;
			var root = await document.GetSyntaxRootAsync(cancellationToken);
			var result = new List<CodeAction>();
			var diagnostic = diagnostics.First ();
			var node = root.FindNode(context.Span);
			//if (!node.IsKind(SyntaxKind.BaseList))
			//	continue;
			var newRoot = root.RemoveNode(node, SyntaxRemoveOptions.KeepNoTrivia);
			context.RegisterCodeFix(CodeActionFactory.Create(node.Span, diagnostic.Severity, "Remove redundant comparison", document.WithSyntaxRoot(newRoot)), diagnostic);
		}
	}
}
