using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
namespace System.Collections.Generic
{
    public partial class CNQuadTree<T>
    {
        /// <summary>
        /// provides a persistent index into the quadtree
        /// subdivides intersecting quadrants to a desired resolution
        /// raises events on transitioning to a new quadrant
        /// </summary>
        public class Probe
        {
            private RectangleF _bounds;
            /// <summary>
            /// probe bounds
            /// </summary>
            public RectangleF Bounds
            {
                get { return _bounds; }
                set
                {
                    _bounds = value;
                    _bounds.X = _position.X - (_bounds.Width / 2);
                    _bounds.Y = _position.Y - (_bounds.Height / 2);
                    //_bounds.Location = new PointF(_position.X - (_bounds.Size.Width / 2), _position.X - (_bounds.Size.Width / 2));
                }
            }

            private float _domainRadius = 0.0f;
            /// <summary>
            /// probe domain radius beyond bounds
            /// </summary>
            public float DomainRadius {
                get { return _domainRadius; }
                set {
                    _domainRadius = value;
                    _domain = new RectangleF(
                        //new PointF(_position.X - (((_bounds.Width + (_domainRadius * 2)) - _bounds.Width) / 2), _position.Y - (((_bounds.Height + (_domainRadius * 2)) - _bounds.Height) / 2)),
                        new PointF(_position.X - ((_bounds.Width / 2) + _domainRadius), _position.Y - ((_bounds.Height / 2) + _domainRadius)),
                        new SizeF((_bounds.Width + (_domainRadius * 2)), (_bounds.Height + (_domainRadius * 2)))
                    );
                }
            }

            private RectangleF _domain;
            /// <summary>
            /// probe domain of intersecting quads individually larger than bounds
            /// </summary>
            public RectangleF Domain {
                get { return _domain; }
                private set { _domain = value; }
            }

            private PointF _position;
            /// <summary>
            /// probe position (center of bounds)
            /// </summary>
            public PointF Position
            {
                get { return _position; }
                set
                {
                    _position = value;
                    //_bounds.Location = new PointF((float)(_position.X) + (_bounds.Size.Width / 2), (float)(_position.Y) + (_bounds.Size.Width / 2));
                    _bounds.X = _position.X - (_bounds.Width / 2);
                    _bounds.Y = _position.Y - (_bounds.Height / 2);
                    //_domain.Location = new PointF(_position.X - (((_bounds.Width + (_domainRadius * 2)) - _bounds.Width) / 2), _position.Y - (((_bounds.Height + (_domainRadius * 2)) - _bounds.Height) / 2));
                    _domain.X = _position.X - ((_bounds.Width / 2) + _domainRadius);
                    _domain.Y = _position.Y - ((_bounds.Height / 2) + _domainRadius);
                    //trigger events on quadrant change?
                }
            }

            public Probe()
            {

            }

            public Probe(PointF location, SizeF size)
            {
                _position = location;
                _bounds = new RectangleF(new PointF(location.X - (size.Width / 2.0f), location.Y - (size.Height / 2.0f)), size);
            }


            public void UpdateLocation(PointF location, T obj)
            {
                
            }

            #region PUBLIC_EVENTS
            public event PropertyChangedEventHandler PropertyChangedEvent;
            #endregion

            #region CUSTOM_EVENT_HANDLER_CLASSES
            public class PropertyChangedEventArgs : EventArgs
            {
                private List<RectangleF> _rects;

                public PropertyChangedEventArgs(List<RectangleF> rects)
                {
                    this._rects = rects;
                }


                public List<RectangleF> Rects
                {
                    [MethodImpl(MethodImplOptions.Synchronized)]
                    get
                    {
                        return _rects;
                    }
                }
            }
            public delegate void PropertyChangedEventHandler(object sender, PropertyChangedEventArgs e);
            #endregion

        }

        public List<Probe> Probes
        {
            get
            {
                return _probes;
            }
            private set
            {
                _probes = value;
            }
        }
        private List<Probe> _probes = new List<Probe>();
    }
}
