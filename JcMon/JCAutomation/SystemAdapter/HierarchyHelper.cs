using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JCAutomation.SystemAdapter
{
	public interface IParent<T>
	{
		T Parent { get; set; }
	}

	public interface IHierarchical<T> : IParent<T>
	{
		T[] Siblings { get; set; }
		T[] Children { get; set; }
	}

	public static class HierarchyHelper
	{
		public static T GetRoot<T>(this T node) where T : class, IParent<T>
		{
			if (node == null) return null;
			var currentNode = node;
			while (currentNode.Parent != null)
			{
				currentNode = currentNode.Parent;
			}

			return currentNode;
		}

		public static TResult BuildParents<T, TResult>(T startNode, Func<T, T> getParentFunc, Func<T, TResult> selectFunc) where TResult : class, IParent<TResult>
		{
			TResult result = null;
			TResult lastResult = null;
			var currentNode = startNode;
			while (!Equals(currentNode, default(T)))
			{
				var currentResult = selectFunc(currentNode);
				if (lastResult != null)
				{
					lastResult.Parent = currentResult;
				}

				lastResult = currentResult;

				if (result == null)
				{
					result = currentResult;
				}

				currentNode = getParentFunc(currentNode);
			}

			return result;
		}

		public static TResult BuildPartialHierarchy<T, TResult>(T startNode, Func<T, T> getParentFunc, Func<T, IEnumerable<T>> getChildrenFunc,
			Func<T, TResult> selector) where TResult : class, IHierarchical<TResult>
		{
			var nodes = new Stack<T>();
			var results = new Stack<TResult>();
			var currentNode = startNode;
			while (!Equals(currentNode, default(T)))
			{
				nodes.Push(currentNode);
				var currentResult = selector(currentNode);
				if (results.Count > 0)
				{
					results.Peek().Parent = currentResult;
				}

				results.Push(currentResult);
				currentNode = getParentFunc(currentNode);
			}

			while (nodes.Count > 1)
			{
				currentNode = nodes.Pop();
				var currentResult = results.Pop();
				var childrenResult = getChildrenFunc(currentNode).Select(selector).ToArray();
				currentResult.Children = childrenResult;
				foreach (var resultChild in childrenResult)
				{
					resultChild.Siblings = childrenResult.Except(new[] {resultChild}).ToArray();
				}
			}

			return nodes.Count == 1 ? results.Pop() : null;
		}
	}
}
