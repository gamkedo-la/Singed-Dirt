using System;
using System.Collections;
using System.Collections.Generic;

namespace TyVoronoi {
    public class Heap<T> {
        IComparer comparer;
        T[] heap;

        public int Count { get; private set; }

        public Heap() : this((IComparer)null) { }
        public Heap(int capacity) : this(capacity, null) { }
        public Heap(IComparer comparer) : this(16, comparer) { }
        public Heap(T[] data) : this(null, data) { }

        public Heap(IComparer comparer, T[] data) : this(data.Length, comparer) {
            for (var i=0; i<data.Length; i++) {
                Insert(data[i]);
            }
        }

        public Heap(int capacity, IComparer comparer) {
            this.comparer = (comparer == null) ? Comparer<T>.Default : comparer;
            this.heap = new T[capacity];
        }

        public void Insert(T value) {
            if (Count >= heap.Length) Array.Resize(ref heap, Count * 2);
            heap[Count] = value;
            SiftUp(Count++);
        }

        public T Extract() {
            var value = Top();
            heap[0] = heap[--Count];
            if (Count > 0) {
                SiftDown(0);
            }
            return value;
        }

        public T Replace(T value) {
            var returnValue = Top();
            heap[0] = value;
            SiftDown(0);
            return returnValue;
        }

        public T Top() {
            if (Count > 0) {
                return heap[0];
            }
            throw new InvalidOperationException("empty heap");
        }

        void SiftUp(int n) {
            var value = heap[n];
            for (var n2 = n / 2; n > 0 && comparer.Compare(value, heap[n2]) > 0; n = n2, n2 /= 2) {
                heap[n] = heap[n2];
            }
            heap[n] = value;
        }

        void SiftDown(int n) {
            var value = heap[n];
            for (var n2 = n * 2; n2 < Count; n = n2, n2 *= 2) {
                if (n2 + 1 < Count && comparer.Compare(heap[n2 + 1], heap[n2]) > 0) n2++;
                if (comparer.Compare(value, heap[n2]) >= 0) break;
                heap[n] = heap[n2];
            }
            heap[n] = value;
        }

        public delegate void ProcessNodeDelegate(T node, object data);

        public void Walk(
            ProcessNodeDelegate processFunction,
            object data
        ) {
            for (var i=0; i<Count; i++) {
                processFunction(heap[i], data);
            }
        }
    }
}
