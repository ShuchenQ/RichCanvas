﻿using RichCanvasDemo.Common;
using RichCanvasDemo.CustomControls;
using RichCanvasDemo.Services;
using RichCanvasDemo.ViewModels;
using RichCanvasDemo.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Input;

namespace RichCanvasDemo
{
    public class MainWindowViewModel : ObservableObject
    {
        private bool _enableGrid;
        private string _gridSpacing;
        private string _elementsCount;
        private bool _enableVirtualization;
        private ICommand drawLineCommand;
        private ICommand resizeCommand;
        private RelayCommand deleteCommand;
        private RelayCommand generateElementsCommand;
        private RelayCommand drawRectCommand;
        private RelayCommand drawBezierCommand;
        private ICommand drawEndedCommand;
        private bool _enableSnapping;
        private bool _disableCache;
        private bool _disableZoom;
        private bool _disableScroll;
        private bool _disableAutoPanning;
        private string _autoPanSpeed = "0.1";
        private string _autoPanTickRate = "0.9";
        private string _scrollFactor;
        private string _zoomFactor;
        private string _maxScale;
        private string _minScale;
        private bool _showProperties;
        private Drawable _selectedItem;
        private ICommand addImageCommand;
        private string scale = "1";
        private bool shouldBringIntoView;
        private Point mousePosition;
        private ICommand addTextCommand;
        private ICommand copyCommand;
        private Drawable _copiedElement;
        private ICommand pasteCommand;
        private readonly FileService _fileService;
        private readonly DialogService _dialogService;

        public ICommand DrawEndedCommand => drawEndedCommand ??= new RelayCommand<RoutedEventArgs>(DrawEnded);
        public ObservableCollection<Drawable> Items { get; }
        public ObservableCollection<Drawable> SelectedItems { get; }
        public ICommand DrawRectCommand => drawRectCommand ??= new RelayCommand(OnDrawCommand);
        public ICommand GenerateElements => generateElementsCommand ??= new RelayCommand(OnGenerateElements);
        public ICommand DrawLineCommand => drawLineCommand ??= new RelayCommand(DrawLine);
        public ICommand ResizeCommand => resizeCommand ??= new RelayCommand(Resize);
        public ICommand DeleteCommand => deleteCommand ??= new RelayCommand(Delete);
        public ICommand DrawBezierCommand => drawBezierCommand ??= new RelayCommand(OnDrawBezier);
        public ICommand AddTextCommand => addTextCommand ??= new RelayCommand(AddText);
        public ICommand AddImageCommand => addImageCommand ??= new RelayCommand(AddImage);
        public ICommand CopyCommand => copyCommand ??= new RelayCommand<Drawable>(Copy);
        public ICommand PasteCommand => pasteCommand ??= new RelayCommand(Paste);

        private void Paste()
        {
            _copiedElement.Left = MousePosition.X;
            _copiedElement.Top = MousePosition.Y;
            Items.Add(_copiedElement);
        }

        private void Copy(Drawable element)
        {
            _copiedElement = element.Clone();
        }

        public bool EnableGrid
        {
            get => _enableGrid;
            set => SetProperty(ref _enableGrid, value);
        }
        public string GridSpacing
        {
            get => _gridSpacing;
            set => SetProperty(ref _gridSpacing, value);
        }

        public string ElementsCount
        {
            get => _elementsCount;
            set => SetProperty(ref _elementsCount, value);
        }
        public bool EnableVirtualization
        {
            get => _enableVirtualization;
            set => SetProperty(ref _enableVirtualization, value);
        }

        public bool EnableSnapping { get => _enableSnapping; set => SetProperty(ref _enableSnapping, value); }

        public bool DisableCache { get => _disableCache; set => SetProperty(ref _disableCache, value); }

        public bool DisableZoom { get => _disableZoom; set => SetProperty(ref _disableZoom, value); }

        public bool DisableScroll { get => _disableScroll; set => SetProperty(ref _disableScroll, value); }

        public bool DisableAutoPanning { get => _disableAutoPanning; set => SetProperty(ref _disableAutoPanning, value); }

        public string AutoPanSpeed { get => _autoPanSpeed; set => SetProperty(ref _autoPanSpeed, value); }

        public string AutoPanTickRate { get => _autoPanTickRate; set => SetProperty(ref _autoPanTickRate, value); }

        public string ScrollFactor { get => _scrollFactor; set => SetProperty(ref _scrollFactor, value); }

        public string ZoomFactor { get => _zoomFactor; set => SetProperty(ref _zoomFactor, value); }

        public string MaxScale { get => _maxScale; set => SetProperty(ref _maxScale, value); }

        public string MinScale { get => _minScale; set => SetProperty(ref _minScale, value); }

        public Drawable SelectedItem { get => _selectedItem; set => SetProperty(ref _selectedItem, value); }

        public bool ShowProperties { get => _showProperties; set => SetProperty(ref _showProperties, value); }

        public string Scale { get => scale; set => SetProperty(ref scale, value); }
        public Point MousePosition { get => mousePosition; set => SetProperty(ref mousePosition, value); }

        public bool ShouldBringIntoView { get => shouldBringIntoView; set => SetProperty(ref shouldBringIntoView, value); }
        public MainWindowViewModel()
        {
            Items = new ObservableCollection<Drawable>();
            SelectedItems = new ObservableCollection<Drawable>();
            SelectedItems.CollectionChanged += SelectedItemsChanged;
            _fileService = new FileService();
            _dialogService = new DialogService();
        }

        private void SelectedItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                if (SelectedItems.Count == 1)
                {
                    SelectedItem = SelectedItems[0];
                }
                else
                {
                    SelectedItem = null;
                }
            }
            ShowProperties = SelectedItem != null && SelectedItem.IsSelected;
        }

        private void OnDrawBezier()
        {
            var bezier = new Bezier
            {
                Points = new List<Point>(),
            };
            Items.Add(bezier);
        }

        private void OnGenerateElements()
        {
            if (!string.IsNullOrEmpty(ElementsCount))
            {
                int elementsCount = int.Parse(ElementsCount);
                for (int i = 0; i < elementsCount; i++)
                {
                    Random rnd = new Random();
                    double left = rnd.Next(-1000, 1000);
                    double top = rnd.Next(-1000, 1000);
                    var item = new Line
                    {
                        Left = left,
                        Top = top,
                        Width = Math.Abs(left / 2 + 30),
                        Height = Math.Abs(top / 2 + 30)
                    };
                    if (item.Width == 0)
                    {
                        item.Width = 10;
                    }
                    if (item.Height == 0)
                    {
                        item.Height = 10;
                    }
                    Items.Add(item);
                }
            }
        }
        private void Delete()
        {
            Items.Remove(SelectedItem);
        }

        private void OnDrawCommand()
        {
            Items.Add(new Rectangle());
        }
        public void DrawConnectedLine(Point p)
        {
            Items.Add(new Line { Top = p.Y, Left = p.X });
        }

        private void DrawLine()
        {
            Items.Add(new Line());
        }

        private void Resize()
        {
            SelectedItem.Height += 20;
        }

        private void DrawEnded(RoutedEventArgs args)
        {
            var element = (Drawable)args.OriginalSource;
            if (element is Bezier bezier)
            {
                bezier.Point1 = new Point(bezier.Width - 20, 0);
                bezier.Point2 = new Point(bezier.Width - 10, 0);
                bezier.Point3 = new Point(bezier.Width, bezier.Height);
            }
        }

        private void AddImage()
        {
            string selectedImagePath;
            _fileService.OpenFileDialog(out selectedImagePath);
            var image = new ImageVisual
            {
                ImageSource = selectedImagePath,
                Height = 100,
                Width = 200
            };
            Items.Add(image);
        }

        private void AddText()
        {
            var text = new TextVisual
            {
                Top = MousePosition.Y,
                Left = MousePosition.X,
                Width = 100,
                Height = 50
            };
            _dialogService.OpenDialog<EditText>(text);
            Items.Add(text);
        }

    }
}
