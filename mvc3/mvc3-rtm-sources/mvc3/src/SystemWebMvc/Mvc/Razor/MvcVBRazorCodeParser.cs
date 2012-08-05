namespace System.Web.Mvc.Razor {
    using System.Globalization;
    using System.Web.Mvc.Resources;
    using System.Web.Razor.Parser;
    using System.Web.Razor.Parser.SyntaxTree;
    using System.Web.Razor.Text;

    public class MvcVBRazorCodeParser : VBCodeParser {
        private const string ModelTypeKeyword = "ModelType";
        private SourceLocation? _endInheritsLocation;
        private bool _modelStatementFound;

        public MvcVBRazorCodeParser() {
            KeywordHandlers.Add(ModelTypeKeyword, ParseModelStatement);
        }

        protected override bool ParseInheritsStatement(CodeBlockInfo block) {
            _endInheritsLocation = CurrentLocation;
            bool result = base.ParseInheritsStatement(block);
            CheckForInheritsAndModelStatements();
            return result;
        }

        private void CheckForInheritsAndModelStatements() {
            if (_modelStatementFound && _endInheritsLocation.HasValue) {
                OnError(_endInheritsLocation.Value, String.Format(CultureInfo.CurrentCulture, MvcResources.MvcRazorCodeParser_CannotHaveModelAndInheritsKeyword, ModelTypeKeyword));
            }
        }

        private bool ParseModelStatement(CodeBlockInfo block) {
            using (StartBlock(BlockType.Directive)) {
                block.ResumeSpans(Context);

                SourceLocation endModelLocation = CurrentLocation;
                bool readWhitespace = RequireSingleWhiteSpace();

                End(MetaCodeSpan.Create(Context, hidden: false, acceptedCharacters: readWhitespace ? AcceptedCharacters.None : AcceptedCharacters.Any));

                if (_modelStatementFound) {
                    OnError(endModelLocation, String.Format(CultureInfo.CurrentCulture, MvcResources.MvcRazorCodeParser_OnlyOneModelStatementIsAllowed, ModelTypeKeyword));
                }

                _modelStatementFound = true;

                // Accept Whitespace up to the new line or non-whitespace character
                Context.AcceptWhiteSpace(includeNewLines: false);

                string typeName = null;
                if (ParserHelpers.IsIdentifierStart(CurrentCharacter)) {
                    using (Context.StartTemporaryBuffer()) {
                        Context.AcceptUntil(c => ParserHelpers.IsNewLine(c));
                        typeName = Context.ContentBuffer.ToString();
                        Context.AcceptTemporaryBuffer();
                    }
                    Context.AcceptNewLine();
                }
                else {
                    OnError(endModelLocation, String.Format(CultureInfo.CurrentCulture, MvcResources.MvcRazorCodeParser_ModelKeywordMustBeFollowedByTypeName, ModelTypeKeyword));
                }
                CheckForInheritsAndModelStatements();
                End(new ModelSpan(Context, typeName));
            }
            return false;
        }
    }
}
