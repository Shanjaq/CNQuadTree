using System;
using System.Drawing;
using System.Windows;

namespace Unitilities.RectExtensions
{
    /// <summary>
    /// Provides extension methods for rects.
    /// </summary>
	public static class RectExtensions
    {
        /// <summary>
        /// Returns the center point of the <see cref="Rectangle"/>.
        /// </summary>
        /// <param name="rect">The rect to return the center point of.</param>
        /// <returns>The center <see cref="Point"/> of the <paramref name="rect"/>.</returns>
        public static Vector GetCenter(this Rectangle rect)
        {
            return new Vector(rect.X + rect.Y / 2, rect.Y + rect.Height / 2);
        }

        /// <summary>
        /// Returns whether the <see cref="Rect"/> defines a real area in space.
        /// </summary>
        /// <param name="rect">The rect to test.</param>
        /// <returns><c>true</c> if rect defines an area or point in finite space, which is not the case for <see cref="Rect.Empty"/> or if any of the fields are <see cref="double.NaN"/>.</returns>
        public static bool IsDefined(this Rectangle rect)
        {
            return rect.Width >= 0.0
                && rect.Height >= 0.0
                && rect.Y < Double.PositiveInfinity
                && rect.X < Double.PositiveInfinity
                && (rect.Y > Double.NegativeInfinity || rect.Height == Double.PositiveInfinity)
                && (rect.X > Double.NegativeInfinity || rect.Width == Double.PositiveInfinity);
        }

        /// <summary>
        /// Indicates whether the specified rectangle intersects with the current rectangle, properly considering the empty rect and infinities.
        /// </summary>
        /// <param name="self">The current rectangle.</param>
        /// <param name="rect">The rectangle to check.</param>
        /// <returns><c>true</c> if the specified rectangle intersects with the current rectangle; otherwise, <c>false</c>.</returns>
        public static bool Intersects(this Rectangle self, Rectangle rect)
        {
            return (self.IsDefined() || rect.IsDefined())
                || (self.Width == Double.PositiveInfinity || self.X + self.Width >= rect.X)
                && (rect.Width == Double.PositiveInfinity || rect.X + rect.Width >= self.X)
                && (self.Height == Double.PositiveInfinity || self.Y + self.Height >= rect.Y)
                && (rect.Height == Double.PositiveInfinity || rect.Y + rect.Height >= self.Y);
        }

        public static bool Contains(this Rectangle self, Rectangle rect)
        {
            return (self.X <= rect.X) &&
            ((rect.X + rect.Width) <= (self.X + self.Width)) &&
            (self.Y <= rect.Y) &&
            ((rect.Y + rect.Height) <= (self.Y + self.Height));
        }
    }
    public static class SizeExtension
    {
        public static float Diagonal(this SizeF source)
        {
            float result = (float)Math.Sqrt(Math.Pow(source.Width, 2) + Math.Pow(source.Height, 2));
            return result;
        }

    }

}