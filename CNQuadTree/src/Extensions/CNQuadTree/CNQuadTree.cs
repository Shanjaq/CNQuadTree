using System;
using Unitilities.RectExtensions;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using IterTools;
using System.Drawing;
using System.Windows;
//using CNQuadTreeTest2;

namespace System.Collections.Generic
{
    public partial class CNQuadTree<T>
    {
        public enum NodeChangeTypes { Add, Remove }
        public class QuadNode
        {
            private T _node; // the actual object being stored here.
            public T Node { get { return _node; } set { _node = value; } }
            public RectangleF Bounds { get; set; }

            public QuadNode()
            {
            }
            public QuadNode(T node)
            {
                _node = node;
            }

        }

        /// <summary>
        /// The MaxTreeDepth limit is required since recursive calls can go that deep if item bounds (height or width) are very small compared to Extent (height or width).
        /// The max depth will prevent stack overflow exception in some of the recursive calls we make.
        /// With a value of 50 the item bounds can be 2^-50 times the extent before the tree stops growing in height.
        /// </summary>
        private const int MaxTreeDepth = 50;

        /// <summary>
        /// Changing bounds is expensive since it has to re-divide the entire thing around all probes
        /// </summary>
        public RectangleF Extent
        {
            get
            {
                return _extent;
            }
            set
            {
                _extent = value;
                _root = new Quadrant(_extent, null, 0);

            }
        }
        private RectangleF _extent = new RectangleF(new PointF(0.0f, 0.0f), new SizeF(1.0f, 1.0f));

        public CNQuadTree()
        {
            _root = new Quadrant(this.Extent, null, 0);
        }

        public CNQuadTree(RectangleF extent)
        {
            _root = new Quadrant(extent, null, 0);
        }

        /// <summary>
        /// Removes all quadrants from the tree.
        /// </summary>
        public void Clear()
        {
            _root = new Quadrant(this.Extent, null, 0);
        }


        public void UpdateProbes()
        {
            List<Tuple<QuadNode, NodeChangeTypes>> nodesModified = new List<Tuple<QuadNode, NodeChangeTypes>>();

            Stack<Quadrant> remove_quads = new Stack<Quadrant>();

            Queue<Quadrant> quads = new Queue<Quadrant>();
            quads.Enqueue(_root);

            while (quads.Count > 0)
            {
                var enumerator = quads.Dequeue();
                bool shouldSplit = false;

                shouldSplit = (_probes.Where(probe => ((enumerator.Bounds.IntersectsWith(probe.Domain) || (enumerator.Bounds.Contains(probe.Bounds))) && (enumerator.Bounds.Size.Diagonal() > (probe.Bounds.Size.Diagonal() * 2)))).Count() > 0);

                if (shouldSplit)
                {
                    if (enumerator.Children != null)
                        enumerator.Children.ToList().ForEach(x =>
                        {
                            quads.Enqueue(x);
                        });
                    else
                        enumerator.Split().ToList().ForEach(x =>
                        {
                            x.Node.Bounds = x.Bounds;
                            quads.Enqueue(x);
                            nodesModified.Add(new Tuple<QuadNode, NodeChangeTypes>(x.Node, NodeChangeTypes.Add));
                        });

                } else {
                    if (enumerator.Children != null)
                    {
                        enumerator.Children.ToList().ForEach(x =>
                        {
                            quads.Enqueue(x);
                            if (x.Commit)
                                nodesModified.Add(new Tuple<QuadNode, NodeChangeTypes>(x.Node, NodeChangeTypes.Remove));
                        });
                        remove_quads.Push(enumerator);
                    }
                }
                _probes.Where(probe => enumerator.Bounds.Contains(probe.Position))
                    .ToList().ForEach(probe =>
                    {
                        if (_probes_quads.ContainsKey(probe))
                        {
                            if ((enumerator.Bounds.Width == _probes_quads[probe].Bounds.Width) && (_probes_quads[probe] != enumerator))
                                SetQuad(probe, enumerator);
                            else if (enumerator.Bounds.Width < _probes_quads[probe].Bounds.Width)
                                SetQuad(probe, enumerator);
                        }
                        else
                            SetQuad(probe, enumerator);
                    }
                );
                enumerator.Commit = true;
            }
            while (remove_quads.Count > 0)
            {
                remove_quads.Pop().Merge();
            }
            if (nodesModified.Count > 0)
                NodeChangedEvent(this, new NodeChangedEventArgs(nodesModified));
        }

        #region PUBLIC_EVENTS
        public event NodeChangedEventHandler NodeChangedEvent;
        #endregion

        #region CUSTOM_EVENT_HANDLER_CLASSES
        public class NodeChangedEventArgs : EventArgs
        {
            private List<Tuple<QuadNode, NodeChangeTypes>> _nodesModified;
            public NodeChangedEventArgs(List<Tuple<QuadNode, NodeChangeTypes>> nodesModified)
            {
                this._nodesModified = nodesModified;
            }

            public List<Tuple<QuadNode, NodeChangeTypes>> NodesModified
            {
                [MethodImpl(MethodImplOptions.Synchronized)]
                get
                {
                    return _nodesModified;
                }
            }
        }
        public delegate void NodeChangedEventHandler(object sender, NodeChangedEventArgs e);
        #endregion
        private Dictionary<Probe, Quadrant> _probes_quads = new Dictionary<Probe, Quadrant>();

        //public Dictionary<Probe, QuadNode> ProbesQuads { get; set; }

        public QuadNode GetQuad(Probe probe, System.Drawing.PointF targetPos = new System.Drawing.PointF(), int neighbor = -1)
        {
            Quadrant quad;

            if (_probes_quads.TryGetValue(probe, out quad))
            {
                if (neighbor >= 0)
                {
                    if (quad.Neighbors[neighbor] != null)
                    {
                        return quad.Neighbors[neighbor].Node;
                    }
                    else
                    {
                        return null;
                    }
                }

                if ((new Vector() { X = targetPos.X, Y = targetPos.Y }).Length == 0.0)
                {
                    return quad.Node;
                }
                else
                {
                    Queue<Quadrant> quads = new Queue<Quadrant>();

                    Quadrant enumerator = quad;

                    bool done = false;
                    while (!done)
                    {
                        if (enumerator.Bounds.Contains(targetPos))
                        {
                            //quads.Enqueue(enumerator);
                            done = true;
                        }
                        else
                        {
                            //quads.Enqueue(enumerator);
                            PointF tmp_offset = System.Drawing.PointF.Subtract(targetPos, new System.Drawing.SizeF(enumerator.Bounds.Location)-new SizeF(enumerator.Bounds.Height, enumerator.Bounds.Height));
                            double distance = Math.Sqrt((tmp_offset.X * tmp_offset.X) + (tmp_offset.Y * tmp_offset.Y));
                            System.Drawing.PointF normalized = new System.Drawing.PointF((float)(tmp_offset.X / distance), (float)(tmp_offset.Y / distance));

                            if (Math.Abs(normalized.X) >= Math.Abs(normalized.Y))
                            {
                                if (normalized.X >= 0)
                                {
                                    if (enumerator.Neighbors[2] != null)
                                    {
                                        enumerator = enumerator.Neighbors[2];
                                    }
                                    else
                                    {
                                        done = true;
                                    }
                                }
                                else
                                {
                                    if (enumerator.Neighbors[0] != null)
                                    {
                                        enumerator = enumerator.Neighbors[0];
                                    }
                                    else
                                    {
                                        done = true;
                                    }
                                }
                            }
                            else
                            {
                                if (normalized.Y >= 0)
                                {
                                    if (enumerator.Neighbors[1] != null)
                                    {
                                        enumerator = enumerator.Neighbors[1];
                                    }
                                    else
                                    {
                                        done = true;
                                    }
                                }
                                else
                                {
                                    if (enumerator.Neighbors[3] != null)
                                    {
                                        enumerator = enumerator.Neighbors[3];
                                    }
                                    else
                                    {
                                        done = true;
                                    }
                                }
                            }
                        }
                    }
                    return enumerator.Node;
                }
            }
            else
            {
                return null;
            }
        }

        private void SetQuad(Probe probe, Quadrant quad)
        {
            _probes_quads[probe] = quad;
        }


    }
}