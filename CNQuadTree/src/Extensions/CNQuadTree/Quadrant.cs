using System;
using System.Collections.Generic;
using Unitilities.RectExtensions;
using System.Linq;
using System.Text;
using System.Drawing;
//using CNQuadTreeTest2;

namespace System.Collections.Generic
{
    public partial class CNQuadTree<T>
    {
        private class Quadrant
        {
            public bool Commit = false;

            private Quadrant _parent;
            public Quadrant Parent { get { return _parent; } }

            private Quadrant[] _children = null;
            public Quadrant[] Children { get { return _children; } set { _children = value; } }

            private Quadrant[] _neighbors = { null, null, null, null };
            public Quadrant[] Neighbors { get { return _neighbors; } }

            private int _location;
            public int Location { get { return _location; } }

            private readonly RectangleF _bounds;
            public RectangleF Bounds { get { return _bounds; } }

            private QuadNode _node = new QuadNode();
            public QuadNode Node
            {
                get
                {
                    return _node;
                }
            }


            public Quadrant(RectangleF bounds, Quadrant parent, int location)
            {
                _parent = parent;
                _bounds = bounds;
                _location = location;
            }

            public IEnumerable<Quadrant> Split()
            {
                float w = _bounds.Width / 2;
                float h = _bounds.Height / 2;

                if (_children == null)
                {
                    Func<Quadrant, int, Quadrant> drilldown = null;
                    drilldown = (quadrant, position) => {
                        return (((quadrant != null) && (quadrant._children[position] != null)) ? drilldown(quadrant._children[position], position) : quadrant);
                    };

                    _children = new Quadrant[] { null, null, null, null };

                    RectangleF[] quad_rects = {
                                new RectangleF(_bounds.Left, _bounds.Top, w, h),
                                new RectangleF(_bounds.Left + w, _bounds.Top, w, h),
                                new RectangleF(_bounds.Left + w, _bounds.Top + h, w, h),
                                new RectangleF(_bounds.Left, _bounds.Top + h, w, h)
                            };

                    //iterate in clockwise order from 0, NW
                    for (int i = 0; i < quad_rects.Count(); i++)
                    {
                        _children[i] = new Quadrant(quad_rects[i], this, i) { _neighbors = this._neighbors.ToArray() }; //copy parent neighbors to children
                    }


                    //inner connections
                    for (int i = 0; i < 4; i++)
                    {
                        for (int j = 0; j < 2; j++)
                        {
                            _children[i]._neighbors[((i + 2) + j) % 4] = _children[((i + 1) + (j * 2)) % 4];
                        }
                    }

                    //outer connections: top right
                    while ((_children[1]._neighbors[1] != null) && (_children[1]._neighbors[1].Bounds.Right < _children[1]._bounds.Right))
                    {
                        if (_children[1]._neighbors[1].Bounds.Right <= _children[1].Bounds.Left)
                            _children[1]._neighbors[1]._neighbors[3] = _children[0];
                        else
                            _children[1]._neighbors[1]._neighbors[3] = _children[1];

                        _children[1]._neighbors[1] = _children[1]._neighbors[1]._neighbors[2]; //iterate right
                    }

                    //outer connections: bottom left
                    while ((_children[3]._neighbors[0] != null) && (_children[3]._neighbors[0].Bounds.Bottom < _children[3]._bounds.Bottom))
                    {
                        if (_children[3]._neighbors[0].Bounds.Bottom <= _children[3].Bounds.Top)
                            _children[3]._neighbors[0]._neighbors[2] = _children[0];
                        else
                            _children[3]._neighbors[0]._neighbors[2] = _children[3];

                        _children[3]._neighbors[0] = _children[3]._neighbors[0]._neighbors[3]; //iterate down
                    }

                    //outer connections: right
                    if (_neighbors[2] != null)
                        _neighbors[2]._neighbors[0] = _children[1];

                    //outer connections: bottom
                    if (_neighbors[3] != null)
                        _neighbors[3]._neighbors[1] = _children[3];
                    
                }

                return _children;
            }

            public void Merge()
            {
                //update neighbors in clockwise order from 0, NW
                
                for (int i = 0; i < 4; i++)
                {
                    for (int j = i; j < (i + 2); j++)
                    {
                        Quadrant neighbor = _children[i]._neighbors[j % 4];
                        if (neighbor != null)
                        {
                            neighbor._neighbors[(j + 2) % 4] = this;
                        }
                    }
                    if (_children[i]._children != null)
                        _children[i].Merge();
                }
                
                _children = null;
            }

            /// <summary>
            /// Returns all quadrants that intersect the given bounds.
            /// The quadrants are returned in order of descending size.
            /// </summary>
            /// <param name="bounds">The bounds that intersects the quadrants you want returned.</param>
            /// <returns>A lazy list of quadrants.</returns>
            internal IEnumerable<Quadrant> GetQuadrantsIntersecting(RectangleF bounds)
            {
                float w = _bounds.Width / 2;
                float h = _bounds.Height / 2;

                RectangleF[] quad_rects = {
                                new RectangleF(_bounds.Left, _bounds.Top, w, h),
                                new RectangleF(_bounds.Left + w, _bounds.Top, w, h),
                                new RectangleF(_bounds.Left + w, _bounds.Top + h, w, h),
                                new RectangleF(_bounds.Left, _bounds.Top + h, w, h)
                            };

                var queue = new Queue<IEnumerator<Quadrant>>();

                for (int i = 0; i < quad_rects.Count(); i++)
                {
                    if (_children[i] != null && quad_rects[i].IntersectsWith(bounds))
                    {
                        queue.Enqueue(_children[i].GetQuadrantsIntersecting(bounds).GetEnumerator());
                    }
                }

                while (queue.Count > 0)
                {
                    var enumerator = queue.Dequeue();
                    if (enumerator.MoveNext())
                    {
                        var current = enumerator.Current;
                        yield return current;
                    }
                }
            }

            internal IEnumerable<Quadrant> GetQuadrantsContainedBy(RectangleF bounds)
            {
                float w = _bounds.Width / 2;
                float h = _bounds.Height / 2;

                RectangleF[] quad_rects = {
                                new RectangleF(_bounds.Left, _bounds.Top, w, h),
                                new RectangleF(_bounds.Left + w, _bounds.Top, w, h),
                                new RectangleF(_bounds.Left + w, _bounds.Top + h, w, h),
                                new RectangleF(_bounds.Left, _bounds.Top + h, w, h)
                            };

                var queue = new Queue<IEnumerator<Quadrant>>();

                for (int i = 0; i < quad_rects.Count(); i++)
                {
                    if (_children[i] != null && quad_rects[i].Contains(bounds))
                    {
                        queue.Enqueue(_children[i].GetQuadrantsContainedBy(bounds).GetEnumerator());
                    }
                }

                while (queue.Count > 0)
                {
                    var enumerator = queue.Dequeue();
                    if (enumerator.MoveNext())
                    {
                        var current = enumerator.Current;
                        yield return current;
                    }
                }
            }
        }

        /// <summary>
        /// The outer CNQuadTree class is essentially just a wrapper around a tree of Quadrants.
        /// </summary>
        private Quadrant _root;
    }
}
