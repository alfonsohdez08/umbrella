//using System;
//using System.Collections.Generic;
//using System.Linq.Expressions;
//using System.Text;

//namespace Umbrella.Expr.Rewritters
//{
//    internal class MemberAccessProjectedRewritter : ExpressionRewritter
//    {
//        public override Expression Rewrite(Expression expression)
//        {
//            var lambda = (LambdaExpression)expression;
//            if (lambda.Body.NodeType == ExpressionType.MemberAccess)
//                Visit(lambda.Body);

//            throw new NotImplementedException();
//        }
//    }

//    internal class MemberAccessProjection: ExpressionVisitor
//    {
//        private bool _foundNewOperator = false;
        
        

//        //public bool IsAMemberAccessBasedProjection(LambdaExpression lambda)
//        //{
//        //    bool foundNewOperator = false;

//        //    try
//        //    {
//        //        Visit(lambda.Body);
//        //        foundNewOperator = _foundNewOperator;

//        //    }
//        //    finally
//        //    {
//        //        _foundNewOperator = false;
//        //    }

//        //    return foundNewOperator;
//        //}

//        //protected override Expression VisitNew(NewExpression n)
//        //{
//        //    _foundNewOperator = true;

//        //    return base.Visit(n);
//        //}
//    }
//}