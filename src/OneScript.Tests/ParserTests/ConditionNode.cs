using OneScript.Language;

namespace OneScript.Tests
{
    class ConditionNode : TestASTNodeBase, IASTConditionNode
    {
        private IASTNode _condition;

        TestASTNodeBase _truePart;
        TestASTNodeBase _falsePart;

        protected override bool EqualsInternal(IASTNode other)
        {
            var otherConditionNode = other as ConditionNode;
            var expression = (TestASTNodeBase)(otherConditionNode._condition);
            return (expression.Equals(_condition)
            && _truePart.Equals(otherConditionNode._truePart)
            && _falsePart.Equals(otherConditionNode._falsePart));
        }

        public IASTNode Condition
        {
            get
            {
                return _condition;
            }
            set
            {
                _condition = value;
            }
        }

        public IASTNode TruePart
        {
            get
            {
                return _truePart;
            }
            set
            {
                _truePart = (TestASTNodeBase)value;
            }
        }

        public IASTNode FalsePart
        {
            get
            {
                return _falsePart;
            }
            set
            {
                _falsePart = (TestASTNodeBase)value;
            }
        }
    }
}