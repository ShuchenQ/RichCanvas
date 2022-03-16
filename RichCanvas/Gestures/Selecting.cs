﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace RichCanvas.Gestures
{
    internal class Selecting
    {
        private Point _selectionRectangleInitialPosition;
        private readonly RichItemsControl _context;
        private readonly List<RichItemContainer> _selectedContainers = new List<RichItemContainer>(16);

        public Selecting(RichItemsControl context)
        {
            _context = context;
            _context.AddHandler(RichItemContainer.DragStartedEvent, new DragStartedEventHandler(OnItemsDragStarted));
            _context.AddHandler(RichItemContainer.DragCompletedEvent, new DragCompletedEventHandler(OnItemsDragCompleted));
            _context.AddHandler(RichItemContainer.DragDeltaEvent, new DragDeltaEventHandler(OnItemsDragDelta));
        }

        private void OnItemsDragCompleted(object sender, DragCompletedEventArgs e)
        {
            _context.Cursor = Cursors.Arrow;
            if (_selectedContainers.Count > 0)
            {
                for (var i = 0; i < _selectedContainers.Count; i++)
                {
                    RichItemContainer container = _selectedContainers[i];
                    var translateTransform = container.TranslateTransform;

                    container.Left += translateTransform.X;
                    container.Top += translateTransform.Y;

                    // Correct the final position
                    if (_context.EnableSnapping)
                    {
                        container.Left = Math.Round(container.Left / _context.GridSpacing) * _context.GridSpacing;
                        container.Top = Math.Round(container.Top / _context.GridSpacing) * _context.GridSpacing;
                    }

                    translateTransform.X = 0;
                    translateTransform.Y = 0;
                }

                _selectedContainers.Clear();
            }
        }

        private void OnItemsDragStarted(object sender, DragStartedEventArgs e)
        {
            IList selectedItems = _context.BaseSelectedItems;

            if (selectedItems.Count > 0)
            {
                // Make sure we're not adding to a previous selection
                if (_selectedContainers.Count > 0)
                {
                    _selectedContainers.Clear();
                }

                // Increase cache capacity
                if (_selectedContainers.Capacity < selectedItems.Count)
                {
                    _selectedContainers.Capacity = selectedItems.Count;
                }

                // Cache selected containers
                for (var i = 0; i < selectedItems.Count; i++)
                {
                    var container = (RichItemContainer)_context.ItemContainerGenerator.ContainerFromItem(selectedItems[i]);
                    if (container.IsDraggable)
                    {
                        _selectedContainers.Add(container);
                    }
                }

                _context.ItemsHost.InvalidateArrange();
                _context.ScrollContainer.SetCurrentScroll();
                e.Handled = true;
            }
        }

        private void OnItemsDragDelta(object sender, DragDeltaEventArgs e)
        {
            double minX = double.MaxValue;
            double minY = double.MaxValue;
            double maxX = double.MinValue;
            double maxY = double.MinValue;
            if (e.HorizontalChange != 0 || e.VerticalChange != 0)
            {
                for (int i = 0; i < _selectedContainers.Count; i++)
                {
                    RichItemContainer container = _selectedContainers[i];
                    TranslateTransform translateTransform = container.TranslateTransform;

                    if (translateTransform != null)
                    {
                        translateTransform.X += e.HorizontalChange;
                        translateTransform.Y += e.VerticalChange;
                        container.CalculateBoundingBox();
                    }
                    minX = Math.Min(minX, container.BoundingBox.Left);
                    minY = Math.Min(minY, container.BoundingBox.Top);
                    maxX = Math.Max(maxX, container.BoundingBox.Right);
                    maxY = Math.Max(maxY, container.BoundingBox.Bottom);

                    container.OnPreviewLocationChanged(new Point(container.Left + translateTransform.X, container.Top + translateTransform.Y));
                }
                _context.ItemsHost.TopLimit = minY;
                _context.ItemsHost.LeftLimit = minX;
                _context.ItemsHost.BottomLimit = maxY;
                _context.ItemsHost.RightLimit = maxX;

                _context.ScrollContainer.SetCurrentScroll();
            }
        }

        internal void Update(Point endLocation)
        {
            double width = Math.Abs(endLocation.X - _selectionRectangleInitialPosition.X);
            double height = Math.Abs(endLocation.Y - _selectionRectangleInitialPosition.Y);
            _context.SelectionRectangle = new Rect(_selectionRectangleInitialPosition.X, _selectionRectangleInitialPosition.Y, width, height);
        }

        internal void OnMouseDown(Point position) => _selectionRectangleInitialPosition = position;

        internal void OnMouseMove(Point position)
        {
            TransformGroup transformGroup = _context.SelectionRectangleTransform;
            var scaleTransform = (ScaleTransform)transformGroup.Children[0];

            double width = position.X - _selectionRectangleInitialPosition.X;
            double height = position.Y - _selectionRectangleInitialPosition.Y;

            if (width < 0 && scaleTransform.ScaleX == 1)
            {
                scaleTransform.ScaleX = -1;
            }

            if (height < 0 && scaleTransform.ScaleY == 1)
            {
                scaleTransform.ScaleY = -1;
            }

            if (height > 0 && scaleTransform.ScaleY == -1)
            {
                scaleTransform.ScaleY = 1;
            }

            if (width > 0 && scaleTransform.ScaleX == -1)
            {
                scaleTransform.ScaleX = 1;
            }

            _context.SelectionRectangle = new Rect(_selectionRectangleInitialPosition.X, _selectionRectangleInitialPosition.Y, Math.Abs(width), Math.Abs(height));
        }

    }
}
