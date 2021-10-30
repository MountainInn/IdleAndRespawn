using System;
using System.Collections.Generic;

class ProcessingChainNamespace
{
    abstract public class ProcessingNode<T>
    {
        abstract public T Process(T input); 
    }

    public class ProcessingNodeFunc<T> : ProcessingNode<T>
    {
        Func<T, T> node;

        public ProcessingNodeFunc(Func<T, T> func)
        {
            node = func;
        }

        public override T Process(T input)
        {
            return node.Invoke(input);
        }
    }

    public class ProcessingNodeChain<T> : ProcessingNode<T>
    {
        protected List<Func<T, T>> chain;

        public ProcessingNodeChain(List<Func<T, T>> chain)
        {
            this.chain = chain;
        }

        public override T Process(T input)
        {
            T result = input;

            foreach (var item in chain)
            {
                result = item.Invoke(result);
            }

            return result;
        }
    }

    public class ProcessingChain<T>
    {
        List<ProcessingNode<T>> chain;

        ProcessingNode<T> lastNode;

    

        public ProcessingChain(List<ProcessingNode<T>> chain, ProcessingNode<T> lastNode)
        {
            this.chain = chain;
            this.lastNode = lastNode;
        }

        public T Process(T input)
        {
            T result = input;

            foreach (var node in chain) { result = node.Process(result); }

            result = lastNode.Process(result);
        
            return result;
        }
    }

}
